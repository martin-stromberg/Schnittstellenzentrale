using Microsoft.Extensions.Localization;
using Moq;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Resources;

namespace Schnittstellenzentrale.Tests.Helpers;

/// <summary>Gemeinsame Mock-Fabrikmethoden für Unit-Tests.</summary>
public static class TestMockFactory
{
    /// <summary>Erstellt einen leeren <see cref="IActivityLogService"/>-Mock.</summary>
    /// <returns>Der konfigurierte Mock.</returns>
    public static Mock<IActivityLogService> CreateActivityLogServiceMock()
    {
        return new Mock<IActivityLogService>();
    }

    /// <summary>Erstellt eine <see cref="SystemEnvironment"/>-Testinstanz mit den angegebenen Werten.</summary>
    /// <param name="id">Die ID der Testumgebung.</param>
    /// <param name="name">Der Name der Testumgebung.</param>
    /// <returns>Die erstellte Testumgebung.</returns>
    public static SystemEnvironment CreateEnv(int id, string name) => new()
    {
        Id = id,
        Name = name,
        Mode = StorageMode.Team,
        Variables = []
    };

    /// <summary>Erstellt einen <see cref="IStringLocalizer{SharedResources}"/>, der jeden Schlüssel unverändert als Wert zurückgibt.</summary>
    /// <returns>Der Fake-Localizer.</returns>
    public static IStringLocalizer<SharedResources> CreateFakeLocalizer()
    {
        var mock = new Mock<IStringLocalizer<SharedResources>>();
        mock.Setup(l => l[It.IsAny<string>()])
            .Returns<string>(key => new LocalizedString(key, key));
        mock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns<string, object[]>((key, args) => new LocalizedString(key, string.Format(key, args)));
        return mock.Object;
    }

    /// <summary>Erstellt gemeinsame Mocks und Testdaten für Coverage-orientierte UI-Fehlerszenarien.</summary>
    /// <returns>Die vorbereitete <see cref="CoverageTestFactory"/>.</returns>
    public static CoverageTestFactory CreateCoverageScenarioDependencies()
    {
        var applicationApiClientMock = new Mock<IApplicationApiClient>();
        applicationApiClientMock
            .Setup(c => c.GetEndpointsAsync(It.IsAny<int>(), It.IsAny<int?>()))
            .ReturnsAsync([]);

        var applicationServiceMock = new Mock<IApplicationService>();

        var environmentRepositoryMock = new Mock<ISystemEnvironmentRepository>();
        environmentRepositoryMock
            .Setup(r => r.GetEnvironmentsAsync(It.IsAny<StorageMode>(), It.IsAny<string?>()))
            .ReturnsAsync([]);

        var activeEnvironmentServiceMock = new Mock<IActiveEnvironmentService>();
        activeEnvironmentServiceMock
            .Setup(s => s.ActiveEnvironment)
            .Returns((SystemEnvironment?)null);

        var restApplication = new Application
        {
            Id = 100,
            Name = "Coverage REST",
            BaseUrl = "https://example.test",
            InterfaceType = InterfaceType.Rest,
            InterfaceUrl = "https://example.test/swagger/v1/swagger.json"
        };

        var odataApplication = new Application
        {
            Id = 101,
            Name = "Coverage OData",
            BaseUrl = "https://example.test",
            InterfaceType = InterfaceType.OData,
            InterfaceUrl = "https://example.test/$metadata"
        };

        var selectedEnvironment = CreateEnv(12, "Coverage-Environment");

        return new CoverageTestFactory(
            applicationApiClientMock,
            applicationServiceMock,
            environmentRepositoryMock,
            activeEnvironmentServiceMock,
            restApplication,
            odataApplication,
            selectedEnvironment);
    }
}
