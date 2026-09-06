using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Definiert Operationen zum Verwalten von Authentifizierungstoken.</summary>
public interface ITokenStore
{
    /// <summary>Erstellt ein Token für einen Benutzer.</summary>
    /// <param name="username">Benutzername, für den das Token erstellt wird.</param>
    /// <returns>Das erstellte Token.</returns>
    Task<AuthToken> CreateTokenAsync(string username);

    /// <summary>Überprüft und rotiert ein Token.</summary>
    /// <param name="tokenString">Zu prüfendes Token.</param>
    /// <returns>Das rotierte Token oder <see langword="null"/>.</returns>
    Task<AuthToken?> ValidateAndRotateAsync(string tokenString);
}
