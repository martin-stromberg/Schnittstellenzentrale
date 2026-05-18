using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Serilog;

namespace Schnittstellenzentrale;

/// <summary>Legt beim Programmstart die Systemgruppe und -anwendung an oder aktualisiert deren URLs.</summary>
public static class SystemEntryInitializer
{
    /// <summary>
    /// Stellt sicher, dass Systemgruppe und -anwendung in der Datenbank vorhanden sind
    /// und deren URLs dem konfigurierten <c>Api:BaseUrl</c>-Wert entsprechen.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();

        try
        {
            var baseUrl = configuration["Api:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                Log.Warning("SystemEntryInitializer: Api:BaseUrl ist nicht konfiguriert. Systemeintrag wird nicht angelegt.");
                return;
            }

            var interfaceUrl = $"{baseUrl}/swagger/v1/swagger.json";

            var group = await repository.GetSystemGroupAsync();
            if (group == null)
            {
                group = await repository.AddGroupAsync(new ApplicationGroup
                {
                    Name = "Schnittstellenzentrale",
                    IsSystem = true
                });
            }

            var systemApp = group.Applications.FirstOrDefault(a => a.IsSystem);
            if (systemApp == null)
            {
                await repository.AddApplicationAsync(new Application
                {
                    Name = "Schnittstellenzentrale",
                    IsSystem = true,
                    BaseUrl = baseUrl,
                    InterfaceUrl = interfaceUrl,
                    InterfaceType = Application.DetectInterfaceType(interfaceUrl),
                    ApplicationGroupId = group.Id
                });
            }
            else if (systemApp.BaseUrl != baseUrl || systemApp.InterfaceUrl != interfaceUrl)
            {
                systemApp.BaseUrl = baseUrl;
                systemApp.InterfaceUrl = interfaceUrl;
                systemApp.InterfaceType = Application.DetectInterfaceType(interfaceUrl);
                await repository.UpdateApplicationAsync(systemApp);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SystemEntryInitializer: Fehler beim Anlegen oder Aktualisieren des Systemeintrags.");
        }
    }
}
