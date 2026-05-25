using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Models;

public class Endpoint
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Enums.HttpMethod Method { get; set; }
    public string RelativePath { get; set; } = string.Empty;
    public string? Body { get; set; }
    public BodyMode BodyMode { get; set; }
    public AuthenticationType AuthenticationType { get; set; }
    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
    public int? EndpointGroupId { get; set; }
    public EndpointGroup? EndpointGroup { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public string? PreRequestScript { get; set; }
    public string? PostRequestScript { get; set; }
    public ICollection<EndpointHeader> Headers { get; set; } = [];
    public ICollection<EndpointQueryParameter> QueryParameters { get; set; } = [];
}
