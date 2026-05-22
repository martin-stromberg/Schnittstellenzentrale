using Schnittstellenzentrale.Core.Enums;

namespace Schnittstellenzentrale.Core.Helpers;

/// <summary>Erzeugt den Schlüssel für den Windows Credential Manager.</summary>
public static class CredentialTargetHelper
{
    /// <summary>Gibt den Credential-Target-String für eine Anwendung und einen Authentifizierungstyp zurück.</summary>
    public static string Build(int applicationId, AuthenticationType authenticationType)
        => $"Schnittstellenzentrale:{applicationId}:{authenticationType}";
}
