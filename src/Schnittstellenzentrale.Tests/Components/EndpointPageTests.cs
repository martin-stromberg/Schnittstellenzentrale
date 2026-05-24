using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für die <see cref="EndpointPage"/>-Komponente.</summary>
public class EndpointPageTests : BunitContext
{
    private readonly Mock<IEndpointRepository> _repoMock = new();
    private readonly Mock<IEndpointExecutionService> _executionMock = new();
    private readonly Mock<IStorageModeService> _storageMock = new();
    private readonly Mock<ISignalRNotificationService> _signalRMock = new();
    private readonly Mock<ICredentialService> _credentialMock = new();

    /// <summary>Initialisiert die Test-Services und JS-Interop-Mocks.</summary>
    public EndpointPageTests()
    {
        _storageMock.Setup(s => s.CurrentMode).Returns(StorageMode.User);

        Services.AddSingleton(_repoMock.Object);
        Services.AddSingleton(_executionMock.Object);
        Services.AddSingleton(_storageMock.Object);
        Services.AddSingleton(_signalRMock.Object);
        Services.AddSingleton(_credentialMock.Object);

        var jsModule = JSInterop.SetupModule("./endpoint-page.js");
        jsModule.SetupVoid("registerSaveShortcut", _ => true);
        jsModule.SetupVoid("enableBeforeUnloadGuard");
        jsModule.SetupVoid("disableBeforeUnloadGuard");
        jsModule.SetupVoid("unregisterSaveShortcut");
    }

    private static Core.Models.Endpoint CreateEndpoint(
        string? body = null,
        string relPath = "/test",
        RequestQueryParamsPanel.QueryParamEntry[]? queryParameters = null) => new()
    {
        Id = 1,
        Name = "Test",
        RelativePath = relPath,
        Method = Core.Enums.HttpMethod.GET,
        Body = body,
        ApplicationId = 1,
        Application = new Application { Id = 1, Name = "App", BaseUrl = "http://example.com" },
        Headers = [],
        QueryParameters = queryParameters?
            .Select(p => new EndpointQueryParameter { Key = p.Key, Value = p.Value })
            .ToList() ?? []
    };

    /// <summary>Ohne Anfrageergebnis ist der Antwortbereich nicht sichtbar.</summary>
    [Fact]
    public void OhneAnfrageergebnis_AntwortBereichNichtSichtbar()
    {
        var cut = Render<EndpointPage>(p => p.Add(x => x.Endpoint, CreateEndpoint()));

        Assert.Empty(cut.FindAll(".response-section"));
    }

    /// <summary>Das Anfrageergebnis zeigt den Response-Body korrekt an.</summary>
    [Fact]
    public void AnfrageErgebnis_ResponseBodyWirdKorrektAngezeigt()
    {
        var endpoint = CreateEndpoint();
        const string responseBody = """{"status":"ok"}""";
        _repoMock.Setup(r => r.GetEndpointByIdAsync(endpoint.Id)).ReturnsAsync(endpoint);
        _executionMock
            .Setup(e => e.ExecuteAsync(It.IsAny<Core.Models.Endpoint>()))
            .ReturnsAsync(new EndpointExecutionResult
            {
                StatusCode = 200,
                ResponseBody = responseBody,
                ResponseHeaders = new Dictionary<string, string>()
            });

        var cut = Render<EndpointPage>(p => p.Add(x => x.Endpoint, endpoint));
        cut.Find("button.btn-success").Click();

        // ResponseBodyPanel formatiert JSON (pretty-print), daher Teilstring prüfen
        Assert.Contains("status", cut.Find("pre").TextContent);
        Assert.Contains("ok", cut.Find("pre").TextContent);
    }

    /// <summary>Das Anfrageergebnis zeigt den HTTP-Statuscode an.</summary>
    [Fact]
    public void AnfrageErgebnis_StatusCodeWirdAngezeigt()
    {
        var endpoint = CreateEndpoint();
        _repoMock.Setup(r => r.GetEndpointByIdAsync(endpoint.Id)).ReturnsAsync(endpoint);
        _executionMock
            .Setup(e => e.ExecuteAsync(It.IsAny<Core.Models.Endpoint>()))
            .ReturnsAsync(new EndpointExecutionResult
            {
                StatusCode = 404,
                ResponseBody = "Not Found",
                ResponseHeaders = new Dictionary<string, string>()
            });

        var cut = Render<EndpointPage>(p => p.Add(x => x.Endpoint, endpoint));
        cut.Find("button.btn-success").Click();

        Assert.Contains("404", cut.Find(".response-section").TextContent);
    }

    /// <summary>Die Body-Textarea zeigt den gespeicherten Body-Inhalt des Endpunkts an.</summary>
    [Fact]
    public void EndpunktMitBody_TextareaZeigtGespeichertenBody()
    {
        const string body = """{"name":"test"}""";
        var endpoint = CreateEndpoint(body: body);

        var cut = Render<EndpointPage>(p => p.Add(x => x.Endpoint, endpoint));
        cut.FindAll("button.nav-link")
            .First(b => b.TextContent.Trim() == "Body")
            .Click();

        Assert.Equal(body, cut.Find("textarea").GetAttribute("value"));
    }

    /// <summary>Platzhalter in RelativePath erscheinen beim Laden als IsPathParameter=true-Einträge im Query-Parameter-Panel.</summary>
    [Fact]
    public void PfadMitPlatzhalter_WirdBeimLadenAlsNichtLoeschbarerEintragAngezeigt()
    {
        var endpoint = CreateEndpoint(relPath: "/api/{id}/items");

        var cut = Render<EndpointPage>(p => p.Add(x => x.Endpoint, endpoint));
        cut.FindAll("button.nav-link")
            .First(b => b.TextContent.Trim() == "Query-Parameter")
            .Click();

        var rows = cut.FindAll(".request-query-params-panel tbody tr");
        Assert.Single(rows);

        var deleteButtons = cut.FindAll(".request-query-params-panel tbody tr .btn-outline-danger");
        Assert.Empty(deleteButtons);

        var inputs = rows[0].QuerySelectorAll("input");
        Assert.Equal("id", inputs[0].GetAttribute("value"));
    }

    /// <summary>Beim erneuten Aufruf von SyncPathParameters bleiben gespeicherte Werte für unveränderte Platzhalter erhalten.</summary>
    [Fact]
    public void PfadMitPlatzhalter_VorhandenerWertBleibtErhalten_WennPlatzhalterUnveraendert()
    {
        var endpoint = CreateEndpoint(relPath: "/api/{id}/items");

        var cut = Render<EndpointPage>(p => p.Add(x => x.Endpoint, endpoint));
        cut.FindAll("button.nav-link")
            .First(b => b.TextContent.Trim() == "Query-Parameter")
            .Click();

        // Wert für den Platzhalter eingeben (Change löst @onchange aus)
        var rows = cut.FindAll(".request-query-params-panel tbody tr");
        var valueInput = rows[0].QuerySelectorAll("input")[1];
        valueInput.Change("42");

        // Pfadfeld blur — SyncPathParameters wird erneut aufgerufen, Pfad unverändert
        var pathInput = cut.Find("input[placeholder='Relativer Pfad']");
        pathInput.Blur();

        // Wert muss noch vorhanden sein
        rows = cut.FindAll(".request-query-params-panel tbody tr");
        Assert.Single(rows);
        var inputs = rows[0].QuerySelectorAll("input");
        Assert.Equal("id", inputs[0].GetAttribute("value"));
        Assert.Equal("42", inputs[1].GetAttribute("value"));
    }

    /// <summary>Ein aus der DB geladener Platzhalter-Wert wird nicht als Duplikat-Eintrag angelegt.</summary>
    [Fact]
    public void GespeicherterPlatzhalterWert_WirdNachLadenNichtDupliziert()
    {
        // Endpunkt so, wie er nach einem Speichern aus der DB zurückkommt:
        // RelativePath enthält den Platzhalter, QueryParameters den gespeicherten Wert (ohne IsPathParameter-Flag)
        var queryParams = new[]
        {
            new RequestQueryParamsPanel.QueryParamEntry { Key = "id", Value = "42" }
        };
        var endpoint = CreateEndpoint(relPath: "/api/{id}/items", queryParameters: queryParams);

        var cut = Render<EndpointPage>(p => p.Add(x => x.Endpoint, endpoint));
        cut.FindAll("button.nav-link")
            .First(b => b.TextContent.Trim() == "Query-Parameter")
            .Click();

        // Es darf nur einen einzigen Eintrag für "id" geben
        var rows = cut.FindAll(".request-query-params-panel tbody tr");
        Assert.Single(rows);

        var inputs = rows[0].QuerySelectorAll("input");
        Assert.Equal("id", inputs[0].GetAttribute("value"));
        Assert.Equal("42", inputs[1].GetAttribute("value"));

        // Kein Löschen-Button — der Eintrag wurde zum Pfad-Parameter hochgestuft
        Assert.Empty(cut.FindAll(".request-query-params-panel tbody tr .btn-outline-danger"));
    }

    /// <summary>Nach OnPathBlur mit geändertem Pfad werden entfernte Platzhalter gelöscht und neue hinzugefügt.</summary>
    [Fact]
    public void GeaenderterPfad_EntferntWeggefalleneUndFuegtNeueHinzu()
    {
        var endpoint = CreateEndpoint(relPath: "/api/{alterId}/items");

        var cut = Render<EndpointPage>(p => p.Add(x => x.Endpoint, endpoint));
        cut.FindAll("button.nav-link")
            .First(b => b.TextContent.Trim() == "Query-Parameter")
            .Click();

        var pathInput = cut.Find("input[placeholder='Relativer Pfad']");
        pathInput.Input("/api/{neuerId}/data");
        pathInput.Blur();

        var rows = cut.FindAll(".request-query-params-panel tbody tr");
        Assert.Single(rows);

        var firstRowInputs = rows[0].QuerySelectorAll("input");
        Assert.Equal("neuerId", firstRowInputs[0].GetAttribute("value"));

        var allKeyValues = rows
            .Select(r => r.QuerySelectorAll("input")[0].GetAttribute("value"))
            .ToList();
        Assert.DoesNotContain("alterId", allKeyValues);
    }

    /// <summary>ExtractAndStripQueryString trennt den Query-String vom Pfad und fügt die Parameter als löschbare Einträge hinzu.</summary>
    [Fact]
    public void PfadMitQueryString_WirdExtrahiertUndPfadBereinigt()
    {
        var endpoint = CreateEndpoint(relPath: "/api/items?filter=active&page=1");

        var cut = Render<EndpointPage>(p => p.Add(x => x.Endpoint, endpoint));
        cut.FindAll("button.nav-link")
            .First(b => b.TextContent.Trim() == "Query-Parameter")
            .Click();

        // Query-Parameter wurden als löschbare Einträge extrahiert
        var rows = cut.FindAll(".request-query-params-panel tbody tr");
        Assert.Equal(2, rows.Count);

        var deleteButtons = cut.FindAll(".request-query-params-panel tbody tr .btn-outline-danger");
        Assert.Equal(2, deleteButtons.Count);

        // Der Pfad wird intern bereinigt — ResolveDisplayUrl() hängt die Parameter als Query-String an,
        // daher zeigt das Pfadfeld die rekonstruierte URL; der Pfad-Anteil selbst enthält kein '?'
        var pathInput = cut.Find("input[placeholder='Relativer Pfad']");
        var displayValue = pathInput.GetAttribute("value") ?? string.Empty;
        var pathPart = displayValue.Contains('?') ? displayValue[..displayValue.IndexOf('?')] : displayValue;
        Assert.Equal("/api/items", pathPart);
    }

    /// <summary>ResolveDisplayUrl zeigt bei leerem Wert den Platzhalter; nach Werteingabe die aufgelöste URL.</summary>
    [Fact]
    public void AufgeloesteUrl_WirdImPfadfeldAngezeigt()
    {
        // Endpunkt mit Pfad-Platzhalter und einem regulären Query-Parameter laden
        var queryParams = new[]
        {
            new RequestQueryParamsPanel.QueryParamEntry { Key = "filter", Value = "active" }
        };
        var endpoint = CreateEndpoint(relPath: "/api/{id}/items", queryParameters: queryParams);

        var cut = Render<EndpointPage>(p => p.Add(x => x.Endpoint, endpoint));

        // Vor der Werteingabe: Platzhalter bleibt im Pfadfeld sichtbar (leer → {id} beibehalten)
        var pathInput = cut.Find("input[placeholder='Relativer Pfad']");
        Assert.Contains("{id}", pathInput.GetAttribute("value") ?? string.Empty);

        cut.FindAll("button.nav-link")
            .First(b => b.TextContent.Trim() == "Query-Parameter")
            .Click();

        // Wert für den Platzhalter eingeben (Change löst @onchange aus)
        var rows = cut.FindAll(".request-query-params-panel tbody tr");
        var idRow = rows.First(r => r.QuerySelector("input")?.GetAttribute("value") == "id");
        var idValueInput = idRow.QuerySelectorAll("input")[1];
        idValueInput.Change("42");

        // Pfadfeld zeigt die aufgelöste URL (Platzhalter ersetzt, Query-Parameter angehängt)
        pathInput = cut.Find("input[placeholder='Relativer Pfad']");
        var displayValue = pathInput.GetAttribute("value") ?? string.Empty;

        Assert.Contains("42", displayValue);
        Assert.Contains("filter=active", displayValue);
        Assert.DoesNotContain("{id}", displayValue);
    }
}
