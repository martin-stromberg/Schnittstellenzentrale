namespace Schnittstellenzentrale.Core.Models;

public class EndpointExecutionResult
{
    public bool Success { get; set; }
    public int? StatusCode { get; set; }
    public string? RequestDetails { get; set; }
    public string? ResponseBody { get; set; }
    public string? ErrorMessage { get; set; }
}
