using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Helpers;

/// <summary>Erstellt einen eindeutigen Schlüssel für einen Endpunkt aus HTTP-Methode und Pfad.</summary>
public static class EndpointKeyHelper
{
    /// <summary>Gibt den Schlüssel im Format <c>METHOD:/pfad</c> zurück.</summary>
    /// <param name="endpoint">Endpunkt, für den der Schlüssel erzeugt wird.</param>
    /// <returns>Schlüssel im Format <c>METHOD:/pfad</c>.</returns>
    public static string BuildKey(Endpoint endpoint) => $"{endpoint.Method}:{endpoint.RelativePath}";
}
