using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Infrastructure.Testing;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für sprachspezifisches Impressum-Verhalten: sprachspezifische Datei vorhanden.</summary>
[Collection("PlaywrightImpressumLanguage")]
[OsInterface]
public class ImpressumPageWithLanguageTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit dem PlaywrightImpressumLanguageServer.</summary>
    /// <param name="server">Die gemeinsame Server-Fixture der Test-Collection.</param>
    /// <param name="output">Testausgabe-Hilfsinstanz für Diagnose- und Dispose-Logs.</param>
    public ImpressumPageWithLanguageTests(PlaywrightImpressumLanguageServer server, ITestOutputHelper output) : base(server, output) { }

    /// <summary>ImpressumSeite_ZeigtSprachspezifischenInhalt_BeiDeutscherSprache: Seite zeigt Inhalt aus impressum.de.md wenn Deutsch-Locale aktiv ist.</summary>
    [Fact]
    public async Task ImpressumSeite_ZeigtSprachspezifischenInhalt_BeiDeutscherSprache()
    {
        await Page.GotoAsync($"{BaseUrl}/impressum");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var content = Page.Locator(".sz-impressum-page").GetByText("deutsches Impressum");
        await Assertions.Expect(content).ToBeVisibleAsync();
    }
}

/// <summary>Playwright-Tests für Impressum-Fallback-Verhalten: keine sprachspezifische Datei vorhanden.</summary>
[Collection("PlaywrightImpressumFallback")]
[OsInterface]
public class ImpressumPageWithFallbackTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit dem PlaywrightImpressumFallbackServer.</summary>
    /// <param name="server">Die gemeinsame Server-Fixture der Test-Collection.</param>
    /// <param name="output">Testausgabe-Hilfsinstanz für Diagnose- und Dispose-Logs.</param>
    public ImpressumPageWithFallbackTests(PlaywrightImpressumFallbackServer server, ITestOutputHelper output) : base(server, output) { }

    /// <summary>ImpressumSeite_ZeigtFallbackInhalt_WennSprachspezifischeDateiFehlt: Seite zeigt Fallback-Inhalt aus impressum.md wenn keine sprachspezifische Datei existiert.</summary>
    [Fact]
    public async Task ImpressumSeite_ZeigtFallbackInhalt_WennSprachspezifischeDateiFehlt()
    {
        await Page.GotoAsync($"{BaseUrl}/impressum");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var content = Page.Locator(".sz-impressum-page").GetByText("Fallback-Impressum");
        await Assertions.Expect(content).ToBeVisibleAsync();
    }
}
