namespace Schnittstellenzentrale.Core.Contracts;

/// <summary>
/// DTO für eine Endpunktgruppe.
/// </summary>
public class EndpointGroupResponse
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
    /// ID der Anwendung, zu der die Gruppe gehört.
    /// </summary>
    public int ApplicationId { get; set; }

    /// <summary>
    /// Optionale ID der übergeordneten Gruppe.
    /// </summary>
    public int? ParentGroupId { get; set; }

    /// <summary>
    /// Versionsstempel für die optimistische Parallelitätskontrolle.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];
}
