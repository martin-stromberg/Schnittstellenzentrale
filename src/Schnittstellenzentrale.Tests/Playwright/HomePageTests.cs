using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Infrastructure.Testing;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für die Startseite: Systemgruppe und Systemendpunkte im Baum.</summary>
[Collection("Playwright")]
[OsInterface]
public class HomePageTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit der gemeinsamen Playwright-Factory.</summary>
    /// <param name="server">Die gemeinsame Server-Fixture der Test-Collection.</param>
    /// <param name="output">Testausgabe-Hilfsinstanz für Diagnose- und Dispose-Logs.</param>
    public HomePageTests(PlaywrightServer server, ITestOutputHelper output) : base(server, output) { }

    /// <summary>Systemgruppe „Schnittstellenzentrale" ist im ApplicationGroupTree sichtbar.</summary>
    [Fact]
    public async Task StartPage_ShowsSystemGroup()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var systemGroup = Page.GetByText("Schnittstellenzentrale").First;
        await Assertions.Expect(systemGroup).ToBeVisibleAsync();
    }

    /// <summary>Endpunkte der Systemanwendung erscheinen nach Aufklappen im Baum.</summary>
    [Fact]
    public async Task StartPage_ShowsOwnApiEndpoints()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var chevronButton = Page.Locator(".sz-tree-chevron-btn").First;
        await chevronButton.ClickAsync();

        var endpointSection = Page.Locator(".sz-tree-children").First;
        await Assertions.Expect(endpointSection).ToBeVisibleAsync();
    }
}
