namespace Schnittstellenzentrale.Core.Models;

public class ImportDiff
{
    public IList<Endpoint> NewEndpoints { get; init; } = [];
    public IList<Endpoint> ChangedEndpoints { get; init; } = [];
    public IList<Endpoint> RemovedEndpoints { get; init; } = [];
    public string? ErrorMessage { get; init; }
    public IDictionary<string, string> BearerTokens { get; init; } = new Dictionary<string, string>();
}
