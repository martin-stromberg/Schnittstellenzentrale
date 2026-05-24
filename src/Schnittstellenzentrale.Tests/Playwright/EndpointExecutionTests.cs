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

    /// <summary>
    /// E2E: Umgebung mit Variable anlegen, aktivieren, Endpunkt mit {{pfad}}-Platzhalter ausführen,
    /// aufgelöste URL im Pfadfeld und Antwortanzeige prüfen.
    /// </summary>
    [Fact]
    public async Task UmgebungMitVariable_Aktivieren_EndpunktSendetAufgeloestUrl()
    {
        await Page.GotoAsync(BaseUrl);

        // Umgebung mit Variable anlegen
        await Page.GetByTitle("Umgebungen verwalten").ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Neu" }).ClickAsync();
        await Page.Locator(".environment-editor input[type='text']").First.FillAsync("VariablenTestUmgebung");
        await Page.GetByRole(AriaRole.Button, new() { Name = "+ Variable hinzufügen" }).ClickAsync();
        var variableRows = Page.Locator(".environment-editor table tbody tr");
        await variableRows.First.Locator("input").First.FillAsync("pfad");
        await variableRows.First.Locator("input").Nth(1).FillAsync("api/application-groups");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();
        await Page.Locator(".btn-close").ClickAsync();

        // Umgebung aktivieren
        var envSelector = Page.Locator(".top-row select").Last;
        await envSelector.SelectOptionAsync(new SelectOptionValue { Label = "VariablenTestUmgebung" });

        // Endpunkt mit {{pfad}}-Platzhalter anlegen
        var systemAppRow = Page.Locator(".sz-tree-row", new() { Has = Page.GetByText("Schnittstellenzentrale") }).First;
        await systemAppRow.Locator("[data-testid=\"context-menu-toggle\"]").ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Endpunkt anlegen" }).ClickAsync();

        var pathInput = Page.GetByPlaceholder("Relativer Pfad");
        await pathInput.FillAsync("/{{pfad}}");
        await pathInput.BlurAsync();

        // Pfadfeld zeigt die aufgelöste URL an
        await Assertions.Expect(pathInput).ToHaveValueAsync("/api/application-groups");

        // Anfrage senden und Antwort prüfen
        await Page.GetByRole(AriaRole.Button, new() { Name = "Anfrage senden" }).ClickAsync();

        var statusText = Page.Locator(".response-section strong").First;
        await Assertions.Expect(statusText).ToBeVisibleAsync();
        var statusCode = await statusText.TextContentAsync();
        Assert.True(int.TryParse(statusCode?.Trim(), out var code) && code >= 200 && code <= 299,
            $"Erwarteter 2xx-Statuscode, erhalten: '{statusCode}'");
    }

    /// <summary>
    /// Eingabe eines Pfads mit Platzhalter und Query-String: Pfad wird bereinigt, Parameter werden korrekt
    /// als löschbare bzw. nicht löschbare Einträge angezeigt, und die gesendete URL enthält den aufgelösten Pfad mit Query-String.
    /// </summary>
    [Fact]
    public async Task EndpunktMitPlatzhalterUndQueryString_ZeigtKorrekteEintraegeUndSendetAufgeloestUrl()
    {
        await Page.GotoAsync(BaseUrl);

        var systemAppRow = Page.Locator(".sz-tree-row", new() { Has = Page.GetByText("Schnittstellenzentrale") }).First;
        var contextMenuToggle = systemAppRow.Locator("[data-testid=\"context-menu-toggle\"]");
        await contextMenuToggle.ClickAsync();

        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Endpunkt anlegen" }).ClickAsync();

        // Pfad mit Platzhalter und Query-String eingeben und Feld verlassen (onblur)
        var pathInput = Page.GetByPlaceholder("Relativer Pfad");
        await pathInput.FillAsync("/api/{id}?filter=active");
        await pathInput.BlurAsync();

        // Der Pfad-Anteil sollte bereinigt worden sein — ResolveDisplayUrl hängt den Query-String
        // rekonstruiert wieder an, daher ist ? im Anzeigewert vorhanden, aber der Pfad-Teil selbst endet vor dem ?
        var displayedPath = await pathInput.InputValueAsync();
        var pathPartOnly = displayedPath.Contains('?') ? displayedPath[..displayedPath.IndexOf('?')] : displayedPath;
        Assert.DoesNotContain("filter=active", pathPartOnly);
        Assert.DoesNotContain("page=", pathPartOnly);

        // Query-Parameter-Tab öffnen und Einträge prüfen
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Query-Parameter" }).ClickAsync();

        var queryParamRows = Page.Locator(".request-query-params-panel tbody tr");
        await Assertions.Expect(queryParamRows).ToHaveCountAsync(2);

        // Pfad-Platzhalter-Eintrag (id) erscheint zuerst (OrderByDescending IsPathParameter); kein Löschen-Button
        var idRow = queryParamRows.Nth(0);
        await Assertions.Expect(idRow.Locator("input").First).ToHaveValueAsync("id");
        await Assertions.Expect(idRow.Locator(".btn-outline-danger")).ToHaveCountAsync(0);

        // Regulärer Query-Parameter (filter) erscheint zweite; Löschen-Button vorhanden
        var filterRow = queryParamRows.Nth(1);
        await Assertions.Expect(filterRow.Locator("input").First).ToHaveValueAsync("filter");
        await Assertions.Expect(filterRow.Locator(".btn-outline-danger")).ToHaveCountAsync(1);

        // Wert für den Platzhalter eingeben
        var idValueInput = idRow.Locator("input").Nth(1);
        await idValueInput.FillAsync("42");
        await idValueInput.BlurAsync();

        // Pfadfeld zeigt die aufgelöste URL an
        await Assertions.Expect(pathInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex("42"));
        await Assertions.Expect(pathInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex("filter=active"));

        // Anfrage senden — der Response-Bereich muss erscheinen (belegt, dass die Anfrage abgeschickt wurde)
        await Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Anfrage senden" }).ClickAsync();

        var statusSpan = Page.Locator(".response-section").GetByText("Status:");
        await Assertions.Expect(statusSpan).ToBeVisibleAsync();
    }
}
