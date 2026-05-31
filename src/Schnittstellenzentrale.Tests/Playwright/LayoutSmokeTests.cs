using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>
/// Prüft, dass das App-Shell-Layout nach dem Laden tatsächlich Platz einnimmt.
/// Fängt fehlende CSS-Definitionen auf, die das Layout auf Höhe/Breite 0 kollabieren lassen.
/// </summary>
[Collection("Playwright")]
public class LayoutSmokeTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit dem gemeinsamen PlaywrightServer.</summary>
    public LayoutSmokeTests(PlaywrightServer server) : base(server) { }

    /// <summary>AppShell muss Breite und Höhe größer als 200px haben.</summary>
    [Fact]
    public async Task AppShell_NimmtVollbildEin()
    {
        await Page.GotoAsync(BaseUrl);

        var shell = Page.Locator(".sz-app-shell");
        await Assertions.Expect(shell).ToBeVisibleAsync();

        var box = await shell.BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.True(box.Width > 200, $"sz-app-shell ist zu schmal: {box.Width}px – CSS-Layout kollabiert.");
        Assert.True(box.Height > 200, $"sz-app-shell ist zu flach: {box.Height}px – CSS-Layout kollabiert.");
    }

    /// <summary>TopBar muss mindestens 30px hoch sein.</summary>
    [Fact]
    public async Task TopBar_IstSichtbarUndHatHoehe()
    {
        await Page.GotoAsync(BaseUrl);

        var topbar = Page.Locator(".sz-topbar");
        await Assertions.Expect(topbar).ToBeVisibleAsync();

        var box = await topbar.BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.True(box.Height >= 30, $"sz-topbar ist zu flach: {box.Height}px – CSS fehlt oder greift nicht.");
    }

    /// <summary>Workspaces-Sidebar und Content-Panel müssen sichtbare Fläche haben.</summary>
    [Fact]
    public async Task WorkspacesBereich_SidebarUndContentHabenFlaeche()
    {
        await Page.GotoAsync(BaseUrl);

        await Page.Locator(".sz-topbar-tab", new() { HasText = "Workspaces" }).ClickAsync();

        var sidebar = Page.Locator(".sz-workspaces-sidebar");
        await Assertions.Expect(sidebar).ToBeVisibleAsync();
        var sidebarBox = await sidebar.BoundingBoxAsync();
        Assert.NotNull(sidebarBox);
        Assert.True(sidebarBox.Height > 100, $"sz-workspaces-sidebar kollabiert: {sidebarBox.Height}px.");

        var content = Page.Locator(".sz-workspaces-content-panel");
        var contentBox = await content.BoundingBoxAsync();
        Assert.NotNull(contentBox);
        Assert.True(contentBox.Width > 100, $"sz-workspaces-content-panel kollabiert: {contentBox.Width}px.");
    }

    /// <summary>Environments-Sidebar muss sichtbare Fläche haben.</summary>
    [Fact]
    public async Task EnvironmentsBereich_SidebarHatFlaeche()
    {
        await Page.GotoAsync(BaseUrl);

        await Page.Locator(".sz-topbar-tab", new() { HasText = "Environments" }).ClickAsync();

        var sidebar = Page.Locator(".sz-environments-sidebar");
        await Assertions.Expect(sidebar).ToBeVisibleAsync();
        var box = await sidebar.BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.True(box.Height > 100, $"sz-environments-sidebar kollabiert: {box.Height}px.");
    }

    /// <summary>History-Content muss sichtbare Fläche haben.</summary>
    [Fact]
    public async Task HistoryBereich_ContentHatFlaeche()
    {
        await Page.GotoAsync(BaseUrl);

        await Page.Locator(".sz-topbar-tab", new() { HasText = "History" }).ClickAsync();

        var historyContent = Page.Locator(".sz-history-content");
        await Assertions.Expect(historyContent).ToBeVisibleAsync();
        var box = await historyContent.BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.True(box.Height > 100, $"sz-history-content kollabiert: {box.Height}px.");
    }
}
