namespace Schnittstellenzentrale.Core.Contracts;

/// <summary>
/// DTO für eine Anwendung.
/// </summary>
public class ApplicationResponse
{
    /// <summary>
    /// Eindeutige ID der Anwendung.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name der Anwendung.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gibt an, ob es sich um eine Systemanwendung handelt.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Basis-URL der Anwendung.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Optionale ID der zugeordneten Anwendungsgruppe.
    /// </summary>
    public int? ApplicationGroupId { get; set; }

    /// <summary>
    /// Beschreibung der Anwendung.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Optionale URL zur Schnittstellendokumentation.
    /// </summary>
    public string? InterfaceUrl { get; set; }

    /// <summary>
    /// Typ der Schnittstelle (siehe <see cref="Enums.InterfaceType"/>).
    /// </summary>
    public int InterfaceType { get; set; }

    /// <summary>
    /// Optionaler Besitzer der Anwendung.
    /// </summary>
    public string? Owner { get; set; }

    /// <summary>
    /// Optionaler Untertitel der Anwendung.
    /// </summary>
    public string? Subtitle { get; set; }

    /// <summary>
    /// Optionale Icon-Daten der Anwendung.
    /// </summary>
    public byte[]? IconData { get; set; }

    /// <summary>
    /// Versionsstempel für die optimistische Parallelitätskontrolle.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];
}
