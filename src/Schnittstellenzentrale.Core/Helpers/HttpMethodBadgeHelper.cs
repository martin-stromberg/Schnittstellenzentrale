namespace Schnittstellenzentrale.Core.Helpers;

/// <summary>Gibt CSS-Klassen für HTTP-Methoden-Badges zurück.</summary>
public static class HttpMethodBadgeHelper
{
    /// <summary>Gibt die CSS-Klasse für das Badge der angegebenen HTTP-Methode zurück.</summary>
    /// <param name="method">HTTP-Methode (z. B. GET, POST), optional.</param>
    /// <returns>CSS-Klasse für das Methoden-Badge.</returns>
    public static string GetMethodBadgeClass(string? method) => method?.ToUpperInvariant() switch
    {
        "GET" => "sz-method-badge sz-method-badge--get",
        "POST" => "sz-method-badge sz-method-badge--post",
        "PUT" => "sz-method-badge sz-method-badge--put",
        "PATCH" => "sz-method-badge sz-method-badge--patch",
        "DELETE" => "sz-method-badge sz-method-badge--delete",
        _ => "sz-method-badge"
    };
}
