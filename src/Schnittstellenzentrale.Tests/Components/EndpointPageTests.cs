using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Endpoint = Schnittstellenzentrale.Core.Models.Endpoint;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für die <see cref="EndpointPage"/>-Komponente.</summary>
public class EndpointPageTests : BunitContext
{
    private readonly Mock<IApplicationApiClient> _apiClientMock = new();
    private readonly Mock<IEndpointExecutionService> _executionMock = new();
    private readonly Mock<IStorageModeService> _storageMock = new();
    private readonly Mock<ISignalRNotificationService> _signalRMock = new();
    private readonly Mock<ICredentialService> _credentialMock = new();
    private readonly Mock<IActiveEnvironmentService> _activeEnvironmentMock = new();

    /// <summary>Initialisiert die Test-Services und JS-Interop-Mocks.</summary>
    public EndpointPageTests()
    {
        _storageMock.Setup(s => s.CurrentMode).Returns(StorageMode.User);
        _activeEnvironmentMock.Setup(s => s.ActiveVariables)
            .Returns(new Dictionary<string, string>());

        Services.AddSingleton(_apiClientMock.Object);
        Services.AddSingleton(_executionMock.Object);
        Services.AddSingleton(_storageMock.Object);
        Services.AddSingleton(_signalRMock.Object);
        Services.AddSingleton(_credentialMock.Object);
        Services.AddSingleton(_activeEnvironmentMock.Object);

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

        Assert.Empty(cut.FindAll(".sz-endpoint-response"));
    }

    /// <summary>Das Anfrageergebnis zeigt den Response-Body korrekt an.</summary>
    [Fact]
    public void AnfrageErgebnis_ResponseBodyWirdKorrektAngezeigt()
    {
        var endpoint = CreateEndpoint();
        const string responseBody = """{"status":"ok"}""";
        _apiClientMock.Setup(r => r.GetEndpointByIdAsync(endpoint.Id)).ReturnsAsync(endpoint);
        _executionMock
            .Setup(e => e.ExecuteAsync(It.IsAny<Core.Models.Endpoint>()))
            .ReturnsAsync(new EndpointExecutionResult
            {
                StatusCode = 200,
                ResponseBody = responseBody,
                ResponseHeaders = new Dictionary<string, string>()
            });

        var cut = Render<EndpointPage>(p => p.Add(x => x.Endpoint, endpoint));
        cut.Find("button.sz-btn-send").Click();

        // ResponseBodyPanel formatiert JSON (pretty-print), daher Teilstring prüfen
        Assert.Contains("status", cut.Find("pre").TextContent);
        Assert.Contains("ok", cut.Find("pre").TextContent);
    }

    /// <summary>Das Anfrageergebnis zeigt den HTTP-Statuscode an.</summary>
    [Fact]
    public void AnfrageErgebnis_StatusCodeWirdAngezeigt()
    {
        var endpoint = CreateEndpoint();
        _apiClientMock.Setup(r => r.GetEndpointByIdAsync(endpoint.Id)).ReturnsAsync(endpoint);
        _executionMock
            .Setup(e => e.ExecuteAsync(It.IsAny<Core.Models.Endpoint>()))
            .ReturnsAsync(new EndpointExecutionResult
            {
                StatusCode = 404,
                ResponseBody = "Not Found",
                ResponseHeaders = new Dictionary<string, string>()
            });

        var cut = Render<EndpointPage>(p => p.Add(x => x.Endpoint, endpoint));
        cut.Find("button.sz-btn-send").Click();

        Assert.Contains("404", cut.Find(".sz-endpoint-response").TextContent);
    }

    /// <summary>Die Body-Textarea zeigt den gespeicherten Body-Inhalt des Endpunkts an.</summary>
    [Fact]
    public void EndpunktMitBody_TextareaZeigtGespeichertenBody()
    {
        const string body = """{"name":"test"}""";
        var endpoint = CreateEndpoint(body: body);

        var cut = Render<EndpointPage>(p => p.Add(x => x.Endpoint, endpoint));
        cut.FindAll("button.sz-endpoint-tab")
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
        cut.FindAll("button.sz-endpoint-tab")
            .First(b => b.TextContent.Trim() == "Query-Parameter")
            .Click();

        var rows = cut.FindAll(".sz-panel-table-wrapper tbody tr");
        Assert.Single(rows);

        var deleteButtons = cut.FindAll(".sz-panel-table-wrapper tbody tr .sz-btn-danger");
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
        cut.FindAll("button.sz-endpoint-tab")
            .First(b => b.TextContent.Trim() == "Query-Parameter")
            .Click();

        // Wert für den Platzhalter eingeben (Change löst @onchange aus)
        var rows = cut.FindAll(".sz-panel-table-wrapper tbody tr");
        var valueInput = rows[0].QuerySelectorAll("input")[1];
        valueInput.Change("42");

        // Pfadfeld blur — SyncPathParameters wird erneut aufgerufen, Pfad unverändert
        var pathInput = cut.Find("input[placeholder='Relativer Pfad']");
        pathInput.Blur();

        // Wert muss noch vorhanden sein
        rows = cut.FindAll(".sz-panel-table-wrapper tbody tr");
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
        cut.FindAll("button.sz-endpoint-tab")
            .First(b => b.TextContent.Trim() == "Query-Parameter")
            .Click();

        // Es darf nur einen einzigen Eintrag für "id" geben
        var rows = cut.FindAll(".sz-panel-table-wrapper tbody tr");
        Assert.Single(rows);

        var inputs = rows[0].QuerySelectorAll("input");
        Assert.Equal("id", inputs[0].GetAttribute("value"));
        Assert.Equal("42", inputs[1].GetAttribute("value"));

        // Kein Löschen-Button — der Eintrag wurde zum Pfad-Parameter hochgestuft
        Assert.Empty(cut.FindAll(".sz-panel-table-wrapper tbody tr .sz-btn-danger"));
    }

    /// <summary>Nach OnPathBlur mit geändertem Pfad werden entfernte Platzhalter gelöscht und neue hinzugefügt.</summary>
    [Fact]
    public void GeaenderterPfad_EntferntWeggefalleneUndFuegtNeueHinzu()
    {
        var endpoint = CreateEndpoint(relPath: "/api/{alterId}/items");

        var cut = Render<EndpointPage>(p => p.Add(x => x.Endpoint, endpoint));
        cut.FindAll("button.sz-endpoint-tab")
            .First(b => b.TextContent.Trim() == "Query-Parameter")
            .Click();

        var pathInput = cut.Find("input[placeholder='Relativer Pfad']");
        pathInput.Input("/api/{neuerId}/data");
        pathInput.Blur();

        var rows = cut.FindAll(".sz-panel-table-wrapper tbody tr");
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
        cut.FindAll("button.sz-endpoint-tab")
            .First(b => b.TextContent.Trim() == "Query-Parameter")
            .Click();

        // Query-Parameter wurden als löschbare Einträge extrahiert
        var rows = cut.FindAll(".sz-panel-table-wrapper tbody tr");
        Assert.Equal(2, rows.Count);

        var deleteButtons = cut.FindAll(".sz-panel-table-wrapper tbody tr .sz-btn-danger");
        Assert.Equal(2, deleteButtons.Count);

        // Der Pfad wird intern bereinigt — ResolveDisplayUrl() hängt die Parameter als Query-String an,
        // daher zeigt das Pfadfeld die rekonstruierte URL; der Pfad-Anteil selbst enthält kein '?'
        var pathInput = cut.Find("input[placeholder='Relativer Pfad']");
        var displayValue = pathInput.GetAttribute("value") ?? string.Empty;
        var pathPart = displayValue.Contains('?') ? displayValue[..displayValue.IndexOf('?')] : displayValue;
        Assert.Equal("/api/items", pathPart);
    }

    /// <summary>Registerkarte „Pre-Request-Skript" ist im DOM vorhanden.</summary>
    [Fact]
    public void PreRequestSkript_RegistorkarteWirdGerendert()
    {
        var endpoint = CreateEndpoint();

        var cut = Render<EndpointPage>(p => p.Add(x => x.Endpoint, endpoint));

        var tab = cut.FindAll("button.sz-endpoint-tab").FirstOrDefault(b => b.TextContent.Trim() == "Pre-Request-Skript");
        Assert.NotNull(tab);
    }

    /// <summary>Registerkarte „Post-Request-Skript" ist im DOM vorhanden.</summary>
    [Fact]
    public void PostRequestSkript_RegistorkarteWirdGerendert()
    {
        var endpoint = CreateEndpoint();

        var cut = Render<EndpointPage>(p => p.Add(x => x.Endpoint, endpoint));

        var tab = cut.FindAll("button.sz-endpoint-tab").FirstOrDefault(b => b.TextContent.Trim() == "Post-Request-Skript");
        Assert.NotNull(tab);
    }

    /// <summary>Textänderung im Pre-Skript-Textarea ruft MarkDirty() auf.</summary>
    [Fact]
    public void PreRequestSkript_AenderungLoestMarkDirtyAus()
    {
        var endpoint = CreateEndpoint();

        var cut = Render<EndpointPage>(p => p.Add(x => x.Endpoint, endpoint));
        cut.FindAll("button.sz-endpoint-tab")
            .First(b => b.TextContent.Trim() == "Pre-Request-Skript")
            .Click();

        cut.Find("textarea").Input("sz.environment.set('x', '1');");

        Assert.NotEmpty(cut.FindAll(".sz-endpoint-dirty-badge"));
    }

    /// <summary>Textänderung im Post-Skript-Textarea ruft MarkDirty() auf.</summary>
    [Fact]
    public void PostRequestSkript_AenderungLoestMarkDirtyAus()
    {
        var endpoint = CreateEndpoint();

        var cut = Render<EndpointPage>(p => p.Add(x => x.Endpoint, endpoint));
        cut.FindAll("button.sz-endpoint-tab")
            .First(b => b.TextContent.Trim() == "Post-Request-Skript")
            .Click();

        cut.Find("textarea").Input("sz.environment.set('x', '1');");

        Assert.NotEmpty(cut.FindAll(".sz-endpoint-dirty-badge"));
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

        cut.FindAll("button.sz-endpoint-tab")
            .First(b => b.TextContent.Trim() == "Query-Parameter")
            .Click();

        // Wert für den Platzhalter eingeben (Change löst @onchange aus)
        var rows = cut.FindAll(".sz-panel-table-wrapper tbody tr");
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
