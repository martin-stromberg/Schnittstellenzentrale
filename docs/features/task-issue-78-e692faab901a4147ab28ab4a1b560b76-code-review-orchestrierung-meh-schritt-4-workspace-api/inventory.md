# Bestandsaufnahme – Workspace-Oberfläche auf API-Pfad umstellen (Issue #89)

## Zusammenfassung

Die Komponente `src/Schnittstellenzentrale/Components/Layout/WorkspacesLayout.razor` nutzt für Endpunkt- und Endpunktgruppen-Operationen aktuell direkt `IEndpointRepository` und trifft komponenteneigene Team-Notify-Entscheidungen über `IStorageModeService` + `ISignalRNotificationService`. Beim Löschen von Anwendungsgruppen komponiert sie mehrstufige Abläufe aus Einzel-API-Aufrufen. Alle benötigten API-Client-Methoden (inkl. `DeleteGroupAsync(id, ApplicationGroupDeleteMode)` aus Schritt 3) existieren bereits — es sind ausschließlich UI-Umbauarbeiten in einer Datei nötig.

## Detaildokumente

- [inventory/workspaces-layout.md](inventory/workspaces-layout.md) — Ist-Zustand der Komponente, Zuordnung Repository-Aufruf → API-Client-Methode
- [inventory/api-pfad.md](inventory/api-pfad.md) — Controller-/Client-Fakten (Kontext-Header, Notify-Politik, Nebenbedingungen)
- [inventory/tests.md](inventory/tests.md) — Betroffene Testlandschaft

## Kernfakten

### WorkspacesLayout.razor (420 Zeilen)

- Injektionen Zeilen 1–8: `INavigationStateService`, `IApplicationApiClient`, `IEndpointRepository` (Zeile 3), `IStorageModeService` (4), `ISignalRNotificationService` (5), `IActivityLogService` (6), `IJSRuntime`, `IStringLocalizer<SharedResources>`.
- `IEndpointRepository`-Zugriffe: Zeilen 272 (`AddEndpointGroupAsync`), 294 (`AddEndpointAsync`), 299 (`GetEndpointByIdAsync`), 325 (`UpdateEndpointGroupAsync`), 340/341 (`GetEndpointsAsync`/`GetEndpointGroupsAsync` für Zählabfrage), 380 (`DeleteEndpointGroupAsync`), 394 (`DeleteEndpointAsync`).
- Notify-Entscheidungen: Zeilen 274–275, 296–297, 327–328, 381–382, 398–399 — jeweils `if (StorageModeService.CurrentMode == StorageMode.Team) await SignalRNotificationService.Notify…Async(…)`. `IStorageModeService` und `ISignalRNotificationService` werden ausschließlich dafür genutzt.
- Gruppen-Löschung: `OnDeleteGroupConfirmedAll` (Zeilen 217–231) lädt die Gruppe frisch, löscht jede Anwendung einzeln via `DeleteApplicationAsync`, dann `DeleteGroupAsync(id)`. `OnDeleteGroupConfirmedGroupOnly` (Zeilen 233–248) setzt bei jeder Anwendung `ApplicationGroupId = null` via `UpdateApplicationAsync`, dann `DeleteGroupAsync(id)`.
- Pfadrekonstruktion nach Endpunkt-Anlage (Zeilen 299–313): nutzt Navigationsobjekte `loaded.Application`, `loaded.Application.ApplicationGroup`, `loaded.EndpointGroup` des EF-geladenen Endpunkts — die API-Antwort enthält diese nicht (Mapping in `ApiControllerBase.MapToResponse`).
- Aktivitätsprotokoll: Zeilen 273, 295, 326 (`ActivityLogService.Log`) — bleibt erhalten.

### API-Client (`IApplicationApiClient`, `ApplicationApiClient`)

- Deckt alle benötigten Operationen ab: `AddEndpointGroupAsync`, `UpdateEndpointGroupAsync`, `DeleteEndpointGroupAsync`, `GetEndpointGroupsAsync`, `GetEndpointGroupByIdAsync`, `AddEndpointAsync`, `UpdateEndpointAsync`, `DeleteEndpointAsync`, `GetEndpointsAsync`, `GetEndpointByIdAsync`, `DeleteGroupAsync(int)`, `DeleteGroupAsync(int, ApplicationGroupDeleteMode)` (Schritt 3), `GetApplicationByIdAsync`, `GetGroupByIdAsync`.
- `ApplicationGroupDeleteMode` (`src/Schnittstellenzentrale.Core/Enums/ApplicationGroupDeleteMode.cs`): `Detach` → `?content=detach`, `Delete` → `?content=delete`. `Core.Enums` ist global in `_Imports.razor` eingebunden — kein zusätzliches `@using` nötig.

### Controller-Pfad

- `EndpointGroupsController` / `EndpointsController` (`api/endpoint-groups`, `api/endpoints`): `[RequiresContextHeaders]`, Token-Rotation, `StorageMode`-auswertende Notify-Politik (`NotifyEndpointGroupChangedAsync` / `NotifyEndpointChangedAsync` nur bei `StorageMode.Team`).
- `ApplicationGroupsController.DeleteAsync(id, content)`: `content=delete|detach` (Default `detach`), 400 bei ungültig, 403 bei Systemgruppe, 404 bei fehlender Gruppe; Notify `NotifyTreeChangedAsync` bei Team-Modus; atomare Ausführung in `ApplicationRepository.DeleteGroupAsync(id, mode)` (transaktional, Schritt 3).

### Semantik-Differenzen zum Ist-Zustand

- `DeleteEndpointGroupAsync` (Repository + Controller-Pfad identisch): EF-Kaskade löscht Kindgruppen/Endpunkte — unverändert.
- `UpdateEndpointGroupAsync` über den Controller setzt `ApplicationId`/`ParentGroupId` aus dem Bestand (nicht vom Client) — gleichwertig für Rename.
- `AddEndpointAsync` über `CreateEndpointRequest`: Name, RelativePath, ApplicationId, EndpointGroupId, Method, BodyMode, Body, AuthenticationType, Pre/PostRequestScript — alle vom Dialog gesetzten Felder werden übertragen.
- Fehlerverhalten: Repository warf direkt Exceptions; API-Pfad wirft `HttpRequestException` via `EnsureSuccessStatusCode` — wird weiterhin im `catch` zu `_errorMessage` gemappt (resx-Keys `WorkspacesLayout_Error_*`).

### Nicht im Scope

- `CollectionContentView.razor` (Zeilen 3, 100): `EndpointRepository.GetEndpointsByApplicationIdsAsync` — reiner Lesezugriff, bleibt laut Anforderung unberührt.
- `SystemEndpointSyncService` (explizite Ausnahme in CLAUDE.md), OData-Controller, `Program.cs` DI-Registrierung von `IEndpointRepository` — unverändert.
