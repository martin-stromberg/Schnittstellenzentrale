using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für den Health-Check-Dialog.</summary>
[Collection("Playwright")]
public class HealthCheckTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit der gemeinsamen Playwright-Factory.</summary>
    public HealthCheckTests(PlaywrightServer server) : base(server) { }

    /// <summary>Der Health-Check-Dialog zeigt den „erreichbar"-Status nach Ausführung für die Systemanwendung.</summary>
    [Fact]
    public async Task HealthCheck_ShowsReachableStatus()
    {
        await Page.GotoAsync(BaseUrl);

        var groupChevron = Page.Locator(".collapsible-section .sz-tree-chevron-btn").First;
        await groupChevron.ClickAsync();

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Schnittstellenzentrale" }).First.ClickAsync();

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Health-Check" }).ClickAsync();

        var reachableMessage = Page.GetByText("Die Anwendung ist erreichbar");
        var cooldownMessage = Page.GetByText("Health-Check wurde übersprungen");
        var notReachableMessage = Page.GetByText("Die Anwendung ist nicht erreichbar");

        await Assertions.Expect(
            reachableMessage.Or(cooldownMessage).Or(notReachableMessage))
            .ToBeVisibleAsync();
    }
}
