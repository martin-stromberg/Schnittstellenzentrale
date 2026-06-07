using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Helpers;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using System.Xml;
using System.Xml.Linq;

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
        var defaultAuthType = bearerTokenValue != null ? AuthenticationType.BearerToken : AuthenticationType.None;

        var entitySetAnnotations = ParseEntitySetAnnotations(xmlContent);
        var operationAnnotations = ParseOperationAnnotations(xmlContent);

        var importedEndpoints = new List<Core.Models.Endpoint>();
        var bearerTokens = new Dictionary<string, string>();

        foreach (var entitySet in model.EntityContainer?.EntitySets() ?? Enumerable.Empty<IEdmEntitySet>())
        {
            var relativePath = BuildRelativePath(application.BaseUrl, serviceUrl, entitySet.Name);
            var relativePathWithKey = BuildRelativePath(application.BaseUrl, serviceUrl, $"{entitySet.Name}({{key}})");

            entitySetAnnotations.TryGetValue(entitySet.Name, out var annotations);
            var authType = ResolveAuthType(annotations, defaultAuthType);
            var postScript = annotations?.GetValueOrDefault("x-sz-post-request-script");

            AddEndpoint(importedEndpoints, bearerTokens, $"GET {entitySet.Name}", Core.Enums.HttpMethod.GET, relativePath, application.Id, authType, bearerTokenValue, postScript: postScript);
            AddEndpoint(importedEndpoints, bearerTokens, $"POST {entitySet.Name}", Core.Enums.HttpMethod.POST, relativePath, application.Id, authType, bearerTokenValue, postScript: postScript);
            AddEndpoint(importedEndpoints, bearerTokens, $"PUT {entitySet.Name}", Core.Enums.HttpMethod.PUT, relativePathWithKey, application.Id, authType, bearerTokenValue, postScript: postScript);
            AddEndpoint(importedEndpoints, bearerTokens, $"PATCH {entitySet.Name}", Core.Enums.HttpMethod.PATCH, relativePathWithKey, application.Id, authType, bearerTokenValue, postScript: postScript);
            AddEndpoint(importedEndpoints, bearerTokens, $"DELETE {entitySet.Name}", Core.Enums.HttpMethod.DELETE, relativePathWithKey, application.Id, authType, bearerTokenValue, postScript: postScript);
        }

        foreach (var operation in model.SchemaElements.OfType<IEdmOperation>())
        {
            var method = operation is IEdmAction ? Core.Enums.HttpMethod.POST : Core.Enums.HttpMethod.GET;
            var relativePath = BuildRelativePath(application.BaseUrl, serviceUrl, operation.Name);

            operationAnnotations.TryGetValue(operation.Name, out var opAnnotations);
            var opAuthType = ResolveAuthType(opAnnotations, defaultAuthType);
            var opPostScript = opAnnotations?.GetValueOrDefault("x-sz-post-request-script");

            AddEndpoint(importedEndpoints, bearerTokens, operation.Name, method, relativePath, application.Id, opAuthType, bearerTokenValue, postScript: opPostScript);
        }

        var existingEndpoints = await _endpointRepository.GetEndpointsAsync(application.Id);

        return ImportDiffCalculator.Calculate(existingEndpoints, importedEndpoints).WithBearerTokens(bearerTokens);
    }

    /// <inheritdoc/>
    public async Task ApplyDiffAsync(ImportDiff diff)
    {
        var groupLookup = new Dictionary<string, EndpointGroup>();

        var applicationId = diff.NewEndpoints.Select(e => e.ApplicationId).FirstOrDefault();
        if (applicationId != 0)
        {
            var existingGroups = await _endpointRepository.GetEndpointGroupsAsync(applicationId);
            foreach (var g in existingGroups.Where(g => g.ParentGroupId == null))
                groupLookup.TryAdd(g.Name, g);
        }

        foreach (var endpoint in diff.NewEndpoints)
        {
            var groupName = ExtractEntitySetName(endpoint.Name);
            if (groupName != null)
            {
                if (!groupLookup.TryGetValue(groupName, out var group))
                {
                    group = await _endpointRepository.AddEndpointGroupAsync(new EndpointGroup
                    {
                        Name = groupName,
                        ApplicationId = endpoint.ApplicationId,
                        ParentGroupId = null
                    });
                    groupLookup[groupName] = group;
                }
                endpoint.EndpointGroupId = group.Id;
            }

            await _endpointRepository.AddEndpointAsync(endpoint);
        }

        foreach (var endpoint in diff.ChangedEndpoints)
            await _endpointRepository.UpdateEndpointAsync(endpoint);

        foreach (var endpoint in diff.RemovedEndpoints)
            await _endpointRepository.DeleteEndpointAsync(endpoint.Id);

        SaveBearerTokenOnce(diff);
    }

    private void SaveBearerTokenOnce(ImportDiff diff)
    {
        var allEndpoints = diff.NewEndpoints.Concat(diff.ChangedEndpoints);
        var writtenTargets = new HashSet<string>();
        foreach (var endpoint in allEndpoints)
        {
            if (endpoint.AuthenticationType != AuthenticationType.BearerToken)
                continue;

            var key = EndpointKeyHelper.BuildKey(endpoint);
            if (!diff.BearerTokens.TryGetValue(key, out var tokenValue))
                continue;

            var credentialTarget = CredentialTargetHelper.Build(endpoint.ApplicationId, AuthenticationType.BearerToken);
            if (!writtenTargets.Add(credentialTarget))
                continue;

            try
            {
                _credentialService.SavePassword(credentialTarget, string.Empty, tokenValue);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Credential Manager konnte für Anwendung {ApplicationId} nicht beschrieben werden.", endpoint.ApplicationId);
            }
        }
    }

    private static string? ExtractEntitySetName(string endpointName)
    {
        // Annahme: Endpunktnamen haben das Format "<METHODE> <EntitySetName>", d.h. kein Leerzeichen im EntitySetName.
        var spaceIndex = endpointName.IndexOf(' ');
        if (spaceIndex < 0)
            return null;
        var name = endpointName[(spaceIndex + 1)..];
        return string.IsNullOrEmpty(name) ? null : name;
    }

    private static void AddEndpoint(
        List<Core.Models.Endpoint> endpoints,
        Dictionary<string, string> bearerTokens,
        string name,
        Core.Enums.HttpMethod method,
        string relativePath,
        int applicationId,
        AuthenticationType authType,
        string? bearerTokenValue,
        string? postScript = null)
    {
        var endpoint = new Core.Models.Endpoint
        {
            Name = name,
            Method = method,
            RelativePath = relativePath,
            ApplicationId = applicationId,
            AuthenticationType = authType,
            PostRequestScript = postScript
        };
        endpoints.Add(endpoint);
        if (bearerTokenValue != null)
            bearerTokens[EndpointKeyHelper.BuildKey(endpoint)] = bearerTokenValue;
    }

    private static AuthenticationType ResolveAuthType(Dictionary<string, string>? annotations, AuthenticationType defaultAuthType)
    {
        if (annotations == null)
            return defaultAuthType;

        if (!annotations.TryGetValue("x-sz-auth-type", out var authTypeStr))
            return defaultAuthType;

        return authTypeStr.ToLowerInvariant() switch
        {
            "bearertoken" or "bearer" => AuthenticationType.BearerToken,
            "negotiate" => AuthenticationType.Negotiate,
            "basic" => AuthenticationType.Basic,
            "none" => AuthenticationType.None,
            _ => defaultAuthType
        };
    }

    private static Dictionary<string, Dictionary<string, string>> ParseEntitySetAnnotations(string xmlContent)
        => ParseAnnotationsForElement(xmlContent, "EntitySet");

    private static Dictionary<string, Dictionary<string, string>> ParseOperationAnnotations(string xmlContent)
    {
        var actions = ParseAnnotationsForElement(xmlContent, "Action");
        var functions = ParseAnnotationsForElement(xmlContent, "Function");
        foreach (var kvp in functions)
            actions.TryAdd(kvp.Key, kvp.Value);
        return actions;
    }

    private static Dictionary<string, Dictionary<string, string>> ParseAnnotationsForElement(string xmlContent, string elementLocalName)
    {
        var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var xdoc = XDocument.Parse(xmlContent);
            var ns = XNamespace.Get("http://docs.oasis-open.org/odata/ns/edm");
            foreach (var element in xdoc.Descendants(ns + elementLocalName))
            {
                var name = element.Attribute("Name")?.Value;
                if (string.IsNullOrEmpty(name))
                    continue;

                var annotations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var annotation in element.Elements(ns + "Annotation"))
                {
                    var term = annotation.Attribute("Term")?.Value;
                    if (string.IsNullOrEmpty(term))
                        continue;
                    // Term kann vollqualifiziert sein (z.B. "Schnittstellenzentrale.V1.x-sz-auth-type")
                    // oder kurz (z.B. "x-sz-auth-type"). Wir normalisieren auf den letzten Segment.
                    var termKey = term.Contains('.') ? term[(term.LastIndexOf('.') + 1)..] : term;
                    var value = annotation.Attribute("String")?.Value ?? annotation.Value;
                    if (!string.IsNullOrEmpty(value))
                        annotations[termKey] = value;
                }
                if (annotations.Count > 0)
                    result[name] = annotations;
            }
        }
        catch (XmlException)
        {
            // Annotationen sind optional — Parsing-Fehler werden ignoriert
        }
        catch (InvalidOperationException)
        {
            // Annotationen sind optional — LINQ-Traversal-Fehler werden ignoriert
        }
        return result;
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

}
