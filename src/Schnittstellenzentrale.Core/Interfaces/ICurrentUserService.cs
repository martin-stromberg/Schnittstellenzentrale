namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Bietet Informationen über den aktuellen Benutzer.</summary>
public interface ICurrentUserService
{
    /// <summary>Ruft den Namen des aktuellen Benutzers ab.</summary>
    /// <returns>Der Benutzername.</returns>
    string GetCurrentUserName();
}
