using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für den OData-Import-Dialog.</summary>
[Collection("Playwright")]
public class ODataImportTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit der gemeinsamen Playwright-Factory.</summary>
    public ODataImportTests(PlaywrightServer server) : base(server) { }

    /// <summary>Nach dem OData-Import sind die importierten Entity-Set-Endpunkte im Baum der Anwendung sichtbar.</summary>
    [Fact]
    public async Task ImportOData_RecognizesODataType_AndImportsEndpoints()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.Locator(".sz-topbar-tab", new() { HasText = "Workspaces" }).ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Neue Anwendung" }).ClickAsync();

        await Page.GetByLabel("Name").FillAsync("OData-Test-Anwendung");
        await Page.GetByLabel("Basis-URL").FillAsync($"{BaseUrl}/odatav4");
        await Page.GetByLabel("Schnittstellen-URL").FillAsync($"{BaseUrl}/odatav4/$metadata");

        await Assertions.Expect(Page.GetByText("OData")).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "OData-Test-Anwendung" })).ToBeVisibleAsync();

        await Page.Locator(".sz-tree-item-btn", new() { HasText = "OData-Test-Anwendung" }).First.ClickAsync();

        await Assertions.Expect(Page.GetByRole(AriaRole.Button, new() { Name = "OData-Import" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "OData-Import" }).ClickAsync();

        await Assertions.Expect(Page.GetByText("OData-Import-Vorschau")).ToBeVisibleAsync();

        await Assertions.Expect(Page.GetByText("GET Applications")).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Übernehmen" }).ClickAsync();

        var appChevron = Page.Locator(".sz-tree-row")
            .Filter(new() { Has = Page.Locator(".sz-tree-item-btn", new() { HasText = "OData-Test-Anwendung" }) })
            .Locator(".sz-tree-chevron-btn");
        await appChevron.ClickAsync();
        await appChevron.ClickAsync();

        await Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "GET Applications" })).ToBeVisibleAsync();
    }

    /// <summary>Eine CRUD-Operation über einen importierten OData-Endpunkt wird persistiert.</summary>
    [Fact]
    public async Task ImportOData_CrudOperation_PersistsChange()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.Locator(".sz-topbar-tab", new() { HasText = "Workspaces" }).ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Neue Anwendung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("OData-CRUD-Anwendung");
        await Page.GetByLabel("Basis-URL").FillAsync($"{BaseUrl}/odatav4");
        await Page.GetByLabel("Schnittstellen-URL").FillAsync($"{BaseUrl}/odatav4/$metadata");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "OData-CRUD-Anwendung" })).ToBeVisibleAsync();

        await Page.Locator(".sz-tree-item-btn", new() { HasText = "OData-CRUD-Anwendung" }).First.ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "OData-Import" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Übernehmen" }).ClickAsync();

        var appChevron = Page.Locator(".sz-tree-row")
            .Filter(new() { Has = Page.Locator(".sz-tree-item-btn", new() { HasText = "OData-CRUD-Anwendung" }) })
            .Locator(".sz-tree-chevron-btn");
        await appChevron.ClickAsync();

        await Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "GET Applications" })).ToBeVisibleAsync();

        await Page.Locator(".sz-tree-item-btn", new() { HasText = "GET Applications" }).First.ClickAsync();

        await Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "GET Applications" })).ToBeVisibleAsync();

        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.Locator(".sz-topbar-tab", new() { HasText = "Workspaces" }).ClickAsync();

        var restoredChevron = Page.Locator(".sz-tree-row")
            .Filter(new() { Has = Page.Locator(".sz-tree-item-btn", new() { HasText = "OData-CRUD-Anwendung" }) })
            .Locator(".sz-tree-chevron-btn");
        await restoredChevron.ClickAsync();

        await Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "GET Applications" })).ToBeVisibleAsync();
    }
}
