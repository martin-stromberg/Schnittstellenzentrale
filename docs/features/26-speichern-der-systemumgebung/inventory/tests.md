# Tests

## Testklassen

### `EnvironmentSelectorTests`
Datei: `src/Schnittstellenzentrale.Tests/Components/EnvironmentSelectorTests.cs`

- `RendertUmgebungenAusRepository` — Prüft, dass aus dem Repository geladene Umgebungen als Dropdown-Optionen erscheinen
- `AktiveUmgebungWirdVorausgewählt` — Prüft, dass die aktive Umgebung im Dropdown selektiert dargestellt wird
- `OhneAktiveUmgebung_ZeigtKeineVorauswahl` — Prüft, dass ohne aktive Umgebung kein Wert vorausgewählt ist
- `RefreshAsync_AktualistertListeOhneFehler` — Prüft, dass `RefreshAsync` die Liste aktualisiert ohne Dispatcher-Exception

**JS-Interop-Mocks:** `localStorage.removeItem` und `localStorage.setItem` sind per `JSInterop.SetupVoid` eingerichtet. Kein Test prüft bisher, ob `localStorage.setItem` oder `localStorage.removeItem` tatsächlich aufgerufen wird.

---

### `MainLayoutTests`
Datei: `src/Schnittstellenzentrale.Tests/Components/MainLayoutTests.cs`

- `Layout_RendertModusSelektor` — Prüft, dass das Modus-Dropdown im Header vorhanden ist
- `Layout_RendertZahnradIcon` — Prüft, dass der Button zur Umgebungsverwaltung gerendert wird
- `DisposeAsync_OhneHubConnection_WirftKeinenFehler` — Prüft, dass `DisposeAsync` ohne aufgebaute HubConnection fehlerfrei ist

**JS-Interop-Mocks:** `localStorage.getItem` (gibt `null` zurück), `localStorage.removeItem` und `localStorage.setItem` sind eingerichtet. Kein Test prüft bisher den Wiederherstellungsablauf (`RestoreEnvironmentFromLocalStorageAsync`) oder den Fallback bei gelöschter Umgebung.

## Hilfsmethoden

### `EnvironmentSelectorTests`
- `CreateEnv(int id, string name)` — Erstellt eine `SystemEnvironment`-Testinstanz mit `StorageMode.Team` und leerer Variablenliste
