using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Helpers;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using System.Xml;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>Importiert Endpunkte aus einem OData v4-CSDL-Metadaten-Dokument.</summary>
public class ODataImportService : IODataImportService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IEndpointRepository _endpointRepository;
    private readonly ICredentialService _credentialService;
    private readonly ILogger<ODataImportService> _logger;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ODataImportService"/>.</summary>
    public ODataImportService(IHttpClientFactory httpClientFactory, IEndpointRepository endpointRepository, ICredentialService credentialService, ILogger<ODataImportService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _endpointRepository = endpointRepository;
        _credentialService = credentialService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ImportDiff> ImportAsync(Core.Models.Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        if (string.IsNullOrEmpty(application.InterfaceUrl))
            return new ImportDiff();

        string xmlContent;
        try
        {
            var client = _httpClientFactory.CreateClient();
            xmlContent = await client.GetStringAsync(application.InterfaceUrl);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "OData-Import abgebrochen für Anwendung {ApplicationId}", application.Id);
            return new ImportDiff { ErrorMessage = $"Abruf der Metadaten wurde abgebrochen: {ex.Message}" };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "OData-Import HTTP-Fehler für Anwendung {ApplicationId}", application.Id);
            return new ImportDiff { ErrorMessage = $"HTTP-Fehler beim Abruf der Metadaten: {ex.Message}" };
        }

        IEdmModel model;
        try
        {
            using var stringReader = new StringReader(xmlContent);
            using var xmlReader = XmlReader.Create(stringReader);
            model = CsdlReader.Parse(xmlReader);
        }
        catch (XmlException ex)
        {
            _logger.LogWarning(ex, "OData-Import XML-Parsing-Fehler für Anwendung {ApplicationId}", application.Id);
            return new ImportDiff { ErrorMessage = $"Ungültiges XML in Metadaten: {ex.Message}" };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OData-Import Parsing-Fehler für Anwendung {ApplicationId}", application.Id);
            return new ImportDiff { ErrorMessage = $"Fehler beim Parsen der Metadaten: {ex.Message}" };
        }

        var serviceUrl = ResolveServiceUrl(application.InterfaceUrl);
        var bearerTokenValue = ReadExistingBearerToken(application.Id);
        var authType = bearerTokenValue != null ? AuthenticationType.BearerToken : AuthenticationType.None;

        var importedEndpoints = new List<Core.Models.Endpoint>();
        var bearerTokens = new Dictionary<string, string>();

        foreach (var entitySet in model.EntityContainer?.EntitySets() ?? Enumerable.Empty<IEdmEntitySet>())
        {
            var relativePath = BuildRelativePath(application.BaseUrl, serviceUrl, entitySet.Name);

            var getEndpoint = new Core.Models.Endpoint
            {
                Name = $"GET {entitySet.Name}",
                Method = Core.Enums.HttpMethod.GET,
                RelativePath = relativePath,
                ApplicationId = application.Id,
                AuthenticationType = authType
            };
            importedEndpoints.Add(getEndpoint);
            if (bearerTokenValue != null)
                bearerTokens[EndpointKeyHelper.BuildKey(getEndpoint)] = bearerTokenValue;

            var postEndpoint = new Core.Models.Endpoint
            {
                Name = $"POST {entitySet.Name}",
                Method = Core.Enums.HttpMethod.POST,
                RelativePath = relativePath,
                ApplicationId = application.Id,
                AuthenticationType = authType
            };
            importedEndpoints.Add(postEndpoint);
            if (bearerTokenValue != null)
                bearerTokens[EndpointKeyHelper.BuildKey(postEndpoint)] = bearerTokenValue;
        }

        foreach (var operation in model.SchemaElements.OfType<IEdmOperation>())
        {
            var method = operation is IEdmAction ? Core.Enums.HttpMethod.POST : Core.Enums.HttpMethod.GET;
            var relativePath = BuildRelativePath(application.BaseUrl, serviceUrl, operation.Name);
            var endpoint = new Core.Models.Endpoint
            {
                Name = operation.Name,
                Method = method,
                RelativePath = relativePath,
                ApplicationId = application.Id,
                AuthenticationType = authType
            };
            importedEndpoints.Add(endpoint);
            if (bearerTokenValue != null)
                bearerTokens[EndpointKeyHelper.BuildKey(endpoint)] = bearerTokenValue;
        }

        var authenticateEndpoint = BuildAuthenticateEndpoint(application, serviceUrl, authType);
        var existingEndpoints = await _endpointRepository.GetEndpointsAsync(application.Id);

        var hasAuthenticate = existingEndpoints.Any(e =>
            e.Method == Core.Enums.HttpMethod.POST &&
            e.RelativePath == authenticateEndpoint.RelativePath);
        if (!hasAuthenticate)
        {
            importedEndpoints.Add(authenticateEndpoint);
            if (bearerTokenValue != null)
                bearerTokens[EndpointKeyHelper.BuildKey(authenticateEndpoint)] = bearerTokenValue;
        }

        return ImportDiffCalculator.Calculate(existingEndpoints, importedEndpoints).WithBearerTokens(bearerTokens);
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

    private static string ResolveServiceUrl(string interfaceUrl)
    {
        const string metadataSuffix = "/$metadata";
        if (interfaceUrl.EndsWith(metadataSuffix, StringComparison.OrdinalIgnoreCase))
            return interfaceUrl[..^metadataSuffix.Length];
        return interfaceUrl;
    }

    private static string BuildRelativePath(string baseUrl, string serviceUrl, string entityName)
    {
        var normalizedBase = baseUrl.TrimEnd('/') + "/";
        var normalizedService = serviceUrl.TrimEnd('/') + "/";
        var fullEndpointUrl = normalizedService + entityName;

        if (fullEndpointUrl.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase))
            return fullEndpointUrl[normalizedBase.Length..];

        return entityName;
    }

    private static Core.Models.Endpoint BuildAuthenticateEndpoint(Core.Models.Application application, string serviceUrl, AuthenticationType authType)
    {
        var relativePath = BuildRelativePath(application.BaseUrl, serviceUrl, "authenticate");
        return new Core.Models.Endpoint
        {
            Name = "POST authenticate",
            Method = Core.Enums.HttpMethod.POST,
            RelativePath = relativePath,
            ApplicationId = application.Id,
            AuthenticationType = authType
        };
    }

    private string? ReadExistingBearerToken(int applicationId)
    {
        try
        {
            var target = CredentialTargetHelper.Build(applicationId, AuthenticationType.BearerToken);
            return _credentialService.GetPassword(target);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Credential Manager konnte für Anwendung {ApplicationId} nicht gelesen werden.", applicationId);
            return null;
        }
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
