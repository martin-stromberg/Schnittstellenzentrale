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
        var query = context.EndpointCallHistory.AsQueryable();

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
            .GroupBy(e => new { e.EndpointId, e.RelativePath, e.HttpMethod })
            .Select(g => new { g.Key.EndpointId, g.Key.RelativePath, g.Key.HttpMethod, CallCount = g.Count() })
            .OrderByDescending(r => r.CallCount)
            .Take(count)
            .ToListAsync();

        return rows
            .Select(r => new TopEndpointResult(r.EndpointId!.Value, r.RelativePath, r.HttpMethod, r.CallCount))
            .ToList();
    }
}
