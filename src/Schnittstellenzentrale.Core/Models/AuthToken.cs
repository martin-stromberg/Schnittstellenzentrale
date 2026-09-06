namespace Schnittstellenzentrale.Core.Models;

/// <summary>Authentifizierungstoken mit Ablaufzeitpunkt.</summary>
public class AuthToken
{
    /// <summary>Der Token-Wert.</summary>
    public string TokenValue { get; set; } = string.Empty;

    /// <summary>Der Ablaufzeitpunkt.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Der Windows-Benutzername.</summary>
    public string WindowsUsername { get; set; } = string.Empty;
}
