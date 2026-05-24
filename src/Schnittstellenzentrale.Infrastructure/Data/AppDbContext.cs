using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Infrastructure.Data;

/// <summary>Entity Framework Core-Datenbankkontext der Anwendung.</summary>
public class AppDbContext : DbContext
{
    /// <summary>Initialisiert eine neue Instanz von <see cref="AppDbContext"/>.</summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>Anwendungsgruppen.</summary>
    public DbSet<ApplicationGroup> ApplicationGroups => Set<ApplicationGroup>();
    /// <summary>Anwendungen.</summary>
    public DbSet<Core.Models.Application> Applications => Set<Core.Models.Application>();
    /// <summary>Endpunktgruppen.</summary>
    public DbSet<EndpointGroup> EndpointGroups => Set<EndpointGroup>();
    /// <summary>Endpunkte.</summary>
    public DbSet<Core.Models.Endpoint> Endpoints => Set<Core.Models.Endpoint>();
    /// <summary>Endpunkt-Header.</summary>
    public DbSet<EndpointHeader> EndpointHeaders => Set<EndpointHeader>();
    /// <summary>Endpunkt-Abfrageparameter.</summary>
    public DbSet<EndpointQueryParameter> EndpointQueryParameters => Set<EndpointQueryParameter>();
    /// <summary>Systemumgebungen.</summary>
    public DbSet<Core.Models.SystemEnvironment> SystemEnvironments => Set<Core.Models.SystemEnvironment>();
    /// <summary>Umgebungsvariablen.</summary>
    public DbSet<EnvironmentVariable> EnvironmentVariables => Set<EnvironmentVariable>();

    /// <inheritdoc/>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateRowVersions();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateRowVersions();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    private void UpdateRowVersions()
    {
        foreach (var entry in ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
        {
            if (entry.Metadata.FindProperty("RowVersion") != null)
            {
                entry.Property("RowVersion").CurrentValue = Guid.NewGuid().ToByteArray();
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IsSystem).HasDefaultValue(false);
            entity.Property(e => e.RowVersion).IsConcurrencyToken();
            entity.HasMany(e => e.Applications)
                  .WithOne(a => a.ApplicationGroup)
                  .HasForeignKey(a => a.ApplicationGroupId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Core.Models.Application>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IsSystem).HasDefaultValue(false);
            entity.Property(e => e.BaseUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.InterfaceUrl).HasMaxLength(500);
            entity.Property(e => e.Owner).HasMaxLength(256);
            entity.Property(e => e.RowVersion).IsConcurrencyToken();
            entity.HasMany(e => e.Endpoints)
                  .WithOne(e => e.Application)
                  .HasForeignKey(e => e.ApplicationId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.EndpointGroups)
                  .WithOne(g => g.Application)
                  .HasForeignKey(g => g.ApplicationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EndpointGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RowVersion).IsConcurrencyToken();
            entity.HasMany(e => e.Endpoints)
                  .WithOne(e => e.EndpointGroup)
                  .HasForeignKey(e => e.EndpointGroupId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.ChildGroups)
                  .WithOne(g => g.ParentGroup)
                  .HasForeignKey(g => g.ParentGroupId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Core.Models.Endpoint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RelativePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.RowVersion).IsConcurrencyToken();
            entity.HasMany(e => e.Headers)
                  .WithOne(h => h.Endpoint)
                  .HasForeignKey(h => h.EndpointId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.QueryParameters)
                  .WithOne(p => p.Endpoint)
                  .HasForeignKey(p => p.EndpointId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EndpointHeader>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(2000);
        });

        modelBuilder.Entity<EndpointQueryParameter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(2000);
        });

        modelBuilder.Entity<Core.Models.SystemEnvironment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Owner).HasMaxLength(256);
            entity.HasIndex(e => new { e.Name, e.Mode, e.Owner }).IsUnique();
            entity.HasMany(e => e.Variables)
                  .WithOne(v => v.SystemEnvironment)
                  .HasForeignKey(v => v.SystemEnvironmentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EnvironmentVariable>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(4000);
            entity.HasIndex(e => new { e.Name, e.SystemEnvironmentId }).IsUnique();
        });
    }
}
