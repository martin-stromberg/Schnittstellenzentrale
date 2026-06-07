using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>bUnit-Tests für die <see cref="ApplicationContentView"/>-Komponente.</summary>
public class ApplicationContentViewTests : BunitContext
{
    private readonly Mock<IApplicationService> _applicationServiceMock = new();
    private readonly Mock<ISwaggerImportService> _swaggerImportMock = new();
    private readonly Mock<IODataImportService> _odataImportMock = new();
    private readonly Mock<IHealthCheckService> _healthCheckMock = new();
    private readonly Mock<IApplicationApiClient> _apiClientMock = new();
    private readonly Mock<IApplicationLinkService> _applicationLinkServiceMock = new();
    private readonly Mock<IHistoryService> _historyServiceMock = new();

    /// <summary>Initialisiert die Test-Services.</summary>
    public ApplicationContentViewTests()
    {
        _apiClientMock.Setup(c => c.GetEndpointsAsync(It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync([]);
        _applicationLinkServiceMock.Setup(s => s.GetLinksAsync(It.IsAny<int>())).ReturnsAsync([]);
        _historyServiceMock.Setup(s => s.GetTopEndpointsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync([]);

        Services.AddSingleton(_applicationServiceMock.Object);
        Services.AddSingleton(_swaggerImportMock.Object);
        Services.AddSingleton(_odataImportMock.Object);
        Services.AddSingleton(_healthCheckMock.Object);
        Services.AddSingleton(_apiClientMock.Object);
        Services.AddSingleton(_applicationLinkServiceMock.Object);
        Services.AddSingleton(_historyServiceMock.Object);
        Services.AddSingleton(TestMockFactory.CreateFakeLocalizer());
    }

    private static Application CreateODataApplication(string interfaceUrl = "http://example.com/$metadata") => new()
    {
        Id = 1,
        Name = "Test OData App",
        BaseUrl = "http://example.com",
        InterfaceType = InterfaceType.OData,
        InterfaceUrl = interfaceUrl
    };

    private static Application CreateRestApplication() => new()
    {
        Id = 2,
        Name = "Test REST App",
        BaseUrl = "http://example.com",
        InterfaceType = InterfaceType.Rest,
        InterfaceUrl = "http://example.com/swagger.json"
    };

    /// <summary>Der OData-Import-Button ist sichtbar, wenn InterfaceType OData und InterfaceUrl gesetzt ist.</summary>
    [Fact]
    public void ODataImportButton_VisibleForODataApplication()
    {
        var app = CreateODataApplication();

        var cut = Render<ApplicationContentView>(p => p.Add(x => x.Application, app));

        var buttons = cut.FindAll("button");
        Assert.Contains(buttons, b => b.TextContent.Contains("ApplicationContentView_Button_ODataImport"));
    }

    /// <summary>Der OData-Import-Button ist nicht sichtbar, wenn InterfaceType nicht OData ist.</summary>
    [Fact]
    public void ODataImportButton_HiddenForRestApplication()
    {
        var app = CreateRestApplication();

        var cut = Render<ApplicationContentView>(p => p.Add(x => x.Application, app));

        var buttons = cut.FindAll("button");
        Assert.DoesNotContain(buttons, b => b.TextContent.Contains("ApplicationContentView_Button_ODataImport"));
    }

    /// <summary>Der OData-Import-Button ist nicht sichtbar, wenn InterfaceUrl leer ist.</summary>
    [Fact]
    public void ODataImportButton_HiddenWhenInterfaceUrlEmpty()
    {
        var app = CreateODataApplication(interfaceUrl: "");

        var cut = Render<ApplicationContentView>(p => p.Add(x => x.Application, app));

        var buttons = cut.FindAll("button");
        Assert.DoesNotContain(buttons, b => b.TextContent.Contains("ApplicationContentView_Button_ODataImport"));
    }

    /// <summary>Bei gesetztem ErrorMessage wird kein Dialog geöffnet und die Fehlermeldung wird angezeigt.</summary>
    [Fact]
    public async Task OpenODataImport_OnError_ShowsErrorMessage()
    {
        var app = CreateODataApplication();
        _odataImportMock
            .Setup(s => s.ImportAsync(app))
            .ReturnsAsync(new ImportDiff { ErrorMessage = "Connection refused" });

        var cut = Render<ApplicationContentView>(p => p.Add(x => x.Application, app));
        var button = cut.FindAll("button")
            .Single(b => b.TextContent.Contains("ApplicationContentView_Button_ODataImport"));

        await cut.InvokeAsync(() => button.Click());

        Assert.Contains("Connection refused", cut.Markup);
        Assert.Empty(cut.FindComponents<ODataImportDialog>());
    }

    /// <summary>Bei erfolgreichem Import wird der ODataImportDialog angezeigt.</summary>
    [Fact]
    public async Task OpenODataImport_OnSuccess_OpensDialog()
    {
        var app = CreateODataApplication();
        _odataImportMock
            .Setup(s => s.ImportAsync(app))
            .ReturnsAsync(new ImportDiff());

        var cut = Render<ApplicationContentView>(p => p.Add(x => x.Application, app));
        var button = cut.FindAll("button")
            .Single(b => b.TextContent.Contains("ApplicationContentView_Button_ODataImport"));

        await cut.InvokeAsync(() => button.Click());

        Assert.Single(cut.FindComponents<ODataImportDialog>());
    }
}
