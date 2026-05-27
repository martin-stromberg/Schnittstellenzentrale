# Umsetzungsplan: Speichern der Systemumgebungsauswahl im localStorage

## Übersicht

Die gewählte Systemumgebung wird modusabhängig im `localStorage` des Browsers gespeichert und beim nächsten Aufruf sowie bei jedem Moduswechsel automatisch wiederhergestellt. Die gesamte Logik (Schreiben, Lesen, Fallback, Fehlerbehandlung) ist laut Bestandsaufnahme bereits vollständig implementiert — es fehlen ausschließlich die Tests, die das tatsächliche Verhalten der `localStorage`-Operationen und des Wiederherstellungsablaufs absichern.

## Programmabläufe

### Umgebung auswählen (Schreiben)

1. Der Anwender wählt eine Umgebung im Dropdown des `EnvironmentSelector`.
2. `EnvironmentSelector.OnSelectionChanged` wird ausgelöst.
3. Der aktuelle Modus wird über `IStorageModeService.CurrentMode` abgerufen.
4. Der Schlüssel wird über `LocalStorageKeys.SelectedEnvironmentId(mode)` ermittelt.
5. Bei gültiger Auswahl: `IJSRuntime.InvokeVoidAsync("localStorage.setItem", key, id)` schreibt die ID.
6. Bei leerer Auswahl oder nicht auffindbarer Umgebung: `IJSRuntime.InvokeVoidAsync("localStorage.removeItem", key)` entfernt den Eintrag.
7. `IActiveEnvironmentService.SetActiveEnvironment` wird mit der ausgewählten Umgebung aufgerufen.

Beteiligte Klassen/Komponenten: `EnvironmentSelector`, `IStorageModeService`, `LocalStorageKeys`, `IJSRuntime`, `IActiveEnvironmentService`

---

### Umgebung beim ersten Render wiederherstellen (Lesen)

1. `MainLayout.OnAfterRenderAsync` wird mit `firstRender = true` aufgerufen.
2. `RestoreEnvironmentFromLocalStorageAsync(StorageModeService.CurrentMode)` wird aufgerufen.
3. Der Schlüssel wird über `LocalStorageKeys.SelectedEnvironmentId(mode)` ermittelt.
4. `IJSRuntime.InvokeAsync<string?>("localStorage.getItem", key)` liest die gespeicherte ID.
5. Ist eine ID vorhanden: `ISystemEnvironmentRepository.GetByIdAsync(id)` lädt die Umgebung.
6. Ist die Umgebung gefunden: `IActiveEnvironmentService.SetActiveEnvironment(environment)` setzt sie als aktiv.
7. Ist die Umgebung nicht mehr in der Datenbank vorhanden (Fallback): `IActiveEnvironmentService.SetActiveEnvironment(null)` und `IJSRuntime.InvokeVoidAsync("localStorage.removeItem", key)` bereinigen den veralteten Eintrag.
8. `JSException` beim Prerendering werden stumm abgefangen.

Beteiligte Klassen/Komponenten: `MainLayout`, `LocalStorageKeys`, `IJSRuntime`, `ISystemEnvironmentRepository`, `IActiveEnvironmentService`

---

### Umgebung bei Moduswechsel wiederherstellen

1. Der Anwender wechselt den Modus im Header.
2. `MainLayout.OnStorageModeChanged` wird ausgelöst.
3. `StorageModeService.SetMode(newMode)` setzt den neuen Modus.
4. `RestoreEnvironmentFromLocalStorageAsync(newMode)` wird mit dem neuen Modus aufgerufen (Ablauf wie oben).
5. `_environmentSelector.RefreshAsync()` aktualisiert die Dropdown-Liste.

Beteiligte Klassen/Komponenten: `MainLayout`, `StorageModeService`, `EnvironmentSelector`

## Neue Klassen

Keine.

## Änderungen an bestehenden Klassen

Keine. Alle produktiven Klassen (`EnvironmentSelector`, `MainLayout`, `LocalStorageKeys`, `ActiveEnvironmentService`, `StorageModeService`, `SystemEnvironmentRepository`) sind laut Bestandsaufnahme vollständig implementiert.

## Datenbankmigrationen

Keine.

## Validierungsregeln

Keine.

## Konfigurationsänderungen

Keine.

## Seiteneffekte und Risiken

- **Prerendering:** `localStorage`-Zugriffe via `IJSRuntime` schlagen beim serverseitigen Prerendering fehl. Die `try/catch`-Kapselung für `JSException` in `RestoreEnvironmentFromLocalStorageAsync` ist bereits implementiert und deckt dieses Risiko ab.

## Umsetzungsreihenfolge

1. Neue Testmethode in `EnvironmentSelectorTests`: localStorage.setItem-Aufruf bei Auswahl prüfen.
2. Neue Testmethode in `EnvironmentSelectorTests`: localStorage.removeItem-Aufruf bei Abwahl prüfen.
3. Neue Testmethode in `MainLayoutTests`: `RestoreEnvironmentFromLocalStorageAsync` — Erfolgsfall (gespeicherte ID vorhanden, Umgebung in DB gefunden).
4. Neue Testmethode in `MainLayoutTests`: `RestoreEnvironmentFromLocalStorageAsync` — Fallback (gespeicherte ID vorhanden, Umgebung nicht mehr in DB).
5. Neue Testmethode in `MainLayoutTests`: `RestoreEnvironmentFromLocalStorageAsync` — kein Eintrag im localStorage (kein Aufruf von SetActiveEnvironment mit Wert).
6. Neue Testmethode in `MainLayoutTests`: Wiederherstellung bei Moduswechsel (neuer Modus bestimmt den Schlüssel).
7. Neue Hilfsmethode in `EnvironmentSelectorTests` oder separate Testklasse `LocalStorageKeysTests`: Schlüsselformat für `Team` und `User` prüfen.

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `AuswählenEinerUmgebung_SchreibtLocalStorage` | `EnvironmentSelectorTests` | `localStorage.setItem` wird mit korrektem Schlüssel und ID aufgerufen, wenn eine Umgebung ausgewählt wird |
| `AbwählenEinerUmgebung_EntferntLocalStorage` | `EnvironmentSelectorTests` | `localStorage.removeItem` wird mit korrektem Schlüssel aufgerufen, wenn die Auswahl geleert wird |
| `Wiederherstellen_GespeicherteIdVorhanden_Setzt AktiveUmgebung` | `MainLayoutTests` | `SetActiveEnvironment` wird mit der aus der DB geladenen Umgebung aufgerufen, wenn eine ID im localStorage liegt |
| `Wiederherstellen_UmgebungNichtMehrInDb_BereinigLocalStorage` | `MainLayoutTests` | `SetActiveEnvironment(null)` und `localStorage.removeItem` werden aufgerufen, wenn die gespeicherte ID nicht mehr in der DB existiert |
| `Wiederherstellen_KeinEintragImLocalStorage_SetzNichts` | `MainLayoutTests` | `SetActiveEnvironment` wird nicht aufgerufen, wenn `localStorage.getItem` `null` zurückgibt |
| `Wiederherstellen_BeiModuswechsel_VerwendetNeuenSchlüssel` | `MainLayoutTests` | Nach Moduswechsel wird der Schlüssel des neuen Modus für `localStorage.getItem` verwendet |
| `SelectedEnvironmentId_TeamModus_GibtKorrektesFormat` | `LocalStorageKeysTests` (neu) | `LocalStorageKeys.SelectedEnvironmentId(StorageMode.Team)` gibt `selectedEnvironmentId_Team` zurück |
| `SelectedEnvironmentId_UserModus_GibtKorrektesFormat` | `LocalStorageKeysTests` (neu) | `LocalStorageKeys.SelectedEnvironmentId(StorageMode.User)` gibt `selectedEnvironmentId_User` zurück |

### Betroffene bestehende Tests

Keine.

## Offene Punkte

Keine.
