# REST-API — Beschreibung

## Zweck

Die REST-API ermöglicht es, Anwendungsgruppen, Anwendungen, Endpunktgruppen und Endpunkte programmatisch zu verwalten, ohne direkt auf die Datenbank zuzugreifen. Intern wird sie ausschließlich von den Blazor-Komponenten über `IApplicationApiClient` genutzt; nach außen steht sie jedem HTTP-Client zur Verfügung, der sich über das Token-Verfahren authentifizieren kann.

## Funktionsweise

Die API umfasst folgende Ressourcenbereiche:

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
