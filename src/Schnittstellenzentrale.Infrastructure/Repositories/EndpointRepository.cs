using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Data;

namespace Schnittstellenzentrale.Infrastructure.Repositories;

public class EndpointRepository : IEndpointRepository
{
    private readonly AppDbContext _context;

    public EndpointRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IList<Core.Models.Endpoint>> GetEndpointsAsync(int applicationId)
    {
        return await _context.Endpoints
            .Include(e => e.Headers)
            .Include(e => e.QueryParameters)
            .Include(e => e.EndpointGroup)
            .Where(e => e.ApplicationId == applicationId)
            .ToListAsync();
    }

    public async Task<IList<Core.Models.Endpoint>> GetUngroupedEndpointsAsync(int applicationId)
    {
        return await _context.Endpoints
            .Include(e => e.Headers)
            .Include(e => e.QueryParameters)
            .Include(e => e.EndpointGroup)
            .Where(e => e.ApplicationId == applicationId && e.EndpointGroupId == null)
            .ToListAsync();
    }

    public async Task<Core.Models.Endpoint?> GetEndpointByIdAsync(int id)
    {
        return await _context.Endpoints
            .Include(e => e.Headers)
            .Include(e => e.QueryParameters)
            .Include(e => e.EndpointGroup)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Core.Models.Endpoint> AddEndpointAsync(Core.Models.Endpoint endpoint)
    {
        _context.Endpoints.Add(endpoint);
        await _context.SaveChangesAsync();
        return endpoint;
    }

    public async Task<Core.Models.Endpoint> UpdateEndpointAsync(Core.Models.Endpoint endpoint)
    {
        _context.Endpoints.Update(endpoint);
        await _context.SaveChangesAsync();
        return endpoint;
    }

    public async Task DeleteEndpointAsync(int id)
    {
        await DeleteByIdAsync(_context.Endpoints, id);
    }

    public async Task<IList<EndpointGroup>> GetEndpointGroupsAsync(int applicationId)
    {
        return await _context.EndpointGroups
            .Where(g => g.ApplicationId == applicationId)
            .ToListAsync();
    }

    public async Task<EndpointGroup?> GetEndpointGroupByIdAsync(int id)
    {
        return await _context.EndpointGroups
            .Include(g => g.Endpoints)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<EndpointGroup> AddEndpointGroupAsync(EndpointGroup group)
    {
        _context.EndpointGroups.Add(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task<EndpointGroup> UpdateEndpointGroupAsync(EndpointGroup group)
    {
        _context.EndpointGroups.Update(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task DeleteEndpointGroupAsync(int id)
    {
        await DeleteByIdAsync(_context.EndpointGroups, id);
    }

    public async Task<EndpointHeader> AddHeaderAsync(EndpointHeader header)
    {
        _context.EndpointHeaders.Add(header);
        await _context.SaveChangesAsync();
        return header;
    }

    public async Task DeleteHeaderAsync(int id)
    {
        await DeleteByIdAsync(_context.EndpointHeaders, id);
    }

    public async Task<EndpointQueryParameter> AddQueryParameterAsync(EndpointQueryParameter parameter)
    {
        _context.EndpointQueryParameters.Add(parameter);
        await _context.SaveChangesAsync();
        return parameter;
    }

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
