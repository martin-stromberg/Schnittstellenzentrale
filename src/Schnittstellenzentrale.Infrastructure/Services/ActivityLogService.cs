using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>Scoped In-Memory-Implementierung von <see cref="IActivityLogService"/>.</summary>
public class ActivityLogService : IActivityLogService
{
    private readonly List<ActivityLogEntry> _entries = [];

    /// <inheritdoc/>
    public IReadOnlyList<ActivityLogEntry> Entries => _entries;

    /// <inheritdoc/>
    public event Action? OnEntryAdded;

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

        _entries.Add(entry);

        try
        {
            OnEntryAdded?.Invoke();
        }
        catch (Exception)
        {
            // Fehler im Event-Handler werden ignoriert
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _entries.Clear();
    }
}
