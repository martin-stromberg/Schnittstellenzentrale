namespace Schnittstellenzentrale.Core.Enums;

/// <summary>
/// Typ einer Schnittstelle.
/// </summary>
public enum InterfaceType
{
    /// <summary>Unbekannter Schnittstellentyp.</summary>
    Unknown = 0,

    /// <summary>REST-Schnittstelle.</summary>
    Rest = 1,

    /// <summary>OData-Schnittstelle.</summary>
    OData = 2
}
