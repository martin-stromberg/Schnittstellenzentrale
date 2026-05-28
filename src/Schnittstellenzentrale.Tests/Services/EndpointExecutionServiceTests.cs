using System.Net;
using System.Net.Http.Headers;
using Moq;
using Moq.Protected;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Services;
using Schnittstellenzentrale.Tests.Helpers;
using ActivityLogCategory = Schnittstellenzentrale.Core.Enums.ActivityLogCategory;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>EndpointExecutionServiceTests</summary>
public class EndpointExecutionServiceTests
{
    private readonly Mock<IHealthCheckService> _healthMock = new();
    private readonly Mock<ICredentialService> _credMock = new();

    private static Core.Models.Application CreateApp() => new()
    {
        Id = 1,
        Name = "TestApp",
        BaseUrl = "http://localhost:5000"
    };

    private static Core.Models.Endpoint CreateEndpoint(
        AuthenticationType authType = AuthenticationType.None,
        string relPath = "/test",
        EndpointQueryParameter[]? queryParameters = null,
        EndpointHeader[]? headers = null,
        string? body = null,
        Core.Enums.HttpMethod method = Core.Enums.HttpMethod.GET,
        string? preRequestScript = null,
        string? postRequestScript = null,
        string name = "Test",
        int id = 1) => new()
    {
        Id = id,
        Name = name,
        Method = method,
        RelativePath = relPath,
        AuthenticationType = authType,
        ApplicationId = 1,
        Application = CreateApp(),
        Headers = headers ?? [],
        QueryParameters = queryParameters ?? [],
        Body = body,
        PreRequestScript = preRequestScript,
        PostRequestScript = postRequestScript
    };

    private static Mock<IActiveEnvironmentService> CreateEmptyActiveEnvironmentMock()
    {
        var mock = new Mock<IActiveEnvironmentService>();
        mock.Setup(s => s.ActiveEnvironment).Returns((Core.Models.SystemEnvironment?)null);
        mock.Setup(s => s.ActiveVariables).Returns(new Dictionary<string, string>());
        return mock;
    }

    private static Mock<IEndpointScriptRunner> CreateScriptRunnerMock(ScriptExecutionResult result)
    {
        var mock = new Mock<IEndpointScriptRunner>();
        mock.Setup(r => r.ExecuteAsync(It.IsAny<string>(), It.IsAny<ScriptContext>()))
            .ReturnsAsync(result);
        return mock;
    }

    private static Mock<IEndpointRepository> CreateEmptyEndpointRepositoryMock()
    {
        var mock = new Mock<IEndpointRepository>();
        mock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync(new List<Core.Models.Endpoint>());
        mock.Setup(r => r.GetEndpointByNameAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(new List<Core.Models.Endpoint>());
        return mock;
    }

    private static Mock<ISystemEnvironmentRepository> CreateEmptyEnvironmentRepositoryMock()
    {
        var mock = new Mock<ISystemEnvironmentRepository>();
        mock.Setup(r => r.UpdateVariableAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    private static Mock<ISignalRNotificationService> CreateEmptySignalRNotificationServiceMock()
    {
        var mock = new Mock<ISignalRNotificationService>();
        mock.Setup(s => s.NotifyEnvironmentChangedAsync()).Returns(Task.CompletedTask);
        return mock;
    }

    private static (EndpointExecutionService, Mock<HttpMessageHandler>, Mock<IHttpClientFactory>) CreateService(
        Mock<ICredentialService> credentialMock,
        HttpStatusCode responseCode = HttpStatusCode.OK,
        string body = "{}",
        Mock<IActiveEnvironmentService>? activeEnvironmentMock = null,
        HttpResponseMessage? customResponse = null,
        Mock<IEndpointScriptRunner>? scriptRunnerMock = null,
        Mock<IEndpointRepository>? endpointRepositoryMock = null,
        Mock<ISystemEnvironmentRepository>? environmentRepositoryMock = null,
        Mock<ISignalRNotificationService>? signalRNotificationServiceMock = null,
        Mock<IActivityLogService>? activityLogServiceMock = null,
        IEndpointScriptRunner? realScriptRunner = null)
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
        IEndpointScriptRunner scriptRunner = realScriptRunner
            ?? (scriptRunnerMock ?? CreateScriptRunnerMock(new ScriptExecutionResult { Success = true })).Object;
        var repoMock = endpointRepositoryMock ?? CreateEmptyEndpointRepositoryMock();
        var envRepoMock = environmentRepositoryMock ?? CreateEmptyEnvironmentRepositoryMock();
        var signalRMock = signalRNotificationServiceMock ?? CreateEmptySignalRNotificationServiceMock();
        var logMock = activityLogServiceMock ?? TestMockFactory.CreateActivityLogServiceMock();

        var service = new EndpointExecutionService(
            factoryMock.Object,
            credentialMock.Object,
            envMock.Object,
            scriptRunner,
            repoMock.Object,
            envRepoMock.Object,
            signalRMock.Object,
            logMock.Object);
        return (service, handlerMock, factoryMock);
    }

    private (EndpointExecutionService service, Func<Uri?> getSentUri) CreateServiceCapturingUri(
        Mock<IActiveEnvironmentService>? activeEnvironmentMock = null,
        Mock<IEndpointScriptRunner>? scriptRunnerMock = null,
        Mock<IEndpointRepository>? endpointRepositoryMock = null)
    {
        Uri? sentUri = null;
        var (service, handlerMock, _) = CreateService(
            _credMock,
            activeEnvironmentMock: activeEnvironmentMock,
            scriptRunnerMock: scriptRunnerMock,
            endpointRepositoryMock: endpointRepositoryMock);
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
        var (service, handlerMock, _) = CreateService(_credMock);
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
        _credMock.Setup(c => c.GetPassword(It.IsAny<string>())).Returns("user:password");
        var (service, handlerMock, _) = CreateService(_credMock);
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
    [Fact]
    public async Task Execute_WithNegotiateAuthType_UsesNegotiateHandler()
    {
        var (service, handlerMock, factoryMock) = CreateService(_credMock);
        var endpoint = CreateEndpoint(AuthenticationType.Negotiate);

        var result = await service.ExecuteAsync(endpoint);

        Assert.True(result.Success);
        handlerMock.Protected().Verify("SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
        factoryMock.Verify(f => f.CreateClient("negotiate"), Times.Once());
    }

    /// <summary>Execute_WithNegotiateWithImpersonationAuthType_UsesNegotiateHandler</summary>
    [Fact]
    public async Task Execute_WithNegotiateWithImpersonationAuthType_UsesNegotiateHandler()
    {
        var (service, _, _) = CreateService(_credMock);
        var endpoint = CreateEndpoint(AuthenticationType.NegotiateWithImpersonation);

        var result = await service.ExecuteAsync(endpoint);

        Assert.True(result.Success);
    }

    /// <summary>Execute_WithAuthTypeBearerToken_SendsBearerHeader</summary>
    [Fact]
    public async Task Execute_WithAuthTypeBearerToken_SendsBearerHeader()
    {
        _credMock.Setup(c => c.GetPassword(It.IsAny<string>())).Returns("mytoken");
        var (service, handlerMock, _) = CreateService(_credMock);
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
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
        response.Headers.Add("X-Custom-Header", "headerValue");
        var (service, _, _) = CreateService(_credMock, customResponse: response);
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
        var (service, handlerMock, _) = CreateService(_credMock);
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
        const string body = "{\"value\":42}";
        var (service, _, _) = CreateService(_credMock, body: body);
        var endpoint = CreateEndpoint(AuthenticationType.None);

        var result = await service.ExecuteAsync(endpoint);

        Assert.NotNull(result.ResponseSizeBytes);
        Assert.Equal(System.Text.Encoding.UTF8.GetByteCount(body), result.ResponseSizeBytes);
    }

    /// <summary>Execute_OnConnectionError_DoesNotCallHealthCheck</summary>
    [Fact]
    public async Task Execute_OnConnectionError_DoesNotCallHealthCheck()
    {
        var (service, handlerMock, _) = CreateService(_credMock);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));
        var endpoint = CreateEndpoint(AuthenticationType.None);

        var result = await service.ExecuteAsync(endpoint);

        Assert.False(result.Success);
        Assert.Null(result.StatusCode);
        _healthMock.Verify(h => h.CheckAsync(It.IsAny<Core.Models.Application>()), Times.Never);
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
        var (service, handlerMock, _) = CreateService(_credMock, activeEnvironmentMock: envMock);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });

        var endpoint = CreateEndpoint(relPath: "/api/test",
            headers: [new EndpointHeader { Key = "{{headerName}}", Value = "{{headerValue}}" }]);

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
        _credMock.Setup(c => c.GetPassword(It.IsAny<string>())).Returns("{{token}}");

        System.Net.Http.HttpRequestMessage? capturedRequest = null;
        var (service, handlerMock, _) = CreateService(_credMock, activeEnvironmentMock: envMock);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });

        var endpoint = CreateEndpoint(AuthenticationType.BearerToken);

        await service.ExecuteAsync(endpoint);

        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest!.Headers.Authorization);
        Assert.Equal("Bearer", capturedRequest.Headers.Authorization!.Scheme);
        Assert.Equal("secret123", capturedRequest.Headers.Authorization.Parameter);
    }

    /// <summary>BuildRequest_ResolvesDoubleBracePlaceholdersBeforeSingleBrace</summary>
    [Fact]
    public async Task BuildRequest_ResolvesDoubleBracePlaceholdersBeforeSingleBrace()
    {
        var envMock = CreateActiveEnvironmentMock(new Dictionary<string, string> { ["env"] = "prod" });
        var (service, getSentUri) = CreateServiceCapturingUri(envMock);
        var endpoint = CreateEndpoint(AuthenticationType.None, "/api/{{env}}/{id}/items",
        [
            new EndpointQueryParameter { Key = "id", Value = "42" }
        ]);

        await service.ExecuteAsync(endpoint);

        var sentUri = getSentUri();
        Assert.NotNull(sentUri);
        Assert.Contains("/api/prod/42/items", sentUri!.PathAndQuery);
        Assert.DoesNotContain("{{", sentUri.PathAndQuery);
        Assert.DoesNotContain("{id}", sentUri.PathAndQuery);
    }

    /// <summary>BuildRequest_ResolvesPlaceholdersInBody</summary>
    [Fact]
    public async Task BuildRequest_ResolvesPlaceholdersInBody()
    {
        var envMock = CreateActiveEnvironmentMock(new Dictionary<string, string> { ["value"] = "42" });

        string? capturedBody = null;
        var (service, handlerMock, _) = CreateService(_credMock, activeEnvironmentMock: envMock);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>(async (req, _) =>
            {
                capturedBody = req.Content != null ? await req.Content.ReadAsStringAsync() : null;
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
            });

        var endpoint = CreateEndpoint(relPath: "/api/items", body: "{\"count\": {{value}}}");

        await service.ExecuteAsync(endpoint);

        Assert.NotNull(capturedBody);
        Assert.Equal("{\"count\": 42}", capturedBody);
    }

    /// <summary>PreScript_SetsEnvironmentVariable_VariableAvailableInRequest</summary>
    [Fact]
    public async Task PreScript_SetsEnvironmentVariable_VariableAvailableInRequest()
    {
        var variables = new Dictionary<string, string> { ["host"] = "original" };
        var envMock = new Mock<IActiveEnvironmentService>();
        envMock.Setup(s => s.ActiveEnvironment).Returns(new Core.Models.SystemEnvironment { Id = 1, Name = "Test", Variables = [] });
        envMock.Setup(s => s.ActiveVariables).Returns(variables);
        envMock.Setup(s => s.SetActiveEnvironment(It.IsAny<Core.Models.SystemEnvironment?>()))
            .Callback<Core.Models.SystemEnvironment?>(env =>
            {
                if (env != null)
                {
                    var newVars = env.Variables.ToDictionary(v => v.Name, v => v.Value);
                    envMock.Setup(s => s.ActiveVariables).Returns(newVars);
                }
            });

        var scriptMock = new Mock<IEndpointScriptRunner>();
        scriptMock.Setup(r => r.ExecuteAsync(It.IsAny<string>(), It.IsAny<ScriptContext>()))
            .Callback<string, ScriptContext>((_, ctx) =>
                ctx.EnvironmentService.SetActiveEnvironment(new Core.Models.SystemEnvironment
                {
                    Id = 1, Name = "Test",
                    Variables = [new Core.Models.EnvironmentVariable { Name = "host", Value = "changed" }]
                }))
            .ReturnsAsync(new ScriptExecutionResult { Success = true });

        Uri? sentUri = null;
        var (service, handlerMock, _) = CreateService(_credMock, activeEnvironmentMock: envMock, scriptRunnerMock: scriptMock);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => sentUri = req.RequestUri)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });

        var endpoint = CreateEndpoint(relPath: "/{{host}}/test",
            preRequestScript: "sz.environment.set('host', 'changed');");

        await service.ExecuteAsync(endpoint);

        Assert.NotNull(sentUri);
        Assert.Contains("/changed/test", sentUri!.PathAndQuery);
        Assert.DoesNotContain("original", sentUri.PathAndQuery);
        scriptMock.Verify(r => r.ExecuteAsync(endpoint.PreRequestScript, It.IsAny<ScriptContext>()), Times.Once);
    }

    /// <summary>PreScript_Fehler_BlockiertHttpRequest_FehlerMeldungImErgebnis</summary>
    [Fact]
    public async Task PreScript_Fehler_BlockiertHttpRequest_FehlerMeldungImErgebnis()
    {
        var scriptMock = new Mock<IEndpointScriptRunner>();
        scriptMock.Setup(r => r.ExecuteAsync(It.IsAny<string>(), It.IsAny<ScriptContext>()))
            .ReturnsAsync(new ScriptExecutionResult { Success = false, ErrorMessage = "Syntaxfehler" });

        var (service, handlerMock, _) = CreateService(_credMock, scriptRunnerMock: scriptMock);

        var endpoint = CreateEndpoint(preRequestScript: "invalid@@");

        var result = await service.ExecuteAsync(endpoint);

        Assert.False(result.Success);
        Assert.Equal("Syntaxfehler", result.ErrorMessage);
        handlerMock.Protected().Verify("SendAsync", Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>PostScript_LiestResponseBody_SetzUmgebungsvariable</summary>
    [Fact]
    public async Task PostScript_LiestResponseBody_SetzUmgebungsvariable()
    {
        var scriptMock = new Mock<IEndpointScriptRunner>();
        scriptMock.Setup(r => r.ExecuteAsync(It.IsAny<string>(), It.IsAny<ScriptContext>()))
            .ReturnsAsync(new ScriptExecutionResult { Success = true });

        var (service, _, _) = CreateService(_credMock, body: """{"token":"abc"}""", scriptRunnerMock: scriptMock);

        var endpoint = CreateEndpoint(postRequestScript: "sz.environment.set('token', sz.response.body.asJson().token);");

        var result = await service.ExecuteAsync(endpoint);

        Assert.True(result.Success);
        scriptMock.Verify(r => r.ExecuteAsync(
            endpoint.PostRequestScript,
            It.Is<ScriptContext>(ctx => ctx.Response != null && ctx.Response.Body!.Contains("token"))),
            Times.Once);
    }

    /// <summary>PostScript_Fehler_HttpErgebnisVorhanden_FehlerMeldungAngehaengt</summary>
    [Fact]
    public async Task PostScript_Fehler_HttpErgebnisVorhanden_FehlerMeldungAngehaengt()
    {
        var scriptMock = new Mock<IEndpointScriptRunner>();
        scriptMock.Setup(r => r.ExecuteAsync(It.IsAny<string>(), It.IsAny<ScriptContext>()))
            .ReturnsAsync(new ScriptExecutionResult { Success = false, ErrorMessage = "Post-Skript-Fehler" });

        var (service, _, _) = CreateService(_credMock, body: "{}", scriptRunnerMock: scriptMock);

        var endpoint = CreateEndpoint(postRequestScript: "invalid@@");

        var result = await service.ExecuteAsync(endpoint);

        Assert.NotNull(result.StatusCode);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("Post-Skript-Fehler", result.ErrorMessage!);
    }

    /// <summary>SzExecute_LoesteAusfuehrungDesZweitenEndpunktsAus</summary>
    [Fact]
    public async Task SzExecute_LoesteAusfuehrungDesZweitenEndpunktsAus()
    {
        var secondEndpoint = CreateEndpoint(relPath: "/second", name: "ZweiterEndpunkt", id: 2);

        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointByNameAsync(1, "ZweiterEndpunkt"))
            .ReturnsAsync(new List<Core.Models.Endpoint> { secondEndpoint });

        var scriptMock = new Mock<IEndpointScriptRunner>();
        scriptMock.Setup(r => r.ExecuteAsync(It.IsAny<string>(), It.IsAny<ScriptContext>()))
            .ReturnsAsync(new ScriptExecutionResult { Success = true });

        var (service, handlerMock, _) = CreateService(_credMock, scriptRunnerMock: scriptMock, endpointRepositoryMock: repoMock);

        ScriptContext? capturedContext = null;
        scriptMock.Setup(r => r.ExecuteAsync("sz.execute('ZweiterEndpunkt');", It.IsAny<ScriptContext>()))
            .Callback<string, ScriptContext>((_, ctx) => capturedContext = ctx)
            .ReturnsAsync(new ScriptExecutionResult { Success = true });

        var endpoint = CreateEndpoint(relPath: "/first", name: "ErsterEndpunkt",
            preRequestScript: "sz.execute('ZweiterEndpunkt');");

        var result = await service.ExecuteAsync(endpoint);

        Assert.NotNull(capturedContext);
    }

    /// <summary>SzExecute_RekursionsschutzGreiftBeimDrittenAufruf</summary>
    [Fact]
    public async Task SzExecute_RekursionsschutzGreiftBeimDrittenAufruf()
    {
        var repoMock = new Mock<IEndpointRepository>();

        var endpoint = CreateEndpoint(relPath: "/rekursiv", name: "Rekursiv",
            preRequestScript: "sz.execute('Rekursiv');");

        repoMock.Setup(r => r.GetEndpointByNameAsync(1, "Rekursiv"))
            .ReturnsAsync(new List<Core.Models.Endpoint> { endpoint });

        var scriptRunner = new EndpointScriptRunner(
            CreateEmptyEnvironmentRepositoryMock().Object,
            CreateEmptySignalRNotificationServiceMock().Object,
            TestMockFactory.CreateActivityLogServiceMock().Object);

        var (realService, _, _) = CreateService(
            _credMock,
            endpointRepositoryMock: repoMock,
            realScriptRunner: scriptRunner);

        var result = await realService.ExecuteAsync(endpoint);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    /// <summary>EndpunktOhneSkript_VerhaeltSichWieBisher</summary>
    [Fact]
    public async Task EndpunktOhneSkript_VerhaeltSichWieBisher()
    {
        var scriptMock = new Mock<IEndpointScriptRunner>();
        var (service, _, _) = CreateService(_credMock, scriptRunnerMock: scriptMock);

        var endpoint = CreateEndpoint(AuthenticationType.None);

        var result = await service.ExecuteAsync(endpoint);

        Assert.True(result.Success);
        scriptMock.Verify(r => r.ExecuteAsync(It.IsAny<string>(), It.IsAny<ScriptContext>()), Times.Never);
    }

    /// <summary>Execute_ErfolgreichRequest_ProtokolliertEndpointExecuted</summary>
    [Fact]
    public async Task Execute_ErfolgreichRequest_ProtokolliertEndpointExecuted()
    {
        var logMock = TestMockFactory.CreateActivityLogServiceMock();
        var (service, _, _) = CreateService(_credMock, activityLogServiceMock: logMock);
        var endpoint = CreateEndpoint(AuthenticationType.None);

        await service.ExecuteAsync(endpoint);

        logMock.Verify(l => l.Log(ActivityLogCategory.EndpointExecuted, It.IsAny<string>(), It.IsAny<string?>()), Times.Once);
    }

    /// <summary>Execute_HttpFehler_ProtokolliertHttpError</summary>
    [Fact]
    public async Task Execute_HttpFehler_ProtokolliertHttpError()
    {
        var logMock = TestMockFactory.CreateActivityLogServiceMock();
        var (service, _, _) = CreateService(_credMock, responseCode: System.Net.HttpStatusCode.BadRequest, activityLogServiceMock: logMock);
        var endpoint = CreateEndpoint(AuthenticationType.None);

        await service.ExecuteAsync(endpoint);

        logMock.Verify(l => l.Log(ActivityLogCategory.HttpError, It.IsAny<string>(), It.IsAny<string?>()), Times.Once);
    }

    /// <summary>Execute_Exception_ProtokolliertInternalError</summary>
    [Fact]
    public async Task Execute_Exception_ProtokolliertInternalError()
    {
        var logMock = TestMockFactory.CreateActivityLogServiceMock();
        var (service, handlerMock, _) = CreateService(_credMock, activityLogServiceMock: logMock);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Test-Exception"));
        var endpoint = CreateEndpoint(AuthenticationType.None);

        await service.ExecuteAsync(endpoint);

        logMock.Verify(l => l.Log(ActivityLogCategory.InternalError, It.IsAny<string>(), It.IsAny<string?>()), Times.Once);
    }

    /// <summary>Execute_MaskiertVariablen_ImDetailString</summary>
    [Fact]
    public async Task Execute_MaskiertVariablen_ImDetailString()
    {
        var logMock = TestMockFactory.CreateActivityLogServiceMock();

        var secretValue = "supersecret";
        var envMock = new Mock<IActiveEnvironmentService>();
        var env = new Core.Models.SystemEnvironment
        {
            Id = 1,
            Name = "Test",
            Variables =
            [
                new Core.Models.EnvironmentVariable { Name = "token", Value = secretValue, IsValueMasked = true }
            ]
        };
        envMock.Setup(s => s.ActiveEnvironment).Returns(env);
        envMock.Setup(s => s.ActiveVariables).Returns(new Dictionary<string, string> { ["token"] = secretValue });

        string? capturedDetails = null;
        logMock.Setup(l => l.Log(ActivityLogCategory.EndpointExecuted, It.IsAny<string>(), It.IsAny<string?>()))
            .Callback<ActivityLogCategory, string, string?>((_, _, d) => capturedDetails = d);

        var (service, _, _) = CreateService(_credMock,
            body: $"{{\"token\":\"{secretValue}\"}}",
            activeEnvironmentMock: envMock,
            activityLogServiceMock: logMock);
        var endpoint = CreateEndpoint(AuthenticationType.None);

        await service.ExecuteAsync(endpoint);

        Assert.NotNull(capturedDetails);
        Assert.DoesNotContain(secretValue, capturedDetails);
        Assert.Contains("***", capturedDetails);
    }

    /// <summary>SzExecute_MehrdeutigerName_GibtFehlerZurueck</summary>
    [Fact]
    public async Task SzExecute_MehrdeutigerName_GibtFehlerZurueck()
    {
        var endpoints = new List<Core.Models.Endpoint>
        {
            CreateEndpoint(relPath: "/a", name: "Doppelt", id: 2),
            CreateEndpoint(relPath: "/b", name: "Doppelt", id: 3)
        };

        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointByNameAsync(1, "Doppelt")).ReturnsAsync(endpoints);

        EndpointExecutionResult? capturedExecResult = null;
        var scriptMock = new Mock<IEndpointScriptRunner>();
        scriptMock.Setup(r => r.ExecuteAsync(It.IsAny<string>(), It.IsAny<ScriptContext>()))
            .Returns<string, ScriptContext>(async (_, ctx) =>
            {
                capturedExecResult = await ctx.ExecuteEndpoint("Doppelt");
                return new ScriptExecutionResult { Success = true };
            });

        var (service, _, _) = CreateService(_credMock, scriptRunnerMock: scriptMock, endpointRepositoryMock: repoMock);

        var endpoint = CreateEndpoint(name: "Aufrufend", preRequestScript: "sz.execute('Doppelt');");

        await service.ExecuteAsync(endpoint);

        Assert.NotNull(capturedExecResult);
        Assert.False(capturedExecResult!.Success);
        Assert.Contains("Mehrdeutig", capturedExecResult.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }
}
