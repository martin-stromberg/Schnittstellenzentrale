using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Bietet Zugriff auf das Aktivitätsprotokoll.</summary>
public interface IActivityLogService
{
    /// <summary>Ruft die Aktivitätsprotokolleinträge ab.</summary>
    IReadOnlyList<ActivityLogEntry> Entries { get; }

    /// <summary>Tritt ein, wenn ein neuer Eintrag hinzugefügt wird.</summary>
    event Action? OnEntryAdded;

    /// <summary>Tritt ein, wenn das Protokoll geleert wird.</summary>
    event Action? OnCleared;

    /// <summary>Fügt dem Aktivitätsprotokoll einen Eintrag hinzu.</summary>
    /// <param name="category">Kategorie des Eintrags.</param>
    /// <param name="message">Nachricht des Eintrags.</param>
    /// <param name="details">Optionale Details.</param>
    void Log(ActivityLogCategory category, string message, string? details = null);

    /// <summary>Leert das Aktivitätsprotokoll.</summary>
    void Clear();
}
