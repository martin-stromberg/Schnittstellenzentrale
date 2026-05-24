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

    private static Mock<IActiveEnvironmentService> CreateEmptyActiveEnvironmentMock()
    {
        var mock = new Mock<IActiveEnvironmentService>();
        mock.Setup(s => s.ActiveEnvironment).Returns((Core.Models.SystemEnvironment?)null);
        mock.Setup(s => s.ActiveVariables).Returns(new Dictionary<string, string>());
        return mock;
    }

    private static (EndpointExecutionService, Mock<HttpMessageHandler>) CreateService(
        Mock<IHealthCheckService> healthCheckMock,
        Mock<ICredentialService> credentialMock,
        HttpStatusCode responseCode = HttpStatusCode.OK,
        string body = "{}",
        Mock<IActiveEnvironmentService>? activeEnvironmentMock = null,
        HttpResponseMessage? customResponse = null)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(customResponse ?? new HttpResponseMessage(responseCode) { Content = new StringContent(body) });

        var httpClient = new HttpClient(handlerMock.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var envMock = activeEnvironmentMock ?? CreateEmptyActiveEnvironmentMock();
        var service = new EndpointExecutionService(factoryMock.Object, healthCheckMock.Object, credentialMock.Object, envMock.Object);
        return (service, handlerMock);
    }

    private static (EndpointExecutionService service, Func<Uri?> getSentUri) CreateServiceCapturingUri(
        Mock<IActiveEnvironmentService>? activeEnvironmentMock = null)
    {
        var healthMock = new Mock<IHealthCheckService>();
        var credMock = new Mock<ICredentialService>();
        Uri? sentUri = null;
        var (service, handlerMock) = CreateService(healthMock, credMock, activeEnvironmentMock: activeEnvironmentMock);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => sentUri = req.RequestUri)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });
        return (service, () => sentUri);
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
        var envMock = CreateEmptyActiveEnvironmentMock();
        var service = new EndpointExecutionService(factoryMock.Object, healthMock.Object, credMock.Object, envMock.Object);
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
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
        response.Headers.Add("X-Custom-Header", "headerValue");
        var (service, _) = CreateService(healthMock, credMock, customResponse: response);
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
        var (service, handlerMock) = CreateService(healthMock, credMock);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(async () =>
            {
                await Task.Delay(20);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
            });
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

        var envMock = CreateEmptyActiveEnvironmentMock();
        var service = new EndpointExecutionService(factoryMock.Object, healthMock.Object, credMock.Object, envMock.Object);
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
        var (service, getSentUri) = CreateServiceCapturingUri();
        var endpoint = CreateEndpoint(AuthenticationType.None, "/api/{id}/items",
        [
            new EndpointQueryParameter { Key = "id", Value = "42" }
        ]);

        await service.ExecuteAsync(endpoint);

        var sentUri = getSentUri();
        Assert.NotNull(sentUri);
        Assert.Contains("/api/42/items", sentUri!.PathAndQuery);
        Assert.DoesNotContain("{id}", sentUri.PathAndQuery);
    }

    /// <summary>Parameter ohne Platzhalter-Treffer im Pfad landen im Query-String; Platzhalter-Werte werden nicht erneut angehängt.</summary>
    [Fact]
    public async Task BuildRequest_HaengtNurNichtPlatzhalterParameterAlsQueryStringAn()
    {
        var (service, getSentUri) = CreateServiceCapturingUri();
        var endpoint = CreateEndpoint(AuthenticationType.None, "/api/{id}/items",
        [
            new EndpointQueryParameter { Key = "id", Value = "42" },
            new EndpointQueryParameter { Key = "filter", Value = "active" }
        ]);

        await service.ExecuteAsync(endpoint);

        var sentUri = getSentUri();
        Assert.NotNull(sentUri);
        var fullUrl = sentUri!.PathAndQuery;
        Assert.Contains("/api/42/items", fullUrl);
        Assert.Contains("filter=active", fullUrl);
        Assert.DoesNotContain("id=42", fullUrl);
    }

    private static Mock<IActiveEnvironmentService> CreateActiveEnvironmentMock(Dictionary<string, string> variables)
    {
        var mock = new Mock<IActiveEnvironmentService>();
        mock.Setup(s => s.ActiveEnvironment).Returns(new Core.Models.SystemEnvironment { Name = "Test" });
        mock.Setup(s => s.ActiveVariables).Returns(variables);
        return mock;
    }

    private static Core.Models.Endpoint CreateEndpointWithHeaders(
        string relPath,
        EndpointHeader[] headers,
        EndpointQueryParameter[]? queryParameters = null) => new()
    {
        Id = 1,
        Name = "Test",
        Method = Core.Enums.HttpMethod.GET,
        RelativePath = relPath,
        AuthenticationType = AuthenticationType.None,
        ApplicationId = 1,
        Application = CreateApp(),
        Headers = headers,
        QueryParameters = queryParameters ?? []
    };

    private static Core.Models.Endpoint CreateEndpointWithBody(string relPath, string body) => new()
    {
        Id = 1,
        Name = "Test",
        Method = Core.Enums.HttpMethod.POST,
        RelativePath = relPath,
        Body = body,
        AuthenticationType = AuthenticationType.None,
        ApplicationId = 1,
        Application = CreateApp(),
        Headers = [],
        QueryParameters = []
    };

    /// <summary>BuildRequest_ResolvesDoubleBracePlaceholders</summary>
    [Fact]
    public async Task BuildRequest_ResolvesDoubleBracePlaceholders()
    {
        var envMock = CreateActiveEnvironmentMock(new Dictionary<string, string> { ["env"] = "prod" });
        var (service, getSentUri) = CreateServiceCapturingUri(envMock);
        var endpoint = CreateEndpoint(AuthenticationType.None, "/api/{{env}}/items");

        await service.ExecuteAsync(endpoint);

        var sentUri = getSentUri();
        Assert.NotNull(sentUri);
        Assert.Contains("/api/prod/items", sentUri!.PathAndQuery);
    }

    /// <summary>BuildRequest_ResolvesSingleBracePlaceholdersFromQueryParameters</summary>
    [Fact]
    public async Task BuildRequest_ResolvesSingleBracePlaceholdersFromQueryParameters()
    {
        var (service, getSentUri) = CreateServiceCapturingUri();
        var endpoint = CreateEndpoint(AuthenticationType.None, "/api/{id}/items",
        [
            new EndpointQueryParameter { Key = "id", Value = "42" }
        ]);

        await service.ExecuteAsync(endpoint);

        var sentUri = getSentUri();
        Assert.NotNull(sentUri);
        Assert.Contains("/api/42/items", sentUri!.PathAndQuery);
    }

    /// <summary>BuildRequest_MissingVariable_ReplacesWithEmptyString</summary>
    [Fact]
    public async Task BuildRequest_MissingVariable_ReplacesWithEmptyString()
    {
        var envMock = CreateActiveEnvironmentMock(new Dictionary<string, string>());
        var (service, getSentUri) = CreateServiceCapturingUri(envMock);
        var endpoint = CreateEndpoint(AuthenticationType.None, "/api/{{missing}}/items");

        await service.ExecuteAsync(endpoint);

        var sentUri = getSentUri();
        Assert.NotNull(sentUri);
        Assert.Contains("/api//items", sentUri!.PathAndQuery);
    }

    /// <summary>BuildRequest_NoActiveEnvironment_ReplacesAllDoubleBracePlaceholdersWithEmptyString</summary>
    [Fact]
    public async Task BuildRequest_NoActiveEnvironment_ReplacesAllDoubleBracePlaceholdersWithEmptyString()
    {
        var (service, getSentUri) = CreateServiceCapturingUri();
        var endpoint = CreateEndpoint(AuthenticationType.None, "/api/{{host}}/{{version}}/items");

        await service.ExecuteAsync(endpoint);

        var sentUri = getSentUri();
        Assert.NotNull(sentUri);
        Assert.Contains("/api///items", sentUri!.PathAndQuery);
        Assert.DoesNotContain("{{", sentUri!.PathAndQuery);
    }

    /// <summary>BuildRequest_ResolvesPlaceholdersInBaseUrl</summary>
    [Fact]
    public async Task BuildRequest_ResolvesPlaceholdersInBaseUrl()
    {
        var envMock = CreateActiveEnvironmentMock(new Dictionary<string, string> { ["host"] = "https://example.com" });
        var (service, getSentUri) = CreateServiceCapturingUri(envMock);

        var endpoint = new Core.Models.Endpoint
        {
            Id = 1,
            Name = "Test",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/api/test",
            AuthenticationType = AuthenticationType.None,
            ApplicationId = 1,
            Application = new Core.Models.Application { Id = 1, Name = "TestApp", BaseUrl = "{{host}}" },
            Headers = [],
            QueryParameters = []
        };

        await service.ExecuteAsync(endpoint);

        var sentUri = getSentUri();
        Assert.NotNull(sentUri);
        Assert.StartsWith("https://example.com", sentUri!.ToString());
    }

    /// <summary>BuildRequest_ResolvesPlaceholdersInRelativePath</summary>
    [Fact]
    public async Task BuildRequest_ResolvesPlaceholdersInRelativePath()
    {
        var envMock = CreateActiveEnvironmentMock(new Dictionary<string, string> { ["version"] = "v2" });
        var (service, getSentUri) = CreateServiceCapturingUri(envMock);
        var endpoint = CreateEndpoint(AuthenticationType.None, "/api/{{version}}/items");

        await service.ExecuteAsync(endpoint);

        var sentUri = getSentUri();
        Assert.NotNull(sentUri);
        Assert.Contains("/api/v2/items", sentUri!.PathAndQuery);
    }

    /// <summary>BuildRequest_ResolvesPlaceholdersInHeaderNamesAndValues</summary>
    [Fact]
    public async Task BuildRequest_ResolvesPlaceholdersInHeaderNamesAndValues()
    {
        var envMock = CreateActiveEnvironmentMock(new Dictionary<string, string>
        {
            ["headerName"] = "X-Custom",
            ["headerValue"] = "myvalue"
        });

        System.Net.Http.HttpRequestMessage? capturedRequest = null;
        var healthMock = new Mock<IHealthCheckService>();
        var credMock = new Mock<ICredentialService>();
        var (service, handlerMock) = CreateService(healthMock, credMock, activeEnvironmentMock: envMock);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });

        var endpoint = CreateEndpointWithHeaders("/api/test",
        [
            new EndpointHeader { Key = "{{headerName}}", Value = "{{headerValue}}" }
        ]);

        await service.ExecuteAsync(endpoint);

        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest!.Headers.Contains("X-Custom"));
        Assert.Equal("myvalue", capturedRequest.Headers.GetValues("X-Custom").First());
    }

    /// <summary>BuildRequest_ResolvesPlaceholdersInQueryParameterNamesAndValues</summary>
    [Fact]
    public async Task BuildRequest_ResolvesPlaceholdersInQueryParameterNamesAndValues()
    {
        var envMock = CreateActiveEnvironmentMock(new Dictionary<string, string>
        {
            ["paramName"] = "filter",
            ["paramValue"] = "active"
        });
        var (service, getSentUri) = CreateServiceCapturingUri(envMock);
        var endpoint = CreateEndpoint(AuthenticationType.None, "/api/items",
        [
            new EndpointQueryParameter { Key = "{{paramName}}", Value = "{{paramValue}}" }
        ]);

        await service.ExecuteAsync(endpoint);

        var sentUri = getSentUri();
        Assert.NotNull(sentUri);
        Assert.Contains("filter=active", sentUri!.Query);
    }

    /// <summary>BuildRequest_ResolvesPlaceholdersInBearerToken</summary>
    [Fact]
    public async Task BuildRequest_ResolvesPlaceholdersInBearerToken()
    {
        var envMock = CreateActiveEnvironmentMock(new Dictionary<string, string> { ["token"] = "secret123" });

        System.Net.Http.HttpRequestMessage? capturedRequest = null;
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });

        var httpClient = new HttpClient(handlerMock.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var credMock = new Mock<ICredentialService>();
        credMock.Setup(c => c.GetPassword(It.IsAny<string>())).Returns("{{token}}");

        var service = new EndpointExecutionService(
            factoryMock.Object,
            new Mock<IHealthCheckService>().Object,
            credMock.Object,
            envMock.Object);

        var endpoint = CreateEndpoint(AuthenticationType.BearerToken);

        await service.ExecuteAsync(endpoint);

        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest!.Headers.Authorization);
        Assert.Equal("Bearer", capturedRequest.Headers.Authorization!.Scheme);
        Assert.Equal("secret123", capturedRequest.Headers.Authorization.Parameter);
    }

    /// <summary>BuildRequest_ResolvesPlaceholdersInBody</summary>
    [Fact]
    public async Task BuildRequest_ResolvesPlaceholdersInBody()
    {
        var envMock = CreateActiveEnvironmentMock(new Dictionary<string, string> { ["value"] = "42" });

        string? capturedBody = null;
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>(async (req, _) =>
            {
                capturedBody = req.Content != null ? await req.Content.ReadAsStringAsync() : null;
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new EndpointExecutionService(
            factoryMock.Object,
            new Mock<IHealthCheckService>().Object,
            new Mock<ICredentialService>().Object,
            envMock.Object);

        var endpoint = CreateEndpointWithBody("/api/items", "{\"count\": {{value}}}");

        await service.ExecuteAsync(endpoint);

        Assert.NotNull(capturedBody);
        Assert.Equal("{\"count\": 42}", capturedBody);
    }
}
