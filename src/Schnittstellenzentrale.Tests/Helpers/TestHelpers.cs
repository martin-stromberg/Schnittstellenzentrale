using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Infrastructure.Data;
using Schnittstellenzentrale.Infrastructure.Repositories;

namespace Schnittstellenzentrale.Tests.Helpers;

public static class TestHelpers
{
    /// <summary>
    /// Erstellt einen <see cref="AppDbContext"/> mit SQLite In-Memory-Provider.
    /// Der Aufrufer muss beide Objekte disposen: zuerst den <see cref="AppDbContext"/>,
    /// dann die <see cref="SqliteConnection"/>. Eine umgekehrte Reihenfolge führt zu SQLite-Fehlern.
    /// </summary>
    public static (AppDbContext Context, SqliteConnection Connection) CreateInMemoryDbContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return (context, connection);
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
            await using var context1 = new AppDbContext(options);
            context1.Database.EnsureCreated();
            await using var context2 = new AppDbContext(options);
            await test(new ApplicationRepository(context1), new ApplicationRepository(context2));
        }
    }
}
