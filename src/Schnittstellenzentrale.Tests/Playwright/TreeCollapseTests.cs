using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für das Auf- und Zuklappen von Baumknoten über Titel und Chevron.</summary>
[Collection("Playwright")]
public class TreeCollapseTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit der gemeinsamen Playwright-Factory.</summary>
    public TreeCollapseTests(PlaywrightTestFactory factory) : base(factory) { }

    /// <summary>Ein Klick auf den ApplicationGroup-Titeltext klappt den Knoten zu; ein zweiter Klick klappt ihn wieder auf.</summary>
    [Fact]
    public async Task ClickApplicationGroupTitle_TogglesCollapse()
    {
        await Page.GotoAsync(BaseUrl);

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Neue Gruppe" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("Collapse-Test-Gruppe");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Neue Anwendung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("Anwendung-In-Gruppe");
        await Page.GetByLabel("Basis-URL").FillAsync("http://test.example.com");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        var groupRow = Page.Locator(".sz-tree-row", new() { Has = Page.GetByText("Collapse-Test-Gruppe") });
        var titleSpan = groupRow.Locator(".sz-tree-node-text");

        await titleSpan.ClickAsync();

        await Microsoft.Playwright.Assertions.Expect(Page.GetByText("Anwendung-In-Gruppe")).Not.ToBeVisibleAsync();

        await titleSpan.ClickAsync();

        await Microsoft.Playwright.Assertions.Expect(Page.GetByText("Anwendung-In-Gruppe")).ToBeVisibleAsync();
    }

    /// <summary>Ein Klick auf den Anwendungsnamen klappt die Anwendung auf und zeigt untergeordnete Elemente an.</summary>
    [Fact]
    public async Task ClickApplicationName_ExpandsApplication()
    {
        await Page.GotoAsync(BaseUrl);

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Neue Anwendung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("Expand-Test-Anwendung");
        await Page.GetByLabel("Basis-URL").FillAsync("http://test.example.com");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Microsoft.Playwright.Assertions.Expect(Page.GetByText("Expand-Test-Anwendung")).ToBeVisibleAsync();

        var appRow = Page.Locator(".sz-tree-row", new() { Has = Page.GetByText("Expand-Test-Anwendung") });
        var appBtn = appRow.Locator(".sz-tree-item-btn");

        await appBtn.ClickAsync();

        await Microsoft.Playwright.Assertions.Expect(appRow.Locator(".sz-tree-chevron-btn .bi-chevron-down")).ToBeVisibleAsync();
    }

    /// <summary>Ein Klick auf den Anwendungsnamen selektiert die Anwendung.</summary>
    [Fact]
    public async Task ClickApplicationName_SelectsApplication()
    {
        await Page.GotoAsync(BaseUrl);

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Neue Anwendung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("Select-Test-Anwendung");
        await Page.GetByLabel("Basis-URL").FillAsync("http://test.example.com");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Microsoft.Playwright.Assertions.Expect(Page.GetByText("Select-Test-Anwendung")).ToBeVisibleAsync();

        var appRow = Page.Locator(".sz-tree-row", new() { Has = Page.GetByText("Select-Test-Anwendung") });
        await appRow.Locator(".sz-tree-item-btn").ClickAsync();

        await Microsoft.Playwright.Assertions.Expect(Page.GetByRole(Microsoft.Playwright.AriaRole.Heading, new() { Name = "Select-Test-Anwendung" })).ToBeVisibleAsync();
    }

    /// <summary>Ein Klick auf einen bereits aufgeklappten Anwendungsnamen klappt die Anwendung wieder zu.</summary>
    [Fact]
    public async Task ClickApplicationName_CollapsesExpandedApplication()
    {
        await Page.GotoAsync(BaseUrl);

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Neue Anwendung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("Collapse-App-Test");
        await Page.GetByLabel("Basis-URL").FillAsync("http://test.example.com");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Microsoft.Playwright.Assertions.Expect(Page.GetByText("Collapse-App-Test")).ToBeVisibleAsync();

        var appRow = Page.Locator(".sz-tree-row", new() { Has = Page.GetByText("Collapse-App-Test") });
        var appBtn = appRow.Locator(".sz-tree-item-btn");

        await appBtn.ClickAsync();
        await Microsoft.Playwright.Assertions.Expect(appRow.Locator(".sz-tree-chevron-btn .bi-chevron-down")).ToBeVisibleAsync();

        await appBtn.ClickAsync();
        await Microsoft.Playwright.Assertions.Expect(appRow.Locator(".sz-tree-chevron-btn .bi-chevron-right")).ToBeVisibleAsync();
    }

    /// <summary>Ein Klick auf den Endpunktordner-Namen klappt den Ordner auf; ein zweiter Klick klappt ihn wieder zu.</summary>
    [Fact]
    public async Task ClickEndpointGroupName_TogglesCollapse()
    {
        await Page.GotoAsync(BaseUrl);

        var systemAppRow = Page.Locator(".sz-tree-row", new() { Has = Page.GetByText("Schnittstellenzentrale") }).First;
        var contextMenuToggle = systemAppRow.Locator("[data-testid=\"context-menu-toggle\"]");
        await contextMenuToggle.ClickAsync();
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Endpunktordner anlegen" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("Collapse-Ordner-Test");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        var appBtn = systemAppRow.Locator(".sz-tree-item-btn");
        await appBtn.ClickAsync();

        var groupRow = Page.Locator(".sz-tree-row", new() { Has = Page.GetByText("Collapse-Ordner-Test") });
        var labelSpan = groupRow.Locator(".sz-tree-item-label");

        await labelSpan.ClickAsync();
        await Microsoft.Playwright.Assertions.Expect(groupRow.Locator(".bi-chevron-down")).ToBeVisibleAsync();

        await labelSpan.ClickAsync();
        await Microsoft.Playwright.Assertions.Expect(groupRow.Locator(".bi-chevron-right")).ToBeVisibleAsync();
    }

    /// <summary>Ein Klick auf den Chevron-Button des Endpunktordners klappt den Ordner auf; ein zweiter Klick klappt ihn wieder zu.</summary>
    [Fact]
    public async Task ClickEndpointGroupChevron_TogglesCollapse()
    {
        await Page.GotoAsync(BaseUrl);

        var systemAppRow = Page.Locator(".sz-tree-row", new() { Has = Page.GetByText("Schnittstellenzentrale") }).First;
        var contextMenuToggle = systemAppRow.Locator("[data-testid=\"context-menu-toggle\"]");
        await contextMenuToggle.ClickAsync();
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Endpunktordner anlegen" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("Chevron-Ordner-Test");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        var appBtn = systemAppRow.Locator(".sz-tree-item-btn");
        await appBtn.ClickAsync();

        var groupRow = Page.Locator(".sz-tree-row", new() { Has = Page.GetByText("Chevron-Ordner-Test") });
        var chevronBtn = groupRow.Locator(".sz-tree-chevron-btn");

        await chevronBtn.ClickAsync();
        await Microsoft.Playwright.Assertions.Expect(groupRow.Locator(".bi-chevron-down")).ToBeVisibleAsync();

        await chevronBtn.ClickAsync();
        await Microsoft.Playwright.Assertions.Expect(groupRow.Locator(".bi-chevron-right")).ToBeVisibleAsync();
    }

    /// <summary>Endpunktordner sind beim Laden des Baums initial zugeklappt.</summary>
    [Fact]
    public async Task EndpointGroupInitiallyCollapsed()
    {
        await Page.GotoAsync(BaseUrl);

        var systemAppRow = Page.Locator(".sz-tree-row", new() { Has = Page.GetByText("Schnittstellenzentrale") }).First;
        var contextMenuToggle = systemAppRow.Locator("[data-testid=\"context-menu-toggle\"]");
        await contextMenuToggle.ClickAsync();
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Endpunktordner anlegen" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("Initial-Collapsed-Ordner");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await contextMenuToggle.ClickAsync();
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Endpunkt anlegen" }).ClickAsync();
        await Page.GetByPlaceholder("Relativer Pfad").FillAsync("/api/test");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Page.ReloadAsync();

        var appBtn = systemAppRow.Locator(".sz-tree-item-btn");
        await appBtn.ClickAsync();

        var groupRow = Page.Locator(".sz-tree-row", new() { Has = Page.GetByText("Initial-Collapsed-Ordner") });
        await Microsoft.Playwright.Assertions.Expect(groupRow.Locator(".bi-chevron-right")).ToBeVisibleAsync();
    }
}
