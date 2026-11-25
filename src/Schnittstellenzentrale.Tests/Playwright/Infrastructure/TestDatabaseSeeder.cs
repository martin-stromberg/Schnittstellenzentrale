using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Data;
using CoreHttpMethod = Schnittstellenzentrale.Core.Enums.HttpMethod;

namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>Setzt die Testdatenbank vor jedem Testlauf auf einen definierten Ausgangszustand zurück.</summary>
public class TestDatabaseSeeder
{
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;

    /// <summary>Initialisiert den Seeder mit dem Service-Provider und der Konfiguration der Factory.</summary>
    /// <param name="services">Der Service-Provider des Testservers.</param>
    /// <param name="configuration">Die Konfiguration des Testservers.</param>
    public TestDatabaseSeeder(IServiceProvider services, IConfiguration configuration)
    {
        _services = services;
        _configuration = configuration;
    }

    /// <summary>Leert alle Datentabellen deterministisch, legt das Schema an (falls noch nicht vorhanden) und führt den Seed-Prozess aus.</summary>
    /// <remarks>
    /// Kein <c>EnsureDeletedAsync</c>: Auf der benannten Shared-Cache-In-Memory-SQLite der
    /// Playwright-Server (<c>Mode=Memory;Cache=Shared</c>) ist der Aufruf ein wirkungsloser
    /// No-op — ohne File-Backing ist <c>DbConnection.DataSource</c> leer, sodass EF Core
    /// weder <c>SqliteConnection.ClearAllPools</c> noch <c>File.Delete</c> erreicht;
    /// <c>Mode=Memory</c>-Verbindungen sind zudem non-pooled, und die offene
    /// Anchor-Connection hält die Datenbank bis zum Collection-Ende am Leben. Daten
    /// überlebten den Reset daher deterministisch (Root Cause von Issue #118).
    /// Stattdessen leert <see cref="ClearAllDataAsync"/> alle modellierten Tabellen
    /// per <c>DELETE FROM</c>.
    /// </remarks>
    public async Task ResetAsync()
    {
        using var scope = _services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var db = factory.CreateDbContext();

        await db.Database.EnsureCreatedAsync();
        await ClearAllDataAsync(db);

        await SystemEntryInitializer.InitializeAsync(_services, _configuration);
        await SeedSystemEndpointsAsync();
    }

    /// <summary>
    /// Legt repräsentative Demo-Daten für die Screenshot-Erzeugung der Präsentationsseite an.
    /// Wird ausschließlich von <c>SiteScreenshotTests</c> nach <see cref="ResetAsync"/> aufgerufen.
    /// Alle Datensätze bleiben im Team-Speichermodus sichtbar, da der Owner-Filter
    /// nur im Benutzermodus greift.
    /// Ein separates Aufräumen von Alt-Daten ist nicht nötig: <see cref="ResetAsync"/>
    /// leert die Datenbank seit dem deterministischen <c>DELETE FROM</c>-Wipe
    /// (Issue #118) vollständig.
    /// </summary>
    public async Task SeedShowcaseDataAsync()
    {
        using var scope = _services.CreateScope();
        var appRepo = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
        var endpointRepo = scope.ServiceProvider.GetRequiredService<IEndpointRepository>();
        var environmentRepo = scope.ServiceProvider.GetRequiredService<ISystemEnvironmentRepository>();

        var (portalApp, orderApp) = await SeedGroupsAndApplicationsAsync(appRepo);
        await SeedEndpointsAsync(endpointRepo, portalApp, orderApp);
        await SeedEnvironmentsAsync(environmentRepo);
    }

    // Leert alle modellierten Tabellen per DELETE FROM — der einzig wirksame Reset auf der
    // benannten Shared-Cache-In-Memory-SQLite (EnsureDeletedAsync ist dort ein No-op, siehe
    // ResetAsync). Die Tabellenliste kommt aus dem EF-Modell statt aus einer handgepflegten
    // Liste: Sie deckt alle DbSets plus künftige Entities ab; Tabellen ohne Entity
    // (z. B. __EFMigrationsHistory) bleiben zu Recht außen vor.
    //
    // PRAGMA foreign_keys ist ein Per-Connection-Pragma und greift nur außerhalb einer
    // Transaktion — die Verbindung wird daher explizit geöffnet und ohne Transaktion
    // gearbeitet (Autocommit je ExecuteSqlRawAsync); zwischen zwei Aufrufen ginge das
    // Pragma auf einer anderen Verbindung sonst verloren. Mit deaktivierten FK-Checks
    // ist die Reihenfolge der Deletes beliebig.
    private static async Task ClearAllDataAsync(AppDbContext db)
    {
        await db.Database.OpenConnectionAsync();
        try
        {
            await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = 0");
            var tableNames = db.Model.GetEntityTypes()
                .Select(e => e.GetTableName())
                .Where(n => n != null)
                .Distinct();
            foreach (var tableName in tableNames)
#pragma warning disable EF1002 // Tabellenname stammt aus dem EF-Modell (GetTableName), nicht aus Nutzereingaben — Identifier sind ohnehin nicht parametrierbar.
                await db.Database.ExecuteSqlRawAsync($"DELETE FROM \"{tableName}\"");
#pragma warning restore EF1002
        }
        finally
        {
            // Restore als Best-Effort-Hygiene im finally: Stünde er im try, ginge er bei
            // einer Exception zwischen den Deletes verloren — die Korrektheit hinge dann
            // implizit am Non-Pooling-Verhalten der Mode=Memory-Verbindung. Schlägt der
            // Restore selbst fehl (bereits defekte Verbindung), ist das unkritisch: Die
            // Verbindung wird anschließend ohnehin geschlossen und verworfen.
            try
            {
                await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = 1");
            }
            catch (Exception)
            {
            }

            await db.Database.CloseConnectionAsync();
        }
    }

    private static async Task<(Application PortalApp, Application OrderApp)> SeedGroupsAndApplicationsAsync(
        IApplicationRepository appRepo)
    {
        var customerGroup = await appRepo.AddGroupAsync(new ApplicationGroup
        {
            Name = "Kundendienste",
            Description = "Externe Fachanwendungen des Kundenbereichs"
        });
        var internalGroup = await appRepo.AddGroupAsync(new ApplicationGroup
        {
            Name = "Interne Services",
            Description = "Interne Plattform- und Infrastrukturdienste"
        });

        var portalApp = await appRepo.AddApplicationAsync(new Application
        {
            Name = "Kundenportal API",
            Description = "REST-Schnittstelle des Kundenportals für Stammdaten und Vorgänge.",
            BaseUrl = "https://kundenportal.example.local",
            InterfaceUrl = "https://kundenportal.example.local/swagger/v1/swagger.json",
            InterfaceType = InterfaceType.Rest,
            Owner = @"TEAM\fachbereich",
            ApplicationGroupId = customerGroup.Id
        });
        var orderApp = await appRepo.AddApplicationAsync(new Application
        {
            Name = "Auftragsservice",
            Description = "Verarbeitung und Abfrage von Kundenaufträgen.",
            BaseUrl = "https://auftraege.example.local",
            Owner = @"TEAM\fachbereich",
            ApplicationGroupId = customerGroup.Id
        });
        await appRepo.AddApplicationAsync(new Application
        {
            Name = "Benachrichtigungsdienst",
            Description = "Versand von E-Mail- und Push-Benachrichtigungen.",
            BaseUrl = "https://notify.example.local",
            Owner = @"TEAM\plattform",
            ApplicationGroupId = internalGroup.Id
        });

        return (portalApp, orderApp);
    }

    private static async Task SeedEndpointsAsync(
        IEndpointRepository endpointRepo,
        Application portalApp,
        Application orderApp)
    {
        var masterDataGroup = await endpointRepo.AddEndpointGroupAsync(new EndpointGroup
        {
            Name = "Stammdaten",
            ApplicationId = portalApp.Id
        });
        await endpointRepo.AddEndpointAsync(new Endpoint
        {
            Name = "GET /api/kunden",
            Method = CoreHttpMethod.GET,
            RelativePath = "/api/kunden",
            ApplicationId = portalApp.Id,
            EndpointGroupId = masterDataGroup.Id
        });
        await endpointRepo.AddEndpointAsync(new Endpoint
        {
            Name = "POST /api/kunden",
            Method = CoreHttpMethod.POST,
            RelativePath = "/api/kunden",
            ApplicationId = portalApp.Id,
            EndpointGroupId = masterDataGroup.Id
        });
        await endpointRepo.AddEndpointAsync(new Endpoint
        {
            Name = "PUT /api/kunden/{id}",
            Method = CoreHttpMethod.PUT,
            RelativePath = "/api/kunden/{id}",
            ApplicationId = portalApp.Id,
            EndpointGroupId = masterDataGroup.Id
        });
        await endpointRepo.AddEndpointAsync(new Endpoint
        {
            Name = "DELETE /api/kunden/{id}",
            Method = CoreHttpMethod.DELETE,
            RelativePath = "/api/kunden/{id}",
            ApplicationId = portalApp.Id,
            EndpointGroupId = masterDataGroup.Id
        });
        await endpointRepo.AddEndpointAsync(new Endpoint
        {
            Name = "GET /api/status",
            Method = CoreHttpMethod.GET,
            RelativePath = "/api/status",
            ApplicationId = portalApp.Id
        });
        await endpointRepo.AddEndpointAsync(new Endpoint
        {
            Name = "GET /api/auftraege",
            Method = CoreHttpMethod.GET,
            RelativePath = "/api/auftraege",
            ApplicationId = orderApp.Id
        });
        await endpointRepo.AddEndpointAsync(new Endpoint
        {
            Name = "POST /api/auftraege",
            Method = CoreHttpMethod.POST,
            RelativePath = "/api/auftraege",
            ApplicationId = orderApp.Id
        });
    }

    private static async Task SeedEnvironmentsAsync(ISystemEnvironmentRepository environmentRepo)
    {
        await environmentRepo.AddAsync(new SystemEnvironment
        {
            Name = "Produktion",
            Description = "Produktivumgebung des Kundenbetriebs",
            Variables =
            [
                new EnvironmentVariable { Name = "basis-url", Value = "https://kundenportal.example.local" },
                new EnvironmentVariable { Name = "api-schluessel", Value = "prod-secret", IsValueMasked = true }
            ]
        });
        await environmentRepo.AddAsync(new SystemEnvironment
        {
            Name = "Test",
            Description = "Testumgebung für Fach- und Integrationstests",
            Variables =
            [
                new EnvironmentVariable { Name = "basis-url", Value = "https://kundenportal-test.example.local" }
            ]
        });
    }

    // Registriert die System-Endpunkte, die in Produktion via SystemEndpointSyncService aus Swagger
    // importiert werden. Im Playwright-Test-Environment ist dieser Service deaktiviert.
    private async Task SeedSystemEndpointsAsync()
    {
        using var scope = _services.CreateScope();
        var appRepo = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
        var endpointRepo = scope.ServiceProvider.GetRequiredService<IEndpointRepository>();

        var group = await appRepo.GetSystemGroupAsync();
        var systemApp = group?.Applications.FirstOrDefault(a => a.IsSystem);
        if (systemApp == null)
            return;

        await endpointRepo.AddEndpointAsync(new Endpoint
        {
            Name = "POST /authenticate",
            Method = CoreHttpMethod.POST,
            RelativePath = "/authenticate",
            ApplicationId = systemApp.Id
        });
    }
}
