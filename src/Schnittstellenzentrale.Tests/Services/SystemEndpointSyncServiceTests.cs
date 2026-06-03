using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Moq;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Swashbuckle.AspNetCore.Swagger;
using CoreHttpMethod = Schnittstellenzentrale.Core.Enums.HttpMethod;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>SystemEndpointSyncServiceTests</summary>
public class SystemEndpointSyncServiceTests
{
    private sealed class TestableSyncService(IServiceScopeFactory scopeFactory, ILogger<SystemEndpointSyncService> logger)
        : SystemEndpointSyncService(scopeFactory, logger)
    {
        /// <summary>RunAsync</summary>
        public Task RunAsync() => ExecuteAsync(CancellationToken.None);
    }

    private static (TestableSyncService Service, Mock<IApplicationRepository> AppRepoMock, Mock<ISwaggerProvider> SwaggerProviderMock, Mock<IEndpointRepository> EndpointRepoMock, Mock<ISwaggerImportService> SwaggerImportServiceMock, Mock<ILogger<SystemEndpointSyncService>> LoggerMock) CreateService()
    {
        var appRepoMock = new Mock<IApplicationRepository>();
        var swaggerProviderMock = new Mock<ISwaggerProvider>();
        var endpointRepoMock = new Mock<IEndpointRepository>();
        var swaggerImportServiceMock = new Mock<ISwaggerImportService>();

        endpointRepoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>())).ReturnsAsync([]);
        swaggerImportServiceMock.Setup(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>())).Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton<IApplicationRepository>(appRepoMock.Object);
        services.AddSingleton<ISwaggerProvider>(swaggerProviderMock.Object);
        services.AddSingleton<IEndpointRepository>(endpointRepoMock.Object);
        services.AddSingleton<ISwaggerImportService>(swaggerImportServiceMock.Object);
        var provider = services.BuildServiceProvider();

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(provider);
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        var loggerMock = new Mock<ILogger<SystemEndpointSyncService>>();
        var service = new TestableSyncService(scopeFactoryMock.Object, loggerMock.Object);

        return (service, appRepoMock, swaggerProviderMock, endpointRepoMock, swaggerImportServiceMock, loggerMock);
    }

    private static ApplicationGroup SystemGroup()
    {
        var app = new Application { Id = 1, IsSystem = true, InterfaceUrl = "http://localhost/swagger.json", InterfaceType = InterfaceType.Rest, BaseUrl = "http://localhost" };
        return new ApplicationGroup { Id = 1, IsSystem = true, Applications = [app] };
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

    private static OpenApiDocument DocumentWithNegotiateAuth(params (string Path, HttpMethod Method, string OperationId)[] operations)
    {
        var doc = DocumentWithPaths(operations);
        foreach (var pathItem in doc.Paths.Values)
            foreach (var operation in pathItem.Operations.Values)
                (operation.Security ??= []).Add(new OpenApiSecurityRequirement
                {
                    { new OpenApiSecuritySchemeReference("Negotiate", null), new List<string>() }
                });
        return doc;
    }

    /// <summary>ExecuteAsync_NewEndpoints_AreAdded</summary>
    [Fact]
    public async Task ExecuteAsync_NewEndpoints_AreAdded()
    {
        var (service, appRepoMock, swaggerProviderMock, _, swaggerImportServiceMock, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithPaths(("/items", HttpMethod.Get, "getItems"), ("/items", HttpMethod.Post, "createItem")));

        await service.RunAsync();

        swaggerImportServiceMock.Verify(s => s.ApplyDiffAsync(
            It.Is<ImportDiff>(d => d.NewEndpoints.Count == 2)), Times.Once);
    }

    /// <summary>ExecuteAsync_RemovedEndpoints_AreDeleted</summary>
    [Fact]
    public async Task ExecuteAsync_RemovedEndpoints_AreDeleted()
    {
        var (service, appRepoMock, swaggerProviderMock, endpointRepoMock, swaggerImportServiceMock, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(new OpenApiDocument { Paths = new OpenApiPaths() });
        endpointRepoMock.Setup(r => r.GetEndpointsAsync(1))
            .ReturnsAsync([new Endpoint { Id = 10, Method = CoreHttpMethod.DELETE, RelativePath = "/items/10", ApplicationId = 1 }]);

        await service.RunAsync();

        swaggerImportServiceMock.Verify(s => s.ApplyDiffAsync(
            It.Is<ImportDiff>(d => d.RemovedEndpoints.Count == 1)), Times.Once);
    }

    /// <summary>ExecuteAsync_ExistingEndpoints_AreLeftUntouched</summary>
    [Fact]
    public async Task ExecuteAsync_ExistingEndpoints_AreLeftUntouched()
    {
        var (service, appRepoMock, swaggerProviderMock, endpointRepoMock, swaggerImportServiceMock, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithPaths(("/items", HttpMethod.Get, "getItems")));
        endpointRepoMock.Setup(r => r.GetEndpointsAsync(1))
            .ReturnsAsync([new Endpoint { Id = 5, Method = CoreHttpMethod.GET, RelativePath = "/items", ApplicationId = 1 }]);

        await service.RunAsync();

        swaggerImportServiceMock.Verify(s => s.ApplyDiffAsync(
            It.Is<ImportDiff>(d => d.NewEndpoints.Count == 0 && d.RemovedEndpoints.Count == 0)), Times.Once);
    }

    /// <summary>ExecuteAsync_WhenSwaggerProviderThrows_LogsErrorAndDoesNotThrow</summary>
    [Fact]
    public async Task ExecuteAsync_WhenSwaggerProviderThrows_LogsErrorAndDoesNotThrow()
    {
        var (service, appRepoMock, swaggerProviderMock, _, swaggerImportServiceMock, loggerMock) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Throws(new InvalidOperationException("Swagger nicht verfügbar"));

        var exception = await Record.ExceptionAsync(() => service.RunAsync());

        Assert.Null(exception);
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        swaggerImportServiceMock.Verify(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>()), Times.Never);
    }

    /// <summary>ExecuteAsync_WhenApplyDiffThrows_DoesNotThrow</summary>
    [Fact]
    public async Task ExecuteAsync_WhenApplyDiffThrows_DoesNotThrow()
    {
        var (service, appRepoMock, swaggerProviderMock, _, swaggerImportServiceMock, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithPaths(("/items", HttpMethod.Get, "getItems")));
        swaggerImportServiceMock.Setup(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>()))
            .ThrowsAsync(new InvalidOperationException("Simulierter Fehler"));

        var exception = await Record.ExceptionAsync(() => service.RunAsync());

        Assert.Null(exception);
    }

    /// <summary>ExecuteAsync_IsIdempotent_OnRepeatedCall</summary>
    [Fact]
    public async Task ExecuteAsync_IsIdempotent_OnRepeatedCall()
    {
        var (service, appRepoMock, swaggerProviderMock, endpointRepoMock, swaggerImportServiceMock, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithPaths(("/items", HttpMethod.Get, "getItems")));
        endpointRepoMock.Setup(r => r.GetEndpointsAsync(1))
            .ReturnsAsync([new Endpoint { Id = 5, Method = CoreHttpMethod.GET, RelativePath = "/items", ApplicationId = 1 }]);

        await service.RunAsync();

        swaggerImportServiceMock.Verify(s => s.ApplyDiffAsync(
            It.Is<ImportDiff>(d => d.NewEndpoints.Count == 0 && d.RemovedEndpoints.Count == 0)), Times.Once);
    }

    /// <summary>ExecuteAsync_WhenSystemGroupMissing_LogsWarningAndSkips</summary>
    [Fact]
    public async Task ExecuteAsync_WhenSystemGroupMissing_LogsWarningAndSkips()
    {
        var (service, appRepoMock, swaggerProviderMock, _, swaggerImportServiceMock, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync((ApplicationGroup?)null);

        var exception = await Record.ExceptionAsync(() => service.RunAsync());

        Assert.Null(exception);
        swaggerProviderMock.Verify(p => p.GetSwagger(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        swaggerImportServiceMock.Verify(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>()), Times.Never);
    }

    /// <summary>ExecuteAsync_WhenSystemAppMissing_LogsWarningAndSkips</summary>
    [Fact]
    public async Task ExecuteAsync_WhenSystemAppMissing_LogsWarningAndSkips()
    {
        var (service, appRepoMock, swaggerProviderMock, _, swaggerImportServiceMock, loggerMock) = CreateService();
        var groupWithoutSystemApp = new ApplicationGroup
        {
            Id = 1,
            IsSystem = true,
            Applications = [new Application { Id = 2, IsSystem = false, InterfaceType = InterfaceType.Rest, BaseUrl = "http://localhost" }]
        };
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(groupWithoutSystemApp);

        var exception = await Record.ExceptionAsync(() => service.RunAsync());

        Assert.Null(exception);
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        swaggerProviderMock.Verify(p => p.GetSwagger(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        swaggerImportServiceMock.Verify(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>()), Times.Never);
    }

    /// <summary>ExecuteAsync_WithNegotiateSecurityScheme_SetsNegotiateAuthenticationType</summary>
    [Fact]
    public async Task ExecuteAsync_WithNegotiateSecurityScheme_SetsNegotiateAuthenticationType()
    {
        var (service, appRepoMock, swaggerProviderMock, _, swaggerImportServiceMock, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithNegotiateAuth(("/api/items", HttpMethod.Get, "getItems")));

        ImportDiff? capturedDiff = null;
        swaggerImportServiceMock.Setup(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>()))
            .Callback<ImportDiff>(d => capturedDiff = d)
            .Returns(Task.CompletedTask);

        await service.RunAsync();

        Assert.NotNull(capturedDiff);
        Assert.Single(capturedDiff.NewEndpoints);
        Assert.Equal(AuthenticationType.Negotiate, capturedDiff.NewEndpoints[0].AuthenticationType);
    }

    /// <summary>ExecuteAsync_WithoutNegotiateSecurityScheme_SetsNoneAuthenticationType</summary>
    [Fact]
    public async Task ExecuteAsync_WithoutNegotiateSecurityScheme_SetsNoneAuthenticationType()
    {
        var (service, appRepoMock, swaggerProviderMock, _, swaggerImportServiceMock, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithPaths(("/api/items", HttpMethod.Get, "getItems")));

        ImportDiff? capturedDiff = null;
        swaggerImportServiceMock.Setup(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>()))
            .Callback<ImportDiff>(d => capturedDiff = d)
            .Returns(Task.CompletedTask);

        await service.RunAsync();

        Assert.NotNull(capturedDiff);
        Assert.Single(capturedDiff.NewEndpoints);
        Assert.Equal(AuthenticationType.None, capturedDiff.NewEndpoints[0].AuthenticationType);
    }

    private static OpenApiDocument DocumentWithBearerToken(string path, HttpMethod method, string operationId, string bearerToken)
    {
        var operation = new OpenApiOperation
        {
            OperationId = operationId,
            Extensions = new Dictionary<string, IOpenApiExtension>
            {
                ["x-sz-bearer-token"] = new JsonNodeExtension(JsonValue.Create(bearerToken))
            }
        };
        var pathItem = new OpenApiPathItem();
        pathItem.AddOperation(method, operation);
        return new OpenApiDocument { Paths = new OpenApiPaths { [path] = pathItem } };
    }

    /// <summary>ExecuteAsync_WithBearerTokenExtension_IncludesBearerTokenInDiff</summary>
    [Fact]
    public async Task ExecuteAsync_WithBearerTokenExtension_IncludesBearerTokenInDiff()
    {
        var (service, appRepoMock, swaggerProviderMock, _, swaggerImportServiceMock, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithBearerToken("/api/items", HttpMethod.Get, "getItems", "my-secret-token"));

        ImportDiff? capturedDiff = null;
        swaggerImportServiceMock.Setup(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>()))
            .Callback<ImportDiff>(d => capturedDiff = d)
            .Returns(Task.CompletedTask);

        await service.RunAsync();

        Assert.NotNull(capturedDiff);
        Assert.True(capturedDiff.BearerTokens.ContainsKey("GET:/api/items"));
        Assert.Equal("my-secret-token", capturedDiff.BearerTokens["GET:/api/items"]);
    }

    private static OpenApiDocument DocumentWithPreRequestScript(string path, HttpMethod method, string operationId, string script)
    {
        var operation = new OpenApiOperation
        {
            OperationId = operationId,
            Extensions = new Dictionary<string, IOpenApiExtension>
            {
                ["x-sz-pre-request-script"] = new JsonNodeExtension(JsonValue.Create(script))
            }
        };
        var pathItem = new OpenApiPathItem();
        pathItem.AddOperation(method, operation);
        return new OpenApiDocument { Paths = new OpenApiPaths { [path] = pathItem } };
    }

    private static OpenApiDocument DocumentWithPostRequestScript(string path, HttpMethod method, string operationId, string script)
    {
        var operation = new OpenApiOperation
        {
            OperationId = operationId,
            Extensions = new Dictionary<string, IOpenApiExtension>
            {
                ["x-sz-post-request-script"] = new JsonNodeExtension(JsonValue.Create(script))
            }
        };
        var pathItem = new OpenApiPathItem();
        pathItem.AddOperation(method, operation);
        return new OpenApiDocument { Paths = new OpenApiPaths { [path] = pathItem } };
    }

    /// <summary>ExecuteAsync_WithPreRequestScript_SetsPreRequestScriptOnEndpoint</summary>
    [Fact]
    public async Task ExecuteAsync_WithPreRequestScript_SetsPreRequestScriptOnEndpoint()
    {
        var (service, appRepoMock, swaggerProviderMock, _, swaggerImportServiceMock, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithPreRequestScript("/api/items", HttpMethod.Get, "getItems", "console.log('pre');"));

        ImportDiff? capturedDiff = null;
        swaggerImportServiceMock.Setup(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>()))
            .Callback<ImportDiff>(d => capturedDiff = d)
            .Returns(Task.CompletedTask);

        await service.RunAsync();

        Assert.NotNull(capturedDiff);
        Assert.Single(capturedDiff.NewEndpoints);
        Assert.Equal("console.log('pre');", capturedDiff.NewEndpoints[0].PreRequestScript);
    }

    /// <summary>ExecuteAsync_WithPostRequestScript_SetsPostRequestScriptOnEndpoint</summary>
    [Fact]
    public async Task ExecuteAsync_WithPostRequestScript_SetsPostRequestScriptOnEndpoint()
    {
        var (service, appRepoMock, swaggerProviderMock, _, swaggerImportServiceMock, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithPostRequestScript("/api/items", HttpMethod.Post, "createItem", "console.log('post');"));

        ImportDiff? capturedDiff = null;
        swaggerImportServiceMock.Setup(s => s.ApplyDiffAsync(It.IsAny<ImportDiff>()))
            .Callback<ImportDiff>(d => capturedDiff = d)
            .Returns(Task.CompletedTask);

        await service.RunAsync();

        Assert.NotNull(capturedDiff);
        Assert.Single(capturedDiff.NewEndpoints);
        Assert.Equal("console.log('post');", capturedDiff.NewEndpoints[0].PostRequestScript);
    }
}
