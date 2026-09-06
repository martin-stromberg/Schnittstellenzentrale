using System.ComponentModel.DataAnnotations;

namespace Schnittstellenzentrale.Core.Contracts;

/// <summary>
/// Anfrage zum Hinzufügen eines Schlüssel-Wert-Paars zu einem Endpunkt.
/// </summary>
public class AddEndpointKeyValueRequest
{
    /// <summary>
    /// Schlüssel des Eintrags (z. B. Header- oder Parametername).
    /// </summary>
    [Required]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Wert des Eintrags.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// ID des Endpunkts, zu dem der Eintrag gehört.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int EndpointId { get; set; }
}
