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

---

## GET /api/endpoint-groups?applicationId={id}

Gibt alle `EndpointGroup`-Objekte einer Anwendung zurück.

**Authentifizierung:** Bearer-Token (aus `/authenticate`).

**Request-Header:**

| Header | Pflicht | Beschreibung |
|--------|---------|--------------|
| `Authorization` | Ja | `Bearer <token>` |
| `X-Storage-Mode` | Nein | `Team` oder `User` (Standard: `User`) |

**Query-Parameter:**

| Parameter | Typ | Pflicht | Beschreibung |
|-----------|-----|---------|--------------|
| `applicationId` | `int` | Ja | ID der Anwendung |

**Response: 200 OK**

```json
[
  { "id": 1, "name": "Stammdaten", "applicationId": 7, "parentGroupId": null, "rowVersion": "AAAAAAA=" },
  { "id": 2, "name": "Untergruppe", "applicationId": 7, "parentGroupId": 1, "rowVersion": "AAAAAAB=" }
]
```

**Response-Header:**

| Header | Beschreibung |
|--------|--------------|
| `X-New-Token` | Neuer Bearer-Token |

**Fehler:**

| Status | Ursache |
|--------|---------|
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |

---

## GET /api/endpoint-groups/{id}

Gibt eine einzelne `EndpointGroup` per ID zurück.

**Authentifizierung:** Bearer-Token.

**Response: 200 OK** — `EndpointGroupResponse` (analog zur Liste oben)

**Response-Header:** `X-New-Token`

**Fehler:**

| Status | Ursache |
|--------|---------|
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |
| 404 Not Found | Endpunktgruppe nicht gefunden |

---

## POST /api/endpoint-groups

Legt eine neue `EndpointGroup` an.

**Authentifizierung:** Bearer-Token.

**Request-Header:**

| Header | Pflicht | Beschreibung |
|--------|---------|--------------|
| `Authorization` | Ja | `Bearer <token>` |
| `X-Storage-Mode` | Nein | `Team` oder `User` |

**Request-Body:**

```json
{
  "name": "Stammdaten",
  "applicationId": 7,
  "parentGroupId": null
}
```

| Feld | Typ | Pflicht | Validierung | Beschreibung |
|------|-----|---------|-------------|--------------|
| `name` | `string` | Ja | `[Required]`, `[MaxLength(200)]` | Anzeigename der Gruppe |
| `applicationId` | `int` | Ja | `[Range(1, int.MaxValue)]` | ID der übergeordneten Anwendung |
| `parentGroupId` | `int?` | Nein | — | ID der übergeordneten Gruppe; `null` für Gruppen der obersten Ebene |

**Response: 201 Created**

```json
{ "id": 1, "name": "Stammdaten", "applicationId": 7, "parentGroupId": null, "rowVersion": "AAAAAAA=" }
```

**Response-Header:** `Location`, `X-New-Token`

**Fehler:**

| Status | Ursache |
|--------|---------|
| 400 Bad Request | Pflichtfeld fehlt oder zu lang |
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |

---

## PUT /api/endpoint-groups/{id}

Aktualisiert eine bestehende `EndpointGroup`.

**Authentifizierung:** Bearer-Token.

**Request-Body:**

```json
{ "name": "Stammdaten (neu)", "rowVersion": "AAAAAAA=" }
```

| Feld | Typ | Pflicht | Validierung | Beschreibung |
|------|-----|---------|-------------|--------------|
| `name` | `string` | Ja | `[Required]`, `[MaxLength(200)]` | Neuer Name |
| `rowVersion` | `byte[]` | Ja | — | Concurrency-Token; muss mit dem gespeicherten Wert übereinstimmen |

**Response: 200 OK** — aktualisierte `EndpointGroupResponse`

**Response-Header:** `X-New-Token`

**Fehler:**

| Status | Ursache |
|--------|---------|
| 400 Bad Request | Pflichtfeld fehlt |
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |
| 404 Not Found | Endpunktgruppe nicht gefunden |

---

## DELETE /api/endpoint-groups/{id}

Löscht eine `EndpointGroup`.

**Authentifizierung:** Bearer-Token.

**Response: 204 No Content**

**Response-Header:** `X-New-Token`

**Fehler:**

| Status | Ursache |
|--------|---------|
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |
| 404 Not Found | Endpunktgruppe nicht gefunden |

---

## GET /api/endpoints?applicationId={id}

Gibt alle `Endpoint`-Objekte einer Anwendung zurück, inklusive `Headers` und `QueryParameters`.

**Authentifizierung:** Bearer-Token.

**Request-Header:**

| Header | Pflicht | Beschreibung |
|--------|---------|--------------|
| `Authorization` | Ja | `Bearer <token>` |
| `X-Storage-Mode` | Nein | `Team` oder `User` |

**Query-Parameter:**

| Parameter | Typ | Pflicht | Beschreibung |
|-----------|-----|---------|--------------|
| `applicationId` | `int` | Ja | ID der Anwendung |

**Response: 200 OK**

```json
[
  {
    "id": 15,
    "name": "Artikel abrufen",
    "method": "Get",
    "relativePath": "/api/articles/{id}",
    "body": null,
    "bodyMode": "None",
    "authenticationType": "None",
    "applicationId": 7,
    "endpointGroupId": 1,
    "rowVersion": "AAAAAAA=",
    "preRequestScript": null,
    "postRequestScript": null,
    "headers": [
      { "id": 3, "key": "Accept", "value": "application/json", "endpointId": 15 }
    ],
    "queryParameters": [
      { "id": 8, "key": "id", "value": "", "endpointId": 15 }
    ]
  }
]
```

**Response-Header:** `X-New-Token`

**Fehler:**

| Status | Ursache |
|--------|---------|
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |

---

## GET /api/endpoints/{id}

Gibt einen einzelnen `Endpoint` per ID zurück, inklusive `Headers` und `QueryParameters`.

**Authentifizierung:** Bearer-Token.

**Response: 200 OK** — `EndpointResponse` (analog zur Liste oben)

**Response-Header:** `X-New-Token`

**Fehler:**

| Status | Ursache |
|--------|---------|
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |
| 404 Not Found | Endpunkt nicht gefunden |

---

## POST /api/endpoints

Legt einen neuen `Endpoint` an.

**Authentifizierung:** Bearer-Token.

**Request-Body:**

```json
{
  "name": "Artikel abrufen",
  "relativePath": "/api/articles/{id}",
  "applicationId": 7,
  "endpointGroupId": 1,
  "method": "Get",
  "bodyMode": "None",
  "body": null,
  "authenticationType": "None",
  "preRequestScript": null,
  "postRequestScript": null
}
```

| Feld | Typ | Pflicht | Validierung | Beschreibung |
|------|-----|---------|-------------|--------------|
| `name` | `string` | Ja | `[Required]`, `[MaxLength(200)]` | Anzeigename des Endpunkts |
| `relativePath` | `string` | Ja | `[Required]`, `[MaxLength(500)]` | Relativer Pfad (ohne Basis-URL) |
| `applicationId` | `int` | Ja | `[Range(1, int.MaxValue)]` | ID der zugehörigen Anwendung |
| `endpointGroupId` | `int?` | Nein | — | ID der Endpunktgruppe; `null` für ungruppierten Endpunkt |
| `method` | `HttpMethod` | Nein | — | HTTP-Methode (z. B. `Get`, `Post`, `Put`, `Delete`) |
| `bodyMode` | `BodyMode` | Nein | — | Body-Modus (z. B. `None`, `Raw`, `FormData`) |
| `body` | `string?` | Nein | — | Request-Body-Text |
| `authenticationType` | `AuthenticationType` | Nein | — | Authentifizierungstyp (z. B. `None`, `BearerToken`, `Negotiate`) |
| `preRequestScript` | `string?` | Nein | — | JavaScript-Code für Pre-Request-Skript |
| `postRequestScript` | `string?` | Nein | — | JavaScript-Code für Post-Request-Skript |

**Response: 201 Created** — `EndpointResponse` mit `headers: []` und `queryParameters: []`

**Response-Header:** `Location`, `X-New-Token`

**Fehler:**

| Status | Ursache |
|--------|---------|
| 400 Bad Request | Pflichtfeld fehlt oder zu lang |
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |

---

## PUT /api/endpoints/{id}

Aktualisiert einen bestehenden `Endpoint`.

**Authentifizierung:** Bearer-Token.

**Request-Body:** alle Felder aus `POST /api/endpoints` ohne `applicationId`, zusätzlich:

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| `rowVersion` | `byte[]` | Ja | Concurrency-Token |

**Response: 200 OK** — aktualisierte `EndpointResponse`

**Response-Header:** `X-New-Token`

**Fehler:**

| Status | Ursache |
|--------|---------|
| 400 Bad Request | Pflichtfeld fehlt |
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |
| 404 Not Found | Endpunkt nicht gefunden |

---

## DELETE /api/endpoints/{id}

Löscht einen `Endpoint`.

**Authentifizierung:** Bearer-Token.

**Response: 204 No Content**

**Response-Header:** `X-New-Token`

**Fehler:**

| Status | Ursache |
|--------|---------|
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |
| 404 Not Found | Endpunkt nicht gefunden |

---

## POST /api/endpoints/headers

Fügt einem Endpunkt einen neuen Header hinzu.

**Authentifizierung:** Bearer-Token.

**Request-Body:**

```json
{ "key": "Accept", "value": "application/json", "endpointId": 15 }
```

| Feld | Typ | Pflicht | Validierung | Beschreibung |
|------|-----|---------|-------------|--------------|
| `key` | `string` | Ja | `[Required]` | Header-Name |
| `value` | `string` | Nein | — | Header-Wert |
| `endpointId` | `int` | Ja | `[Range(1, int.MaxValue)]` | ID des Endpunkts |

**Response: 201 Created**

```json
{ "id": 3, "key": "Accept", "value": "application/json", "endpointId": 15 }
```

**Response-Header:** `X-New-Token`

**Fehler:**

| Status | Ursache |
|--------|---------|
| 400 Bad Request | Pflichtfeld fehlt |
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |

---

## DELETE /api/endpoints/headers/{id}

Löscht einen Header eines Endpunkts.

**Authentifizierung:** Bearer-Token.

**Response: 204 No Content**

**Response-Header:** `X-New-Token`

**Fehler:**

| Status | Ursache |
|--------|---------|
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |

---

## POST /api/endpoints/query-parameters

Fügt einem Endpunkt einen neuen Query-Parameter hinzu.

**Authentifizierung:** Bearer-Token.

**Request-Body:**

```json
{ "key": "filter", "value": "active", "endpointId": 15 }
```

| Feld | Typ | Pflicht | Validierung | Beschreibung |
|------|-----|---------|-------------|--------------|
| `key` | `string` | Ja | `[Required]` | Parameter-Name |
| `value` | `string` | Nein | — | Parameter-Wert |
| `endpointId` | `int` | Ja | `[Range(1, int.MaxValue)]` | ID des Endpunkts |

**Response: 201 Created**

```json
{ "id": 8, "key": "filter", "value": "active", "endpointId": 15 }
```

**Response-Header:** `X-New-Token`

**Fehler:**

| Status | Ursache |
|--------|---------|
| 400 Bad Request | Pflichtfeld fehlt |
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |

---

## DELETE /api/endpoints/query-parameters/{id}

Löscht einen Query-Parameter eines Endpunkts.

**Authentifizierung:** Bearer-Token.

**Response: 204 No Content**

**Response-Header:** `X-New-Token`

**Fehler:**

| Status | Ursache |
|--------|---------|
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |

---

## POST /api/applications/{id}/odata-import/apply

Wendet einen OData-Import-Diff serverseitig auf eine Anwendung an. Dieser Endpunkt wird normalerweise intern von `IApplicationApiClient.ApplyODataDiffAsync()` aufgerufen, kann aber auch von externen Clients für OData-Import-Workflows verwendet werden.

**Authentifizierung:** Bearer-Token (aus `/authenticate`).

**Request-Header:**

| Header | Pflicht | Beschreibung |
|--------|---------|--------------|
| `Authorization` | Ja | `Bearer <token>` |

**Pfadparameter:**

| Parameter | Typ | Pflicht | Beschreibung |
|-----------|-----|---------|--------------|
| `id` | `int` | Ja | ID der Anwendung |

**Request-Body:** `ImportDiff`-Objekt mit der Struktur:

```json
{
  "errorMessage": null,
  "newEndpoints": [
    {
      "id": 0,
      "name": "GET Products",
      "method": "Get",
      "relativePath": "/api/products",
      "applicationId": 42,
      "authenticationType": "BearerToken",
      "rowVersion": []
    }
  ],
  "changedEndpoints": [],
  "removedEndpoints": [],
  "bearerTokens": {
    "Products": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `errorMessage` | `string?` | Fehlermeldung aus dem Import-Scan; wird ignoriert bei Anwendung des Diffs |
| `newEndpoints` | `Endpoint[]` | Neue Endpunkte zum Einfügen |
| `changedEndpoints` | `Endpoint[]` | Zu ändernde Endpunkte |
| `removedEndpoints` | `Endpoint[]` | Zu löschende Endpunkte (nach ID) |
| `bearerTokens` | `Dictionary<string, string>` | Token-Zuordnung: Key ist Endpunkt-Name oder Entity-Set-Name, Value ist Token-String |

**Response: 204 No Content**

Der Diff wird vollständig angewendet:
- Neue Endpunkte werden eingefügt (mit entsprechenden Gruppen nach Entity-Set-Namen).
- Bestehende Endpunkte werden aktualisiert.
- Endpunkte, die nur auf der Datenbank-Seite existieren, werden gelöscht.
- Bearer-Tokens aus dem `bearerTokens`-Dictionary werden persistiert, wenn der zugehörige Endpunkt die Authentifizierung auf `BearerToken` setzt.

**Response-Header:** `X-New-Token`

**Fehler:**

| Status | Ursache |
|--------|---------|
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |
| 404 Not Found | Anwendung nicht gefunden |
| 422 Unprocessable Entity | Die Anwendung hat den Interface-Typ `OData` nicht; nur OData-Anwendungen unterstützen diesen Import-Endpunkt |
