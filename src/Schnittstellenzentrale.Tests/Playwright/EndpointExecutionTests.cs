using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für die Ausführung von Endpunkten über die EndpointPage.</summary>
[Collection("Playwright")]
public class EndpointExecutionTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit der gemeinsamen Playwright-Factory.</summary>
    public EndpointExecutionTests(PlaywrightTestFactory factory) : base(factory) { }

    /// <summary>Eine HTTP-2xx-Response erscheint im Response-Bereich nach dem Ausführen eines Endpunkts.</summary>
    [Fact]
    public async Task ExecuteEndpoint_ReturnsSuccessResponse()
    {
        await Page.GotoAsync(BaseUrl);

        var systemAppRow = Page.Locator(".sz-tree-row", new() { Has = Page.GetByText("Schnittstellenzentrale") }).First;
        var contextMenuToggle = systemAppRow.Locator("[data-testid=\"context-menu-toggle\"]");
        await contextMenuToggle.ClickAsync();

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Endpunkt anlegen" }).ClickAsync();

        await Page.GetByPlaceholder("Relativer Pfad").FillAsync("/api/application-groups");

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Anfrage senden" }).ClickAsync();

        var statusSpan = Page.Locator(".response-section").GetByText("Status:");
        await Assertions.Expect(statusSpan).ToBeVisibleAsync();

        var statusText = await Page.Locator(".response-section strong").First.TextContentAsync();
        Assert.True(int.TryParse(statusText?.Trim(), out var statusCode),
            $"Statuscode konnte nicht geparst werden: '{statusText}'");
        Assert.InRange(statusCode, 200, 299);
    }
}
