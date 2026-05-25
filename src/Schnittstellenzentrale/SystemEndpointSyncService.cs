using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Swashbuckle.AspNetCore.Swagger;
using CoreHttpMethod = Schnittstellenzentrale.Core.Enums.HttpMethod;
using Endpoint = Schnittstellenzentrale.Core.Models.Endpoint;

namespace Schnittstellenzentrale;

/// <summary>Führt nach App-Start einmalig den selektiven Endpunktabgleich für die Systemanwendung durch.</summary>
public class SystemEndpointSyncService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SystemEndpointSyncService> _logger;
    private readonly ICredentialService _credentialService;

    /// <summary>Initialisiert eine neue Instanz des <see cref="SystemEndpointSyncService"/>.</summary>
    public SystemEndpointSyncService(IServiceScopeFactory scopeFactory, ILogger<SystemEndpointSyncService> logger, ICredentialService credentialService)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _credentialService = credentialService;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();

            var applicationRepository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
            var group = await applicationRepository.GetSystemGroupAsync();
            if (group == null)
            {
                _logger.LogWarning("Systemgruppe nicht gefunden. Endpunktabgleich wird übersprungen.");
                return;
            }

            var systemApp = group.Applications.FirstOrDefault(a => a.IsSystem);
            if (systemApp == null)
            {
                _logger.LogWarning("Systemanwendung nicht gefunden. Endpunktabgleich wird übersprungen.");
                return;
            }

            try
            {
                var swaggerProvider = scope.ServiceProvider.GetRequiredService<ISwaggerProvider>();
                var document = swaggerProvider.GetSwagger("v1");

                var importedEndpoints = new List<Endpoint>();
                var bearerTokens = new Dictionary<string, string>();
                foreach (var path in document.Paths)
                    foreach (var operation in path.Value.Operations)
                    {
                        var method = MapHttpMethod(operation.Key.ToString());
                        var bearerTokenValue = ReadExtensionString(operation.Value?.Extensions, "x-sz-bearer-token");
                        var authType = string.IsNullOrEmpty(bearerTokenValue)
                            ? DetectAuthenticationType(operation.Value)
                            : AuthenticationType.BearerToken;
                        if (!string.IsNullOrEmpty(bearerTokenValue))
                            bearerTokens[$"{method}:{path.Key}"] = bearerTokenValue;
                        importedEndpoints.Add(new Endpoint
                        {
                            Name = operation.Value?.OperationId ?? $"{operation.Key} {path.Key}",
                            Method = method,
                            RelativePath = path.Key,
                            ApplicationId = systemApp.Id,
                            AuthenticationType = authType,
                            PreRequestScript = ReadExtensionString(operation.Value?.Extensions, "x-sz-pre-request-script"),
                            PostRequestScript = ReadExtensionString(operation.Value?.Extensions, "x-sz-post-request-script"),
                            Headers = BuildDefaultHeaders(operation.Value)
                        });
                    }

                var endpointRepository = scope.ServiceProvider.GetRequiredService<IEndpointRepository>();
                var existingEndpoints = await endpointRepository.GetEndpointsAsync(systemApp.Id);
                var existingGroups = await endpointRepository.GetEndpointGroupsAsync(systemApp.Id);
                var groupLookup = existingGroups.ToDictionary(g => (g.Name, g.ParentGroupId));

                var existingKeys = existingEndpoints.Select(BuildKey).ToHashSet();
                var importedKeys = importedEndpoints.Select(BuildKey).ToHashSet();

                foreach (var endpoint in importedEndpoints.Where(e => !existingKeys.Contains(BuildKey(e))))
                {
                    endpoint.EndpointGroupId = await ResolveGroupIdAsync(
                        endpoint.RelativePath, systemApp.Id, endpointRepository, groupLookup);
                    await endpointRepository.AddEndpointAsync(endpoint);
                    SaveBearerTokenIfPresent(endpoint, bearerTokens, systemApp.Id);
                }

                foreach (var endpoint in existingEndpoints.Where(e => !importedKeys.Contains(BuildKey(e))))
                    await endpointRepository.DeleteEndpointAsync(endpoint.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Endpunktabgleich.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Endpunktabgleich.");
        }
    }

    private static async Task<int?> ResolveGroupIdAsync(
        string relativePath,
        int applicationId,
        IEndpointRepository endpointRepository,
        Dictionary<(string Name, int? ParentGroupId), EndpointGroup> groupLookup)
    {
        int? parentGroupId = null;
        foreach (var segment in ParseGroupSegments(relativePath))
        {
            var key = (segment, parentGroupId);
            if (!groupLookup.TryGetValue(key, out var endpointGroup))
            {
                endpointGroup = await endpointRepository.AddEndpointGroupAsync(new EndpointGroup
                {
                    Name = segment,
                    ApplicationId = applicationId,
                    ParentGroupId = parentGroupId
                });
                groupLookup[key] = endpointGroup;
            }
            parentGroupId = endpointGroup.Id;
        }
        return parentGroupId;
    }

    private static IEnumerable<string> ParseGroupSegments(string relativePath)
    {
        foreach (var segment in relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment.Equals("api", StringComparison.OrdinalIgnoreCase))
                continue;
            if (segment.StartsWith('{'))
                continue;
            yield return segment;
        }
    }

    private static string BuildKey(Endpoint endpoint) => $"{endpoint.Method}:{endpoint.RelativePath}";

    private static AuthenticationType DetectAuthenticationType(OpenApiOperation? operation)
    {
        if (operation?.Security is not { Count: > 0 })
            return AuthenticationType.BearerToken;

        foreach (var requirement in operation.Security)
            foreach (var schemeKey in requirement.Keys)
                if (schemeKey is OpenApiSecuritySchemeReference schemeRef &&
                    string.Equals(schemeRef.Reference?.Id, "Negotiate", StringComparison.OrdinalIgnoreCase))
                    return AuthenticationType.Negotiate;

        return AuthenticationType.BearerToken;
    }

    private static List<EndpointHeader> BuildDefaultHeaders(OpenApiOperation? operation)
    {
        if (operation?.Parameters == null)
            return [];

        return operation.Parameters
            .Where(p => p.In == ParameterLocation.Header)
            .Select(p => new EndpointHeader
            {
                Key = p.Name,
                Value = ExtractDefaultValue(p)
            })
            .ToList();
    }

    private static string ExtractDefaultValue(IOpenApiParameter param)
    {
        if (param.Schema?.Default is not System.Text.Json.Nodes.JsonValue jsonVal)
            return string.Empty;

        return jsonVal.TryGetValue<string>(out var str) ? str : string.Empty;
    }

    private static CoreHttpMethod MapHttpMethod(string method) =>
        method.ToUpperInvariant() switch
        {
            "GET" => CoreHttpMethod.GET,
            "POST" => CoreHttpMethod.POST,
            "PUT" => CoreHttpMethod.PUT,
            "DELETE" => CoreHttpMethod.DELETE,
            "PATCH" => CoreHttpMethod.PATCH,
            "HEAD" => CoreHttpMethod.HEAD,
            "OPTIONS" => CoreHttpMethod.OPTIONS,
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unbekannte HTTP-Methode.")
        };

    private void SaveBearerTokenIfPresent(Endpoint endpoint, Dictionary<string, string> bearerTokens, int applicationId)
    {
        var key = BuildKey(endpoint);
        if (!bearerTokens.TryGetValue(key, out var tokenValue))
            return;

        try
        {
            var credentialTarget = Core.Helpers.CredentialTargetHelper.Build(applicationId, AuthenticationType.BearerToken);
            _credentialService.SavePassword(credentialTarget, string.Empty, tokenValue);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Credential Manager konnte für Endpunkt {Key} nicht beschrieben werden.", key);
        }
    }

    private static string? ReadExtensionString(IDictionary<string, IOpenApiExtension>? extensions, string key)
    {
        if (extensions == null || !extensions.TryGetValue(key, out var extension))
            return null;

        if (extension is JsonNodeExtension jne && jne.Node is JsonValue jv && jv.TryGetValue<string>(out var value))
            return value;

        return null;
    }
}
