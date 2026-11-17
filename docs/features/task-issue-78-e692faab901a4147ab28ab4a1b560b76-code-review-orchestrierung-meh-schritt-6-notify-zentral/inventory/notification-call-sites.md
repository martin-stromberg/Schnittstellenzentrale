# Detail: Benachrichtigungs-Auslösestellen und -Empfänger

## Auslösestellen (Ist)

| Datei | Stelle | Aufruf | Modusprüfung |
|-------|--------|--------|--------------|
| `Controllers/ApplicationsController.cs` | Z. 130–131, 172–173, 208–209 | `NotifyTreeChangedAsync()` | `storageMode == StorageMode.Team` |
| `Controllers/ApplicationGroupsController.cs` | Z. 105–106, 146–147, 201–202 | `NotifyTreeChangedAsync()` | `storageMode == StorageMode.Team` |
| `Controllers/EndpointsController.cs` | Z. 127–128, 185–186, 217–218 | `NotifyEndpointChangedAsync(id, appId)` | `context.StorageMode == StorageMode.Team` |
| `Controllers/EndpointGroupsController.cs` | Z. 106–107, 149–150, 181–182 | `NotifyEndpointGroupChangedAsync(id, appId)` | `context.StorageMode == StorageMode.Team` |
| `Components/Shared/EndpointPage.razor` | Z. 445–446 | `NotifyEndpointChangedAsync(_model.Id, _model.ApplicationId)` | `StorageModeService.CurrentMode == StorageMode.Team` |
| `Components/Shared/EnvironmentManagementOverlay.razor` | Z. 134, 149 | `NotifyEnvironmentChangedAsync()` | außerhalb Scope |
| `Infrastructure/Services/EndpointScriptRunner.cs` | Z. 219 | `NotifyEnvironmentChangedAsync()` | außerhalb Scope |

## Client-seitige Event-Handler

`ApplicationGroupTree.razor` (Hub-Verbindung `/hubs/endpoint`, Z. 120–155):
- `TreeChanged` → `LoadDataAsync()` + `StateHasChanged` (kompletter Baum-Reload)
- `EndpointChanged(int endpointId, int applicationId)` → `ReloadApplicationDataAsync(applicationId)`
- `EndpointGroupChanged(int groupId, int applicationId)` → `ReloadApplicationDataAsync(applicationId)`
- Abonniert Gruppe `workspace` (Z. 148) sowie `application:{id}` beim Expandieren (Z. 176, 270)

`AppShell.razor` Z. 140: `EnvironmentChanged` → `OnEnvironmentChanged()`.

`EndpointHub.cs`: Gruppenmethoden `SubscribeToApplication`, `SubscribeToGroup`, `SubscribeToWorkspace`, `SubscribeToEnvironments` (+ Unsubscribe-Pendants).

## Konsequenz für die Policy-Mapping-Tabelle

| Mutierte Entität | Benachrichtigungsart |
|------------------|----------------------|
| Application (alle Mutationen) | `NotifyTreeChangedAsync` |
| ApplicationGroup (alle Mutationen) | `NotifyTreeChangedAsync` |
| Endpoint (alle Mutationen inkl. Header/Query-Parameter) | `NotifyEndpointChangedAsync(endpointId, applicationId)` |
| EndpointGroup (alle Mutationen) | `NotifyEndpointGroupChangedAsync(groupId, applicationId)` |
| Import-Apply / Startabgleich (mehrentitäts-übergreifend) | `NotifyTreeChangedAsync` (eine einzige Nachricht pro Vorgang) |
