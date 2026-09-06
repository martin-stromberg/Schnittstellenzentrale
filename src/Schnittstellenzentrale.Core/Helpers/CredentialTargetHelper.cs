using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Helpers;

/// <summary>Erzeugt den Schlüssel für den Windows Credential Manager.</summary>
public static class CredentialTargetHelper
{
    /// <summary>Gibt den Credential-Target-String für eine Anwendung und einen Authentifizierungstyp zurück.</summary>
    /// <param name="applicationId">ID der Anwendung.</param>
    /// <param name="authenticationType">Authentifizierungstyp.</param>
    /// <returns>Credential-Target-String im Format <c>Schnittstellenzentrale:{id}:{typ}</c>.</returns>
    public static string Build(int applicationId, AuthenticationType authenticationType)
        => $"Schnittstellenzentrale:{applicationId}:{authenticationType}";
}
