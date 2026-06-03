using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
