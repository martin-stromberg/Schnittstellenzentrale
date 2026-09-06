namespace Schnittstellenzentrale.Core.Contracts;

/// <summary>
/// DTO für die Antwort einer Authentifizierung.
/// </summary>
public class AuthenticateResponse
{
    /// <summary>
    /// Ausgestelltes Authentifizierungstoken.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
