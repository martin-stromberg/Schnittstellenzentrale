namespace Schnittstellenzentrale.Core.Models;

public class EndpointHeader
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int EndpointId { get; set; }
    public Endpoint Endpoint { get; set; } = null!;
}
