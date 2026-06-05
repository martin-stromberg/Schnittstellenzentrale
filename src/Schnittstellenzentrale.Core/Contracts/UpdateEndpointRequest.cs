using System.ComponentModel.DataAnnotations;
using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Contracts;

public class UpdateEndpointRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string RelativePath { get; set; } = string.Empty;

    public int? EndpointGroupId { get; set; }
    public Enums.HttpMethod Method { get; set; }
    public BodyMode BodyMode { get; set; }
    public string? Body { get; set; }
    public AuthenticationType AuthenticationType { get; set; }
    public string? PreRequestScript { get; set; }
    public string? PostRequestScript { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public IList<UpdateEndpointKeyValueItem> Headers { get; set; } = [];
    public IList<UpdateEndpointKeyValueItem> QueryParameters { get; set; } = [];
}
