namespace Schnittstellenzentrale.Core.Contracts;

public class ApplicationGroupResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public IList<ApplicationResponse> Applications { get; set; } = [];
}
