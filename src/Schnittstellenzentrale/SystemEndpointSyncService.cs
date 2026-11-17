using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Schnittstellenzentrale.Core.Interfaces;
using Swashbuckle.AspNetCore.Swagger;

namespace Schnittstellenzentrale;

/// <summary>Führt nach App-Start einmalig den Endpunktabgleich für die Systemanwendung durch.</summary>
public class SystemEndpointSyncService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SystemEndpointSyncService> _logger;

    /// <summary>Initialisiert eine neue Instanz des <see cref="SystemEndpointSyncService"/>.</summary>
    /// <param name="scopeFactory">Die Service-Scope-Factory.</param>
    /// <param name="logger">Der Logger.</param>
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
                var swaggerImportService = scope.ServiceProvider.GetRequiredService<ISwaggerImportService>();
                var document = swaggerProvider.GetSwagger("v1");
                await swaggerImportService.SyncFromDocumentAsync(document, systemApp.Id);
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
}
