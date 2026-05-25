using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using CoreHttpMethod = Schnittstellenzentrale.Core.Enums.HttpMethod;

namespace Schnittstellenzentrale.Infrastructure.Helpers;

/// <summary>Gemeinsame Hilfsmethoden für das Auslesen von OpenAPI-Operationen und -Erweiterungen.</summary>
public static class SwaggerOperationHelper
{
    /// <summary>Liest den Stringwert einer OpenAPI-Erweiterung (<c>x-sz-*</c>) aus.</summary>
    public static string? ReadExtensionString(IDictionary<string, IOpenApiExtension>? extensions, string key)
    {
        if (extensions == null || !extensions.TryGetValue(key, out var extension))
            return null;

        if (extension is JsonNodeExtension jne && jne.Node is JsonValue jv && jv.TryGetValue<string>(out var value))
            return value;

        return null;
    }

    /// <summary>Konvertiert einen HTTP-Methodennamen in das projekteigene <see cref="CoreHttpMethod"/>-Enum.</summary>
    public static CoreHttpMethod MapHttpMethod(string method) =>
        method.ToUpperInvariant() switch
        {
            "GET" => CoreHttpMethod.GET,
            "POST" => CoreHttpMethod.POST,
            "PUT" => CoreHttpMethod.PUT,
            "DELETE" => CoreHttpMethod.DELETE,
            "PATCH" => CoreHttpMethod.PATCH,
            "HEAD" => CoreHttpMethod.HEAD,
            "OPTIONS" => CoreHttpMethod.OPTIONS,
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unbekannte HTTP-Methode.")
        };
}
