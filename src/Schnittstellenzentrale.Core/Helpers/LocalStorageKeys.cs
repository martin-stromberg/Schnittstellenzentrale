using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Helpers;

/// <summary>Schlüssel für lokale Browser-Speicherwerte.</summary>
public static class LocalStorageKeys
{
    /// <summary>Gibt den Schlüssel für die aktuell gewählte Umgebungs-ID im gewählten Speichermodus zurück.</summary>
    /// <param name="mode">Der Speichermodus.</param>
    /// <returns>Der lokale Speicherschlüssel.</returns>
    public static string SelectedEnvironmentId(StorageMode mode) => $"selectedEnvironmentId_{mode}";

    /// <summary>Schlüssel für die Anzeigeart des Aktivitätsprotokolls.</summary>
    public const string ActivityLogDisplayMode = "activityLogDisplayMode";

    /// <summary>Schlüssel für die Höhe des Aktivitätsprotokoll-Panels.</summary>
    public const string ActivityLogPanelHeight = "activityLogPanelHeight";

    /// <summary>Schlüssel für den gewählten Speichermodus.</summary>
    public const string StorageMode = "storageMode";
}
