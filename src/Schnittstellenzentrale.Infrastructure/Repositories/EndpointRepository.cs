using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Data;

namespace Schnittstellenzentrale.Infrastructure.Repositories;

/// <summary>EF-Core-Implementierung von <see cref="IEndpointRepository"/>.</summary>
public class EndpointRepository : IEndpointRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    /// <summary>Initialisiert eine neue Instanz des <see cref="EndpointRepository"/>.</summary>
    public EndpointRepository(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    /// <inheritdoc/>
    public async Task<IList<Core.Models.Endpoint>> GetEndpointsAsync(int applicationId)
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.Endpoints
            .Include(e => e.Headers)
            .Include(e => e.QueryParameters)
            .Include(e => e.EndpointGroup)
            .Where(e => e.ApplicationId == applicationId)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Core.Models.Endpoint?> GetEndpointByIdAsync(int id)
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.Endpoints
            .Include(e => e.Application)
            .Include(e => e.Headers)
            .Include(e => e.QueryParameters)
            .Include(e => e.EndpointGroup)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <inheritdoc/>
    public async Task<Core.Models.Endpoint> AddEndpointAsync(Core.Models.Endpoint endpoint)
    {
        await using var context = await _factory.CreateDbContextAsync();
        context.Endpoints.Add(endpoint);
        await context.SaveChangesAsync();
        return endpoint;
    }

    /// <inheritdoc/>
    public async Task<Core.Models.Endpoint> UpdateEndpointAsync(Core.Models.Endpoint endpoint)
    {
        await using var context = await _factory.CreateDbContextAsync();

        var existing = await context.Endpoints
            .Include(e => e.Headers)
            .Include(e => e.QueryParameters)
            .FirstOrDefaultAsync(e => e.Id == endpoint.Id)
            ?? throw new InvalidOperationException($"Endpunkt {endpoint.Id} nicht gefunden.");

        context.Entry(existing).Property(e => e.RowVersion).OriginalValue = endpoint.RowVersion;

        existing.ApplyUpdate(endpoint);

        foreach (var header in existing.Headers.ToList())
            context.Remove(header);
        foreach (var param in existing.QueryParameters.ToList())
            context.Remove(param);

        foreach (var header in endpoint.Headers)
            context.EndpointHeaders.Add(new EndpointHeader { Key = header.Key, Value = header.Value, EndpointId = existing.Id });
        foreach (var param in endpoint.QueryParameters)
            context.EndpointQueryParameters.Add(new EndpointQueryParameter { Key = param.Key, Value = param.Value, EndpointId = existing.Id });

        await context.SaveChangesAsync();
        return existing;
    }

    /// <inheritdoc/>
    public async Task DeleteEndpointAsync(int id)
    {
        await DeleteByIdAsync<Core.Models.Endpoint>(id);
    }

    /// <inheritdoc/>
    public async Task<IList<EndpointGroup>> GetEndpointGroupsAsync(int applicationId)
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.EndpointGroups
            .Where(g => g.ApplicationId == applicationId)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<EndpointGroup?> GetEndpointGroupByIdAsync(int id)
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.EndpointGroups
            .Include(g => g.Endpoints)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    /// <inheritdoc/>
    public async Task<EndpointGroup> AddEndpointGroupAsync(EndpointGroup group)
    {
        await using var context = await _factory.CreateDbContextAsync();
        context.EndpointGroups.Add(group);
        await context.SaveChangesAsync();
        return group;
    }

    /// <inheritdoc/>
    public async Task<EndpointGroup> UpdateEndpointGroupAsync(EndpointGroup group)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var existing = await context.EndpointGroups.FindAsync(group.Id)
            ?? throw new InvalidOperationException($"Endpunktgruppe {group.Id} nicht gefunden.");
        context.Entry(existing).Property(e => e.RowVersion).OriginalValue = group.RowVersion;
        existing.ApplyUpdate(group);
        await context.SaveChangesAsync();
        return existing;
    }

    /// <inheritdoc/>
    public async Task DeleteEndpointGroupAsync(int id)
    {
        await DeleteByIdAsync<EndpointGroup>(id);
    }

    /// <inheritdoc/>
    public async Task<EndpointHeader> AddHeaderAsync(EndpointHeader header)
    {
        await using var context = await _factory.CreateDbContextAsync();
        context.EndpointHeaders.Add(header);
        await context.SaveChangesAsync();
        return header;
    }

    /// <inheritdoc/>
    public async Task DeleteHeaderAsync(int id)
    {
        await DeleteByIdAsync<EndpointHeader>(id);
    }

    /// <inheritdoc/>
    public async Task<EndpointQueryParameter> AddQueryParameterAsync(EndpointQueryParameter parameter)
    {
        await using var context = await _factory.CreateDbContextAsync();
        context.EndpointQueryParameters.Add(parameter);
        await context.SaveChangesAsync();
        return parameter;
    }

    /// <inheritdoc/>
    public async Task DeleteQueryParameterAsync(int id)
    {
        await DeleteByIdAsync<EndpointQueryParameter>(id);
    }

    private async Task DeleteByIdAsync<T>(int id) where T : class
    {
        await using var context = await _factory.CreateDbContextAsync();
        var entity = await context.Set<T>().FindAsync(id);
        if (entity is not null)
        {
            context.Set<T>().Remove(entity);
            await context.SaveChangesAsync();
        }
    }
}
