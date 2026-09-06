using System.ComponentModel.DataAnnotations;
using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Contracts;

/// <summary>
/// Anfrage zum Aktualisieren eines Endpunkts.
/// </summary>
public class UpdateEndpointRequest
{
    /// <summary>
    /// Name des Endpunkts.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Relativer Pfad des Endpunkts.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// Optionale ID der zugeordneten Endpunktgruppe.
    /// </summary>
    public int? EndpointGroupId { get; set; }

    /// <summary>
    /// HTTP-Methode des Endpunkts.
    /// </summary>
    public Enums.HttpMethod Method { get; set; }

    /// <summary>
    /// Body-Format des Requests.
    /// </summary>
    public BodyMode BodyMode { get; set; }

    /// <summary>
    /// Optionaler Request-Body.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Authentifizierungstyp des Endpunkts.
    /// </summary>
    public AuthenticationType AuthenticationType { get; set; }

    /// <summary>
    /// Optionales Pre-Request-Skript.
    /// </summary>
    public string? PreRequestScript { get; set; }

    /// <summary>
    /// Optionales Post-Request-Skript.
    /// </summary>
    public string? PostRequestScript { get; set; }

    /// <summary>
    /// Versionsstempel für die optimistische Parallelitätskontrolle.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];

    /// <summary>
    /// HTTP-Header des Endpunkts.
    /// </summary>
    public IList<UpdateEndpointKeyValueItem> Headers { get; set; } = [];

    /// <summary>
    /// Query-Parameter des Endpunkts.
    /// </summary>
    public IList<UpdateEndpointKeyValueItem> QueryParameters { get; set; } = [];
}
