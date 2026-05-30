namespace Schnittstellenzentrale.Core.Contracts;

public class ApplicationGroupResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public string? Description { get; set; }
    public string? Subtitle { get; set; }
    public byte[]? IconData { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public IList<ApplicationResponse> Applications { get; set; } = [];
}
