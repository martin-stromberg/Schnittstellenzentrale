using Microsoft.Extensions.Options;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Services;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>ApplicationGroupServiceTests</summary>
public class ApplicationGroupServiceTests
{
    /// <summary>ApplicationGroupService_UpdateIcon_PersistiertBytes</summary>
    [Fact]
    public async Task ApplicationGroupService_UpdateIcon_PersistiertBytes()
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        {
            await using (var ctx = factory.CreateDbContext())
            {
                var group = new ApplicationGroup { Name = "TestGruppe" };
                ctx.ApplicationGroups.Add(group);
                await ctx.SaveChangesAsync();
            }

            var settings = Options.Create(new UploadSettings { MaxIconSizeBytes = 524288 });
            var service = new ApplicationGroupService(factory, settings);

            await using (var ctx = factory.CreateDbContext())
            {
                var group = ctx.ApplicationGroups.First();
                var iconData = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

                await service.UpdateIconAsync(group.Id, iconData);

                var updated = ctx.ApplicationGroups.Find(group.Id);
                Assert.NotNull(updated);
                Assert.Equal(iconData, updated!.IconData);
            }
        }
    }

    /// <summary>ApplicationGroupService_UpdateIcon_ZuGroßeDatei_WirftException</summary>
    [Fact]
    public async Task ApplicationGroupService_UpdateIcon_ZuGroßeDatei_WirftException()
    {
        var (factory, connection) = TestHelpers.CreateInMemoryDbContext();
        await using (connection)
        {
            await using (var ctx = factory.CreateDbContext())
            {
                var group = new ApplicationGroup { Name = "TestGruppe" };
                ctx.ApplicationGroups.Add(group);
                await ctx.SaveChangesAsync();
            }

            var maxBytes = 100;
            var settings = Options.Create(new UploadSettings { MaxIconSizeBytes = maxBytes });
            var service = new ApplicationGroupService(factory, settings);

            await using (var ctx = factory.CreateDbContext())
            {
                var group = ctx.ApplicationGroups.First();
                var tooBig = new byte[maxBytes + 1];

                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => service.UpdateIconAsync(group.Id, tooBig));
            }
        }
    }
}
