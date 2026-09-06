using System.Text.Json.Serialization;

namespace Schnittstellenzentrale.Core.Models;

/// <summary>Query-Parameter eines Endpunkts.</summary>
public class EndpointQueryParameter
{
    /// <summary>Der eindeutige Bezeichner.</summary>
    public int Id { get; set; }

    /// <summary>Der Schlüssel des Parameters.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Der Wert des Parameters.</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>ID des zugeordneten Endpunkts.</summary>
    public int EndpointId { get; set; }

    /// <summary>Der zugeordnete Endpunkt (optional).</summary>
    [JsonIgnore]
    public Endpoint? Endpoint { get; set; }
}
