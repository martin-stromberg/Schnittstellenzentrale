namespace Schnittstellenzentrale.Core.Models;

public class EndpointGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
    public byte[] RowVersion { get; set; } = [];
    public ICollection<Endpoint> Endpoints { get; set; } = [];
}
