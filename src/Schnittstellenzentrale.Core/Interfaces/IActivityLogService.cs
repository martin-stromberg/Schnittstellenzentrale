using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

public interface IActivityLogService
{
    IReadOnlyList<ActivityLogEntry> Entries { get; }
    event Action? OnEntryAdded;
    void Log(ActivityLogCategory category, string message, string? details = null);
    void Clear();
}
