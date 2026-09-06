namespace Schnittstellenzentrale.Core.Enums;

/// <summary>
/// Authentifizierungstyp für HTTP-Requests.
/// </summary>
public enum AuthenticationType
{
    /// <summary>Keine Authentifizierung.</summary>
    None,

    /// <summary>Basic-Authentifizierung.</summary>
    Basic,

    /// <summary>Negotiate-Authentifizierung.</summary>
    Negotiate,

    /// <summary>Authentifizierung per Bearer-Token.</summary>
    BearerToken,

    /// <summary>Negotiate-Authentifizierung mit Impersonation.</summary>
    NegotiateWithImpersonation
}
