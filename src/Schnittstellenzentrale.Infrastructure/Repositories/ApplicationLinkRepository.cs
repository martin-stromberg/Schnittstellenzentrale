using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Data;

namespace Schnittstellenzentrale.Infrastructure.Repositories;

/// <summary>EF-Core-Implementierung von <see cref="IApplicationLinkRepository"/>.</summary>
public class ApplicationLinkRepository : IApplicationLinkRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ApplicationLinkRepository"/>.</summary>
    public ApplicationLinkRepository(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    /// <inheritdoc/>
    public async Task<IList<ApplicationLink>> GetByApplicationIdAsync(int applicationId)
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.ApplicationLinks
            .Where(l => l.ApplicationId == applicationId)
            .OrderBy(l => l.SortOrder)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<ApplicationLink> AddAsync(ApplicationLink link)
    {
        await using var context = await _factory.CreateDbContextAsync();
        context.ApplicationLinks.Add(link);
        await context.SaveChangesAsync();
        return link;
    }

    /// <inheritdoc/>
    public async Task<ApplicationLink> UpdateAsync(ApplicationLink link)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var existing = await context.ApplicationLinks.FindAsync(link.Id)
            ?? throw new InvalidOperationException($"Link {link.Id} nicht gefunden.");
        context.Entry(existing).Property(e => e.RowVersion).OriginalValue = link.RowVersion;
        existing.Url = link.Url;
        existing.Label = link.Label;
        existing.SortOrder = link.SortOrder;
        await context.SaveChangesAsync();
        return existing;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int linkId)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var link = await context.ApplicationLinks.FindAsync(linkId);
        if (link != null)
        {
            context.ApplicationLinks.Remove(link);
            await context.SaveChangesAsync();
        }
    }
}
