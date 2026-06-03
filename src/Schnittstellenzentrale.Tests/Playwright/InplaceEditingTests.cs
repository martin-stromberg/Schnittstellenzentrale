using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für In-place-Editing von Name und Untertitel.</summary>
[Collection("Playwright")]
public class InplaceEditingTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit der gemeinsamen Playwright-Factory.</summary>
    public InplaceEditingTests(PlaywrightServer server) : base(server) { }

    private async Task NavigateToCollectionContentAsync()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.Locator(".sz-topbar-tab", new() { HasText = "Workspaces" }).ClickAsync();
        await Page.Locator(".sz-tree-node-text").First.ClickAsync();
    }

    /// <summary>Name einer Sammlung wird inline editiert und gespeichert.</summary>
    [Fact]
    public async Task Playwright_InplaceEditing_Sammlung_Name_Speichern()
    {
        await NavigateToCollectionContentAsync();

        var titleSpan = Page.Locator(".sz-content-title").First;
        await titleSpan.ClickAsync();

        var nameInput = Page.Locator(".sz-inplace-input").First;
        await Assertions.Expect(nameInput).ToBeVisibleAsync();

        await nameInput.FillAsync("Umbenannte Sammlung");
        await nameInput.PressAsync("Enter");

        await Assertions.Expect(Page.Locator(".sz-content-title").First).ToContainTextAsync("Umbenannte Sammlung");
    }

    /// <summary>Leerer Name wird nicht akzeptiert.</summary>
    [Fact]
    public async Task Playwright_InplaceEditing_Sammlung_Name_PflichtfeldValidierung()
    {
        await NavigateToCollectionContentAsync();

        var titleSpan = Page.Locator(".sz-content-title").First;
        await titleSpan.ClickAsync();

        var nameInput = Page.Locator(".sz-inplace-input").First;
        await nameInput.FillAsync(string.Empty);
        await nameInput.PressAsync("Enter");

        var errorSpan = Page.Locator(".sz-inplace-error");
        await Assertions.Expect(errorSpan).ToBeVisibleAsync();
    }

    /// <summary>Untertitel einer Anwendung wird gespeichert.</summary>
    [Fact]
    public async Task Playwright_InplaceEditing_Anwendung_Subtitle_Speichern()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.Locator(".sz-topbar-tab", new() { HasText = "Workspaces" }).ClickAsync();

        await Page.Locator(".sz-tree-node-text").First.ClickAsync();

        var appBtn = Page.Locator(".sz-tree-item-btn").First;
        await appBtn.ClickAsync();

        var subtitleSpan = Page.Locator(".sz-content-subtitle").First;
        await subtitleSpan.ClickAsync();

        var subtitleInput = Page.Locator(".sz-inplace-subtitle").First;
        await Assertions.Expect(subtitleInput).ToBeVisibleAsync();

        await subtitleInput.FillAsync("Mein Untertitel");
        await subtitleInput.PressAsync("Enter");

        await Assertions.Expect(Page.Locator(".sz-content-subtitle").First).ToContainTextAsync("Mein Untertitel");
    }

    /// <summary>Escape beendet Bearbeitungsmodus ohne Änderung.</summary>
    [Fact]
    public async Task Playwright_InplaceEditing_Escape_BrichtAb()
    {
        await NavigateToCollectionContentAsync();

        var titleSpan = Page.Locator(".sz-content-title").First;
        var originalName = await titleSpan.InnerTextAsync();

        await titleSpan.ClickAsync();

        var nameInput = Page.Locator(".sz-inplace-input").First;
        await nameInput.FillAsync("Wird nicht gespeichert");
        await nameInput.PressAsync("Escape");

        await Assertions.Expect(Page.Locator(".sz-content-title").First).ToHaveTextAsync(originalName);
    }
}
