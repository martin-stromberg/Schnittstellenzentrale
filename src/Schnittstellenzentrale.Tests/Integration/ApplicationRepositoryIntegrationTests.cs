using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Repositories;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Integration;

public class ApplicationRepositoryIntegrationTests
{
    private static async Task ExecuteWithContextAsync(Func<ApplicationRepository, Task> test)
    {
        var (context, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        await using (context)
        {
            await test(new ApplicationRepository(context));
        }
    }

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
}
