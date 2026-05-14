using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Schnittstellenzentrale.Infrastructure.Data;

public static class DatabaseProviderFactory
{
    public static void RegisterDbContext(IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration.GetValue<string>("DatabaseProvider") ?? "SQLite";
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=schnittstellenzentrale.db";

        if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));
        }
        else
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connectionString));
        }
    }
}
