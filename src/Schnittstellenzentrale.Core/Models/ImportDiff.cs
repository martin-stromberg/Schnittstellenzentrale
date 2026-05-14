namespace Schnittstellenzentrale.Core.Models;

public class ImportDiff
{
    public IList<Endpoint> NewEndpoints { get; set; } = [];
    public IList<Endpoint> ChangedEndpoints { get; set; } = [];
    public IList<Endpoint> RemovedEndpoints { get; set; } = [];
    public string? ErrorMessage { get; set; }
}
