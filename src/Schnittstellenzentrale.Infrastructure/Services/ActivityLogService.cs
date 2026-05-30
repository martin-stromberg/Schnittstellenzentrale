using Microsoft.Extensions.Logging;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>Scoped In-Memory-Implementierung von <see cref="IActivityLogService"/>.</summary>
public class ActivityLogService : IActivityLogService
{
    private readonly List<ActivityLogEntry> _entries = [];
    private readonly Lock _entriesLock = new();
    private readonly ILogger<ActivityLogService> _logger;

    /// <summary>Initialisiert eine neue Instanz des <see cref="ActivityLogService"/>.</summary>
    public ActivityLogService(ILogger<ActivityLogService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public IReadOnlyList<ActivityLogEntry> Entries { get { lock (_entriesLock) { return _entries.ToList(); } } }

    /// <inheritdoc/>
    public event Action? OnEntryAdded;

    /// <inheritdoc/>
    public event Action? OnCleared;

    /// <inheritdoc/>
    public void Log(ActivityLogCategory category, string message, string? details = null)
    {
        var entry = new ActivityLogEntry
        {
            Timestamp = DateTime.Now,
            Category = category,
            Message = message,
            Details = details
        };

        lock (_entriesLock)
        {
            _entries.Add(entry);
        }

        try
        {
            OnEntryAdded?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Fehler im OnEntryAdded-Event-Handler.");
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        lock (_entriesLock)
        {
            _entries.Clear();
        }

        try
        {
            OnCleared?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Fehler im OnCleared-Event-Handler.");
        }
    }
}
