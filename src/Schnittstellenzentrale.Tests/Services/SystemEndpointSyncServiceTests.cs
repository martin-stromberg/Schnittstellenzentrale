using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Moq;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Swashbuckle.AspNetCore.Swagger;
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

    private static (TestableSyncService Service, Mock<IApplicationRepository> AppRepoMock, Mock<ISwaggerProvider> SwaggerProviderMock, Mock<ISwaggerImportService> SwaggerImportServiceMock, Mock<ILogger<SystemEndpointSyncService>> LoggerMock) CreateService()
    {
        var appRepoMock = new Mock<IApplicationRepository>();
        var swaggerProviderMock = new Mock<ISwaggerProvider>();
        var swaggerImportServiceMock = new Mock<ISwaggerImportService>();

        swaggerImportServiceMock
            .Setup(s => s.SyncFromDocumentAsync(It.IsAny<OpenApiDocument>(), It.IsAny<int>()))
            .ReturnsAsync(new ImportDiff());

        // IEndpointRepository wird bewusst NICHT registriert: Der Dienst darf die
        // Persistenz nicht mehr direkt ansprechen — ein Auflösungsversuch würde
        // eine InvalidOperationException auslösen und den SyncFromDocumentAsync-Verify scheitern lassen.
        var services = new ServiceCollection();
        services.AddSingleton<IApplicationRepository>(appRepoMock.Object);
        services.AddSingleton<ISwaggerProvider>(swaggerProviderMock.Object);
        services.AddSingleton<ISwaggerImportService>(swaggerImportServiceMock.Object);
        var provider = services.BuildServiceProvider();

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(provider);
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        var loggerMock = new Mock<ILogger<SystemEndpointSyncService>>();
        var service = new TestableSyncService(scopeFactoryMock.Object, loggerMock.Object);

        return (service, appRepoMock, swaggerProviderMock, swaggerImportServiceMock, loggerMock);
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

    /// <summary>ExecuteAsync_CallsSyncFromDocumentAsync_WithDocumentAndApplicationId</summary>
    [Fact]
    public async Task ExecuteAsync_CallsSyncFromDocumentAsync_WithDocumentAndApplicationId()
    {
        var (service, appRepoMock, swaggerProviderMock, swaggerImportServiceMock, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        var document = DocumentWithPaths(("/items", HttpMethod.Get, "getItems"), ("/items", HttpMethod.Post, "createItem"));
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null)).Returns(document);

        await service.RunAsync();

        swaggerImportServiceMock.Verify(s => s.SyncFromDocumentAsync(
            It.Is<OpenApiDocument>(d => ReferenceEquals(d, document)), 1), Times.Once);
    }

    /// <summary>ExecuteAsync_WhenSystemGroupMissing_LogsWarningAndSkips</summary>
    [Fact]
    public async Task ExecuteAsync_WhenSystemGroupMissing_LogsWarningAndSkips()
    {
        var (service, appRepoMock, swaggerProviderMock, swaggerImportServiceMock, _) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync((ApplicationGroup?)null);

        var exception = await Record.ExceptionAsync(() => service.RunAsync());

        Assert.Null(exception);
        swaggerProviderMock.Verify(p => p.GetSwagger(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        swaggerImportServiceMock.Verify(s => s.SyncFromDocumentAsync(It.IsAny<OpenApiDocument>(), It.IsAny<int>()), Times.Never);
    }

    /// <summary>ExecuteAsync_WhenSystemAppMissing_LogsWarningAndSkips</summary>
    [Fact]
    public async Task ExecuteAsync_WhenSystemAppMissing_LogsWarningAndSkips()
    {
        var (service, appRepoMock, swaggerProviderMock, swaggerImportServiceMock, loggerMock) = CreateService();
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
        swaggerImportServiceMock.Verify(s => s.SyncFromDocumentAsync(It.IsAny<OpenApiDocument>(), It.IsAny<int>()), Times.Never);
    }

    /// <summary>ExecuteAsync_WhenSwaggerProviderThrows_LogsErrorAndDoesNotThrow</summary>
    [Fact]
    public async Task ExecuteAsync_WhenSwaggerProviderThrows_LogsErrorAndDoesNotThrow()
    {
        var (service, appRepoMock, swaggerProviderMock, swaggerImportServiceMock, loggerMock) = CreateService();
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
        swaggerImportServiceMock.Verify(s => s.SyncFromDocumentAsync(It.IsAny<OpenApiDocument>(), It.IsAny<int>()), Times.Never);
    }

    /// <summary>ExecuteAsync_WhenSyncFromDocumentAsyncThrows_LogsErrorAndDoesNotThrow</summary>
    [Fact]
    public async Task ExecuteAsync_WhenSyncFromDocumentAsyncThrows_LogsErrorAndDoesNotThrow()
    {
        var (service, appRepoMock, swaggerProviderMock, swaggerImportServiceMock, loggerMock) = CreateService();
        appRepoMock.Setup(r => r.GetSystemGroupAsync()).ReturnsAsync(SystemGroup());
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null))
            .Returns(DocumentWithPaths(("/items", HttpMethod.Get, "getItems")));
        swaggerImportServiceMock.Setup(s => s.SyncFromDocumentAsync(It.IsAny<OpenApiDocument>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Simulierter Fehler"));

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
    }
}
