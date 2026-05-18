# REST-API — API-Dokumentation

## Übersicht

Alle Endpunkte sind unter der in `appsettings.json` konfigurierten Basis-URL (`Api:BaseUrl`) erreichbar. Datenendpunkte erfordern einen gültigen Bearer-Token, der über `/authenticate` bezogen wird.

---

## POST /authenticate

Gibt ein kurzlebiges Token zurück, das für nachfolgende Datenendpunkt-Aufrufe benötigt wird.

**Authentifizierung:** Windows-Authentifizierung (Negotiate); der aufrufende Benutzer muss eine gültige Windows-Identität besitzen.

**Request-Body:** keiner

**Response: 200 OK**

```json
{
  "token": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `token` | `string` (GUID) | Bearer-Token für nachfolgende Datenendpunkt-Aufrufe; 5 Minuten gültig |

**Fehler:**

| Status | Ursache |
|--------|---------|
| 401 Unauthorized | Windows-Identität konnte nicht ermittelt werden |

---

## POST /api/application-groups

Legt eine neue `ApplicationGroup` an.

**Authentifizierung:** Bearer-Token (aus `/authenticate`).

**Request-Header:**

| Header | Pflicht | Beschreibung |
|--------|---------|--------------|
| `Authorization` | Ja | `Bearer <token>` |
| `X-Storage-Mode` | Nein | `Team` oder `User` (Standard: `User`); steuert SignalR-Benachrichtigungen |

**Request-Body:**

```json
{
  "name": "Backend-Services"
}
```

| Feld | Typ | Pflicht | Validierung | Beschreibung |
|------|-----|---------|-------------|--------------|
| `name` | `string` | Ja | `[Required]`, `[MaxLength(200)]` | Anzeigename der Gruppe |

**Response: 201 Created**

```json
{
  "id": 42,
  "name": "Backend-Services"
}
```

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `id` | `int` | Datenbankschlüssel der neu angelegten Gruppe |
| `name` | `string` | Name der Gruppe |

**Response-Header:**

| Header | Beschreibung |
|--------|--------------|
| `Location` | URL der angelegten Ressource, z. B. `/api/application-groups/42` |
| `X-New-Token` | Neuer Bearer-Token; ersetzt den verwendeten Token |

**Fehler:**

| Status | Ursache |
|--------|---------|
| 400 Bad Request | Pflichtfeld fehlt oder überschreitet die maximale Länge |
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |

---

## POST /api/applications

Legt eine neue `Application` an.

**Authentifizierung:** Bearer-Token (aus `/authenticate`).

**Request-Header:**

| Header | Pflicht | Beschreibung |
|--------|---------|--------------|
| `Authorization` | Ja | `Bearer <token>` |
| `X-Storage-Mode` | Nein | `Team` oder `User` (Standard: `User`) |

**Request-Body:**

```json
{
  "name": "Bestellservice",
  "baseUrl": "https://intern/bestellservice",
  "description": "Verarbeitet Bestellungen",
  "interfaceUrl": "https://intern/bestellservice/swagger/v1/swagger.json",
  "applicationGroupId": 42,
  "owner": null
}
```

| Feld | Typ | Pflicht | Validierung | Beschreibung |
|------|-----|---------|-------------|--------------|
| `name` | `string` | Ja | `[Required]`, `[MaxLength(200)]` | Anzeigename der Anwendung |
| `baseUrl` | `string` | Ja | `[Required]`, `[MaxLength(500)]` | Basis-URL des Dienstes |
| `description` | `string?` | Nein | — | Optionale Beschreibung |
| `interfaceUrl` | `string?` | Nein | `[MaxLength(500)]` | URL zur API-Beschreibung; der `InterfaceType` wird serverseitig automatisch erkannt |
| `applicationGroupId` | `int?` | Nein | — | ID der Gruppe; `null` für gruppenlose Anwendung |
| `owner` | `string?` | Nein | `[MaxLength(256)]` | Windows-Benutzername; im Benutzermodus vom Client gesetzt |

**Response: 201 Created**

```json
{
  "id": 7,
  "name": "Bestellservice",
  "baseUrl": "https://intern/bestellservice",
  "applicationGroupId": 42
}
```

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `id` | `int` | Datenbankschlüssel der neu angelegten Anwendung |
| `name` | `string` | Name der Anwendung |
| `baseUrl` | `string` | Basis-URL |
| `applicationGroupId` | `int?` | ID der zugeordneten Gruppe oder `null` |

**Response-Header:**

| Header | Beschreibung |
|--------|--------------|
| `Location` | URL der angelegten Ressource, z. B. `/api/applications/7` |
| `X-New-Token` | Neuer Bearer-Token; ersetzt den verwendeten Token |

**Fehler:**

| Status | Ursache |
|--------|---------|
| 400 Bad Request | Pflichtfeld fehlt oder überschreitet die maximale Länge |
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |
