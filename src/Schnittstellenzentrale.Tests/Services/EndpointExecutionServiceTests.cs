using System.Net;
using System.Net.Http.Headers;
using Moq;
using Moq.Protected;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Services;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>EndpointExecutionServiceTests</summary>
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

    private static Core.Models.Endpoint CreateEndpoint(AuthenticationType authType, string relPath, EndpointQueryParameter[]? queryParameters = null) => new()
    {
        Id = 1,
        Name = "Test",
        Method = Core.Enums.HttpMethod.GET,
        RelativePath = relPath,
        AuthenticationType = authType,
        ApplicationId = 1,
        Application = CreateApp(),
        Headers = [],
        QueryParameters = queryParameters ?? []
    };

    private static (EndpointExecutionService, Mock<HttpMessageHandler>) CreateService(
        Mock<IHealthCheckService> healthCheckMock,
        Mock<ICredentialService> credentialMock,
        HttpStatusCode responseCode = HttpStatusCode.OK,
        string body = "{}")
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(responseCode) { Content = new StringContent(body) });

        var httpClient = new HttpClient(handlerMock.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new EndpointExecutionService(factoryMock.Object, healthCheckMock.Object, credentialMock.Object);
        return (service, handlerMock);
    }

    /// <summary>Execute_WithAuthTypeNone_SendsRequestWithoutCredentials</summary>
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

    /// <summary>Execute_WithAuthTypeBasic_SendsBasicAuthHeader</summary>
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

    /// <summary>Execute_WithNegotiateAuthType_UsesNegotiateHandler</summary>
    [Theory]
    [InlineData(AuthenticationType.Negotiate)]
    [InlineData(AuthenticationType.NegotiateWithImpersonation)]
    public async Task Execute_WithNegotiateAuthType_UsesNegotiateHandler(AuthenticationType authType)
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
        var endpoint = CreateEndpoint(authType);

        await service.ExecuteAsync(endpoint);

        factoryMock.Verify(f => f.CreateClient("negotiate"), Times.Once());
    }

    /// <summary>Execute_WithAuthTypeBearerToken_SendsBearerHeader</summary>
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

    /// <summary>Execute_SetsResponseHeaders</summary>
    [Fact]
    public async Task Execute_SetsResponseHeaders()
    {
        var healthMock = new Mock<IHealthCheckService>();
        var credMock = new Mock<ICredentialService>();
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}")
        };
        response.Headers.Add("X-Custom-Header", "headerValue");
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new EndpointExecutionService(factoryMock.Object, healthMock.Object, credMock.Object);
        var endpoint = CreateEndpoint(AuthenticationType.None);

        var result = await service.ExecuteAsync(endpoint);

        Assert.NotNull(result.ResponseHeaders);
        Assert.True(result.ResponseHeaders!.ContainsKey("X-Custom-Header"));
        Assert.Equal("headerValue", result.ResponseHeaders["X-Custom-Header"]);
    }

    /// <summary>Execute_SetsDurationMs</summary>
    [Fact]
    public async Task Execute_SetsDurationMs()
    {
        var healthMock = new Mock<IHealthCheckService>();
        var credMock = new Mock<ICredentialService>();
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(async () =>
            {
                await Task.Delay(20);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new EndpointExecutionService(factoryMock.Object, healthMock.Object, credMock.Object);
        var endpoint = CreateEndpoint(AuthenticationType.None);

        var result = await service.ExecuteAsync(endpoint);

        Assert.NotNull(result.DurationMs);
        Assert.True(result.DurationMs > 0);
    }

    /// <summary>Execute_SetsResponseSizeBytes</summary>
    [Fact]
    public async Task Execute_SetsResponseSizeBytes()
    {
        var healthMock = new Mock<IHealthCheckService>();
        var credMock = new Mock<ICredentialService>();
        const string body = "{\"value\":42}";
        var (service, _) = CreateService(healthMock, credMock, body: body);
        var endpoint = CreateEndpoint(AuthenticationType.None);

        var result = await service.ExecuteAsync(endpoint);

        Assert.NotNull(result.ResponseSizeBytes);
        Assert.Equal(System.Text.Encoding.UTF8.GetByteCount(body), result.ResponseSizeBytes);
    }

    /// <summary>Execute_OnConnectionError_DoesNotCallHealthCheck</summary>
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

    /// <summary>Platzhalter im RelativePath werden durch den zugehörigen QueryParameter-Wert ersetzt; fehlende Werte ergeben leere Strings.</summary>
    [Fact]
    public async Task BuildRequest_ErsetztPfadPlatzhalterDurchGespeicherteWerte()
    {
        var healthMock = new Mock<IHealthCheckService>();
        var credMock = new Mock<ICredentialService>();

        Uri? sentUri = null;
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => sentUri = req.RequestUri)
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("{}") });

        var httpClient = new HttpClient(handlerMock.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new EndpointExecutionService(factoryMock.Object, healthMock.Object, credMock.Object);
        var endpoint = CreateEndpoint(AuthenticationType.None, "/api/{id}/items",
        [
            new EndpointQueryParameter { Key = "id", Value = "42" }
        ]);

        await service.ExecuteAsync(endpoint);

        Assert.NotNull(sentUri);
        Assert.Contains("/api/42/items", sentUri!.PathAndQuery);
        Assert.DoesNotContain("{id}", sentUri.PathAndQuery);
    }

    /// <summary>Parameter ohne Platzhalter-Treffer im Pfad landen im Query-String; Platzhalter-Werte werden nicht erneut angehängt.</summary>
    [Fact]
    public async Task BuildRequest_HaengtNurNichtPlatzhalterParameterAlsQueryStringAn()
    {
        var healthMock = new Mock<IHealthCheckService>();
        var credMock = new Mock<ICredentialService>();

        Uri? sentUri = null;
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => sentUri = req.RequestUri)
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("{}") });

        var httpClient = new HttpClient(handlerMock.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new EndpointExecutionService(factoryMock.Object, healthMock.Object, credMock.Object);
        var endpoint = CreateEndpoint(AuthenticationType.None, "/api/{id}/items",
        [
            new EndpointQueryParameter { Key = "id", Value = "42" },
            new EndpointQueryParameter { Key = "filter", Value = "active" }
        ]);

        await service.ExecuteAsync(endpoint);

        Assert.NotNull(sentUri);
        var fullUrl = sentUri!.PathAndQuery;
        Assert.Contains("/api/42/items", fullUrl);
        Assert.Contains("filter=active", fullUrl);
        Assert.DoesNotContain("id=42", fullUrl);
    }
}
