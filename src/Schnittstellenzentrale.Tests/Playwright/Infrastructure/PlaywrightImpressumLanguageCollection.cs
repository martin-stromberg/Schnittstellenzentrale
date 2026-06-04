namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>xUnit-Collection für sprachabhängige Impressum-Playwright-Tests; läuft mit einer eigenen Server-Instanz mit Fallback- und sprachspezifischer Impressum-Datei.</summary>
[CollectionDefinition("PlaywrightImpressumLanguage")]
public class PlaywrightImpressumLanguageCollection : ICollectionFixture<PlaywrightImpressumLanguageServer>
{
}
