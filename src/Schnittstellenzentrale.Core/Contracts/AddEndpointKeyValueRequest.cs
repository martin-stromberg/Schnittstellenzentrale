using System.ComponentModel.DataAnnotations;

namespace Schnittstellenzentrale.Core.Contracts;

public class AddEndpointKeyValueRequest
{
    [Required]
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int EndpointId { get; set; }
}
