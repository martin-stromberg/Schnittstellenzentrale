namespace Schnittstellenzentrale.Core.Models;

public class EndpointExecutionResult
{
    public bool Success { get; set; }
    public bool HttpSuccess { get; set; }
    public int? StatusCode { get; set; }
    public string? RequestDetails { get; set; }
    public string? ResponseBody { get; set; }
    public string? ErrorMessage { get; set; }
    public IDictionary<string, string>? ResponseHeaders { get; set; }
    public long? DurationMs { get; set; }
    public long? ResponseSizeBytes { get; set; }
}
