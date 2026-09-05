using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Helpers;
using Schnittstellenzentrale.Core.Models;
using CoreHttpMethod = Schnittstellenzentrale.Core.Enums.HttpMethod;

namespace Schnittstellenzentrale.Infrastructure.Helpers;

/// <summary>Gemeinsame Hilfsmethoden für das Auslesen von OpenAPI-Operationen und -Erweiterungen.</summary>
public static class SwaggerOperationHelper
{
    private const string DefaultAuthenticateEndpointName = "Authenticate";
    private const string DefaultAuthenticateExecuteCall = "sz.execute('Authenticate')";

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

    /// <summary>Wandelt ein OpenAPI-Dokument in eine Endpunktliste samt Bearer-Token-Map um.</summary>
    public static (List<Endpoint> Endpoints, Dictionary<string, string> BearerTokens) MapDocumentToEndpoints(
        OpenApiDocument document, int applicationId)
    {
        var endpoints = new List<Endpoint>();
        var bearerTokens = new Dictionary<string, string>();
        var authenticateEndpointName = FindAuthenticateEndpointName(document);

        foreach (var path in document.Paths)
            foreach (var operation in path.Value.Operations ?? [])
            {
                var method = MapHttpMethod(operation.Key.ToString());
                var name = BuildEndpointName(operation.Value, operation.Key, path.Key);
                var relativePath = path.Key;
                var isAuthenticateEndpoint = IsAuthenticateOperation(name, method, relativePath);
                var bearerTokenValue = ReadExtensionString(operation.Value?.Extensions, "x-sz-bearer-token");
                var authType = string.IsNullOrEmpty(bearerTokenValue)
                    ? DetectAuthenticationType(operation.Value)
                    : AuthenticationType.BearerToken;
                var endpoint = new Endpoint
                {
                    Name = name,
                    Method = method,
                    RelativePath = relativePath,
                    ApplicationId = applicationId,
                    AuthenticationType = authType,
                    PreRequestScript = ReadExtensionString(operation.Value?.Extensions, "x-sz-pre-request-script"),
                    PostRequestScript = ResolvePostRequestScript(
                        ReadExtensionString(operation.Value?.Extensions, "x-sz-post-request-script"),
                        authenticateEndpointName,
                        isAuthenticateEndpoint),
                    Headers = BuildDefaultHeaders(operation.Value)
                };
                if (!string.IsNullOrEmpty(bearerTokenValue))
                    bearerTokens[EndpointKeyHelper.BuildKey(endpoint)] = bearerTokenValue;
                endpoints.Add(endpoint);
            }

        return (endpoints, bearerTokens);
    }

    private static string BuildEndpointName(OpenApiOperation? operation, object method, string path)
        => operation?.OperationId ?? $"{method} {path}";

    private static string? FindAuthenticateEndpointName(OpenApiDocument document)
    {
        foreach (var path in document.Paths)
            foreach (var operation in path.Value.Operations ?? [])
            {
                var method = MapHttpMethod(operation.Key.ToString());
                var name = BuildEndpointName(operation.Value, operation.Key, path.Key);
                if (IsAuthenticateOperation(name, method, path.Key))
                    return name;
            }

        return null;
    }

    private static string? ResolvePostRequestScript(string? script, string? authenticateEndpointName, bool isAuthenticateEndpoint)
    {
        if (string.IsNullOrEmpty(script)
            || isAuthenticateEndpoint
            || string.IsNullOrEmpty(authenticateEndpointName)
            || authenticateEndpointName.Equals(DefaultAuthenticateEndpointName, StringComparison.Ordinal)
            || !script.Contains(DefaultAuthenticateExecuteCall, StringComparison.Ordinal))
            return script;

        return script.Replace(
            DefaultAuthenticateExecuteCall,
            $"sz.execute('{EscapeJavaScriptSingleQuotedString(authenticateEndpointName)}')",
            StringComparison.Ordinal);
    }

    private static bool IsAuthenticateOperation(string name, CoreHttpMethod method, string relativePath)
    {
        if (!method.Equals(CoreHttpMethod.POST))
            return false;

        if (name.Equals(DefaultAuthenticateEndpointName, StringComparison.OrdinalIgnoreCase)
            || name.EndsWith(".Authenticate", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith("_Authenticate", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith("AuthenticateAsync", StringComparison.OrdinalIgnoreCase))
            return true;

        var path = relativePath.Trim().Trim('/').Trim();
        return path.Equals("authenticate", StringComparison.OrdinalIgnoreCase)
            || path.Equals("Authenticate()", StringComparison.OrdinalIgnoreCase);
    }

    private static string EscapeJavaScriptSingleQuotedString(string value)
        => value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("'", "\\'", StringComparison.Ordinal);

    private static AuthenticationType DetectAuthenticationType(OpenApiOperation? operation)
    {
        if (operation?.Security is not { Count: > 0 })
            return AuthenticationType.None;

        foreach (var requirement in operation.Security)
            foreach (var schemeKey in requirement.Keys)
                if (schemeKey is OpenApiSecuritySchemeReference schemeRef &&
                    string.Equals(schemeRef.Reference?.Id, "Negotiate", StringComparison.OrdinalIgnoreCase))
                    return AuthenticationType.Negotiate;

        return AuthenticationType.None;
    }

    private static List<EndpointHeader> BuildDefaultHeaders(OpenApiOperation? operation)
    {
        if (operation?.Parameters == null)
            return [];

        return operation.Parameters
            .Where(p => p.In == ParameterLocation.Header)
            .Select(p => new EndpointHeader
            {
                Key = p.Name ?? string.Empty,
                Value = ExtractDefaultValue(p)
            })
            .ToList();
    }

    private static string ExtractDefaultValue(IOpenApiParameter param)
    {
        if (param.Schema?.Default is not JsonValue jsonVal)
            return string.Empty;

        return jsonVal.TryGetValue<string>(out var str) ? str : string.Empty;
    }
}
