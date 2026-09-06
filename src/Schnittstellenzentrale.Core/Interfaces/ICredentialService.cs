namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Bietet Zugriff auf gespeicherte Anmeldeinformationen.</summary>
public interface ICredentialService
{
    /// <summary>Ruft das gespeicherte Passwort für ein Ziel ab.</summary>
    /// <param name="target">Ziel, für das das Passwort gespeichert ist.</param>
    /// <returns>Das Passwort oder <see langword="null"/>.</returns>
    string? GetPassword(string target);

    /// <summary>Speichert Anmeldeinformationen für ein Ziel.</summary>
    /// <param name="target">Ziel, für das die Anmeldeinformationen gespeichert werden.</param>
    /// <param name="username">Benutzername.</param>
    /// <param name="password">Passwort.</param>
    void SavePassword(string target, string username, string password);

    /// <summary>Löscht gespeicherte Anmeldeinformationen für ein Ziel.</summary>
    /// <param name="target">Ziel, dessen Anmeldeinformationen gelöscht werden.</param>
    void DeletePassword(string target);
}
