using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Infrastructure.Testing;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;
using Xunit.Abstractions;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für die Impressum-Seite und den Impressum-Link im Sidebar-Footer.</summary>
[Collection("Playwright")]
[OsInterface]
public class ImpressumPageTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit dem gemeinsamen PlaywrightServer (keine Impressum-Datei vorhanden).</summary>
    /// <param name="server">Die gemeinsame Server-Fixture der Test-Collection.</param>
    /// <param name="output">Testausgabe-Hilfsinstanz für Diagnose- und Dispose-Logs.</param>
    public ImpressumPageTests(PlaywrightServer server, ITestOutputHelper output) : base(server, output) { }

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
[OsInterface]
public class ImpressumPageWithFileTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit dem PlaywrightImpressumServer (Impressum-Datei ist vorhanden).</summary>
    /// <param name="server">Die gemeinsame Server-Fixture der Test-Collection.</param>
    /// <param name="output">Testausgabe-Hilfsinstanz für Diagnose- und Dispose-Logs.</param>
    public ImpressumPageWithFileTests(PlaywrightImpressumServer server, ITestOutputHelper output) : base(server, output) { }

    /// <summary>ImpressumSeite_ZeigtInhalt_WennDateiVorhanden: /impressum zeigt Überschrift und Inhalt, wenn Impressum-Datei vorhanden ist.</summary>
    [Fact]
    public async Task ImpressumSeite_ZeigtInhalt_WennDateiVorhanden()
    {
        await Page.GotoAsync($"{BaseUrl}/impressum");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var page = Page.Locator(".sz-impressum-page");
        await Assertions.Expect(page).ToBeVisibleAsync();

        // Heading aus Lokalisierung (EN-Fallback oder DE) — erster h1 ist der lokalisierte Seitentitel
        var heading = Page.Locator(".sz-impressum-page h1").First;
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
