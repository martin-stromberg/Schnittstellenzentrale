using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>Basisklasse für alle Playwright-Tests; verwaltet Playwright-Lebenszyklus, Tracing und Datenbankzustand.</summary>
public abstract class PlaywrightTestBase : IAsyncLifetime
{
    // Budget für den Trace-Stop beim Dispose. Überschreitung wird nur geloggt;
    // das Trace-ZIP wird ggf. verspätet geschrieben (Tracing.StopAsync kennt kein
    // CancellationToken).
    private static readonly TimeSpan TraceStopBudget = TimeSpan.FromSeconds(30);

    /// <summary>Timeout für einzelne Playwright-Aktionen (Assertions, Klicks, Locator).
    /// Startwert: der Playwright-Default (30 s), hier explizit gemacht.</summary>
    private const float DefaultActionTimeoutMs = 30_000;

    /// <summary>Timeout für Navigationen. Blazor-Server-Erstaufruf (SignalR-Connect +
    /// Circuit) kann unter CI-Last über 30 s liegen; 60 s bleibt weit unter dem
    /// Job-Timeout der os-interface-tests-Lane (45 min).</summary>
    private const float DefaultNavigationTimeoutMs = 60_000;

    // Verzeichnis für Trace-ZIPs — absolut am Repo-Root verankert über
    // TestPaths.FindRepoRoot(). Ein relativer Pfad läge unter dem
    // Testhost-Arbeitsverzeichnis (bin/<cfg>/net11.0/) und der CI-Upload
    // "playwright-traces/" fände die Dateien nie.
    private static readonly string TraceDirectory =
        Path.Combine(TestPaths.FindRepoRoot(), "playwright-traces");

    private readonly PlaywrightServer _server;
    private readonly ITestOutputHelper _output;
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    /// <summary>Der aktive Browser-Kontext für den laufenden Test.</summary>
    protected IBrowserContext Context { get; private set; } = null!;

    /// <summary>Die aktive Seite des Browser-Kontexts.</summary>
    protected IPage Page { get; private set; } = null!;

    /// <summary>Die Basis-URL des Testservers.</summary>
    protected string BaseUrl { get; private set; } = string.Empty;

    /// <summary>Der Name der abgeleiteten Testklasse, verwendet für Trace-Dateinamen.</summary>
    /// <value>Der Name der abgeleiteten Testklasse.</value>
    protected virtual string TestName => GetType().Name;

    /// <summary>Die gemeinsame Playwright-Server-Instanz der Test-Collection.</summary>
    /// <value>Der <see cref="PlaywrightServer"/> für Zugriff auf <see cref="PlaywrightServer.Services"/> und <see cref="PlaywrightServer.BaseAddress"/>.</value>
    protected PlaywrightServer Server => _server;

    /// <summary>Initialisiert die Basisklasse mit dem gemeinsamen Playwright-Server.</summary>
    /// <param name="server">Der von der Test-Collection bereitgestellte <see cref="PlaywrightServer"/>.</param>
    /// <param name="output">xUnit-Testausgabe für Dispose-/Timeout-Diagnosen (landet im
    /// TRX-<c>&lt;Output&gt;</c>-Block statt im nicht-kapturierten Konsolen-Stream).</param>
    protected PlaywrightTestBase(PlaywrightServer server, ITestOutputHelper output)
    {
        _server = server;
        _output = output;
    }

    /// <inheritdoc/>
    public async ValueTask InitializeAsync()
    {
        var seeder = new TestDatabaseSeeder(
            _server.Services,
            _server.Services.GetRequiredService<IConfiguration>());
        await seeder.ResetAsync();

        BaseUrl = _server.BaseAddress;

        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = TestPaths.IsHeadless() });
        Context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            Locale = "de-DE",
            ExtraHTTPHeaders = new Dictionary<string, string> { ["Accept-Language"] = "de-DE,de;q=0.9" }
        });
        ApplyDefaultTimeouts(Context);

        await Context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });

        Page = await Context.NewPageAsync();

        await OnInitializedAsync();
    }

    /// <summary>Wird nach der Playwright-Initialisierung aufgerufen. Abgeleitete Klassen können hier zusätzliche Browser-Kontexte anlegen.</summary>
    protected virtual Task OnInitializedAsync() => Task.CompletedTask;

    /// <summary>Legt einen weiteren Browser-Kontext mit aktiviertem Tracing an.</summary>
    /// <returns>Der neu angelegte Browser-Kontext.</returns>
    protected Task<IBrowserContext> CreateAdditionalContextAsync()
        => CreateAdditionalContextAsync("de-DE", "de-DE,de;q=0.9");

    /// <summary>Legt einen weiteren Browser-Kontext mit der angegebenen Locale und aktiviertem Tracing an.</summary>
    /// <param name="locale">Die Locale des Browser-Kontexts.</param>
    /// <param name="acceptLanguage">Der Accept-Language-Header des Browser-Kontexts.</param>
    /// <returns>Der neu angelegte Browser-Kontext.</returns>
    protected async Task<IBrowserContext> CreateAdditionalContextAsync(string locale, string acceptLanguage)
    {
        var context = await _browser!.NewContextAsync(new BrowserNewContextOptions
        {
            Locale = locale,
            ExtraHTTPHeaders = new Dictionary<string, string> { ["Accept-Language"] = acceptLanguage }
        });
        ApplyDefaultTimeouts(context);
        await context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });
        return context;
    }

    /// <summary>Setzt die dokumentierten Default-Timeouts auf einem Browser-Kontext.</summary>
    /// <param name="context">Der zu konfigurierende Browser-Kontext.</param>
    private static void ApplyDefaultTimeouts(IBrowserContext context)
    {
        context.SetDefaultTimeout(DefaultActionTimeoutMs);
        context.SetDefaultNavigationTimeout(DefaultNavigationTimeoutMs);
    }

    /// <inheritdoc/>
    /// <remarks>Dispose ist komplett best-effort: Jeder Schritt läuft einzeln mit
    /// Fehlersammlung, Trace-Stop mit <see cref="TraceStopBudget"/>-Deadline — ein
    /// Cleanup-Fehler darf den eigentlichen Test-Assert nie überdecken (kein
    /// AggregateException-/Dispose-Overlay mehr). Die Methode wirft nie.</remarks>
    public async ValueTask DisposeAsync()
    {
        var errors = new List<Exception>();

        if (Context is not null)
            await StopTraceWithBudgetAsync(Context, $"{TestName}.zip", errors);

        try { await OnDisposingAsync(); }
        catch (Exception error) { errors.Add(error); }

        if (Context is not null)
        {
            try { await Context.DisposeAsync(); }
            catch (Exception error) { errors.Add(error); }
        }

        if (_browser is not null)
        {
            try { await _browser.DisposeAsync(); }
            catch (Exception error) { errors.Add(error); }
        }

        if (_playwright is not null)
        {
            try { _playwright.Dispose(); }
            catch (Exception error) { errors.Add(error); }
        }

        foreach (var error in errors)
            LogDisposeError($"PlaywrightTestBase.DisposeAsync: Cleanup-Fehler (best-effort, nicht werfend): {error}");
    }

    /// <summary>Wird vor dem Dispose der Basis-Ressourcen aufgerufen. Abgeleitete Klassen können hier zusätzliche Kontexte disposen.</summary>
    protected virtual Task OnDisposingAsync() => Task.CompletedTask;

    /// <summary>Stoppt das Tracing eines zusätzlichen Browser-Kontexts best-effort mit dem
    /// <see cref="TraceStopBudget"/> — für abgeleitete Klassen mit eigenen Kontexten
    /// (z. B. SignalRSyncTests). Fehler werden nur geloggt, nie geworfen.</summary>
    /// <param name="context">Der Browser-Kontext, dessen Tracing gestoppt wird.</param>
    /// <param name="traceFileName">Basis-Dateiname des Trace-ZIP (ohne Verzeichnis);
    /// wird intern um einen UTC-Zeitstempel-Suffix erweitert.</param>
    /// <returns>Ein Task für den asynchronen Abschluss.</returns>
    protected async Task StopTraceBestEffortAsync(IBrowserContext context, string traceFileName)
    {
        var errors = new List<Exception>();
        await StopTraceWithBudgetAsync(context, traceFileName, errors);
        foreach (var error in errors)
            LogDisposeError($"StopTraceBestEffortAsync '{traceFileName}': {error}");
    }

    /// <summary>Stoppt das Tracing mit Zeitbudget. Bei Budget-Überschreitung wird der
    /// verwaiste Stop-Task per <see cref="TaskContinuationOptions.OnlyOnFaulted"/>-Continuation
    /// beobachtet (sonst droht UnobservedTaskException; das ZIP kann verspätet unter dem
    /// Folge-Dispose geschrieben werden — akzeptiert und dokumentiert).</summary>
    /// <param name="context">Der Browser-Kontext, dessen Tracing gestoppt wird.</param>
    /// <param name="traceFileName">Zieldateiname des Trace-ZIPs (unter dem Trace-Verzeichnis).</param>
    /// <param name="errors">Fehlersammlung, in die Restfehler eingetragen werden.</param>
    private async Task StopTraceWithBudgetAsync(IBrowserContext context, string traceFileName, List<Exception> errors)
    {
        try
        {
            Directory.CreateDirectory(TraceDirectory);
            var uniqueFileName = UniqueTraceFileName(traceFileName);
            var stopTask = context.Tracing.StopAsync(new TracingStopOptions
            {
                Path = Path.Combine(TraceDirectory, uniqueFileName)
            });
            var completed = await Task.WhenAny(stopTask, Task.Delay(TraceStopBudget));
            if (completed == stopTask)
            {
                await stopTask;
                return;
            }

            var message = $"Tracing.StopAsync für '{uniqueFileName}' überschritt das Budget von "
                + $"{TraceStopBudget.TotalSeconds}s — das Trace-ZIP wird ggf. verspätet geschrieben.";
            _output.WriteLine(message);
            errors.Add(new TimeoutException(message));
            // Verwaisten Task beobachten, damit kein UnobservedTaskException droht.
            _ = stopTask.ContinueWith(
                t => Console.Error.WriteLine(
                    $"PlaywrightTestBase: nachträglich fehlgeschlagener Trace-Stop '{uniqueFileName}': "
                    + t.Exception?.GetBaseException().Message),
                TaskContinuationOptions.OnlyOnFaulted);
        }
        catch (Exception error)
        {
            errors.Add(error);
        }
    }

    /// <summary>Ergänzt einen Trace-Dateinamen um einen UTC-Zeitstempel-Suffix
    /// (<c>HHmmssfff</c>), damit mehrere Tests derselben Klasse einander das
    /// Trace-ZIP nicht überschreiben.</summary>
    /// <param name="fileName">Der Basis-Dateiname inkl. Erweiterung.</param>
    /// <returns>Der Dateiname mit eingefügtem Zeitstempel vor der Erweiterung.</returns>
    private static string UniqueTraceFileName(string fileName)
        => $"{Path.GetFileNameWithoutExtension(fileName)}-{DateTime.UtcNow:HHmmssfff}{Path.GetExtension(fileName)}";

    private void LogDisposeError(string message)
    {
        try { _output.WriteLine(message); }
        catch (Exception) { /* Output-Helper darf den Dispose-Pfad nicht brechen. */ }
        Console.Error.WriteLine(message);
    }
}
