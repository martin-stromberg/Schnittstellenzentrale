namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>xUnit-Collection für Impressum-Tests mit Fallback-Datei (keine sprachspezifische Variante).</summary>
[CollectionDefinition("PlaywrightImpressumFallback")]
public class PlaywrightImpressumFallbackCollection : ICollectionFixture<PlaywrightImpressumFallbackServer>
{
}
