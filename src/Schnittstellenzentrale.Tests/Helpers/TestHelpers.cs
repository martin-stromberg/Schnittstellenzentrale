using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Infrastructure.Data;
using Schnittstellenzentrale.Infrastructure.Repositories;

namespace Schnittstellenzentrale.Tests.Helpers;

/// <summary>Statische Hilfsklasse mit Hilfsmethoden für Datenbankcontexte in Tests.</summary>
public static class TestHelpers
{
    /// <summary>
    /// Erstellt eine <see cref="IDbContextFactory{AppDbContext}"/> mit SQLite In-Memory-Provider.
    /// Der Aufrufer muss die <see cref="SqliteConnection"/> disposen, nachdem alle Factory-erstellten
    /// Contexts disposed wurden.
    /// </summary>
    public static (IDbContextFactory<AppDbContext> Factory, SqliteConnection Connection) CreateInMemoryDbContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new AppDbContext(options))
        {
            context.Database.EnsureCreated();
        }

        return (new FixedOptionsDbContextFactory(options), connection);
    }

    /// <summary>
    /// Führt einen Test mit zwei unabhängigen <see cref="ApplicationRepository"/>-Instanzen über
    /// dieselbe SQLite-Connection aus. Ermöglicht Concurrency-Tests, bei denen zwei Kontexte
    /// denselben Datenbankzustand sehen müssen.
    /// </summary>
    public static async Task ExecuteWithTwoContextsAsync(Func<ApplicationRepository, ApplicationRepository, Task> test)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (connection)
        {
            using (var initContext = new AppDbContext(options))
            {
                initContext.Database.EnsureCreated();
            }

            var factory1 = new FixedOptionsDbContextFactory(options);
            var factory2 = new FixedOptionsDbContextFactory(options);
            await test(new ApplicationRepository(factory1), new ApplicationRepository(factory2));
        }
    }

    private sealed class FixedOptionsDbContextFactory(DbContextOptions<AppDbContext> options)
        : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => new(options);
    }
}
