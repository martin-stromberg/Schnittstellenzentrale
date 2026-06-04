namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>xUnit-Collection für Impressum-Playwright-Tests; läuft mit einer eigenen Server-Instanz mit Impressum-Datei.</summary>
[CollectionDefinition("PlaywrightImpressum")]
public class PlaywrightImpressumCollection : ICollectionFixture<PlaywrightImpressumServer>
{
}
