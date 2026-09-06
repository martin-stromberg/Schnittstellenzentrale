using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Models;

/// <summary>Repräsentiert einen aufrufbaren Endpunkt einer Anwendung.</summary>
public class Endpoint
{
    /// <summary>Der eindeutige Bezeichner.</summary>
    public int Id { get; set; }

    /// <summary>Der Name des Endpunkts.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Die HTTP-Methode.</summary>
    public Enums.HttpMethod Method { get; set; }

    /// <summary>Der relative Pfad.</summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>Der Request-Body (optional).</summary>
    public string? Body { get; set; }

    /// <summary>Der Body-Modus.</summary>
    public BodyMode BodyMode { get; set; }

    /// <summary>Der Authentifizierungstyp.</summary>
    public AuthenticationType AuthenticationType { get; set; }

    /// <summary>ID der zugeordneten Anwendung.</summary>
    public int ApplicationId { get; set; }

    /// <summary>Die zugeordnete Anwendung (optional).</summary>
    public Application? Application { get; set; }

    /// <summary>ID der zugeordneten Endpunktgruppe (optional).</summary>
    public int? EndpointGroupId { get; set; }

    /// <summary>Die zugeordnete Endpunktgruppe (optional).</summary>
    public EndpointGroup? EndpointGroup { get; set; }

    /// <summary>Die Zeilenversionskennung für Optimistic Concurrency.</summary>
    public byte[] RowVersion { get; set; } = [];

    /// <summary>Das vor dem Request auszuführende Skript (optional).</summary>
    public string? PreRequestScript { get; set; }

    /// <summary>Das nach dem Request auszuführende Skript (optional).</summary>
    public string? PostRequestScript { get; set; }

    /// <summary>Die zugeordneten Header.</summary>
    public ICollection<EndpointHeader> Headers { get; set; } = [];

    /// <summary>Die zugeordneten Query-Parameter.</summary>
    public ICollection<EndpointQueryParameter> QueryParameters { get; set; } = [];
}
