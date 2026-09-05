using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Schnittstellenzentrale.Components.Shared;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Helpers;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Components;

/// <summary>Ergänzt gezielte bUnit-Tests für bislang unterabgedeckte UI- und State-Pfade.</summary>
public class CoverageGapComponentTests : BunitContext
{
    /// <summary>Swagger-Import zeigt bei Fehler eine Fehlermeldung an und öffnet keinen Dialog.</summary>
    [Fact]
    public async Task CoverageGap_SwaggerImportError_ShowsErrorWithoutDialog()
    {
        var dependencies = TestMockFactory.CreateCoverageScenarioDependencies();
        dependencies.ApplicationApiClientMock
            .Setup(c => c.ImportMetadataAsync(dependencies.RestApplication.Id))
            .ReturnsAsync(new Core.Models.ImportDiff { ErrorMessage = "Coverage-Fehlerpfad" });

        var swaggerImportMock = new Mock<ISwaggerImportService>();
        var odataImportMock = new Mock<IODataImportService>();
        var healthCheckMock = new Mock<IHealthCheckService>();
        var applicationLinkServiceMock = new Mock<IApplicationLinkService>();
        var historyServiceMock = new Mock<IHistoryService>();
        applicationLinkServiceMock.Setup(s => s.GetLinksAsync(It.IsAny<int>())).ReturnsAsync([]);
        historyServiceMock.Setup(s => s.GetTopEndpointsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync([]);

        Services.AddSingleton(dependencies.ApplicationServiceMock.Object);
        Services.AddSingleton(swaggerImportMock.Object);
        Services.AddSingleton(odataImportMock.Object);
        Services.AddSingleton(healthCheckMock.Object);
        Services.AddSingleton(dependencies.ApplicationApiClientMock.Object);
        Services.AddSingleton(applicationLinkServiceMock.Object);
        Services.AddSingleton(historyServiceMock.Object);
        Services.AddSingleton(TestMockFactory.CreateFakeLocalizer());

        var cut = Render<ApplicationContentView>(parameters => parameters.Add(x => x.Application, dependencies.RestApplication));
        var button = cut.FindAll("button")
            .Single(b => b.TextContent.Contains("ApplicationContentView_Button_SwaggerImport"));

        await cut.InvokeAsync(() => button.Click());

        Assert.Contains("Coverage-Fehlerpfad", cut.Markup);
        Assert.Empty(cut.FindComponents<SwaggerImportDialog>());
    }

    /// <summary>Eine ungültige Umgebungsauswahl verändert weder Storage noch aktive Umgebung.</summary>
    [Fact]
    public async Task CoverageGap_EnvironmentSelectionInvalid_DoesNotPersistState()
    {
        var dependencies = TestMockFactory.CreateCoverageScenarioDependencies();
        dependencies.EnvironmentRepositoryMock
            .Setup(r => r.GetEnvironmentsAsync(It.IsAny<StorageMode>(), It.IsAny<string?>()))
            .ReturnsAsync([dependencies.SelectedEnvironment]);

        var storageModeServiceMock = new Mock<IStorageModeService>();
        storageModeServiceMock.Setup(s => s.CurrentMode).Returns(StorageMode.Team);
        var currentUserServiceMock = new Mock<ICurrentUserService>();

        Services.AddSingleton(dependencies.EnvironmentRepositoryMock.Object);
        Services.AddSingleton(dependencies.ActiveEnvironmentServiceMock.Object);
        Services.AddSingleton(storageModeServiceMock.Object);
        Services.AddSingleton(currentUserServiceMock.Object);
        Services.AddSingleton(TestMockFactory.CreateFakeLocalizer());

        JSInterop.SetupVoid("localStorage.removeItem", _ => true).SetVoidResult();
        JSInterop.SetupVoid("localStorage.setItem", _ => true).SetVoidResult();

        var cut = Render<EnvironmentSelector>();
        await cut.InvokeAsync(() => cut.Find("select").Change("invalid"));

        var expectedKey = LocalStorageKeys.SelectedEnvironmentId(StorageMode.Team);
        Assert.DoesNotContain(JSInterop.Invocations, i => i.Identifier == "localStorage.setItem");
        Assert.DoesNotContain(
            JSInterop.Invocations,
            i => i.Identifier == "localStorage.removeItem" && Equals(i.Arguments.ElementAt(0), expectedKey));
        dependencies.ActiveEnvironmentServiceMock.Verify(
            s => s.SetActiveEnvironment(It.IsAny<Core.Models.SystemEnvironment?>()),
            Times.Never);
    }
}
