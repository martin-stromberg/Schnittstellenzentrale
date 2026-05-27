# Anforderung: Speichern der Systemumgebungsauswahl im localStorage

## Fachliche Zusammenfassung

Wählt der Anwender im Header der Anwendung eine Systemumgebung aus, soll diese Auswahl persistent im `localStorage` des Browsers gespeichert werden. Beim nächsten Aufruf der Anwendung wird die zuletzt gewählte Umgebung automatisch wiederhergestellt und als aktive Umgebung gesetzt. Da die Auswahl pro `StorageMode` gespeichert wird, bleibt die zuletzt verwendete Umgebung sowohl für den Team-Modus als auch für den Benutzermodus getrennt erhalten.

## Betroffene Klassen und Komponenten

### Bestehende Klassen (Erweiterung)

- `EnvironmentSelector` (`EnvironmentSelector.razor`) — Schreibt bei Änderung der Dropdown-Auswahl den `localStorage`-Eintrag für den aktuellen `StorageMode`; entfernt den Eintrag bei Abwahl.
- `MainLayout` (`MainLayout.razor`) — Liest beim ersten Render den `localStorage`-Eintrag über `RestoreEnvironmentFromLocalStorageAsync` und aktiviert die gespeicherte Umgebung.
- `LocalStorageKeys` (`LocalStorageKeys.cs`) — Statische Hilfsklasse; stellt den Schlüsselnamen `SelectedEnvironmentId(StorageMode)` bereit (z. B. `selectedEnvironmentId_Team`).

### Beteiligte Services und Interfaces

- `IActiveEnvironmentService` / `ActiveEnvironmentService` — setzt und hält die aktive Umgebung im Blazor-Circuit.
- `ISystemEnvironmentRepository` / `SystemEnvironmentRepository` — lädt die Umgebung beim Wiederherstellen per ID aus der Datenbank.
- `IJSRuntime` — JavaScript-Interop für `localStorage.getItem`, `localStorage.setItem` und `localStorage.removeItem`.

### Tests

- `EnvironmentSelectorTests` — Prüft, dass der `localStorage`-Eintrag bei Auswahl geschrieben und bei Abwahl entfernt wird.
- `MainLayoutTests` — Prüft die Wiederherstellung beim ersten Render sowie den Fallback bei nicht mehr vorhandener Umgebung.

## Implementierungsansatz

- **Auslöser (Schreiben):** `EnvironmentSelector.OnSelectionChanged` wird bei jeder Benutzerinteraktion mit dem Dropdown ausgelöst. Bei gültiger Auswahl wird via `IJSRuntime.InvokeVoidAsync("localStorage.setItem", key, id)` der Schlüssel `LocalStorageKeys.SelectedEnvironmentId(mode)` gesetzt. Bei leerer Auswahl oder nicht auffindbarer Umgebung wird `localStorage.removeItem` aufgerufen.
- **Auslöser (Lesen):** `MainLayout.OnAfterRenderAsync(firstRender: true)` ruft `RestoreEnvironmentFromLocalStorageAsync(StorageModeService.CurrentMode)` auf. Die Methode liest den gespeicherten Wert, lädt die Umgebung per `ISystemEnvironmentRepository.GetByIdAsync` und ruft `IActiveEnvironmentService.SetActiveEnvironment` auf.
- **Fallback bei gelöschter Umgebung:** Existiert die gespeicherte ID nicht mehr in der Datenbank, wird `SetActiveEnvironment(null)` gesetzt und der `localStorage`-Eintrag entfernt.
- **Moduswechsel:** Bei `StorageModeService.SetMode` wird `RestoreEnvironmentFromLocalStorageAsync` mit dem neuen Modus erneut aufgerufen, sodass die Umgebungsauswahl modusabhängig wiederhergestellt wird.
- **Fehlerbehandlung:** `localStorage`-Zugriffe werden mit `try/catch` gekapselt, um `JSException`-Fehler beim Server-Prerendering (vor dem WASM/Interaktiv-Rendering) stumm abzufangen.

## Konfiguration

Das Feature ist nicht konfigurierbar. Der `localStorage`-Schlüssel wird durch `LocalStorageKeys.SelectedEnvironmentId(StorageMode)` fest definiert und ergibt sich aus dem Enum-Wert des Modus (z. B. `selectedEnvironmentId_Team`, `selectedEnvironmentId_User`).

## Offene Fragen

Keine. Die Anforderung ist eindeutig und vollständig aus den bestehenden Projektdokumenten ableitbar. Die Implementierungsstrategie ist bereits durch vergleichbare `localStorage`-Verwendung im Projekt (Dark-Mode, Aktivitätsprotokoll) etabliert.
