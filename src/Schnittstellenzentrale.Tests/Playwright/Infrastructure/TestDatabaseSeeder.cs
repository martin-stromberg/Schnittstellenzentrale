using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Infrastructure.Data;

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
    }
}
