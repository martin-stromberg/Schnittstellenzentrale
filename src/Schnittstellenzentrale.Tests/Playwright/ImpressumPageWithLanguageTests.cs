using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>Playwright-Tests für sprachabhängiges Impressum-Verhalten über den Accept-Language-Header.</summary>
[Collection("PlaywrightImpressumLanguage")]
public class ImpressumPageWithLanguageTests : PlaywrightTestBase
{
    private IBrowserContext _germanContext = null!;
    private IPage _germanPage = null!;

    private IBrowserContext _frenchContext = null!;
    private IPage _frenchPage = null!;

    /// <summary>Initialisiert den Test mit dem PlaywrightImpressumLanguageServer.</summary>
    public ImpressumPageWithLanguageTests(PlaywrightImpressumLanguageServer server) : base(server) { }

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        _germanContext = await CreateAdditionalContextAsync();
        await _germanContext.SetExtraHTTPHeadersAsync(
            new Dictionary<string, string> { { "Accept-Language", "de" } });
        _germanPage = await _germanContext.NewPageAsync();

        _frenchContext = await CreateAdditionalContextAsync();
        await _frenchContext.SetExtraHTTPHeadersAsync(
            new Dictionary<string, string> { { "Accept-Language", "fr" } });
        _frenchPage = await _frenchContext.NewPageAsync();
    }

    /// <inheritdoc/>
    protected override async Task OnDisposingAsync()
    {
        Directory.CreateDirectory("playwright-traces");

        await _germanContext.Tracing.StopAsync(new TracingStopOptions
        {
            Path = Path.Combine("playwright-traces", $"{nameof(ImpressumPageWithLanguageTests)}-de.zip")
        });
        await _germanContext.DisposeAsync();

        await _frenchContext.Tracing.StopAsync(new TracingStopOptions
        {
            Path = Path.Combine("playwright-traces", $"{nameof(ImpressumPageWithLanguageTests)}-fr.zip")
        });
        await _frenchContext.DisposeAsync();
    }

    /// <summary>ImpressumSeite_ZeigtSprachspezifischenInhalt_BeiDeutscherSprache: Accept-Language de → Seite zeigt Inhalt aus impressum.de.md.</summary>
    [Fact]
    public async Task ImpressumSeite_ZeigtSprachspezifischenInhalt_BeiDeutscherSprache()
    {
        await _germanPage.GotoAsync($"{BaseUrl}/impressum");
        await _germanPage.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var content = _germanPage.Locator(".sz-impressum-page").GetByText("deutsches Impressum");
        await Assertions.Expect(content).ToBeVisibleAsync();
    }

    /// <summary>ImpressumSeite_ZeigtFallbackInhalt_WennSprachspezifischeDateiFehlt: Accept-Language fr + nur impressum.md → Seite zeigt Fallback-Inhalt.</summary>
    [Fact]
    public async Task ImpressumSeite_ZeigtFallbackInhalt_WennSprachspezifischeDateiFehlt()
    {
        await _frenchPage.GotoAsync($"{BaseUrl}/impressum");
        await _frenchPage.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var content = _frenchPage.Locator(".sz-impressum-page").GetByText("Fallback-Impressum");
        await Assertions.Expect(content).ToBeVisibleAsync();
    }
}
