using System.ComponentModel.DataAnnotations;

namespace Schnittstellenzentrale.Core.Contracts;

/// <summary>
/// Anfrage zum Aktualisieren einer Anwendungsgruppe.
/// </summary>
public class UpdateApplicationGroupRequest
{
    /// <summary>
    /// Name der Gruppe.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}
