# Logik

## `AppShell`
Datei: `src/Schnittstellenzentrale/Components/Layout/AppShell.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OnLocationChanged` | `private` | Synchronisiert den aktuellen Navigationsbereich mit der URL und wechselt bei `Impressum` zwischen zuletzt aktivem Bereich und Impressum-Ansicht. |
| `OnStorageModeChanged` | `private` | Reagiert auf Moduswechsel und lädt die gespeicherte Umgebung für den neuen Storage-Modus neu. |
| `OnAfterRenderAsync` | `protected override async` | Initialisiert Theme/StorageMode, stellt die Umgebung aus `localStorage` wieder her und baut die SignalR-Verbindung auf. |
| `ConnectHubAsync` | `private async` | Startet die Hub-Verbindung und lauscht auf `EnvironmentChanged`, damit Umgebungsänderungen live aktualisiert werden. |
| `RestoreEnvironmentFromLocalStorageAsync` | `private async` | Liest die gespeicherte Umgebungs-ID aus `localStorage`, lädt sie via `IApplicationApiClient` und setzt `IActiveEnvironmentService`. |
| `OnEnvironmentChanged` | `private async` | Aktualisiert die aktive Umgebung nach Server- oder Hub-Änderungen und refreshes die Environment-Auswahl. |
| `ClearEnvironmentAndRemoveStorageKeyAsync` | `private async` | Setzt die aktive Umgebung auf `null` und entfernt den gespeicherten Umgebungs-Key aus `localStorage`. |
| `OpenEnvironmentManagementAsync` | `internal async` | Öffnet das `EnvironmentManagementOverlay`. |
| `OnStateChanged` | `private` | Aktualisiert den Render-Zustand nach Änderungen in Theme/Navigation/Umgebung. |
| `DisposeAsync` | `public async` | Entfernt Event-Handler und schließt die SignalR-Verbindung sauber. |

Abonnierte Events: `StorageModeService.OnModeChanged`, `ThemeService.OnThemeChanged`, `ActiveEnvironmentService.OnActiveEnvironmentChanged`, `NavigationStateService.OnAreaChanged`, `NavigationManager.LocationChanged`.
Publizierte Events: keine direkten Produkt-Events; `AppShell` aktiviert jedoch über `NavigationStateService.SetAreaAsync` und SignalR-Callbacks interne UI-Updates.

## `EnvironmentSelector`
Datei: `src/Schnittstellenzentrale/Components/Shared/EnvironmentSelector.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `RefreshAsync` | `public async` | Lädt die Umgebungs-Liste neu und erzwingt ein erneutes Rendern. |
| `OnInitializedAsync` | `protected override async` | Registriert den Listener auf `ActiveEnvironmentService.OnEnvironmentListChanged` und lädt die Umgebungen initial. |
| `OnEnvironmentListChanged` | `private` | Aktualisiert die Liste asynchron nach Umgebungsänderungen. |
| `LoadEnvironmentsAsync` | `private async` | Holt Umgebungen über `ISystemEnvironmentRepository` und setzt die aktuell ausgewählte ID. |
| `OnSelectionChanged` | `private async` | Verarbeitet `change`-Events der Auswahl, prüft Validität der ID und entscheidet zwischen `SetItem`/`RemoveItem`. |
| `ApplySelectionAsync` | `private async` | Persistiert die Auswahl in `localStorage` und setzt die aktive Umgebung über `IActiveEnvironmentService`. |
| `ClearSelectionAsync` | `private async` | Entfernt den Eintrag aus `localStorage` und setzt die aktive Umgebung auf `null`. |
| `Dispose` | `public` | Entfernt den Listener für `OnEnvironmentListChanged`. |

Abonnierte Events: `ActiveEnvironmentService.OnEnvironmentListChanged`.
Publizierte Events: `OnEnvironmentSelectedByUser` via `EventCallback<SystemEnvironment?>`.

## `ApplicationContentView`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationContentView.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OnParametersSetAsync` | `protected override async` | Lädt die Anzahl der Endpunkte der aktuellen Anwendung über `IApplicationApiClient`. |
| `OnNameChanged` | `private async` | Aktualisiert den Namen der Anwendung über `IApplicationService`. |
| `OnSubtitleChanged` | `private async` | Aktualisiert den Untertitel der Anwendung über `IApplicationService`. |
| `OnIconChanged` | `private async` | Aktualisiert das Icon der Anwendung über `IApplicationService`. |
| `OpenSwaggerImportAsync` | `private async` | Ruft `ImportMetadataAsync` auf und öffnet bei Erfolg das `SwaggerImportDialog`. |
| `OpenODataImportAsync` | `private async` | Ruft `ImportMetadataAsync` auf und öffnet bei Erfolg das `ODataImportDialog`. |
| `RunHealthCheckAsync` | `private async` | Führt den Health-Check für die Anwendung über `IHealthCheckService` aus und zeigt den Dialog. |
| `CloseSwaggerImport` | `private void` | Schließt das Swagger-Import-Dialogfenster. |
| `CloseODataImport` | `private void` | Schließt das OData-Import-Dialogfenster. |
| `CloseHealthCheck` | `private void` | Schließt das Health-Check-Dialogfenster. |
| `OnHealthCheckRemove` | `private async` | Schließt das Health-Check-Dialogfenster und entfernt die Anwendung über `IApplicationApiClient`. |

Abonnierte Events: keine Produkt-Events im Komponentenmodell; die Logik nutzt aufgerufene Services und Dialog-Callbacks.
Publizierte Events: keine eigenen allgemeinen Events; `Component`-Callbacks wie `OnClose` werden an Dialog-Komponenten weitergereicht.
