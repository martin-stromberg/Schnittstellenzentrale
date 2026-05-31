# Interfaces

## `IApplicationApiClient`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IApplicationApiClient.cs`

Aktuell enthält das Interface **ausschließlich** Methoden für `ApplicationGroup` und `Application`. Methoden für `EndpointGroup` und `Endpoint` fehlen vollständig.

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `GetGroupsAsync` | `StorageMode storageMode, string owner` | `Task<IList<ApplicationGroup>>` | Alle ApplicationGroups laden |
| `GetGroupByIdAsync` | `int id` | `Task<ApplicationGroup?>` | Eine ApplicationGroup per ID |
| `AddGroupAsync` | `ApplicationGroup group` | `Task<ApplicationGroup>` | ApplicationGroup anlegen |
| `UpdateGroupAsync` | `ApplicationGroup group` | `Task<ApplicationGroup>` | ApplicationGroup aktualisieren |
| `DeleteGroupAsync` | `int id` | `Task` | ApplicationGroup löschen |
| `GetUngroupedApplicationsAsync` | `StorageMode storageMode, string owner` | `Task<IList<Application>>` | Ungrouped Applications laden |
| `GetApplicationByIdAsync` | `int id` | `Task<Application?>` | Application per ID |
| `AddApplicationAsync` | `Application application` | `Task<Application>` | Application anlegen |
| `UpdateApplicationAsync` | `Application application` | `Task<Application>` | Application aktualisieren |
| `DeleteApplicationAsync` | `int id` | `Task` | Application löschen |

---

## `IEndpointRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IEndpointRepository.cs`

Vollständig definiert. Wird derzeit von `EndpointRepository` implementiert und von den Blazor-Komponenten sowie `SystemEndpointSyncService` verwendet.

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `GetEndpointsAsync` | `int applicationId` | `Task<IList<Endpoint>>` | Alle Endpunkte einer Anwendung |
| `GetEndpointsByApplicationIdsAsync` | `IEnumerable<int> applicationIds` | `Task<IList<Endpoint>>` | Endpunkte mehrerer Anwendungen |
| `GetEndpointByIdAsync` | `int id` | `Task<Endpoint?>` | Einzelner Endpunkt per ID |
| `GetEndpointByNameAsync` | `int applicationId, string name` | `Task<IList<Endpoint>>` | Endpunkte per Name |
| `AddEndpointAsync` | `Endpoint endpoint` | `Task<Endpoint>` | Endpunkt anlegen |
| `UpdateEndpointAsync` | `Endpoint endpoint` | `Task<Endpoint>` | Endpunkt aktualisieren |
| `DeleteEndpointAsync` | `int id` | `Task` | Endpunkt löschen |
| `GetEndpointGroupsAsync` | `int applicationId` | `Task<IList<EndpointGroup>>` | Alle Gruppen einer Anwendung |
| `GetEndpointGroupByIdAsync` | `int id` | `Task<EndpointGroup?>` | Gruppe per ID |
| `AddEndpointGroupAsync` | `EndpointGroup group` | `Task<EndpointGroup>` | Gruppe anlegen |
| `UpdateEndpointGroupAsync` | `EndpointGroup group` | `Task<EndpointGroup>` | Gruppe aktualisieren |
| `DeleteEndpointGroupAsync` | `int id` | `Task` | Gruppe löschen |
| `AddHeaderAsync` | `EndpointHeader header` | `Task<EndpointHeader>` | Einzelnen Header anlegen |
| `DeleteHeaderAsync` | `int id` | `Task` | Header löschen |
| `AddQueryParameterAsync` | `EndpointQueryParameter parameter` | `Task<EndpointQueryParameter>` | Query-Parameter anlegen |
| `DeleteQueryParameterAsync` | `int id` | `Task` | Query-Parameter löschen |
