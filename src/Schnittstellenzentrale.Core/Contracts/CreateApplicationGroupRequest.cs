using System.ComponentModel.DataAnnotations;

namespace Schnittstellenzentrale.Core.Contracts;

/// <summary>
/// Anfrage zum Erstellen einer Anwendungsgruppe.
/// </summary>
public class CreateApplicationGroupRequest
{
    /// <summary>
    /// Name der neuen Gruppe.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}
