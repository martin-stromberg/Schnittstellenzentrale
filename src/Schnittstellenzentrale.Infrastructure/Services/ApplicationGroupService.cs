using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Infrastructure.Data;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>Implementierung von <see cref="IApplicationGroupService"/>.</summary>
public class ApplicationGroupService : IApplicationGroupService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly UploadSettings _uploadSettings;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ApplicationGroupService"/>.</summary>
    public ApplicationGroupService(IDbContextFactory<AppDbContext> factory, IOptions<UploadSettings> uploadSettings)
    {
        _factory = factory;
        _uploadSettings = uploadSettings.Value;
    }

    /// <inheritdoc/>
    public async Task UpdateNameAsync(int groupId, string name)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var group = await context.ApplicationGroups.FindAsync(groupId)
            ?? throw new InvalidOperationException($"Gruppe {groupId} nicht gefunden.");
        group.Name = name;
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateDescriptionAsync(int groupId, string? description)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var group = await context.ApplicationGroups.FindAsync(groupId)
            ?? throw new InvalidOperationException($"Gruppe {groupId} nicht gefunden.");
        group.Description = description;
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateSubtitleAsync(int groupId, string? subtitle)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var group = await context.ApplicationGroups.FindAsync(groupId)
            ?? throw new InvalidOperationException($"Gruppe {groupId} nicht gefunden.");
        group.Subtitle = subtitle;
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateIconAsync(int groupId, byte[] iconData)
    {
        if (iconData.Length > _uploadSettings.MaxIconSizeBytes)
            throw new InvalidOperationException($"Icon-Datei überschreitet die maximale Größe von {_uploadSettings.MaxIconSizeBytes} Bytes.");

        await using var context = await _factory.CreateDbContextAsync();
        var group = await context.ApplicationGroups.FindAsync(groupId)
            ?? throw new InvalidOperationException($"Gruppe {groupId} nicht gefunden.");
        group.IconData = iconData;
        await context.SaveChangesAsync();
    }
}
