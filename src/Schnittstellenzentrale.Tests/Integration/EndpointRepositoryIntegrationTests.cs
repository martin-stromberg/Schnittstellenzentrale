using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Repositories;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Integration;

public class EndpointRepositoryIntegrationTests
{
    [Fact]
    public async Task AddThenUpdate_WithDifferentInstance_DoesNotThrowTrackingConflict()
    {
        var (context, connection) = TestHelpers.CreateInMemoryDbContext();
        try
        {
            var app = new Core.Models.Application { Name = "App", BaseUrl = "http://app" };
            context.Applications.Add(app);
            await context.SaveChangesAsync();
            context.Entry(app).State = EntityState.Detached;

            var repository = new EndpointRepository(context);

            await repository.AddEndpointAsync(new Core.Models.Endpoint
            {
                Name = "Original",
                Method = Core.Enums.HttpMethod.GET,
                RelativePath = "/test",
                ApplicationId = app.Id
            });

            var loaded = await repository.GetEndpointByIdAsync(
                await context.Endpoints.Select(e => e.Id).FirstAsync());

            loaded!.Name = "Updated";

            var result = await repository.UpdateEndpointAsync(loaded);

            Assert.Equal("Updated", result.Name);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task AddThenUpdate_EndpointGroup_WithDifferentInstance_DoesNotThrowTrackingConflict()
    {
        var (context, connection) = TestHelpers.CreateInMemoryDbContext();
        try
        {
            var app = new Core.Models.Application { Name = "App", BaseUrl = "http://app" };
            context.Applications.Add(app);
            await context.SaveChangesAsync();

            var repository = new EndpointRepository(context);

            await repository.AddEndpointGroupAsync(new EndpointGroup
            {
                Name = "Original",
                ApplicationId = app.Id
            });

            var loaded = await repository.GetEndpointGroupByIdAsync(
                await context.EndpointGroups.Select(g => g.Id).FirstAsync());

            loaded!.Name = "Updated";

            var result = await repository.UpdateEndpointGroupAsync(loaded);

            Assert.Equal("Updated", result.Name);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task SaveEndpoint_ConcurrentWrite_DetectsConflict()
    {
        await TestHelpers.ExecuteWithTwoEndpointContextsAsync(async (ctx1, ctx2) =>
        {
            var app = new Core.Models.Application { Name = "App", BaseUrl = "http://app" };
            ctx1.Context.Applications.Add(app);
            await ctx1.Context.SaveChangesAsync();

            var endpoint = await ctx1.Repo.AddEndpointAsync(new Core.Models.Endpoint
            {
                Name = "Endpoint1",
                Method = Core.Enums.HttpMethod.GET,
                RelativePath = "/test",
                ApplicationId = app.Id
            });

            var endpointFromRepo2 = await ctx2.Repo.GetEndpointByIdAsync(endpoint.Id);

            endpoint.Name = "UpdatedByContext1";
            await ctx1.Repo.UpdateEndpointAsync(endpoint);

            endpointFromRepo2!.Name = "UpdatedByContext2";

            await Assert.ThrowsAnyAsync<DbUpdateConcurrencyException>(async () =>
                await ctx2.Repo.UpdateEndpointAsync(endpointFromRepo2));
        });
    }

    [Fact]
    public async Task UpdateEndpoint_WithApplicationIncluded_CalledTwiceWithDifferentInstances_DoesNotThrowTrackingConflict()
    {
        // Reproduces: after the first UpdateEndpointAsync the Application entity stays tracked in
        // the long-lived Blazor Server DbContext (only the Endpoint itself is detached). When a
        // second Update is called with a *different* Endpoint instance (e.g. after navigating away
        // and back), EF Core's relationship-fixup tries to re-attach the old Endpoint reference
        // that is still sitting in Application.Endpoints, causing the "another instance with the
        // same key value is already being tracked" conflict.
        var (context, connection) = TestHelpers.CreateInMemoryDbContext();
        try
        {
            var app = new Core.Models.Application { Name = "App", BaseUrl = "http://app" };
            context.Applications.Add(app);
            await context.SaveChangesAsync();
            context.Entry(app).State = EntityState.Detached;

            var repository = new EndpointRepository(context);

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
            // At this point Application is still tracked; Application.Endpoints references loaded1.

            // Second load returns a *different* C# object for the same endpoint + application.
            var loaded2 = await repository.GetEndpointByIdAsync(added.Id);
            loaded2!.Name = "Updated";

            // This must not throw InvalidOperationException about tracking conflict.
            await repository.UpdateEndpointAsync(loaded2);

            var final = await repository.GetEndpointByIdAsync(added.Id);
            Assert.Equal("Updated", final!.Name);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task DeleteEndpointGroup_WithEndpoints_CascadesDelete()
    {
        var (context, connection) = TestHelpers.CreateInMemoryDbContext();
        try
        {
            var app = new Core.Models.Application { Name = "App", BaseUrl = "http://app" };
            context.Applications.Add(app);
            await context.SaveChangesAsync();

            var group = new EndpointGroup { Name = "Group", ApplicationId = app.Id };
            context.EndpointGroups.Add(group);
            await context.SaveChangesAsync();

            var endpoint1 = new Core.Models.Endpoint
            {
                Name = "Endpoint1",
                Method = Core.Enums.HttpMethod.GET,
                RelativePath = "/e1",
                ApplicationId = app.Id,
                EndpointGroupId = group.Id
            };
            var endpoint2 = new Core.Models.Endpoint
            {
                Name = "Endpoint2",
                Method = Core.Enums.HttpMethod.GET,
                RelativePath = "/e2",
                ApplicationId = app.Id,
                EndpointGroupId = group.Id
            };
            context.Endpoints.AddRange(endpoint1, endpoint2);
            await context.SaveChangesAsync();

            var repository = new EndpointRepository(context);
            await repository.DeleteEndpointGroupAsync(group.Id);

            Assert.False(await context.EndpointGroups.AnyAsync(g => g.Id == group.Id));
            Assert.False(await context.Endpoints.AnyAsync(e => e.Id == endpoint1.Id));
            Assert.False(await context.Endpoints.AnyAsync(e => e.Id == endpoint2.Id));
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task DeleteEndpointGroup_WithoutEndpoints_DeletesGroup()
    {
        var (context, connection) = TestHelpers.CreateInMemoryDbContext();
        try
        {
            var app = new Core.Models.Application { Name = "App", BaseUrl = "http://app" };
            context.Applications.Add(app);
            await context.SaveChangesAsync();

            var group = new EndpointGroup { Name = "EmptyGroup", ApplicationId = app.Id };
            context.EndpointGroups.Add(group);
            await context.SaveChangesAsync();

            var repository = new EndpointRepository(context);
            await repository.DeleteEndpointGroupAsync(group.Id);

            Assert.False(await context.EndpointGroups.AnyAsync(g => g.Id == group.Id));
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
