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

    /// <summary>Löscht das Datenbankschema, legt es neu an und führt den Seed-Prozess aus.</summary>
    public async Task ResetAsync()
    {
        using var scope = _services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var db = factory.CreateDbContext();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        await SystemEntryInitializer.InitializeAsync(_services, _configuration);
        await SeedSystemEndpointsAsync();
    }

    /// <summary>
    /// Legt repräsentative Demo-Daten für die Screenshot-Erzeugung der Präsentationsseite an.
    /// Wird ausschließlich von <c>SiteScreenshotTests</c> nach <see cref="ResetAsync"/> aufgerufen.
    /// Alle Datensätze bleiben im Team-Speichermodus sichtbar, da der Owner-Filter
    /// nur im Benutzermodus greift.
    /// Die Methode räumt zuvor alle Nicht-System-Daten auf: Auf der benannten
    /// Shared-Cache-In-Memory-SQLite der Playwright-Server wirkt
    /// <c>EnsureDeletedAsync</c> nicht zuverlässig, sodass sich Daten über die
    /// Facts einer Klasse sonst akkumulieren würden.
    /// </summary>
    public async Task SeedShowcaseDataAsync()
    {
        using var scope = _services.CreateScope();
        var appRepo = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
        var endpointRepo = scope.ServiceProvider.GetRequiredService<IEndpointRepository>();
        var environmentRepo = scope.ServiceProvider.GetRequiredService<ISystemEnvironmentRepository>();

        await ClearNonSystemDataAsync(appRepo, environmentRepo);

        var (portalApp, orderApp) = await SeedGroupsAndApplicationsAsync(appRepo);
        await SeedEndpointsAsync(endpointRepo, portalApp, orderApp);
        await SeedEnvironmentsAsync(environmentRepo);
    }

    // Entfernt Nicht-System-Gruppen (kaskadierend), ungruppierte Nicht-System-Anwendungen
    // und sämtliche Umgebungen; System-Einträge bleiben über den IsSystem-Filter erhalten.
    private static async Task ClearNonSystemDataAsync(
        IApplicationRepository appRepo,
        ISystemEnvironmentRepository environmentRepo)
    {
        foreach (var group in (await appRepo.GetGroupsAsync(StorageMode.Team, string.Empty)).Where(g => !g.IsSystem))
            await appRepo.DeleteGroupAsync(group.Id, ApplicationGroupDeleteMode.Delete);
        foreach (var app in (await appRepo.GetUngroupedApplicationsAsync(StorageMode.Team, string.Empty)).Where(a => !a.IsSystem))
            await appRepo.DeleteApplicationAsync(app.Id);
        foreach (var environment in await environmentRepo.GetEnvironmentsAsync(StorageMode.Team, null))
            await environmentRepo.DeleteAsync(environment.Id);
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
