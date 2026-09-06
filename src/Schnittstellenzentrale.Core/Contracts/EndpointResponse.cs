using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Contracts;

/// <summary>
/// DTO für einen Endpunkt.
/// </summary>
public class EndpointResponse
{
    /// <summary>
    /// Eindeutige ID des Endpunkts.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name des Endpunkts.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// HTTP-Methode des Endpunkts.
    /// </summary>
    public Enums.HttpMethod Method { get; set; }

    /// <summary>
    /// Relativer Pfad des Endpunkts.
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// Optionaler Request-Body.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Body-Format des Requests.
    /// </summary>
    public BodyMode BodyMode { get; set; }

    /// <summary>
    /// Authentifizierungstyp des Endpunkts.
    /// </summary>
    public AuthenticationType AuthenticationType { get; set; }

    /// <summary>
    /// ID der Anwendung, zu der der Endpunkt gehört.
    /// </summary>
    public int ApplicationId { get; set; }

    /// <summary>
    /// Optionale ID der zugeordneten Endpunktgruppe.
    /// </summary>
    public int? EndpointGroupId { get; set; }

    /// <summary>
    /// Versionsstempel für die optimistische Parallelitätskontrolle.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];

    /// <summary>
    /// Optionales Pre-Request-Skript.
    /// </summary>
    public string? PreRequestScript { get; set; }

    /// <summary>
    /// Optionales Post-Request-Skript.
    /// </summary>
    public string? PostRequestScript { get; set; }

    /// <summary>
    /// HTTP-Header des Endpunkts.
    /// </summary>
    public IList<EndpointKeyValueResponse> Headers { get; set; } = [];

    /// <summary>
    /// Query-Parameter des Endpunkts.
    /// </summary>
    public IList<EndpointKeyValueResponse> QueryParameters { get; set; } = [];
}
