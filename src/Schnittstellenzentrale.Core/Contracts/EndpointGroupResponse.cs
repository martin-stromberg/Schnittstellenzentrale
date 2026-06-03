namespace Schnittstellenzentrale.Core.Contracts;

public class EndpointGroupResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ApplicationId { get; set; }
    public int? ParentGroupId { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
