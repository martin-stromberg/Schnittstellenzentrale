namespace Schnittstellenzentrale.Core.Models;

public class AuthToken
{
    public string TokenValue { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string WindowsUsername { get; set; } = string.Empty;
}
