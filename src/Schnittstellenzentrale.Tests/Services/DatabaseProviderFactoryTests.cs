using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Infrastructure.Data;

namespace Schnittstellenzentrale.Tests.Services;

public class DatabaseProviderFactoryTests
{
    [Fact]
    public void CreateSqliteContext_ReturnsSqliteDbContext()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "DatabaseProvider", "SQLite" },
                { "ConnectionStrings:Default", "Data Source=:memory:" }
            })
            .Build();

        var services = new ServiceCollection();
        DatabaseProviderFactory.RegisterDbContext(services, config);
        var provider = services.BuildServiceProvider();

        var factory = provider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var context = factory.CreateDbContext();

        Assert.NotNull(context);
        Assert.Contains("Sqlite", context.Database.ProviderName, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CreateSqlServerContext_ReturnsSqlServerDbContext()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "DatabaseProvider", "SqlServer" },
                { "ConnectionStrings:Default", "Server=(local);Database=Test;Integrated Security=true;TrustServerCertificate=true" }
            })
            .Build();

        var services = new ServiceCollection();
        DatabaseProviderFactory.RegisterDbContext(services, config);
        var provider = services.BuildServiceProvider();

        var factory = provider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var context = factory.CreateDbContext();

        Assert.NotNull(context);
        Assert.Contains("SqlServer", context.Database.ProviderName, StringComparison.OrdinalIgnoreCase);
    }
}
