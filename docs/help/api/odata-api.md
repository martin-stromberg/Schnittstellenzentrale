# OData v4-API — API-Dokumentation

## Übersicht

Der OData v4-Service ist unter dem Präfix `/odatav4` erreichbar. Er exponiert vier Entity-Sets und ein CSDL-Metadaten-Dokument. Alle Datenendpunkte erfordern einen gültigen Bearer-Token. Der Token kann direkt über `GET /odatav4/Authenticate()` oder `POST /odatav4/Authenticate()` (Windows/Negotiate) bezogen werden — unabhängig von der REST-API. OData-Abfrageoptionen (`$filter`, `$select`, `$expand`, `$orderby`, `$top`, `$skip`) werden auf allen Collection-Endpunkten unterstützt. Der `X-Storage-Mode`-Header wird von allen Datencontrollern ausgewertet (Standard: `User`), um die zurückgegebenen Datensätze zu filtern.

---

## OData-Metadaten-Import

Das unter `GET /odatav4/$metadata` veröffentlichte CSDL-Dokument wird auch vom internen **OData-Import-Workflow** verwendet: Ist eine Anwendung vom Typ `OData`, erscheint in der Detailansicht (`ApplicationContentView`) die Schaltfläche **OData-Import**. Ein Klick darauf ruft das CSDL-Dokument von `Application.InterfaceUrl` ab, leitet daraus Endpunkte ab (je ein GET und ein POST pro Entity-Set sowie Endpunkte für OData-Operationen) und zeigt eine Import-Vorschau. Nach Bestätigung werden die Endpunkte automatisch in der Datenbank angelegt oder aktualisiert.

Der OData-Import-Workflow ist analog zum Swagger-Import für REST-Anwendungen. Der Import liest optional die proprietäre Annotation `x-sz-bearer-token` aus dem CSDL-Dokument (auf Entity-Set- oder Operationsebene). Ist diese vorhanden, wird der importierte Endpunkt mit Authentifizierungstyp `BearerToken` versehen und der Token-Wert persistiert.

Technische Details und Anwenderanleitung zum OData-Import sind unter [Endpunkte](../endpunkte/beschreibung.md) dokumentiert.

---

## GET /odatav4/Authenticate()  ·  POST /odatav4/Authenticate()

Authentifiziert den aktuellen Windows-Benutzer per Negotiate und gibt einen Bearer-Token für die OData-API zurück. Beide Methoden (GET und POST) sind unterstützt — GET folgt der OData-Unbound-Function-Konvention, POST der Unbound-Action-Konvention.

**Authentifizierung:** Windows/Negotiate (wird vom Browser oder Client automatisch ausgehandelt)

**Response: 200 OK**

```json
{
  "token": "eyJhb..."
}
```

**Fehler:**

| Status | Ursache |
|--------|---------|
| 401 Unauthorized | Windows-Authentifizierung fehlgeschlagen oder Benutzeridentität nicht verfügbar |

Der zurückgegebene Token wird in allen weiteren OData-Requests als `Authorization: Bearer <token>` mitgesendet. Jede erfolgreiche OData-Antwort enthält im Header `X-New-Token` einen rotierten Nachfolge-Token, der den vorherigen ersetzt.

---

## GET /odatav4/$metadata

Gibt das CSDL-Metadaten-Dokument des OData-Service zurück.

**Authentifizierung:** keine (öffentlich zugänglich)

**Response: 200 OK** — CSDL-XML mit vier Entity-Sets: `Applications`, `ApplicationGroups`, `Endpoints`, `EndpointGroups`.

---

## GET /odatav4/Applications

Gibt alle Anwendungen als OData-Collection zurück.

**Authentifizierung:** Bearer-Token (aus `POST /odatav4/Authenticate()`).

**Request-Header:**

| Header | Pflicht | Beschreibung |
|--------|---------|--------------|
| `Authorization` | Ja | `Bearer <token>` |
| `X-Storage-Mode` | Nein | `Team` oder `User` (Standard: `User`); filtert die Datensätze je nach Speichermoduszugehörigkeit des authentifizierten Benutzers |

**OData-Abfrageoptionen:** `$filter`, `$select`, `$expand`, `$orderby`, `$top`, `$skip`, `$count`

**Response: 200 OK**

```json
{
  "@odata.context": "/odatav4/$metadata#Applications",
  "value": [
    {
      "id": 7,
      "name": "Bestellservice",
      "baseUrl": "https://intern/bestellservice",
      "interfaceUrl": "https://intern/bestellservice/swagger/v1/swagger.json",
      "interfaceType": "Swagger",
      "applicationGroupId": 42,
      "isSystem": false
    }
  ]
}
```

**Response-Header:**

| Header | Beschreibung |
|--------|--------------|
| `X-New-Token` | Neuer Bearer-Token; ersetzt den verwendeten Token |

**Fehler:**

| Status | Ursache |
|--------|---------|
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |

---

## GET /odatav4/Applications({key})

Gibt eine einzelne Anwendung per ID zurück.

**Authentifizierung:** Bearer-Token.

**Pfadparameter:**

| Parameter | Typ | Beschreibung |
|-----------|-----|--------------|
| `key` | `int` | ID der Anwendung |

**Response: 200 OK** — einzelnes `Application`-Objekt (analog zur Collection)

**Fehler:**

| Status | Ursache |
|--------|---------|
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |
| 404 Not Found | Anwendung nicht gefunden |

---

## POST /odatav4/Applications

Legt eine neue Anwendung an.

**Authentifizierung:** Bearer-Token.

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

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `name` | `string` | Anzeigename der Anwendung |
| `baseUrl` | `string` | Basis-URL des Dienstes |
| `description` | `string?` | Optionale Beschreibung |
| `interfaceUrl` | `string?` | URL zur API-Beschreibung; `InterfaceType` wird serverseitig automatisch erkannt |
| `applicationGroupId` | `int?` | ID der Gruppe; `null` für gruppenlose Anwendung |
| `owner` | `string?` | Windows-Benutzername |
| `id` | `int` | Wird serverseitig ignoriert (auf 0 zurückgesetzt) |
| `rowVersion` | `byte[]` | Wird bei POST serverseitig ignoriert |

**Response: 201 Created**

```json
{
  "id": 7,
  "name": "Bestellservice",
  "baseUrl": "https://intern/bestellservice",
  "interfaceType": "Swagger"
}
```

**Response-Header:**

| Header | Beschreibung |
|--------|--------------|
| `Location` | URL der angelegten Ressource, z. B. `/odatav4/Applications(7)` |
| `X-New-Token` | Neuer Bearer-Token |

**Fehler:**

| Status | Ursache |
|--------|---------|
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |

---

## PUT /odatav4/Applications({key})

Ersetzt eine Anwendung vollständig.

**Authentifizierung:** Bearer-Token.

**Pfadparameter:** `key` (int) — ID der Anwendung

**Request-Body:** analog zu `POST /odatav4/Applications`, jedoch **mit** nicht-leerem `rowVersion`-Feld (Optimistic-Concurrency-Pflichtfeld).

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `rowVersion` | `byte[]` | Pflichtfeld — muss den aktuellen RowVersion-Wert des Objekts enthalten |

`id` und `isSystem` werden serverseitig ignoriert.

**Response: 200 OK** — aktualisiertes `Application`-Objekt

**Fehler:**

| Status | Ursache |
|--------|---------|
| 400 Bad Request | `rowVersion` fehlt oder ist leer |
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |
| 403 Forbidden | Anwendung ist eine Systemanwendung (`IsSystem == true`) |
| 404 Not Found | Anwendung nicht gefunden |

---

## PATCH /odatav4/Applications({key})

Aktualisiert einzelne Felder einer Anwendung (partielles Update).

**Authentifizierung:** Bearer-Token.

**Request-Body:** JSON-Objekt mit den zu ändernden Feldern und dem Pflichtfeld `rowVersion` (Feldnamen case-insensitiv):

```json
{
  "description": "Aktualisierte Beschreibung",
  "applicationGroupId": null,
  "rowVersion": "AAAAAAA="
}
```

Unterstützte Felder: `name`, `description`, `baseUrl`, `interfaceUrl`, `owner`, `applicationGroupId`, `subtitle`, `iconData`, `rowVersion` (Pflicht).

**Null-Behandlung:** String-Felder (`name`, `description`, etc.) können explizit auf `null` gesetzt werden, um sie zu leeren. Numerische Felder wie `applicationGroupId` können auf `null` gesetzt werden. Der Wert `null` setzt das Feld auf seinen leeren Zustand oder `null`-Wert.

**Type-Guards:** Numerische Felder wie `applicationGroupId` akzeptieren nur JSON-Number-Werte oder `null` — String-Werte oder Arrays werden mit 400 Bad Request abgewiesen.

`id` wird im PATCH-Body ignoriert.

**Response: 200 OK** — aktualisiertes `Application`-Objekt

**Fehler:**

| Status | Ursache |
|--------|---------|
| 400 Bad Request | `rowVersion` fehlt, hat ungültiges Base64-Format oder ein Feld hat einen ungültigen Datentyp |
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |
| 403 Forbidden | Anwendung ist eine Systemanwendung |
| 404 Not Found | Anwendung nicht gefunden |
| 409 Conflict | Die Anwendung wurde zwischenzeitlich von einem anderen Client geändert |

---

## DELETE /odatav4/Applications({key})

Löscht eine Anwendung.

**Authentifizierung:** Bearer-Token.

**Response: 204 No Content**

**Fehler:**

| Status | Ursache |
|--------|---------|
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |
| 403 Forbidden | Anwendung ist eine Systemanwendung |
| 404 Not Found | Anwendung nicht gefunden |

---

## GET /odatav4/ApplicationGroups

Gibt alle Anwendungsgruppen als OData-Collection zurück.

**Authentifizierung:** Bearer-Token.

**Request-Header:**

| Header | Pflicht | Beschreibung |
|--------|---------|--------------|
| `Authorization` | Ja | `Bearer <token>` |
| `X-Storage-Mode` | Nein | `Team` oder `User` (Standard: `User`); filtert Gruppen nach Speichermoduszugehörigkeit |

**OData-Abfrageoptionen:** `$filter`, `$select`, `$expand`, `$orderby`, `$top`, `$skip`, `$count`

**Response: 200 OK** — OData-Collection mit `ApplicationGroup`-Objekten

**Fehler:** 401 Unauthorized

---

## GET /odatav4/ApplicationGroups({key})

Gibt eine einzelne Anwendungsgruppe per ID zurück.

**Fehler:** 401 Unauthorized, 404 Not Found

---

## POST /odatav4/ApplicationGroups

Legt eine neue Anwendungsgruppe an.

**Request-Body:**

```json
{
  "name": "Backend-Services",
  "description": null,
  "subtitle": null
}
```

`id` und `rowVersion` werden ignoriert.

**Response: 201 Created** mit `Location`-Header und `X-New-Token`

**Fehler:** 401 Unauthorized

---

## PUT /odatav4/ApplicationGroups({key})

Ersetzt eine Anwendungsgruppe vollständig.

**Felder:** `name`, `description`, `subtitle`, `iconData`, `rowVersion` (Pflichtfeld)

`id` und `isSystem` werden serverseitig ignoriert.

**Response: 200 OK**

**Fehler:** 400 Bad Request (`rowVersion` fehlt), 401 Unauthorized, 403 Forbidden (Systemgruppe), 404 Not Found

---

## PATCH /odatav4/ApplicationGroups({key})

Partielles Update einer Anwendungsgruppe.

Unterstützte Felder: `name`, `description`, `subtitle`, `iconData`, `rowVersion` (Pflicht).

**Null-Behandlung:** `description` und `subtitle` können auf `null` gesetzt werden. Das Feld `name` akzeptiert nur String-Werte.

**Response: 200 OK**

**Fehler:**

| Status | Ursache |
|--------|---------|
| 400 Bad Request | `rowVersion` fehlt oder ein Feld hat einen ungültigen Datentyp |
| 401 Unauthorized | Token fehlt oder ist ungültig |
| 403 Forbidden | Gruppe ist eine Systemgruppe |
| 404 Not Found | Gruppe nicht gefunden |
| 409 Conflict | Die Gruppe wurde zwischenzeitlich von einem anderen Client geändert |

---

## DELETE /odatav4/ApplicationGroups({key})

Löscht eine Anwendungsgruppe.

**Response: 204 No Content**

**Fehler:** 401 Unauthorized, 403 Forbidden (Systemgruppe), 404 Not Found

---

## GET /odatav4/Endpoints

Gibt alle Endpunkte der Anwendungen zurück, die für den authentifizierten Benutzer im gewählten Speichermodus sichtbar sind.

**Authentifizierung:** Bearer-Token.

**Request-Header:**

| Header | Pflicht | Beschreibung |
|--------|---------|--------------|
| `Authorization` | Ja | `Bearer <token>` |
| `X-Storage-Mode` | Nein | `Team` oder `User` (Standard: `User`); filtert die Datensätze nach Speichermoduszugehörigkeit |

**OData-Abfrageoptionen:** `$filter`, `$select`, `$expand`, `$orderby`, `$top`, `$skip`, `$count`

**Response: 200 OK** — OData-Collection mit `Endpoint`-Objekten (ohne `Headers` und `QueryParameters` — diese Felder sind im EDM-Modell ausgeblendet)

**Fehler:** 401 Unauthorized

---

## GET /odatav4/Endpoints({key})

Gibt einen einzelnen Endpunkt per ID zurück.

**Fehler:** 401 Unauthorized, 404 Not Found

---

## POST /odatav4/Endpoints

Legt einen neuen Endpunkt an.

**Request-Body:**

```json
{
  "name": "Artikel abrufen",
  "relativePath": "/api/articles/{id}",
  "applicationId": 7,
  "method": "Get",
  "bodyMode": "None",
  "authenticationType": "None",
  "endpointGroupId": null
}
```

`id` und `rowVersion` werden ignoriert.

**Response: 201 Created** — gespeicherter Endpunkt mit `Location`-Header

**Fehler:**

| Status | Ursache |
|--------|---------|
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |
| 403 Forbidden | Die zugehörige Anwendung (`applicationId`) ist eine Systemanwendung |
| 404 Not Found | Anwendung mit `applicationId` nicht gefunden |

---

## PUT /odatav4/Endpoints({key})

Ersetzt einen Endpunkt vollständig.

**Felder:** `name`, `method`, `relativePath`, `body`, `bodyMode`, `authenticationType`, `endpointGroupId`, `preRequestScript`, `postRequestScript`, `rowVersion` (Pflichtfeld)

`id` wird serverseitig ignoriert.

**Response: 200 OK**

**Fehler:** 400 Bad Request (`rowVersion` fehlt), 401 Unauthorized, 403 Forbidden (Systemanwendung), 404 Not Found

---

## PATCH /odatav4/Endpoints({key})

Partielles Update eines Endpunkts.

Unterstützte Felder: `name`, `relativePath`, `body`, `preRequestScript`, `postRequestScript`, `endpointGroupId`, `method`, `authenticationType`, `rowVersion` (Pflicht).

**Null-Behandlung:** String-Felder können auf `null` gesetzt werden. Das Feld `endpointGroupId` kann auf `null` gesetzt werden (um den Endpunkt aus einer Gruppe zu entfernen).

**Type-Guards:** Numerische Felder wie `endpointGroupId` akzeptieren nur JSON-Number-Werte oder `null`.

**Response: 200 OK**

**Fehler:**

| Status | Ursache |
|--------|---------|
| 400 Bad Request | `rowVersion` fehlt, ein Feld hat einen ungültigen Datentyp, oder `endpointGroupId` gehört nicht zur selben Anwendung |
| 401 Unauthorized | Token fehlt oder ist ungültig |
| 403 Forbidden | Endpunkt gehört zu einer Systemanwendung |
| 404 Not Found | Endpunkt nicht gefunden |
| 409 Conflict | Der Endpunkt wurde zwischenzeitlich von einem anderen Client geändert |

---

## DELETE /odatav4/Endpoints({key})

Löscht einen Endpunkt.

**Response: 204 No Content**

**Fehler:** 401 Unauthorized, 403 Forbidden (Systemanwendung), 404 Not Found

---

## GET /odatav4/EndpointGroups

Gibt alle Endpunktgruppen der Anwendungen zurück, die für den authentifizierten Benutzer im gewählten Speichermodus sichtbar sind.

**Authentifizierung:** Bearer-Token.

**Request-Header:**

| Header | Pflicht | Beschreibung |
|--------|---------|--------------|
| `Authorization` | Ja | `Bearer <token>` |
| `X-Storage-Mode` | Nein | `Team` oder `User` (Standard: `User`); filtert die Datensätze nach Speichermoduszugehörigkeit |

**OData-Abfrageoptionen:** `$filter`, `$select`, `$expand`, `$orderby`, `$top`, `$skip`, `$count`

**Response: 200 OK** — OData-Collection mit `EndpointGroup`-Objekten

**Fehler:** 401 Unauthorized

---

## GET /odatav4/EndpointGroups({key})

Gibt eine einzelne Endpunktgruppe per ID zurück.

**Fehler:** 401 Unauthorized, 404 Not Found

---

## POST /odatav4/EndpointGroups

Legt eine neue Endpunktgruppe an.

**Request-Body:**

```json
{
  "name": "Stammdaten",
  "applicationId": 7,
  "parentGroupId": null
}
```

`id` und `rowVersion` werden ignoriert.

**Response: 201 Created** mit `Location`-Header

**Fehler:**

| Status | Ursache |
|--------|---------|
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |
| 403 Forbidden | Die zugehörige Anwendung ist eine Systemanwendung |
| 404 Not Found | Anwendung mit `applicationId` nicht gefunden |

---

## PUT /odatav4/EndpointGroups({key})

Ersetzt eine Endpunktgruppe vollständig.

**Felder:** `name`, `parentGroupId`, `rowVersion` (Pflichtfeld)

`id` und `applicationId` werden serverseitig ignoriert.

**Response: 200 OK**

**Fehler:** 400 Bad Request (`rowVersion` fehlt), 401 Unauthorized, 403 Forbidden (Systemanwendung), 404 Not Found

---

## PATCH /odatav4/EndpointGroups({key})

Partielles Update einer Endpunktgruppe.

Unterstützte Felder: `name`, `parentGroupId`, `rowVersion` (Pflicht).

**Null-Behandlung:** `parentGroupId` kann auf `null` gesetzt werden (um die Gruppe auf die oberste Ebene zu verschieben).

**Type-Guards:** Das Feld `parentGroupId` akzeptiert nur JSON-Number-Werte oder `null`.

**Response: 200 OK**

**Fehler:**

| Status | Ursache |
|--------|---------|
| 400 Bad Request | `rowVersion` fehlt, ein Feld hat einen ungültigen Datentyp, oder `parentGroupId` gehört nicht zur selben Anwendung |
| 401 Unauthorized | Token fehlt oder ist ungültig |
| 403 Forbidden | Gruppe gehört zu einer Systemanwendung |
| 404 Not Found | Gruppe nicht gefunden |
| 409 Conflict | Die Gruppe wurde zwischenzeitlich von einem anderen Client geändert |

---

## DELETE /odatav4/EndpointGroups({key})

Löscht eine Endpunktgruppe.

**Response: 204 No Content**

**Fehler:** 401 Unauthorized, 403 Forbidden (Systemanwendung), 404 Not Found
