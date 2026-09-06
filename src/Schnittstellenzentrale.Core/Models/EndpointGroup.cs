namespace Schnittstellenzentrale.Core.Models;

/// <summary>Gruppierung von Endpunkten innerhalb einer Anwendung.</summary>
public class EndpointGroup
{
    /// <summary>Der eindeutige Bezeichner.</summary>
    public int Id { get; set; }

    /// <summary>Der Name der Gruppe.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>ID der zugeordneten Anwendung.</summary>
    public int ApplicationId { get; set; }

    /// <summary>Die zugeordnete Anwendung.</summary>
    public Application Application { get; set; } = null!;

    /// <summary>ID der übergeordneten Gruppe (optional).</summary>
    public int? ParentGroupId { get; set; }

    /// <summary>Die übergeordnete Gruppe (optional).</summary>
    public EndpointGroup? ParentGroup { get; set; }

    /// <summary>Die Zeilenversionskennung für Optimistic Concurrency.</summary>
    public byte[] RowVersion { get; set; } = [];

    /// <summary>Die enthaltenen Endpunkte.</summary>
    public ICollection<Endpoint> Endpoints { get; set; } = [];

    /// <summary>Die untergeordneten Gruppen.</summary>
    public ICollection<EndpointGroup> ChildGroups { get; set; } = [];
}
