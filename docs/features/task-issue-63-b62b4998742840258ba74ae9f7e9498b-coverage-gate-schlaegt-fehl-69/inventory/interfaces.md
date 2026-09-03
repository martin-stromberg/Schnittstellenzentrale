# Interfaces

## `IApplicationApiClient`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IApplicationApiClient.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `GetGroupsAsync` | `(StorageMode storageMode, string owner)` | `Task<IList<ApplicationGroup>>` | Lädt Anwendungsgruppen für den aktuellen Modus/Besitzer. |
| `GetGroupByIdAsync` | `(int id)` | `Task<ApplicationGroup?>` | Holt eine Gruppe anhand der ID. |
| `AddGroupAsync` | `(ApplicationGroup group)` | `Task<ApplicationGroup>` | Erstellt eine Gruppe. |
| `UpdateGroupAsync` | `(ApplicationGroup group)` | `Task<ApplicationGroup>` | Aktualisiert eine Gruppe. |
| `DeleteGroupAsync` | `(int id)` | `Task` | Entfernt eine Gruppe. |
| `GetUngroupedApplicationsAsync` | `(StorageMode storageMode, string owner)` | `Task<IList<Application>>` | Lädt ungeordnete Anwendungen. |
| `GetApplicationByIdAsync` | `(int id)` | `Task<Application?>` | Holt eine Anwendung nach ID. |
| `AddApplicationAsync` | `(Application application)` | `Task<Application>` | Erstellt eine neue Anwendung. |
| `UpdateApplicationAsync` | `(Application application)` | `Task<Application>` | Aktualisiert eine Anwendung. |
| `DeleteApplicationAsync` | `(int id)` | `Task` | Entfernt eine Anwendung. |
| `GetEndpointGroupsAsync` | `(int applicationId)` | `Task<IList<EndpointGroup>>` | Lädt Endpoint-Gruppen mithilfe der Anwendungs-ID. |
| `GetEndpointGroupByIdAsync` | `(int id)` | `Task<EndpointGroup?>` | Holt eine Endpoint-Gruppe nach ID. |
| `AddEndpointGroupAsync` | `(EndpointGroup group)` | `Task<EndpointGroup>` | Erstellt eine Endpoint-Gruppe. |
| `UpdateEndpointGroupAsync` | `(EndpointGroup group)` | `Task<EndpointGroup>` | Aktualisiert eine Endpoint-Gruppe. |
| `DeleteEndpointGroupAsync` | `(int id)` | `Task` | Entfernt eine Endpoint-Gruppe. |
| `GetEndpointsAsync` | `(int applicationId, int? endpointGroupId = null)` | `Task<IList<Endpoint>>` | Lädt Endpunkte einer Anwendung bzw. Gruppe. |
| `GetEndpointByIdAsync` | `(int id)` | `Task<Endpoint?>` | Holt einen Endpoint nach ID. |
| `AddEndpointAsync` | `(Endpoint endpoint)` | `Task<Endpoint>` | Erstellt einen Endpoint. |
| `UpdateEndpointAsync` | `(Endpoint endpoint)` | `Task<Endpoint>` | Aktualisiert einen Endpoint. |
| `DeleteEndpointAsync` | `(int id)` | `Task` | Entfernt einen Endpoint. |
| `AddHeaderAsync` | `(EndpointHeader header)` | `Task<EndpointHeader>` | Fügt einen Header hinzu. |
| `DeleteHeaderAsync` | `(int id)` | `Task` | Entfernt einen Header. |
| `AddQueryParameterAsync` | `(EndpointQueryParameter parameter)` | `Task<EndpointQueryParameter>` | Fügt einen Query-Parameter hinzu. |
| `DeleteQueryParameterAsync` | `(int id)` | `Task` | Entfernt einen Query-Parameter. |
| `GetEnvironmentByIdAsync` | `(int id)` | `Task<SystemEnvironment?>` | Holt eine Umgebung nach ID. |
| `ImportMetadataAsync` | `(int applicationId)` | `Task<ImportDiff>` | Importiert Swagger/OData-Metadaten und liefert Unterschiede zurück. |
| `ApplyODataDiffAsync` | `(int applicationId, ImportDiff diff)` | `Task` | Wendet OData-Änderungen auf die Anwendung an. |

## `IApplicationService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IApplicationService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `UpdateNameAsync` | `(int applicationId, string name)` | `Task` | Aktualisiert den Namen einer Anwendung. |
| `UpdateSubtitleAsync` | `(int applicationId, string? subtitle)` | `Task` | Aktualisiert den Untertitel einer Anwendung. |
| `UpdateIconAsync` | `(int applicationId, byte[] iconData)` | `Task` | Aktualisiert das Icon einer Anwendung. |

