using System.ComponentModel.DataAnnotations;

namespace Schnittstellenzentrale.Core.Contracts;

/// <summary>
/// Anfrage zum Aktualisieren einer Anwendung.
/// </summary>
public class UpdateApplicationRequest
{
    /// <summary>
    /// Name der Anwendung.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Basis-URL der Anwendung.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Optionale Beschreibung der Anwendung.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optionale URL zur Schnittstellendokumentation.
    /// </summary>
    [MaxLength(500)]
    public string? InterfaceUrl { get; set; }

    /// <summary>
    /// Optionale ID der zugeordneten Anwendungsgruppe.
    /// </summary>
    public int? ApplicationGroupId { get; set; }

    /// <summary>
    /// Optionaler Besitzer der Anwendung.
    /// </summary>
    [MaxLength(256)]
    public string? Owner { get; set; }
}
