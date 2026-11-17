# UI-Komponenten

## `WorkspacesLayout.razor`
Datei: `src/Schnittstellenzentrale/Components/Layout/WorkspacesLayout.razor`

Injections (Z. 1–8): `INavigationStateService`, `IApplicationApiClient`, **`IEndpointRepository`** (Z. 3), `IStorageModeService`, **`ISignalRNotificationService`** (Z. 5), `IActivityLogService`, `IJSRuntime`, `IStringLocalizer<SharedResources>`.

Direkte Repository- bzw. Notify-Aufrufe (Repository-Pfad, nicht API-Pfad):

| Handler | Zeilen | Verhalten |
|---------|--------|-----------|
| `OnCreateEndpointGroupConfirmed` | 264–279 | `EndpointRepository.AddEndpointGroupAsync` (Z. 272); ActivityLog; `StorageMode.Team` → `NotifyEndpointGroupChangedAsync` (Z. 274–275) |
| `HandleCreateEndpointRequested` | 281–317 | `EndpointRepository.AddEndpointAsync` (Z. 294); ActivityLog; Team → `NotifyEndpointChangedAsync` (Z. 296–297); `GetEndpointByIdAsync` (Z. 299); Auswahl setzen |
| `OnEndpointGroupRenamed` | 321–333 | `EndpointRepository.UpdateEndpointGroupAsync` (Z. 325); ActivityLog; Team → Notify (Z. 327–328) |
| `HandleDeleteEndpointGroupRequested` | 335–346 | `GetEndpointsAsync` + `GetEndpointGroupsAsync` für Endpunktzahl im Dialog (Z. 340–342) |
| `OnEndpointGroupDeleteConfirmed` | 364–386 | Selektion bereinigen; `EndpointRepository.DeleteEndpointGroupAsync` (Z. 380); Team → `NotifyEndpointGroupChangedAsync` (Z. 381–382) |
| `HandleDeleteEndpointRequested` | 388–403 | JS-`confirm`; `EndpointRepository.DeleteEndpointAsync` (Z. 394); Team → `NotifyEndpointChangedAsync` (Z. 398–399) |

UI-komponierte Mehrschritt-Löschung der Anwendungsgruppe (über `IApplicationApiClient`, nicht atomar):

| Handler | Zeilen | Verhalten |
|---------|--------|-----------|
| `OnDeleteGroupConfirmedAll` | 217–231 | `GetGroupByIdAsync` → jede App einzeln `DeleteApplicationAsync` → `DeleteGroupAsync` — Teilerfolg möglich |
| `OnDeleteGroupConfirmedGroupOnly` | 233–248 | jede App einzeln `UpdateApplicationAsync` mit `ApplicationGroupId = null` → `DeleteGroupAsync` — Teilerfolg möglich |

App-/Gruppen-CRUD läuft bereits über `IApplicationApiClient` (`OnGroupRenamed` Z. 211, `OnDeleteApplicationConfirmed` Z. 255). `IActivityLogService.Log`-Aufrufe liegen in den Handlern (Z. 170, 181, 212, 273, 295, 326). Hilfsmethoden `CountEndpointsRecursive` (Z. 348–354), `CollectGroupIdsRecursive` (Z. 356–362).

## `ConfirmDeleteGroupDialog.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ConfirmDeleteGroupDialog.razor`

Zwei Bestätigungsvarianten: `OnDeleteAll` („Alles löschen") und `OnDeleteGroupOnly` („Nur Gruppe löschen") als getrennte `EventCallback<ApplicationGroup>` (Z. 17–18), plus `OnCancel`. Zeigt `Group.Applications.Count` an. Resx-Schlüssel `ConfirmDeleteGroupDialog_*` vorhanden.

## `SwaggerImportDialog.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/SwaggerImportDialog.razor`

`@inject ISwaggerImportService SwaggerImportService` (Z. 1) — **direkter Service-Aufruf, kein API-Pfad**. `ApplyAsync` ruft `SwaggerImportService.ApplyDiffAsync(diff)` (Z. 11–14). Rendert `ImportDialog` mit `Diff`, `Application`, `OnApply`, `OnClose`.

## `ImportDialog.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ImportDialog.razor`

Gemeinsamer Dialog für Swagger- und OData-Import (`SwaggerImportDialog`/`ODataImportDialog` wrappen ihn). `ApplyAsync` (Z. 96–120) baut `selectedDiff` aus den Checkbox-Auswahlmengen und ruft `OnApply`; fängt `DbUpdateConcurrencyException` und generische Fehler für die Anzeige ab.

## `ODataImportDialog.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ODataImportDialog.razor`

Vergleichspfad: ruft `ApiClient.ApplyODataDiffAsync(Application.Id, diff)` (Z. 13) — bereits API-Pfad mit Validierung.

## `ApplicationContentView.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationContentView.razor`

`OpenSwaggerImportAsync` (Z. 136–147) und `OpenODataImportAsync` (Z. 149–160) rufen beide `ApplicationApiClient.ImportMetadataAsync(Application.Id)`; bei `ErrorMessage` Anzeige, sonst Diff an `SwaggerImportDialog`/`ODataImportDialog` (Z. 45–51).

## `EndpointPage.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor` (652 Zeilen)

Injections u. a. `IApplicationApiClient`, `IStorageModeService`, `ISignalRNotificationService` (Z. 7).

| Methode | Zeilen | Verhalten |
|---------|--------|-----------|
| `SaveAsync` | 366–388 | Vollmengen-Modell: `_model.Headers`/`QueryParameters` aus Panel-Einträgen neu aufgebaut (Z. 372–373); `PersistAsync`; `DbUpdateConcurrencyException` → `ConcurrencyWarningDialog` |
| `PersistAsync` | 414–423 | `Id == 0` → `CreateEndpointWithDetailsAsync`; sonst `ApplicationApiClient.UpdateEndpointAsync` (REST-PUT mit Kindern) |
| `CreateEndpointWithDetailsAsync` | 425–442 | `AddEndpointAsync` + je Kind einzeln `AddHeaderAsync`/`AddQueryParameterAsync` |
| `AddHeaderAsync` / `DeleteHeaderAsync` / `AddQueryParameterAsync` / `DeleteQueryParameterAsync` | 444–474 | **Sofort-Persistenz pro Panel-Edit** über API-Einzelendpunkte — parallel zur Vollmengen-Ersetzung im PUT (redundante Kinder-Pflege) |
| `OnSaveSuccessAsync` | 476–484 | `StorageModeService.CurrentMode == Team` → `SignalRNotificationService.NotifyEndpointChangedAsync` (Z. 481–482) — **zweite Notification** zusätzlich zur Controller-Notify im selben `PUT` |
| `SendRequestAsync` | 486–541 | Speichert vor Ausführung bei Dirty; lädt Endpoint + Application via ApiClient; `IEndpointExecutionService.ExecuteAsync` |
| `LoadModelFromParameter` / `SyncAutoContentType` / `ExtractAndStripQueryString` / `SyncPathParameters` | 191–219, 289–320, 551–620 | lokales Modell/Panel-Sync; `ExtractAndStripQueryString` verschiebt `?`-Teil in QueryParameter-Einträge |

## `CollectionContentView.razor` (Randbefund)
Datei: `src/Schnittstellenzentrale/Components/Shared/CollectionContentView.razor`

Zweite Komponente mit `IEndpointRepository`-Injection (Z. 3) — rein lesend: `GetEndpointsByApplicationIdsAsync` für Top-5-Endpunktliste einer Gruppe (Z. 100); nutzt außerdem `IApplicationRepository` für Zähler. Nicht im Änderungspfad der Anforderung, aber ebenfalls ein API-First-Verstoß (lesend).

## `EnvironmentManagementOverlay.razor` (Randbefund)
Datei: `src/Schnittstellenzentrale/Components/Shared/EnvironmentManagementOverlay.razor`

Eigener Notify-Pfad: `StorageMode.Team` → `NotifyEnvironmentChangedAsync` (Z. 133–134, 148–149) — betrifft Umgebungen, nicht den Scope der Anforderung.

## Resx-Bestand
`src/Schnittstellenzentrale/Resources/SharedResources.resx` + `.de.resx` enthalten die Schlüssel `WorkspacesLayout_*` (17 Schlüssel, Z. 779–839), `ConfirmDeleteGroupDialog_*` (7 Schlüssel, Z. 275–299), `ImportDialog_*`, `EndpointPage_*` — Schema `{KomponentenName}_{Rolle}` mit `Comment`-Einträgen vorhanden.
