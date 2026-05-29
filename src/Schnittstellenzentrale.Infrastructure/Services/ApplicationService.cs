using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Infrastructure.Data;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>Implementierung von <see cref="IApplicationService"/>.</summary>
public class ApplicationService : IApplicationService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly UploadSettings _uploadSettings;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ApplicationService"/>.</summary>
    public ApplicationService(IDbContextFactory<AppDbContext> factory, IOptions<UploadSettings> uploadSettings)
    {
        _factory = factory;
        _uploadSettings = uploadSettings.Value;
    }

    /// <inheritdoc/>
    public async Task UpdateNameAsync(int applicationId, string name)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var application = await context.Applications.FindAsync(applicationId)
            ?? throw new InvalidOperationException($"Anwendung {applicationId} nicht gefunden.");
        application.Name = name;
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateSubtitleAsync(int applicationId, string? subtitle)
    {
        await using var context = await _factory.CreateDbContextAsync();
        var application = await context.Applications.FindAsync(applicationId)
            ?? throw new InvalidOperationException($"Anwendung {applicationId} nicht gefunden.");
        application.Subtitle = subtitle;
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateIconAsync(int applicationId, byte[] iconData)
    {
        if (iconData.Length > _uploadSettings.MaxIconSizeBytes)
            throw new InvalidOperationException($"Icon-Datei überschreitet die maximale Größe von {_uploadSettings.MaxIconSizeBytes} Bytes.");

        await using var context = await _factory.CreateDbContextAsync();
        var application = await context.Applications.FindAsync(applicationId)
            ?? throw new InvalidOperationException($"Anwendung {applicationId} nicht gefunden.");
        application.IconData = iconData;
        await context.SaveChangesAsync();
    }
}
