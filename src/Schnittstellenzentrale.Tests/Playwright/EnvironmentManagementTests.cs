using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für die Verwaltung von Systemumgebungen und Variablen.</summary>
[Collection("Playwright")]
public class EnvironmentManagementTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit der gemeinsamen Playwright-Factory.</summary>
    public EnvironmentManagementTests(PlaywrightServer server) : base(server) { }

    /// <summary>
    /// E2E: Maskierter Variablenwert erscheint nicht im Klartext im DOM;
    /// nach Klick auf Auge-Icon wird Wert sichtbar.
    /// </summary>
    [Fact]
    public async Task MaskierterWert_IstNichtImKlartextImDomSichtbar()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Environments-Tab klicken, um Umgebungsverwaltung zu öffnen
        await Page.Locator(".sz-topbar-tab", new() { HasText = "Environments" }).ClickAsync();

        // Neue Umgebung anlegen
        await Page.GetByRole(AriaRole.Button, new() { Name = "+ Neue Umgebung" }).ClickAsync();
        await Page.Locator(".sz-env-create-form .sz-input").FillAsync("MaskierungsTest");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Anlegen" }).ClickAsync();

        // Neu angelegte Umgebung selektieren
        await Page.GetByRole(AriaRole.Button, new() { Name = "MaskierungsTest" }).ClickAsync();

        // Variable hinzufügen
        await Page.GetByRole(AriaRole.Button, new() { Name = "+ Variable hinzufügen" }).ClickAsync();
        var variableRows = Page.Locator(".environment-editor table tbody tr");
        await variableRows.First.Locator("input").First.FillAsync("geheimnis");
        await variableRows.First.Locator("input").Nth(1).FillAsync("SuperGeheim123");

        // Maskierung aktivieren (Schloss-Icon klicken)
        await variableRows.First.Locator("button[title]").ClickAsync();

        // Wert-Input sollte nun vom Typ "password" sein
        var valueInput = variableRows.First.Locator("input[type='password']");
        await Assertions.Expect(valueInput).ToBeVisibleAsync();

        // Wert sollte nicht im Klartext im DOM sichtbar sein
        var pageContent = await Page.ContentAsync();
        Assert.DoesNotContain("SuperGeheim123", pageContent);

        // Auge-Icon klicken, um Wert anzuzeigen
        await variableRows.First.Locator("button[title]").ClickAsync();

        var visibleInput = variableRows.First.Locator("input[type='text']").Last;
        await Assertions.Expect(visibleInput).ToBeVisibleAsync();
    }
}
