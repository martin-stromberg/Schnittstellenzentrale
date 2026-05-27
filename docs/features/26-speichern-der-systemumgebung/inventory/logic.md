# Logik

## `EnvironmentSelector`
Datei: `src/Schnittstellenzentrale/Components/Shared/EnvironmentSelector.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OnInitializedAsync` | `protected override` | Lädt die Umgebungsliste beim ersten Initialisieren |
| `LoadEnvironmentsAsync` | `private` | Lädt Umgebungen über `ISystemEnvironmentRepository.GetEnvironmentsAsync`; setzt `_selectedId` anhand der aktiven Umgebung |
| `OnSelectionChanged(ChangeEventArgs)` | `private` | Reagiert auf Dropdown-Änderungen; schreibt via `IJSRuntime` in den `localStorage` oder entfernt den Eintrag; ruft `IActiveEnvironmentService.SetActiveEnvironment` auf |
| `RefreshAsync` | `public` | Aktualisiert die Umgebungsliste und löst `StateHasChanged` aus; wird von `MainLayout` nach Moduswechsel aufgerufen |

Abonnierte Events: keine  
Publizierte Events: `OnEnvironmentSelectedByUser` (EventCallback, ausgelöst bei jeder Auswahländerung)

**localStorage-Operationen in `OnSelectionChanged`:**
- Bei gültiger Auswahl: `localStorage.setItem(key, id)` mit Schlüssel `LocalStorageKeys.SelectedEnvironmentId(mode)`
- Bei leerer Auswahl oder nicht auffindbare Umgebung: `localStorage.removeItem(key)`

---

## `MainLayout`
Datei: `src/Schnittstellenzentrale/Components/Layout/MainLayout.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OnInitialized` | `protected override` | Abonniert Events von `StorageModeService`, `ThemeService` und `ActiveEnvironmentService` |
| `OnAfterRenderAsync(bool)` | `protected override` | Beim ersten Render: initialisiert Theme, ruft `RestoreEnvironmentFromLocalStorageAsync` auf, stellt SignalR-Verbindung her |
| `RestoreEnvironmentFromLocalStorageAsync(StorageMode)` | `private` | Liest `localStorage`-Wert per `IJSRuntime.InvokeAsync`, lädt Umgebung per `ISystemEnvironmentRepository.GetByIdAsync`, setzt via `IActiveEnvironmentService.SetActiveEnvironment`; entfernt veralteten Eintrag bei nicht mehr existierender Umgebung; JSException wird stumm abgefangen |
| `OnStorageModeChanged(ChangeEventArgs)` | `private` | Setzt neuen Modus via `StorageModeService.SetMode`, ruft `RestoreEnvironmentFromLocalStorageAsync` mit neuem Modus auf, aktualisiert `EnvironmentSelector` |
| `OnEnvironmentSelectedByUser(SystemEnvironment?)` | `private` | Protokolliert Umgebungswechsel über `IActivityLogService.Log` |
| `OnEnvironmentChanged` | `private` | Behandelt SignalR-Benachrichtigung: aktualisiert aktive Umgebung, entfernt ungültigen `localStorage`-Eintrag, ruft `_environmentSelector.RefreshAsync` auf |
| `ConnectHubAsync` | `private` | Baut SignalR-Verbindung auf, abonniert `EnvironmentChanged`-Nachrichten |
| `DisposeAsync` | `public` | Meldet Event-Handler ab, trennt SignalR-Verbindung |
| `ToggleActivityLog` | `private` | Schaltet Aktivitätsprotokoll-Panel ein/aus |
| `OnActivityLogDisplayModeChanged(string)` | `private` | Setzt Anzeigemodus des Protokoll-Panels |
| `OnActivityLogPanelHeightChanged(int)` | `private` | Setzt Höhe des Protokoll-Panels |
| `OpenEnvironmentManagementAsync` | `private` | Öffnet das Umgebungsverwaltungs-Overlay |
| `OnStateChanged` | `private` | Löst `StateHasChanged` aus; als Event-Handler für `StorageModeService.OnModeChanged`, `ThemeService.OnThemeChanged`, `ActiveEnvironmentService.OnActiveEnvironmentChanged` |

Abonnierte Events:
- `StorageModeService.OnModeChanged` → `OnStateChanged`
- `ThemeService.OnThemeChanged` → `OnStateChanged`
- `ActiveEnvironmentService.OnActiveEnvironmentChanged` → `OnStateChanged`
- SignalR-Event `EnvironmentChanged` → `OnEnvironmentChanged`

**localStorage-Operationen in `RestoreEnvironmentFromLocalStorageAsync`:**
- `localStorage.getItem(key)` — liest gespeicherte ID
- `localStorage.removeItem(key)` — löscht Eintrag bei nicht mehr vorhandener Umgebung

---

## `ActiveEnvironmentService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ActiveEnvironmentService.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `SetActiveEnvironment(SystemEnvironment?)` | `public` | Setzt `ActiveEnvironment` und `ActiveVariables`; löst `OnActiveEnvironmentChanged` aus |

Publizierte Events: `OnActiveEnvironmentChanged`

---

## `StorageModeService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/StorageModeService.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `SetMode(StorageMode)` | `public` | Setzt `CurrentMode`; löst `OnModeChanged` aus (nur bei tatsächlicher Änderung) |

Publizierte Events: `OnModeChanged`

---

## `SystemEnvironmentRepository`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/SystemEnvironmentRepository.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `GetEnvironmentsAsync(StorageMode, string?)` | `public` | Lädt gefilterte Umgebungsliste inkl. Variablen; filtert nach Modus und Owner |
| `GetByIdAsync(int)` | `public` | Lädt einzelne Umgebung per ID inkl. Variablen; gibt `null` zurück wenn nicht gefunden |
| `AddAsync(SystemEnvironment)` | `public` | Legt neue Umgebung an; setzt Owner bei `StorageMode.User` |
| `UpdateAsync(SystemEnvironment)` | `public` | Aktualisiert Umgebung und Variablen |
| `DeleteAsync(int)` | `public` | Löscht Umgebung per ID |
| `UpdateVariableAsync(int, string, string)` | `public` | Aktualisiert einzelne Variable einer Umgebung |
| `ApplyOwnerFilter(IQueryable, StorageMode, string?)` | `private static` | Hilfsmethode zur Filterung nach Modus und Owner |

---

## `StorageModeServiceExtensions`
Datei: `src/Schnittstellenzentrale.Core/Helpers/StorageModeServiceExtensions.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `GetCurrentOwner(IStorageModeService, ICurrentUserService)` | `public static` | Gibt den aktuellen Benutzernamen zurück wenn `StorageMode.User`, sonst `null`; wird in `EnvironmentSelector.LoadEnvironmentsAsync` verwendet |

---

## `LocalStorageKeys`
Datei: `src/Schnittstellenzentrale.Core/Helpers/LocalStorageKeys.cs`

| Methode / Feld | Sichtbarkeit | Kurzbeschreibung |
|----------------|-------------|------------------|
| `SelectedEnvironmentId(StorageMode)` | `public static` | Gibt den `localStorage`-Schlüssel zurück, z. B. `selectedEnvironmentId_Team` |
| `ActivityLogDisplayMode` | `public const string` | Schlüssel für Anzeigemodus des Aktivitätsprotokolls (bereits vorhanden) |
| `ActivityLogPanelHeight` | `public const string` | Schlüssel für Panel-Höhe des Aktivitätsprotokolls (bereits vorhanden) |
