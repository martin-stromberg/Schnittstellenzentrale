using System.Net;
using System.Net.Http.Headers;
using Moq;
using Moq.Protected;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Services;

namespace Schnittstellenzentrale.Tests.Services;

public class EndpointExecutionServiceTests
{
    private static Core.Models.Application CreateApp() => new()
    {
        Id = 1,
        Name = "TestApp",
        BaseUrl = "http://localhost:5000"
    };

    private static Core.Models.Endpoint CreateEndpoint(AuthenticationType authType) => new()
    {
        Id = 1,
        Name = "Test",
        Method = Core.Enums.HttpMethod.GET,
        RelativePath = "/test",
        AuthenticationType = authType,
        ApplicationId = 1,
        Application = CreateApp(),
        Headers = [],
        QueryParameters = []
    };

    private static (EndpointExecutionService, Mock<HttpMessageHandler>) CreateService(
        Mock<IHealthCheckService> healthCheckMock,
        Mock<ICredentialService> credentialMock,
        HttpStatusCode responseCode = HttpStatusCode.OK)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(responseCode) { Content = new StringContent("{}") });

        var httpClient = new HttpClient(handlerMock.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new EndpointExecutionService(factoryMock.Object, healthCheckMock.Object, credentialMock.Object);
        return (service, handlerMock);
    }

    [Fact]
    public async Task Execute_WithAuthTypeNone_SendsRequestWithoutCredentials()
    {
        var healthMock = new Mock<IHealthCheckService>();
        var credMock = new Mock<ICredentialService>();
        var (service, handlerMock) = CreateService(healthMock, credMock);
        var endpoint = CreateEndpoint(AuthenticationType.None);

        var result = await service.ExecuteAsync(endpoint);

        Assert.True(result.Success);
        handlerMock.Protected().Verify("SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r => r.Headers.Authorization == null),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task Execute_WithAuthTypeBasic_SendsBasicAuthHeader()
    {
        var healthMock = new Mock<IHealthCheckService>();
        var credMock = new Mock<ICredentialService>();
        credMock.Setup(c => c.GetPassword(It.IsAny<string>())).Returns("user:password");
        var (service, handlerMock) = CreateService(healthMock, credMock);
        var endpoint = CreateEndpoint(AuthenticationType.Basic);

        var result = await service.ExecuteAsync(endpoint);

        Assert.True(result.Success);
        handlerMock.Protected().Verify("SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r => r.Headers.Authorization != null
                && r.Headers.Authorization.Scheme == "Basic"),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task Execute_WithAuthTypeNegotiate_UsesNegotiateHandler()
    {
        var healthMock = new Mock<IHealthCheckService>();
        var credMock = new Mock<ICredentialService>();
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerMock.Object));
        var service = new EndpointExecutionService(factoryMock.Object, healthMock.Object, credMock.Object);
        var endpoint = CreateEndpoint(AuthenticationType.Negotiate);

        await service.ExecuteAsync(endpoint);

        factoryMock.Verify(f => f.CreateClient("negotiate"), Times.Once());
    }

    [Fact]
    public async Task Execute_WithAuthTypeBearerToken_SendsBearerHeader()
    {
        var healthMock = new Mock<IHealthCheckService>();
        var credMock = new Mock<ICredentialService>();
        credMock.Setup(c => c.GetPassword(It.IsAny<string>())).Returns("mytoken");
        var (service, handlerMock) = CreateService(healthMock, credMock);
        var endpoint = CreateEndpoint(AuthenticationType.BearerToken);

        var result = await service.ExecuteAsync(endpoint);

        Assert.True(result.Success);
        handlerMock.Protected().Verify("SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r => r.Headers.Authorization != null
                && r.Headers.Authorization.Scheme == "Bearer"
                && r.Headers.Authorization.Parameter == "mytoken"),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task Execute_WithAuthTypeNegotiateWithImpersonation_RunsImpersonated()
    {
        var healthMock = new Mock<IHealthCheckService>();
        var credMock = new Mock<ICredentialService>();
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerMock.Object));
        var service = new EndpointExecutionService(factoryMock.Object, healthMock.Object, credMock.Object);
        var endpoint = CreateEndpoint(AuthenticationType.NegotiateWithImpersonation);

        await service.ExecuteAsync(endpoint);

        factoryMock.Verify(f => f.CreateClient("negotiate"), Times.Once());
    }

    [Fact]
    public async Task Execute_OnConnectionError_DoesNotCallHealthCheck()
    {
        var healthMock = new Mock<IHealthCheckService>();
        var credMock = new Mock<ICredentialService>();

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var httpClient = new HttpClient(handlerMock.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new EndpointExecutionService(factoryMock.Object, healthMock.Object, credMock.Object);
        var endpoint = CreateEndpoint(AuthenticationType.None);

        var result = await service.ExecuteAsync(endpoint);

        // Der Service triggert keinen Health-Check mehr (Verantwortung liegt in der UI).
        // Ein Verbindungsfehler wird als Ergebnis ohne StatusCode zurückgegeben.
        Assert.False(result.Success);
        Assert.Null(result.StatusCode);
        healthMock.Verify(h => h.CheckAsync(It.IsAny<Core.Models.Application>()), Times.Never);
    }
}
