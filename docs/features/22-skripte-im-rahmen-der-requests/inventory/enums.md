# Enums

## `AuthenticationType`
Datei: `src/Schnittstellenzentrale.Core/Enums/AuthenticationType.cs`

| Wert | Bedeutung |
|---|---|
| `None` | Keine Authentifizierung |
| `Basic` | HTTP Basic Authentication |
| `Negotiate` | Windows-Negotiate (Kerberos/NTLM) |
| `BearerToken` | Bearer-Token-Authentifizierung; Token wird über `ICredentialService` aus dem Windows Credential Manager gelesen |
| `NegotiateWithImpersonation` | Negotiate mit Windows-Impersonation |

Hinweis: Der Wert `BearerToken` ist bereits vorhanden. Kein neuer Enum-Wert ist für die Anforderung nötig. Der Bearer-Token-Wert selbst wird nicht im Enum, sondern im Credential Manager (Schlüssel gebildet durch `CredentialTargetHelper.Build`) gespeichert.
