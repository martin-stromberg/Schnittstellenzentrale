using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Models;

/// <summary>Eintrag im Aktivitätsprotokoll.</summary>
public class ActivityLogEntry
{
    /// <summary>Zeitpunkt des Eintrags.</summary>
    public DateTime Timestamp { get; init; }

    /// <summary>Kategorie des Eintrags.</summary>
    public ActivityLogCategory Category { get; init; }

    /// <summary>Nachricht des Eintrags.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Zusätzliche Details (optional).</summary>
    public string? Details { get; init; }
}
