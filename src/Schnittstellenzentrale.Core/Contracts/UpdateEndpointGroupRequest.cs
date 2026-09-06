using System.ComponentModel.DataAnnotations;

namespace Schnittstellenzentrale.Core.Contracts;

/// <summary>
/// Anfrage zum Aktualisieren einer Endpunktgruppe.
/// </summary>
public class UpdateEndpointGroupRequest
{
    /// <summary>
    /// Name der Gruppe.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Versionsstempel für die optimistische Parallelitätskontrolle.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];
}
