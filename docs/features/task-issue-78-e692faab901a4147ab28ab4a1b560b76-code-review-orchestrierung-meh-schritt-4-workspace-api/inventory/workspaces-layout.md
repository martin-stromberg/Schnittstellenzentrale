# Detail: WorkspacesLayout.razor – Ist-Zustand und Zuordnung

Datei: `src/Schnittstellenzentrale/Components/Layout/WorkspacesLayout.razor` (420 Zeilen)

## Zu ersetzende Blöcke

| Methode / Block | Zeilen | Ist-Zugriff | Soll-Zugriff (IApplicationApiClient) |
|-----------------|--------|-------------|-------------------------------------|
| `OnCreateEndpointGroupConfirmed` | 264–279 | `EndpointRepository.AddEndpointGroupAsync` + Team-Notify | `AddEndpointGroupAsync` (Notify im Controller) |
| `HandleCreateEndpointRequested` | 281–317 | `EndpointRepository.AddEndpointAsync`, `GetEndpointByIdAsync` (mit Nav-Objekten) + Team-Notify | `AddEndpointAsync`, dann Pfadrekonstruktion via `GetEndpointByIdAsync` + `GetApplicationByIdAsync` + `GetGroupByIdAsync` + `GetEndpointGroupByIdAsync` |
| `OnEndpointGroupRenamed` | 321–333 | `EndpointRepository.UpdateEndpointGroupAsync` + Team-Notify | `UpdateEndpointGroupAsync` |
| `HandleDeleteEndpointGroupRequested` (Zählabfrage) | 335–346 | `EndpointRepository.GetEndpointsAsync`, `GetEndpointGroupsAsync` | gleichnamige Client-Methoden |
| `OnEndpointGroupDeleteConfirmed` | 364–386 | `EndpointRepository.DeleteEndpointGroupAsync` + Team-Notify | `DeleteEndpointGroupAsync` |
| `HandleDeleteEndpointRequested` | 388–403 | `EndpointRepository.DeleteEndpointAsync` + Team-Notify | `DeleteEndpointAsync` |
| `OnDeleteGroupConfirmedAll` | 217–231 | `GetGroupByIdAsync` + N× `DeleteApplicationAsync` + `DeleteGroupAsync(id)` | `DeleteGroupAsync(id, ApplicationGroupDeleteMode.Delete)` |
| `OnDeleteGroupConfirmedGroupOnly` | 233–248 | N× `UpdateApplicationAsync` (detach) + `DeleteGroupAsync(id)` | `DeleteGroupAsync(id, ApplicationGroupDeleteMode.Detach)` |

## Zu entfernende Injektionen

- Zeile 3: `@inject IEndpointRepository EndpointRepository`
- Zeile 4: `@inject IStorageModeService StorageModeService`
- Zeile 5: `@inject ISignalRNotificationService SignalRNotificationService`

`IStorageModeService`/`ISignalRNotificationService` werden in der Komponente ausschließlich für die entfallenden Notify-Entscheidungen genutzt (Zeilen 274, 296, 327, 381, 398).

## Zu behaltende Elemente

- `IActivityLogService` + alle `ActivityLogService.Log`-Aufrufe (Zeilen 170, 181, 212, 273, 295, 326)
- `IJSRuntime` (confirm-Dialog beim Endpunkt-Löschen, Zeile 392)
- `_errorMessage`-Pattern mit resx-Keys `WorkspacesLayout_Error_*`, `WorkspacesLayout_Log_*`, `WorkspacesLayout_Confirm_*`, `WorkspacesLayout_DefaultEndpointName`
- `CloseAllPanels`-State-Bereinigung inkl. `_deleteTargetEndpoints`/`_deleteTargetAllGroups`
- `CountEndpointsRecursive`/`CollectGroupIdsRecursive` (Hilfslogik für Zählung und Selektions-Rücksetzung, bleibt clientseitig)
- `ConfirmDeleteGroupDialog` bleibt unverändert (Callbacks `OnDeleteAll`/`OnDeleteGroupOnly` liefern weiter die Gruppe)

## Pfadrekonstruktion nach Endpunkt-Anlage

Ist (Zeilen 299–313): `EndpointRepository.GetEndpointByIdAsync` liefert EF-Entität mit `Application`, `Application.ApplicationGroup`, `EndpointGroup` → Pfad `[ApplicationGroup?, Application, EndpointGroup?, Endpoint]`.

Soll: `GetEndpointByIdAsync` (API) liefert Endpunkt ohne Nav-Objekte → `Application` via `GetApplicationByIdAsync(loaded.ApplicationId)`, `ApplicationGroup` via `GetGroupByIdAsync(app.ApplicationGroupId)` (optional), `EndpointGroup` via `GetEndpointGroupByIdAsync(loaded.EndpointGroupId)` (optional). Gleiche Pfadstruktur, nur direkte Parent-Gruppe (keine Ahnenkette) — wie bisher.
