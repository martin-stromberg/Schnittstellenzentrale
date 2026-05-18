using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Models;

public class Application
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSystem { get; set; } = false;
    public string Description { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? InterfaceUrl { get; set; }
    public InterfaceType InterfaceType { get; set; }
    public string? Owner { get; set; }
    public int? ApplicationGroupId { get; set; }
    public ApplicationGroup? ApplicationGroup { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<Endpoint> Endpoints { get; set; } = [];
    public ICollection<EndpointGroup> EndpointGroups { get; set; } = [];

    public static InterfaceType DetectInterfaceType(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return InterfaceType.Unknown;
        if (url.Contains("$metadata", StringComparison.OrdinalIgnoreCase)) return InterfaceType.OData;
        if (url.Contains("swagger", StringComparison.OrdinalIgnoreCase) ||
            url.Contains("openapi", StringComparison.OrdinalIgnoreCase)) return InterfaceType.Rest;
        return InterfaceType.Unknown;
    }

    public Application Clone() => new()
    {
        Id = Id,
        Name = Name,
        Description = Description,
        BaseUrl = BaseUrl,
        InterfaceUrl = InterfaceUrl,
        InterfaceType = InterfaceType,
        Owner = Owner,
        ApplicationGroupId = ApplicationGroupId,
        RowVersion = RowVersion
    };
}
