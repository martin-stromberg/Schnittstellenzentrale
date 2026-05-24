using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Repositories;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Integration;

/// <summary>EndpointRepositoryIntegrationTests</summary>
public class EndpointRepositoryIntegrationTests
{
    /// <summary>AddThenUpdate_WithDifferentInstance_DoesNotThrowTrackingConflict</summary>
    [Fact]
    public async Task AddThenUpdate_WithDifferentInstance_DoesNotThrowTrackingConflict()
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        {
            var app = new Core.Models.Application { Name = "App", BaseUrl = "http://app" };
            await using (var context = factory.CreateDbContext())
            {
                context.Applications.Add(app);
                await context.SaveChangesAsync();
            }

            var repository = new EndpointRepository(factory);

            await repository.AddEndpointAsync(new Core.Models.Endpoint
            {
                Name = "Original",
                Method = Core.Enums.HttpMethod.GET,
                RelativePath = "/test",
                ApplicationId = app.Id
            });

            int endpointId;
            await using (var context = factory.CreateDbContext())
            {
                endpointId = await context.Endpoints.Select(e => e.Id).FirstAsync();
            }

            var loaded = await repository.GetEndpointByIdAsync(endpointId);
            loaded!.Name = "Updated";

            var result = await repository.UpdateEndpointAsync(loaded);

            Assert.Equal("Updated", result.Name);
        }
    }

    /// <summary>AddThenUpdate_EndpointGroup_WithDifferentInstance_DoesNotThrowTrackingConflict</summary>
    [Fact]
    public async Task AddThenUpdate_EndpointGroup_WithDifferentInstance_DoesNotThrowTrackingConflict()
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        {
            var app = new Core.Models.Application { Name = "App", BaseUrl = "http://app" };
            await using (var context = factory.CreateDbContext())
            {
                context.Applications.Add(app);
                await context.SaveChangesAsync();
            }

            var repository = new EndpointRepository(factory);

            await repository.AddEndpointGroupAsync(new EndpointGroup
            {
                Name = "Original",
                ApplicationId = app.Id
            });

            int groupId;
            await using (var context = factory.CreateDbContext())
            {
                groupId = await context.EndpointGroups.Select(g => g.Id).FirstAsync();
            }

            var loaded = await repository.GetEndpointGroupByIdAsync(groupId);
            loaded!.Name = "Updated";

            var result = await repository.UpdateEndpointGroupAsync(loaded);

            Assert.Equal("Updated", result.Name);
        }
    }

    /// <summary>SaveEndpoint_ConcurrentWrite_DetectsConflict</summary>
    [Fact]
    public async Task SaveEndpoint_ConcurrentWrite_DetectsConflict()
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        {
            var app = new Core.Models.Application { Name = "App", BaseUrl = "http://app" };
            await using (var context = factory.CreateDbContext())
            {
                context.Applications.Add(app);
                await context.SaveChangesAsync();
            }

            var repo1 = new EndpointRepository(factory);
            var repo2 = new EndpointRepository(factory);

            var endpoint = await repo1.AddEndpointAsync(new Core.Models.Endpoint
            {
                Name = "Endpoint1",
                Method = Core.Enums.HttpMethod.GET,
                RelativePath = "/test",
                ApplicationId = app.Id
            });

            var endpointFromRepo2 = await repo2.GetEndpointByIdAsync(endpoint.Id);

            endpoint.Name = "UpdatedByContext1";
            await repo1.UpdateEndpointAsync(endpoint);

            endpointFromRepo2!.Name = "UpdatedByContext2";

            await Assert.ThrowsAnyAsync<DbUpdateConcurrencyException>(async () =>
                await repo2.UpdateEndpointAsync(endpointFromRepo2));
        }
    }

    /// <summary>UpdateEndpoint_WithApplicationIncluded_CalledTwiceWithDifferentInstances_DoesNotThrowTrackingConflict</summary>
    [Fact]
    public async Task UpdateEndpoint_WithApplicationIncluded_CalledTwiceWithDifferentInstances_DoesNotThrowTrackingConflict()
    {
        // Reproduces: after the first UpdateEndpointAsync the Application entity stays tracked in
        // the long-lived Blazor Server DbContext (only the Endpoint itself is detached). When a
        // second Update is called with a *different* Endpoint instance (e.g. after navigating away
        // and back), EF Core's relationship-fixup tries to re-attach the old Endpoint reference
        // that is still sitting in Application.Endpoints, causing the "another instance with the
        // same key value is already being tracked" conflict.
        var (factory, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        {
            var app = new Core.Models.Application { Name = "App", BaseUrl = "http://app" };
            await using (var context = factory.CreateDbContext())
            {
                context.Applications.Add(app);
                await context.SaveChangesAsync();
            }

            var repository = new EndpointRepository(factory);

            var added = await repository.AddEndpointAsync(new Core.Models.Endpoint
            {
                Name = "Original",
                Method = Core.Enums.HttpMethod.GET,
                RelativePath = "/test",
                ApplicationId = app.Id
            });

            // First load with Application included – same as what happens in the UI after
            // a new endpoint is created (Home.razor calls GetEndpointByIdAsync with Include(Application)).
            var loaded1 = await repository.GetEndpointByIdAsync(added.Id);
            await repository.UpdateEndpointAsync(loaded1!);

            // Second load returns a *different* C# object for the same endpoint + application.
            var loaded2 = await repository.GetEndpointByIdAsync(added.Id);
            loaded2!.Name = "Updated";

            // This must not throw InvalidOperationException about tracking conflict.
            await repository.UpdateEndpointAsync(loaded2);

            var final = await repository.GetEndpointByIdAsync(added.Id);
            Assert.Equal("Updated", final!.Name);
        }
    }

    /// <summary>DeleteEndpointGroup_WithEndpoints_CascadesDelete</summary>
    [Fact]
    public async Task DeleteEndpointGroup_WithEndpoints_CascadesDelete()
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        {
            var app = new Core.Models.Application { Name = "App", BaseUrl = "http://app" };
            EndpointGroup group;
            Core.Models.Endpoint endpoint1;
            Core.Models.Endpoint endpoint2;

            await using (var context = factory.CreateDbContext())
            {
                context.Applications.Add(app);
                await context.SaveChangesAsync();

                group = new EndpointGroup { Name = "Group", ApplicationId = app.Id };
                context.EndpointGroups.Add(group);
                await context.SaveChangesAsync();

                endpoint1 = new Core.Models.Endpoint
                {
                    Name = "Endpoint1",
                    Method = Core.Enums.HttpMethod.GET,
                    RelativePath = "/e1",
                    ApplicationId = app.Id,
                    EndpointGroupId = group.Id
                };
                endpoint2 = new Core.Models.Endpoint
                {
                    Name = "Endpoint2",
                    Method = Core.Enums.HttpMethod.GET,
                    RelativePath = "/e2",
                    ApplicationId = app.Id,
                    EndpointGroupId = group.Id
                };
                context.Endpoints.AddRange(endpoint1, endpoint2);
                await context.SaveChangesAsync();
            }

            var repository = new EndpointRepository(factory);
            await repository.DeleteEndpointGroupAsync(group.Id);

            await using (var context = factory.CreateDbContext())
            {
                Assert.False(await context.EndpointGroups.AnyAsync(g => g.Id == group.Id));
                Assert.False(await context.Endpoints.AnyAsync(e => e.Id == endpoint1.Id));
                Assert.False(await context.Endpoints.AnyAsync(e => e.Id == endpoint2.Id));
            }
        }
    }

    /// <summary>DeleteEndpointGroup_WithoutEndpoints_DeletesGroup</summary>
    [Fact]
    public async Task DeleteEndpointGroup_WithoutEndpoints_DeletesGroup()
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        {
            var app = new Core.Models.Application { Name = "App", BaseUrl = "http://app" };
            EndpointGroup group;

            await using (var context = factory.CreateDbContext())
            {
                context.Applications.Add(app);
                await context.SaveChangesAsync();

                group = new EndpointGroup { Name = "EmptyGroup", ApplicationId = app.Id };
                context.EndpointGroups.Add(group);
                await context.SaveChangesAsync();
            }

            var repository = new EndpointRepository(factory);
            await repository.DeleteEndpointGroupAsync(group.Id);

            await using (var context = factory.CreateDbContext())
            {
                Assert.False(await context.EndpointGroups.AnyAsync(g => g.Id == group.Id));
            }
        }
    }
}
