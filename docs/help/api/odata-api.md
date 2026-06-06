# OData v4-API — API-Dokumentation

## Übersicht

Der OData v4-Service ist unter dem Präfix `/odatav4` erreichbar. Er exponiert vier Entity-Sets und ein CSDL-Metadaten-Dokument. Alle Datenendpunkte erfordern einen gültigen Bearer-Token. Der Token kann direkt über `GET /odatav4/authenticate` oder `POST /odatav4/authenticate` (Windows/Negotiate) bezogen werden — unabhängig von der REST-API. OData-Abfrageoptionen (`$filter`, `$select`, `$expand`, `$orderby`, `$top`, `$skip`) werden auf allen Collection-Endpunkten unterstützt.

---

## GET /odatav4/authenticate  ·  POST /odatav4/authenticate

Authentifiziert den aktuellen Windows-Benutzer per Negotiate und gibt einen Bearer-Token für die OData-API zurück.

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

**Authentifizierung:** Bearer-Token (aus `POST /authenticate`).

**Request-Header:**

| Header | Pflicht | Beschreibung |
|--------|---------|--------------|
| `Authorization` | Ja | `Bearer <token>` |

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
| `rowVersion` | `byte[]` | Wird serverseitig ignoriert |

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

**Request-Body:** analog zu `POST /odatav4/Applications`

**Response: 200 OK** — aktualisiertes `Application`-Objekt

**Fehler:**

| Status | Ursache |
|--------|---------|
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |
| 403 Forbidden | Anwendung ist eine Systemanwendung (`IsSystem == true`) |
| 404 Not Found | Anwendung nicht gefunden |

---

## PATCH /odatav4/Applications({key})

Aktualisiert einzelne Felder einer Anwendung (partielles Update).

**Authentifizierung:** Bearer-Token.

**Request-Body:** JSON-Objekt mit nur den zu ändernden Feldern (Feldnamen case-insensitiv):

```json
{
  "description": "Aktualisierte Beschreibung",
  "applicationGroupId": null
}
```

Unterstützte Felder: `name`, `description`, `baseUrl`, `interfaceUrl`, `owner`, `applicationGroupId`, `subtitle`, `iconData`.

`id` und `rowVersion` werden auch im PATCH-Body ignoriert.

**Response: 200 OK** — aktualisiertes `Application`-Objekt

**Fehler:**

| Status | Ursache |
|--------|---------|
| 401 Unauthorized | Token fehlt, unbekannt oder abgelaufen |
| 403 Forbidden | Anwendung ist eine Systemanwendung |
| 404 Not Found | Anwendung nicht gefunden |

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

**Felder:** `name`, `description`, `subtitle`, `iconData`

**Response: 200 OK**

**Fehler:** 401 Unauthorized, 403 Forbidden (Systemgruppe), 404 Not Found

---

## PATCH /odatav4/ApplicationGroups({key})

Partielles Update einer Anwendungsgruppe.

Unterstützte Felder: `name`, `description`, `subtitle`.

**Response: 200 OK**

**Fehler:** 401 Unauthorized, 403 Forbidden (Systemgruppe), 404 Not Found

---

## DELETE /odatav4/ApplicationGroups({key})

Löscht eine Anwendungsgruppe.

**Response: 204 No Content**

**Fehler:** 401 Unauthorized, 403 Forbidden (Systemgruppe), 404 Not Found

---

## GET /odatav4/Endpoints

Gibt alle Endpunkte aller Anwendungen (ohne Systemanwendungsfilterung) zurück.

**Authentifizierung:** Bearer-Token.

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

**Felder:** `name`, `method`, `relativePath`, `body`, `bodyMode`, `authenticationType`, `endpointGroupId`, `preRequestScript`, `postRequestScript`

**Response: 200 OK**

**Fehler:** 401 Unauthorized, 403 Forbidden (Systemanwendung), 404 Not Found

---

## PATCH /odatav4/Endpoints({key})

Partielles Update eines Endpunkts.

Unterstützte Felder: `name`, `relativePath`, `body`, `preRequestScript`, `postRequestScript`, `endpointGroupId`, `method`, `authenticationType`.

**Response: 200 OK**

**Fehler:** 401 Unauthorized, 403 Forbidden (Systemanwendung), 404 Not Found

---

## DELETE /odatav4/Endpoints({key})

Löscht einen Endpunkt.

**Response: 204 No Content**

**Fehler:** 401 Unauthorized, 403 Forbidden (Systemanwendung), 404 Not Found

---

## GET /odatav4/EndpointGroups

Gibt alle Endpunktgruppen zurück.

**Authentifizierung:** Bearer-Token.

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

**Felder:** `name`, `parentGroupId`

**Response: 200 OK**

**Fehler:** 401 Unauthorized, 403 Forbidden (Systemanwendung), 404 Not Found

---

## PATCH /odatav4/EndpointGroups({key})

Partielles Update einer Endpunktgruppe.

Unterstützte Felder: `name`, `parentGroupId`.

**Response: 200 OK**

**Fehler:** 401 Unauthorized, 403 Forbidden (Systemanwendung), 404 Not Found

---

## DELETE /odatav4/EndpointGroups({key})

Löscht eine Endpunktgruppe.

**Response: 204 No Content**

**Fehler:** 401 Unauthorized, 403 Forbidden (Systemanwendung), 404 Not Found
