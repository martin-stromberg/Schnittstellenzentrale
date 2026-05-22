using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Schnittstellenzentrale.Infrastructure.Data;

/// <summary>Registriert den <see cref="AppDbContext"/> abhängig vom konfigurierten Datenbank-Provider.</summary>
public static class DatabaseProviderFactory
{
    /// <summary>Liest <c>DatabaseProvider</c> aus der Konfiguration und registriert den passenden EF-Core-Provider samt gefilterten Migrationen.</summary>
    public static void RegisterDbContext(IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration.GetValue<string>("DatabaseProvider") ?? "SQLite";
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=schnittstellenzentrale.db";

        if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString)
                       .ReplaceService<IMigrationsAssembly, SqlServerMigrationsAssembly>());
        }
        else
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connectionString)
                       .ReplaceService<IMigrationsAssembly, SqliteMigrationsAssembly>());
        }
    }
}
