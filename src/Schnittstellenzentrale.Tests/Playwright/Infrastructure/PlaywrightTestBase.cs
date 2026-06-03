using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>Basisklasse für alle Playwright-Tests; verwaltet Playwright-Lebenszyklus, Tracing und Datenbankzustand.</summary>
public abstract class PlaywrightTestBase : IAsyncLifetime
{
    private readonly PlaywrightServer _server;
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;

    /// <summary>Der aktive Browser-Kontext für den laufenden Test.</summary>
    protected IBrowserContext Context { get; private set; } = null!;

    /// <summary>Die aktive Seite des Browser-Kontexts.</summary>
    protected IPage Page { get; private set; } = null!;

    /// <summary>Die Basis-URL des Testservers.</summary>
    protected string BaseUrl { get; private set; } = string.Empty;

    /// <summary>Der Name der abgeleiteten Testklasse, verwendet für Trace-Dateinamen.</summary>
    protected virtual string TestName => GetType().Name;

    /// <summary>Initialisiert die Basisklasse mit dem gemeinsamen Playwright-Server.</summary>
    protected PlaywrightTestBase(PlaywrightServer server)
    {
        _server = server;
    }

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        var seeder = new TestDatabaseSeeder(
            _server.Services,
            _server.Services.GetRequiredService<IConfiguration>());
        await seeder.ResetAsync();

        BaseUrl = _server.BaseAddress;

        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
        Context = await _browser.NewContextAsync();

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
    protected async Task<IBrowserContext> CreateAdditionalContextAsync()
    {
        var context = await _browser.NewContextAsync();
        await context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });
        return context;
    }

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        Directory.CreateDirectory("playwright-traces");
        await Context.Tracing.StopAsync(new TracingStopOptions
        {
            Path = Path.Combine("playwright-traces", $"{TestName}.zip")
        });

        await OnDisposingAsync();

        await Context.DisposeAsync();
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    /// <summary>Wird vor dem Dispose der Basis-Ressourcen aufgerufen. Abgeleitete Klassen können hier zusätzliche Kontexte disposen.</summary>
    protected virtual Task OnDisposingAsync() => Task.CompletedTask;
}
