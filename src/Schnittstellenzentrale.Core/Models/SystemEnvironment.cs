using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Models;

/// <summary>Systemumgebung mit Variablen und Speichermodus.</summary>
public class SystemEnvironment
{
    /// <summary>Der eindeutige Bezeichner.</summary>
    public int Id { get; set; }

    /// <summary>Der Name der Umgebung.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Der Speichermodus.</summary>
    public StorageMode Mode { get; set; }

    /// <summary>Der Eigentümer (optional).</summary>
    public string? Owner { get; set; }

    /// <summary>Die Beschreibung (optional).</summary>
    public string? Description { get; set; }

    /// <summary>Die enthaltenen Umgebungsvariablen.</summary>
    public ICollection<EnvironmentVariable> Variables { get; set; } = [];
}
