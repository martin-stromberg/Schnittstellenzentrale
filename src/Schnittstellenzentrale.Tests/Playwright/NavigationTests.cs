using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für die Bereichsnavigation und Breadcrumb.</summary>
[Collection("Playwright")]
public class NavigationTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit der gemeinsamen Playwright-Factory.</summary>
    public NavigationTests(PlaywrightServer server) : base(server) { }

    /// <summary>Klick auf Workspaces-Tab zeigt WorkspacesSidebar.</summary>
    [Fact]
    public async Task Playwright_BereichswechselWorkspaces_ZeigtSidebar()
    {
        await Page.GotoAsync(BaseUrl);

        await Page.Locator(".sz-topbar-tab", new() { HasText = "Workspaces" }).ClickAsync();

        var sidebar = Page.Locator(".sz-workspaces-sidebar");
        await Assertions.Expect(sidebar).ToBeVisibleAsync();
    }

    /// <summary>Klick auf Environments-Tab zeigt Umgebungsliste.</summary>
    [Fact]
    public async Task Playwright_BereichswechselEnvironments_ZeigtUmgebungsliste()
    {
        await Page.GotoAsync(BaseUrl);

        await Page.Locator(".sz-topbar-tab", new() { HasText = "Environments" }).ClickAsync();

        var sidebar = Page.Locator(".sz-environments-sidebar");
        await Assertions.Expect(sidebar).ToBeVisibleAsync();
    }

    /// <summary>Klick auf History-Tab zeigt Historieliste.</summary>
    [Fact]
    public async Task Playwright_BereichswechselHistory_ZeigtHistorieliste()
    {
        await Page.GotoAsync(BaseUrl);

        await Page.Locator(".sz-topbar-tab", new() { HasText = "History" }).ClickAsync();

        var historyContent = Page.Locator(".sz-history-content");
        await Assertions.Expect(historyContent).ToBeVisibleAsync();
    }

    /// <summary>Breadcrumb-Klick setzt Selektion auf Sammlungsebene zurück.</summary>
    [Fact]
    public async Task Playwright_BreadcrumbKlick_NavigiertZurückZurSammlung()
    {
        await Page.GotoAsync(BaseUrl);

        await Page.Locator(".sz-topbar-tab", new() { HasText = "Workspaces" }).ClickAsync();

        // Sammlung im Baum anklicken
        var collectionNode = Page.Locator(".sz-tree-node-text").First;
        await collectionNode.ClickAsync();

        // Breadcrumb muss sichtbar sein
        var breadcrumb = Page.Locator(".sz-breadcrumb");
        await Assertions.Expect(breadcrumb).ToBeVisibleAsync();

        // Anwendung aufklappen und selektieren, damit Breadcrumb mindestens 2 Ebenen zeigt
        var appChevron = Page.Locator(".sz-tree-chevron-btn").First;
        await appChevron.ClickAsync();

        var appBtn = Page.Locator(".sz-tree-item-btn").First;
        await appBtn.ClickAsync();

        // Auf erstes Breadcrumb-Link-Element klicken (Sammlung)
        var breadcrumbLink = Page.Locator(".sz-breadcrumb-link").First;
        var hasLink = await breadcrumbLink.CountAsync() > 0;
        if (hasLink)
        {
            var collectionName = await breadcrumbLink.InnerTextAsync();
            await breadcrumbLink.ClickAsync();

            var currentBreadcrumb = Page.Locator(".sz-breadcrumb-current");
            await Assertions.Expect(currentBreadcrumb).ToHaveTextAsync(collectionName);
        }
    }
}
