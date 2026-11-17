using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Data;

namespace Schnittstellenzentrale.Infrastructure.Repositories;

/// <summary>EF-Core-Implementierung von <see cref="IApplicationRepository"/>.</summary>
public class ApplicationRepository : IApplicationRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ApplicationRepository"/>.</summary>
    /// <param name="factory">Fabrik zum Erzeugen des DbContext.</param>
    public ApplicationRepository(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    /// <inheritdoc/>
    public async Task<IList<ApplicationGroup>> GetGroupsAsync(StorageMode storageMode, string owner)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var query = context.ApplicationGroups.Include(g => g.Applications).AsQueryable();
        if (storageMode == StorageMode.User)
        {
            var ownerGroupIds = await ApplyOwnerFilter(context.Applications.AsQueryable(), storageMode, owner)
                .Where(a => a.ApplicationGroupId != null)
                .Select(a => a.ApplicationGroupId!.Value)
                .Distinct()
                .ToListAsync();
            query = query.Where(g => ownerGroupIds.Contains(g.Id));
        }

        return await query.ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<ApplicationGroup?> GetGroupByIdAsync(int id)
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.ApplicationGroups
            .Include(g => g.Applications)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    /// <inheritdoc/>
    public async Task<ApplicationGroup?> GetSystemGroupAsync()
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.ApplicationGroups
            .Include(g => g.Applications)
            .FirstOrDefaultAsync(g => g.IsSystem);
    }

    /// <inheritdoc/>
    public async Task<ApplicationGroup> AddGroupAsync(ApplicationGroup group)
    {
        await using var context = await _factory.CreateDbContextAsync();
        context.ApplicationGroups.Add(group);
        await context.SaveChangesAsync();
        return group;
    }

    /// <inheritdoc/>
    public async Task<ApplicationGroup> UpdateGroupAsync(ApplicationGroup group)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var existing = await context.ApplicationGroups.FindAsync(group.Id)
            ?? throw new InvalidOperationException($"Gruppe {group.Id} nicht gefunden.");
        context.Entry(existing).Property(e => e.RowVersion).OriginalValue = group.RowVersion;
        existing.ApplyUpdate(group);
        await context.SaveChangesAsync();
        return existing;
    }

    /// <inheritdoc/>
    public async Task DeleteGroupAsync(int id)
    {
        await DeleteGroupAsync(id, ApplicationGroupDeleteMode.Detach);
    }

    /// <inheritdoc/>
    public async Task DeleteGroupAsync(int id, ApplicationGroupDeleteMode mode)
    {
        await using var context = await _factory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var group = await context.ApplicationGroups
            .Include(g => g.Applications)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (group == null)
            return;

        if (mode == ApplicationGroupDeleteMode.Delete)
        {
            var applicationIds = group.Applications.Select(a => a.Id).ToList();
            var endpointGroups = await context.EndpointGroups
                .Where(g => applicationIds.Contains(g.ApplicationId))
                .ToListAsync();
            var endpoints = await context.Endpoints
                .Where(e => applicationIds.Contains(e.ApplicationId))
                .ToListAsync();

            // Getrackte Entitäten werden von EF topologisch geordnet gelöscht —
            // Endpunktgruppen stehen auf DeleteBehavior.Restrict, Kindgruppen müssen
            // daher vor ihren Eltern entfernt werden.
            context.Endpoints.RemoveRange(endpoints);
            context.EndpointGroups.RemoveRange(endpointGroups);
            context.Applications.RemoveRange(group.Applications);
        }
        else
        {
            foreach (var application in group.Applications)
                application.ApplicationGroupId = null;
        }

        context.ApplicationGroups.Remove(group);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    /// <inheritdoc/>
    public async Task<IList<Core.Models.Application>> GetApplicationsAsync(StorageMode storageMode, string owner)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var query = ApplyOwnerFilter(
                context.Applications
            .Include(a => a.ApplicationGroup)
            .AsQueryable(),
                storageMode,
                owner);

        return await query.ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IList<Core.Models.Application>> GetUngroupedApplicationsAsync(StorageMode storageMode, string owner)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var query = ApplyOwnerFilter(
            context.Applications
            .Where(a => a.ApplicationGroupId == null)
            .AsQueryable(),
            storageMode,
            owner);

        return await query.ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Core.Models.Application?> GetApplicationByIdAsync(int id)
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.Applications
            .Include(a => a.ApplicationGroup)
            .Include(a => a.Endpoints).ThenInclude(e => e.Headers)
            .Include(a => a.Endpoints).ThenInclude(e => e.QueryParameters)
            .Include(a => a.EndpointGroups)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    /// <inheritdoc/>
    public async Task<Core.Models.Application> AddApplicationAsync(Core.Models.Application application)
    {
        await using var context = await _factory.CreateDbContextAsync();
        context.Applications.Add(application);
        await context.SaveChangesAsync();
        return application;
    }

    /// <inheritdoc/>
    public async Task<Core.Models.Application> UpdateApplicationAsync(Core.Models.Application application)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var existing = await context.Applications.FindAsync(application.Id)
            ?? throw new InvalidOperationException($"Anwendung {application.Id} nicht gefunden.");
        context.Entry(existing).Property(e => e.RowVersion).OriginalValue = application.RowVersion;
        existing.ApplyUpdate(application);
        await context.SaveChangesAsync();
        return existing;
    }

    /// <inheritdoc/>
    public async Task DeleteApplicationAsync(int id)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var application = await context.Applications.FindAsync(id);
        if (application != null)
        {
            context.Applications.Remove(application);
            await context.SaveChangesAsync();
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetApplicationCountByGroupAsync(int groupId)
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.Applications.CountAsync(a => a.ApplicationGroupId == groupId);
    }

    /// <inheritdoc/>
    public async Task<int> GetEndpointCountByGroupAsync(int groupId)
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.Applications
            .Where(a => a.ApplicationGroupId == groupId)
            .SelectMany(a => a.Endpoints)
            .CountAsync();
    }

    private static IQueryable<Core.Models.Application> ApplyOwnerFilter(
        IQueryable<Core.Models.Application> query,
        StorageMode storageMode,
        string owner)
    {
        return storageMode == StorageMode.User
            ? query.Where(a => a.Owner == owner)
            : query;
    }
}
