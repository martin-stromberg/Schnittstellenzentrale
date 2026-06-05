# Übersetzte Anforderung – Modus und Umgebung im localStorage speichern

## Fachliche Zusammenfassung

Der aktuell gewählte `StorageMode` (Team oder Benutzer) sowie die zuletzt aktive `SystemEnvironment` werden beim nächsten Anwendungsstart automatisch aus dem `localStorage` des Browsers wiederhergestellt. Bisher speichert `StorageModeService` den Modus nur im Arbeitsspeicher (In-Memory-Zustand), sodass er bei einem Seitenneuladen immer auf den Standardwert `StorageMode.Team` zurückfällt. Die Anforderung erweitert diesen Service um Lese- und Schreibzugriffe auf `localStorage`, analog zum bestehenden Muster in `ThemeService`. Die Wiederherstellung der aktiven Umgebung per `selectedEnvironmentId_<mode>` ist bereits implementiert – sie muss jedoch nach der Moduswiederherstellung erneut ausgelöst werden, da der Modus nun beim ersten Render bekannt ist.

## Betroffene Klassen und Komponenten

### Interfaces

- `IStorageModeService` — Erweiterung um eine asynchrone Initialisierungsmethode:
  - Neue Methode `Task InitializeAsync()` (analog zu `IThemeService.InitializeAsync()`)
  - Bestehende Methode `void SetMode(StorageMode mode)` bleibt erhalten; sie kann optional erweitert werden, um die Persistierung auszulösen (`Task SetModeAsync(StorageMode mode)`)

### Logikklassen / Services

- `StorageModeService` — Kernänderung:
  - Konstruktor erhält `IJSRuntime` als Abhängigkeit (analog zu `ThemeService`)
  - `InitializeAsync()`: Liest den gespeicherten Modus per `localStorage.getItem` aus einem neuen `LocalStorageKeys`-Schlüssel (z. B. `storageMode`) und setzt `CurrentMode` entsprechend
  - `SetMode()` bzw. neues `SetModeAsync()`: Schreibt den gewählten Modus per `localStorage.setItem` zurück

### Enums / Hilfklassen

- `LocalStorageKeys` — neuer Konstanten-Eintrag:
  - `public const string StorageMode = "storageMode";`

### UI-Komponenten

- `AppShell.razor` — Aufruf von `StorageModeService.InitializeAsync()` in `OnAfterRenderAsync` beim ersten Render, vor dem Aufruf von `RestoreEnvironmentFromLocalStorageAsync` (damit der wiederhergestellte Modus beim Umgebungs-Restore bereits gilt)
- `TopBar.razor` — `OnStorageModeChanged`-Handler muss nach der Modusänderung die Persistierung sicherstellen; sofern `SetModeAsync` eingeführt wird, ist der Handler auf `async` umzustellen

### Tests

- `StorageModeServiceTests` (neu) — Unit-Tests mit gemocktem `IJSRuntime`:
  - `InitializeAsync` liest gespeicherten Wert und setzt `CurrentMode`
  - `InitializeAsync` behält Standardwert `StorageMode.Team`, wenn kein Wert im Storage vorhanden ist
  - `InitializeAsync` behält Standardwert, wenn der gespeicherte Wert ungültig ist
  - `SetMode` (bzw. `SetModeAsync`) schreibt den Wert nach `localStorage`
  - `OnModeChanged` wird gefeuert
  - Doppelter Aufruf von `InitializeAsync` importiert das JS-Modul nur einmal (analog `ThemeServiceTests`)
- `AppShellTests` (bestehend oder neu) — bUnit-Test prüft, dass `StorageModeService.InitializeAsync()` beim ersten Render vor `RestoreEnvironmentFromLocalStorageAsync` aufgerufen wird

## Implementierungsansatz

Der bestehende `ThemeService` dient als Referenzimplementierung. Das dort verwendete Muster — JS-Modul per `import` laden, `getStoredTheme`/`setStoredTheme` aufrufen, `InitializeAsync()` im `AppShell.OnAfterRenderAsync(firstRender)` aufrufen — wird für `StorageModeService` übernommen:

1. Neues JS-Modul `storage-mode.js` (oder Erweiterung von `theme.js` mit eigenem Modul) mit `getStoredMode()` / `setStoredMode(value)`.
2. `StorageModeService` lädt das Modul lazy (wie `ThemeService`), liest/schreibt über `IJSObjectReference`.
3. `AppShell.OnAfterRenderAsync` ruft `StorageModeService.InitializeAsync()` vor `RestoreEnvironmentFromLocalStorageAsync` auf, da die Umgebungswiederherstellung den aktuellen Modus benötigt (`LocalStorageKeys.SelectedEnvironmentId(mode)`).

Abhängigkeiten:
- Kein neues NuGet-Paket erforderlich
- `IJSRuntime` ist in der Infrastruktur bereits als DI-Service registriert und in `ThemeService` genutzt
- `LocalStorageKeys.SelectedEnvironmentId(mode)` bleibt unverändert; der korrekte Schlüssel ergibt sich automatisch aus dem wiederhergestellten `CurrentMode`

## Konfiguration

Keine neue Konfigurationsoption erforderlich. Der localStorage-Schlüssel `storageMode` ist ein Hard-coded-Konstant in `LocalStorageKeys` und damit ausreichend für eine Single-Instanz-Anwendung ohne Mehrbenutzerkontexte im Browser.

## Offene Fragen

1. **Erweiterung `SetMode` zu `SetModeAsync`:** Soll `IStorageModeService.SetMode()` zu einer `Task`-Rückgabe geändert werden (bricht bestehende Aufrufer in `TopBar.razor` und Tests), oder wird die Persistierung intern feuer-und-vergiss (`_ = PersistModeAsync(mode)`) umgesetzt? Letzteres vermeidet Breaking Changes am Interface.

2. **Eigenes JS-Modul vs. direkte `IJSRuntime`-Aufrufe:** `ThemeService` kapselt den Zugriff in ein eigenes `.js`-Modul. Da `EnvironmentSelector` bereits direkt `JSRuntime.InvokeVoidAsync("localStorage.setItem", ...)` aufruft, wäre auch ein direkter Ansatz ohne JS-Modul konsistent. Welches Muster soll für den neuen Service gelten?

3. **Verhalten bei ungültigem gespeicherten Wert:** Fällt der Service auf `StorageMode.Team` zurück (wie `ThemeService` auf `ColorScheme.Light`)? Das erscheint sinnvoll, sollte aber explizit bestätigt werden.

4. **Zeitpunkt der Moduswiederherstellung vs. Umgebungswiederherstellung:** Der aktuelle `AppShell`-Code ruft `RestoreEnvironmentFromLocalStorageAsync(StorageModeService.CurrentMode)` auf – wenn `InitializeAsync` des Modus davor läuft, ist alles korrekt sequenziert. Ist die Reihenfolge in `OnAfterRenderAsync` verbindlich und darf nicht durch parallele `Task.WhenAll`-Aufrufe gebrochen werden?
