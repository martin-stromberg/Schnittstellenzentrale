# Tests – Bestandsaufnahme

## Testklassen

### `ApplicationCrudTests`
Datei: `src/Schnittstellenzentrale.Tests/Playwright/ApplicationCrudTests.cs`
Collection: `Playwright`

- `CreateApplication_AppearsInTree` — Legt eine Anwendung an und prüft, ob der Name im Baum erscheint
- `EditApplication_UpdatesNameInTree` — Benennt eine Anwendung um und prüft den neuen Namen im Baum
- `DeleteApplication_DisappearsFromTree` — Löscht eine Anwendung und prüft, dass sie nicht mehr im Baum erscheint

Kein Test prüft das Auf-/Zuklappen von Knoten über den Titeltext oder den Chevron-Button.

---

### `StorageModeTests`
Datei: `src/Schnittstellenzentrale.Tests/Playwright/StorageModeTests.cs`
Collection: `Playwright`

- `SwitchToTeamMode_ShowsTeamData` — Wechselt in den Team-Modus und prüft, dass `.sz-tree-body` sichtbar ist
- `SwitchBackToUserMode_ShowsUserData` — Wechselt zurück in den User-Modus und prüft `.sz-tree-body`

---

### `SignalRSyncTests`
Datei: `src/Schnittstellenzentrale.Tests/Playwright/SignalRSyncTests.cs`
Collection: `PlaywrightSignalR`

- `BrowserA_CreatesApp_BrowserB_ReceivesViaSignalR` — Prüft, dass Browser B eine von Browser A angelegte Anwendung per SignalR-Update erhält

---

## Hilfsmethoden

### `PlaywrightTestBase`
Datei: `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightTestBase.cs`

- `InitializeAsync()` — Setzt Datenbank zurück (`TestDatabaseSeeder.ResetAsync`), startet Playwright-Browser mit Tracing
- `DisposeAsync()` — Speichert Playwright-Trace als ZIP, bereinigt Ressourcen
- `CreateAdditionalContextAsync(string)` — Erstellt einen weiteren Browser-Kontext mit aktiviertem Tracing (genutzt in `SignalRSyncTests`)

### `TestDatabaseSeeder`
Datei: `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/TestDatabaseSeeder.cs`

- `ResetAsync()` — Löscht und recreiert das Datenbankschema; führt `SystemEntryInitializer.InitializeAsync` aus

## Feststellungen zum Testabdeckungsstand

- Für `CollapsibleSection` existieren **keine** Unit-Tests (keine bUnit-Testklasse gefunden).
- Für `ApplicationGroupTree` existieren **keine** Unit-Tests (keine bUnit-Testklasse gefunden).
- Das Kollaps-Verhalten des Baums (Chevron-Klick, Titelklick) ist durch **keine** bestehenden Tests abgedeckt.
- Playwright-Tests für den Baum beschränken sich auf CRUD-Operationen; kein Test interagiert mit dem Auf-/Zuklappen.
