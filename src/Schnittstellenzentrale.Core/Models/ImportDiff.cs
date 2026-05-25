namespace Schnittstellenzentrale.Core.Models;

public class ImportDiff
{
    public IList<Endpoint> NewEndpoints { get; init; } = [];
    public IList<Endpoint> ChangedEndpoints { get; init; } = [];
    public IList<Endpoint> RemovedEndpoints { get; init; } = [];
    public string? ErrorMessage { get; init; }
    public IDictionary<string, string> BearerTokens { get; init; } = new Dictionary<string, string>();

    public ImportDiff WithBearerTokens(IDictionary<string, string> bearerTokens) => new()
    {
        NewEndpoints = NewEndpoints,
        ChangedEndpoints = ChangedEndpoints,
        RemovedEndpoints = RemovedEndpoints,
        ErrorMessage = ErrorMessage,
        BearerTokens = bearerTokens
    };
}
