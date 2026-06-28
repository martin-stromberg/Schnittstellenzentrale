namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>xUnit-Collection für OData-Endpunkt-Ausführungstests; verhindert parallele Ausführung mit anderen Playwright-Tests.</summary>
[CollectionDefinition("PlaywrightODataEndpoint")]
public class PlaywrightODataEndpointCollection : ICollectionFixture<PlaywrightODataEndpointServer>
{
}
