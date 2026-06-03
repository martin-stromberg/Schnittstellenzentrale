using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für das Anlegen, Bearbeiten und Löschen von Anwendungen.</summary>
[Collection("Playwright")]
public class ApplicationCrudTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit der gemeinsamen Playwright-Factory.</summary>
    public ApplicationCrudTests(PlaywrightServer server) : base(server) { }

    /// <summary>Eine neu angelegte Anwendung erscheint im Baum.</summary>
    [Fact]
    public async Task CreateApplication_AppearsInTree()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.Locator(".sz-topbar-tab", new() { HasText = "Workspaces" }).ClickAsync();

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Neue Anwendung" }).ClickAsync();

        await Page.GetByLabel("Name").FillAsync("Testanwendung");
        await Page.GetByLabel("Basis-URL").FillAsync("http://test.example.com");

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "Testanwendung" })).ToBeVisibleAsync();
    }

    /// <summary>Der geänderte Name einer Anwendung erscheint im Baum.</summary>
    [Fact]
    public async Task EditApplication_UpdatesNameInTree()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.Locator(".sz-topbar-tab", new() { HasText = "Workspaces" }).ClickAsync();

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Neue Anwendung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("Umbenennung-Test");
        await Page.GetByLabel("Basis-URL").FillAsync("http://test.example.com");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "Umbenennung-Test" })).ToBeVisibleAsync();

        var appRow = Page.Locator(".sz-tree-row", new() { Has = Page.Locator(".sz-tree-item-btn", new() { HasText = "Umbenennung-Test" }) });
        var contextMenuToggle = appRow.Locator("[data-testid=\"context-menu-toggle\"]");
        await contextMenuToggle.ClickAsync();

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Bearbeiten" }).ClickAsync();

        var nameInput = Page.GetByLabel("Name");
        await nameInput.FillAsync("Umbenannt");

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "Umbenannt" })).ToBeVisibleAsync();
    }

    /// <summary>Eine gelöschte Anwendung ist nicht mehr im Baum vorhanden.</summary>
    [Fact]
    public async Task DeleteApplication_DisappearsFromTree()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.Locator(".sz-topbar-tab", new() { HasText = "Workspaces" }).ClickAsync();

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Neue Anwendung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("Zu-loeschende-Anwendung");
        await Page.GetByLabel("Basis-URL").FillAsync("http://test.example.com");
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "Zu-loeschende-Anwendung" })).ToBeVisibleAsync();

        var appRow = Page.Locator(".sz-tree-row", new() { Has = Page.Locator(".sz-tree-item-btn", new() { HasText = "Zu-loeschende-Anwendung" }) });
        var contextMenuToggle = appRow.Locator("[data-testid=\"context-menu-toggle\"]");
        await contextMenuToggle.ClickAsync();

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Löschen" }).First.ClickAsync();

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Löschen" }).ClickAsync();

        await Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "Zu-loeschende-Anwendung" })).Not.ToBeVisibleAsync();
    }
}
