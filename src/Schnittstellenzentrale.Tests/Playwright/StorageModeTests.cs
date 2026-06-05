using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für den Speichermodus-Wechsel und localStorage-Persistierung.</summary>
[Collection("Playwright")]
public class StorageModeTests : PlaywrightTestBase
{
    private readonly PlaywrightServer _server;

    /// <summary>Initialisiert den Test mit der gemeinsamen Playwright-Factory.</summary>
    public StorageModeTests(PlaywrightServer server) : base(server)
    {
        _server = server;
    }

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

    /// <summary>Der Speichermodus wird nach einem Seitenreload aus localStorage wiederhergestellt.</summary>
    [Fact]
    public async Task Modus_WirdNachReloadAusLocalStorageWiederhergestellt()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator(".sz-topbar-select").SelectOptionAsync("User");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.ReloadAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Assertions.Expect(Page.Locator(".sz-topbar-select")).ToHaveValueAsync("User");
    }

    /// <summary>Die aktive Umgebung wird nach einem Seitenreload aus localStorage wiederhergestellt.</summary>
    [Fact]
    public async Task AktiveUmgebung_WirdNachReloadAusLocalStorageWiederhergestellt()
    {
        using var scope = _server.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ISystemEnvironmentRepository>();
        var env = await repo.AddAsync(new SystemEnvironment
        {
            Name = "Playwright-Testumgebung",
            Mode = StorageMode.Team
        });

        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var envSelector = Page.Locator(".sz-topbar-actions .form-select");
        await envSelector.SelectOptionAsync(env.Id.ToString());
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.ReloadAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Assertions.Expect(envSelector).ToHaveValueAsync(env.Id.ToString());
    }

    /// <summary>Nach Reload werden Modus und Umgebung gemeinsam wiederhergestellt.</summary>
    [Fact]
    public async Task ModusUndUmgebung_WerdenNachReloadGemeinsamWiederhergestellt()
    {
        using var scope = _server.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ISystemEnvironmentRepository>();
        var env = await repo.AddAsync(new SystemEnvironment
        {
            Name = "Playwright-Benutzerumgebung",
            Mode = StorageMode.User,
            Owner = @"TEST\testuser"
        });

        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.Locator(".sz-topbar-select").SelectOptionAsync("User");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var envSelector = Page.Locator(".sz-topbar-actions .form-select");
        await envSelector.SelectOptionAsync(env.Id.ToString());
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.ReloadAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Assertions.Expect(Page.Locator(".sz-topbar-select")).ToHaveValueAsync("User");
        await Assertions.Expect(envSelector).ToHaveValueAsync(env.Id.ToString());
    }
}
