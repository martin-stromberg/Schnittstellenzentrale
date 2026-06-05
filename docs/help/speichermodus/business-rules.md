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

## Lazy-Import des JS-Moduls

**Beschreibung:** Das JavaScript-Modul `storage-mode.js` wird erst beim ersten Zugriff importiert und dann gecacht. Weitere Aufrufe verwenden das gecachte Objekt, ohne ein erneutes `import` auszulösen.

**Umsetzung:** `StorageModeService.GetModuleAsync()` — `_module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./storage-mode.js");`
