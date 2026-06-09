using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>
/// E2E-Tests, die sicherstellen, dass die eigene OData-API als Anwendung registriert werden kann,
/// OData-Endpunkte ausgeführt werden können und OData-Filterausdrücke korrekt verarbeitet werden.
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

    /// <summary>
    /// GET Endpoints mit OData-Filter: nur Endpunkte, deren Name „Endpunkt" enthält,
    /// werden zurückgegeben; andere Endpunkte erscheinen nicht im Ergebnis.
    /// </summary>
    [Fact]
    public async Task OdataApi_GetEndpoints_MitContainsFilter_GibtNurPassendeEndpunkteZurueck()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.Locator(".sz-topbar-tab", new() { HasText = "Workspaces" }).ClickAsync();

        // ── SZ-OData-E2E registrieren und OData-Metadaten importieren ────────────
        await Page.GetByRole(AriaRole.Button, new() { Name = "+ Neue Anwendung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("SZ-OData-E2E");
        await Page.GetByLabel("Basis-URL").FillAsync($"{BaseUrl}/odatav4");
        await Page.GetByLabel("Schnittstellen-URL").FillAsync($"{BaseUrl}/odatav4/$metadata");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();
        await Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "SZ-OData-E2E" })).ToBeVisibleAsync();

        await Page.Locator(".sz-tree-item-btn", new() { HasText = "SZ-OData-E2E" }).First.ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "OData-Import" }).ClickAsync();
        await Assertions.Expect(Page.GetByText("OData-Import-Vorschau")).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Übernehmen" }).ClickAsync();

        // ── Testdaten: Endpunkte mit und ohne "Endpunkt" im Namen anlegen ────────
        await Page.GetByRole(AriaRole.Button, new() { Name = "+ Neue Anwendung" }).ClickAsync();
        await Page.GetByLabel("Name").FillAsync("Filter-Test-App");
        await Page.GetByLabel("Basis-URL").FillAsync(BaseUrl);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();
        await Assertions.Expect(Page.Locator(".sz-tree-item-btn", new() { HasText = "Filter-Test-App" })).ToBeVisibleAsync();

        var filterAppRow = Page.Locator(".sz-tree-row")
            .Filter(new() { Has = Page.Locator(".sz-tree-item-btn", new() { HasText = "Filter-Test-App" }) });

        await filterAppRow.Locator("[data-testid=\"context-menu-toggle\"]").ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Endpunkt anlegen" }).ClickAsync();
        await Page.Locator(".sz-endpoint-name-input").FillAsync("Test-Endpunkt-Alpha");
        await Page.Locator(".sz-endpoint-name-row").GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await filterAppRow.Locator("[data-testid=\"context-menu-toggle\"]").ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Endpunkt anlegen" }).ClickAsync();
        await Page.Locator(".sz-endpoint-name-input").FillAsync("Test-Endpunkt-Beta");
        await Page.Locator(".sz-endpoint-name-row").GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        await filterAppRow.Locator("[data-testid=\"context-menu-toggle\"]").ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Endpunkt anlegen" }).ClickAsync();
        await Page.Locator(".sz-endpoint-name-input").FillAsync("Andere-Schnittstelle");
        await Page.Locator(".sz-endpoint-name-row").GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        // ── Authenticate ausführen, damit der Bearer-Token im Environment steht ──
        var appRow = Page.Locator(".sz-tree-row")
            .Filter(new() { Has = Page.Locator(".sz-tree-item-btn", new() { HasText = "SZ-OData-E2E" }) });
        var appChevron = appRow.Locator(".sz-tree-chevron-btn");
        await appChevron.ClickAsync();
        await appChevron.ClickAsync();

        await Assertions.Expect(
            Page.Locator(".sz-tree-item-btn", new() { HasText = "Authenticate" }).First).ToBeVisibleAsync();
        await Page.Locator(".sz-tree-item-btn", new() { HasText = "Authenticate" }).First.ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Anfrage senden" }).ClickAsync();
        await Assertions.Expect(Page.Locator(".response-section strong").First).ToBeVisibleAsync();

        // ── GET Endpoints mit $filter=contains(Name,'Endpunkt') ─────────────────
        await appChevron.ClickAsync();
        await appChevron.ClickAsync();

        var endpointsChevron = Page.Locator(".sz-tree-row")
            .Filter(new() { Has = Page.Locator(".sz-tree-item-label", new() { HasText = "Endpoints" }) })
            .Locator(".sz-tree-chevron-btn");
        await endpointsChevron.ClickAsync();

        await Page.Locator(".sz-tree-item-btn", new() { HasText = "GET Endpoints" }).First.ClickAsync();
        await Assertions.Expect(Page.Locator(".sz-endpoint-name-input")).ToHaveValueAsync("GET Endpoints");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Query-Parameter" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "+ Parameter" }).ClickAsync();

        var newParamRow = Page.Locator(".request-query-params-panel tbody tr").Last;
        await newParamRow.Locator("input").First.FillAsync("$filter");
        await newParamRow.Locator("input").First.BlurAsync();
        await newParamRow.Locator("input").Nth(1).FillAsync("contains(Name,'Endpunkt')");
        await newParamRow.Locator("input").Nth(1).BlurAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Anfrage senden" }).ClickAsync();

        var statusLocator = Page.Locator(".response-section strong").First;
        await Assertions.Expect(statusLocator).ToBeVisibleAsync();
        var filterStatus = await statusLocator.TextContentAsync();
        Assert.True(
            int.TryParse(filterStatus?.Trim(), out var filterCode) && filterCode is >= 200 and <= 299,
            $"GET Endpoints mit Filter: Erwarteter 2xx-Statuscode, erhalten: '{filterStatus}'");

        var responseBody = Page.Locator(".sz-response-body-pre");
        await Assertions.Expect(responseBody).ToContainTextAsync("Test-Endpunkt-Alpha");
        await Assertions.Expect(responseBody).ToContainTextAsync("Test-Endpunkt-Beta");
        await Assertions.Expect(responseBody).Not.ToContainTextAsync("Andere-Schnittstelle");
    }
}
