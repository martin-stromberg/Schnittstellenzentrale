using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für die Impressum-Seite und den Impressum-Link im Sidebar-Footer.</summary>
[Collection("Playwright")]
public class ImpressumPageTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit dem gemeinsamen PlaywrightServer (keine Impressum-Datei vorhanden).</summary>
    public ImpressumPageTests(PlaywrightServer server) : base(server) { }

    /// <summary>ImpressumSeite_ZeigtHinweis_WennDateiFehlt: /impressum zeigt lokalisierten Hinweistext, wenn Datei fehlt.</summary>
    [Fact]
    public async Task ImpressumSeite_ZeigtHinweis_WennDateiFehlt()
    {
        await Page.GotoAsync($"{BaseUrl}/impressum");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var page = Page.Locator(".sz-impressum-page");
        await Assertions.Expect(page).ToBeVisibleAsync();

        var hinweis = Page.GetByText("No imprint available.").Or(Page.GetByText("Kein Impressum verfügbar."));
        await Assertions.Expect(hinweis).ToBeVisibleAsync();
    }

    /// <summary>SidebarFooter_LinkFehlt_WennDateiNichtVorhanden: Impressum-Link im Sidebar-Footer fehlt, wenn Datei nicht vorhanden ist.</summary>
    [Fact]
    public async Task SidebarFooter_LinkFehlt_WennDateiNichtVorhanden()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var link = Page.Locator(".sz-sidebar-footer-link[href='/impressum']");
        await Assertions.Expect(link).ToHaveCountAsync(0);
    }
}

/// <summary>Playwright-Tests für die Impressum-Seite mit vorhandener Impressum-Datei.</summary>
[Collection("PlaywrightImpressum")]
public class ImpressumPageWithFileTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit dem PlaywrightImpressumServer (Impressum-Datei ist vorhanden).</summary>
    public ImpressumPageWithFileTests(PlaywrightImpressumServer server) : base(server) { }

    /// <summary>ImpressumSeite_ZeigtInhalt_WennDateiVorhanden: /impressum zeigt Überschrift und Inhalt, wenn Impressum-Datei vorhanden ist.</summary>
    [Fact]
    public async Task ImpressumSeite_ZeigtInhalt_WennDateiVorhanden()
    {
        await Page.GotoAsync($"{BaseUrl}/impressum");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var page = Page.Locator(".sz-impressum-page");
        await Assertions.Expect(page).ToBeVisibleAsync();

        // Heading aus Lokalisierung (EN-Fallback oder DE)
        var heading = Page.Locator(".sz-impressum-page h1");
        await Assertions.Expect(heading).ToBeVisibleAsync();

        // Gerenderter Markdown-Inhalt aus der Testdatei (eindeutig, nicht die Seitenüberschrift)
        var content = Page.Locator(".sz-impressum-page").GetByText("Dies ist ein Test-Impressum.");
        await Assertions.Expect(content).ToBeVisibleAsync();
    }

    /// <summary>SidebarFooter_LinkSichtbar_WennDateiVorhanden: Impressum-Link im Sidebar-Footer ist sichtbar, wenn Datei vorhanden ist.</summary>
    [Fact]
    public async Task SidebarFooter_LinkSichtbar_WennDateiVorhanden()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var link = Page.Locator(".sz-sidebar-footer a[href='/impressum']");
        await Assertions.Expect(link).ToBeVisibleAsync();
    }
}
