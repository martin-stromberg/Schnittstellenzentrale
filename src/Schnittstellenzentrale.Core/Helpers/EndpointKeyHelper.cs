using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Helpers;

/// <summary>Erstellt einen eindeutigen Schlüssel für einen Endpunkt aus HTTP-Methode und Pfad.</summary>
public static class EndpointKeyHelper
{
    /// <summary>Gibt den Schlüssel im Format <c>METHOD:/pfad</c> zurück.</summary>
    public static string BuildKey(Endpoint endpoint) => $"{endpoint.Method}:{endpoint.RelativePath}";
}
