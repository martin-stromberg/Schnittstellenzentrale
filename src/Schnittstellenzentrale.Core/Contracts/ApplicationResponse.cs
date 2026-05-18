namespace Schnittstellenzentrale.Core.Contracts;

public class ApplicationResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public int? ApplicationGroupId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? InterfaceUrl { get; set; }
    public int InterfaceType { get; set; }
    public string? Owner { get; set; }
}
