# Logik

## `ApplicationRepository`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/ApplicationRepository.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `GetGroupsAsync(StorageMode, string)` | public | Lädt alle `ApplicationGroup`-Einträge inkl. `Applications`; im User-Modus nur Gruppen mit eigenen Anwendungen |
| `GetGroupByIdAsync(int)` | public | Lädt eine Gruppe per ID inkl. `Applications` |
| `GetSystemGroupAsync()` | public | Lädt die einzige Systemgruppe (`IsSystem == true`) |
| `AddGroupAsync(ApplicationGroup)` | public | Persistiert eine neue Gruppe |
| `UpdateGroupAsync(ApplicationGroup)` | public | Aktualisiert eine bestehende Gruppe (Concurrency-Check) |
| `DeleteGroupAsync(int)` | public | Löscht eine Gruppe per ID |
| `GetApplicationsAsync(StorageMode, string)` | public | Lädt alle Anwendungen inkl. Gruppe; im User-Modus gefiltert nach Eigentümer |
| `GetUngroupedApplicationsAsync(StorageMode, string)` | public | Lädt Anwendungen ohne Gruppe |
| `GetApplicationByIdAsync(int)` | public | Lädt eine Anwendung mit vollständigen Navigation-Properties (Endpoints, Headers, QueryParameters, EndpointGroups) |
| `AddApplicationAsync(Application)` | public | Persistiert eine neue Anwendung |
| `UpdateApplicationAsync(Application)` | public | Aktualisiert eine Anwendung (Concurrency-Check) |
| `DeleteApplicationAsync(int)` | public | Löscht eine Anwendung per ID |
| `ApplyOwnerFilter(...)` | private static | Filtert Anwendungen nach Eigentümer im User-Modus |

Fehlend (laut Anforderung): Methoden für `ApplicationLink`-CRUD; Methode zum Zählen von Anwendungen/Endpunkten je Gruppe.

---

## `ActivityLogService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ActivityLogService.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `Log(ActivityLogCategory, string, string?)` | public | Fügt einen neuen Eintrag hinzu und feuert `OnEntryAdded` |
| `Clear()` | public | Leert die Eintrags-Liste |

Publizierte Events: `OnEntryAdded` (Action?)

Abonnierte Events: keine

Hinweis: Rein In-Memory, Scoped. Keine Datenbankpersistenz. Der Dienst wird von `EndpointExecutionService`, `MainLayout`, `ApplicationGroupTree` und `Home.razor` aufgerufen.

---

## `ThemeService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ThemeService.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `InitializeAsync()` | public | Liest das gespeicherte Theme aus `localStorage` via JS-Interop |
| `SetTheme(ColorScheme)` | public | Setzt Theme, persistiert es in `localStorage` und feuert `OnThemeChanged` |
| `PersistTheme(ColorScheme)` | private | Ruft JS-Modul `theme.js` auf (setStoredTheme, applyTheme) |
| `GetModuleAsync()` | private | Lazy-lädt das JS-Modul `./theme.js` |

Publizierte Events: `OnThemeChanged` (Action?)

Abonniert von: `MainLayout` (OnStateChanged)

---

## `StorageModeService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/StorageModeService.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `SetMode(StorageMode)` | public | Setzt den aktuellen Modus und feuert `OnModeChanged` |

Publizierte Events: `OnModeChanged` (Action?)

Abonniert von: `MainLayout`, `ApplicationGroupTree`

---

## `MainLayout`
Datei: `src/Schnittstellenzentrale/Components/Layout/MainLayout.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `OnInitialized()` | protected override | Abonniert Events von `StorageModeService`, `ThemeService`, `ActiveEnvironmentService` |
| `OnAfterRenderAsync(bool)` | protected override | Initialisiert Theme, stellt Umgebung aus localStorage wieder her, verbindet SignalR-Hub |
| `ConnectHubAsync()` | private | Baut SignalR-Verbindung auf; abonniert `EnvironmentChanged`-Event |
| `OnStorageModeChanged(ChangeEventArgs)` | private | Reagiert auf UI-Moduswechsel: setzt Mode, loggt, stellt Umgebung wieder her |
| `OnEnvironmentSelectedByUser(SystemEnvironment?)` | private | Loggt den Umgebungswechsel |
| `ToggleActivityLog()` | private | Öffnet/schließt das ActivityLog-Panel |
| `OnActivityLogDisplayModeChanged(string)` | private | Speichert den Display-Modus des Panels |
| `OnActivityLogPanelHeightChanged(int)` | private | Speichert die Panel-Höhe |
| `RestoreEnvironmentFromLocalStorageAsync(StorageMode)` | private | Liest `localStorage` und setzt die aktive Umgebung |
| `OnEnvironmentChanged()` | private | Aktualisiert aktive Umgebung nach externen Änderungen (SignalR) |
| `ClearEnvironmentAndRemoveStorageKeyAsync(string)` | private | Setzt Umgebung auf null, entfernt localStorage-Eintrag |
| `OpenEnvironmentManagementAsync()` | private | Öffnet das `EnvironmentManagementOverlay` |
| `DisposeAsync()` | public | Meldet Events ab, trennt SignalR-Verbindung |

Abonnierte Events: `StorageModeService.OnModeChanged`, `ThemeService.OnThemeChanged`, `ActiveEnvironmentService.OnActiveEnvironmentChanged`, SignalR `EnvironmentChanged`

---

## `ApplicationGroupTree`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `RefreshAsync()` | public | Lädt alle Daten neu und triggert StateHasChanged |
| `ExpandApplicationAsync(int)` | public | Klappt eine Anwendung auf und abonniert SignalR-Kanal |
| `LoadDataAsync()` | private | Lädt Gruppen, ungrouped Applications und Endpunktdaten |
| `ReloadApplicationDataAsync(int)` | private | Lädt EndpointGroups und Endpoints einer Anwendung neu |
| `SelectAndToggleApplication(int)` | private | Wählt Anwendung aus und klappt Baum auf/zu |
| `ToggleEndpointGroupExpanded(int)` | private | Klappt Endpunktgruppe auf/zu |
| `OnModeChanged()` | private | Reagiert auf StorageMode-Wechsel: Reset + Reload |
| `OnDrop(int?)` | private | Verarbeitet Drag-Drop (Anwendung in Gruppe verschieben) |
| `DisposeAsync()` | public | Meldet Events ab, trennt SignalR |

Abonnierte Events: `StorageModeService.OnModeChanged`, SignalR `EndpointChanged`, `EndpointGroupChanged`

Parameter (EventCallbacks):
- `OnApplicationSelected`, `OnSelectionCleared`, `OnCreateGroupRequested`, `OnCreateApplicationRequested`
- `OnEditApplicationRequested`, `OnRenameGroupRequested`, `OnDeleteGroupRequested`, `OnDeleteApplicationRequested`
- `OnCreateEndpointGroupRequested`, `OnCreateEndpointRequested`, `OnRenameEndpointGroupRequested`
- `OnDeleteEndpointGroupRequested`, `OnDeleteEndpointRequested`, `OnEndpointSelected`

---

## `EndpointExecutionService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `ExecuteAsync(Endpoint)` | public | Führt einen Endpunkt aus, loggt das Ergebnis via `IActivityLogService` |

Schreibt Ausführungsergebnisse nur in `IActivityLogService` (In-Memory); keine Datenbankpersistenz der History.

---

## `EnvironmentManagementOverlay`
Datei: `src/Schnittstellenzentrale/Components/Shared/EnvironmentManagementOverlay.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `OpenAsync()` | public | Öffnet das Overlay und lädt Umgebungen |
| `LoadEnvironmentsAsync()` | private | Lädt Umgebungen per Repository |
| `StartCreate()` / `StartEdit(SystemEnvironment)` / `StartDelete(SystemEnvironment)` | private | Steuert Formularansichten |
| `ConfirmDeleteAsync()` | private | Löscht Umgebung, benachrichtigt via SignalR |
| `OnEnvironmentSaved(SystemEnvironment)` | private | Callback nach Speichern; benachrichtigt via SignalR |

Parameter (EventCallbacks): `OnEnvironmentsChanged`

---

## `EnvironmentEditor`
Datei: `src/Schnittstellenzentrale/Components/Shared/EnvironmentEditor.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `SaveAsync()` | private | Validiert und speichert (Anlegen oder Aktualisieren) |
| `ValidateInput()` | private | Prüft Name (Pflicht, max. 200 Zeichen) und Variablen |
| `AddVariable()` / `RemoveVariable(EnvironmentVariable)` | private | Verwaltung der Variablenliste |

Parameter (EventCallbacks): `OnSaved`, `OnCancel`; Parameter: `ExistingEnvironment`

---

## `ApplicationCard`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationCard.razor`

Zeigt Details einer Anwendung (Name, Description, BaseUrl, InterfaceUrl) und bietet Swagger-Import, OData-Import und Health-Check an. Kein eigener Beschreibungs-Bearbeitungsmodus.

---

## `Home.razor`
Datei: `src/Schnittstellenzentrale/Components/Pages/Home.razor`

Einzige Page-Komponente. Verwaltet den Inhaltsbereich als `if-else-if`-Kette:
- Gruppeneditor, Anwendungseditor, Umbenennung, Löschbestätigungen, Ordneroperationen, `EndpointPage`, `ApplicationCard`, Leertext

Kein eigenständiger Layout-Bereich für Environments oder History.
