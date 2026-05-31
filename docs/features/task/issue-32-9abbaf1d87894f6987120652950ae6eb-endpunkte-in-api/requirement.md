# Anforderungsübersetzung – Endpunkte in API

## Fachliche Zusammenfassung

`EndpointGroup`- und `Endpoint`-Daten werden in mehreren Blazor-Komponenten (`ApplicationGroupTree`, `FolderContentView`, `ApplicationContentView`, `EndpointPage`) noch direkt über `IEndpointRepository` aus der Datenbank gelesen. Diese Direktzugriffe müssen vollständig auf `IApplicationApiClient` umgestellt werden, der bereits den etablierten Transportweg über die interne REST-API mit Token-Rotation und Storage-Mode-Header verwendet. Parallel dazu sind REST-Endpunkte für Endpoint-Gruppen und Endpunkte in der eigenen API bereitzustellen sowie der `ApplicationApiClient` um die entsprechenden Methoden zu erweitern. Das Architekturprinzip „alle Datenentitäten werden ausschließlich über `IApplicationApiClient` abgerufen" ist in `CLAUDE.md` zu verankern.

Darüber hinaus sind die Tests des `ApplicationApiClient` zu vervollständigen: Unit-Tests mit gemocktem `HttpMessageHandler` (Muster wie in `ApplicationApiClientTests`) und Integrationstests gegen eine per `WebApplicationFactory` bereitgestellte API (Muster wie in `ApplicationsControllerIntegrationTests`). Das Testprinzip — beide Testarten für alle API-Clients — ist ebenfalls in `CLAUDE.md` aufzunehmen.

Der `SystemEndpointSyncService` und der zugehörige `SystemEntryInitializer` legen beim ersten Start automatisch die Schnittstellenzentrale als Systemanwendung mit Endpunkten an. Die bestehenden Tests in `SystemEndpointSyncServiceTests` und `SystemEntryInitializerTests` sind so zu erweitern, dass sie vollständig prüfen, ob bei der automatischen Anlage auch Endpunkte inklusive aller Abhängigkeiten (Autorisierungstypen, Pre-/Post-Request-Skripte, Credential-Manager-Einträge für Bearer-Token) korrekt angelegt werden.

## Betroffene Klassen und Komponenten

### Neue Artefakte

- `EndpointGroupResponse` — neues Response-DTO in `Schnittstellenzentrale.Core/Contracts/`
- `EndpointResponse` — neues Response-DTO in `Schnittstellenzentrale.Core/Contracts/`
- `CreateEndpointGroupRequest` — neues Request-DTO in `Schnittstellenzentrale.Core/Contracts/`
- `UpdateEndpointGroupRequest` — neues Request-DTO in `Schnittstellenzentrale.Core/Contracts/`
- `CreateEndpointRequest` — neues Request-DTO in `Schnittstellenzentrale.Core/Contracts/`
- `UpdateEndpointRequest` — neues Request-DTO in `Schnittstellenzentrale.Core/Contracts/`
- `EndpointGroupsController` — neuer API-Controller `[Route("api/endpoint-groups")]`
- `EndpointsController` — neuer API-Controller `[Route("api/endpoints")]`
- `ApplicationApiClientIntegrationTests` — neue Integrationstestklasse in `Schnittstellenzentrale.Tests/Integration/`

### Zu erweiternde Artefakte

- `IApplicationApiClient` — neue Methoden für `EndpointGroup`- und `Endpoint`-CRUD sowie für Abfrage nach `applicationId`
- `ApplicationApiClient` — Implementierung der neuen Methoden inklusive Mapping von Response-DTOs auf Domänenmodelle
- `ApplicationApiClientTests` — Unit-Tests für alle neuen Methoden (GET, POST, PUT, DELETE je Ressourcentyp)
- `ApplicationGroupTree.razor` — `ReloadApplicationDataAsync` von `IEndpointRepository` auf `IApplicationApiClient` umstellen
- `FolderContentView.razor` — `OnParametersSetAsync` von `IEndpointRepository` auf `IApplicationApiClient` umstellen
- `ApplicationContentView.razor` — `OnParametersSetAsync` von `IEndpointRepository` auf `IApplicationApiClient` umstellen
- `EndpointPage.razor` — `ForceSaveAsync` und `SendRequestAsync` von `IEndpointRepository` auf `IApplicationApiClient` umstellen
- `SystemEndpointSyncServiceTests` — Testfälle für vollständige Endpoint-Anlage (Auth, Skripte, Credentials) ergänzen
- `SystemEntryInitializerTests` — Testfall ergänzen, der nach `InitializeAsync` prüft, ob Endpunkte mit korrekter Autorisierung, Skripten und Credential-Einträgen angelegt wurden
- `CLAUDE.md` — Abschnitt „Architekturprinzip: API-First" und Abschnitt „Testanforderungen für API-Clients" hinzufügen

### Nicht betroffen (Direktzugriff bleibt bestehen)

- `EndpointPage.razor` — Persistenz-Operationen (`AddEndpointAsync`, `UpdateEndpointAsync`) können über `IApplicationApiClient` laufen; das Lesen des frischen `RowVersion`-Werts bei `ForceSaveAsync` muss über den Client erfolgen.
- `SystemEndpointSyncService` — verwendet `IEndpointRepository` intern beim Startup-Abgleich; das ist architektonisch vertretbar, da der Service serverseitig läuft und keinen HTTP-Roundtrip benötigt. (Annahme — ggf. mit dem Team zu klären.)

## Implementierungsansatz

### 1. API-Endpunkte bereitstellen

Analog zu `ApplicationsController` und `ApplicationGroupsController` werden zwei neue Controller erstellt:

- `EndpointGroupsController` mit Routes:
  - `GET /api/endpoint-groups?applicationId={id}` — liefert alle Gruppen einer Anwendung
  - `GET /api/endpoint-groups/{id}` — liefert eine Gruppe per ID
  - `POST /api/endpoint-groups` — legt eine neue Gruppe an
  - `PUT /api/endpoint-groups/{id}` — aktualisiert eine Gruppe
  - `DELETE /api/endpoint-groups/{id}` — löscht eine Gruppe
- `EndpointsController` mit Routes:
  - `GET /api/endpoints?applicationId={id}` — liefert alle Endpunkte einer Anwendung
  - `GET /api/endpoints/{id}` — liefert einen Endpunkt per ID
  - `POST /api/endpoints` — legt einen Endpunkt an
  - `PUT /api/endpoints/{id}` — aktualisiert einen Endpunkt
  - `DELETE /api/endpoints/{id}` — löscht einen Endpunkt

Alle Controller erben von `ApiControllerBase`, verwenden `[RequiresContextHeaders]` und delegieren an `IEndpointRepository`. Token-Rotation und Storage-Mode-Verarbeitung folgen dem bestehenden Muster.

### 2. IApplicationApiClient und ApplicationApiClient erweitern

`IApplicationApiClient` erhält neue Methoden, die den neuen Controller-Routen entsprechen. `ApplicationApiClient` implementiert diese Methoden mit den gleichen Hilfsmethoden (`BuildGetRequest`, `BuildRequestWithBody`, `BuildDeleteRequest`, `SendWithTokenAsync` etc.) und mappt Response-DTOs auf Domänenmodelle.

### 3. Blazor-Komponenten umstellen

- `ApplicationGroupTree.razor`: `ReloadApplicationDataAsync` ruft statt `IEndpointRepository.GetEndpointGroupsAsync` und `GetEndpointsAsync` die entsprechenden Methoden des `IApplicationApiClient` auf. `IEndpointRepository`-Injektion entfällt.
- `FolderContentView.razor`: `OnParametersSetAsync` nutzt `IApplicationApiClient` statt `IEndpointRepository`.
- `ApplicationContentView.razor`: `OnParametersSetAsync` nutzt `IApplicationApiClient` statt `IEndpointRepository`.
- `EndpointPage.razor`: `ForceSaveAsync` (Lesen von `RowVersion`) und `SendRequestAsync` (Reload nach Save) nutzen `IApplicationApiClient`. Persistenz-Operationen (`PersistAsync`) ebenfalls umstellen.

### 4. Tests vervollständigen

**Unit-Tests** (`ApplicationApiClientTests`): Für jede neue Methode in `IApplicationApiClient` wird ein Testfall nach dem bestehenden Muster ergänzt. Geprüft werden: korrekte HTTP-Methode, URL-Pfad, Pflicht-Header (`Authorization`, `X-Storage-Mode`), Response-Mapping.

**Integrationstests** (`ApplicationApiClientIntegrationTests`): Analog zu `ApplicationsControllerIntegrationTests` und `ApplicationGroupsControllerIntegrationTests`. Die `ControllerTestFactory` wird wiederverwendet; die Testfälle decken die Happy Paths und wichtige Fehlerfälle (401, 404) der neuen Controller ab.

**SystemEndpointSyncServiceTests** erweitern: Neue Testfälle prüfen, dass beim Sync für Swagger-Operationen mit `x-sz-bearer-token`-Extension der `ICredentialService.SavePassword` mit korrektem Target aufgerufen wird, dass Negotiate-Auth korrekt gesetzt wird, und dass Pre-/Post-Request-Skripte (`x-sz-pre-request-script`, `x-sz-post-request-script`) in den gespeicherten Endpunkten vorhanden sind.

**SystemEntryInitializerTests** erweitern: Ein neuer Testfall prüft nach `InitializeAsync` und `SystemEndpointSyncService.ExecuteAsync`, dass Endpunkte für die Systemanwendung vorhanden sind, mindestens einer davon Autorisierung `Negotiate` besitzt, und dass der `ICredentialService` für Bearer-Token-Endpunkte aufgerufen wurde.

### 5. CLAUDE.md aktualisieren

Zwei neue Abschnitte:

- **Architekturprinzip**: Alle Datenentitäten (Anwendungsgruppen, Anwendungen, Endpunktgruppen, Endpunkte) werden ausschließlich über `IApplicationApiClient` abgerufen. Direktzugriffe auf Repository-Interfaces aus UI-Komponenten sind nicht erlaubt.
- **Testanforderungen für API-Clients**: Für jeden API-Client (und jeden neuen Controller) sind sowohl Unit-Tests mit gemocktem `HttpMessageHandler` als auch Integrationstests mit `WebApplicationFactory` bereitzustellen.

## Konfiguration

Keine neue Konfiguration erforderlich. Die neuen Controller-Routen werden automatisch über das bestehende ASP.NET-Core-Routing registriert. Die `ControllerTestFactory` deckt die neuen Endpunkte ohne Änderungen ab.

## Offene Fragen

1. **Direktzugriff in `SystemEndpointSyncService`**: Der Service schreibt beim Startup direkt über `IEndpointRepository`. Soll er zukünftig ebenfalls über `IApplicationApiClient` arbeiten (erfordert, dass die API zu diesem Zeitpunkt bereits läuft), oder bleibt der direkte DB-Zugriff für den Startup-Sync explizit erlaubt?

2. **`EndpointPage.razor` — Persistenz-Pfad**: Die Persistenz-Operationen (`AddEndpointAsync`, `UpdateEndpointAsync`, `AddHeaderAsync`, `DeleteHeaderAsync`, `AddQueryParameterAsync`, `DeleteQueryParameterAsync`) laufen derzeit über `IEndpointRepository`. Sollen Header- und QueryParameter-Endpunkte ebenfalls in der API exponiert und über `IApplicationApiClient` aufgerufen werden, oder werden diese atomaren Operationen zunächst ausgenommen?

3. **Storage-Mode für Endpoint-Abfragen**: Endpunkte und Endpunktgruppen sind über `ApplicationId` einer Anwendung zugeordnet. Soll der `X-Storage-Mode`-Header bei den neuen Controller-Endpunkten mitgeführt werden (analog zu Anwendungen), oder sind Endpunkte storage-mode-unabhängig?

4. **SignalR-Notifications**: Sollen die neuen Controller (`EndpointsController`, `EndpointGroupsController`) ebenfalls `ISignalRNotificationService`-Aufrufe auslösen, analog zu `ApplicationsController`?

5. **Testabdeckung für `ControllerTestFactory`**: Die `ControllerTestFactory` entfernt alle `IHostedService`-Registrierungen. Muss für den kombinierten Integrations-Testfall (SystemEntryInitializer + SystemEndpointSyncService) eine separate Factory erstellt werden, die den Sync-Service explizit ausführt?
