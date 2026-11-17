using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Data;
using Schnittstellenzentrale.Infrastructure.Repositories;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Integration;

/// <summary>ApplicationRepositoryIntegrationTests</summary>
public class ApplicationRepositoryIntegrationTests
{
    private static async Task ExecuteWithContextAsync(Func<ApplicationRepository, Task> test)
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        {
            await test(new ApplicationRepository(factory));
        }
    }

    /// <summary>GetApplications_WithStorageModeUser_ReturnsOnlyUserData</summary>
    [Fact]
    public async Task GetApplications_WithStorageModeUser_ReturnsOnlyUserData()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            await repo.AddApplicationAsync(new Core.Models.Application { Name = "TeamApp", BaseUrl = "http://team", Owner = "DOMAIN\\user2" });
            await repo.AddApplicationAsync(new Core.Models.Application { Name = "UserApp", BaseUrl = "http://user", Owner = "DOMAIN\\user1" });

            var result = await repo.GetApplicationsAsync(StorageMode.User, "DOMAIN\\user1");

            Assert.Single(result);
            Assert.Equal("UserApp", result[0].Name);
        });
    }

    /// <summary>GetApplications_WithStorageModeTeam_ReturnsTeamData</summary>
    [Fact]
    public async Task GetApplications_WithStorageModeTeam_ReturnsTeamData()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            await repo.AddApplicationAsync(new Core.Models.Application { Name = "App1", BaseUrl = "http://app1", Owner = "DOMAIN\\user1" });
            await repo.AddApplicationAsync(new Core.Models.Application { Name = "App2", BaseUrl = "http://app2", Owner = "DOMAIN\\user2" });

            var result = await repo.GetApplicationsAsync(StorageMode.Team, "DOMAIN\\user1");

            Assert.Equal(2, result.Count);
        });
    }

    /// <summary>AddGroup_PersistsNewGroup</summary>
    [Fact]
    public async Task AddGroup_PersistsNewGroup()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            await repo.AddGroupAsync(new ApplicationGroup { Name = "Testgruppe" });

            var result = await repo.GetGroupsAsync(StorageMode.Team, string.Empty);

            Assert.Single(result);
            Assert.Equal("Testgruppe", result[0].Name);
        });
    }

    /// <summary>AddApplication_WithGroup_PersistsApplication</summary>
    [Fact]
    public async Task AddApplication_WithGroup_PersistsApplication()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "Gruppe" });
            await repo.AddApplicationAsync(new Core.Models.Application { Name = "App", BaseUrl = "http://app", ApplicationGroupId = group.Id });

            var result = await repo.GetApplicationsAsync(StorageMode.Team, string.Empty);

            Assert.Single(result);
            Assert.Equal("App", result[0].Name);
            Assert.Equal(group.Id, result[0].ApplicationGroupId);
        });
    }

    /// <summary>AddApplication_WithoutGroup_PersistsUngroupedApplication</summary>
    [Fact]
    public async Task AddApplication_WithoutGroup_PersistsUngroupedApplication()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            await repo.AddApplicationAsync(new Core.Models.Application { Name = "OhneGruppe", BaseUrl = "http://app" });

            var result = await repo.GetUngroupedApplicationsAsync(StorageMode.Team, string.Empty);

            Assert.Single(result);
            Assert.Equal("OhneGruppe", result[0].Name);
        });
    }

    /// <summary>AddApplication_WithStorageModeUser_FiltersToCurrentOwner</summary>
    [Fact]
    public async Task AddApplication_WithStorageModeUser_FiltersToCurrentOwner()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            await repo.AddApplicationAsync(new Core.Models.Application { Name = "UserApp", BaseUrl = "http://app", Owner = "DOMAIN\\user1" });

            var result = await repo.GetApplicationsAsync(StorageMode.User, "DOMAIN\\user1");

            Assert.Single(result);
            Assert.Equal("DOMAIN\\user1", result[0].Owner);
        });
    }

    /// <summary>UpdateGroup_RenamesGroup</summary>
    [Fact]
    public async Task UpdateGroup_RenamesGroup()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "Alter Name" });
            group.Name = "Neuer Name";

            await repo.UpdateGroupAsync(group);

            var result = await repo.GetGroupsAsync(StorageMode.Team, string.Empty);
            Assert.Single(result);
            Assert.Equal("Neuer Name", result[0].Name);
        });
    }

    /// <summary>UpdateApplication_ChangesGroup</summary>
    [Fact]
    public async Task UpdateApplication_ChangesGroup()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            var group1 = await repo.AddGroupAsync(new ApplicationGroup { Name = "Gruppe1" });
            var group2 = await repo.AddGroupAsync(new ApplicationGroup { Name = "Gruppe2" });
            var app = await repo.AddApplicationAsync(new Core.Models.Application { Name = "App", BaseUrl = "http://app", ApplicationGroupId = group1.Id });

            app.ApplicationGroupId = group2.Id;
            await repo.UpdateApplicationAsync(app);

            var result = await repo.GetApplicationsAsync(StorageMode.Team, string.Empty);
            Assert.Single(result);
            Assert.Equal(group2.Id, result[0].ApplicationGroupId);
        });
    }

    /// <summary>UpdateApplication_SetsGroupToNull</summary>
    [Fact]
    public async Task UpdateApplication_SetsGroupToNull()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "Gruppe" });
            var app = await repo.AddApplicationAsync(new Core.Models.Application { Name = "App", BaseUrl = "http://app", ApplicationGroupId = group.Id });

            app.ApplicationGroupId = null;
            await repo.UpdateApplicationAsync(app);

            var result = await repo.GetUngroupedApplicationsAsync(StorageMode.Team, string.Empty);
            Assert.Single(result);
            Assert.Null(result[0].ApplicationGroupId);
        });
    }

    /// <summary>DeleteGroup_SetsMemberApplicationsGroupless</summary>
    [Fact]
    public async Task DeleteGroup_SetsMemberApplicationsGroupless()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "Gruppe" });
            await repo.AddApplicationAsync(new Core.Models.Application { Name = "App", BaseUrl = "http://app", ApplicationGroupId = group.Id });

            await repo.DeleteGroupAsync(group.Id);

            var ungrouped = await repo.GetUngroupedApplicationsAsync(StorageMode.Team, string.Empty);
            Assert.Single(ungrouped);
            Assert.Null(ungrouped[0].ApplicationGroupId);
        });
    }

    /// <summary>DeleteApplication_RemovesApplication</summary>
    [Fact]
    public async Task DeleteApplication_RemovesApplication()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            var app = await repo.AddApplicationAsync(new Core.Models.Application { Name = "App", BaseUrl = "http://app" });

            await repo.DeleteApplicationAsync(app.Id);

            var result = await repo.GetApplicationsAsync(StorageMode.Team, string.Empty);
            Assert.Empty(result);
        });
    }

    /// <summary>DeleteGroup_WithApplicationsDeletedFirst_RemovesGroupAndApplications</summary>
    [Fact]
    public async Task DeleteGroup_WithApplicationsDeletedFirst_RemovesGroupAndApplications()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "Gruppe" });
            var app1 = await repo.AddApplicationAsync(new Core.Models.Application { Name = "App1", BaseUrl = "http://app1", ApplicationGroupId = group.Id });
            var app2 = await repo.AddApplicationAsync(new Core.Models.Application { Name = "App2", BaseUrl = "http://app2", ApplicationGroupId = group.Id });

            await repo.DeleteApplicationAsync(app1.Id);
            await repo.DeleteApplicationAsync(app2.Id);
            await repo.DeleteGroupAsync(group.Id);

            var applications = await repo.GetApplicationsAsync(StorageMode.Team, string.Empty);
            var groups = await repo.GetGroupsAsync(StorageMode.Team, string.Empty);
            Assert.Empty(applications);
            Assert.Empty(groups);
        });
    }

    /// <summary>GetGroups_WithStorageModeUser_ReturnsOnlyGroupsWithOwnedApplications</summary>
    [Fact]
    public async Task GetGroups_WithStorageModeUser_ReturnsOnlyGroupsWithOwnedApplications()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            var groupOwned = await repo.AddGroupAsync(new ApplicationGroup { Name = "Eigene Gruppe" });
            var groupOther = await repo.AddGroupAsync(new ApplicationGroup { Name = "Fremde Gruppe" });
            await repo.AddApplicationAsync(new Core.Models.Application { Name = "EigeneApp", BaseUrl = "http://owned", Owner = "DOMAIN\\user1", ApplicationGroupId = groupOwned.Id });
            await repo.AddApplicationAsync(new Core.Models.Application { Name = "FremdeApp", BaseUrl = "http://other", Owner = "DOMAIN\\user2", ApplicationGroupId = groupOther.Id });

            var result = await repo.GetGroupsAsync(StorageMode.User, "DOMAIN\\user1");

            Assert.Single(result);
            Assert.Equal("Eigene Gruppe", result[0].Name);
        });
    }

    /// <summary>GetUngroupedApplications_WithStorageModeUser_ReturnsOnlyOwnUngroupedApplications</summary>
    [Fact]
    public async Task GetUngroupedApplications_WithStorageModeUser_ReturnsOnlyOwnUngroupedApplications()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            await repo.AddApplicationAsync(new Core.Models.Application { Name = "EigeneApp", BaseUrl = "http://own", Owner = "DOMAIN\\user1" });
            await repo.AddApplicationAsync(new Core.Models.Application { Name = "FremdeApp", BaseUrl = "http://other", Owner = "DOMAIN\\user2" });
            var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "Gruppe" });
            await repo.AddApplicationAsync(new Core.Models.Application { Name = "GruppierteApp", BaseUrl = "http://grouped", Owner = "DOMAIN\\user1", ApplicationGroupId = group.Id });

            var result = await repo.GetUngroupedApplicationsAsync(StorageMode.User, "DOMAIN\\user1");

            Assert.Single(result);
            Assert.Equal("EigeneApp", result[0].Name);
            Assert.Equal("DOMAIN\\user1", result[0].Owner);
            Assert.Null(result[0].ApplicationGroupId);
        });
    }

    /// <summary>UpdateApplication_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException</summary>
    [Fact]
    public async Task UpdateApplication_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException()
    {
        await TestHelpers.ExecuteWithTwoContextsAsync(async (repo1, repo2) =>
        {
            var app = await repo1.AddApplicationAsync(new Core.Models.Application { Name = "App", BaseUrl = "http://app" });
            var originalRowVersion = app.RowVersion;

            app.Name = "Erste Änderung";
            await repo1.UpdateApplicationAsync(app);

            var staleApp = new Core.Models.Application
            {
                Id = app.Id,
                Name = "Zweite Änderung",
                BaseUrl = app.BaseUrl,
                RowVersion = originalRowVersion
            };

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                () => repo2.UpdateApplicationAsync(staleApp));
        });
    }

    /// <summary>UpdateGroup_WithNewInstanceAfterAdd_ShouldRenameGroup</summary>
    [Fact]
    public async Task UpdateGroup_WithNewInstanceAfterAdd_ShouldRenameGroup()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "Original" });

            // Simulates what RenameGroupDialog does: new instance with same Id + RowVersion
            var renamed = new ApplicationGroup { Id = group.Id, Name = "Umbenannt", RowVersion = group.RowVersion };

            await repo.UpdateGroupAsync(renamed);

            var result = await repo.GetGroupsAsync(StorageMode.Team, string.Empty);
            Assert.Single(result);
            Assert.Equal("Umbenannt", result[0].Name);
        });
    }

    /// <summary>UpdateGroup_WithNewInstanceAfterGetGroups_ShouldRenameGroup</summary>
    [Fact]
    public async Task UpdateGroup_WithNewInstanceAfterGetGroups_ShouldRenameGroup()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "Original" });
            await repo.GetGroupsAsync(StorageMode.Team, string.Empty);

            var renamed = new ApplicationGroup { Id = group.Id, Name = "Umbenannt", RowVersion = group.RowVersion };

            await repo.UpdateGroupAsync(renamed);

            var result = await repo.GetGroupsAsync(StorageMode.Team, string.Empty);
            Assert.Single(result);
            Assert.Equal("Umbenannt", result[0].Name);
        });
    }

    /// <summary>UpdateApplication_WithNewInstanceAfterAdd_ShouldUpdateApplication</summary>
    [Fact]
    public async Task UpdateApplication_WithNewInstanceAfterAdd_ShouldUpdateApplication()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            var app = await repo.AddApplicationAsync(new Core.Models.Application { Name = "Original", BaseUrl = "http://app" });

            // Simulates what ApplicationGroupTree does: new instance with same Id
            var updated = new Core.Models.Application
            {
                Id = app.Id,
                Name = "Geändert",
                BaseUrl = "http://app",
                RowVersion = app.RowVersion
            };

            await repo.UpdateApplicationAsync(updated);

            var result = await repo.GetApplicationsAsync(StorageMode.Team, string.Empty);
            Assert.Single(result);
            Assert.Equal("Geändert", result[0].Name);
        });
    }

    /// <summary>UpdateApplication_WithNewInstanceAfterGetApplications_ShouldUpdateApplication</summary>
    [Fact]
    public async Task UpdateApplication_WithNewInstanceAfterGetApplications_ShouldUpdateApplication()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            var app = await repo.AddApplicationAsync(new Core.Models.Application { Name = "Original", BaseUrl = "http://app" });
            await repo.GetApplicationsAsync(StorageMode.Team, string.Empty);

            var updated = new Core.Models.Application
            {
                Id = app.Id,
                Name = "Geändert",
                BaseUrl = "http://app",
                RowVersion = app.RowVersion
            };

            await repo.UpdateApplicationAsync(updated);

            var result = await repo.GetApplicationsAsync(StorageMode.Team, string.Empty);
            Assert.Single(result);
            Assert.Equal("Geändert", result[0].Name);
        });
    }

    /// <summary>AusGruppeEntfernen_NachGetGroupsAsync_PersistiertInDb</summary>
    [Fact]
    public async Task AusGruppeEntfernen_NachGetGroupsAsync_PersistiertInDb()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "Gruppe" });
            await repo.AddApplicationAsync(new Core.Models.Application { Name = "App", BaseUrl = "http://app", ApplicationGroupId = group.Id });

            // Exakt so wie ApplicationGroupTree: lädt via GetGroupsAsync (mit Navigation-Fixup)
            var groups = await repo.GetGroupsAsync(StorageMode.Team, string.Empty);
            var app = groups[0].Applications.First();

            // Exakt so wie OnRemoveFromGroupRequested: setzt nur die FK, nicht die Navigation
            app.ApplicationGroupId = null;
            await repo.UpdateApplicationAsync(app);

            var ungrouped = await repo.GetUngroupedApplicationsAsync(StorageMode.Team, string.Empty);
            Assert.Single(ungrouped);
            Assert.Null(ungrouped[0].ApplicationGroupId);
        });
    }

    /// <summary>UpdateGroup_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException</summary>
    [Fact]
    public async Task UpdateGroup_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException()
    {
        await TestHelpers.ExecuteWithTwoContextsAsync(async (repo1, repo2) =>
        {
            var group = await repo1.AddGroupAsync(new ApplicationGroup { Name = "Gruppe" });
            var originalRowVersion = group.RowVersion;

            group.Name = "Erste Änderung";
            await repo1.UpdateGroupAsync(group);

            var staleGroup = new ApplicationGroup
            {
                Id = group.Id,
                Name = "Zweite Änderung",
                RowVersion = originalRowVersion
            };

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                () => repo2.UpdateGroupAsync(staleGroup));
        });
    }

    /// <summary>DeleteGroup_WithDeleteMode_DeletesApplicationsEndpointsAndGroups</summary>
    [Fact]
    public async Task DeleteGroup_WithDeleteMode_DeletesApplicationsEndpointsAndGroups()
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        {
            var repo = new ApplicationRepository(factory);
            var endpointRepo = new EndpointRepository(factory);

            var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "Gruppe" });
            var app = await repo.AddApplicationAsync(new Core.Models.Application { Name = "App", BaseUrl = "http://app", ApplicationGroupId = group.Id });
            var parentGroup = await endpointRepo.AddEndpointGroupAsync(new EndpointGroup { Name = "Elterngruppe", ApplicationId = app.Id });
            var childGroup = await endpointRepo.AddEndpointGroupAsync(new EndpointGroup { Name = "Kindgruppe", ApplicationId = app.Id, ParentGroupId = parentGroup.Id });
            await endpointRepo.AddEndpointAsync(new Core.Models.Endpoint { Name = "Ep1", RelativePath = "/e1", ApplicationId = app.Id, EndpointGroupId = childGroup.Id });
            await endpointRepo.AddEndpointAsync(new Core.Models.Endpoint { Name = "Ep2", RelativePath = "/e2", ApplicationId = app.Id });

            await repo.DeleteGroupAsync(group.Id, ApplicationGroupDeleteMode.Delete);

            Assert.Empty(await repo.GetGroupsAsync(StorageMode.Team, string.Empty));
            Assert.Empty(await repo.GetApplicationsAsync(StorageMode.Team, string.Empty));
            Assert.Empty(await endpointRepo.GetEndpointGroupsAsync(app.Id));
            Assert.Empty(await endpointRepo.GetEndpointsAsync(app.Id));
        }
    }

    /// <summary>DeleteGroup_WithDetachMode_KeepsApplicationsUngrouped</summary>
    [Fact]
    public async Task DeleteGroup_WithDetachMode_KeepsApplicationsUngrouped()
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        {
            var repo = new ApplicationRepository(factory);
            var endpointRepo = new EndpointRepository(factory);

            var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "Gruppe" });
            var app = await repo.AddApplicationAsync(new Core.Models.Application { Name = "App", BaseUrl = "http://app", ApplicationGroupId = group.Id });
            var endpointGroup = await endpointRepo.AddEndpointGroupAsync(new EndpointGroup { Name = "Gruppe1", ApplicationId = app.Id });
            await endpointRepo.AddEndpointAsync(new Core.Models.Endpoint { Name = "Ep1", RelativePath = "/e1", ApplicationId = app.Id, EndpointGroupId = endpointGroup.Id });

            await repo.DeleteGroupAsync(group.Id, ApplicationGroupDeleteMode.Detach);

            Assert.Empty(await repo.GetGroupsAsync(StorageMode.Team, string.Empty));

            var ungrouped = await repo.GetUngroupedApplicationsAsync(StorageMode.Team, string.Empty);
            Assert.Single(ungrouped);
            Assert.Equal(app.Id, ungrouped[0].Id);
            Assert.Null(ungrouped[0].ApplicationGroupId);

            // Endpunkte und Endpunktgruppen der erhaltenen Anwendung bleiben erhalten.
            Assert.Single(await endpointRepo.GetEndpointGroupsAsync(app.Id));
            Assert.Single(await endpointRepo.GetEndpointsAsync(app.Id));
        }
    }

    /// <summary>DeleteGroup_WithDeleteMode_OnSaveFailure_RollsBackEverything</summary>
    [Fact]
    public async Task DeleteGroup_WithDeleteMode_OnSaveFailure_RollsBackEverything()
    {
        await ExecuteWithFailingSaveContextAsync(async (repo, failingRepo, endpointRepo) =>
        {
            var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "Gruppe" });
            var app = await repo.AddApplicationAsync(new Core.Models.Application { Name = "App", BaseUrl = "http://app", ApplicationGroupId = group.Id });
            await endpointRepo.AddEndpointAsync(new Core.Models.Endpoint { Name = "Ep1", RelativePath = "/e1", ApplicationId = app.Id });

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => failingRepo.DeleteGroupAsync(group.Id, ApplicationGroupDeleteMode.Delete));

            Assert.Single(await repo.GetGroupsAsync(StorageMode.Team, string.Empty));
            Assert.Single(await repo.GetApplicationsAsync(StorageMode.Team, string.Empty));
            Assert.Single(await endpointRepo.GetEndpointsAsync(app.Id));
        });
    }

    /// <summary>DeleteGroup_WithDetachMode_OnSaveFailure_RollsBackEverything</summary>
    [Fact]
    public async Task DeleteGroup_WithDetachMode_OnSaveFailure_RollsBackEverything()
    {
        await ExecuteWithFailingSaveContextAsync(async (repo, failingRepo, endpointRepo) =>
        {
            var group = await repo.AddGroupAsync(new ApplicationGroup { Name = "Gruppe" });
            var app = await repo.AddApplicationAsync(new Core.Models.Application { Name = "App", BaseUrl = "http://app", ApplicationGroupId = group.Id });

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => failingRepo.DeleteGroupAsync(group.Id, ApplicationGroupDeleteMode.Detach));

            Assert.Single(await repo.GetGroupsAsync(StorageMode.Team, string.Empty));
            var applications = await repo.GetApplicationsAsync(StorageMode.Team, string.Empty);
            Assert.Single(applications);
            Assert.Equal(group.Id, applications[0].ApplicationGroupId);
        });
    }

    private static async Task ExecuteWithFailingSaveContextAsync(
        Func<ApplicationRepository, ApplicationRepository, EndpointRepository, Task> test)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (connection)
        {
            using (var initContext = new AppDbContext(options))
            {
                initContext.Database.EnsureCreated();
            }

            var factory = new FixedOptionsDbContextFactory(options);
            var failingFactory = new FailingSaveDbContextFactory(options);
            await test(
                new ApplicationRepository(factory),
                new ApplicationRepository(failingFactory),
                new EndpointRepository(factory));
        }
    }

    private sealed class FixedOptionsDbContextFactory(DbContextOptions<AppDbContext> options)
        : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => new(options);
    }

    private sealed class FailingSaveDbContextFactory(DbContextOptions<AppDbContext> options)
        : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => new FailingSaveDbContext(options);
    }

    private sealed class FailingSaveDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
    {
        /// <summary>Anzahl der aufgerufenen Speicherversuche (zur optionalen Verifikation im Test).</summary>
        public int SaveAttempts { get; private set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveAttempts++;
            return Task.FromException<int>(new InvalidOperationException("Simulierter Fehler beim Speichern."));
        }
    }
}
