# REST-API — Beschreibung

## Zweck

Die REST-API ermöglicht es, Anwendungsgruppen und Anwendungen programmatisch anzulegen, ohne direkt auf die Datenbank zuzugreifen. Intern wird sie von den Blazor-Komponenten `ApplicationGroupEditor` und `ApplicationEditor` genutzt; nach außen steht sie jedem HTTP-Client zur Verfügung, der sich über das Token-Verfahren authentifizieren kann.

## Funktionsweise

Die API besteht aus drei Endpunkten:

| Endpunkt | Methode | Zweck |
|----------|---------|-------|
| `/authenticate` | POST | Token beziehen (erfordert Windows-Authentifizierung) |
| `/api/application-groups` | POST | Neue `ApplicationGroup` anlegen |
| `/api/applications` | POST | Neue `Application` anlegen |

Der Ablauf für jeden Datenaufruf ist zweistufig:

1. Der Client ruft `/authenticate` auf. ASP.NET Core wertet die Windows-Identität des Aufrufers aus und gibt ein kurzlebiges Token (GUID, 5 Minuten Ablaufzeit) zurück.
2. Der Client übergibt dieses Token als Bearer-Token im `Authorization`-Header bei jedem nachfolgenden Datenendpunkt-Aufruf. Nach jedem erfolgreichen Aufruf wird das verwendete Token invalidiert und ein neues Token im Response-Header `X-New-Token` zurückgegeben.

Die Controller delegieren Schreiboperationen an das bestehende `IApplicationRepository`; die Datenbankschicht bleibt unverändert. Im Team-Modus lösen die Controller SignalR-Benachrichtigungen aus, damit andere verbundene Clients den aktuellen Stand erhalten.

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

**Beispiel: Anwendung anlegen**

```
POST /api/applications
Authorization: Bearer 9c1c2ab0-...
X-Storage-Mode: Team
Content-Type: application/json

{
  "name": "Bestellservice",
  "baseUrl": "https://intern/bestellservice",
  "description": "Verarbeitet Bestellungen",
  "interfaceUrl": "https://intern/bestellservice/swagger/v1/swagger.json",
  "applicationGroupId": 42
}

→ HTTP 201 Created
Location: /api/applications/7
X-New-Token: b4f8e...
{ "id": 7, "name": "Bestellservice", "baseUrl": "https://intern/bestellservice", "applicationGroupId": 42 }
```

## Einschränkungen

- Die API unterstützt derzeit ausschließlich Anlage-Operationen (POST). Lese-, Aktualisierungs- und Löschoperationen sind nicht implementiert.
- Lesezugriffe (z. B. Laden der Gruppenliste) gehen weiterhin direkt über `IApplicationRepository`.
- Tokens sind nicht persistiert; ein Neustart der Anwendung invalidiert alle aktiven Tokens.
- Ein Token wird nach jedem erfolgreichen Datenendpunkt-Aufruf rotiert; das alte Token ist danach nicht mehr verwendbar.
