using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Data;
using Schnittstellenzentrale.Infrastructure.Repositories;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Integration;

public class EndpointRepositoryIntegrationTests
{
    [Fact]
    public async Task SaveEndpoint_ConcurrentWrite_DetectsConflict()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context1 = new AppDbContext(options);
        context1.Database.EnsureCreated();

        var app = new Core.Models.Application { Name = "App", BaseUrl = "http://app" };
        context1.Applications.Add(app);
        await context1.SaveChangesAsync();

        var endpoint = new Core.Models.Endpoint
        {
            Name = "Endpoint1",
            Method = Core.Enums.HttpMethod.GET,
            RelativePath = "/test",
            ApplicationId = app.Id
        };
        context1.Endpoints.Add(endpoint);
        await context1.SaveChangesAsync();

        await using var context2 = new AppDbContext(options);
        var endpointFromContext2 = await context2.Endpoints.FirstAsync(e => e.Id == endpoint.Id);

        endpoint.Name = "UpdatedByContext1";
        await context1.SaveChangesAsync();

        endpointFromContext2.Name = "UpdatedByContext2";

        await Assert.ThrowsAnyAsync<DbUpdateConcurrencyException>(async () =>
        {
            context2.Endpoints.Update(endpointFromContext2);
            await context2.SaveChangesAsync();
        });
    }
}
