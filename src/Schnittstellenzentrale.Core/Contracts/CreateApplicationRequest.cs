using System.ComponentModel.DataAnnotations;

namespace Schnittstellenzentrale.Core.Contracts;

public class CreateApplicationRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string BaseUrl { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(500)]
    public string? InterfaceUrl { get; set; }

    public int? ApplicationGroupId { get; set; }

    [MaxLength(256)]
    public string? Owner { get; set; }
}
