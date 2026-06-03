namespace Schnittstellenzentrale.Core.Contracts;

public class EndpointKeyValueResponse
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int EndpointId { get; set; }
}
