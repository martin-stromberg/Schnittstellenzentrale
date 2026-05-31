using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Data;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>Implementierung von <see cref="IHistoryService"/>.</summary>
public class HistoryService : IHistoryService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    /// <summary>Initialisiert eine neue Instanz von <see cref="HistoryService"/>.</summary>
    public HistoryService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    /// <inheritdoc/>
    public async Task AddEntryAsync(EndpointCallHistoryEntry entry)
    {
        await using var context = await _factory.CreateDbContextAsync();
        context.EndpointCallHistory.Add(entry);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<(IList<EndpointCallHistoryEntry> Items, int TotalCount)> GetPagedAsync(HistoryFilter filter, int page, int pageSize)
    {
        page = Math.Max(1, page);
        await using var context = await _factory.CreateDbContextAsync();
        var query = context.EndpointCallHistory.Include(e => e.Application).AsQueryable();

        if (filter.ApplicationId.HasValue)
            query = query.Where(e => e.ApplicationId == filter.ApplicationId.Value);

        if (filter.EndpointId.HasValue)
            query = query.Where(e => e.EndpointId == filter.EndpointId.Value);

        if (filter.From.HasValue)
            query = query.Where(e => e.ExecutedAt >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(e => e.ExecutedAt <= filter.To.Value);

        query = query.OrderByDescending(e => e.ExecutedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<IList<TopEndpointResult>> GetTopEndpointsAsync(int applicationId, int count)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var rows = await context.EndpointCallHistory
            .Where(e => e.ApplicationId == applicationId && e.EndpointId != null)
            .GroupBy(e => e.EndpointId)
            .Select(g => new { EndpointId = g.Key, CallCount = g.Count() })
            .OrderByDescending(r => r.CallCount)
            .Take(count)
            .GroupJoin(context.Endpoints,
                r => r.EndpointId,
                e => e.Id,
                (r, endpoints) => new { r.EndpointId, r.CallCount, Endpoints = endpoints })
            .SelectMany(
                r => r.Endpoints.DefaultIfEmpty(),
                (r, e) => new TopEndpointResult(r.EndpointId!.Value, e != null ? e.RelativePath : "(gelöscht)", e != null ? e.Method.ToString() : string.Empty, r.CallCount))
            .ToListAsync();

        return rows.OrderByDescending(r => r.CallCount).ToList();
    }
}
