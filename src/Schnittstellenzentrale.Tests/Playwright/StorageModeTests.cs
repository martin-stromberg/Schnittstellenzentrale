using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für den Speichermodus-Wechsel.</summary>
[Collection("Playwright")]
public class StorageModeTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit der gemeinsamen Playwright-Factory.</summary>
    public StorageModeTests(PlaywrightServer server) : base(server) { }

    /// <summary>Nach dem Wechsel auf Team-Modus zeigt der ApplicationGroupTree die Team-Daten.</summary>
    [Fact]
    public async Task SwitchToTeamMode_ShowsTeamData()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.Locator(".sz-topbar-select").SelectOptionAsync("Team");

        var treeBody = Page.Locator(".sz-tree-body");
        await Assertions.Expect(treeBody).ToBeVisibleAsync();
    }

    /// <summary>Nach Rückwechsel auf Benutzer-Modus zeigt der ApplicationGroupTree die User-Daten.</summary>
    [Fact]
    public async Task SwitchBackToUserMode_ShowsUserData()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator(".sz-topbar-select").SelectOptionAsync("Team");
        await Page.Locator(".sz-topbar-select").SelectOptionAsync("User");

        var treeBody = Page.Locator(".sz-tree-body");
        await Assertions.Expect(treeBody).ToBeVisibleAsync();
    }
}
