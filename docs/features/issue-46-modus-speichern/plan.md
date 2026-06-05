# Umsetzungsplan: Modus und Umgebung im localStorage speichern

## Übersicht

`StorageModeService` wird um `localStorage`-Persistierung erweitert, indem das vollständige Referenzmuster aus `ThemeService` übernommen wird: ein neues JS-Modul `storage-mode.js`, eine `InitializeAsync()`-Methode sowie Persistierung beim Setzen des Modus. `AppShell.OnAfterRenderAsync` ruft die neue Methode vor `RestoreEnvironmentFromLocalStorageAsync` auf, damit der wiederhergestellte Modus beim Umgebungs-Restore bereits gilt. Betroffen sind `LocalStorageKeys`, `IStorageModeService`, `StorageModeService`, `AppShell` und `TopBar` sowie neue Unit-Tests und ein neuer bUnit-Test.

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|-----------------|------------|
| Persistierung beim Setzen des Modus | `SetMode()` bleibt `void`; Persistierung erfolgt intern fire-and-forget (`_ = PersistModeAsync(mode)`) | Vermeidet Breaking Change am Interface; `TopBar` und alle bestehenden Aufrufer bleiben unverändert. Exakt das Muster, das die Anforderung als bevorzugte Option nennt. |
| JS-Zugriff | Eigenes Modul `storage-mode.js` mit `getStoredMode()` / `setStoredMode(value)`, geladen per lazy `import` (Service Layer + Gateway) | Konsistent mit `ThemeService` als verbindlichem Referenzmuster. `EnvironmentSelector` ist eine Ausnahme, kein Präzedenzfall für neue Services. |
| Fallback bei ungültigem gespeichertem Wert | Rückfall auf `StorageMode.Team` | Analog zu `ThemeService` → `ColorScheme.Light`; sinnvolles Default-Verhalten, da `Team` der Standardwert ist. |
| Sequenzierung in `OnAfterRenderAsync` | Sequenzielle `await`-Aufrufe (kein `Task.WhenAll`) | `RestoreEnvironmentFromLocalStorageAsync` benötigt den bereits wiederhergestellten `CurrentMode`; parallele Ausführung würde den Restore mit dem alten In-Memory-Standardwert ausführen. |

## Programmabläufe

### Modus beim Anwendungsstart wiederherstellen

1. `AppShell.OnAfterRenderAsync(firstRender)` wird beim ersten Render aufgerufen.
2. `AppShell` ruft `StorageModeService.InitializeAsync()` auf.
3. `StorageModeService.InitializeAsync()` ruft `GetModuleAsync()` auf, das `storage-mode.js` per `import` lädt (lazy, gecacht).
4. `InitializeAsync()` ruft `getStoredMode()` im JS-Modul auf und erhält den gespeicherten Wert (oder `null`).
5. Bei gültigem Wert: `CurrentMode` wird gesetzt und `OnModeChanged` gefeuert.
6. Bei fehlendem oder ungültigem Wert: `CurrentMode` bleibt `StorageMode.Team`; kein Event.
7. Anschließend ruft `AppShell` `RestoreEnvironmentFromLocalStorageAsync(StorageModeService.CurrentMode)` auf — nun mit dem wiederhergestellten Modus.

Beteiligte Klassen/Komponenten: `AppShell`, `StorageModeService`, `storage-mode.js`

---

### Modus beim Wechsel persistieren

1. Benutzer wählt neuen Modus im `<select>` in `TopBar`.
2. `TopBar.OnStorageModeChanged(ChangeEventArgs)` parst den Wert und ruft `StorageModeService.SetMode(mode)` auf.
3. `StorageModeService.SetMode()` prüft Gleichheit; bei Änderung: setzt `CurrentMode`, löst `_ = PersistModeAsync(mode)` fire-and-forget aus, feuert `OnModeChanged`.
4. `StorageModeService.PersistModeAsync()` ruft `GetModuleAsync()` auf und ruft `setStoredMode(value)` im JS-Modul auf.
5. `AppShell.OnStorageModeChanged()` reagiert auf `OnModeChanged` und ruft `RestoreEnvironmentFromLocalStorageAsync` mit dem neuen Modus auf (bestehendes Verhalten, unverändert).

Beteiligte Klassen/Komponenten: `TopBar`, `StorageModeService`, `storage-mode.js`, `AppShell`

## Neue Klassen

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `StorageModeServiceTests` | xUnit-Testklasse | Unit-Tests für `StorageModeService` mit gemocktem `IJSRuntime` |
| `AppShellTests` | bUnit-Testklasse | Prüft Initialisierungsreihenfolge in `AppShell.OnAfterRenderAsync` |
| `storage-mode.js` | JavaScript-Modul | Kapselt `getStoredMode()` und `setStoredMode(value)` gegen `localStorage` |

## Änderungen an bestehenden Klassen

### `LocalStorageKeys` (statische Helferklasse)

- **Neue Eigenschaften:** `StorageMode` (`const string`, Wert `"storageMode"`) — Schlüssel für den `localStorage`-Eintrag des Speichermodus

---

### `IStorageModeService` (Interface)

- **Neue Methoden:** `InitializeAsync()` — Rückgabe `Task`; liest den gespeicherten Modus aus `localStorage` und setzt `CurrentMode`. Analog zu `IThemeService.InitializeAsync()`.

---

### `StorageModeService` (Logikklasse)

- **Neue Eigenschaften:** Privates Feld `_jsRuntime` (`IJSRuntime`) sowie privates Feld `_module` (`IJSObjectReference?`) für das gecachte JS-Modul.
- **Geänderte Methoden:** `SetMode(StorageMode mode)` — Zusätzlich zu bestehendem Verhalten (Gleichheitsprüfung, `CurrentMode` setzen, `OnModeChanged` feuern) wird `_ = PersistModeAsync(mode)` fire-and-forget ausgelöst.
- **Neue Methoden:**
  - `InitializeAsync()` — Liest `getStoredMode()` aus dem JS-Modul; setzt `CurrentMode` bei gültigem Wert, behält `StorageMode.Team` bei fehlendem oder ungültigem Wert.
  - `PersistModeAsync(StorageMode mode)` (`private async Task`) — Ruft `setStoredMode(value)` im JS-Modul auf.
  - `GetModuleAsync()` (`private async Task<IJSObjectReference>`) — Lädt `storage-mode.js` lazy per `import`; gibt gecachtes Modul zurück.
- **Konstruktor:** Erhält `IJSRuntime` als Abhängigkeit (Constructor Injection).

---

### `AppShell` (Razor-Komponente)

- **Geänderte Methoden:** `OnAfterRenderAsync(bool firstRender)` — Ruft `StorageModeService.InitializeAsync()` sequenziell vor `RestoreEnvironmentFromLocalStorageAsync(StorageModeService.CurrentMode)` auf.

---

### `LocalStorageKeysTests` (Testklasse)

- **Neue Tests:** Test für `LocalStorageKeys.StorageMode` — prüft, dass der Wert `"storageMode"` lautet.

## Datenbankmigrationen

Keine.

## Validierungsregeln

Keine.

## Konfigurationsänderungen

Keine.

## Seiteneffekte und Risiken

- **`AppShell.OnAfterRenderAsync` Reihenfolge:** Das Einfügen von `await StorageModeService.InitializeAsync()` verlängert den ersten Render geringfügig. Das Risiko ist minimal, da `ThemeService.InitializeAsync()` denselben Pfad bereits durchläuft.
- **`StorageModeService`-DI-Registrierung:** Der Service wird aktuell wahrscheinlich als Singleton oder Scoped registriert. `IJSRuntime` ist Scoped — die DI-Registrierung muss prüfen und sicherstellen, dass `StorageModeService` als Scoped (nicht Singleton) registriert ist, analog zu `ThemeService`.
- **`EnvironmentSelectorTests`:** Diese bUnit-Tests mocken `IStorageModeService`. Da nur das Interface um `InitializeAsync()` erweitert wird und die Mocks keine Implementierung benötigen, sind die Tests nicht gebrochen — der Mock muss jedoch das neue Interface-Member implementieren (Moq tut dies automatisch).

## Umsetzungsreihenfolge

1. `LocalStorageKeys` — Konstante `StorageMode = "storageMode"` hinzufügen.
2. `storage-mode.js` — Neues JS-Modul mit `getStoredMode()` und `setStoredMode(value)` erstellen.
3. `IStorageModeService` — Methode `InitializeAsync()` (`Task`) hinzufügen.
4. `StorageModeService` — `IJSRuntime` per Constructor Injection aufnehmen; `GetModuleAsync()`, `InitializeAsync()`, `PersistModeAsync()` implementieren; `SetMode()` um fire-and-forget-Aufruf erweitern.
5. DI-Registrierung prüfen und ggf. `StorageModeService` von Singleton auf Scoped umstellen.
6. `AppShell.OnAfterRenderAsync` — `await StorageModeService.InitializeAsync()` vor `RestoreEnvironmentFromLocalStorageAsync` einfügen.
7. `LocalStorageKeysTests` — Test für neue Konstante ergänzen.
8. `StorageModeServiceTests` — Neue Unit-Testklasse mit allen erforderlichen Tests erstellen.
9. `AppShellTests` — Neue bUnit-Testklasse erstellen und Initialisierungsreihenfolge prüfen.

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `InitializeAsync_SetsCurrentMode_WhenStoredValueIsValid` | `StorageModeServiceTests` | `InitializeAsync` setzt `CurrentMode` auf gespeicherten gültigen Wert |
| `InitializeAsync_KeepsDefaultMode_WhenNoStoredValue` | `StorageModeServiceTests` | `InitializeAsync` behält `StorageMode.Team`, wenn kein Wert gespeichert ist |
| `InitializeAsync_KeepsDefaultMode_WhenStoredValueIsInvalid` | `StorageModeServiceTests` | `InitializeAsync` behält `StorageMode.Team` bei ungültigem gespeicherten Wert |
| `SetMode_PersistsValueToLocalStorage` | `StorageModeServiceTests` | `SetMode` schreibt den Wert per `setStoredMode` in `localStorage` |
| `SetMode_FiresOnModeChanged` | `StorageModeServiceTests` | `SetMode` löst `OnModeChanged` aus |
| `SetMode_DoesNotFire_WhenValueUnchanged` | `StorageModeServiceTests` | `SetMode` feuert Event nicht, wenn Modus bereits gesetzt |
| `InitializeAsync_ImportsModuleOnlyOnce_WhenCalledTwice` | `StorageModeServiceTests` | JS-Modul wird beim zweiten `InitializeAsync`-Aufruf nicht erneut importiert |
| `OnAfterRender_CallsStorageModeInitializeAsync_BeforeRestoreEnvironment` | `AppShellTests` | `StorageModeService.InitializeAsync()` wird beim ersten Render vor `RestoreEnvironmentFromLocalStorageAsync` aufgerufen |
| `StorageMode_Constant_HasExpectedValue` | `LocalStorageKeysTests` | `LocalStorageKeys.StorageMode` hat den Wert `"storageMode"` |

### Betroffene bestehende Tests

Keine.

## Offene Punkte

Keine.
