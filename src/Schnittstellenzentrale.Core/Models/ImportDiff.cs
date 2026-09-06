namespace Schnittstellenzentrale.Core.Models;

/// <summary>Ergebnis eines Endpunkt-Imports mit Differenzen und ggf. Bearer-Tokens.</summary>
public class ImportDiff
{
    /// <summary>Neu erkannte Endpunkte.</summary>
    public IList<Endpoint> NewEndpoints { get; init; } = [];

    /// <summary>Geänderte Endpunkte.</summary>
    public IList<Endpoint> ChangedEndpoints { get; init; } = [];

    /// <summary>Entfernte Endpunkte.</summary>
    public IList<Endpoint> RemovedEndpoints { get; init; } = [];

    /// <summary>Fehlermeldung beim Import (optional).</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Zugeordnete Bearer-Tokens pro Anwendungsname.</summary>
    /// <value>Die Bearer-Tokens pro Anwendungsname.</value>
    public IDictionary<string, string> BearerTokens { get; init; } = new Dictionary<string, string>();

    /// <summary>Erzeugt eine Kopie mit den angegebenen Bearer-Tokens.</summary>
    /// <param name="bearerTokens">Die neuen Bearer-Tokens.</param>
    /// <returns>Die Kopie mit aktualisierten Bearer-Tokens.</returns>
    public ImportDiff WithBearerTokens(IDictionary<string, string> bearerTokens) => new()
    {
        NewEndpoints = NewEndpoints,
        ChangedEndpoints = ChangedEndpoints,
        RemovedEndpoints = RemovedEndpoints,
        ErrorMessage = ErrorMessage,
        BearerTokens = bearerTokens
    };
}
