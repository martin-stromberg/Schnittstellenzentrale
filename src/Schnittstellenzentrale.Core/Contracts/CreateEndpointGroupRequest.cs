using System.ComponentModel.DataAnnotations;

namespace Schnittstellenzentrale.Core.Contracts;

public class CreateEndpointGroupRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ApplicationId { get; set; }

    public int? ParentGroupId { get; set; }
}
