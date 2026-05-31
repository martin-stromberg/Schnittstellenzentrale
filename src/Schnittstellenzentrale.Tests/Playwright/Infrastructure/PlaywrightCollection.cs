namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>xUnit-Collection für alle Playwright-Tests; verhindert parallele Ausführung zwischen Testklassen.</summary>
[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightServer>
{
}
