using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Models;

/// <summary>Repräsentiert eine Anwendung mit Endpunkten und Metadaten.</summary>
public class Application
{
    /// <summary>Der eindeutige Bezeichner.</summary>
    public int Id { get; set; }

    /// <summary>Der Name der Anwendung.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gibt an, ob es sich um einen Systemeintrag handelt.</summary>
    public bool IsSystem { get; set; } = false;

    /// <summary>Die Beschreibung.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Die Basis-URL der Anwendung.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Die URL der Schnittstellenbeschreibung (optional).</summary>
    public string? InterfaceUrl { get; set; }

    /// <summary>Der Typ der Schnittstelle.</summary>
    public InterfaceType InterfaceType { get; set; }

    /// <summary>Der Eigentümer (optional).</summary>
    public string? Owner { get; set; }

    /// <summary>ID der übergeordneten Anwendungsgruppe (optional).</summary>
    public int? ApplicationGroupId { get; set; }

    /// <summary>Die übergeordnete Anwendungsgruppe (optional).</summary>
    public ApplicationGroup? ApplicationGroup { get; set; }

    /// <summary>Der Untertitel (optional).</summary>
    public string? Subtitle { get; set; }

    /// <summary>Icon-Daten (optional).</summary>
    public byte[]? IconData { get; set; }

    /// <summary>Die Zeilenversionskennung für Optimistic Concurrency.</summary>
    public byte[] RowVersion { get; set; } = [];

    /// <summary>Die zugeordneten Endpunkte.</summary>
    public ICollection<Endpoint> Endpoints { get; set; } = [];

    /// <summary>Die zugeordneten Anwendungslinks.</summary>
    public ICollection<ApplicationLink> Links { get; set; } = [];

    /// <summary>Die zugeordneten Endpunktgruppen.</summary>
    public ICollection<EndpointGroup> EndpointGroups { get; set; } = [];

    /// <summary>Ermittelt den Schnittstellentyp aus der angegebenen URL.</summary>
    /// <param name="url">Die zu prüfende URL.</param>
    /// <returns>Der ermittelte Schnittstellentyp.</returns>
    public static InterfaceType DetectInterfaceType(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return InterfaceType.Unknown;
        if (url.Contains("$metadata", StringComparison.OrdinalIgnoreCase)) return InterfaceType.OData;
        if (url.Contains("swagger", StringComparison.OrdinalIgnoreCase) ||
            url.Contains("openapi", StringComparison.OrdinalIgnoreCase)) return InterfaceType.Rest;
        return InterfaceType.Unknown;
    }

    /// <summary>Erzeugt eine flache Kopie der Anwendung.</summary>
    /// <returns>Die kopierte <see cref="Application"/>.</returns>
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
