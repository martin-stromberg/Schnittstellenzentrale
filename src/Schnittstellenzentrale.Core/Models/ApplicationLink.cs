namespace Schnittstellenzentrale.Core.Models;

public class ApplicationLink
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
    public string? Url { get; set; }
    public string? Label { get; set; }
    public int? SortOrder { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
