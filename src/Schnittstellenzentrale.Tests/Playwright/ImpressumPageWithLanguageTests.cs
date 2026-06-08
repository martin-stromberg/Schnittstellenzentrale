using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für sprachspezifisches Impressum-Verhalten: sprachspezifische Datei vorhanden.</summary>
[Collection("PlaywrightImpressumLanguage")]
public class ImpressumPageWithLanguageTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit dem PlaywrightImpressumLanguageServer.</summary>
    public ImpressumPageWithLanguageTests(PlaywrightImpressumLanguageServer server) : base(server) { }

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
public class ImpressumPageWithFallbackTests : PlaywrightTestBase
{
    /// <summary>Initialisiert den Test mit dem PlaywrightImpressumFallbackServer.</summary>
    public ImpressumPageWithFallbackTests(PlaywrightImpressumFallbackServer server) : base(server) { }

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
