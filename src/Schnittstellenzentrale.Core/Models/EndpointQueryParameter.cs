using System.Text.Json.Serialization;

namespace Schnittstellenzentrale.Core.Models;

public class EndpointQueryParameter
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int EndpointId { get; set; }
    [JsonIgnore]
    public Endpoint? Endpoint { get; set; }
}
