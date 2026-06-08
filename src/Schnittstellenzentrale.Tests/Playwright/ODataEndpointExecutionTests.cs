using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>
/// E2E-Tests, die sicherstellen, dass die eigene OData-API als Anwendung registriert werden kann,
/// alle drei Endpunkte (Authenticate, GET ApplicationGroups, GET Applications) ausgeführt werden können
/// und jeweils ein JSON-Ergebnis liefern.
/// </summary>
[Collection("PlaywrightODataEndpoint")]
public class ODataEndpointExecutionTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit dem OData-Endpunkt-Server.</summary>
    public ODataEndpointExecutionTests(PlaywrightODataEndpointServer server) : base(server) { }

    /// <summary>
    /// Vollständiger Ablauf: eigene OData-API registrieren, Metadaten importieren,
    /// Authenticate ausführen (liefert Token), GET ApplicationGroups und GET Applications
    /// ausführen (liefern jeweils OData-JSON mit "value"-Property).
    /// </summary>
    [Fact]
    public async Task OdataApi_Registrieren_Importieren_UndAlleEndpunkteErfolgreichAusfuehren()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.Locator(".sz-topbar-tab", new() { HasText = "Workspaces" }).ClickAsync();

        // Anwendung anlegen
        await Page.GetByRole(AriaRole.Button, new() { Name = "+ Neue Anwendung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("SZ-OData-E2E");
        await Page.GetByLabel("Basis-URL").FillAsync($"{BaseUrl}/odatav4");
        await Page.GetByLabel("Schnittstellen-URL").FillAsync($"{BaseUrl}/odatav4/$metadata");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();
        await Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "SZ-OData-E2E" })).ToBeVisibleAsync();

        // OData-Metadaten importieren
        await Page.Locator(".sz-tree-item-btn", new() { HasText = "SZ-OData-E2E" }).First.ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "OData-Import" }).ClickAsync();
        await Assertions.Expect(Page.GetByText("OData-Import-Vorschau")).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Übernehmen" }).ClickAsync();

        // Baum aufklappen
        var appRow = Page.Locator(".sz-tree-row")
            .Filter(new() { Has = Page.Locator(".sz-tree-item-btn", new() { HasText = "SZ-OData-E2E" }) });
        var appChevron = appRow.Locator(".sz-tree-chevron-btn");
        await appChevron.ClickAsync();
        await appChevron.ClickAsync();

        // ── Authenticate ──────────────────────────────────────────────────────────
        await Assertions.Expect(
            Page.Locator(".sz-tree-item-btn", new() { HasText = "Authenticate" }).First).ToBeVisibleAsync();
        await Page.Locator(".sz-tree-item-btn", new() { HasText = "Authenticate" }).First.ClickAsync();
        await Assertions.Expect(Page.Locator(".sz-endpoint-name-input")).ToHaveValueAsync("Authenticate");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Anfrage senden" }).ClickAsync();

        var statusLocator = Page.Locator(".response-section strong").First;
        await Assertions.Expect(statusLocator).ToBeVisibleAsync();
        var authenticateStatus = await statusLocator.TextContentAsync();
        Assert.True(
            int.TryParse(authenticateStatus?.Trim(), out var authCode) && authCode is >= 200 and <= 299,
            $"Authenticate: Erwarteter 2xx-Statuscode, erhalten: '{authenticateStatus}'");
        await Assertions.Expect(Page.Locator(".sz-response-body-pre")).ToContainTextAsync("token");

        // ── GET ApplicationGroups ─────────────────────────────────────────────────
        await appChevron.ClickAsync();
        await appChevron.ClickAsync();

        var applicationGroupsChevron = Page.Locator(".sz-tree-row")
            .Filter(new() { Has = Page.Locator(".sz-tree-item-label", new() { HasText = "ApplicationGroups" }) })
            .Locator(".sz-tree-chevron-btn");
        await applicationGroupsChevron.ClickAsync();

        await Page.Locator(".sz-tree-item-btn", new() { HasText = "GET ApplicationGroups" }).First.ClickAsync();
        await Assertions.Expect(Page.Locator(".sz-endpoint-name-input")).ToHaveValueAsync("GET ApplicationGroups");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Anfrage senden" }).ClickAsync();

        await Assertions.Expect(statusLocator).ToBeVisibleAsync();
        var groupsStatus = await statusLocator.TextContentAsync();
        Assert.True(
            int.TryParse(groupsStatus?.Trim(), out var groupsCode) && groupsCode is >= 200 and <= 299,
            $"GET ApplicationGroups: Erwarteter 2xx-Statuscode, erhalten: '{groupsStatus}'");
        await Assertions.Expect(Page.Locator(".sz-response-body-pre")).ToContainTextAsync("isSystem");

        // ── GET Applications ──────────────────────────────────────────────────────
        await appChevron.ClickAsync();
        await appChevron.ClickAsync();

        var applicationsChevron = Page.Locator(".sz-tree-row")
            .Filter(new() { Has = Page.Locator(".sz-tree-item-label", new() { HasText = "Applications" }) })
            .Locator(".sz-tree-chevron-btn");
        await applicationsChevron.ClickAsync();

        await Page.Locator(".sz-tree-item-btn", new() { HasText = "GET Applications" }).First.ClickAsync();
        await Assertions.Expect(Page.Locator(".sz-endpoint-name-input")).ToHaveValueAsync("GET Applications");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Anfrage senden" }).ClickAsync();

        await Assertions.Expect(statusLocator).ToBeVisibleAsync();
        var appsStatus = await statusLocator.TextContentAsync();
        Assert.True(
            int.TryParse(appsStatus?.Trim(), out var appsCode) && appsCode is >= 200 and <= 299,
            $"GET Applications: Erwarteter 2xx-Statuscode, erhalten: '{appsStatus}'");
        await Assertions.Expect(Page.Locator(".sz-response-body-pre")).ToContainTextAsync("isSystem");
    }
}
