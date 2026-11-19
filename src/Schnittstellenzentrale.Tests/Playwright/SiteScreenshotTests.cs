using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>
/// Erzeugt die eingecheckten Screenshot-PNGs der GitHub-Pages-Präsentationsseite unter
/// <c>site/screenshots/</c>. Die Facts sind Generatoren: Sie fahren die echte App mit
/// Showcase-Daten hoch, navigieren repräsentative Ansichten und schreiben PNG-Dateien
/// in das Repository-Arbeitsverzeichnis.
/// </summary>
[Collection("Playwright")]
public class SiteScreenshotTests : PlaywrightTestBase
{
    private const int ViewportWidth = 1440;
    private const int ViewportHeight = 900;

    /// <summary>Initialisiert den Test mit dem gemeinsamen Playwright-Server.</summary>
    /// <param name="server">Der von der Test-Collection bereitgestellte <see cref="PlaywrightServer"/>.</param>
    public SiteScreenshotTests(PlaywrightServer server) : base(server) { }

    /// <summary>Workspaces-Ansicht mit aufgeklapptem Baum im hellen Theme → site/screenshots/home-light.png.</summary>
    [Fact]
    public async Task Workspaces_Light_Screenshot()
    {
        await SeedShowcaseDataAsync();
        await PreparePageAsync();
        await ExpandShowcaseTreeAsync();

        await SaveScreenshotAsync("home-light.png");
    }

    /// <summary>Anwendungsdetailansicht im hellen Theme → site/screenshots/application-light.png.</summary>
    [Fact]
    public async Task ApplicationDetail_Light_Screenshot()
    {
        await SeedShowcaseDataAsync();
        await PreparePageAsync();
        await ExpandGroupAsync("Kundendienste", "Kundenportal API");

        var appButton = Page.Locator(".sz-tree-item-btn", new() { HasText = "Kundenportal API" });
        await ClickUntilVisibleAsync(appButton, Page.Locator(".sz-content-view .sz-hero"));

        await SaveScreenshotAsync("application-light.png");
    }

    /// <summary>Endpunktansicht im hellen Theme → site/screenshots/endpoint-light.png.</summary>
    [Fact]
    public async Task EndpointPage_Light_Screenshot()
    {
        await SeedShowcaseDataAsync();
        await PreparePageAsync();
        await ExpandGroupAsync("Kundendienste", "Kundenportal API");

        var appButton = Page.Locator(".sz-tree-item-btn", new() { HasText = "Kundenportal API" });
        await ClickUntilVisibleAsync(appButton, Page.Locator(".sz-content-view .sz-hero"));

        var endpointGroupRow = Page.Locator(".sz-tree-row", new()
        {
            Has = Page.Locator(".sz-tree-item-label[title='Stammdaten']")
        });
        var endpointButton = Page.Locator(".sz-tree-item-btn", new() { HasText = "GET /api/kunden" });
        await ClickUntilVisibleAsync(endpointGroupRow.Locator(".sz-tree-chevron-btn"), endpointButton);

        await ClickUntilVisibleAsync(endpointButton, Page.Locator(".sz-endpoint-page"));

        await SaveScreenshotAsync("endpoint-light.png");
    }

    /// <summary>Workspaces-Ansicht mit aufgeklapptem Baum im dunklen Theme → site/screenshots/home-dark.png.</summary>
    [Fact]
    public async Task Workspaces_Dark_Screenshot()
    {
        await SeedShowcaseDataAsync();
        await Context.AddInitScriptAsync("localStorage.setItem('colorScheme', 'Dark');");
        await PreparePageAsync();
        await Page.WaitForFunctionAsync("() => document.documentElement.classList.contains('dark')");
        await ExpandShowcaseTreeAsync();

        await SaveScreenshotAsync("home-dark.png");
    }

    private async Task SeedShowcaseDataAsync()
    {
        var seeder = new TestDatabaseSeeder(
            Server.Services,
            Server.Services.GetRequiredService<IConfiguration>());
        await seeder.SeedShowcaseDataAsync();
    }

    private async Task PreparePageAsync()
    {
        await Page.SetViewportSizeAsync(ViewportWidth, ViewportHeight);
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Assertions.Expect(Page.Locator(".sz-tree-node-text").First).ToBeVisibleAsync();
    }

    private async Task ExpandShowcaseTreeAsync()
    {
        await ExpandGroupAsync("Kundendienste", "Kundenportal API");
        await ExpandGroupAsync("Interne Services", "Benachrichtigungsdienst");

        var appRow = Page.Locator(".sz-tree-row", new()
        {
            Has = Page.Locator(".sz-tree-item-btn", new() { HasText = "Kundenportal API" })
        });
        var expandedChild = Page.Locator(".sz-tree-item-btn", new() { HasText = "GET /api/kunden" });
        await ClickUntilVisibleAsync(appRow.Locator(".sz-tree-chevron-btn"), Page.Locator(".sz-tree-item-label[title='Stammdaten']"));

        var endpointGroupRow = Page.Locator(".sz-tree-row", new()
        {
            Has = Page.Locator(".sz-tree-item-label[title='Stammdaten']")
        });
        await ClickUntilVisibleAsync(endpointGroupRow.Locator(".sz-tree-chevron-btn"), expandedChild);
    }

    private async Task ExpandGroupAsync(string groupName, string expectedChildText)
    {
        var section = Page.Locator(".collapsible-section", new()
        {
            Has = Page.Locator($".sz-tree-node-text[title='{groupName}']")
        });
        // Der erwartete Kindknoten wird innerhalb der Gruppen-Sektion gesucht,
        // damit die Expansion der tatsächlich angeforderten Gruppe verifiziert wird.
        var childButton = section.Locator(".sz-tree-item-btn", new() { HasText = expectedChildText });
        await ClickUntilVisibleAsync(section.Locator(".sz-tree-chevron-btn").First, childButton);
    }

    // Klicks vor Aufbau des Blazor-Circuits gehen verloren – deshalb bis zur Sichtbarkeit wiederholen.
    private static async Task ClickUntilVisibleAsync(ILocator trigger, ILocator expected)
    {
        var deadline = DateTime.UtcNow.AddSeconds(20);
        while (true)
        {
            await trigger.ClickAsync();
            try
            {
                await Assertions.Expect(expected).ToBeVisibleAsync(new() { Timeout = 1500 });
                return;
            }
            catch (PlaywrightException) when (DateTime.UtcNow < deadline)
            {
            }
        }
    }

    private async Task SaveScreenshotAsync(string fileName)
    {
        var directory = Path.Combine(TestPaths.FindRepoRoot(), "site", "screenshots");
        Directory.CreateDirectory(directory);
        await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(directory, fileName)
        });
    }
}
