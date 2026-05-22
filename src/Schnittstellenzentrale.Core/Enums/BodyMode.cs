namespace Schnittstellenzentrale.Core.Enums;

/// <summary>
/// Steuert das Body-Format und den automatisch gesetzten Content-Type-Header.
/// </summary>
public enum BodyMode
{
    /// <summary>Kein Body und kein automatischer Content-Type.</summary>
    None,

    /// <summary>JSON-Body mit Content-Type <c>application/json</c>.</summary>
    Json,

    /// <summary>XML-Body mit Content-Type <c>application/xml</c>.</summary>
    Xml,

    /// <summary>Reiner Text mit Content-Type <c>text/plain</c>.</summary>
    PlainText
}
