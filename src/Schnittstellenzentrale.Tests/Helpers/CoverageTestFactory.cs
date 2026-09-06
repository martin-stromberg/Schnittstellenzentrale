using Moq;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Tests.Helpers;

/// <summary>Zentralisiert gemeinsame Mocks und Fixture-Daten für Coverage-orientierte Testszenarien.</summary>
public sealed class CoverageTestFactory
{
    /// <summary>Initialisiert die Instanz mit den vorbereiteten Mocks und Testdaten.</summary>
    /// <param name="applicationApiClientMock">Mock für <see cref="IApplicationApiClient"/>.</param>
    /// <param name="applicationServiceMock">Mock für <see cref="IApplicationService"/>.</param>
    /// <param name="environmentRepositoryMock">Mock für <see cref="ISystemEnvironmentRepository"/>.</param>
    /// <param name="activeEnvironmentServiceMock">Mock für <see cref="IActiveEnvironmentService"/>.</param>
    /// <param name="restApplication">REST-Anwendung für Testszenarien.</param>
    /// <param name="odataApplication">OData-Anwendung für Testszenarien.</param>
    /// <param name="selectedEnvironment">Ausgewählte Umgebung für Testszenarien.</param>
    public CoverageTestFactory(
        Mock<IApplicationApiClient> applicationApiClientMock,
        Mock<IApplicationService> applicationServiceMock,
        Mock<ISystemEnvironmentRepository> environmentRepositoryMock,
        Mock<IActiveEnvironmentService> activeEnvironmentServiceMock,
        Application restApplication,
        Application odataApplication,
        SystemEnvironment selectedEnvironment)
    {
        ApplicationApiClientMock = applicationApiClientMock;
        ApplicationServiceMock = applicationServiceMock;
        EnvironmentRepositoryMock = environmentRepositoryMock;
        ActiveEnvironmentServiceMock = activeEnvironmentServiceMock;
        RestApplication = restApplication;
        ODataApplication = odataApplication;
        SelectedEnvironment = selectedEnvironment;
    }

    /// <summary>Mock für <see cref="IApplicationApiClient"/>.</summary>
    public Mock<IApplicationApiClient> ApplicationApiClientMock { get; }

    /// <summary>Mock für <see cref="IApplicationService"/>.</summary>
    public Mock<IApplicationService> ApplicationServiceMock { get; }

    /// <summary>Mock für <see cref="ISystemEnvironmentRepository"/>.</summary>
    public Mock<ISystemEnvironmentRepository> EnvironmentRepositoryMock { get; }

    /// <summary>Mock für <see cref="IActiveEnvironmentService"/>.</summary>
    public Mock<IActiveEnvironmentService> ActiveEnvironmentServiceMock { get; }

    /// <summary>REST-Anwendung für Testszenarien.</summary>
    public Application RestApplication { get; }

    /// <summary>OData-Anwendung für Testszenarien.</summary>
    public Application ODataApplication { get; }

    /// <summary>Ausgewählte Umgebung für Testszenarien.</summary>
    public SystemEnvironment SelectedEnvironment { get; }
}
