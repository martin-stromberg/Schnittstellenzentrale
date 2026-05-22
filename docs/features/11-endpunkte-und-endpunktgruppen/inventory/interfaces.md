# Interfaces

## `ISignalRNotificationService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ISignalRNotificationService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `NotifyApplicationChangedAsync` | `int applicationId` | `Task` | Benachrichtigt alle Clients, die die Anwendung abonniert haben |
| `NotifyGroupChangedAsync` | `int groupId` | `Task` | Benachrichtigt alle Clients, die die Gruppe abonniert haben |

**Fehlende Methoden laut Anforderung:** `NotifyEndpointChangedAsync(int endpointId)` und `NotifyEndpointGroupChangedAsync(int endpointGroupId)` sind noch nicht definiert.

---

## `IEndpointRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IEndpointRepository.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `GetEndpointsAsync` | `int applicationId` | `Task<IList<Endpoint>>` | Alle Endpunkte einer Anwendung (inkl. Headers, QueryParams, Gruppe) |
| `GetUngroupedEndpointsAsync` | `int applicationId` | `Task<IList<Endpoint>>` | Nur Endpunkte ohne Gruppe |
| `GetEndpointByIdAsync` | `int id` | `Task<Endpoint?>` | Einzelnen Endpunkt per Id (inkl. Headers, QueryParams, Gruppe) |
| `AddEndpointAsync` | `Endpoint endpoint` | `Task<Endpoint>` | Neuen Endpunkt speichern |
| `UpdateEndpointAsync` | `Endpoint endpoint` | `Task<Endpoint>` | Endpunkt aktualisieren |
| `DeleteEndpointAsync` | `int id` | `Task` | Endpunkt löschen |
| `GetEndpointGroupsAsync` | `int applicationId` | `Task<IList<EndpointGroup>>` | Alle Gruppen einer Anwendung |
| `GetEndpointGroupByIdAsync` | `int id` | `Task<EndpointGroup?>` | Einzelne Gruppe per Id (inkl. Endpoints) |
| `AddEndpointGroupAsync` | `EndpointGroup group` | `Task<EndpointGroup>` | Neue Gruppe speichern |
| `UpdateEndpointGroupAsync` | `EndpointGroup group` | `Task<EndpointGroup>` | Gruppe aktualisieren |
| `DeleteEndpointGroupAsync` | `int id` | `Task` | Gruppe löschen |
| `AddHeaderAsync` | `EndpointHeader header` | `Task<EndpointHeader>` | Header zu Endpunkt hinzufügen |
| `DeleteHeaderAsync` | `int id` | `Task` | Header löschen |
| `AddQueryParameterAsync` | `EndpointQueryParameter parameter` | `Task<EndpointQueryParameter>` | Query-Parameter hinzufügen |
| `DeleteQueryParameterAsync` | `int id` | `Task` | Query-Parameter löschen |

---

## `IEndpointExecutionService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IEndpointExecutionService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `ExecuteAsync` | `Endpoint endpoint` | `Task<EndpointExecutionResult>` | Führt den HTTP-Endpunkt aus und gibt das Ergebnis zurück |
