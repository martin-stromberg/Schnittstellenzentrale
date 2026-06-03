using AngleSharp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Moq;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Data;
using Schnittstellenzentrale.Services;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>
/// Startet einen echten Kestrel-Server für Playwright-Tests.
/// API-Calls des Blazor-Servers laufen über einen In-Process-Handler gegen <see cref="PlaywrightApiFactory"/> —
/// kein TCP, kein Kestrel-Deadlock.
/// Beide Server teilen eine SQLite-In-Memory-Verbindung — kein Datei-Locking zwischen Testläufen.
/// </summary>
public class PlaywrightServer : IAsyncLifetime
{
    /// <summary>Die gestartete WebApplication-Instanz.</summary>
    protected WebApplication? _app;
    private PlaywrightApiFactory? _apiFactory;
    private SqliteConnection? _anchorConnection;
    private string _dbConnectionString = string.Empty;
    /// <summary>Proxy für den SignalR-Notification-Service der PlaywrightApiFactory; kann nach dem App-Start konfiguriert werden.</summary>
    private protected readonly SignalRNotificationProxy ApiNotificationProxy = new();

    /// <summary>Bind-URL des Kestrel-Servers; kann in Unterklassen überschrieben werden um Port-Konflikte zu vermeiden.</summary>
    protected virtual string BindUrl => "http://127.0.0.1:5099";

    /// <summary>Basis-URL des gestarteten Kestrel-Servers.</summary>
    public string BaseAddress => BindUrl;

    /// <summary>Service-Provider des laufenden Servers.</summary>
    public IServiceProvider Services => _app!.Services;

    /// <summary>Kann in Unterklassen überschrieben werden, um weitere Services zu ersetzen.</summary>
    protected virtual void ConfigureTestServices(IServiceCollection services) { }

    /// <summary>Wird nach dem Start des Kestrel-Servers aufgerufen. Unterklassen können hier z.B. den <see cref="ApiNotificationProxy"/> konfigurieren.</summary>
    protected virtual Task OnAfterStartAsync() => Task.CompletedTask;

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        // Eindeutiger Name verhindert Kollisionen zwischen parallelen Test-Collections.
        // Die Anchor-Verbindung bleibt offen, damit SQLite die In-Memory-DB nicht verwirft.
        var dbName = Guid.NewGuid().ToString("N");
        _dbConnectionString = $"Data Source={dbName};Mode=Memory;Cache=Shared";
        _anchorConnection = new SqliteConnection(_dbConnectionString);
        _anchorConnection.Open();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(s => s.GetCurrentUserName()).Returns(@"TEST\testuser");

        _apiFactory = new PlaywrightApiFactory(_dbConnectionString, currentUserMock.Object, ApiNotificationProxy);
        var inProcessHandler = _apiFactory.Server.CreateHandler();

        var contentRoot = FindWebProjectRoot();

        _app = await Program.BuildWebApplicationAsync(
            [],
            new WebApplicationOptions
            {
                EnvironmentName = "Playwright",
                ContentRootPath = contentRoot,
                ApplicationName = typeof(Program).Assembly.GetName().Name
            },
            services =>
            {
                services.RemoveAll<IAuthenticationSchemeProvider>();
                services.RemoveAll<IAuthenticationHandlerProvider>();
                services.RemoveAll<IConfigureOptions<AuthenticationOptions>>();
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

                services.RemoveAll<IHostedService>();

                services.RemoveAll<ISignalRNotificationService>();
                services.AddScoped(_ => (ISignalRNotificationService)ApiNotificationProxy);

                services.RemoveAll<ICurrentUserService>();
                services.AddSingleton(_ => currentUserMock.Object);

                // Jeder Server öffnet eine eigene Verbindung zur benannten In-Memory-DB.
                // Geteiltes Verbindungsobjekt würde bei gleichzeitigem Zugriff zu SQLite-Fehlern führen.
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<IDbContextFactory<AppDbContext>>();
                services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(_dbConnectionString));

                // API-Calls (ApplicationApiClient u.a.) über In-Process-Handler statt echtem TCP
                services.PostConfigureAll<HttpClientFactoryOptions>(options =>
                    options.HttpMessageHandlerBuilderActions.Add(b => b.PrimaryHandler = inProcessHandler));

                // SwaggerImportService: der HTTP-Fetch von swagger.json hängt im Playwright-Test-Environment.
                // Playwright-Tests prüfen den UI-Fluss — das Swagger-Parsing testet SwaggerImportServiceTests.
                var swaggerMock = new Mock<ISwaggerImportService>();
                swaggerMock.Setup(s => s.ImportAsync(It.IsAny<Application>()))
                    .ReturnsAsync(new ImportDiff
                    {
                        NewEndpoints = [new Endpoint { Name = "GET /api/test", Method = Core.Enums.HttpMethod.GET, RelativePath = "/api/test", ApplicationId = 0 }]
                    });
                swaggerMock.Setup(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>())).Returns(Task.CompletedTask);
                services.RemoveAll<ISwaggerImportService>();
                services.AddScoped(_ => swaggerMock.Object);

                ConfigureTestServices(services);
            });

        _app.Urls.Clear();
        _app.Urls.Add(BindUrl);
        await _app.StartAsync();
        await OnAfterStartAsync();

        // Swagger-Dokument vorwärmen (Swashbuckle braucht beim ersten Aufruf länger)
        using var warmupClient = _apiFactory.CreateClient();
        await warmupClient.GetAsync("/swagger/v1/swagger.json");
    }

    private static string FindWebProjectRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            var candidate = Path.Combine(current.FullName, "src", "Schnittstellenzentrale");
            if (Directory.Exists(Path.Combine(candidate, "wwwroot")))
                return candidate;
            current = current.Parent;
        }
        throw new DirectoryNotFoundException(
            "Schnittstellenzentrale-Projektverzeichnis nicht gefunden. Suche startete in: " + AppContext.BaseDirectory);
    }

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
        _apiFactory?.Dispose();
        _anchorConnection?.Dispose();
    }
}
