using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Infrastructure.Data;

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
}
