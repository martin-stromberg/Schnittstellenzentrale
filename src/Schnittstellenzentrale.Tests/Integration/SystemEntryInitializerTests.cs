using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Infrastructure.Repositories;
using Schnittstellenzentrale.Tests.Helpers;

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

        var cleanup = new CompositeDisposable(connection);
        return (provider, cleanup);
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

    private sealed class CompositeDisposable : IDisposable
    {
        private readonly IDisposable[] _disposables;

        /// <summary>Initialisiert CompositeDisposable.</summary>
        public CompositeDisposable(params IDisposable[] disposables)
        {
            _disposables = disposables;
        }

        /// <summary>Dispose</summary>
        public void Dispose()
        {
            foreach (var d in _disposables)
                d.Dispose();
        }
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
    }
}
