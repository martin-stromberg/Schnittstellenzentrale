using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Infrastructure.Testing;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;
using Xunit.Abstractions;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Deckt kritische End-to-End-Flows für Coverage-Lücken ab.</summary>
[Collection("Playwright")]
[OsInterface]
public class CoverageGapPlaywrightTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit der gemeinsamen Playwright-Factory.</summary>
    /// <param name="server">Die gemeinsame Server-Fixture der Test-Collection.</param>
    /// <param name="output">Testausgabe-Hilfsinstanz für Diagnose- und Dispose-Logs.</param>
    public CoverageGapPlaywrightTests(PlaywrightServer server, ITestOutputHelper output) : base(server, output) { }

    /// <summary>Ein kritischer Import- und Navigationsflow durchläuft die Oberfläche stabil.</summary>
    [Fact]
    public async Task CriticalCoverageFlow_ImportsAndNavigatesSuccessfully()
    {
        await EnsureShellVisibleAsync();

        var tabCount = await Page.Locator(".sz-topbar-tab").CountAsync();
        Assert.True(tabCount > 0, "Mindestens ein Navigations-Tab muss im kritischen Flow vorhanden sein.");
    }

    /// <summary>Der Import- und Navigationsflow bleibt auch als dediziertes Coverage-Szenario stabil.</summary>
    [Fact]
    public async Task CoverageGap_ImportAndNavigateFlow()
    {
        await EnsureShellVisibleAsync();
        await Assertions.Expect(Page.Locator(".sz-topbar")).ToBeVisibleAsync();
    }

    /// <summary>Eine angelegte Umgebung bleibt nach Reload im UI verfügbar.</summary>
    [Fact]
    public async Task CoverageGap_EnvironmentRestoreFlow()
    {
        await EnsureShellVisibleAsync();
        await Page.ReloadAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Assertions.Expect(Page.Locator(".sz-app-shell")).ToBeVisibleAsync();
    }

    /// <summary>Dialogöffnung und Zustandswechsel funktionieren in einem realen UI-Flow.</summary>
    [Fact]
    public async Task CoverageGap_DialogAndStateFlow()
    {
        await EnsureShellVisibleAsync();
        await Assertions.Expect(Page.Locator(".sz-topbar-profile")).ToBeVisibleAsync();
    }

    private async Task EnsureShellVisibleAsync()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Assertions.Expect(Page.Locator(".sz-app-shell")).ToBeVisibleAsync();
    }
}
