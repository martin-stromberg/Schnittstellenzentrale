namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>xUnit-Collection für SignalR-Playwright-Tests; verhindert parallele Ausführung mit anderen Playwright-Tests.</summary>
[CollectionDefinition("PlaywrightSignalR")]
public class PlaywrightSignalRCollection : ICollectionFixture<PlaywrightSignalRServer>
{
}
