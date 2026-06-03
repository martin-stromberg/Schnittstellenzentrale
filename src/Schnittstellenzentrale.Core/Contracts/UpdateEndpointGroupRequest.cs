using System.ComponentModel.DataAnnotations;

namespace Schnittstellenzentrale.Core.Contracts;

public class UpdateEndpointGroupRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public byte[] RowVersion { get; set; } = [];
}
