using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für den Swagger-Import-Dialog.</summary>
[Collection("Playwright")]
public class SwaggerImportTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit der gemeinsamen Playwright-Factory.</summary>
    public SwaggerImportTests(PlaywrightServer server) : base(server) { }

    /// <summary>Nach dem Swagger-Import sind die importierten Endpunkte im Baum der Systemanwendung sichtbar.</summary>
    [Fact]
    public async Task ImportSwagger_ImportsEndpointsIntoTree()
    {
        await Page.GotoAsync(BaseUrl);

        var groupChevron = Page.Locator(".collapsible-section .sz-tree-chevron-btn").First;
        await groupChevron.ClickAsync();

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Schnittstellenzentrale" }).First.ClickAsync();

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Swagger-Import" }).ClickAsync();

        await Assertions.Expect(Page.GetByText("Swagger-Import-Vorschau")).ToBeVisibleAsync();

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Übernehmen" }).ClickAsync();

        var systemAppChevron = Page.Locator(".sz-tree-row")
            .Filter(new() { Has = Page.Locator(".sz-tree-item-btn", new() { HasText = "Schnittstellenzentrale" }) })
            .Locator(".sz-tree-chevron-btn");
        await systemAppChevron.ClickAsync();

        await Assertions.Expect(Page.Locator(".sz-tree-children").First).ToBeVisibleAsync();
    }
}
