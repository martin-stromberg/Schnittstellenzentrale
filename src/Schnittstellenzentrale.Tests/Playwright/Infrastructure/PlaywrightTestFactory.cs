using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Infrastructure.Data;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>Startet einen echten Kestrel-Testserver für Playwright-Tests mit Auth-Bypass, Datei-SQLite und Service-Overrides.</summary>
public class PlaywrightTestFactory : WebApplicationFactory<Program>
{
    private const string TestDbName = "schnittstellenzentrale-tests.db";
    private const string TestBaseUrl = "http://127.0.0.1:5099";

    /// <summary>Basis-URL des gestarteten Kestrel-Servers.</summary>
    public string BaseAddress { get; private set; } = string.Empty;

    /// <inheritdoc/>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseUrls(TestBaseUrl);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = $"Data Source={TestDbName}",
                ["DatabaseProvider"] = "SQLite"
            });
        });

        builder.ConfigureTestServices(ConfigureTestServices);
    }

    /// <summary>Registriert die Test-Services. Kann in Unterklassen überschrieben werden.</summary>
    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
        services.RemoveAll<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
        services.RemoveAll<Microsoft.AspNetCore.Authentication.IAuthenticationHandlerProvider>();
        services.RemoveAll<Microsoft.Extensions.Options.IConfigureOptions<AuthenticationOptions>>();

        services.AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

        services.RemoveAll<DbContextOptions<AppDbContext>>();
        services.RemoveAll<IDbContextFactory<AppDbContext>>();

        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={TestDbName}"));

        services.RemoveAll<IHostedService>();

        var signalRMock = new Mock<ISignalRNotificationService>();
        services.RemoveAll<ISignalRNotificationService>();
        services.AddScoped(_ => signalRMock.Object);

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(s => s.GetCurrentUserName()).Returns("TEST\\testuser");
        services.RemoveAll<ICurrentUserService>();
        services.AddSingleton(_ => currentUserMock.Object);
    }

    /// <inheritdoc/>
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Delete stale test DB so MigrateAsync starts clean
        foreach (var f in new[] { TestDbName, TestDbName + "-wal", TestDbName + "-shm" })
            if (File.Exists(f)) File.Delete(f);

        var host = base.CreateHost(builder);

        BaseAddress = TestBaseUrl;

        var configuration = host.Services.GetRequiredService<IConfiguration>();
        configuration["Api:BaseUrl"] = BaseAddress;

        using var scope = host.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var db = factory.CreateDbContext();
        db.Database.EnsureCreated();

        SystemEntryInitializer.InitializeAsync(host.Services, configuration).GetAwaiter().GetResult();

        return host;
    }
}
