namespace Schnittstellenzentrale.Core.Models;

public class ApplicationGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSystem { get; set; } = false;
    public byte[] RowVersion { get; set; } = [];
    public ICollection<Application> Applications { get; set; } = [];
}
