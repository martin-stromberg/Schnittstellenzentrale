using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Moq;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Helpers;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Swashbuckle.AspNetCore.Swagger;
using CoreHttpMethod = Schnittstellenzentrale.Core.Enums.HttpMethod;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>SystemEndpointSyncServiceTests</summary>
public class SystemEndpointSyncServiceTests
{
    private sealed class TestableSyncService(IServiceScopeFactory scopeFactory, ILogger<SystemEndpointSyncService> logger, ICredentialService credentialService)
        : SystemEndpointSyncService(scopeFactory, logger, credentialService)
    {
        /// <summary>RunAsync</summary>
        public Task RunAsync() => ExecuteAsync(CancellationToken.None);
    }

    private static (TestableSyncService Service, Mock<IApplicationRepository> AppRepoMock, Mock<ISwaggerProvider> SwaggerProviderMock, Mock<IEndpointRepository> EndpointRepoMock, Mock<ILogger<SystemEndpointSyncService>> LoggerMock, Mock<ICredentialService> CredentialServiceMock) CreateService()
    {
        var appRepoMock = new Mock<IApplicationRepository>();
        var swaggerProviderMock = new Mock<ISwaggerProvider>();
        var endpointRepoMock = new Mock<IEndpointRepository>();

        endpointRepoMock.Setup(r => r.AddEndpointAsync(It.IsAny<Endpoint>()))
            .ReturnsAsync((Endpoint e) => e);
        endpointRepoMock.Setup(r => r.GetEndpointsAsync(It.IsAny<int>()))
            .ReturnsAsync([]);
        endpointRepoMock.Setup(r => r.GetEndpointGroupsAsync(It.IsAny<int>()))
            .ReturnsAsync([]);
        endpointRepoMock.Setup(r => r.AddEndpointGroupAsync(It.IsAny<EndpointGroup>()))
            .ReturnsAsync((EndpointGroup g) => { g.Id = 1; return g; });

        var services = new ServiceCollection();
        services.AddSingleton<IApplicationRepository>(appRepoMock.Object);
        services.AddSingleton<ISwaggerProvider>(swaggerProviderMock.Object);
        services.AddSingleton<IEndpointRepository>(endpointRepoMock.Object);
        var provider = services.BuildServiceProvider();

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(provider);
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        var loggerMock = new Mock<ILogger<SystemEndpointSyncService>>();
        var credentialServiceMock = new Mock<ICredentialService>();
        var service = new TestableSyncService(scopeFactoryMock.Object, loggerMock.Object, credentialServiceMock.Object);

        return (service, appRepoMock, swaggerProviderMock, endpointRepoMock, loggerMock, credentialServiceMock);
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
        doc.Components = new OpenApiComponents
        {
            SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Negotiate"] = new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "negotiate" }
            }
        };
        return doc;
    }

    /// <summary>ExecuteAsync_NewEndpoints_AreAdded</summary>
    [Fact]
    public async Task ExecuteAsync_NewEndpoints_AreAdded()
    {
        var (service, appRepoMock, swaggerProviderMock, endpointRepoMock, _, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithPaths(("/items", HttpMethod.Get, "getItems"), ("/items", HttpMethod.Post, "createItem")));

        await service.RunAsync();

        endpointRepoMock.Verify(r => r.AddEndpointAsync(It.IsAny<Endpoint>()), Times.Exactly(2));
        endpointRepoMock.Verify(r => r.DeleteEndpointAsync(It.IsAny<int>()), Times.Never);
    }

    /// <summary>ExecuteAsync_RemovedEndpoints_AreDeleted</summary>
    [Fact]
    public async Task ExecuteAsync_RemovedEndpoints_AreDeleted()
    {
        var (service, appRepoMock, swaggerProviderMock, endpointRepoMock, _, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(new OpenApiDocument { Paths = new OpenApiPaths() });
        endpointRepoMock.Setup(r => r.GetEndpointsAsync(1))
            .ReturnsAsync([new Endpoint { Id = 10, Method = CoreHttpMethod.DELETE, RelativePath = "/items/10", ApplicationId = 1 }]);

        await service.RunAsync();

        endpointRepoMock.Verify(r => r.DeleteEndpointAsync(10), Times.Once);
        endpointRepoMock.Verify(r => r.AddEndpointAsync(It.IsAny<Endpoint>()), Times.Never);
    }

    /// <summary>ExecuteAsync_ExistingEndpoints_AreLeftUntouched</summary>
    [Fact]
    public async Task ExecuteAsync_ExistingEndpoints_AreLeftUntouched()
    {
        var (service, appRepoMock, swaggerProviderMock, endpointRepoMock, _, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithPaths(("/items", HttpMethod.Get, "getItems")));
        endpointRepoMock.Setup(r => r.GetEndpointsAsync(1))
            .ReturnsAsync([new Endpoint { Id = 5, Method = CoreHttpMethod.GET, RelativePath = "/items", ApplicationId = 1 }]);

        await service.RunAsync();

        endpointRepoMock.Verify(r => r.AddEndpointAsync(It.IsAny<Endpoint>()), Times.Never);
        endpointRepoMock.Verify(r => r.DeleteEndpointAsync(It.IsAny<int>()), Times.Never);
    }

    /// <summary>ExecuteAsync_WhenSwaggerProviderThrows_LogsErrorAndDoesNotThrow</summary>
    [Fact]
    public async Task ExecuteAsync_WhenSwaggerProviderThrows_LogsErrorAndDoesNotThrow()
    {
        var (service, appRepoMock, swaggerProviderMock, endpointRepoMock, loggerMock, _) = CreateService();
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
        endpointRepoMock.Verify(r => r.AddEndpointAsync(It.IsAny<Endpoint>()), Times.Never);
        endpointRepoMock.Verify(r => r.DeleteEndpointAsync(It.IsAny<int>()), Times.Never);
    }

    /// <summary>ExecuteAsync_WhenDbThrows_DoesNotThrow</summary>
    [Fact]
    public async Task ExecuteAsync_WhenDbThrows_DoesNotThrow()
    {
        var (service, appRepoMock, swaggerProviderMock, endpointRepoMock, _, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithPaths(("/items", HttpMethod.Get, "getItems")));
        endpointRepoMock.Setup(r => r.AddEndpointAsync(It.IsAny<Endpoint>()))
            .ThrowsAsync(new InvalidOperationException("Simulierter Datenbankfehler"));

        var exception = await Record.ExceptionAsync(() => service.RunAsync());

        Assert.Null(exception);
    }

    /// <summary>ExecuteAsync_IsIdempotent_OnRepeatedCall</summary>
    [Fact]
    public async Task ExecuteAsync_IsIdempotent_OnRepeatedCall()
    {
        var (service, appRepoMock, swaggerProviderMock, endpointRepoMock, _, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithPaths(("/items", HttpMethod.Get, "getItems")));
        endpointRepoMock.Setup(r => r.GetEndpointsAsync(1))
            .ReturnsAsync([new Endpoint { Id = 5, Method = CoreHttpMethod.GET, RelativePath = "/items", ApplicationId = 1 }]);

        await service.RunAsync();

        endpointRepoMock.Verify(r => r.AddEndpointAsync(It.IsAny<Endpoint>()), Times.Never);
        endpointRepoMock.Verify(r => r.DeleteEndpointAsync(It.IsAny<int>()), Times.Never);
    }

    /// <summary>ExecuteAsync_WhenSystemGroupMissing_LogsWarningAndSkips</summary>
    [Fact]
    public async Task ExecuteAsync_WhenSystemGroupMissing_LogsWarningAndSkips()
    {
        var (service, appRepoMock, swaggerProviderMock, endpointRepoMock, _, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync((ApplicationGroup?)null);

        var exception = await Record.ExceptionAsync(() => service.RunAsync());

        Assert.Null(exception);
        swaggerProviderMock.Verify(p => p.GetSwagger(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        endpointRepoMock.Verify(r => r.AddEndpointAsync(It.IsAny<Endpoint>()), Times.Never);
        endpointRepoMock.Verify(r => r.DeleteEndpointAsync(It.IsAny<int>()), Times.Never);
    }

    /// <summary>ExecuteAsync_WhenSystemAppMissing_LogsWarningAndSkips</summary>
    [Fact]
    public async Task ExecuteAsync_WhenSystemAppMissing_LogsWarningAndSkips()
    {
        var (service, appRepoMock, swaggerProviderMock, endpointRepoMock, loggerMock, _) = CreateService();
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
        endpointRepoMock.Verify(r => r.AddEndpointAsync(It.IsAny<Endpoint>()), Times.Never);
        endpointRepoMock.Verify(r => r.DeleteEndpointAsync(It.IsAny<int>()), Times.Never);
    }

    /// <summary>ExecuteAsync_NewEndpoint_GroupsAreCreatedFromUrlSegments</summary>
    [Fact]
    public async Task ExecuteAsync_NewEndpoint_GroupsAreCreatedFromUrlSegments()
    {
        var (service, appRepoMock, swaggerProviderMock, endpointRepoMock, _, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithPaths(("/api/applications/ungrouped", HttpMethod.Get, "getUngrouped")));

        var createdGroups = new List<EndpointGroup>();
        int nextId = 10;
        endpointRepoMock.Setup(r => r.AddEndpointGroupAsync(It.IsAny<EndpointGroup>()))
            .ReturnsAsync((EndpointGroup g) => { g.Id = nextId++; createdGroups.Add(g); return g; });

        await service.RunAsync();

        Assert.Equal(2, createdGroups.Count);
        Assert.Equal("applications", createdGroups[0].Name);
        Assert.Null(createdGroups[0].ParentGroupId);
        Assert.Equal("ungrouped", createdGroups[1].Name);
        Assert.Equal(10, createdGroups[1].ParentGroupId);

        endpointRepoMock.Verify(r => r.AddEndpointAsync(It.Is<Endpoint>(e => e.EndpointGroupId == 11)), Times.Once);
    }

    /// <summary>ExecuteAsync_PathParameterSegments_AreSkipped</summary>
    [Fact]
    public async Task ExecuteAsync_PathParameterSegments_AreSkipped()
    {
        var (service, appRepoMock, swaggerProviderMock, endpointRepoMock, _, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithPaths(("/api/application-groups/{id}", HttpMethod.Get, "getGroup")));

        var createdGroups = new List<EndpointGroup>();
        endpointRepoMock.Setup(r => r.AddEndpointGroupAsync(It.IsAny<EndpointGroup>()))
            .ReturnsAsync((EndpointGroup g) => { g.Id = 20; createdGroups.Add(g); return g; });

        await service.RunAsync();

        Assert.Single(createdGroups);
        Assert.Equal("application-groups", createdGroups[0].Name);
        Assert.Null(createdGroups[0].ParentGroupId);

        endpointRepoMock.Verify(r => r.AddEndpointAsync(It.Is<Endpoint>(e => e.EndpointGroupId == 20)), Times.Once);
    }

    /// <summary>ExecuteAsync_ExistingGroups_AreReusedAndNotCreatedAgain</summary>
    [Fact]
    public async Task ExecuteAsync_ExistingGroups_AreReusedAndNotCreatedAgain()
    {
        var (service, appRepoMock, swaggerProviderMock, endpointRepoMock, _, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithPaths(
                ("/api/items", HttpMethod.Get, "getItems"),
                ("/api/items", HttpMethod.Post, "createItem")));

        endpointRepoMock.Setup(r => r.GetEndpointGroupsAsync(1))
            .ReturnsAsync([new EndpointGroup { Id = 5, Name = "items", ApplicationId = 1, ParentGroupId = null }]);

        await service.RunAsync();

        endpointRepoMock.Verify(r => r.AddEndpointGroupAsync(It.IsAny<EndpointGroup>()), Times.Never);
        endpointRepoMock.Verify(r => r.AddEndpointAsync(It.Is<Endpoint>(e => e.EndpointGroupId == 5)), Times.Exactly(2));
    }

    /// <summary>ExecuteAsync_WithNegotiateSecurityScheme_SetsNegotiateAuthenticationType</summary>
    [Fact]
    public async Task ExecuteAsync_WithNegotiateSecurityScheme_SetsNegotiateAuthenticationType()
    {
        var (service, appRepoMock, swaggerProviderMock, endpointRepoMock, _, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithNegotiateAuth(("/api/items", HttpMethod.Get, "getItems")));

        var addedEndpoints = new List<Endpoint>();
        endpointRepoMock.Setup(r => r.AddEndpointAsync(It.IsAny<Endpoint>()))
            .Callback<Endpoint>(e => addedEndpoints.Add(e))
            .ReturnsAsync((Endpoint e) => e);

        await service.RunAsync();

        Assert.Single(addedEndpoints);
        Assert.Equal(AuthenticationType.Negotiate, addedEndpoints[0].AuthenticationType);
    }

    /// <summary>ExecuteAsync_WithoutNegotiateSecurityScheme_SetsNoneAuthenticationType</summary>
    [Fact]
    public async Task ExecuteAsync_WithoutNegotiateSecurityScheme_SetsNoneAuthenticationType()
    {
        var (service, appRepoMock, swaggerProviderMock, endpointRepoMock, _, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithPaths(("/api/items", HttpMethod.Get, "getItems")));

        var addedEndpoints = new List<Endpoint>();
        endpointRepoMock.Setup(r => r.AddEndpointAsync(It.IsAny<Endpoint>()))
            .Callback<Endpoint>(e => addedEndpoints.Add(e))
            .ReturnsAsync((Endpoint e) => e);

        await service.RunAsync();

        Assert.Single(addedEndpoints);
        Assert.Equal(AuthenticationType.None, addedEndpoints[0].AuthenticationType);
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

    /// <summary>ExecuteAsync_WithBearerTokenExtension_SavesBearerToken</summary>
    [Fact]
    public async Task ExecuteAsync_WithBearerTokenExtension_SavesBearerToken()
    {
        var (service, appRepoMock, swaggerProviderMock, endpointRepoMock, _, credentialServiceMock) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithBearerToken("/api/items", HttpMethod.Get, "getItems", "my-secret-token"));

        await service.RunAsync();

        credentialServiceMock.Verify(
            c => c.SavePassword(
                CredentialTargetHelper.Build(1, AuthenticationType.BearerToken),
                string.Empty,
                "my-secret-token"),
            Times.Once);
    }
}
