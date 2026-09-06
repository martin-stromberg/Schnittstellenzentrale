namespace Schnittstellenzentrale.Core.Models;

/// <summary>Gruppierung von Anwendungen.</summary>
public class ApplicationGroup
{
    /// <summary>Der eindeutige Bezeichner.</summary>
    public int Id { get; set; }

    /// <summary>Der Name der Gruppe.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gibt an, ob es sich um einen Systemeintrag handelt.</summary>
    public bool IsSystem { get; set; } = false;

    /// <summary>Die Beschreibung (optional).</summary>
    public string? Description { get; set; }

    /// <summary>Der Untertitel (optional).</summary>
    public string? Subtitle { get; set; }

    /// <summary>Icon-Daten (optional).</summary>
    public byte[]? IconData { get; set; }

    /// <summary>Die Zeilenversionskennung für Optimistic Concurrency.</summary>
    public byte[] RowVersion { get; set; } = [];

    /// <summary>Die enthaltenen Anwendungen.</summary>
    public ICollection<Application> Applications { get; set; } = [];
}
