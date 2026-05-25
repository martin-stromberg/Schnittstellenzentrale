using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Repositories;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Integration;

/// <summary>SystemEnvironmentRepositoryIntegrationTests</summary>
public class SystemEnvironmentRepositoryIntegrationTests
{
    private static async Task ExecuteWithContextAsync(Func<SystemEnvironmentRepository, Task> test,
        string currentUser = "DOMAIN\\testuser")
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        {
            var currentUserService = new TestHelpers.FixedCurrentUserService(currentUser);
            await test(new SystemEnvironmentRepository(factory, currentUserService));
        }
    }

    private static async Task ExecuteWithTwoUsersAsync(
        Func<SystemEnvironmentRepository, SystemEnvironmentRepository, Task> test)
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        {
            var user1Service = new TestHelpers.FixedCurrentUserService("DOMAIN\\testuser");
            var user2Service = new TestHelpers.FixedCurrentUserService("DOMAIN\\anderer");
            await test(
                new SystemEnvironmentRepository(factory, user1Service),
                new SystemEnvironmentRepository(factory, user2Service));
        }
    }

    /// <summary>AddEnvironment_PersistsEnvironment</summary>
    [Fact]
    public async Task AddEnvironment_PersistsEnvironment()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            var environment = new SystemEnvironment
            {
                Name = "TestUmgebung",
                Mode = StorageMode.Team,
                Variables = [new EnvironmentVariable { Name = "url", Value = "https://example.com" }]
            };

            var added = await repo.AddAsync(environment);

            var result = await repo.GetByIdAsync(added.Id);
            Assert.NotNull(result);
            Assert.Equal("TestUmgebung", result!.Name);
            Assert.Single(result.Variables);
            Assert.Equal("url", result.Variables.First().Name);
            Assert.Equal("https://example.com", result.Variables.First().Value);
        });
    }

    /// <summary>AddEnvironment_WithDuplicateName_ThrowsConstraintException</summary>
    [Fact]
    public async Task AddEnvironment_WithDuplicateName_ThrowsConstraintException()
    {
        await ExecuteWithTwoUsersAsync(async (repo1, _) =>
        {
            await repo1.AddAsync(new SystemEnvironment { Name = "Duplikat", Mode = StorageMode.User });

            await Assert.ThrowsAnyAsync<DbUpdateException>(async () =>
                await repo1.AddAsync(new SystemEnvironment { Name = "Duplikat", Mode = StorageMode.User }));
        });
    }

    /// <summary>DeleteEnvironment_CascadesVariables</summary>
    [Fact]
    public async Task DeleteEnvironment_CascadesVariables()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            var environment = new SystemEnvironment
            {
                Name = "MitVariablen",
                Mode = StorageMode.Team,
                Variables =
                [
                    new EnvironmentVariable { Name = "var1", Value = "wert1" },
                    new EnvironmentVariable { Name = "var2", Value = "wert2" }
                ]
            };
            var added = await repo.AddAsync(environment);

            await repo.DeleteAsync(added.Id);

            var result = await repo.GetByIdAsync(added.Id);
            Assert.Null(result);
        });
    }

    /// <summary>GetEnvironments_WithStorageModeUser_ReturnsOnlyOwnedEnvironments</summary>
    [Fact]
    public async Task GetEnvironments_WithStorageModeUser_ReturnsOnlyOwnedEnvironments()
    {
        await ExecuteWithTwoUsersAsync(async (repo1, repo2) =>
        {
            await repo1.AddAsync(new SystemEnvironment { Name = "Eigene", Mode = StorageMode.User });
            await repo2.AddAsync(new SystemEnvironment { Name = "Fremde", Mode = StorageMode.User });

            var result = await repo1.GetEnvironmentsAsync(StorageMode.User, "DOMAIN\\testuser");

            Assert.Single(result);
            Assert.Equal("Eigene", result[0].Name);
        });
    }

    /// <summary>GetEnvironments_WithStorageModeTeam_ReturnsAllTeamEnvironments</summary>
    [Fact]
    public async Task GetEnvironments_WithStorageModeTeam_ReturnsAllTeamEnvironments()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            await repo.AddAsync(new SystemEnvironment { Name = "Team1", Mode = StorageMode.Team });
            await repo.AddAsync(new SystemEnvironment { Name = "Team2", Mode = StorageMode.Team });

            var result = await repo.GetEnvironmentsAsync(StorageMode.Team, null);

            Assert.Equal(2, result.Count);
        });
    }

    /// <summary>UpdateEnvironment_PersistsChanges</summary>
    [Fact]
    public async Task UpdateEnvironment_PersistsChanges()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            var environment = new SystemEnvironment
            {
                Name = "Original",
                Mode = StorageMode.Team,
                Variables = [new EnvironmentVariable { Name = "var1", Value = "alt" }]
            };
            var added = await repo.AddAsync(environment);

            added.Name = "Geändert";
            added.Variables.First().Value = "neu";
            await repo.UpdateAsync(added);

            var result = await repo.GetByIdAsync(added.Id);
            Assert.NotNull(result);
            Assert.Equal("Geändert", result!.Name);
            Assert.Equal("neu", result.Variables.First().Value);
        });
    }

    /// <summary>UpdateVariableAsync_ExistingVariable_UpdatesValue</summary>
    [Fact]
    public async Task UpdateVariableAsync_ExistingVariable_UpdatesValue()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            var environment = new SystemEnvironment
            {
                Name = "TestUmgebung",
                Mode = StorageMode.Team,
                Variables = [new EnvironmentVariable { Name = "host", Value = "old", IsValueMasked = true }]
            };
            var added = await repo.AddAsync(environment);

            await repo.UpdateVariableAsync(added.Id, "host", "new");

            var result = await repo.GetByIdAsync(added.Id);
            Assert.NotNull(result);
            var variable = result!.Variables.FirstOrDefault(v => v.Name == "host");
            Assert.NotNull(variable);
            Assert.Equal("new", variable!.Value);
            Assert.True(variable.IsValueMasked);
        });
    }

    /// <summary>UpdateVariableAsync_NewVariable_InsertsVariable</summary>
    [Fact]
    public async Task UpdateVariableAsync_NewVariable_InsertsVariable()
    {
        await ExecuteWithContextAsync(async repo =>
        {
            var environment = new SystemEnvironment
            {
                Name = "TestUmgebung",
                Mode = StorageMode.Team,
                Variables = []
            };
            var added = await repo.AddAsync(environment);

            await repo.UpdateVariableAsync(added.Id, "newvar", "newvalue");

            var result = await repo.GetByIdAsync(added.Id);
            Assert.NotNull(result);
            Assert.Single(result!.Variables);
            var variable = result.Variables.First();
            Assert.Equal("newvar", variable.Name);
            Assert.Equal("newvalue", variable.Value);
        });
    }
}
