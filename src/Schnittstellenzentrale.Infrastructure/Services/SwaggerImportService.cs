using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Helpers;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>Importiert Endpunkte aus einer Swagger/OpenAPI-Definition und berechnet den Diff zum bestehenden Bestand.</summary>
public class SwaggerImportService : ISwaggerImportService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IEndpointRepository _endpointRepository;
    private readonly ICredentialService _credentialService;
    private readonly ILogger<SwaggerImportService> _logger;

    /// <summary>Initialisiert eine neue Instanz von <see cref="SwaggerImportService"/>.</summary>
    public SwaggerImportService(IHttpClientFactory httpClientFactory, IEndpointRepository endpointRepository, ICredentialService credentialService, ILogger<SwaggerImportService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _endpointRepository = endpointRepository;
        _credentialService = credentialService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ImportDiff> ImportAsync(Core.Models.Application application)
    {
        if (string.IsNullOrEmpty(application.InterfaceUrl))
            return new ImportDiff();

        Stream fetchedStream;
        try
        {
            fetchedStream = await FetchSwaggerStreamAsync(application.InterfaceUrl);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Swagger-Import HTTP-Fehler für Anwendung {ApplicationId}", application.Id);
            return new ImportDiff { ErrorMessage = $"HTTP-Fehler beim Abruf der Swagger-Definition: {ex.Message}" };
        }

        await using var stream = fetchedStream;
        var (document, parseError) = await ParseSwaggerDocumentAsync(stream);
        if (document == null)
        {
            _logger.LogWarning("Swagger-Import Parsing-Fehler für Anwendung {ApplicationId}", application.Id);
            return parseError!;
        }

        var (importedEndpoints, bearerTokens) = MapToEndpoints(document, application.Id);

        var existingEndpoints = await _endpointRepository.GetEndpointsAsync(application.Id);
        return ImportDiffCalculator.Calculate(existingEndpoints, importedEndpoints).WithBearerTokens(bearerTokens);
    }

    private async Task<Stream> FetchSwaggerStreamAsync(string url)
    {
        var client = _httpClientFactory.CreateClient();
        return await client.GetStreamAsync(url);
    }

    private static async Task<(Microsoft.OpenApi.OpenApiDocument? document, ImportDiff? error)> ParseSwaggerDocumentAsync(Stream stream)
    {
        var reader = new OpenApiJsonReader();
        var result = await reader.ReadAsync(stream, null, new OpenApiReaderSettings(), default);
        var diagnostics = result.Diagnostic;

        if (diagnostics?.Errors.Any() == true)
        {
            var errors = string.Join("; ", diagnostics.Errors.Select(e => e.Message));
            return (null, new ImportDiff { ErrorMessage = $"Fehler beim Parsen der Swagger-Definition: {errors}" });
        }

        return (result.Document, null);
    }

    private static (List<Core.Models.Endpoint> endpoints, Dictionary<string, string> bearerTokens) MapToEndpoints(
        Microsoft.OpenApi.OpenApiDocument document, int applicationId)
    {
        var endpoints = new List<Core.Models.Endpoint>();
        var bearerTokens = new Dictionary<string, string>();

        foreach (var path in document.Paths)
        {
            foreach (var operation in path.Value.Operations ?? [])
            {
                var method = MapHttpMethod(operation.Key.ToString());
                var endpoint = new Core.Models.Endpoint
                {
                    Name = operation.Value.OperationId ?? $"{operation.Key} {path.Key}",
                    Method = method,
                    RelativePath = path.Key,
                    ApplicationId = applicationId,
                    PreRequestScript = ReadExtensionString(operation.Value.Extensions, "x-sz-pre-request-script"),
                    PostRequestScript = ReadExtensionString(operation.Value.Extensions, "x-sz-post-request-script")
                };

                var bearerToken = ReadExtensionString(operation.Value.Extensions, "x-sz-bearer-token");
                if (!string.IsNullOrEmpty(bearerToken))
                {
                    endpoint.AuthenticationType = AuthenticationType.BearerToken;
                    bearerTokens[$"{method}:{path.Key}"] = bearerToken;
                }

                endpoints.Add(endpoint);
            }
        }

        return (endpoints, bearerTokens);
    }

    /// <inheritdoc/>
    public async Task ApplyDiffAsync(ImportDiff diff)
    {
        foreach (var endpoint in diff.NewEndpoints)
        {
            await _endpointRepository.AddEndpointAsync(endpoint);
            SaveBearerTokenIfPresent(endpoint, diff);
        }

        foreach (var endpoint in diff.ChangedEndpoints)
        {
            await _endpointRepository.UpdateEndpointAsync(endpoint);
            SaveBearerTokenIfPresent(endpoint, diff);
        }

        foreach (var endpoint in diff.RemovedEndpoints)
            await _endpointRepository.DeleteEndpointAsync(endpoint.Id);
    }

    private void SaveBearerTokenIfPresent(Core.Models.Endpoint endpoint, ImportDiff diff)
    {
        if (endpoint.AuthenticationType != AuthenticationType.BearerToken)
            return;

        var key = $"{endpoint.Method}:{endpoint.RelativePath}";
        if (!diff.BearerTokens.TryGetValue(key, out var tokenValue))
            return;

        try
        {
            var credentialTarget = CredentialTargetHelper.Build(endpoint.ApplicationId, AuthenticationType.BearerToken);
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

    private static Core.Enums.HttpMethod MapHttpMethod(string method)
    {
        return method.ToUpperInvariant() switch
        {
            "GET" => Core.Enums.HttpMethod.GET,
            "POST" => Core.Enums.HttpMethod.POST,
            "PUT" => Core.Enums.HttpMethod.PUT,
            "DELETE" => Core.Enums.HttpMethod.DELETE,
            "PATCH" => Core.Enums.HttpMethod.PATCH,
            "HEAD" => Core.Enums.HttpMethod.HEAD,
            "OPTIONS" => Core.Enums.HttpMethod.OPTIONS,
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unbekannte HTTP-Methode in Swagger-Definition.")
        };
    }
}
