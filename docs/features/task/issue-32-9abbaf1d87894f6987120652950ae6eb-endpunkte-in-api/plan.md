# Umsetzungsplan: Endpunkte in API

## Übersicht

`EndpointGroup`- und `Endpoint`-Ressourcen werden als REST-Endpunkte in der eigenen API bereitgestellt (`EndpointGroupsController`, `EndpointsController`). `IApplicationApiClient` wird um die entsprechenden CRUD-Methoden erweitert — einschließlich der atomaren Header- und QueryParameter-Operationen —, und alle Direktzugriffe auf `IEndpointRepository` aus den Blazor-Komponenten (`ApplicationGroupTree`, `FolderContentView`, `ApplicationContentView`, `EndpointPage`) werden vollständig auf den Client umgestellt. Ergänzt werden Unit- und Integrationstests für den `ApplicationApiClient`, erweiterte Tests für `SystemEndpointSyncService` und `SystemEntryInitializer` sowie zwei neue Abschnitte in `CLAUDE.md`.

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|-----------------|------------|
| `EndpointGroupsController` / `EndpointsController` | Gateway (analog zu `ApplicationGroupsController` / `ApplicationsController`) — erbt von `ApiControllerBase`, delegiert an `IEndpointRepository`, verwendet `[RequiresContextHeaders]` | Einheitliches Controller-Muster im Projekt; kein zusätzlicher Service-Layer nötig, da die Controller ausschließlich als HTTP-Gateway zum Repository fungieren |
| Response-DTOs (`EndpointGroupResponse`, `EndpointResponse`) | Value Object-DTOs (flach, ohne Navigationseigenschaften außer geschachtelten Child-Collections wo sinnvoll) | Bestehende Response-DTOs (`ApplicationGroupResponse`, `ApplicationResponse`) verwenden dasselbe Muster; keine Domänenlogik in DTOs |
| SignalR-Benachrichtigungen in neuen Controllern | Ja — analog zu `ApplicationsController` und `ApplicationGroupsController` | Konsistenz; UI-Komponenten (z. B. `ApplicationGroupTree`) reagieren bereits auf `EndpointChanged`/`EndpointGroupChanged`-Events via SignalR |
| `X-Storage-Mode`-Header für neue Controller | `[RequiresContextHeaders]` analog zu bestehenden Controllern | Endpunkte sind über `ApplicationId` einer Anwendung zugeordnet, die storage-mode-gebunden ist; Konsistenz und Vollständigkeit der Kontextinformationen sprechen für die Übernahme |
| `EndpointPage.razor` — Header/QueryParameter-Persistenz | Ebenfalls auf `IApplicationApiClient` umstellen (innerhalb dieser Anforderung) | Vollständige Beseitigung des `IEndpointRepository`-Direktzugriffs aus allen UI-Komponenten; vier neue atomare API-Routen werden in `EndpointsController` ergänzt |
| `SystemEndpointSyncService` — Direktzugriff auf `IEndpointRepository` | Bleibt erhalten (explizite Ausnahme) | Der Service läuft serverseitig beim Startup; ein HTTP-Roundtrip über `IApplicationApiClient` wäre zu diesem Zeitpunkt nicht zuverlässig und würde eine Kreisabhängigkeit auf den eigenen API-Stack erzeugen. Ausnahme wird in `CLAUDE.md` dokumentiert. |
| Kombinierter Integrationstestfall (SystemEntryInitializer + SystemEndpointSyncService) | Separate Hilfsmethode/-klasse in `SystemEntryInitializerTests` | `ControllerTestFactory` entfernt alle `IHostedService`-Registrierungen und ist daher nicht geeignet; `TestHelpers.CreateInMemoryDbContext` wird direkt verwendet, `SystemEndpointSyncService` manuell instanziiert |

## Programmabläufe

### Abruf von Endpunktgruppen und Endpunkten über die API

1. Blazor-Komponente ruft `IApplicationApiClient.GetEndpointGroupsAsync(applicationId)` auf.
2. `ApplicationApiClient` baut GET-Request via `BuildGetRequest` mit `Authorization`- und `X-Storage-Mode`-Header.
3. `SendWithTokenAsync` übermittelt den Request; bei 401 wird Token über `ExecuteWithTokenAsync` erneuert.
4. `EndpointGroupsController.GetAllAsync` nimmt den Request entgegen, ruft `ParseRequestContextAsync` auf und delegiert an `IEndpointRepository.GetEndpointGroupsAsync(applicationId)`.
5. Controller mappt jede `EndpointGroup` auf `EndpointGroupResponse` und gibt `200 OK` zurück.
6. `ApplicationApiClient` empfängt die Response, mappt `EndpointGroupResponse`-Liste auf Domäneobjekte (`EndpointGroup`) via `MapToEndpointGroup` und gibt sie zurück.
7. Blazor-Komponente nutzt die Domäneobjekte für die Darstellung.

Beteiligte Klassen/Komponenten: `IApplicationApiClient`, `ApplicationApiClient`, `EndpointGroupsController`, `ApiControllerBase`, `IEndpointRepository`, `EndpointRepository`, `EndpointGroupResponse`

### Abruf eines einzelnen Endpunkts per ID

1. Blazor-Komponente (z. B. `EndpointPage`) ruft `IApplicationApiClient.GetEndpointByIdAsync(id)` auf.
2. `ApplicationApiClient` baut GET-Request für `/api/endpoints/{id}`.
3. `SendWithTokenNullableAsync` übermittelt den Request; gibt `null` bei 404 zurück.
4. `EndpointsController.GetByIdAsync(id)` delegiert an `IEndpointRepository.GetEndpointByIdAsync(id)`.
5. Controller mappt `Endpoint` auf `EndpointResponse` (inkl. `Headers`, `QueryParameters`) und gibt `200 OK` oder `404` zurück.
6. `ApplicationApiClient` mappt `EndpointResponse` auf `Endpoint`-Domänenobjekt via `MapToEndpoint`.

Beteiligte Klassen/Komponenten: `IApplicationApiClient`, `ApplicationApiClient`, `EndpointsController`, `ApiControllerBase`, `IEndpointRepository`, `EndpointRepository`, `EndpointResponse`

### Anlegen und Aktualisieren eines Endpunkts

1. `EndpointPage` ruft `IApplicationApiClient.AddEndpointAsync(endpoint)` oder `UpdateEndpointAsync(endpoint)` auf.
2. `ApplicationApiClient` baut POST- bzw. PUT-Request via `BuildRequestWithBody` mit JSON-serialisiertem `CreateEndpointRequest` / `UpdateEndpointRequest`.
3. `EndpointsController.CreateAsync` / `UpdateAsync` empfängt den Request, legt Endpunkt über `IEndpointRepository` an bzw. aktualisiert ihn.
4. Controller mappt gespeicherten `Endpoint` auf `EndpointResponse`, gibt `201 Created` / `200 OK` zurück.
5. `ApplicationApiClient` mappt `EndpointResponse` auf Domänenobjekt und gibt es zurück; `RowVersion` ist im zurückgegebenen Objekt enthalten.
6. `EndpointPage` aktualisiert `_model.RowVersion` aus dem zurückgegebenen Objekt.
7. Controller löst `ISignalRNotificationService`-Benachrichtigung aus (bei Team-Mode).

Beteiligte Klassen/Komponenten: `IApplicationApiClient`, `ApplicationApiClient`, `EndpointsController`, `ApiControllerBase`, `IEndpointRepository`, `EndpointRepository`, `CreateEndpointRequest`, `UpdateEndpointRequest`, `EndpointResponse`, `ISignalRNotificationService`

### ForceSaveAsync in EndpointPage (RowVersion-Aktualisierung)

1. `EndpointPage.ForceSaveAsync` ruft `IApplicationApiClient.GetEndpointByIdAsync(_model.Id)` auf.
2. Erhält aktuelles `Endpoint`-Objekt mit frischem `RowVersion`.
3. Überschreibt `_model.RowVersion` mit dem gelesenen Wert.
4. Ruft anschließend `UpdateEndpointAsync` auf.

Beteiligte Klassen/Komponenten: `EndpointPage`, `IApplicationApiClient`, `ApplicationApiClient`, `EndpointsController`

### Anlegen und Löschen einzelner Header und QueryParameter

1. `EndpointPage` ruft `IApplicationApiClient.AddHeaderAsync(header)` oder `DeleteHeaderAsync(headerId)` auf (analog für QueryParameter).
2. `ApplicationApiClient` baut POST- bzw. DELETE-Request via `BuildRequestWithBody` / `BuildDeleteRequest` für `/api/endpoints/headers` bzw. `/api/endpoints/headers/{id}`.
3. `EndpointsController.AddHeaderAsync` / `DeleteHeaderAsync` delegiert an `IEndpointRepository.AddHeaderAsync` / `DeleteHeaderAsync`.
4. Controller gibt `201 Created` / `204 No Content` zurück.
5. `ApplicationApiClient` gibt `EndpointHeader` / `EndpointQueryParameter` zurück (bei Add) oder nichts (bei Delete).
6. `EndpointPage` aktualisiert die lokale `_model.Headers`- bzw. `_model.QueryParameters`-Collection.

Beteiligte Klassen/Komponenten: `IApplicationApiClient`, `ApplicationApiClient`, `EndpointsController`, `ApiControllerBase`, `IEndpointRepository`, `EndpointRepository`

### Initialisierung mit Endpunktprüfung (kombinierter Integrations-Ablauf)

1. `SystemEntryInitializer.InitializeAsync` legt Systemgruppe und Systemanwendung an (unverändert).
2. `SystemEndpointSyncService.ExecuteAsync` liest Swagger-Dokument via `ISwaggerProvider`.
3. `BuildImportedEndpoints` erzeugt Endpunktliste inkl. Autorisierungstypen, Pre-/Post-Skripten, Bearer-Token-Extension.
4. `ApplyDiffAsync` legt neue Endpunkte über `IEndpointRepository.AddEndpointAsync` an, löscht entfernte.
5. `SaveBearerTokenIfPresent` ruft `ICredentialService.SavePassword` für Endpunkte mit `x-sz-bearer-token`-Extension auf.

Beteiligte Klassen/Komponenten: `SystemEntryInitializer`, `SystemEndpointSyncService`, `ISwaggerProvider`, `IEndpointRepository`, `ICredentialService`

## Neue Klassen

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `EndpointGroupResponse` | Datenmodellklasse (DTO) | Response-Repräsentation einer `EndpointGroup` für die REST-API |
| `EndpointResponse` | Datenmodellklasse (DTO) | Response-Repräsentation eines `Endpoint` inkl. `Headers` und `QueryParameters` |
| `EndpointHeaderResponse` | Datenmodellklasse (DTO) | Response-Repräsentation eines `EndpointHeader` (für Add-Antwort und Einbettung in `EndpointResponse`) |
| `EndpointQueryParameterResponse` | Datenmodellklasse (DTO) | Response-Repräsentation eines `EndpointQueryParameter` (für Add-Antwort und Einbettung in `EndpointResponse`) |
| `CreateEndpointGroupRequest` | Datenmodellklasse (DTO) | Request-Body für POST `/api/endpoint-groups` |
| `UpdateEndpointGroupRequest` | Datenmodellklasse (DTO) | Request-Body für PUT `/api/endpoint-groups/{id}` |
| `CreateEndpointRequest` | Datenmodellklasse (DTO) | Request-Body für POST `/api/endpoints` |
| `UpdateEndpointRequest` | Datenmodellklasse (DTO) | Request-Body für PUT `/api/endpoints/{id}` |
| `AddEndpointHeaderRequest` | Datenmodellklasse (DTO) | Request-Body für POST `/api/endpoints/headers` |
| `AddEndpointQueryParameterRequest` | Datenmodellklasse (DTO) | Request-Body für POST `/api/endpoints/query-parameters` |
| `EndpointGroupsController` | Controller | REST-API-Controller für `/api/endpoint-groups` (CRUD) |
| `EndpointsController` | Controller | REST-API-Controller für `/api/endpoints` (CRUD + Header/QueryParameter-Operationen) |
| `ApplicationApiClientIntegrationTests` | Testklasse | Integrationstests für `ApplicationApiClient` gegen `WebApplicationFactory` |

## Änderungen an bestehenden Klassen

### `IApplicationApiClient` (Interface)

- **Neue Methoden:**
  - `GetEndpointGroupsAsync(int applicationId)` — Alle Endpunktgruppen einer Anwendung; Rückgabe `Task<IList<EndpointGroup>>`
  - `GetEndpointGroupByIdAsync(int id)` — Eine Endpunktgruppe per ID; Rückgabe `Task<EndpointGroup?>`
  - `AddEndpointGroupAsync(EndpointGroup group)` — Endpunktgruppe anlegen; Rückgabe `Task<EndpointGroup>`
  - `UpdateEndpointGroupAsync(EndpointGroup group)` — Endpunktgruppe aktualisieren; Rückgabe `Task<EndpointGroup>`
  - `DeleteEndpointGroupAsync(int id)` — Endpunktgruppe löschen; Rückgabe `Task`
  - `GetEndpointsAsync(int applicationId)` — Alle Endpunkte einer Anwendung; Rückgabe `Task<IList<Endpoint>>`
  - `GetEndpointByIdAsync(int id)` — Einzelner Endpunkt per ID; Rückgabe `Task<Endpoint?>`
  - `AddEndpointAsync(Endpoint endpoint)` — Endpunkt anlegen; Rückgabe `Task<Endpoint>`
  - `UpdateEndpointAsync(Endpoint endpoint)` — Endpunkt aktualisieren; Rückgabe `Task<Endpoint>`
  - `DeleteEndpointAsync(int id)` — Endpunkt löschen; Rückgabe `Task`
  - `AddHeaderAsync(EndpointHeader header)` — Einzelnen Header anlegen; Rückgabe `Task<EndpointHeader>`
  - `DeleteHeaderAsync(int id)` — Header löschen; Rückgabe `Task`
  - `AddQueryParameterAsync(EndpointQueryParameter parameter)` — Query-Parameter anlegen; Rückgabe `Task<EndpointQueryParameter>`
  - `DeleteQueryParameterAsync(int id)` — Query-Parameter löschen; Rückgabe `Task`

### `ApplicationApiClient` (Klasse)

- **Neue Methoden:**
  - `GetEndpointGroupsAsync(int applicationId)` — GET `/api/endpoint-groups?applicationId={id}` via `BuildGetRequest` + `SendWithTokenAsync`; mappt Response-Liste via `MapToEndpointGroup`
  - `GetEndpointGroupByIdAsync(int id)` — GET `/api/endpoint-groups/{id}` via `SendWithTokenNullableAsync`; gibt `null` bei 404
  - `AddEndpointGroupAsync(EndpointGroup group)` — POST `/api/endpoint-groups` via `BuildRequestWithBody` + `SendWithTokenAsync`; mappt Response via `MapToEndpointGroup`
  - `UpdateEndpointGroupAsync(EndpointGroup group)` — PUT `/api/endpoint-groups/{id}` via `BuildRequestWithBody` + `SendWithTokenAsync`; mappt Response
  - `DeleteEndpointGroupAsync(int id)` — DELETE `/api/endpoint-groups/{id}` via `BuildDeleteRequest` + `SendWithTokenNoContentAsync`
  - `GetEndpointsAsync(int applicationId)` — GET `/api/endpoints?applicationId={id}`; mappt Response-Liste via `MapToEndpoint`
  - `GetEndpointByIdAsync(int id)` — GET `/api/endpoints/{id}`; gibt `null` bei 404
  - `AddEndpointAsync(Endpoint endpoint)` — POST `/api/endpoints`; mappt Response via `MapToEndpoint`
  - `UpdateEndpointAsync(Endpoint endpoint)` — PUT `/api/endpoints/{id}`; mappt Response
  - `DeleteEndpointAsync(int id)` — DELETE `/api/endpoints/{id}`
  - `AddHeaderAsync(EndpointHeader header)` — POST `/api/endpoints/headers`; mappt `EndpointHeaderResponse` auf `EndpointHeader`
  - `DeleteHeaderAsync(int id)` — DELETE `/api/endpoints/headers/{id}`
  - `AddQueryParameterAsync(EndpointQueryParameter parameter)` — POST `/api/endpoints/query-parameters`; mappt `EndpointQueryParameterResponse` auf `EndpointQueryParameter`
  - `DeleteQueryParameterAsync(int id)` — DELETE `/api/endpoints/query-parameters/{id}`
  - `MapToEndpointGroup(EndpointGroupResponse)` (private static) — Mappt DTO auf `EndpointGroup`-Domänenobjekt
  - `MapToEndpoint(EndpointResponse)` (private static) — Mappt DTO auf `Endpoint`-Domänenobjekt inkl. `Headers` und `QueryParameters`

### `ApiControllerBase` (abstrakte Klasse)

- **Neue Methoden:**
  - `MapToResponse(EndpointGroup)` (protected static) — Mappt `EndpointGroup` auf `EndpointGroupResponse`
  - `MapToResponse(Endpoint)` (protected static) — Mappt `Endpoint` auf `EndpointResponse` inkl. `Headers` und `QueryParameters`

### `ApplicationGroupTree.razor` (Blazor-Komponente)

- **Geänderte Methoden:** `ReloadApplicationDataAsync` — ersetzt `IEndpointRepository.GetEndpointGroupsAsync` und `GetEndpointsAsync` durch `IApplicationApiClient.GetEndpointGroupsAsync` und `GetEndpointsAsync`
- Die `IEndpointRepository`-Injektion entfällt vollständig.

### `FolderContentView.razor` (Blazor-Komponente)

- **Geänderte Methoden:** `OnParametersSetAsync` — ersetzt `IEndpointRepository.GetEndpointsAsync` durch `IApplicationApiClient.GetEndpointsAsync`; clientseitige Filterung nach `EndpointGroupId` bleibt erhalten
- Die `IEndpointRepository`-Injektion entfällt vollständig; stattdessen wird `IApplicationApiClient` injiziert.

### `ApplicationContentView.razor` (Blazor-Komponente)

- **Geänderte Methoden:** `OnParametersSetAsync` — ersetzt `IEndpointRepository.GetEndpointsAsync` durch `IApplicationApiClient.GetEndpointsAsync` für die Zählung `_endpointCount`
- Die `IEndpointRepository`-Injektion entfällt; `IApplicationApiClient` ist bereits injiziert.

### `EndpointPage.razor` (Blazor-Komponente)

- **Geänderte Methoden:**
  - `ForceSaveAsync` — ersetzt `IEndpointRepository.GetEndpointByIdAsync` durch `IApplicationApiClient.GetEndpointByIdAsync` zum Lesen des aktuellen `RowVersion`-Werts
  - `PersistAsync` — ersetzt `IEndpointRepository.AddEndpointAsync` / `UpdateEndpointAsync` durch `IApplicationApiClient.AddEndpointAsync` / `UpdateEndpointAsync`; `RowVersion` wird aus der API-Response übernommen
  - `SendRequestAsync` — ersetzt `IEndpointRepository.GetEndpointByIdAsync` durch `IApplicationApiClient.GetEndpointByIdAsync` für den Reload vor Ausführung
  - `AddHeaderAsync` — ersetzt `IEndpointRepository.AddHeaderAsync` durch `IApplicationApiClient.AddHeaderAsync`
  - `DeleteHeaderAsync` — ersetzt `IEndpointRepository.DeleteHeaderAsync` durch `IApplicationApiClient.DeleteHeaderAsync`
  - `AddQueryParameterAsync` — ersetzt `IEndpointRepository.AddQueryParameterAsync` durch `IApplicationApiClient.AddQueryParameterAsync`
  - `DeleteQueryParameterAsync` — ersetzt `IEndpointRepository.DeleteQueryParameterAsync` durch `IApplicationApiClient.DeleteQueryParameterAsync`
- Die `IEndpointRepository`-Injektion entfällt vollständig; stattdessen wird `IApplicationApiClient` injiziert.

### `ApplicationApiClientTests` (Testklasse)

- **Neue Testmethoden:** Für jede der 14 neuen Methoden in `IApplicationApiClient` wird ein Testfall nach bestehendem Muster ergänzt (s. Abschnitt Tests).

### `SystemEndpointSyncServiceTests` (Testklasse)

- **Neue Testmethoden:** Zwei neue Testfälle für Pre-/Post-Request-Skript-Verarbeitung (s. Abschnitt Tests).

### `SystemEntryInitializerTests` (Testklasse)

- **Neue Testmethoden:** Ein neuer kombinierter Integrationstestfall mit separater Hilfsmethode/-klasse (s. Abschnitt Tests).

### `CLAUDE.md` (Dokumentation)

- **Neuer Abschnitt** „Architekturprinzip: API-First": Alle Datenentitäten (Anwendungsgruppen, Anwendungen, Endpunktgruppen, Endpunkte) werden aus UI-Komponenten ausschließlich über `IApplicationApiClient` abgerufen. Direktzugriffe auf Repository-Interfaces aus Blazor-Komponenten sind nicht erlaubt. Explizite Ausnahme: `SystemEndpointSyncService` verwendet `IEndpointRepository` direkt beim Startup-Abgleich, da zu diesem Zeitpunkt kein zuverlässiger HTTP-Roundtrip möglich ist.
- **Neuer Abschnitt** „Testanforderungen für API-Clients": Für jeden API-Client und jeden neuen Controller sind sowohl Unit-Tests mit gemocktem `HttpMessageHandler` als auch Integrationstests mit `WebApplicationFactory` bereitzustellen.

## Datenbankmigrationen

Keine. Es werden ausschließlich neue API-Schichten und Client-Code ergänzt; das Datenbankschema bleibt unverändert.

## Validierungsregeln

| Feld / Objekt | Regel | Fehlerfall |
|---------------|-------|------------|
| `CreateEndpointRequest.Name` | Pflichtfeld, nicht leer | `400 Bad Request` |
| `CreateEndpointRequest.RelativePath` | Pflichtfeld, nicht leer | `400 Bad Request` |
| `CreateEndpointRequest.ApplicationId` | Pflichtfeld, > 0 | `400 Bad Request` |
| `UpdateEndpointRequest.Name` | Pflichtfeld, nicht leer | `400 Bad Request` |
| `UpdateEndpointRequest.RelativePath` | Pflichtfeld, nicht leer | `400 Bad Request` |
| `UpdateEndpointRequest.RowVersion` | Pflichtfeld (Concurrency-Token) | `400 Bad Request` / `409 Conflict` bei Mismatch |
| `CreateEndpointGroupRequest.Name` | Pflichtfeld, nicht leer | `400 Bad Request` |
| `CreateEndpointGroupRequest.ApplicationId` | Pflichtfeld, > 0 | `400 Bad Request` |
| `UpdateEndpointGroupRequest.Name` | Pflichtfeld, nicht leer | `400 Bad Request` |
| `UpdateEndpointGroupRequest.RowVersion` | Pflichtfeld (Concurrency-Token) | `400 Bad Request` / `409 Conflict` bei Mismatch |
| `AddEndpointHeaderRequest.Key` | Pflichtfeld, nicht leer | `400 Bad Request` |
| `AddEndpointHeaderRequest.EndpointId` | Pflichtfeld, > 0 | `400 Bad Request` |
| `AddEndpointQueryParameterRequest.Key` | Pflichtfeld, nicht leer | `400 Bad Request` |
| `AddEndpointQueryParameterRequest.EndpointId` | Pflichtfeld, > 0 | `400 Bad Request` |

## Konfigurationsänderungen

Keine. Die neuen Controller-Routen werden automatisch über das bestehende ASP.NET-Core-Routing registriert. Die `ControllerTestFactory` deckt die neuen Endpunkte ohne Änderungen ab.

## Seiteneffekte und Risiken

- **`ApplicationGroupTree.razor`**: Die SignalR-Handler `EndpointChanged` und `EndpointGroupChanged` rufen bereits `ReloadApplicationDataAsync` auf. Nach der Umstellung läuft dieser Reload über die API; dadurch entsteht bei jedem SignalR-Event ein HTTP-Roundtrip. Das ist architektonisch korrekt, aber messbar langsamer als der direkte DB-Zugriff.
- **`FolderContentView.razor`**: Die clientseitige Filterung (`e.EndpointGroupId == EndpointGroup.Id`) lädt weiterhin alle Endpunkte der Anwendung und filtert dann im Client. Das ist identisch zum heutigen Verhalten und damit kein neues Risiko, aber es sollte bekannt sein.
- **`SystemEndpointSyncService`**: Bleibt als dokumentierte Ausnahme auf `IEndpointRepository`. Zukünftige Änderungen an diesem Service müssen explizit berücksichtigen, dass er nicht den API-Client-Pfad verwendet.
- **Compile-Fehler beim Entfernen von `IEndpointRepository`-Injektionen**: Sobald die Injektionen aus den Komponenten entfernt werden, schlagen alle Build-Schritte bis zur Implementierung fehl. Die Umstellung sollte je Komponente atomar erfolgen (Injektion entfernen und Aufrufe ersetzen in einem Schritt).

## Umsetzungsreihenfolge

1. Response-DTOs anlegen: `EndpointGroupResponse`, `EndpointResponse` (mit `Headers`- und `QueryParameters`-Listen), `EndpointHeaderResponse`, `EndpointQueryParameterResponse` in `Schnittstellenzentrale.Core/Contracts/`
2. Request-DTOs anlegen: `CreateEndpointGroupRequest`, `UpdateEndpointGroupRequest`, `CreateEndpointRequest`, `UpdateEndpointRequest`, `AddEndpointHeaderRequest`, `AddEndpointQueryParameterRequest` in `Schnittstellenzentrale.Core/Contracts/`
3. Mapping-Methoden in `ApiControllerBase` ergänzen: `MapToResponse(EndpointGroup)`, `MapToResponse(Endpoint)`
4. `EndpointGroupsController` erstellen (CRUD, SignalR, `[RequiresContextHeaders]`)
5. `EndpointsController` erstellen (CRUD + Header/QueryParameter-Routen, SignalR, `[RequiresContextHeaders]`)
6. `IApplicationApiClient` um die 14 neuen Methoden erweitern
7. `ApplicationApiClient` implementieren: alle 14 Methoden inkl. `MapToEndpointGroup` und `MapToEndpoint`
8. `ApplicationGroupTree.razor` umstellen: `IEndpointRepository`-Injektion entfernen, `ReloadApplicationDataAsync` auf `IApplicationApiClient` umstellen
9. `FolderContentView.razor` umstellen: `IEndpointRepository`-Injektion durch `IApplicationApiClient` ersetzen, `OnParametersSetAsync` anpassen
10. `ApplicationContentView.razor` umstellen: `IEndpointRepository`-Aufruf in `OnParametersSetAsync` durch `IApplicationApiClient` ersetzen
11. `EndpointPage.razor` umstellen: `IEndpointRepository`-Injektion durch `IApplicationApiClient` ersetzen, alle betroffenen Methoden anpassen (`ForceSaveAsync`, `PersistAsync`, `SendRequestAsync`, `AddHeaderAsync`, `DeleteHeaderAsync`, `AddQueryParameterAsync`, `DeleteQueryParameterAsync`)
12. Unit-Tests in `ApplicationApiClientTests` für alle 14 neuen Methoden ergänzen
13. `ApplicationApiClientIntegrationTests` erstellen (Happy Paths + 401/404 für beide neuen Controller inkl. Header/QueryParameter-Routen)
14. `SystemEndpointSyncServiceTests` erweitern: Testfälle für Pre-/Post-Request-Skripte
15. `SystemEntryInitializerTests` erweitern: kombinierter Testfall mit separater Hilfsmethode/-klasse und `SystemEndpointSyncService`
16. `CLAUDE.md` aktualisieren: Abschnitt „Architekturprinzip: API-First" (inkl. Ausnahme für `SystemEndpointSyncService`) und „Testanforderungen für API-Clients"

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `GetEndpointGroupsAsync_SendsCorrectRequestAndReturnsMappedList` | `ApplicationApiClientTests` | GET `/api/endpoint-groups?applicationId={id}`, korrekte Header, Response-Mapping |
| `GetEndpointGroupByIdAsync_ReturnsNullOn404` | `ApplicationApiClientTests` | `null`-Rückgabe bei 404-Response |
| `AddEndpointGroupAsync_IssuesTokenAndSendsCorrectRequest_ReturnsResponse` | `ApplicationApiClientTests` | POST `/api/endpoint-groups`, Token-Ausstellung, Mapping |
| `UpdateEndpointGroupAsync_SendsCorrectPutRequestAndReturnsMappedGroup` | `ApplicationApiClientTests` | PUT `/api/endpoint-groups/{id}`, alle Felder |
| `DeleteEndpointGroupAsync_SendsCorrectDeleteRequest` | `ApplicationApiClientTests` | DELETE `/api/endpoint-groups/{id}` |
| `GetEndpointsAsync_SendsCorrectRequestAndReturnsMappedList` | `ApplicationApiClientTests` | GET `/api/endpoints?applicationId={id}`, Response-Mapping inkl. Headers/QueryParameters |
| `GetEndpointByIdAsync_ReturnsNullOn404` | `ApplicationApiClientTests` | `null`-Rückgabe bei 404-Response |
| `AddEndpointAsync_IssuesTokenAndSendsCorrectRequest_ReturnsResponse` | `ApplicationApiClientTests` | POST `/api/endpoints`, alle Felder inkl. `AuthenticationType`, `RowVersion` |
| `UpdateEndpointAsync_SendsCorrectPutRequestAndReturnsMappedEndpoint` | `ApplicationApiClientTests` | PUT `/api/endpoints/{id}`, alle Felder |
| `DeleteEndpointAsync_SendsCorrectDeleteRequest` | `ApplicationApiClientTests` | DELETE `/api/endpoints/{id}` |
| `AddHeaderAsync_SendsCorrectPostRequestAndReturnsMappedHeader` | `ApplicationApiClientTests` | POST `/api/endpoints/headers`, Mapping `EndpointHeaderResponse` auf `EndpointHeader` |
| `DeleteHeaderAsync_SendsCorrectDeleteRequest` | `ApplicationApiClientTests` | DELETE `/api/endpoints/headers/{id}` |
| `AddQueryParameterAsync_SendsCorrectPostRequestAndReturnsMappedParameter` | `ApplicationApiClientTests` | POST `/api/endpoints/query-parameters`, Mapping `EndpointQueryParameterResponse` auf `EndpointQueryParameter` |
| `DeleteQueryParameterAsync_SendsCorrectDeleteRequest` | `ApplicationApiClientTests` | DELETE `/api/endpoints/query-parameters/{id}` |
| CRUD-Tests für `/api/endpoint-groups` (Happy Path + 401 + 404) | `ApplicationApiClientIntegrationTests` | Vollständiges CRUD gegen `WebApplicationFactory`; korrekter HTTP-Statuscode und Response-Body |
| CRUD-Tests für `/api/endpoints` (Happy Path + 401 + 404) | `ApplicationApiClientIntegrationTests` | Vollständiges CRUD gegen `WebApplicationFactory`; korrekter HTTP-Statuscode und Response-Body |
| Header/QueryParameter-Tests für `/api/endpoints/headers` und `/api/endpoints/query-parameters` | `ApplicationApiClientIntegrationTests` | Add + Delete gegen `WebApplicationFactory`; korrekter Statuscode und Response-Body |
| `ExecuteAsync_WithPreRequestScript_SetsPreRequestScriptOnEndpoint` | `SystemEndpointSyncServiceTests` | `x-sz-pre-request-script`-Extension wird im angelegten Endpunkt als `PreRequestScript` gespeichert |
| `ExecuteAsync_WithPostRequestScript_SetsPostRequestScriptOnEndpoint` | `SystemEndpointSyncServiceTests` | `x-sz-post-request-script`-Extension wird im angelegten Endpunkt als `PostRequestScript` gespeichert |
| Hilfsmethode `CreateSyncServiceWithInMemoryDb` | `SystemEntryInitializerTests` | Erstellt In-Memory-Datenbank, instanziiert `SystemEndpointSyncService` mit gemocktem `ISwaggerProvider` und `ICredentialService`; stellt kombinierten Ablauf bereit |
| `InitializeAsync_AndSyncService_CreatesEndpointsWithCorrectAuthorizationAndCredentials` | `SystemEntryInitializerTests` | Nach `InitializeAsync` + `SystemEndpointSyncService.ExecuteAsync`: Endpunkte vorhanden, mindestens einer mit `Negotiate`-Auth, `ICredentialService.SavePassword` für Bearer-Token-Endpunkte aufgerufen |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| Alle Tests in `SystemEntryInitializerTests` | Mit Einführung der Hilfsmethode `CreateSyncServiceWithInMemoryDb` können gemeinsame Setup-Teile in Hilfsmethoden ausgelagert werden — keine inhaltliche Änderung an bestehenden Tests erwartet |

## Offene Punkte

Keine.
