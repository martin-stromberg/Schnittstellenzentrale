#pragma warning disable CS1591
using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ApplicationGroup> ApplicationGroups => Set<ApplicationGroup>();
    public DbSet<Core.Models.Application> Applications => Set<Core.Models.Application>();
    public DbSet<EndpointGroup> EndpointGroups => Set<EndpointGroup>();
    public DbSet<Core.Models.Endpoint> Endpoints => Set<Core.Models.Endpoint>();
    public DbSet<EndpointHeader> EndpointHeaders => Set<EndpointHeader>();
    public DbSet<EndpointQueryParameter> EndpointQueryParameters => Set<EndpointQueryParameter>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateRowVersions();
        return base.SaveChangesAsync(cancellationToken);
    }

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
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
                  .OnDelete(DeleteBehavior.SetNull);
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
    }
}
