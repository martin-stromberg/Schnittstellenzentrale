using System.Text.Json.Serialization;

namespace Schnittstellenzentrale.Core.Models;

/// <summary>HTTP-Header eines Endpunkts.</summary>
public class EndpointHeader
{
    /// <summary>Der eindeutige Bezeichner.</summary>
    public int Id { get; set; }

    /// <summary>Der Schlüssel des Headers.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Der Wert des Headers.</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>ID des zugeordneten Endpunkts.</summary>
    public int EndpointId { get; set; }

    /// <summary>Der zugeordnete Endpunkt (optional).</summary>
    [JsonIgnore]
    public Endpoint? Endpoint { get; set; }
}
