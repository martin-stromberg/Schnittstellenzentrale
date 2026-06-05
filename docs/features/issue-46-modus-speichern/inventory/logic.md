# Logik

## `StorageModeService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/StorageModeService.cs`

Implementiert `IStorageModeService`. Hält den Modus ausschließlich im Arbeitsspeicher; kein `IJSRuntime`-Zugriff vorhanden.

| Methode / Member | Sichtbarkeit | Kurzbeschreibung |
|------------------|-------------|------------------|
| `CurrentMode` | `public` | Auto-Property, Standardwert `StorageMode.Team` |
| `OnModeChanged` | `public event` | Wird in `SetMode` nach Zustandsänderung ausgelöst |
| `SetMode(StorageMode mode)` | `public` | Prüft Gleichheit, setzt `CurrentMode`, feuert `OnModeChanged` |

Fehlend: `IJSRuntime`-Abhängigkeit im Konstruktor, `InitializeAsync()`-Methode, Persistierung via `localStorage`.

---

## `ThemeService` (Referenzimplementierung)
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ThemeService.cs`

Implementiert `IThemeService`. Zeigt das vollständige localStorage-Muster mit `IJSRuntime` und lazy-geladenem JS-Modul.

| Methode / Member | Sichtbarkeit | Kurzbeschreibung |
|------------------|-------------|------------------|
| `CurrentScheme` | `public` | Auto-Property, Standardwert `ColorScheme.Light` |
| `OnThemeChanged` | `public event` | Wird in `SetTheme` nach Zustandsänderung ausgelöst |
| `SetTheme(ColorScheme scheme)` | `public async Task` | Validiert, setzt `CurrentScheme`, persistiert, feuert Event |
| `InitializeAsync()` | `public async Task` | Liest `getStoredTheme` aus JS-Modul, setzt `CurrentScheme` |
| `PersistTheme(ColorScheme scheme)` | `private async Task` | Ruft `setStoredTheme` und `applyTheme` im JS-Modul auf |
| `GetModuleAsync()` | `private async Task<IJSObjectReference>` | Lädt `theme.js` lazy per `import`; gibt gecachtes Modul zurück |

Abonnierte Events: keine.
Publizierte Events: `OnThemeChanged`.

---

## `StorageModeServiceExtensions`
Datei: `src/Schnittstellenzentrale.Core/Helpers/StorageModeServiceExtensions.cs`

Erweiterungsmethoden für `IStorageModeService`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `GetCurrentOwner(IStorageModeService, ICurrentUserService)` | `public static` | Gibt den aktuellen Benutzernamen zurück, wenn Modus `User` ist; sonst `null` |

---

## `AppShell` (Razor-Komponente)
Datei: `src/Schnittstellenzentrale/Components/Layout/AppShell.razor`

Relevant für den Initialisierungsfluss beim ersten Render.

| Methode / Member | Sichtbarkeit | Kurzbeschreibung |
|------------------|-------------|------------------|
| `OnInitialized()` | `protected override` | Abonniert `StorageModeService.OnModeChanged`, `ThemeService.OnThemeChanged` u. a. |
| `OnAfterRenderAsync(bool firstRender)` | `protected override async Task` | Ruft beim ersten Render `ThemeService.InitializeAsync()` und `RestoreEnvironmentFromLocalStorageAsync` auf |
| `OnStorageModeChanged()` | `private` | Reagiert auf Moduswechsel; ruft `RestoreEnvironmentFromLocalStorageAsync` mit neuem Modus auf |
| `RestoreEnvironmentFromLocalStorageAsync(StorageMode mode)` | `private async Task` | Liest `selectedEnvironmentId_{mode}` aus `localStorage` per direktem `JSRuntime.InvokeAsync` |
| `ClearEnvironmentAndRemoveStorageKeyAsync(string key)` | `private async Task` | Entfernt Umgebungsauswahl und löscht den `localStorage`-Schlüssel |
| `ConnectHubAsync()` | `private async Task` | Baut SignalR-Verbindung zu `/hubs/endpoint` auf |
| `OnEnvironmentChanged()` | `private async Task` | Aktualisiert die aktive Umgebung bei Hub-Meldung |
| `OpenEnvironmentManagementAsync()` | `internal async Task` | Öffnet das Umgebungs-Management-Overlay |
| `DisposeAsync()` | `public async ValueTask` | Meldet alle Event-Abonnements ab, trennt Hub-Verbindung |

Abonnierte Events: `StorageModeService.OnModeChanged`, `ThemeService.OnThemeChanged`, `ActiveEnvironmentService.OnActiveEnvironmentChanged`, `NavigationStateService.OnAreaChanged`, `NavigationManager.LocationChanged`.

Hinweis: `StorageModeService.InitializeAsync()` wird in `OnAfterRenderAsync` noch **nicht** aufgerufen. Der Aufruf von `RestoreEnvironmentFromLocalStorageAsync` erfolgt direkt mit `StorageModeService.CurrentMode` (In-Memory-Standardwert `StorageMode.Team`).

---

## `TopBar` (Razor-Komponente)
Datei: `src/Schnittstellenzentrale/Components/Layout/TopBar.razor`

Enthält das `<select>`-Element zur Modusauswahl durch den Benutzer.

| Methode / Member | Sichtbarkeit | Kurzbeschreibung |
|------------------|-------------|------------------|
| `OnInitialized()` | `protected override` | Abonniert `NavigationStateService.OnAreaChanged` und `StorageModeService.OnModeChanged` |
| `OnStorageModeChanged(ChangeEventArgs e)` | `private async Task` | Parst den gewählten Wert, ruft `StorageModeService.SetMode(mode)` auf, ruft `EnvironmentSelector.RefreshAsync()` auf |
| `RefreshEnvironmentSelectorAsync()` | `public async Task` | Delegiert an `EnvironmentSelector.RefreshAsync()` |
| `SetAreaAsync(NavigationArea area)` | `private async Task` | Delegiert an `NavigationStateService.SetAreaAsync` |
| `BuildInitials(string userName)` | `private static string` | Erzeugt Initialen aus Benutzernamen |
| `Dispose()` | `public` | Meldet Event-Abonnements ab |

Abonnierte Events: `NavigationStateService.OnAreaChanged`, `StorageModeService.OnModeChanged`.

Hinweis: `OnStorageModeChanged` ist synchron-delegiert (ruft `SetMode` synchron auf). Eine `async`-Persistierung per `SetModeAsync` ist noch nicht implementiert.

---

## `theme.js` (JavaScript-Modul, Referenz)
Datei: `src/Schnittstellenzentrale/wwwroot/theme.js`

Exportiert folgende Funktionen, die von `ThemeService` per `IJSObjectReference` aufgerufen werden:

| Funktion | Zweck |
|----------|-------|
| `getStoredTheme()` | Liest `colorScheme` aus `localStorage` |
| `setStoredTheme(scheme)` | Schreibt `colorScheme` in `localStorage` |
| `applyTheme(scheme)` | Setzt/entfernt CSS-Klasse `dark` am `<html>`-Element |
| `getAndApplyStoredTheme()` | Kombiniert Lesen und Anwenden (für `theme-init.js`) |

Ein analoges Modul `storage-mode.js` existiert noch **nicht**.
