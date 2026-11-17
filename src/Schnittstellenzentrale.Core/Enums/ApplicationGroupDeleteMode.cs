namespace Schnittstellenzentrale.Core.Enums;

/// <summary>
/// Steuert, wie enthaltene Anwendungen beim Löschen einer Anwendungsgruppe behandelt werden.
/// </summary>
public enum ApplicationGroupDeleteMode
{
    /// <summary>Enthaltene Anwendungen werden in die ungruppierte Menge umgehängt.</summary>
    Detach,

    /// <summary>Enthaltene Anwendungen werden samt Endpunkten und Endpunktgruppen mitgelöscht.</summary>
    Delete
}
