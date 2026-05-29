using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Services;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>HistoryServiceTests</summary>
public class HistoryServiceTests
{
    /// <summary>HistoryService_AddEntry_PersistiertEintrag</summary>
    [Fact]
    public async Task HistoryService_AddEntry_PersistiertEintrag()
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContextWithHistory();
        await using (connection)
        {
            var service = new HistoryService(factory);
            var entry = new EndpointCallHistoryEntry
            {
                ExecutedAt = DateTime.UtcNow,
                HttpMethod = "GET",
                RelativePath = "/api/test",
                StatusCode = 200,
                DurationMs = 42
            };

            await service.AddEntryAsync(entry);

            await using var ctx = factory.CreateDbContext();
            Assert.Equal(1, ctx.EndpointCallHistory.Count());
            Assert.Equal("/api/test", ctx.EndpointCallHistory.First().RelativePath);
        }
    }

    /// <summary>HistoryService_GetPaged_ReturnsKorrekteSortiertheitUndFilterung</summary>
    [Fact]
    public async Task HistoryService_GetPaged_ReturnsKorrekteSortiertheitUndFilterung()
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContextWithHistory();
        await using (connection)
        {
            await using (var ctx = factory.CreateDbContext())
            {
                var app = new Application { Id = 1, Name = "App", BaseUrl = "http://example.com", Description = string.Empty };
                ctx.Applications.Add(app);
                var base_ = DateTime.UtcNow.AddHours(-3);
                ctx.EndpointCallHistory.AddRange(
                    new EndpointCallHistoryEntry { ApplicationId = 1, ExecutedAt = base_.AddHours(2), HttpMethod = "GET", RelativePath = "/a" },
                    new EndpointCallHistoryEntry { ApplicationId = 1, ExecutedAt = base_.AddHours(1), HttpMethod = "POST", RelativePath = "/b" },
                    new EndpointCallHistoryEntry { ApplicationId = 2, ExecutedAt = base_.AddHours(3), HttpMethod = "GET", RelativePath = "/other" }
                );
                await ctx.SaveChangesAsync();
            }

            var service = new HistoryService(factory);
            var filter = new HistoryFilter(ApplicationId: 1, EndpointId: null, From: null, To: null);
            var (items, total) = await service.GetPagedAsync(filter, page: 1, pageSize: 10);

            Assert.Equal(2, total);
            Assert.Equal(2, items.Count);
            Assert.True(items[0].ExecutedAt >= items[1].ExecutedAt);
            Assert.All(items, i => Assert.Equal(1, i.ApplicationId));
        }
    }

    /// <summary>HistoryService_GetTopEndpoints_GibtTop5Zurück</summary>
    [Fact]
    public async Task HistoryService_GetTopEndpoints_GibtTop5Zurück()
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContextWithHistory();
        await using (connection)
        {
            await using (var ctx = factory.CreateDbContext())
            {
                var app = new Application { Id = 1, Name = "App", BaseUrl = "http://example.com", Description = string.Empty };
                ctx.Applications.Add(app);
                var now = DateTime.UtcNow;
                for (var i = 0; i < 3; i++)
                    ctx.EndpointCallHistory.Add(new EndpointCallHistoryEntry { ApplicationId = 1, EndpointId = 10, RelativePath = "/top", HttpMethod = "GET", ExecutedAt = now });
                ctx.EndpointCallHistory.Add(new EndpointCallHistoryEntry { ApplicationId = 1, EndpointId = 20, RelativePath = "/second", HttpMethod = "GET", ExecutedAt = now });
                await ctx.SaveChangesAsync();
            }

            var service = new HistoryService(factory);
            var top = await service.GetTopEndpointsAsync(applicationId: 1, count: 5);

            Assert.Equal(2, top.Count);
            Assert.Equal(10, top[0].EndpointId);
            Assert.Equal(3, top[0].CallCount);
            Assert.Equal(20, top[1].EndpointId);
            Assert.Equal(1, top[1].CallCount);
        }
    }
}
