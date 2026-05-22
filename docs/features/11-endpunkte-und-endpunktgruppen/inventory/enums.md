# Enums

## `AuthenticationType`
Datei: `src/Schnittstellenzentrale.Core/Enums/AuthenticationType.cs`

| Wert | Bedeutung |
|---|---|
| `None` | Keine Authentifizierung |
| `Basic` | HTTP Basic Auth (Benutzername/Passwort aus Windows Credential Manager) |
| `Negotiate` | Windows-Authentifizierung (Kerberos/NTLM) |
| `BearerToken` | Bearer-Token aus Windows Credential Manager |
| `NegotiateWithImpersonation` | Windows-Authentifizierung mit Impersonation des aktuellen Benutzers |

Wird von `Endpoint.AuthenticationType` und `EndpointExecutionService` verwendet.

---

## `HttpMethod`
Datei: `src/Schnittstellenzentrale.Core/Enums/HttpMethod.cs`

| Wert | Bedeutung |
|---|---|
| `GET` | HTTP GET |
| `POST` | HTTP POST |
| `PUT` | HTTP PUT |
| `DELETE` | HTTP DELETE |
| `PATCH` | HTTP PATCH |
| `HEAD` | HTTP HEAD |
| `OPTIONS` | HTTP OPTIONS |

Wird von `Endpoint.Method` und `EndpointExecutionService.BuildRequest` verwendet.

---

**Fehlender Enum laut Anforderung:** `BodyMode` (`None`, `Json`, `Xml`, `PlainText`) existiert noch nicht.
