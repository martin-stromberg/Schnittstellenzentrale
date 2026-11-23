using System.Globalization;
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
    /// <value>Die gemeinsame Proxy-Instanz für SignalR-Benachrichtigungen.</value>
    private protected readonly SignalRNotificationProxy ApiNotificationProxy = new();

    /// <summary>Bind-URL des Kestrel-Servers; kann in Unterklassen überschrieben werden um Port-Konflikte zu vermeiden.</summary>
    protected virtual string BindUrl => "http://127.0.0.1:5099";

    /// <summary>Basis-URL des gestarteten Kestrel-Servers.</summary>
    public string BaseAddress => BindUrl;

    /// <summary>Service-Provider des laufenden Servers.</summary>
    public IServiceProvider Services => _app!.Services;

    /// <summary>Kann in Unterklassen überschrieben werden, um weitere Services zu ersetzen.</summary>
    /// <param name="services">Die Service-Collection der Testanwendung.</param>
    protected virtual void ConfigureTestServices(IServiceCollection services) { }

    /// <summary>Wird nach dem Start des Kestrel-Servers aufgerufen. Unterklassen können hier z.B. den <see cref="ApiNotificationProxy"/> konfigurieren.</summary>
    protected virtual Task OnAfterStartAsync() => Task.CompletedTask;

    /// <inheritdoc/>
    /// <remarks>Die Init-Phasen tragen Marker; Fehler werden mit Phasen-Label, BindUrl und
    /// Ursache erneut geworfen, damit ein Startfehler im TRX diagnostizierbar bleibt statt
    /// erst beim ersten <c>Page.GotoAsync</c> als Timeout-Flake aufzutreten.</remarks>
    public virtual async Task InitializeAsync()
    {
        var phase = "Anchor-DB";
        try
        {
            // Alle Server-Threads rendern Blazor-Komponenten auf Deutsch, unabhängig vom System-Locale der Testmaschine.
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("de-DE");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("de-DE");

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

            phase = "BuildWebApplicationAsync";
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
                    swaggerMock.Setup(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>(), It.IsAny<int>())).Returns(Task.CompletedTask);
                    services.RemoveAll<ISwaggerImportService>();
                    services.AddScoped(_ => swaggerMock.Object);

                    // Icon-Upload-Limit auf 512 KB setzen (Standardwert), da appsettings.json 5 MB konfiguriert
                    services.PostConfigure<Schnittstellenzentrale.Infrastructure.Services.UploadSettings>(o => o.MaxIconSizeBytes = 524288);

                    // Impressum-Pfad auf einen garantiert nicht vorhandenen Pfad setzen, damit
                    // ImpressumService.IsAvailable() false zurückgibt und der Sidebar-Link ausgeblendet bleibt.
                    var noImpressumPath = Path.Combine(Path.GetTempPath(), $"no-impressum-{Guid.NewGuid():N}.md");
                    services.PostConfigure<Schnittstellenzentrale.Infrastructure.Services.ImpressumSettings>(o => o.FilePath = noImpressumPath);

                    ConfigureTestServices(services);
                });

            _app.Urls.Clear();
            _app.Urls.Add(BindUrl);
            phase = "StartAsync";
            await _app.StartAsync();

            // Echten Kestrel-Socket abpollen: eine nicht erreichbare Bindung (Port-Kollision,
            // gestörter Listener) soll hier als Startfehler sichtbar werden, nicht erst als
            // Timeout beim ersten Page.GotoAsync des Tests.
            phase = "WaitForServerReady";
            await WaitForServerReadyAsync();

            phase = "OnAfterStartAsync";
            await OnAfterStartAsync();

            // Swagger-Dokument vorwärmen (Swashbuckle braucht beim ersten Aufruf länger)
            phase = "Swagger-Warmup";
            using var warmupClient = _apiFactory.CreateClient();
            using var warmupResponse = await warmupClient.GetAsync("/swagger/v1/swagger.json");
        }
        catch (Exception error)
        {
            throw new InvalidOperationException(
                $"PlaywrightServer-Initialisierung fehlgeschlagen in Phase '{phase}' "
                + $"(BindUrl: {BindUrl}): {error.Message}", error);
        }
    }

    // Deadline für den Server-Readiness-Poll: größer als jedes Phasenbudget,
    // weit unter dem 45-min-Job-Timeout der os-interface-tests-Lane.
    private static readonly TimeSpan ServerReadyDeadline = TimeSpan.FromSeconds(60);

    // Poll-Intervall des Server-Readiness-Polls.
    private static readonly TimeSpan ServerReadyPollInterval = TimeSpan.FromMilliseconds(500);

    /// <summary>Wartet, bis der gebundene Kestrel-Socket HTTP-Anfragen beantwortet.
    /// <b>Jede</b> empfangene HTTP-Antwort gilt als „ready" — Status egal (auch 404 oder
    /// Redirects zählen): Die Readiness-Frage ist „Kestrel-Listener + Middleware-Pipeline
    /// beantworten Requests", nicht „Route existiert/2xx". Nur Verbindungsfehler und
    /// Request-Timeouts zählen als nicht-bereit.</summary>
    /// <exception cref="TimeoutException">Der Server antwortete nicht innerhalb von
    /// <see cref="ServerReadyDeadline"/>; die Meldung enthält BindUrl und letzten Fehler.</exception>
    private async Task WaitForServerReadyAsync()
    {
        var deadline = DateTime.UtcNow + ServerReadyDeadline;
        Exception? lastError = null;
        // Kein System-Proxy: Auf Runnern mit gesetzten HTTP_PROXY-Variablen würde die
        // 127.0.0.1-Anfrage sonst über den Proxy geroutet und falsch als "nicht bereit"
        // gewertet (genau das Timeout-Flake-Bild, das dieser Poll vermeiden soll).
        using var httpClient = new HttpClient(new HttpClientHandler { UseProxy = false }) { Timeout = TimeSpan.FromSeconds(5) };
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var response = await httpClient.GetAsync(BindUrl);
                return;
            }
            catch (Exception error) when (error is HttpRequestException or TaskCanceledException or OperationCanceledException)
            {
                lastError = error;
                await Task.Delay(ServerReadyPollInterval);
            }
        }

        throw new TimeoutException(
            $"Kestrel-Server unter {BindUrl} antwortete nicht innerhalb von "
            + $"{ServerReadyDeadline.TotalSeconds}s. Letzter Fehler: {lastError?.Message ?? "kein Request erreicht"}");
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
    /// <remarks>Dispose ist komplett best-effort: Jeder Schritt läuft einzeln, Fehler werden
    /// gesammelt und nach <see cref="Console.Error"/> protokolliert — die Methode wirft nie,
    /// damit ein Cleanup-Fehler den eigentlichen Test-Assert nicht überdeckt.</remarks>
    public virtual async Task DisposeAsync()
    {
        var errors = new List<Exception>();

        if (_app is not null)
        {
            try { await _app.StopAsync(); }
            catch (Exception error) { errors.Add(error); }
            try { await _app.DisposeAsync(); }
            catch (Exception error) { errors.Add(error); }
        }

        try { _apiFactory?.Dispose(); }
        catch (Exception error) { errors.Add(error); }

        try { _anchorConnection?.Dispose(); }
        catch (Exception error) { errors.Add(error); }

        foreach (var error in errors)
        {
            Console.Error.WriteLine(
                $"PlaywrightServer.DisposeAsync: Cleanup-Fehler (best-effort, nicht werfend): {error}");
        }
    }
}
