# REST-API und OData v4-API — Beschreibung

## Zweck

Die Schnittstellenzentrale stellt zwei API-Oberflächen bereit:

**REST-API** — ermöglicht es, Anwendungsgruppen, Anwendungen, Endpunktgruppen und Endpunkte programmatisch zu verwalten, ohne direkt auf die Datenbank zuzugreifen. Intern wird sie ausschließlich von den Blazor-Komponenten über `IApplicationApiClient` genutzt; nach außen steht sie jedem HTTP-Client zur Verfügung, der sich über das Token-Verfahren authentifizieren kann.

**OData v4-API** — stellt dieselben vier Kernobjekte als OData-Entity-Sets unter dem Präfix `/odatav4` bereit. Sie richtet sich primär an maschinelle Clients, den `IODataImportService` und OData-kompatible Werkzeuge. Ein CSDL-Metadaten-Dokument wird automatisch unter `GET /odatav4/$metadata` veröffentlicht.

## Funktionsweise

### REST-API

Die REST-API umfasst folgende Ressourcenbereiche:

| Endpunkte | Zweck |
|-----------|-------|
| `POST /authenticate` | Token beziehen (erfordert Windows-Authentifizierung) |
| `/api/application-groups` | CRUD für `ApplicationGroup` |
| `/api/applications` | CRUD für `Application` |
| `/api/endpoint-groups` | CRUD für `EndpointGroup` |
| `/api/endpoints` | CRUD für `Endpoint` inkl. Header- und Query-Parameter-Routen |

Der Ablauf für jeden Datenaufruf ist zweistufig:

1. Der Client ruft `/authenticate` auf. ASP.NET Core wertet die Windows-Identität des Aufrufers aus und gibt ein kurzlebiges Token (GUID, 5 Minuten Ablaufzeit) zurück.
2. Der Client übergibt dieses Token als Bearer-Token im `Authorization`-Header bei jedem nachfolgenden Datenendpunkt-Aufruf. Nach jedem erfolgreichen Aufruf wird das verwendete Token invalidiert und ein neues Token im Response-Header `X-New-Token` zurückgegeben.

Die Controller delegieren Lese- und Schreiboperationen an `IEndpointRepository` bzw. `IApplicationRepository`; die Datenbankschicht bleibt unverändert. Im Team-Modus lösen die Controller SignalR-Benachrichtigungen aus, damit andere verbundene Clients den aktuellen Stand erhalten.

### OData v4-API

Die OData-API exponiert dieselben vier Kernobjekte als OData-Entity-Sets:

| Entity-Set | Präfix | Zweck |
|------------|--------|-------|
| `Applications` | `/odatav4/Applications` | CRUD für `Application` |
| `ApplicationGroups` | `/odatav4/ApplicationGroups` | CRUD für `ApplicationGroup` |
| `Endpoints` | `/odatav4/Endpoints` | CRUD für `Endpoint` |
| `EndpointGroups` | `/odatav4/EndpointGroups` | CRUD für `EndpointGroup` |
| `$metadata` | `/odatav4/$metadata` | CSDL-Metadaten-Dokument |

Alle Endpunkte erfordern denselben Bearer-Token aus `/odatav4/Authenticate()`. OData-Standardabfragemöglichkeiten wie `$filter`, `$select`, `$expand`, `$orderby`, `$top` und `$skip` werden auf Collection-Endpunkten unterstützt. Der `X-Storage-Mode`-Header wird von allen Datencontrollern ausgewertet (Standard: `User`); Collection-Endpunkte liefern nur die Datensätze des authentifizierten Benutzers im gewählten Modus — analog zur REST-API.

## Beispiele

**Beispiel: Anwendungsgruppe anlegen**

```
POST /authenticate
→ { "token": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }

POST /api/application-groups
Authorization: Bearer 3fa85f64-5717-4562-b3fc-2c963f66afa6
X-Storage-Mode: Team
Content-Type: application/json

{ "name": "Backend-Services" }

→ HTTP 201 Created
Location: /api/application-groups/42
X-New-Token: 9c1c2ab0-...
{ "id": 42, "name": "Backend-Services" }
```

**Beispiel: Endpunkt anlegen**

```
POST /api/endpoints
Authorization: Bearer 9c1c2ab0-...
X-Storage-Mode: User
Content-Type: application/json

{
  "name": "Artikel abrufen",
  "relativePath": "/api/articles/{id}",
  "applicationId": 7,
  "method": "Get",
  "authenticationType": "None"
}

→ HTTP 201 Created
Location: /api/endpoints/15
X-New-Token: c7d1f...
{ "id": 15, "name": "Artikel abrufen", "relativePath": "/api/articles/{id}", ... }
```

**Beispiel: Header zu einem Endpunkt hinzufügen**

```
POST /api/endpoints/headers
Authorization: Bearer c7d1f...
X-Storage-Mode: User
Content-Type: application/json

{ "key": "Accept", "value": "application/json", "endpointId": 15 }

→ HTTP 201 Created
{ "id": 3, "key": "Accept", "value": "application/json", "endpointId": 15 }
```

## Einschränkungen

- Tokens sind nicht persistiert; ein Neustart der Anwendung invalidiert alle aktiven Tokens.
- Ein Token wird nach jedem erfolgreichen Datenendpunkt-Aufruf rotiert; das alte Token ist danach nicht mehr verwendbar.
- Die atomaren DELETE-Routen für Header und Query-Parameter (`DELETE /api/endpoints/headers/{id}`, `DELETE /api/endpoints/query-parameters/{id}`) geben kein 404 zurück, wenn die ID nicht existiert — der Aufruf von `IEndpointRepository.DeleteHeaderAsync` schlägt dann mit einer Datenbankausnahme fehl.
- Die OData-API unterstützt keine Optimistic Concurrency über Standard-OData-ETags; stattdessen wird `RowVersion` als Pflichtfeld bei PUT und PATCH übergeben. PATCH fängt `DbUpdateConcurrencyException` ab und antwortet mit `409 Conflict`; bei PUT wird die Exception nicht explizit behandelt.
- Die OData-API exponiert keine Header und Query-Parameter von `Endpoint`-Objekten (diese Navigationseigenschaften sind im EDM-Modell bewusst ausgeblendet, da die OData-API primär für den `IODataImportService`-Workflow ausgelegt ist).
