namespace Schnittstellenzentrale.Core.Contracts;

/// <summary>
/// DTO für eine Anwendungsgruppe.
/// </summary>
public class ApplicationGroupResponse
{
    /// <summary>
    /// Eindeutige ID der Gruppe.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name der Gruppe.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gibt an, ob es sich um eine Systemgruppe handelt.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Optionale Beschreibung der Gruppe.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optionaler Untertitel der Gruppe.
    /// </summary>
    public string? Subtitle { get; set; }

    /// <summary>
    /// Optionale Icon-Daten der Gruppe.
    /// </summary>
    public byte[]? IconData { get; set; }

    /// <summary>
    /// Versionsstempel für die optimistische Parallelitätskontrolle.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];

    /// <summary>
    /// Anwendungen, die dieser Gruppe zugeordnet sind.
    /// </summary>
    public IList<ApplicationResponse> Applications { get; set; } = [];
}
