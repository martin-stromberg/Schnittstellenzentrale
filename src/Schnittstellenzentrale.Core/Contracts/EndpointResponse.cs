using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Contracts;

public class EndpointResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Enums.HttpMethod Method { get; set; }
    public string RelativePath { get; set; } = string.Empty;
    public string? Body { get; set; }
    public BodyMode BodyMode { get; set; }
    public AuthenticationType AuthenticationType { get; set; }
    public int ApplicationId { get; set; }
    public int? EndpointGroupId { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public string? PreRequestScript { get; set; }
    public string? PostRequestScript { get; set; }
    public IList<EndpointKeyValueResponse> Headers { get; set; } = [];
    public IList<EndpointKeyValueResponse> QueryParameters { get; set; } = [];
}
