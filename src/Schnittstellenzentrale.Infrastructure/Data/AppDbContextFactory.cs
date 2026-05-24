using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Schnittstellenzentrale.Infrastructure.Data;

/// <summary>Design-Time-Factory für <see cref="AppDbContext"/>. Unterstützt SQLite (Standard) und SQL Server via <c>--DatabaseProvider SqlServer</c>.</summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <inheritdoc/>
    public AppDbContext CreateDbContext(string[] args)
    {
        var provider = "SQLite";
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals("--DatabaseProvider", StringComparison.OrdinalIgnoreCase))
            {
                provider = args[i + 1];
                break;
            }
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Schnittstellenzentrale;Trusted_Connection=True;TrustServerCertificate=True;")
                          .ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsAssembly, SqlServerMigrationsAssembly>();
        else
            optionsBuilder.UseSqlite("Data Source=schnittstellenzentrale.db")
                          .ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsAssembly, SqliteMigrationsAssembly>();

        return new AppDbContext(optionsBuilder.Options);
    }
}
