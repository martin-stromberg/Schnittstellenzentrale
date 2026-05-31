using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Repositories;
using Schnittstellenzentrale.Infrastructure.Services;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>ApplicationLinkServiceTests</summary>
public class ApplicationLinkServiceTests
{
    private static async Task<Application> SeedApplicationAsync(
        Microsoft.EntityFrameworkCore.IDbContextFactory<Schnittstellenzentrale.Infrastructure.Data.AppDbContext> factory)
    {
        await using var ctx = factory.CreateDbContext();
        var app = new Application { Name = "App", BaseUrl = "http://example.com", Description = string.Empty };
        ctx.Applications.Add(app);
        await ctx.SaveChangesAsync();
        return app;
    }

    /// <summary>ApplicationLinkService_GetLinks_GibtLinksZurück</summary>
    [Fact]
    public async Task ApplicationLinkService_GetLinks_GibtLinksZurück()
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        {
            var app = await SeedApplicationAsync(factory);
            var repo = new ApplicationLinkRepository(factory);
            var service = new ApplicationLinkService(repo);

            await repo.AddAsync(new ApplicationLink { ApplicationId = app.Id, Url = "https://a.example.com", Label = "A" });
            await repo.AddAsync(new ApplicationLink { ApplicationId = app.Id, Url = "https://b.example.com", Label = "B" });

            var links = await service.GetLinksAsync(app.Id);

            Assert.Equal(2, links.Count);
        }
    }

    /// <summary>ApplicationLinkService_AddLink_PersistiertLink</summary>
    [Fact]
    public async Task ApplicationLinkService_AddLink_PersistiertLink()
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        {
            var app = await SeedApplicationAsync(factory);
            var repo = new ApplicationLinkRepository(factory);
            var service = new ApplicationLinkService(repo);

            var link = new ApplicationLink { ApplicationId = app.Id, Url = "https://new.example.com", Label = "Neu" };
            var added = await service.AddLinkAsync(link);

            Assert.True(added.Id > 0);

            var links = await service.GetLinksAsync(app.Id);
            Assert.Single(links);
            Assert.Equal("https://new.example.com", links[0].Url);
        }
    }

    /// <summary>ApplicationLinkService_UpdateLink_AktualisierLink</summary>
    [Fact]
    public async Task ApplicationLinkService_UpdateLink_AktualisierLink()
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        {
            var app = await SeedApplicationAsync(factory);
            var repo = new ApplicationLinkRepository(factory);
            var service = new ApplicationLinkService(repo);

            var added = await repo.AddAsync(new ApplicationLink { ApplicationId = app.Id, Url = "https://old.example.com", Label = "Alt" });

            added.Url = "https://new.example.com";
            added.Label = "Neu";
            var updated = await service.UpdateLinkAsync(added);

            Assert.Equal("https://new.example.com", updated.Url);
            Assert.Equal("Neu", updated.Label);
        }
    }

    /// <summary>ApplicationLinkService_DeleteLink_EntferntLink</summary>
    [Fact]
    public async Task ApplicationLinkService_DeleteLink_EntferntLink()
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        {
            var app = await SeedApplicationAsync(factory);
            var repo = new ApplicationLinkRepository(factory);
            var service = new ApplicationLinkService(repo);

            var added = await repo.AddAsync(new ApplicationLink { ApplicationId = app.Id, Url = "https://del.example.com", Label = "Löschen" });

            await service.DeleteLinkAsync(added.Id);

            var links = await service.GetLinksAsync(app.Id);
            Assert.Empty(links);
        }
    }
}
