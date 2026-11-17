using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi;
using Moq;
using Moq.Protected;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Services;
using CoreHttpMethod = Schnittstellenzentrale.Core.Enums.HttpMethod;
using HttpMethod = System.Net.Http.HttpMethod;

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

    /// <summary>Import_GeneratedStandardScript_UsesActualAuthenticateEndpointName</summary>
    [Fact]
    public async Task Import_GeneratedStandardScript_UsesActualAuthenticateEndpointName()
    {
        const string swagger = """
            {
              "openapi": "3.0.0",
              "info": { "title": "Test", "version": "1.0" },
              "paths": {
                "/authenticate": {
                  "post": {
                    "operationId": "Auth_AuthenticateAsync",
                    "x-sz-post-request-script": "sz.environment.set('schnittstellenzentrale.authToken', sz.response.body.asJson().token);",
                    "responses": { "200": { "description": "OK" } }
                  }
                },
                "/items": {
                  "get": {
                    "operationId": "getItems",
                    "x-sz-bearer-token": "{{schnittstellenzentrale.authToken}}",
                    "x-sz-post-request-script": "if (sz.response.statusCode === 401) {\n  if (sz.execute('Authenticate')) {\n    sz.repeat();\n  }\n}",
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

        var itemEndpoint = diff.NewEndpoints.Single(e => e.Name == "getItems");
        Assert.Contains("sz.execute('Auth_AuthenticateAsync')", itemEndpoint.PostRequestScript);
        Assert.DoesNotContain("sz.execute('Authenticate')", itemEndpoint.PostRequestScript);
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
        repoMock.Setup(r => r.GetEndpointGroupsAsync(It.IsAny<int>())).ReturnsAsync([]);
        repoMock.Setup(r => r.AddEndpointGroupAsync(It.IsAny<Core.Models.EndpointGroup>()))
            .ReturnsAsync((Core.Models.EndpointGroup g) => { g.Id = 1; return g; });
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

    /// <summary>ApplyDiff_NewEndpoints_GroupsCreatedFromUrlSegments</summary>
    [Fact]
    public async Task ApplyDiff_NewEndpoints_GroupsCreatedFromUrlSegments()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointGroupsAsync(It.IsAny<int>())).ReturnsAsync([]);

        var createdGroups = new List<Core.Models.EndpointGroup>();
        int nextId = 10;
        repoMock.Setup(r => r.AddEndpointGroupAsync(It.IsAny<Core.Models.EndpointGroup>()))
            .ReturnsAsync((Core.Models.EndpointGroup g) => { g.Id = nextId++; createdGroups.Add(g); return g; });

        Core.Models.Endpoint? addedEndpoint = null;
        repoMock.Setup(r => r.AddEndpointAsync(It.IsAny<Core.Models.Endpoint>()))
            .Callback<Core.Models.Endpoint>(e => addedEndpoint = e)
            .ReturnsAsync((Core.Models.Endpoint e) => e);

        var service = CreateService(SwaggerWithGetPost, repoMock);
        var diff = new ImportDiff
        {
            NewEndpoints = [new Core.Models.Endpoint
            {
                Name = "getUngrouped",
                Method = Core.Enums.HttpMethod.GET,
                RelativePath = "/api/applications/ungrouped",
                ApplicationId = 1
            }]
        };

        await service.ApplyDiffAsync(diff);

        Assert.Equal(2, createdGroups.Count);
        Assert.Equal("applications", createdGroups[0].Name);
        Assert.Null(createdGroups[0].ParentGroupId);
        Assert.Equal("ungrouped", createdGroups[1].Name);
        Assert.Equal(10, createdGroups[1].ParentGroupId);
        Assert.NotNull(addedEndpoint);
        Assert.Equal(11, addedEndpoint.EndpointGroupId);
    }

    /// <summary>ApplyDiff_PathParameterSegments_AreSkipped</summary>
    [Fact]
    public async Task ApplyDiff_PathParameterSegments_AreSkipped()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointGroupsAsync(It.IsAny<int>())).ReturnsAsync([]);

        var createdGroups = new List<Core.Models.EndpointGroup>();
        repoMock.Setup(r => r.AddEndpointGroupAsync(It.IsAny<Core.Models.EndpointGroup>()))
            .ReturnsAsync((Core.Models.EndpointGroup g) => { g.Id = 20; createdGroups.Add(g); return g; });

        Core.Models.Endpoint? addedEndpoint = null;
        repoMock.Setup(r => r.AddEndpointAsync(It.IsAny<Core.Models.Endpoint>()))
            .Callback<Core.Models.Endpoint>(e => addedEndpoint = e)
            .ReturnsAsync((Core.Models.Endpoint e) => e);

        var service = CreateService(SwaggerWithGetPost, repoMock);
        var diff = new ImportDiff
        {
            NewEndpoints = [new Core.Models.Endpoint
            {
                Name = "getGroup",
                Method = Core.Enums.HttpMethod.GET,
                RelativePath = "/api/application-groups/{id}",
                ApplicationId = 1
            }]
        };

        await service.ApplyDiffAsync(diff);

        Assert.Single(createdGroups);
        Assert.Equal("application-groups", createdGroups[0].Name);
        Assert.Null(createdGroups[0].ParentGroupId);
        Assert.NotNull(addedEndpoint);
        Assert.Equal(20, addedEndpoint.EndpointGroupId);
    }

    /// <summary>ApplyDiff_ExistingGroups_AreReusedAndNotCreatedAgain</summary>
    [Fact]
    public async Task ApplyDiff_ExistingGroups_AreReusedAndNotCreatedAgain()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointGroupsAsync(It.IsAny<int>()))
            .ReturnsAsync([new Core.Models.EndpointGroup { Id = 5, Name = "items", ApplicationId = 1, ParentGroupId = null }]);
        repoMock.Setup(r => r.AddEndpointAsync(It.IsAny<Core.Models.Endpoint>()))
            .ReturnsAsync((Core.Models.Endpoint e) => e);

        var service = CreateService(SwaggerWithGetPost, repoMock);
        var diff = new ImportDiff
        {
            NewEndpoints =
            [
                new Core.Models.Endpoint { Name = "getItems", Method = Core.Enums.HttpMethod.GET, RelativePath = "/api/items", ApplicationId = 1 },
                new Core.Models.Endpoint { Name = "createItem", Method = Core.Enums.HttpMethod.POST, RelativePath = "/api/items", ApplicationId = 1 }
            ]
        };

        await service.ApplyDiffAsync(diff);

        repoMock.Verify(r => r.AddEndpointGroupAsync(It.IsAny<Core.Models.EndpointGroup>()), Times.Never);
        repoMock.Verify(r => r.AddEndpointAsync(It.Is<Core.Models.Endpoint>(e => e.EndpointGroupId == 5)), Times.Exactly(2));
    }

    /// <summary>ApplyDiff_WithErrorMessage_DoesNotApplyAnyChanges</summary>
    [Fact]
    public async Task ApplyDiff_WithErrorMessage_DoesNotApplyAnyChanges()
    {
        var repoMock = new Mock<IEndpointRepository>();
        var service = CreateService(SwaggerWithGetPost, repoMock);
        var diff = new ImportDiff
        {
            ErrorMessage = "Duplizierter Endpoint-Schlüssel im Import: GET:/items",
            NewEndpoints = [new Core.Models.Endpoint { Name = "getItems", Method = CoreHttpMethod.GET, RelativePath = "/items", ApplicationId = 1 }],
            RemovedEndpoints = [new Core.Models.Endpoint { Id = 7, Name = "old", Method = CoreHttpMethod.DELETE, RelativePath = "/old", ApplicationId = 1 }]
        };

        await service.ApplyDiffAsync(diff);

        repoMock.Verify(r => r.AddEndpointAsync(It.IsAny<Core.Models.Endpoint>()), Times.Never);
        repoMock.Verify(r => r.UpdateEndpointAsync(It.IsAny<Core.Models.Endpoint>()), Times.Never);
        repoMock.Verify(r => r.DeleteEndpointAsync(It.IsAny<int>()), Times.Never);
    }

    private static OpenApiDocument DocumentWithPaths(params (string Path, HttpMethod Method, string OperationId)[] operations)
    {
        var paths = new OpenApiPaths();
        foreach (var group in operations.GroupBy(o => o.Path))
        {
            var pathItem = new OpenApiPathItem();
            foreach (var (_, method, operationId) in group)
                pathItem.AddOperation(method, new OpenApiOperation { OperationId = operationId });
            paths[group.Key] = pathItem;
        }
        return new OpenApiDocument { Paths = paths };
    }

    /// <summary>SyncFromDocument_NewEndpoints_AreAdded</summary>
    [Fact]
    public async Task SyncFromDocument_NewEndpoints_AreAdded()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        repoMock.Setup(r => r.GetEndpointGroupsAsync(It.IsAny<int>())).ReturnsAsync([]);
        repoMock.Setup(r => r.AddEndpointGroupAsync(It.IsAny<Core.Models.EndpointGroup>()))
            .ReturnsAsync((Core.Models.EndpointGroup g) => { g.Id = 1; return g; });
        repoMock.Setup(r => r.AddEndpointAsync(It.IsAny<Core.Models.Endpoint>()))
            .ReturnsAsync((Core.Models.Endpoint e) => e);
        var service = CreateService(SwaggerWithGetPost, repoMock);
        var document = DocumentWithPaths(("/items", HttpMethod.Get, "getItems"), ("/items", HttpMethod.Post, "createItem"));

        var diff = await service.SyncFromDocumentAsync(document, 1);

        Assert.Null(diff.ErrorMessage);
        repoMock.Verify(r => r.AddEndpointAsync(It.IsAny<Core.Models.Endpoint>()), Times.Exactly(2));
        repoMock.Verify(r => r.UpdateEndpointAsync(It.IsAny<Core.Models.Endpoint>()), Times.Never);
        repoMock.Verify(r => r.DeleteEndpointAsync(It.IsAny<int>()), Times.Never);
    }

    /// <summary>SyncFromDocument_ChangedEndpoint_IsUpdated</summary>
    [Fact]
    public async Task SyncFromDocument_ChangedEndpoint_IsUpdated()
    {
        var existing = new List<Core.Models.Endpoint>
        {
            new() { Id = 5, Name = "alterName", Method = CoreHttpMethod.GET, RelativePath = "/items", ApplicationId = 1 }
        };
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync(existing);
        repoMock.Setup(r => r.UpdateEndpointAsync(It.IsAny<Core.Models.Endpoint>()))
            .ReturnsAsync((Core.Models.Endpoint e) => e);
        var service = CreateService(SwaggerWithGetPost, repoMock);
        var document = DocumentWithPaths(("/items", HttpMethod.Get, "getItems"));

        var diff = await service.SyncFromDocumentAsync(document, 1);

        Assert.Null(diff.ErrorMessage);
        repoMock.Verify(r => r.UpdateEndpointAsync(
            It.Is<Core.Models.Endpoint>(e => e.Id == 5 && e.Name == "getItems")), Times.Once);
        repoMock.Verify(r => r.AddEndpointAsync(It.IsAny<Core.Models.Endpoint>()), Times.Never);
        repoMock.Verify(r => r.DeleteEndpointAsync(It.IsAny<int>()), Times.Never);
    }

    /// <summary>SyncFromDocument_RemovedEndpoint_IsDeleted</summary>
    [Fact]
    public async Task SyncFromDocument_RemovedEndpoint_IsDeleted()
    {
        var existing = new List<Core.Models.Endpoint>
        {
            new() { Id = 9, Name = "alt", Method = CoreHttpMethod.DELETE, RelativePath = "/items/{id}", ApplicationId = 1 }
        };
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync(existing);
        repoMock.Setup(r => r.GetEndpointGroupsAsync(It.IsAny<int>())).ReturnsAsync([]);
        repoMock.Setup(r => r.AddEndpointGroupAsync(It.IsAny<Core.Models.EndpointGroup>()))
            .ReturnsAsync((Core.Models.EndpointGroup g) => { g.Id = 1; return g; });
        repoMock.Setup(r => r.AddEndpointAsync(It.IsAny<Core.Models.Endpoint>()))
            .ReturnsAsync((Core.Models.Endpoint e) => e);
        var service = CreateService(SwaggerWithGetPost, repoMock);
        var document = DocumentWithPaths(("/items", HttpMethod.Get, "getItems"));

        var diff = await service.SyncFromDocumentAsync(document, 1);

        Assert.Null(diff.ErrorMessage);
        repoMock.Verify(r => r.DeleteEndpointAsync(9), Times.Once);
        repoMock.Verify(r => r.UpdateEndpointAsync(It.IsAny<Core.Models.Endpoint>()), Times.Never);
    }

    /// <summary>SyncFromDocument_WithBearerToken_SavesCredential</summary>
    [Fact]
    public async Task SyncFromDocument_WithBearerToken_SavesCredential()
    {
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        repoMock.Setup(r => r.GetEndpointGroupsAsync(It.IsAny<int>())).ReturnsAsync([]);
        repoMock.Setup(r => r.AddEndpointGroupAsync(It.IsAny<Core.Models.EndpointGroup>()))
            .ReturnsAsync((Core.Models.EndpointGroup g) => { g.Id = 1; return g; });
        repoMock.Setup(r => r.AddEndpointAsync(It.IsAny<Core.Models.Endpoint>()))
            .ReturnsAsync((Core.Models.Endpoint e) => e);
        var credentialMock = new Mock<ICredentialService>();
        var service = CreateService(SwaggerWithGetPost, repoMock, credentialMock);

        var operation = new OpenApiOperation
        {
            OperationId = "getItems",
            Extensions = new Dictionary<string, IOpenApiExtension>
            {
                ["x-sz-bearer-token"] = new JsonNodeExtension(JsonValue.Create("my-secret-token"))
            }
        };
        var pathItem = new OpenApiPathItem();
        pathItem.AddOperation(HttpMethod.Get, operation);
        var document = new OpenApiDocument { Paths = new OpenApiPaths { ["/items"] = pathItem } };

        var diff = await service.SyncFromDocumentAsync(document, 1);

        Assert.Null(diff.ErrorMessage);
        credentialMock.Verify(c => c.SavePassword(
            It.Is<string>(t => t.Contains("BearerToken")),
            It.IsAny<string>(),
            "my-secret-token"), Times.Once);
    }

    /// <summary>SyncFromDocument_WhenDuplicateKeysInExisting_ReturnsErrorAndDoesNotApply</summary>
    [Fact]
    public async Task SyncFromDocument_WhenDuplicateKeysInExisting_ReturnsErrorAndDoesNotApply()
    {
        var existing = new List<Core.Models.Endpoint>
        {
            new() { Id = 1, Name = "a", Method = CoreHttpMethod.GET, RelativePath = "/items", ApplicationId = 1 },
            new() { Id = 2, Name = "b", Method = CoreHttpMethod.GET, RelativePath = "/items", ApplicationId = 1 }
        };
        var repoMock = new Mock<IEndpointRepository>();
        repoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync(existing);
        var service = CreateService(SwaggerWithGetPost, repoMock);
        var document = DocumentWithPaths(("/items", HttpMethod.Get, "getItems"));

        var diff = await service.SyncFromDocumentAsync(document, 1);

        Assert.NotNull(diff.ErrorMessage);
        repoMock.Verify(r => r.AddEndpointAsync(It.IsAny<Core.Models.Endpoint>()), Times.Never);
        repoMock.Verify(r => r.UpdateEndpointAsync(It.IsAny<Core.Models.Endpoint>()), Times.Never);
        repoMock.Verify(r => r.DeleteEndpointAsync(It.IsAny<int>()), Times.Never);
    }
}
