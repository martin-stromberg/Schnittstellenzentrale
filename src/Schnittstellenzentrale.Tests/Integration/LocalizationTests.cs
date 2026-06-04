using System.Net;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Integration;

/// <summary>Integrationstests für das Accept-Language-basierte Lokalisierungsverhalten.</summary>
public class LocalizationTests : IClassFixture<ControllerTestFactory>
{
    private readonly ControllerTestFactory _factory;

    /// <summary>Initialisiert LocalizationTests.</summary>
    public LocalizationTests(ControllerTestFactory factory)
    {
        _factory = factory;
    }

    /// <summary>GET-Request mit Accept-Language: de liefert deutschen UI-Text in der Response.</summary>
    [Fact]
    public async Task DeRequestMitAcceptLanguageDe_ZeigtDeutscheTexte()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept-Language", "de");

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Neu laden", content);
    }

    /// <summary>GET-Request mit Accept-Language: en liefert englischen UI-Text.</summary>
    [Fact]
    public async Task DeRequestMitAcceptLanguageEn_ZeigtEnglischeTexte()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept-Language", "en");

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Reload", content);
    }

    /// <summary>GET-Request ohne Accept-Language-Header fällt auf Englisch zurück.</summary>
    [Fact]
    public async Task DeRequestOhneAcceptLanguage_ZeigtEnglischeTexte()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Reload", content);
    }

    /// <summary>GET-Request mit unbekanntem Accept-Language-Header fällt auf Englisch zurück.</summary>
    [Fact]
    public async Task DeRequestMitUnbekannterSprache_ZeigtEnglischeTexte()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept-Language", "fr");

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Reload", content);
    }
}
