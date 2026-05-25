using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Infrastructure.Data;

namespace Schnittstellenzentrale.Infrastructure.Repositories;

/// <summary>EF-Core-Implementierung von <see cref="ISystemEnvironmentRepository"/>.</summary>
public class SystemEnvironmentRepository : ISystemEnvironmentRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>Initialisiert eine neue Instanz von <see cref="SystemEnvironmentRepository"/>.</summary>
    public SystemEnvironmentRepository(IDbContextFactory<AppDbContext> factory, ICurrentUserService currentUserService)
    {
        _factory = factory;
        _currentUserService = currentUserService;
    }

    /// <inheritdoc/>
    public async Task<IList<SystemEnvironment>> GetEnvironmentsAsync(StorageMode mode, string? owner)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var query = context.SystemEnvironments
            .Include(e => e.Variables)
            .AsQueryable();
        query = ApplyOwnerFilter(query, mode, owner);
        return await query.OrderBy(e => e.Name).ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<SystemEnvironment?> GetByIdAsync(int id)
    {
        await using var context = await _factory.CreateDbContextAsync();
        return await context.SystemEnvironments
            .Include(e => e.Variables)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <inheritdoc/>
    public async Task<SystemEnvironment> AddAsync(SystemEnvironment systemEnvironment)
    {
        if (systemEnvironment.Mode == StorageMode.User)
            systemEnvironment.Owner = _currentUserService.GetCurrentUserName();

        await using var context = await _factory.CreateDbContextAsync();
        context.SystemEnvironments.Add(systemEnvironment);
        await context.SaveChangesAsync();
        return systemEnvironment;
    }

    /// <inheritdoc/>
    public async Task<SystemEnvironment> UpdateAsync(SystemEnvironment systemEnvironment)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var existing = await context.SystemEnvironments
            .Include(e => e.Variables)
            .FirstOrDefaultAsync(e => e.Id == systemEnvironment.Id)
            ?? throw new InvalidOperationException($"Systemumgebung {systemEnvironment.Id} nicht gefunden.");

        existing.Name = systemEnvironment.Name;
        existing.Mode = systemEnvironment.Mode;

        var existingVariableIds = existing.Variables.Select(v => v.Id).ToHashSet();
        var updatedVariableIds = systemEnvironment.Variables.Where(v => v.Id != 0).Select(v => v.Id).ToHashSet();

        foreach (var toRemove in existing.Variables.Where(v => !updatedVariableIds.Contains(v.Id)).ToList())
            existing.Variables.Remove(toRemove);

        foreach (var variable in systemEnvironment.Variables)
        {
            if (variable.Id == 0)
            {
                existing.Variables.Add(variable);
            }
            else
            {
                var existingVar = existing.Variables.FirstOrDefault(v => v.Id == variable.Id);
                if (existingVar != null)
                {
                    existingVar.Name = variable.Name;
                    existingVar.Value = variable.Value;
                    existingVar.IsValueMasked = variable.IsValueMasked;
                }
            }
        }

        await context.SaveChangesAsync();
        return existing;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var environment = await context.SystemEnvironments.FindAsync(id);
        if (environment != null)
        {
            context.SystemEnvironments.Remove(environment);
            await context.SaveChangesAsync();
        }
    }

    /// <inheritdoc/>
    public async Task UpdateVariableAsync(int environmentId, string name, string value)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var existing = await context.SystemEnvironments
            .Include(e => e.Variables)
            .FirstOrDefaultAsync(e => e.Id == environmentId)
            ?? throw new InvalidOperationException($"Systemumgebung {environmentId} nicht gefunden.");

        var variable = existing.Variables.FirstOrDefault(v => v.Name == name);
        if (variable != null)
        {
            variable.Value = value;
        }
        else
        {
            existing.Variables.Add(new EnvironmentVariable { Name = name, Value = value });
        }

        await context.SaveChangesAsync();
    }

    private static IQueryable<SystemEnvironment> ApplyOwnerFilter(
        IQueryable<SystemEnvironment> query,
        StorageMode mode,
        string? owner)
    {
        return mode == StorageMode.User
            ? query.Where(e => e.Mode == StorageMode.User && e.Owner == owner)
            : query.Where(e => e.Mode == StorageMode.Team);
    }
}
