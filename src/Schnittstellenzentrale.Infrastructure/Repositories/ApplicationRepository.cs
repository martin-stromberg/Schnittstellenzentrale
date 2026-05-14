using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Data;

namespace Schnittstellenzentrale.Infrastructure.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly AppDbContext _context;

    public ApplicationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IList<ApplicationGroup>> GetGroupsAsync(StorageMode storageMode, string owner)
    {
        var query = _context.ApplicationGroups.Include(g => g.Applications).AsQueryable();
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

    public async Task<ApplicationGroup?> GetGroupByIdAsync(int id)
    {
        return await _context.ApplicationGroups
            .Include(g => g.Applications)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<ApplicationGroup> AddGroupAsync(ApplicationGroup group)
    {
        _context.ApplicationGroups.Add(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task<ApplicationGroup> UpdateGroupAsync(ApplicationGroup group)
    {
        _context.ApplicationGroups.Update(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task DeleteGroupAsync(int id)
    {
        var group = await _context.ApplicationGroups.FindAsync(id);
        if (group != null)
        {
            _context.ApplicationGroups.Remove(group);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IList<Core.Models.Application>> GetApplicationsAsync(StorageMode storageMode, string owner)
    {
        var query = ApplyOwnerFilter(
                _context.Applications
            .Include(a => a.ApplicationGroup)
            .AsQueryable(),
                storageMode,
                owner);

        return await query.ToListAsync();
    }

    public async Task<IList<Core.Models.Application>> GetUngroupedApplicationsAsync(StorageMode storageMode, string owner)
    {
        var query = ApplyOwnerFilter(
            _context.Applications
            .Where(a => a.ApplicationGroupId == null)
            .AsQueryable(),
            storageMode,
            owner);

        return await query.ToListAsync();
    }

    public async Task<Core.Models.Application?> GetApplicationByIdAsync(int id)
    {
        return await _context.Applications
            .Include(a => a.ApplicationGroup)
            .Include(a => a.Endpoints).ThenInclude(e => e.Headers)
            .Include(a => a.Endpoints).ThenInclude(e => e.QueryParameters)
            .Include(a => a.EndpointGroups)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Core.Models.Application> AddApplicationAsync(Core.Models.Application application)
    {
        _context.Applications.Add(application);
        await _context.SaveChangesAsync();
        return application;
    }

    public async Task<Core.Models.Application> UpdateApplicationAsync(Core.Models.Application application)
    {
        _context.Applications.Update(application);
        await _context.SaveChangesAsync();
        return application;
    }

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
