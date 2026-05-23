# Interfaces

## `IApplicationRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IApplicationRepository.cs`

Bleibt unverändert — kein Bestandteil des Refactorings.

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `GetGroupsAsync` | `StorageMode storageMode, string owner` | `Task<IList<ApplicationGroup>>` | Alle Gruppen laden, ggf. gefiltert nach Owner |
| `GetGroupByIdAsync` | `int id` | `Task<ApplicationGroup?>` | Gruppe per Id laden |
| `GetSystemGroupAsync` | — | `Task<ApplicationGroup?>` | Systemgruppe laden |
| `AddGroupAsync` | `ApplicationGroup group` | `Task<ApplicationGroup>` | Neue Gruppe anlegen |
| `UpdateGroupAsync` | `ApplicationGroup group` | `Task<ApplicationGroup>` | Gruppe aktualisieren |
| `DeleteGroupAsync` | `int id` | `Task` | Gruppe löschen |
| `GetApplicationsAsync` | `StorageMode storageMode, string owner` | `Task<IList<Application>>` | Alle Anwendungen laden, ggf. gefiltert |
| `GetUngroupedApplicationsAsync` | `StorageMode storageMode, string owner` | `Task<IList<Application>>` | Nur ungrouped Anwendungen laden |
| `GetApplicationByIdAsync` | `int id` | `Task<Application?>` | Anwendung per Id laden |
| `AddApplicationAsync` | `Application application` | `Task<Application>` | Neue Anwendung anlegen |
| `UpdateApplicationAsync` | `Application application` | `Task<Application>` | Anwendung aktualisieren |
| `DeleteApplicationAsync` | `int id` | `Task` | Anwendung löschen |

---

## `IEndpointRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IEndpointRepository.cs`

Bleibt unverändert — kein Bestandteil des Refactorings.

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `GetEndpointsAsync` | `int applicationId` | `Task<IList<Endpoint>>` | Alle Endpunkte einer Anwendung laden |
| `GetEndpointByIdAsync` | `int id` | `Task<Endpoint?>` | Endpunkt per Id laden |
| `AddEndpointAsync` | `Endpoint endpoint` | `Task<Endpoint>` | Neuen Endpunkt anlegen |
| `UpdateEndpointAsync` | `Endpoint endpoint` | `Task<Endpoint>` | Endpunkt aktualisieren |
| `DeleteEndpointAsync` | `int id` | `Task` | Endpunkt löschen |
| `GetEndpointGroupsAsync` | `int applicationId` | `Task<IList<EndpointGroup>>` | Alle Endpunktgruppen einer Anwendung laden |
| `GetEndpointGroupByIdAsync` | `int id` | `Task<EndpointGroup?>` | Endpunktgruppe per Id laden |
| `AddEndpointGroupAsync` | `EndpointGroup group` | `Task<EndpointGroup>` | Neue Endpunktgruppe anlegen |
| `UpdateEndpointGroupAsync` | `EndpointGroup group` | `Task<EndpointGroup>` | Endpunktgruppe aktualisieren |
| `DeleteEndpointGroupAsync` | `int id` | `Task` | Endpunktgruppe löschen |
| `AddHeaderAsync` | `EndpointHeader header` | `Task<EndpointHeader>` | Neuen Header anlegen |
| `DeleteHeaderAsync` | `int id` | `Task` | Header löschen |
| `AddQueryParameterAsync` | `EndpointQueryParameter parameter` | `Task<EndpointQueryParameter>` | Neuen QueryParameter anlegen |
| `DeleteQueryParameterAsync` | `int id` | `Task` | QueryParameter löschen |
