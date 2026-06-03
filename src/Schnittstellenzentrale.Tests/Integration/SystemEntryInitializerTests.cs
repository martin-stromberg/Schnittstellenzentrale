using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Moq;
using System.Text.Json.Nodes;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Helpers;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Infrastructure.Data;
using Schnittstellenzentrale.Infrastructure.Repositories;
using Schnittstellenzentrale.Tests.Helpers;
using Swashbuckle.AspNetCore.Swagger;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Schnittstellenzentrale.Tests.Integration;

/// <summary>SystemEntryInitializerTests</summary>
public class SystemEntryInitializerTests
{
    private static (IServiceProvider Services, IDisposable Cleanup) BuildServices()
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContext();

        var services = new ServiceCollection();
        services.AddScoped<IApplicationRepository>(sp => new ApplicationRepository(factory));
        var provider = services.BuildServiceProvider();

        return (provider, connection);
    }

    private static IConfiguration BuildConfiguration(string? baseUrl)
    {
        var data = new Dictionary<string, string?>();
        if (baseUrl != null)
            data["Api:BaseUrl"] = baseUrl;
        return new ConfigurationBuilder().AddInMemoryCollection(data).Build();
    }

    /// <summary>InitializeAsync_WhenGroupAndApplicationMissing_CreatesBoth</summary>
    [Fact]
    public async Task InitializeAsync_WhenGroupAndApplicationMissing_CreatesBoth()
    {
        var (services, cleanup) = BuildServices();
        using (cleanup)
        {
            var config = BuildConfiguration("https://localhost:5001");
            await SystemEntryInitializer.InitializeAsync(services, config);

            var repo = services.GetRequiredService<IApplicationRepository>();
            var group = await repo.GetSystemGroupAsync();

            Assert.NotNull(group);
            Assert.True(group.IsSystem);
            Assert.Equal("Schnittstellenzentrale", group.Name);
            Assert.Single(group.Applications, a => a.IsSystem);
            var app = group.Applications.First(a => a.IsSystem);
            Assert.Equal("https://localhost:5001", app.BaseUrl);
            Assert.Equal("https://localhost:5001/swagger/v1/swagger.json", app.InterfaceUrl);
        }
    }

    /// <summary>InitializeAsync_WhenGroupExistsButApplicationMissing_CreatesApplication</summary>
    [Fact]
    public async Task InitializeAsync_WhenGroupExistsButApplicationMissing_CreatesApplication()
    {
        var (services, cleanup) = BuildServices();
        using (cleanup)
        {
            var config = BuildConfiguration("https://localhost:5001");
            var repo = services.GetRequiredService<IApplicationRepository>();
            await repo.AddGroupAsync(new Schnittstellenzentrale.Core.Models.ApplicationGroup
            {
                Name = "Schnittstellenzentrale",
                IsSystem = true
            });

            await SystemEntryInitializer.InitializeAsync(services, config);

            var group = await repo.GetSystemGroupAsync();
            Assert.NotNull(group);
            Assert.Single(group.Applications, a => a.IsSystem);
        }
    }

    /// <summary>InitializeAsync_WhenUrlDiffers_UpdatesBaseUrlAndInterfaceUrl</summary>
    [Fact]
    public async Task InitializeAsync_WhenUrlDiffers_UpdatesBaseUrlAndInterfaceUrl()
    {
        var (services, cleanup) = BuildServices();
        using (cleanup)
        {
            var config = BuildConfiguration("https://localhost:5001");
            await SystemEntryInitializer.InitializeAsync(services, config);

            var newConfig = BuildConfiguration("https://localhost:7000");
            await SystemEntryInitializer.InitializeAsync(services, newConfig);

            var repo = services.GetRequiredService<IApplicationRepository>();
            var group = await repo.GetSystemGroupAsync();
            var app = group!.Applications.First(a => a.IsSystem);
            Assert.Equal("https://localhost:7000", app.BaseUrl);
            Assert.Equal("https://localhost:7000/swagger/v1/swagger.json", app.InterfaceUrl);
        }
    }

    /// <summary>InitializeAsync_WhenUrlMatches_MakesNoChanges</summary>
    [Fact]
    public async Task InitializeAsync_WhenUrlMatches_MakesNoChanges()
    {
        var (services, cleanup) = BuildServices();
        using (cleanup)
        {
            var config = BuildConfiguration("https://localhost:5001");
            await SystemEntryInitializer.InitializeAsync(services, config);
            await SystemEntryInitializer.InitializeAsync(services, config);

            var repo = services.GetRequiredService<IApplicationRepository>();
            var group = await repo.GetSystemGroupAsync();
            Assert.Single(group!.Applications, a => a.IsSystem);
        }
    }

    /// <summary>InitializeAsync_IsIdempotent_OnRepeatedCall</summary>
    [Fact]
    public async Task InitializeAsync_IsIdempotent_OnRepeatedCall()
    {
        var (services, cleanup) = BuildServices();
        using (cleanup)
        {
            var config = BuildConfiguration("https://localhost:5001");
            await SystemEntryInitializer.InitializeAsync(services, config);
            await SystemEntryInitializer.InitializeAsync(services, config);
            await SystemEntryInitializer.InitializeAsync(services, config);

            var repo = services.GetRequiredService<IApplicationRepository>();
            var group = await repo.GetSystemGroupAsync();
            Assert.NotNull(group);
            Assert.Single(group.Applications, a => a.IsSystem);
        }
    }

    /// <summary>InitializeAsync_WhenDbThrows_DoesNotPropagateException</summary>
    [Fact]
    public async Task InitializeAsync_WhenDbThrows_DoesNotPropagateException()
    {
        var config = BuildConfiguration("https://localhost:5001");

        var services = new ServiceCollection();
        services.AddScoped<IApplicationRepository, ThrowingApplicationRepository>();
        var provider = services.BuildServiceProvider();

        var exception = await Record.ExceptionAsync(() =>
            SystemEntryInitializer.InitializeAsync(provider, config));

        Assert.Null(exception);
    }

    /// <summary>InitializeAsync_WhenBaseUrlMissing_SkipsAndLogs</summary>
    [Fact]
    public async Task InitializeAsync_WhenBaseUrlMissing_SkipsAndLogs()
    {
        var (services, cleanup) = BuildServices();
        using (cleanup)
        {
            var config = BuildConfiguration(null);

            var exception = await Record.ExceptionAsync(() =>
                SystemEntryInitializer.InitializeAsync(services, config));

            Assert.Null(exception);

            var repo = services.GetRequiredService<IApplicationRepository>();
            var group = await repo.GetSystemGroupAsync();
            Assert.Null(group);
        }
    }

    private static (TestableSyncServiceForInitTest Service, Mock<ICredentialService> CredentialServiceMock) CreateSyncServiceWithInMemoryDb(
        IDbContextFactory<Schnittstellenzentrale.Infrastructure.Data.AppDbContext> factory,
        ISwaggerProvider swaggerProvider)
    {
        var endpointRepo = new EndpointRepository(factory);
        var appRepo = new ApplicationRepository(factory);
        var credentialServiceMock = new Mock<ICredentialService>();

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var swaggerImportService = new Schnittstellenzentrale.Infrastructure.Services.SwaggerImportService(
            httpClientFactoryMock.Object,
            endpointRepo,
            credentialServiceMock.Object,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<Schnittstellenzentrale.Infrastructure.Services.SwaggerImportService>.Instance);

        var services = new ServiceCollection();
        services.AddSingleton<IApplicationRepository>(appRepo);
        services.AddSingleton<IEndpointRepository>(endpointRepo);
        services.AddSingleton<ISwaggerProvider>(swaggerProvider);
        services.AddSingleton<ISwaggerImportService>(swaggerImportService);
        var provider = services.BuildServiceProvider();

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(provider);
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        var loggerMock = new Mock<ILogger<SystemEndpointSyncService>>();

        var service = new TestableSyncServiceForInitTest(scopeFactoryMock.Object, loggerMock.Object);
        return (service, credentialServiceMock);
    }

    /// <summary>InitializeAsync_AndSyncService_CreatesEndpointsWithCorrectAuthorizationAndCredentials</summary>
    [Fact]
    public async Task InitializeAsync_AndSyncService_CreatesEndpointsWithCorrectAuthorizationAndCredentials()
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContext();
        using var cleanup = connection;

        var services = new ServiceCollection();
        services.AddScoped<IApplicationRepository>(sp => new ApplicationRepository(factory));
        var provider = services.BuildServiceProvider();

        var config = BuildConfiguration("https://localhost:5001");
        await SystemEntryInitializer.InitializeAsync(provider, config);

        var swaggerProviderMock = new Mock<ISwaggerProvider>();
        var negotiateOperation = new OpenApiOperation { OperationId = "getNegotiate" };
        (negotiateOperation.Security ??= []).Add(new OpenApiSecurityRequirement
        {
            { new OpenApiSecuritySchemeReference("Negotiate", null), new List<string>() }
        });
        var bearerTokenExtension = new JsonNodeExtension(
            System.Text.Json.Nodes.JsonValue.Create("test-bearer-token"));
        var bearerOperation = new OpenApiOperation
        {
            OperationId = "getBearerToken",
            Extensions = new Dictionary<string, IOpenApiExtension>
            {
                ["x-sz-bearer-token"] = bearerTokenExtension
            }
        };

        var paths = new OpenApiPaths();
        var negotiatePath = new OpenApiPathItem();
        negotiatePath.AddOperation(HttpMethod.Get, negotiateOperation);
        paths["/api/negotiate"] = negotiatePath;

        var bearerPath = new OpenApiPathItem();
        bearerPath.AddOperation(HttpMethod.Get, bearerOperation);
        paths["/api/bearer"] = bearerPath;

        var document = new OpenApiDocument { Paths = paths };
        swaggerProviderMock.Setup(p => p.GetSwagger("v1", null, null)).Returns(document);

        var (syncService, credentialServiceMock) = CreateSyncServiceWithInMemoryDb(factory, swaggerProviderMock.Object);

        await syncService.RunAsync();

        var endpointRepo = new EndpointRepository(factory);
        var appRepo = new ApplicationRepository(factory);
        var group = await appRepo.GetSystemGroupAsync();
        Assert.NotNull(group);
        var systemApp = group.Applications.First(a => a.IsSystem);

        var endpoints = await endpointRepo.GetEndpointsAsync(systemApp.Id);
        Assert.NotEmpty(endpoints);
        Assert.Contains(endpoints, e => e.AuthenticationType == AuthenticationType.Negotiate);

        credentialServiceMock.Verify(
            c => c.SavePassword(
                CredentialTargetHelper.Build(systemApp.Id, AuthenticationType.BearerToken),
                string.Empty,
                "test-bearer-token"),
            Times.Once);
    }

    private sealed class TestableSyncServiceForInitTest(IServiceScopeFactory scopeFactory, ILogger<SystemEndpointSyncService> logger)
        : SystemEndpointSyncService(scopeFactory, logger)
    {
        public Task RunAsync() => ExecuteAsync(System.Threading.CancellationToken.None);
    }

    private sealed class ThrowingApplicationRepository : IApplicationRepository
    {
        /// <summary>GetSystemGroupAsync</summary>
        public Task<Schnittstellenzentrale.Core.Models.ApplicationGroup?> GetSystemGroupAsync() =>
            throw new InvalidOperationException("Simulierter Datenbankfehler");

        /// <summary>GetGroupsAsync</summary>
        public Task<System.Collections.Generic.IList<Schnittstellenzentrale.Core.Models.ApplicationGroup>> GetGroupsAsync(Schnittstellenzentrale.Core.Enums.StorageMode storageMode, string owner) => throw new NotImplementedException();
        /// <summary>GetGroupByIdAsync</summary>
        public Task<Schnittstellenzentrale.Core.Models.ApplicationGroup?> GetGroupByIdAsync(int id) => throw new NotImplementedException();
        /// <summary>AddGroupAsync</summary>
        public Task<Schnittstellenzentrale.Core.Models.ApplicationGroup> AddGroupAsync(Schnittstellenzentrale.Core.Models.ApplicationGroup group) => throw new NotImplementedException();
        /// <summary>UpdateGroupAsync</summary>
        public Task<Schnittstellenzentrale.Core.Models.ApplicationGroup> UpdateGroupAsync(Schnittstellenzentrale.Core.Models.ApplicationGroup group) => throw new NotImplementedException();
        /// <summary>DeleteGroupAsync</summary>
        public Task DeleteGroupAsync(int id) => throw new NotImplementedException();
        /// <summary>GetApplicationsAsync</summary>
        public Task<System.Collections.Generic.IList<Schnittstellenzentrale.Core.Models.Application>> GetApplicationsAsync(Schnittstellenzentrale.Core.Enums.StorageMode storageMode, string owner) => throw new NotImplementedException();
        /// <summary>GetUngroupedApplicationsAsync</summary>
        public Task<System.Collections.Generic.IList<Schnittstellenzentrale.Core.Models.Application>> GetUngroupedApplicationsAsync(Schnittstellenzentrale.Core.Enums.StorageMode storageMode, string owner) => throw new NotImplementedException();
        /// <summary>GetApplicationByIdAsync</summary>
        public Task<Schnittstellenzentrale.Core.Models.Application?> GetApplicationByIdAsync(int id) => throw new NotImplementedException();
        /// <summary>AddApplicationAsync</summary>
        public Task<Schnittstellenzentrale.Core.Models.Application> AddApplicationAsync(Schnittstellenzentrale.Core.Models.Application application) => throw new NotImplementedException();
        /// <summary>UpdateApplicationAsync</summary>
        public Task<Schnittstellenzentrale.Core.Models.Application> UpdateApplicationAsync(Schnittstellenzentrale.Core.Models.Application application) => throw new NotImplementedException();
        /// <summary>DeleteApplicationAsync</summary>
        public Task DeleteApplicationAsync(int id) => throw new NotImplementedException();
        /// <summary>GetApplicationCountByGroupAsync</summary>
        public Task<int> GetApplicationCountByGroupAsync(int groupId) => throw new NotImplementedException();
        /// <summary>GetEndpointCountByGroupAsync</summary>
        public Task<int> GetEndpointCountByGroupAsync(int groupId) => throw new NotImplementedException();
    }
}
