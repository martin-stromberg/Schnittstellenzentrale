# Tests

## Testklassen

### `ApplicationContentViewTests`
- `ODataImportButton_VisibleForODataApplication` — prüft, ob der OData-Import-Button nur bei OData-Anwendungen sichtbar ist.
- `ODataImportButton_HiddenForRestApplication` — stellt sicher, dass REST-Anwendungen keine OData-Import-Schaltfläche zeigen.
- `ODataImportButton_HiddenWhenInterfaceUrlEmpty` — prüft das Verhalten bei leerer `InterfaceUrl`.
- `OpenODataImport_OnError_ShowsErrorMessage` — validiert die Fehleranzeige beim gescheiterten Import.
- `OpenODataImport_OnSuccess_OpensDialog` — prüft das Öffnen des OData-Dialogs bei Erfolg.
- `OpenSwaggerImport_ClearsPreviousErrorMessage` — prüft das Zurücksetzen einer vorherigen Fehlermeldung.
- `OpenSwaggerImport_OnError_ShowsErrorMessage` — validiert die Fehlermeldung beim Swagger-Import.

### `EnvironmentSelectorTests`
- `RendertUmgebungenAusRepository` — prüft das Rendern von Umgebungs-Optionen aus dem Repository.
- `AktiveUmgebungWirdVorausgewählt` — validiert die Auswahl der aktuell aktiven Umgebung.
- `OhneAktiveUmgebung_ZeigtKeineVorauswahl` — prüft den Fall ohne aktive Umgebung.
- `RefreshAsync_AktualistertListeOhneFehler` — bestätigt die sichere Aktualisierung der Optionen.
- `AuswählenEinerUmgebung_SchreibtLocalStorage` — prüft `localStorage.setItem` bei Auswahl.
- `AbwählenEinerUmgebung_EntferntLocalStorage` — prüft `localStorage.removeItem` beim Leeren der Auswahl.
- `AuswählenNichtExistierenderId_EntferntLocalStorageUndSetztNull` — prüft die Bereinigung für veraltete bzw. unbekannte Umgebungs-IDs.

### `MainLayoutTests`
- `AppShell_RendertWorkspacesTab` — prüft, ob der Workspaces-Tab dargestellt wird.
- `AppShell_RendertEnvironmentsTab` — prüft den Environments-Tab.
- `AppShell_RendertHistoryTab` — prüft den History-Tab.
- `AppShell_RendertModusSelektor` — prüft den StorageMode-Selektor.
- `AppShell_RendertProfilIcon` — prüft die Darstellung des Profil-Icons.
- `DisposeAsync_OhneHubConnection_WirftKeinenFehler` — validiert sauberes Dispose ohne Hub-Verbindung.
- `Wiederherstellen_GespeicherteIdVorhanden_SetzAktiveUmgebung` — prüft das Wiederherstellen der Umgebung aus `localStorage`.
- `Wiederherstellen_UmgebungNichtMehrInDb_BereinigTLocalStorage` — prüft die Bereinigung, wenn die Speicherung nicht mehr existiert.
- `Wiederherstellen_KeinEintragImLocalStorage_SetzNichts` — prüft Null-Fall ohne Eintrag.
- `AppShell_SetAreaAsync_AktualisiertBereich` — prüft Bereichswechsel durch `NavigationStateService`.
- `Wiederherstellen_BeiModuswechsel_VerwendetNeuenSchlüssel` — prüft die Schlüsselwahl beim Wechsel des Modus.

### `AppShellTests`
- `OnAfterRender_CallsStorageModeInitializeAsync_BeforeRestoreEnvironment` — prüft die Reihenfolge beim Initialisieren und Wiederherstellen der Umgebung.

### Weitere bUnit-Komponententests
- `ImportDialogTests` — prüft Import-Dialog-/Bestätigungslogik im UI.
- `EndpointPageTests` — prüft Rendering und Interaktion auf der Endpoint-Seite.
- `ApplicationContextMenuTests` — prüft Kontextmenü-Interaktionen für Anwendungen.
- `EndpointContextMenuTests` — prüft Kontextmenü-Interaktionen für Endpoints.
- `EndpointGroupContextMenuTests` — prüft Kontextmenü-Interaktionen für Endpoint-Gruppen.

### Playwright-Tests
- `ApplicationCrudTests` — CRUD-Flow für Anwendungselemente.
- `EndpointGroupCrudTests` — CRUD-Flow für Endpoint-Gruppen.
- `GroupCrudTests` — CRUD-Flow für Gruppen.
- `EnvironmentManagementTests` — UI-Flow zur Umgebungserstellung und Verwaltung.
- `NavigationTests` — Navigation und Bereichswechsel über die Oberfläche.
- `LayoutSmokeTests` — prüft, ob das Layout nicht kollabiert (`BoundingBoxAsync`).
- `StorageModeTests`, `SwaggerImportTests`, `ODataImportTests`, `InplaceEditingTests`, `TreeCollapseTests` und weitere Playwright-Dateien decken UI- und E2E-Flows ab.

## Hilfsmethoden

### `TestMockFactory`
- `CreateActivityLogServiceMock` — liefert einen `IActivityLogService`-Mock für UI-Tests.
- `CreateEnv` — erzeugt eine einfache `SystemEnvironment`-Testinstanz mit ID, Name und Storage-Modus.
- `CreateFakeLocalizer` — liefert einen `IStringLocalizer<SharedResources>`, der lokalisierten Text exakt als Schlüssel zurückliefert und damit das Rendern von UI-Komponenten testbar macht.

### `ControllerTestFactory`
- Wird für Integrationstests genutzt, um ein Test-Host-Setup für API-/Controller-Tests bereitzustellen und damit die echte App-Konfiguration in Tests nachzubilden.

