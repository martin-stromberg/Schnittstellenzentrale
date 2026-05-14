using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Repositories;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Integration;

public class ApplicationRepositoryIntegrationTests
{
    [Fact]
    public async Task GetApplications_WithStorageModeUser_ReturnsOnlyUserData()
    {
        var (context, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        await using (context)
        {
            context.Applications.Add(new Core.Models.Application { Name = "TeamApp", BaseUrl = "http://team", Owner = "DOMAIN\\user2" });
            context.Applications.Add(new Core.Models.Application { Name = "UserApp", BaseUrl = "http://user", Owner = "DOMAIN\\user1" });
            await context.SaveChangesAsync();

            var repo = new ApplicationRepository(context);
            var result = await repo.GetApplicationsAsync(StorageMode.User, "DOMAIN\\user1");

            Assert.Single(result);
            Assert.Equal("UserApp", result[0].Name);
        }
    }

    [Fact]
    public async Task GetApplications_WithStorageModeTeam_ReturnsTeamData()
    {
        var (context, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        await using (context)
        {
            context.Applications.Add(new Core.Models.Application { Name = "App1", BaseUrl = "http://app1", Owner = "DOMAIN\\user1" });
            context.Applications.Add(new Core.Models.Application { Name = "App2", BaseUrl = "http://app2", Owner = "DOMAIN\\user2" });
            await context.SaveChangesAsync();

            var repo = new ApplicationRepository(context);
            var result = await repo.GetApplicationsAsync(StorageMode.Team, "DOMAIN\\user1");

            Assert.Equal(2, result.Count);
        }
    }
}
