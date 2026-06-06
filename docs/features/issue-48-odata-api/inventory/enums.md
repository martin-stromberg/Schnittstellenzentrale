# Enums

## `InterfaceType`
Datei: `src/Schnittstellenzentrale.Core/Enums/InterfaceType.cs`

| Wert | Bedeutung |
|---|---|
| `Unknown = 0` | Kein erkannter Schnittstellentyp |
| `Rest = 1` | OpenAPI/Swagger-Schnittstelle |
| `OData = 2` | OData v4-Schnittstelle (erkannt über `$metadata` in der URL) |

Automatische Erkennung erfolgt in `Application.DetectInterfaceType(string? url)`.

---

## `StorageMode`
Datei: `src/Schnittstellenzentrale.Core/Enums/StorageMode.cs`

| Wert | Bedeutung |
|---|---|
| `Team` | Geteilter Modus — Daten für alle Benutzer sichtbar |
| `User` | Privater Modus — Daten sind besitzerbezogen gefiltert |

Wird aus dem `X-Storage-Mode`-Request-Header in `ApiControllerBase.ParseStorageMode()` ermittelt. Relevant für die offene Frage, ob die OData-API diesen Header ebenfalls auswerten soll.

---

## `AuthenticationType`
Datei: `src/Schnittstellenzentrale.Core/Enums/AuthenticationType.cs`

| Wert | Bedeutung |
|---|---|
| `None` | Keine Authentifizierung |
| `Basic` | HTTP Basic Authentication |
| `Negotiate` | NTLM/Kerberos (Windows-Authentifizierung) |
| `BearerToken` | Bearer-Token |
| `NegotiateWithImpersonation` | Negotiate mit Impersonation |

---

## `BodyMode`
Datei: `src/Schnittstellenzentrale.Core/Enums/BodyMode.cs`

| Wert | Bedeutung |
|---|---|
| `None` | Kein Request-Body |
| `Json` | JSON-Body mit Content-Type `application/json` |
| `Xml` | XML-Body mit Content-Type `application/xml` |
| `PlainText` | Reiner Text mit Content-Type `text/plain` |
