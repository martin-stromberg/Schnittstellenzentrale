namespace Schnittstellenzentrale.Core.Contracts;

/// <summary>
/// DTO für ein Schlüssel-Wert-Paar eines Endpunkts (z. B. Header oder Query-Parameter).
/// </summary>
public class EndpointKeyValueResponse
{
    /// <summary>
    /// Eindeutige ID des Eintrags.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Schlüssel des Eintrags.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Wert des Eintrags.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// ID des Endpunkts, zu dem der Eintrag gehört.
    /// </summary>
    public int EndpointId { get; set; }
}
