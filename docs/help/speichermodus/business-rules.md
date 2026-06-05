# Speichermodus — Business Rules

## Standardmodus bei fehlendem oder ungültigem gespeichertem Wert

**Beschreibung:** Wenn im `localStorage` kein Wert für den Schlüssel `storageMode` vorhanden ist oder der gespeicherte Wert keinem gültigen `StorageMode`-Enum-Wert entspricht, wird der Modus `StorageMode.Team` verwendet.

**Bedingungen:**
- Kein gespeicherter Wert (z. B. erster Besuch, Browser-Daten gelöscht)
- Gespeicherter Wert ist eine unbekannte Zeichenkette (z. B. nach einer Umbenennung der Enum-Werte)

**Verhalten:**
- `CurrentMode` wird nicht verändert; es gilt der Initialisierungsstandardwert `StorageMode.Team`.
- `OnModeChanged` wird nicht ausgelöst.

**Umsetzung:** `StorageModeService.InitializeAsync()` — `Enum.TryParse<StorageMode>(stored, ignoreCase: true, out var parsed)` liefert `false`; die Zuweisung an `CurrentMode` wird übersprungen.

---

## Keine doppelte Persistierung bei unverändertem Modus

**Beschreibung:** Wird `SetMode` mit demselben Wert wie `CurrentMode` aufgerufen, werden weder `localStorage` beschrieben noch `OnModeChanged` ausgelöst.

**Bedingungen:**
- `mode == CurrentMode`

**Verhalten:**
- Methode kehrt sofort zurück (`return`).
- Keine JS-Interop-Aufrufe, kein Event.

**Umsetzung:** `StorageModeService.SetMode()` — Gleichheitsprüfung `if (CurrentMode == mode) return;` am Methodenbeginn.

---

## Persistierung erfolgt fire-and-forget

**Beschreibung:** Das Schreiben des Modus in `localStorage` ist von der synchronen Zustandsänderung entkoppelt. `SetMode` ist `void` und löst die Persistierung asynchron aus, ohne auf deren Abschluss zu warten.

**Bedingungen:**
- `SetMode` wird aufgerufen.

**Verhalten:**
- `CurrentMode` und `OnModeChanged` werden sofort aktualisiert.
- `PersistModeAsync` wird mit `_ = PersistModeAsync(mode)` fire-and-forget gestartet.
- Fehler in `PersistModeAsync` (`JSException`, `TaskCanceledException`) werden stillschweigend abgefangen und beeinflussen den Zustand der Anwendung nicht.

**Umsetzung:** `StorageModeService.SetMode()` und `StorageModeService.PersistModeAsync()` — das Interface `IStorageModeService.SetMode()` bleibt `void`, sodass bestehende Aufrufer nicht geändert werden müssen.

---

## Sequenzielle Initialisierung: Modus vor Umgebung wiederherstellen

**Beschreibung:** Die Umgebungswiederherstellung liest den `localStorage`-Schlüssel `selectedEnvironmentId_<mode>`, wobei `<mode>` vom aktuellen `StorageModeService.CurrentMode` abhängt. Deshalb muss `StorageModeService.InitializeAsync()` vollständig abgeschlossen sein, bevor `RestoreEnvironmentFromLocalStorageAsync` aufgerufen wird.

**Bedingungen:**
- Erster Render von `AppShell`

**Verhalten:**
- `await StorageModeService.InitializeAsync()` wird sequenziell (kein `Task.WhenAll`) vor `await RestoreEnvironmentFromLocalStorageAsync(StorageModeService.CurrentMode)` aufgerufen.
- Wäre die Reihenfolge umgekehrt oder parallel, würde die Umgebung stets im Standardmodus `Team` gesucht, unabhängig von der gespeicherten Modus-Präferenz.

**Umsetzung:** `AppShell.OnAfterRenderAsync(bool firstRender)` — explizit sequenzielle `await`-Aufrufe.

---

## OnModeChanged wird bei Initialisierung ausgelöst, wenn ein gültiger Wert vorliegt

**Beschreibung:** `InitializeAsync()` löst `OnModeChanged` aus, wenn der gespeicherte Wert ein gültiger `StorageMode`-Enum-Wert ist. Damit rendert sich `TopBar` nach der Initialisierung neu und zeigt den wiederhergestellten Modus in der Auswahlbox an.

**Bedingungen:**
- `localStorage` enthält einen Wert für `storageMode`
- Der Wert kann mit `Enum.TryParse<StorageMode>(ignoreCase: true)` erfolgreich geparst werden

**Verhalten:**
- `CurrentMode` wird auf den geparsten Wert gesetzt.
- `OnModeChanged` wird ausgelöst.
- Für den Gegensatz (kein Wert / ungültiger Wert) siehe Regel „Standardmodus bei fehlendem oder ungültigem gespeichertem Wert" — dort wird `OnModeChanged` nicht ausgelöst.

**Umsetzung:** `StorageModeService.InitializeAsync()` — `OnModeChanged?.Invoke()` innerhalb des `if`-Zweigs nach erfolgreicher Zuweisung von `CurrentMode`.

---

## Umgebungsselektor nach Restore explizit aktualisieren

**Beschreibung:** Nach der Wiederherstellung der Umgebung aus dem `localStorage` ruft `AppShell` explizit `_topBar.RefreshEnvironmentSelectorAsync()` auf. Dieser Aufruf ist notwendig, weil `EnvironmentSelector` beim ersten Render bereits initialisiert ist und ein `StateHasChanged()` der Elternkomponente die interne Ladeoperation des Selektors nicht erneut auslöst.

**Bedingungen:**
- Erster Render von `AppShell`

**Verhalten:**
- `RestoreEnvironmentFromLocalStorageAsync` setzt `ActiveEnvironmentService.ActiveEnvironment`.
- Anschliessend ruft `AppShell.OnAfterRenderAsync` `_topBar.RefreshEnvironmentSelectorAsync()` auf.
- `RefreshEnvironmentSelectorAsync` delegiert an `EnvironmentSelector.RefreshAsync()`, das die Umgebungsliste neu lädt und den aktiven Eintrag selektiert.

**Umsetzung:** `AppShell.OnAfterRenderAsync(bool firstRender)` — sequenziell nach `RestoreEnvironmentFromLocalStorageAsync`.

---

## API-First: Umgebungsabfrage über IApplicationApiClient

**Beschreibung:** `AppShell` greift für die Umgebungsabfrage beim Restore ausschliesslich über `IApplicationApiClient.GetEnvironmentByIdAsync()` zu, nicht direkt auf `ISystemEnvironmentRepository`. Damit bleibt die Komponente vom Datenzugriff entkoppelt und das API-First-Prinzip des Projekts gewahrt.

**Bedingungen:**
- `RestoreEnvironmentFromLocalStorageAsync` findet eine gültige ID im `localStorage`

**Verhalten:**
- Es wird `IApplicationApiClient.GetEnvironmentByIdAsync(id)` aufgerufen, der intern `GET /api/system-environments/{id}` ausführt.
- Gibt der Endpunkt `null` zurück (404), wird die gespeicherte ID aus dem `localStorage` entfernt und `ActiveEnvironmentService.ActiveEnvironment` auf `null` gesetzt.

**Umsetzung:** `AppShell.RestoreEnvironmentFromLocalStorageAsync()` — Injection von `IApplicationApiClient` statt `ISystemEnvironmentRepository`.

---

## Lazy-Import des JS-Moduls

**Beschreibung:** Das JavaScript-Modul `storage-mode.js` wird erst beim ersten Zugriff importiert und dann gecacht. Weitere Aufrufe verwenden das gecachte Objekt, ohne ein erneutes `import` auszulösen.

**Umsetzung:** `StorageModeService.GetModuleAsync()` — `_module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./storage-mode.js");`
