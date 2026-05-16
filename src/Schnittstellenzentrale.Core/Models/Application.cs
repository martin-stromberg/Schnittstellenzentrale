namespace Schnittstellenzentrale.Core.Models;

public class Application
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? SwaggerUrl { get; set; }
    public string? MetadataUrl { get; set; }
    public string? Owner { get; set; }
    public int? ApplicationGroupId { get; set; }
    public ApplicationGroup? ApplicationGroup { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<Endpoint> Endpoints { get; set; } = [];
    public ICollection<EndpointGroup> EndpointGroups { get; set; } = [];

    public Application Clone() => new()
    {
        Id = Id,
        Name = Name,
        Description = Description,
        BaseUrl = BaseUrl,
        SwaggerUrl = SwaggerUrl,
        MetadataUrl = MetadataUrl,
        Owner = Owner,
        ApplicationGroupId = ApplicationGroupId,
        RowVersion = RowVersion
    };
}
