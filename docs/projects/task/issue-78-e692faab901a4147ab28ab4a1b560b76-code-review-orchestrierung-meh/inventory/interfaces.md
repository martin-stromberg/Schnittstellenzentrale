# Interfaces

## `ISwaggerImportService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ISwaggerImportService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `ImportAsync` | `Application application` | `Task<ImportDiff>` | HTTP-Fetch + Parse + Diff-Berechnung (Z. 11) |
| `ApplyDiffAsync` | `ImportDiff diff` | `Task` | Wendet Diff an (Z. 15) |

`SyncFromDocumentAsync(OpenApiDocument, int)` ist **nicht** vorhanden.

## `IApplicationApiClient`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IApplicationApiClient.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `GetGroupsAsync` | `StorageMode, string owner` | `Task<IList<ApplicationGroup>>` | Gruppenliste (Z. 13) |
| `GetGroupByIdAsync` | `int id` | `Task<ApplicationGroup?>` | Einzelgruppe (Z. 18) |
| `AddGroupAsync` | `ApplicationGroup` | `Task<ApplicationGroup>` | Gruppe anlegen (Z. 23) |
| `UpdateGroupAsync` | `ApplicationGroup` | `Task<ApplicationGroup>` | Gruppe aktualisieren (Z. 28) |
| `DeleteGroupAsync` | `int id` | `Task` | Gruppe löschen — **kein** `content`-Parameter (Z. 32) |
| `GetUngroupedApplicationsAsync` | `StorageMode, string owner` | `Task<IList<Application>>` | Ungruppierte Apps (Z. 38) |
| `GetApplicationByIdAsync` | `int id` | `Task<Application?>` | Einzel-App (Z. 43) |
| `AddApplicationAsync` / `UpdateApplicationAsync` / `DeleteApplicationAsync` | s. Datei | s. Datei | App-CRUD (Z. 48–57) |
| `GetEndpointGroupsAsync` | `int applicationId` | `Task<IList<EndpointGroup>>` | Endpunktgruppen (Z. 62) |
| `GetEndpointGroupByIdAsync` | `int id` | `Task<EndpointGroup?>` | (Z. 67) |
| `AddEndpointGroupAsync` / `UpdateEndpointGroupAsync` / `DeleteEndpointGroupAsync` | s. Datei | s. Datei | Endpunktgruppen-CRUD (Z. 72–81) |
| `GetEndpointsAsync` | `int applicationId, int? endpointGroupId` | `Task<IList<Endpoint>>` | Endpunktliste (Z. 87) |
| `GetEndpointByIdAsync` | `int id` | `Task<Endpoint?>` | (Z. 92) |
| `AddEndpointAsync` / `UpdateEndpointAsync` / `DeleteEndpointAsync` | s. Datei | s. Datei | Endpunkt-CRUD (Z. 97–106) |
| `AddHeaderAsync` / `DeleteHeaderAsync` | `EndpointHeader` / `int id` | `Task<EndpointHeader>` / `Task` | Header-Einzelpflege (Z. 111–115) |
| `AddQueryParameterAsync` / `DeleteQueryParameterAsync` | analog | analog | QueryParam-Einzelpflege (Z. 120–124) |
| `GetEnvironmentByIdAsync` | `int id` | `Task<SystemEnvironment?>` | (Z. 129) |
| `ImportMetadataAsync` | `int applicationId` | `Task<ImportDiff>` | `POST /api/applications/{id}/import` (Z. 134) |
| `ApplyODataDiffAsync` | `int applicationId, ImportDiff diff` | `Task` | `POST …/odata-import/apply` (Z. 139) |

`ApplySwaggerDiffAsync(int, ImportDiff)` ist **nicht** vorhanden.

## `IEndpointRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IEndpointRepository.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `GetEndpointsAsync` | `int applicationId` | `Task<IList<Endpoint>>` | inkl. Headers/QueryParameters/EndpointGroup |
| `GetAllEndpointsAsync` | — | `Task<IList<Endpoint>>` | inkl. Application+Group |
| `GetEndpointsByApplicationIdsAsync` | `IEnumerable<int>` | `Task<IList<Endpoint>>` | Multi-App-Abfrage (OData-Get, CollectionContentView) |
| `GetByGroupIdAsync` | `int endpointGroupId` | `Task<IList<Endpoint>>` | |
| `GetEndpointByIdAsync` | `int id` | `Task<Endpoint?>` | inkl. Application+ApplicationGroup |
| `GetEndpointByNameAsync` | `int applicationId, string name` | `Task<IList<Endpoint>>` | |
| `AddEndpointAsync` | `Endpoint` | `Task<Endpoint>` | |
| `UpdateEndpointAsync` | `Endpoint` | `Task<Endpoint>` | **ersetzt bereits Skalare + Kinder** (Impl. Z. 104–130) |
| `DeleteEndpointAsync` | `int id` | `Task` | |
| `GetEndpointGroupsAsync` / `GetAllEndpointGroupsAsync` / `GetEndpointGroupByIdAsync` | s. Datei | s. Datei | Gruppen-Reads |
| `AddEndpointGroupAsync` / `UpdateEndpointGroupAsync` / `DeleteEndpointGroupAsync` | s. Datei | s. Datei | Gruppen-CRUD |
| `GetHeaderByIdAsync` / `AddHeaderAsync` / `DeleteHeaderAsync` | s. Datei | s. Datei | Header-Einzelpflege |
| `GetQueryParameterByIdAsync` / `AddQueryParameterAsync` / `DeleteQueryParameterAsync` | s. Datei | s. Datei | QueryParam-Einzelpflege |

`UpdateAggregateAsync(Endpoint)` ist **nicht** vorhanden.

## `IApplicationRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IApplicationRepository.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `GetGroupsAsync` | `StorageMode, string owner` | `Task<IList<ApplicationGroup>>` | inkl. `Applications`; User-Modus filtert auf Gruppen mit eigenen Apps |
| `GetGroupByIdAsync` | `int id` | `Task<ApplicationGroup?>` | inkl. `Applications` |
| `GetSystemGroupAsync` | — | `Task<ApplicationGroup?>` | `IsSystem`-Gruppe inkl. Apps (SystemEndpointSyncService) |
| `AddGroupAsync` / `UpdateGroupAsync` | `ApplicationGroup` | `Task<ApplicationGroup>` | |
| `DeleteGroupAsync` | `int id` | `Task` | Nur Gruppen-Entity; `ApplicationGroupId` der Apps → `null` via FK-`SetNull` |
| `GetApplicationsAsync` / `GetUngroupedApplicationsAsync` / `GetApplicationByIdAsync` | s. Datei | s. Datei | App-Reads (Detail inkl. Endpoints+Children+EndpointGroups) |
| `AddApplicationAsync` / `UpdateApplicationAsync` / `DeleteApplicationAsync` | s. Datei | s. Datei | App-CRUD |
| `GetApplicationCountByGroupAsync` / `GetEndpointCountByGroupAsync` | `int groupId` | `Task<int>` | Zähler für `CollectionContentView` |

Eine Methode für atomares Gruppen-Löschen mit Inhalt (`DeleteGroupWithContentAsync` o. ä.) ist **nicht** vorhanden.

## `ISignalRNotificationService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ISignalRNotificationService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `NotifyTreeChangedAsync` | — | `Task` | Gruppe `workspace` → `TreeChanged` |
| `NotifyApplicationChangedAsync` | `int applicationId` | `Task` | `application:{id}` → `ApplicationChanged` |
| `NotifyGroupChangedAsync` | `int groupId` | `Task` | `group:{id}` → `GroupChanged` |
| `NotifyEndpointChangedAsync` | `int endpointId, int applicationId` | `Task` | `application:{id}` → `EndpointChanged` |
| `NotifyEndpointGroupChangedAsync` | `int endpointGroupId, int applicationId` | `Task` | `application:{id}` → `EndpointGroupChanged` |
| `NotifyEnvironmentChangedAsync` | — | `Task` | `environments` → `EnvironmentChanged` |

Kein eingebauter StorageMode-Check — die `Team`-Bedingung liegt ausschließlich in den Aufrufern.

## `IODataImportService` (Referenz-Struktur)
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IODataImportService.cs`

Identisches Shape zu `ISwaggerImportService`: `ImportAsync(Application)` + `ApplyDiffAsync(ImportDiff)`.

## `IStorageModeService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IStorageModeService.cs`

`CurrentMode` (`StorageMode`), `OnModeChanged` (event), `SetMode(StorageMode)`, `InitializeAsync()` — Quelle der `Team`-Prüfung in UI-Komponenten.
