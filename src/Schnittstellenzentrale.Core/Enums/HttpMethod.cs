namespace Schnittstellenzentrale.Core.Enums;

/// <summary>
/// HTTP-Methode eines Endpunkts.
/// </summary>
public enum HttpMethod
{
    /// <summary>HTTP GET.</summary>
    GET,

    /// <summary>HTTP POST.</summary>
    POST,

    /// <summary>HTTP PUT.</summary>
    PUT,

    /// <summary>HTTP DELETE.</summary>
    DELETE,

    /// <summary>HTTP PATCH.</summary>
    PATCH,

    /// <summary>HTTP HEAD.</summary>
    HEAD,

    /// <summary>HTTP OPTIONS.</summary>
    OPTIONS
}
