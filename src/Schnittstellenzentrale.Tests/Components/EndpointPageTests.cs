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

    private static Core.Models.Endpoint CreateEndpoint(string? body = null) => new()
    {
        Id = 1,
        Name = "Test",
        RelativePath = "/test",
        Method = Core.Enums.HttpMethod.GET,
        Body = body,
        ApplicationId = 1,
        Application = new Application { Id = 1, Name = "App", BaseUrl = "http://example.com" },
        Headers = [],
        QueryParameters = []
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
}
