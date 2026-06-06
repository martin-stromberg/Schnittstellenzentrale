# Interfaces

## `IApplicationRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IApplicationRepository.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `GetGroupsAsync` | `StorageMode, string owner` | `Task<IList<ApplicationGroup>>` | Alle Gruppen nach Modus und Besitzer |
| `GetGroupByIdAsync` | `int id` | `Task<ApplicationGroup?>` | Einzelne Gruppe per ID |
| `GetSystemGroupAsync` | — | `Task<ApplicationGroup?>` | Systemgruppe abrufen |
| `AddGroupAsync` | `ApplicationGroup` | `Task<ApplicationGroup>` | Gruppe anlegen |
| `UpdateGroupAsync` | `ApplicationGroup` | `Task<ApplicationGroup>` | Gruppe aktualisieren |
| `DeleteGroupAsync` | `int id` | `Task` | Gruppe löschen |
| `GetApplicationsAsync` | `StorageMode, string owner` | `Task<IList<Application>>` | Alle Anwendungen nach Modus und Besitzer |
| `GetUngroupedApplicationsAsync` | `StorageMode, string owner` | `Task<IList<Application>>` | Ungrupierte Anwendungen |
| `GetApplicationByIdAsync` | `int id` | `Task<Application?>` | Einzelne Anwendung per ID |
| `AddApplicationAsync` | `Application` | `Task<Application>` | Anwendung anlegen |
| `UpdateApplicationAsync` | `Application` | `Task<Application>` | Anwendung aktualisieren |
| `DeleteApplicationAsync` | `int id` | `Task` | Anwendung löschen |
| `GetApplicationCountByGroupAsync` | `int groupId` | `Task<int>` | Anzahl Anwendungen in einer Gruppe |
| `GetEndpointCountByGroupAsync` | `int groupId` | `Task<int>` | Anzahl Endpunkte in einer Gruppe |

Wird direkt von `ApplicationsController`, `ApplicationGroupsController` und `SystemEndpointSyncService` verwendet. Für die neuen OData-Controller vorgesehen.

---

## `IEndpointRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IEndpointRepository.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `GetEndpointsAsync` | `int applicationId` | `Task<IList<Endpoint>>` | Alle Endpunkte einer Anwendung |
| `GetEndpointsByApplicationIdsAsync` | `IEnumerable<int> applicationIds` | `Task<IList<Endpoint>>` | Endpunkte mehrerer Anwendungen |
| `GetByGroupIdAsync` | `int endpointGroupId` | `Task<IList<Endpoint>>` | Endpunkte einer Gruppe |
| `GetEndpointByIdAsync` | `int id` | `Task<Endpoint?>` | Einzelner Endpunkt per ID |
| `GetEndpointByNameAsync` | `int applicationId, string name` | `Task<IList<Endpoint>>` | Endpunkte nach Name |
| `AddEndpointAsync` | `Endpoint` | `Task<Endpoint>` | Endpunkt anlegen |
| `UpdateEndpointAsync` | `Endpoint` | `Task<Endpoint>` | Endpunkt aktualisieren |
| `DeleteEndpointAsync` | `int id` | `Task` | Endpunkt löschen |
| `GetEndpointGroupsAsync` | `int applicationId` | `Task<IList<EndpointGroup>>` | Alle Gruppen einer Anwendung |
| `GetEndpointGroupByIdAsync` | `int id` | `Task<EndpointGroup?>` | Einzelne Gruppe per ID |
| `AddEndpointGroupAsync` | `EndpointGroup` | `Task<EndpointGroup>` | Gruppe anlegen |
| `UpdateEndpointGroupAsync` | `EndpointGroup` | `Task<EndpointGroup>` | Gruppe aktualisieren |
| `DeleteEndpointGroupAsync` | `int id` | `Task` | Gruppe löschen |
| `GetHeaderByIdAsync` | `int id` | `Task<EndpointHeader?>` | Header per ID |
| `AddHeaderAsync` | `EndpointHeader` | `Task<EndpointHeader>` | Header hinzufügen |
| `DeleteHeaderAsync` | `int id` | `Task` | Header löschen |
| `GetQueryParameterByIdAsync` | `int id` | `Task<EndpointQueryParameter?>` | Query-Parameter per ID |
| `AddQueryParameterAsync` | `EndpointQueryParameter` | `Task<EndpointQueryParameter>` | Query-Parameter hinzufügen |
| `DeleteQueryParameterAsync` | `int id` | `Task` | Query-Parameter löschen |

Wird direkt von `EndpointsController`, `EndpointGroupsController`, `ODataImportService` und `SystemEndpointSyncService` verwendet. Für die neuen OData-Controller vorgesehen.

---

## `IODataImportService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IODataImportService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `ImportAsync` | `Application application` | `Task<ImportDiff>` | Metadaten abrufen und Diff zu bestehenden Endpunkten berechnen |
| `ApplyDiffAsync` | `ImportDiff diff` | `Task` | Diff in die Datenbank schreiben |

Implementiert von `ODataImportService` in `Schnittstellenzentrale.Infrastructure`.

---

## `ITokenStore`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ITokenStore.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `CreateTokenAsync` | `string username` | `Task<AuthToken>` | Neues Bearer-Token ausstellen |
| `ValidateAndRotateAsync` | `string tokenString` | `Task<AuthToken?>` | Token validieren und rotieren; `null` wenn ungültig |

Wird von `ApiControllerBase` für die Token-Validierung aller REST-Controller verwendet.
