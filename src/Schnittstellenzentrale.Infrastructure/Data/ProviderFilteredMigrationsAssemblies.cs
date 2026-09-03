using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

namespace Schnittstellenzentrale.Infrastructure.Data;

// EF1001: MigrationsAssembly ist eine interne EF-Core-API. Es gibt keinen unterstützten
// öffentlichen Weg, die pro Provider sichtbaren Migrationen einzuschränken (Multi-Provider-
// Migrations-Filterung) - dieses Muster wird von Microsoft selbst so dokumentiert. Bewusste,
// eng begrenzte Ausnahme statt eines Workarounds, der EF-Core-Verhalten neu implementieren
// müsste.
#pragma warning disable EF1001

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

#pragma warning restore EF1001
