using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Data;

namespace Schnittstellenzentrale.Infrastructure.Repositories;

/// <summary>EF-Core-Implementierung von <see cref="IEndpointRepository"/>.</summary>
public class EndpointRepository : IEndpointRepository
{
    private readonly AppDbContext _context;

    /// <summary>Initialisiert eine neue Instanz des <see cref="EndpointRepository"/>.</summary>
    public EndpointRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IList<Core.Models.Endpoint>> GetEndpointsAsync(int applicationId)
    {
        return await _context.Endpoints
            .AsNoTracking()
            .Include(e => e.Headers)
            .Include(e => e.QueryParameters)
            .Include(e => e.EndpointGroup)
            .Where(e => e.ApplicationId == applicationId)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Core.Models.Endpoint?> GetEndpointByIdAsync(int id)
    {
        return await _context.Endpoints
            .AsNoTracking()
            .Include(e => e.Application)
            .Include(e => e.Headers)
            .Include(e => e.QueryParameters)
            .Include(e => e.EndpointGroup)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <inheritdoc/>
    public async Task<Core.Models.Endpoint> AddEndpointAsync(Core.Models.Endpoint endpoint)
    {
        _context.ChangeTracker.Clear();
        _context.Endpoints.Add(endpoint);
        await _context.SaveChangesAsync();
        _context.Entry(endpoint).State = EntityState.Detached;
        return endpoint;
    }

    /// <inheritdoc/>
    public async Task<Core.Models.Endpoint> UpdateEndpointAsync(Core.Models.Endpoint endpoint)
    {
        // Clear all tracked entities before Update so that navigation-property relationship
        // fixup cannot find stale references from a previous operation in the long-lived
        // Blazor Server DbContext (e.g. Application.Endpoints still pointing to an old instance).
        _context.ChangeTracker.Clear();
        _context.Endpoints.Update(endpoint);
        await _context.SaveChangesAsync();
        _context.Entry(endpoint).State = EntityState.Detached;
        return endpoint;
    }

    /// <inheritdoc/>
    public async Task DeleteEndpointAsync(int id)
    {
        await DeleteByIdAsync(_context.Endpoints, id);
    }

    /// <inheritdoc/>
    public async Task<IList<EndpointGroup>> GetEndpointGroupsAsync(int applicationId)
    {
        return await _context.EndpointGroups
            .AsNoTracking()
            .Where(g => g.ApplicationId == applicationId)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<EndpointGroup?> GetEndpointGroupByIdAsync(int id)
    {
        return await _context.EndpointGroups
            .AsNoTracking()
            .Include(g => g.Endpoints)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    /// <inheritdoc/>
    public async Task<EndpointGroup> AddEndpointGroupAsync(EndpointGroup group)
    {
        _context.ChangeTracker.Clear();
        _context.EndpointGroups.Add(group);
        await _context.SaveChangesAsync();
        _context.Entry(group).State = EntityState.Detached;
        return group;
    }

    /// <inheritdoc/>
    public async Task<EndpointGroup> UpdateEndpointGroupAsync(EndpointGroup group)
    {
        _context.ChangeTracker.Clear();
        _context.EndpointGroups.Update(group);
        await _context.SaveChangesAsync();
        _context.Entry(group).State = EntityState.Detached;
        return group;
    }

    /// <inheritdoc/>
    public async Task DeleteEndpointGroupAsync(int id)
    {
        await DeleteByIdAsync(_context.EndpointGroups, id);
    }

    /// <inheritdoc/>
    public async Task<EndpointHeader> AddHeaderAsync(EndpointHeader header)
    {
        _context.EndpointHeaders.Add(header);
        await _context.SaveChangesAsync();
        _context.Entry(header).State = EntityState.Detached;
        return header;
    }

    /// <inheritdoc/>
    public async Task DeleteHeaderAsync(int id)
    {
        await DeleteByIdAsync(_context.EndpointHeaders, id);
    }

    /// <inheritdoc/>
    public async Task<EndpointQueryParameter> AddQueryParameterAsync(EndpointQueryParameter parameter)
    {
        _context.EndpointQueryParameters.Add(parameter);
        await _context.SaveChangesAsync();
        _context.Entry(parameter).State = EntityState.Detached;
        return parameter;
    }

    /// <inheritdoc/>
    public async Task DeleteQueryParameterAsync(int id)
    {
        await DeleteByIdAsync(_context.EndpointQueryParameters, id);
    }

    private async Task DeleteByIdAsync<T>(DbSet<T> dbSet, int id) where T : class
    {
        var entity = await dbSet.FindAsync(id);
        if (entity == null)
            return;

        dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
