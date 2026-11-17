# Bestandsaufnahme

Untersuchungsgegenstand: SignalR-Benachrichtigungs-Auslösepolitik auf allen mutierenden Pfaden.

## Zusammenfassung

Der Benachrichtigungs-Transport (`ISignalRNotificationService` → `SignalRNotificationService<EndpointHub>` → SignalR-Gruppen/Events) ist zentral und stabil. Die **Auslöseentscheidung** (`StorageMode == Team`) ist dagegen dezentral in jedem REST-Controller und in einer Razor-Komponente dupliziert. OData-Controller, Header-/Query-Parameter-Endpunkte, Import-Apply-Endpunkte und der Startabgleich benachrichtigen **gar nicht**.

Detaildokumente:
- [inventory/notification-call-sites.md](inventory/notification-call-sites.md) — alle Auslösestellen, Gruppen und Client-Handler
- [inventory/mutating-paths.md](inventory/mutating-paths.md) — vollständige Liste aller mutierenden API-Pfade mit Benachrichtigungs-Ist-Zustand

## Schlüsselfakten

### Transport (unverändert, `src/Schnittstellenzentrale.Infrastructure/Services/SignalRNotificationService.cs`)

| Methode | SignalR-Gruppe | Event |
|---------|---------------|-------|
| `NotifyTreeChangedAsync` | `workspace` | `TreeChanged` |
| `NotifyApplicationChangedAsync(id)` | `application:{id}` | `ApplicationChanged` |
| `NotifyGroupChangedAsync(id)` | `group:{id}` | `GroupChanged` |
| `NotifyEndpointChangedAsync(epId, appId)` | `application:{appId}` | `EndpointChanged` |
| `NotifyEndpointGroupChangedAsync(gId, appId)` | `application:{appId}` | `EndpointGroupChanged` |
| `NotifyEnvironmentChangedAsync` | `environments` | `EnvironmentChanged` |

Client-Handler existieren nur für `TreeChanged`, `EndpointChanged`, `EndpointGroupChanged` (`ApplicationGroupTree.razor` Z. 129–145) und `EnvironmentChanged` (`AppShell.razor` Z. 140). `ApplicationChanged`/`GroupChanged` hat kein Client-Handler — bleibt unverändert.

### Auslösestellen heute

- **REST benachrichtigt** (jeweils `if (... == StorageMode.Team)`):
  - `ApplicationsController` Create/Update/Delete → `NotifyTreeChangedAsync` (Z. 130–131, 172–173, 208–209)
  - `ApplicationGroupsController` Create/Update/Delete → `NotifyTreeChangedAsync` (Z. 105–106, 146–147, 201–202)
  - `EndpointsController` Create/Update/Delete → `NotifyEndpointChangedAsync` (Z. 127–128, 185–186, 217–218)
  - `EndpointGroupsController` Create/Update/Delete → `NotifyEndpointGroupChangedAsync` (Z. 106–107, 149–150, 181–182)
- **REST benachrichtigt NICHT**: `EndpointsController` AddHeader/DeleteHeader/AddQueryParameter/DeleteQueryParameter (Z. 236–335); `ApplicationImportController` ApplyODataDiff/ApplySwaggerDiff.
- **OData benachrichtigt NICHT**: alle mutierenden Actions der vier `OData*Controller` (Post/Put/Patch/Delete).
- **UI doppelt**: `EndpointPage.razor` Z. 445–446 feuert zusätzlich `NotifyEndpointChangedAsync` nach dem Speichern über `ApplicationApiClient` (REST-Pfad, der bereits benachrichtigt).
- **Startabgleich**: `SystemEndpointSyncService` ruft `ISwaggerImportService.SyncFromDocumentAsync` auf — ohne Benachrichtigung.
- **Umgebungen**: `EnvironmentManagementOverlay.razor` und `EndpointScriptRunner` nutzen `NotifyEnvironmentChangedAsync` — außerhalb des Scopes.

### Kontextmechanik

- REST: `ApiControllerBase.ParseRequestContextAsync()` liefert `RequestContext` mit `StorageMode` (aus `X-Storage-Mode`-Header).
- OData: `ODataControllerBase.ParseStorageMode()` liest denselben Header; Authentifizierung via `IAsyncActionFilter` in der Basisklasse.
- StorageMode-Enum: `Schnittstellenzentrale.Core.Enums.StorageMode` (`User`, `Team`).

### Tests

- `ControllerTestFactory` (Z. 59–61) ersetzt `ISignalRNotificationService` durch einen **nicht exponierten** `Mock` — OData-Benachrichtigungen werden technisch abgefangen, aber nicht inhaltlich verifiziert. Der Mock muss zugänglich gemacht werden.
- OData-Integrationstests: `src/Schnittstellenzentrale.Tests/Integration/ODataControllerIntegrationTests.cs` (~2041 Zeilen, `IClassFixture<ControllerTestFactory>`).
- Playwright-SignalR-Test: `SignalRSyncTests.BrowserA_CreatesApp_BrowserB_ReceivesViaSignalR` (Team-Modus, TreeChanged).
- `EndpointPageTests` registriert einen `Mock<ISignalRNotificationService>` (Z. 20, 34), verifiziert ihn aber nicht — Entfernen des Page-Calls bricht keinen Test.
- Bekannter Flake (diff-unabhängig): `ODataEndpointExecutionTests.OdataApi_GetEndpoints_MitContainsFilter`.

### DI

- `Program.cs` Z. 141: `AddScoped<ISignalRNotificationService, SignalRNotificationService<EndpointHub>>()`.
