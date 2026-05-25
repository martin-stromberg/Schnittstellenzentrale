using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Services;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>SwaggerImportServiceTests</summary>
public class SwaggerImportServiceTests
{
    private const string SwaggerWithGetPost = """
        {
          "openapi": "3.0.0",
          "info": { "title": "Test", "version": "1.0" },
          "paths": {
            "/items": {
              "get": { "operationId": "getItems", "responses": { "200": { "description": "OK" } } },
              "post": { "operationId": "createItem", "responses": { "200": { "description": "OK" } } }
            }
          }
        }
        """;

    private static Core.Models.Application CreateTestApplication() =>
        new() { Id = 1, InterfaceUrl = "http://localhost/swagger.json", InterfaceType = Core.Enums.InterfaceType.Rest, BaseUrl = "http://localhost" };

    private static SwaggerImportService CreateService(
        string swaggerJson,
        Mock<IEndpointRepository> repoMock,
        Mock<ICredentialService>? credentialMock = null)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(swaggerJson, Encoding.UTF8, "application/json")
            });

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(handlerMock.Object));

        var credential = credentialMock ?? new Mock<ICredentialService>();
        return new SwaggerImportService(factoryMock.Object, repoMock.Object, credential.Object, NullLogger<SwaggerImportService>.Instance);
    }

    /// <summary>Import_NewSwaggerDefinition_ReturnsCorrectDiff</summary>
    [Fact]
    public async Task Import_NewSwaggerDefinition_ReturnsCorrectDiff()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        var service = CreateService(SwaggerWithGetPost, repoMock);
        var app = CreateTestApplication();

        var diff = await service.ImportAsync(app);

        Assert.Equal(2, diff.NewEndpoints.Count);
        Assert.Empty(diff.ChangedEndpoints);
        Assert.Empty(diff.RemovedEndpoints);
    }

    /// <summary>Import_ChangedSwaggerOperation_ReturnsChangedInDiff</summary>
    [Fact]
    public async Task Import_ChangedSwaggerOperation_ReturnsChangedInDiff()
    {
        var existing = new List<Core.Models.Endpoint>
        {
            new() { Id = 1, Name = "oldName", Method = Core.Enums.HttpMethod.GET, RelativePath = "/items", ApplicationId = 1 }
        };
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync(existing);
        var service = CreateService(SwaggerWithGetPost, repoMock);
        var app = CreateTestApplication();

        var diff = await service.ImportAsync(app);

        Assert.Contains(diff.ChangedEndpoints, e => e.RelativePath == "/items" && e.Method == Core.Enums.HttpMethod.GET);
    }

    /// <summary>Import_RemovedSwaggerOperation_ReturnsRemovedInDiff</summary>
    [Fact]
    public async Task Import_RemovedSwaggerOperation_ReturnsRemovedInDiff()
    {
        var existing = new List<Core.Models.Endpoint>
        {
            new() { Id = 1, Name = "deleteItems", Method = Core.Enums.HttpMethod.DELETE, RelativePath = "/items", ApplicationId = 1 }
        };
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync(existing);
        var service = CreateService(SwaggerWithGetPost, repoMock);
        var app = CreateTestApplication();

        var diff = await service.ImportAsync(app);

        Assert.Contains(diff.RemovedEndpoints, e => e.Method == Core.Enums.HttpMethod.DELETE && e.RelativePath == "/items");
    }

    /// <summary>Import_WithPostRequestScript_SetsPostRequestScriptOnEndpoint</summary>
    [Fact]
    public async Task Import_WithPostRequestScript_SetsPostRequestScriptOnEndpoint()
    {
        const string swagger = """
            {
              "openapi": "3.0.0",
              "info": { "title": "Test", "version": "1.0" },
              "paths": {
                "/items": {
                  "get": {
                    "operationId": "getItems",
                    "x-sz-post-request-script": "sz.environment.set('x', sz.response.body.raw);",
                    "responses": { "200": { "description": "OK" } }
                  }
                }
              }
            }
            """;
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        var service = CreateService(swagger, repoMock);
        var app = CreateTestApplication();

        var diff = await service.ImportAsync(app);

        Assert.Equal("sz.environment.set('x', sz.response.body.raw);", diff.NewEndpoints[0].PostRequestScript);
    }

    /// <summary>Import_WithPreRequestScript_SetsPreRequestScriptOnEndpoint</summary>
    [Fact]
    public async Task Import_WithPreRequestScript_SetsPreRequestScriptOnEndpoint()
    {
        const string swagger = """
            {
              "openapi": "3.0.0",
              "info": { "title": "Test", "version": "1.0" },
              "paths": {
                "/items": {
                  "get": {
                    "operationId": "getItems",
                    "x-sz-pre-request-script": "sz.request.headers['X-Token'] = sz.environment.get('token');",
                    "responses": { "200": { "description": "OK" } }
                  }
                }
              }
            }
            """;
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        var service = CreateService(swagger, repoMock);
        var app = CreateTestApplication();

        var diff = await service.ImportAsync(app);

        Assert.Equal("sz.request.headers['X-Token'] = sz.environment.get('token');", diff.NewEndpoints[0].PreRequestScript);
    }

    /// <summary>Import_WithBearerToken_SetsBearerTokenAuthTypeAndStoresBearerTokens</summary>
    [Fact]
    public async Task Import_WithBearerToken_SetsBearerTokenAuthTypeAndStoresBearerTokens()
    {
        const string swagger = """
            {
              "openapi": "3.0.0",
              "info": { "title": "Test", "version": "1.0" },
              "paths": {
                "/items": {
                  "get": {
                    "operationId": "getItems",
                    "x-sz-bearer-token": "{{schnittstellenzentrale.authToken}}",
                    "responses": { "200": { "description": "OK" } }
                  }
                }
              }
            }
            """;
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        var service = CreateService(swagger, repoMock);
        var app = CreateTestApplication();

        var diff = await service.ImportAsync(app);

        Assert.Equal(Core.Enums.AuthenticationType.BearerToken, diff.NewEndpoints[0].AuthenticationType);
        Assert.True(diff.BearerTokens.ContainsKey("GET:/items"));
        Assert.Equal("{{schnittstellenzentrale.authToken}}", diff.BearerTokens["GET:/items"]);
    }

    /// <summary>Import_WithoutExtensions_LeavesScriptFieldsNull</summary>
    [Fact]
    public async Task Import_WithoutExtensions_LeavesScriptFieldsNull()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        var service = CreateService(SwaggerWithGetPost, repoMock);
        var app = CreateTestApplication();

        var diff = await service.ImportAsync(app);

        Assert.All(diff.NewEndpoints, e =>
        {
            Assert.Null(e.PreRequestScript);
            Assert.Null(e.PostRequestScript);
            Assert.Equal(Core.Enums.AuthenticationType.None, e.AuthenticationType);
        });
    }

    /// <summary>Import_ReImport_MissingExtensions_ResetsScriptsAndAuthType</summary>
    [Fact]
    public async Task Import_ReImport_MissingExtensions_ResetsScriptsAndAuthType()
    {
        var existing = new List<Core.Models.Endpoint>
        {
            new()
            {
                Id = 1,
                Name = "getItems",
                Method = Core.Enums.HttpMethod.GET,
                RelativePath = "/items",
                ApplicationId = 1,
                PreRequestScript = "oldPre",
                PostRequestScript = "oldPost",
                AuthenticationType = Core.Enums.AuthenticationType.BearerToken
            }
        };
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync(existing);
        var service = CreateService(SwaggerWithGetPost, repoMock);
        var app = CreateTestApplication();

        var diff = await service.ImportAsync(app);

        var changed = diff.ChangedEndpoints.FirstOrDefault(e => e.RelativePath == "/items" && e.Method == Core.Enums.HttpMethod.GET);
        Assert.NotNull(changed);
        Assert.Null(changed.PreRequestScript);
        Assert.Null(changed.PostRequestScript);
        Assert.Equal(Core.Enums.AuthenticationType.None, changed.AuthenticationType);
    }

    /// <summary>ApplyDiff_WithBearerToken_CallsSavePassword</summary>
    [Fact]
    public async Task ApplyDiff_WithBearerToken_CallsSavePassword()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.AddEndpointAsync(It.IsAny<Core.Models.Endpoint>()))
            .ReturnsAsync((Core.Models.Endpoint e) => e);
        var credentialMock = new Mock<ICredentialService>();
        var service = CreateService(SwaggerWithGetPost, repoMock, credentialMock);

        var endpoint = new Core.Models.Endpoint
        {
            Id = 0,
            Name = "getItems",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/items",
            ApplicationId = 1,
            AuthenticationType = Core.Enums.AuthenticationType.BearerToken
        };
        var diff = new ImportDiff
        {
            NewEndpoints = [endpoint],
            BearerTokens = new Dictionary<string, string> { ["GET:/items"] = "myToken" }
        };

        await service.ApplyDiffAsync(diff);

        credentialMock.Verify(c => c.SavePassword(
            It.Is<string>(t => t.Contains("BearerToken")),
            It.IsAny<string>(),
            "myToken"), Times.Once);
    }
}
