using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Infrastructure.Testing;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für das Auf- und Zuklappen von Baumknoten über Titel und Chevron.</summary>
[Collection("Playwright")]
[OsInterface]
public class TreeCollapseTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit der gemeinsamen Playwright-Factory.</summary>
    /// <param name="server">Die gemeinsame Server-Fixture der Test-Collection.</param>
    /// <param name="output">Testausgabe-Hilfsinstanz für Diagnose- und Dispose-Logs.</param>
    public TreeCollapseTests(PlaywrightServer server, ITestOutputHelper output) : base(server, output) { }

    /// <summary>Ein Klick auf den ApplicationGroup-Titeltext klappt den Knoten zu; ein zweiter Klick klappt ihn wieder auf.</summary>
    [Fact]
    public async Task ClickApplicationGroupTitle_TogglesCollapse()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "+ Neue Sammlung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("Collapse-Test-Gruppe");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();
        await Microsoft.Playwright.Assertions.Expect(Page.Locator(".sz-alert-danger")).Not.ToBeVisibleAsync();

        await Page.Locator(".sz-workspaces-sidebar").GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "+ Neue Anwendung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("Anwendung-In-Gruppe");
        await Page.GetByLabel("Basis-URL").FillAsync("http://test.example.com");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();
        await Microsoft.Playwright.Assertions.Expect(Page.Locator(".sz-alert-danger")).Not.ToBeVisibleAsync();

        var groupRow = Page.Locator(".sz-tree-row", new() { Has = Page.Locator(".sz-tree-node-text", new() { HasText = "Collapse-Test-Gruppe" }) });
        var titleSpan = groupRow.Locator(".sz-tree-node-text");

        await titleSpan.ClickAsync();

        await Microsoft.Playwright.Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "Anwendung-In-Gruppe" })).Not.ToBeVisibleAsync();

        await titleSpan.ClickAsync();

        await Microsoft.Playwright.Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "Anwendung-In-Gruppe" })).ToBeVisibleAsync();
    }

    /// <summary>Ein Klick auf den Anwendungsnamen klappt die Anwendung auf und zeigt untergeordnete Elemente an.</summary>
    [Fact]
    public async Task ClickApplicationName_ExpandsApplication()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "+ Neue Anwendung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("Expand-Test-Anwendung");
        await Page.GetByLabel("Basis-URL").FillAsync("http://test.example.com");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Microsoft.Playwright.Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "Expand-Test-Anwendung" })).ToBeVisibleAsync();

        var appRow = Page.Locator(".sz-tree-row", new() { Has = Page.Locator(".sz-tree-item-btn", new() { HasText = "Expand-Test-Anwendung" }) });
        var appBtn = appRow.Locator(".sz-tree-item-btn");

        await appBtn.ClickAsync();

        await Microsoft.Playwright.Assertions.Expect(appRow.Locator(".sz-tree-chevron-btn .sz-icon-chevron-down")).ToBeVisibleAsync();
    }

    /// <summary>Ein Klick auf den Anwendungsnamen selektiert die Anwendung.</summary>
    [Fact]
    public async Task ClickApplicationName_SelectsApplication()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "+ Neue Anwendung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("Select-Test-Anwendung");
        await Page.GetByLabel("Basis-URL").FillAsync("http://test.example.com");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Microsoft.Playwright.Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "Select-Test-Anwendung" })).ToBeVisibleAsync();

        var appRow = Page.Locator(".sz-tree-row", new() { Has = Page.Locator(".sz-tree-item-btn", new() { HasText = "Select-Test-Anwendung" }) });
        await appRow.Locator(".sz-tree-item-btn").ClickAsync();

        await Microsoft.Playwright.Assertions.Expect(Page.Locator(".sz-content-title", new() { HasText = "Select-Test-Anwendung" })).ToBeVisibleAsync();
    }

    /// <summary>Ein Klick auf einen bereits aufgeklappten Anwendungsnamen klappt die Anwendung wieder zu.</summary>
    [Fact]
    public async Task ClickApplicationName_CollapsesExpandedApplication()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "+ Neue Anwendung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("Collapse-App-Test");
        await Page.GetByLabel("Basis-URL").FillAsync("http://test.example.com");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Microsoft.Playwright.Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "Collapse-App-Test" })).ToBeVisibleAsync();

        var appRow = Page.Locator(".sz-tree-row", new() { Has = Page.Locator(".sz-tree-item-btn", new() { HasText = "Collapse-App-Test" }) });
        var appBtn = appRow.Locator(".sz-tree-item-btn");

        await appBtn.ClickAsync();
        await Microsoft.Playwright.Assertions.Expect(appRow.Locator(".sz-tree-chevron-btn .sz-icon-chevron-down")).ToBeVisibleAsync();

        await appBtn.ClickAsync();
        await Microsoft.Playwright.Assertions.Expect(appRow.Locator(".sz-tree-chevron-btn .sz-icon-chevron-right")).ToBeVisibleAsync();
    }

    // Klappt die Systemgruppe auf und verifiziert das Ergebnis (Flake-Mechanismus:
    // siehe PlaywrightExpand.ExpandWithRetryAsync). Erfolgskriterium ist die sichtbare
    // App-Schaltfläche, geklickt wird der Gruppen-Chevron.
    private Task ExpandSystemGroupAsync()
    {
        var appBtn = SystemAppRow.Locator(".sz-tree-item-btn");
        var groupChevron = Page.Locator(".collapsible-section .sz-tree-chevron-btn").First;

        return PlaywrightExpand.ExpandWithRetryAsync(appBtn, groupChevron);
    }

    // Klappt die Systemanwendung auf und verifiziert das Ergebnis — Spiegelbild von
    // ExpandSystemGroupAsync eine Ebene tiefer (Flake-Mechanismus: siehe
    // PlaywrightExpand.ExpandWithRetryAsync). Erfolgskriterium ist der Down-Chevron
    // der Anwendung, geklickt wird die App-Schaltfläche.
    private Task ExpandSystemApplicationAsync()
    {
        var appBtn = SystemAppRow.Locator(".sz-tree-item-btn");
        var expandedChevron = SystemAppRow.Locator(".sz-tree-chevron-btn .sz-icon-chevron-down");

        return PlaywrightExpand.ExpandWithRetryAsync(expandedChevron, appBtn);
    }

    private ILocator SystemAppRow =>
        Page.Locator(".sz-tree-row", new() { Has = Page.Locator(".sz-tree-item-btn", new() { HasText = "Schnittstellenzentrale" }) });

    /// <summary>Ein Klick auf den Endpunktordner-Namen klappt den Ordner auf; ein zweiter Klick klappt ihn wieder zu.</summary>
    [Fact]
    public async Task ClickEndpointGroupName_TogglesCollapse()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await ExpandSystemGroupAsync();

        var contextMenuToggle = SystemAppRow.Locator("[data-testid=\"context-menu-toggle\"]");
        await contextMenuToggle.ClickAsync();
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Ordner anlegen" }).ClickAsync();
        var nameInput = Page.GetByLabel("Name");
        await nameInput.FillAsync("Collapse-Ordner-Test");
        await nameInput.PressAsync("Tab");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Anlegen" }).ClickAsync();

        await ExpandSystemApplicationAsync();

        var groupRow = Page.Locator(".sz-tree-row", new() { Has = Page.GetByText("Collapse-Ordner-Test") });
        var labelSpan = groupRow.Locator(".sz-tree-item-label");

        await labelSpan.ClickAsync();
        await Microsoft.Playwright.Assertions.Expect(groupRow.Locator(".sz-icon-chevron-down")).ToBeVisibleAsync();

        await labelSpan.ClickAsync();
        await Microsoft.Playwright.Assertions.Expect(groupRow.Locator(".sz-icon-chevron-right")).ToBeVisibleAsync();
    }

    /// <summary>Ein Klick auf den Chevron-Button des Endpunktordners klappt den Ordner auf; ein zweiter Klick klappt ihn wieder zu.</summary>
    [Fact]
    public async Task ClickEndpointGroupChevron_TogglesCollapse()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await ExpandSystemGroupAsync();

        var contextMenuToggle = SystemAppRow.Locator("[data-testid=\"context-menu-toggle\"]");
        await contextMenuToggle.ClickAsync();
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Ordner anlegen" }).ClickAsync();
        var nameInput = Page.GetByLabel("Name");
        await nameInput.FillAsync("Chevron-Ordner-Test");
        await nameInput.PressAsync("Tab");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Anlegen" }).ClickAsync();

        await ExpandSystemApplicationAsync();

        var groupRow = Page.Locator(".sz-tree-row", new() { Has = Page.GetByText("Chevron-Ordner-Test") });
        var chevronBtn = groupRow.Locator(".sz-tree-chevron-btn");

        await chevronBtn.ClickAsync();
        await Microsoft.Playwright.Assertions.Expect(groupRow.Locator(".sz-icon-chevron-down")).ToBeVisibleAsync();

        await chevronBtn.ClickAsync();
        await Microsoft.Playwright.Assertions.Expect(groupRow.Locator(".sz-icon-chevron-right")).ToBeVisibleAsync();
    }

    /// <summary>Endpunktordner sind beim Laden des Baums initial zugeklappt.</summary>
    [Flaky]
    [Fact]
    public async Task EndpointGroupInitiallyCollapsed()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await ExpandSystemGroupAsync();

        var contextMenuToggle = SystemAppRow.Locator("[data-testid=\"context-menu-toggle\"]");
        await contextMenuToggle.ClickAsync();
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Ordner anlegen" }).ClickAsync();
        var nameInput = Page.GetByLabel("Name");
        await nameInput.FillAsync("Initial-Collapsed-Ordner");
        await nameInput.PressAsync("Tab");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Anlegen" }).ClickAsync();

        await contextMenuToggle.ClickAsync();
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Endpunkt anlegen" }).ClickAsync();
        await Page.GetByPlaceholder("Relativer Pfad").FillAsync("/api/test");
        await Microsoft.Playwright.Assertions.Expect(Page.Locator(".sz-endpoint-dirty-badge")).ToBeVisibleAsync();
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        // Speicher-Abschluss abwarten: Das Verschwinden des Dirty-Badges markiert das
        // Ende von OnSaveSuccessAsync (REST-Write abgeschlossen, beforeunload-Guard
        // aufgehoben); erst dann kann der Reload nicht mehr blockiert werden.
        await Microsoft.Playwright.Assertions.Expect(Page.Locator(".sz-endpoint-dirty-badge")).Not.ToBeVisibleAsync(new() { Timeout = 15_000 });
        await Microsoft.Playwright.Assertions.Expect(Page.Locator(".sz-alert-danger")).Not.ToBeVisibleAsync();
        await Microsoft.Playwright.Assertions.Expect(Page.Locator(".sz-error")).Not.ToBeVisibleAsync();

        await Page.ReloadAsync();
        await ExpandSystemGroupAsync();
        await ExpandSystemApplicationAsync();

        var groupRow = Page.Locator(".sz-tree-row", new() { Has = Page.GetByText("Initial-Collapsed-Ordner") });
        await Microsoft.Playwright.Assertions.Expect(groupRow.Locator(".sz-icon-chevron-right")).ToBeVisibleAsync(new() { Timeout = 15_000 });
    }
}
