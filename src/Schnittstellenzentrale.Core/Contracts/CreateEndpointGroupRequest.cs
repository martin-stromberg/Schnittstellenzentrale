using System.ComponentModel.DataAnnotations;

namespace Schnittstellenzentrale.Core.Contracts;

/// <summary>
/// Anfrage zum Erstellen einer Endpunktgruppe.
/// </summary>
public class CreateEndpointGroupRequest
{
    /// <summary>
    /// Name der neuen Gruppe.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ID der Anwendung, zu der die Gruppe gehört.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int ApplicationId { get; set; }

    /// <summary>
    /// Optionale ID der übergeordneten Gruppe.
    /// </summary>
    public int? ParentGroupId { get; set; }
}
