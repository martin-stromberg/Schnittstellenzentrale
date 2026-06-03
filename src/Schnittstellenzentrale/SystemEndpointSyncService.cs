using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Schnittstellenzentrale.Core.Helpers;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Helpers;
using Swashbuckle.AspNetCore.Swagger;

namespace Schnittstellenzentrale;

/// <summary>Führt nach App-Start einmalig den selektiven Endpunktabgleich für die Systemanwendung durch.</summary>
public class SystemEndpointSyncService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SystemEndpointSyncService> _logger;

    /// <summary>Initialisiert eine neue Instanz des <see cref="SystemEndpointSyncService"/>.</summary>
    public SystemEndpointSyncService(IServiceScopeFactory scopeFactory, ILogger<SystemEndpointSyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
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
                var endpointRepository = scope.ServiceProvider.GetRequiredService<IEndpointRepository>();
                var swaggerImportService = scope.ServiceProvider.GetRequiredService<ISwaggerImportService>();
                await SyncEndpointsAsync(swaggerProvider, endpointRepository, swaggerImportService, systemApp);
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

    private static async Task SyncEndpointsAsync(
        ISwaggerProvider swaggerProvider,
        IEndpointRepository endpointRepository,
        ISwaggerImportService swaggerImportService,
        Application systemApp)
    {
        var document = swaggerProvider.GetSwagger("v1");
        var (importedEndpoints, bearerTokens) = SwaggerOperationHelper.MapDocumentToEndpoints(document, systemApp.Id);

        var existingEndpoints = await endpointRepository.GetEndpointsAsync(systemApp.Id);
        var existingKeys = existingEndpoints.Select(EndpointKeyHelper.BuildKey).ToHashSet();
        var importedKeys = importedEndpoints.Select(EndpointKeyHelper.BuildKey).ToHashSet();

        var diff = new ImportDiff
        {
            NewEndpoints = importedEndpoints.Where(e => !existingKeys.Contains(EndpointKeyHelper.BuildKey(e))).ToList(),
            RemovedEndpoints = existingEndpoints.Where(e => !importedKeys.Contains(EndpointKeyHelper.BuildKey(e))).ToList()
        }.WithBearerTokens(bearerTokens);

        await swaggerImportService.ApplyDiffAsync(diff);
    }
}
