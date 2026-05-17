using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Readers;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Infrastructure.Services;

public class SwaggerImportService : ISwaggerImportService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IEndpointRepository _endpointRepository;
    private readonly ILogger<SwaggerImportService> _logger;

    public SwaggerImportService(IHttpClientFactory httpClientFactory, IEndpointRepository endpointRepository, ILogger<SwaggerImportService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _endpointRepository = endpointRepository;
        _logger = logger;
    }

    public async Task<ImportDiff> ImportAsync(Core.Models.Application application)
    {
        if (string.IsNullOrEmpty(application.InterfaceUrl))
            return new ImportDiff();

        Stream? fetchedStream;
        try
        {
            var client = _httpClientFactory.CreateClient();
            fetchedStream = await client.GetStreamAsync(application.InterfaceUrl);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Swagger-Import HTTP-Fehler für Anwendung {ApplicationId}", application.Id);
            return new ImportDiff { ErrorMessage = $"HTTP-Fehler beim Abruf der Swagger-Definition: {ex.Message}" };
        }

        await using var stream = fetchedStream;
        var reader = new OpenApiStreamReader();
        var document = reader.Read(stream, out var diagnostics);

        if (diagnostics.Errors.Any())
        {
            var errors = string.Join("; ", diagnostics.Errors.Select(e => e.Message));
            _logger.LogWarning("Swagger-Import Parsing-Fehler für Anwendung {ApplicationId}: {Errors}", application.Id, errors);
            return new ImportDiff { ErrorMessage = $"Fehler beim Parsen der Swagger-Definition: {errors}" };
        }

        var importedEndpoints = new List<Core.Models.Endpoint>();

        foreach (var path in document.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                var method = MapHttpMethod(operation.Key.ToString());
                var endpoint = new Core.Models.Endpoint
                {
                    Name = operation.Value.OperationId ?? $"{operation.Key} {path.Key}",
                    Method = method,
                    RelativePath = path.Key,
                    ApplicationId = application.Id
                };
                importedEndpoints.Add(endpoint);
            }
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

    private Core.Enums.HttpMethod MapHttpMethod(string method)
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
