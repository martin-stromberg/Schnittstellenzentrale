using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Reader;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Helpers;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Helpers;

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
        var (document, parseError) = await ParseSwaggerDocumentAsync(stream, application.InterfaceUrl);
        if (document == null)
        {
            _logger.LogWarning("Swagger-Import Parsing-Fehler für Anwendung {ApplicationId}", application.Id);
            return parseError!;
        }

        var (importedEndpoints, bearerTokens) = SwaggerOperationHelper.MapDocumentToEndpoints(document, application.Id);

        var existingEndpoints = await _endpointRepository.GetEndpointsAsync(application.Id);
        return ImportDiffCalculator.Calculate(existingEndpoints, importedEndpoints).WithBearerTokens(bearerTokens);
    }

    private async Task<Stream> FetchSwaggerStreamAsync(string url)
    {
        var client = _httpClientFactory.CreateClient();
        return await client.GetStreamAsync(url);
    }

    private static async Task<(Microsoft.OpenApi.OpenApiDocument? document, ImportDiff? error)> ParseSwaggerDocumentAsync(Stream stream, string url)
    {
        var reader = new OpenApiJsonReader();
        var result = await reader.ReadAsync(stream, new Uri(url), new OpenApiReaderSettings(), default);
        var diagnostics = result.Diagnostic;

        if (diagnostics?.Errors.Any() == true)
        {
            var errors = string.Join("; ", diagnostics.Errors.Select(e => e.Message));
            return (null, new ImportDiff { ErrorMessage = $"Fehler beim Parsen der Swagger-Definition: {errors}" });
        }

        return (result.Document, null);
    }

    /// <inheritdoc/>
    public async Task ApplyDiffAsync(ImportDiff diff)
    {
        Dictionary<(string Name, int? ParentGroupId), EndpointGroup>? groupLookup = null;

        foreach (var endpoint in diff.NewEndpoints)
        {
            if (groupLookup == null)
            {
                var existingGroups = await _endpointRepository.GetEndpointGroupsAsync(endpoint.ApplicationId);
                groupLookup = existingGroups.ToDictionary(g => (g.Name, g.ParentGroupId));
            }
            endpoint.EndpointGroupId = await EndpointGroupHelper.ResolveGroupIdAsync(
                endpoint.RelativePath, endpoint.ApplicationId, _endpointRepository, groupLookup);
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

        var key = EndpointKeyHelper.BuildKey(endpoint);
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

}
