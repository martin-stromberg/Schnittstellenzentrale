using System.ComponentModel.DataAnnotations;
using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Contracts;

public class CreateEndpointRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string RelativePath { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ApplicationId { get; set; }

    public int? EndpointGroupId { get; set; }
    public Enums.HttpMethod Method { get; set; }
    public BodyMode BodyMode { get; set; }
    public string? Body { get; set; }
    public AuthenticationType AuthenticationType { get; set; }
    public string? PreRequestScript { get; set; }
    public string? PostRequestScript { get; set; }
}
