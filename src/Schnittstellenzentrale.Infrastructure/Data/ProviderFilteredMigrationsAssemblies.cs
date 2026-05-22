using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

namespace Schnittstellenzentrale.Infrastructure.Data;

/// <summary>Schränkt die sichtbaren Migrationen auf den SQLite-Namespace ein.</summary>
internal sealed class SqliteMigrationsAssembly(
    ICurrentDbContext currentContext,
    IDbContextOptions options,
    IMigrationsIdGenerator idGenerator,
    IDiagnosticsLogger<DbLoggerCategory.Migrations> logger)
    : MigrationsAssembly(currentContext, options, idGenerator, logger)
{
    private IReadOnlyDictionary<string, TypeInfo>? _cached;

    /// <inheritdoc/>
    public override IReadOnlyDictionary<string, TypeInfo> Migrations =>
        _cached ??= base.Migrations
            .Where(m => m.Value.Namespace == "Schnittstellenzentrale.Infrastructure.Data.Migrations")
            .ToDictionary(m => m.Key, m => m.Value);
}

/// <summary>Schränkt die sichtbaren Migrationen auf den SQL-Server-Namespace ein.</summary>
internal sealed class SqlServerMigrationsAssembly(
    ICurrentDbContext currentContext,
    IDbContextOptions options,
    IMigrationsIdGenerator idGenerator,
    IDiagnosticsLogger<DbLoggerCategory.Migrations> logger)
    : MigrationsAssembly(currentContext, options, idGenerator, logger)
{
    private IReadOnlyDictionary<string, TypeInfo>? _cached;

    /// <inheritdoc/>
    public override IReadOnlyDictionary<string, TypeInfo> Migrations =>
        _cached ??= base.Migrations
            .Where(m => m.Value.Namespace == "Schnittstellenzentrale.Infrastructure.Data.SqlServerMigrations")
            .ToDictionary(m => m.Key, m => m.Value);
}
