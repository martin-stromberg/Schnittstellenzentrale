using System.ComponentModel.DataAnnotations;

namespace Schnittstellenzentrale.Core.Contracts;

public class CreateApplicationGroupRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}
