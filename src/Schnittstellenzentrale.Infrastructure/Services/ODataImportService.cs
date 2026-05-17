using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using System.Xml;

namespace Schnittstellenzentrale.Infrastructure.Services;

public class ODataImportService : IODataImportService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IEndpointRepository _endpointRepository;
    private readonly ILogger<ODataImportService> _logger;

    public ODataImportService(IHttpClientFactory httpClientFactory, IEndpointRepository endpointRepository, ILogger<ODataImportService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _endpointRepository = endpointRepository;
        _logger = logger;
    }

    public async Task<ImportDiff> ImportAsync(Core.Models.Application application)
    {
        if (string.IsNullOrEmpty(application.InterfaceUrl))
            return new ImportDiff();

        string xmlContent;
        try
        {
            var client = _httpClientFactory.CreateClient();
            xmlContent = await client.GetStringAsync(application.InterfaceUrl);
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

        var importedEndpoints = new List<Core.Models.Endpoint>();

        foreach (var entitySet in model.EntityContainer?.EntitySets() ?? Enumerable.Empty<IEdmEntitySet>())
        {
            importedEndpoints.Add(new Core.Models.Endpoint
            {
                Name = $"GET {entitySet.Name}",
                Method = Core.Enums.HttpMethod.GET,
                RelativePath = entitySet.Name,
                ApplicationId = application.Id
            });
            importedEndpoints.Add(new Core.Models.Endpoint
            {
                Name = $"POST {entitySet.Name}",
                Method = Core.Enums.HttpMethod.POST,
                RelativePath = entitySet.Name,
                ApplicationId = application.Id
            });
        }

        foreach (var operation in model.SchemaElements.OfType<IEdmOperation>())
        {
            var method = operation is IEdmAction ? Core.Enums.HttpMethod.POST : Core.Enums.HttpMethod.GET;
            importedEndpoints.Add(new Core.Models.Endpoint
            {
                Name = operation.Name,
                Method = method,
                RelativePath = operation.Name,
                ApplicationId = application.Id
            });
        }

        var existingEndpoints = await _endpointRepository.GetEndpointsAsync(application.Id);
        return ImportDiffCalculator.Calculate(existingEndpoints, importedEndpoints);
    }

    public async Task ApplyDiffAsync(ImportDiff diff)
    {
        foreach (var endpoint in diff.NewEndpoints)
            await _endpointRepository.AddEndpointAsync(endpoint);

        foreach (var endpoint in diff.ChangedEndpoints)
            await _endpointRepository.UpdateEndpointAsync(endpoint);

        foreach (var endpoint in diff.RemovedEndpoints)
            await _endpointRepository.DeleteEndpointAsync(endpoint.Id);
    }
}
