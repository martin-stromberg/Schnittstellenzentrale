namespace Schnittstellenzentrale.Core.Models;

public class EndpointCallHistoryEntry
{
    public long Id { get; set; }
    public int? ApplicationId { get; set; }
    public Application? Application { get; set; }
    public int? EndpointId { get; set; }
    public Endpoint? Endpoint { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public string? HttpMethod { get; set; }
    public string? RelativePath { get; set; }
    public int? StatusCode { get; set; }
    public int? DurationMs { get; set; }
}
