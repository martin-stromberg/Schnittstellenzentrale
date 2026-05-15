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
}
