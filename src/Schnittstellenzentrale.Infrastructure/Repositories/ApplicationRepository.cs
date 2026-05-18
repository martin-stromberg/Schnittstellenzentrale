using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Data;

namespace Schnittstellenzentrale.Infrastructure.Repositories;

/// <summary>EF-Core-Implementierung von <see cref="IApplicationRepository"/>.</summary>
public class ApplicationRepository : IApplicationRepository
{
    private readonly AppDbContext _context;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ApplicationRepository"/>.</summary>
    public ApplicationRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IList<ApplicationGroup>> GetGroupsAsync(StorageMode storageMode, string owner)
    {
        var query = _context.ApplicationGroups.AsNoTracking().Include(g => g.Applications).AsQueryable();
        if (storageMode == StorageMode.User)
        {
            var ownerGroupIds = await ApplyOwnerFilter(_context.Applications.AsQueryable(), storageMode, owner)
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
        return await _context.ApplicationGroups
            .AsNoTracking()
            .Include(g => g.Applications)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    /// <inheritdoc/>
    public async Task<ApplicationGroup?> GetSystemGroupAsync()
    {
        return await _context.ApplicationGroups
            .AsNoTracking()
            .Include(g => g.Applications)
            .FirstOrDefaultAsync(g => g.IsSystem);
    }

    /// <inheritdoc/>
    public async Task<ApplicationGroup> AddGroupAsync(ApplicationGroup group)
    {
        _context.ApplicationGroups.Add(group);
        await _context.SaveChangesAsync();
        return group;
    }

    /// <inheritdoc/>
    public async Task<ApplicationGroup> UpdateGroupAsync(ApplicationGroup group)
    {
        var tracked = _context.ChangeTracker.Entries<ApplicationGroup>()
            .FirstOrDefault(e => e.Entity.Id == group.Id);
        if (tracked != null)
            tracked.State = EntityState.Detached;

        _context.ApplicationGroups.Update(group);
        await _context.SaveChangesAsync();
        return group;
    }

    /// <inheritdoc/>
    public async Task DeleteGroupAsync(int id)
    {
        var group = await _context.ApplicationGroups.FindAsync(id);
        if (group != null)
        {
            _context.ApplicationGroups.Remove(group);
            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc/>
    public async Task<IList<Core.Models.Application>> GetApplicationsAsync(StorageMode storageMode, string owner)
    {
        var query = ApplyOwnerFilter(
                _context.Applications
            .AsNoTracking()
            .Include(a => a.ApplicationGroup)
            .AsQueryable(),
                storageMode,
                owner);

        return await query.ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IList<Core.Models.Application>> GetUngroupedApplicationsAsync(StorageMode storageMode, string owner)
    {
        var query = ApplyOwnerFilter(
            _context.Applications
            .AsNoTracking()
            .Where(a => a.ApplicationGroupId == null)
            .AsQueryable(),
            storageMode,
            owner);

        return await query.ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Core.Models.Application?> GetApplicationByIdAsync(int id)
    {
        return await _context.Applications
            .AsNoTracking()
            .Include(a => a.ApplicationGroup)
            .Include(a => a.Endpoints).ThenInclude(e => e.Headers)
            .Include(a => a.Endpoints).ThenInclude(e => e.QueryParameters)
            .Include(a => a.EndpointGroups)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    /// <inheritdoc/>
    public async Task<Core.Models.Application> AddApplicationAsync(Core.Models.Application application)
    {
        _context.Applications.Add(application);
        await _context.SaveChangesAsync();
        return application;
    }

    /// <inheritdoc/>
    public async Task<Core.Models.Application> UpdateApplicationAsync(Core.Models.Application application)
    {
        var trackedApp = _context.ChangeTracker.Entries<Core.Models.Application>()
            .FirstOrDefault(e => e.Entity.Id == application.Id);
        if (trackedApp != null)
            trackedApp.State = EntityState.Detached;

        // AsNoTracking queries set ApplicationGroup via fixup, which causes a tracking conflict
        // when Update() traverses the graph and finds an already-tracked ApplicationGroup instance.
        if (application.ApplicationGroup != null)
        {
            var trackedGroup = _context.ChangeTracker.Entries<ApplicationGroup>()
                .FirstOrDefault(e => e.Entity.Id == application.ApplicationGroup.Id);
            if (trackedGroup != null)
                trackedGroup.State = EntityState.Detached;
            application.ApplicationGroup = null;
        }

        _context.Applications.Update(application);
        await _context.SaveChangesAsync();
        return application;
    }

    /// <inheritdoc/>
    public async Task DeleteApplicationAsync(int id)
    {
        var application = await _context.Applications.FindAsync(id);
        if (application != null)
        {
            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();
        }
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
