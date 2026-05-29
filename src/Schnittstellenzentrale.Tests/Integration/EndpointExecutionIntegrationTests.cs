using Moq;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Helpers;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Services;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Integration;

/// <summary>
/// Integrationstest: echtes Token, echter Test-Server, realer <see cref="EndpointExecutionService"/>.
/// </summary>
public class EndpointExecutionIntegrationTests : IAsyncLifetime
{
    private ControllerTestFactory _factory = null!;
    private HttpClient _apiClient = null!;

    /// <inheritdoc/>
    public Task InitializeAsync()
    {
        _factory = new ControllerTestFactory();
        _apiClient = _factory.CreateClient();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DisposeAsync()
    {
        _apiClient.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Legt einen Endpunkt auf die eigene API an, versieht ihn mit einem gültigen Bearer-Token
    /// und prüft, dass die Ausführung ein positives Ergebnis liefert.
    /// </summary>
    [Fact]
    public async Task ExecuteEndpoint_OwnApiWithBearerToken_ReturnsSuccess()
    {
        // Arrange — echtes Token vom Test-Server holen
        var token = await _factory.ObtainTokenAsync(_apiClient);

        const int applicationId = 99;
        var credentialTarget = CredentialTargetHelper.Build(applicationId, AuthenticationType.BearerToken);

        var application = new Application
        {
            Id = applicationId,
            Name = "Eigene API (Test)",
            BaseUrl = "http://localhost"
        };

        var endpoint = new Core.Models.Endpoint
        {
            Id = 1,
            ApplicationId = applicationId,
            Application = application,
            Name = "Anwendungsgruppen abrufen",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/api/application-groups",
            AuthenticationType = AuthenticationType.BearerToken,
            BodyMode = BodyMode.None,
            Headers =
            [
                new EndpointHeader { Key = "X-Storage-Mode", Value = "Team" },
                new EndpointHeader { Key = "X-Owner",        Value = "test" }
            ],
            QueryParameters = []
        };

        var credentialServiceMock = new Mock<ICredentialService>();
        credentialServiceMock
            .Setup(c => c.GetPassword(credentialTarget))
            .Returns(token);

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(_factory.Server.CreateHandler()));

        var activeEnvMock = new Mock<IActiveEnvironmentService>();
        activeEnvMock.Setup(s => s.ActiveVariables).Returns(new Dictionary<string, string>());

        var scriptRunnerMock = new Mock<IEndpointScriptRunner>();
        scriptRunnerMock.Setup(r => r.ExecuteAsync(It.IsAny<string>(), It.IsAny<ScriptContext>()))
            .ReturnsAsync(new ScriptExecutionResult { Success = true });

        var endpointRepoMock = new Mock<IEndpointRepository>();
        endpointRepoMock.Setup(r => r.GetEndpointByNameAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Core.Models.Endpoint>());

        var historyServiceMock = new Mock<IHistoryService>();
        historyServiceMock.Setup(h => h.AddEntryAsync(It.IsAny<Core.Models.EndpointCallHistoryEntry>()))
            .Returns(Task.CompletedTask);

        var executionService = new EndpointExecutionService(
            httpClientFactoryMock.Object,
            credentialServiceMock.Object,
            activeEnvMock.Object,
            scriptRunnerMock.Object,
            endpointRepoMock.Object,
            new Mock<ISystemEnvironmentRepository>().Object,
            new Mock<ISignalRNotificationService>().Object,
            new Mock<IActivityLogService>().Object,
            historyServiceMock.Object);

        // Act
        var result = await executionService.ExecuteAsync(endpoint);

        // Assert
        Assert.True(result.Success,
            $"HTTP {result.StatusCode}: {result.ErrorMessage ?? result.ResponseBody}");
        Assert.Equal(200, result.StatusCode);
    }
}
