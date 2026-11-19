# Tests — Bestandsaufnahme und Ausgangszustand

## Test-Ausgangszustand vor der Umsetzung

- **Zeitpunkt:** 2026-11-19, ~20:16–20:25 Uhr (UTC+01:00, lokale Windows-Zeit)
- **Branch:** `task/issue-80-618da43981904b78b3108ab7addaf9b6-github-page-anwendung-mit-mode`
- **Commit-ID:** `7c113da74c772b6e4adab383766833f1dfde5e42` („Backmerge from main to staging (#95)", 2026-11-17 12:24:35 +0100)
- **Uncommittete Änderungen im getesteten Stand:** Nur untracked Dateien unter `docs/features/task/issue-80-…/` (`requirement.md`, `decisions.md`, `todo.md`, `inventory/`); keine Änderungen an getrackten Dateien.
- **Testumgebung:** Windows (Git Bash), `dotnet --version` = **10.0.404** (`global.json` pinnt `10.0.301`, `rollForward: latestFeature`), `Microsoft.Playwright` 1.52.0, Chromium bereits über das `InstallPlaywright`-MSBuild-Target installiert.
- **Ermittelte Testsuiten und Quellen der Testbefehle:** Zwei Suiten aus einem Testprojekt (`src/Schnittstellenzentrale.Tests`), Befehle laut `CLAUDE.md`/`README.md` und CI (`pr-staging-ci.yml`):
  - Unit-/Integrationstests: `dotnet test --filter "FullyQualifiedName!~Playwright"`
  - Playwright-E2E: `dotnet test --filter "FullyQualifiedName~Playwright"` (in CI `continue-on-error: true`)
- **Abweichung zur Standardumgebung:** Für den Playwright-Lauf wurde `CI=true` exportiert, damit `PlaywrightTestBase.IsHeadless()` den Browser headless startet (lokal sonst sichtbar); kein Einfluss auf Testlogik, dokumentiert wegen der geänderten Env-Variable.

### Testläufe

| Lauf | Befehl inkl. Filter | Arbeitsverzeichnis | Exit-Code | Erfolgreich | Fehlgeschlagen | Übersprungen | Nachweis |
|------|--------------------|--------------------|-----------|-------------|----------------|--------------|----------|
| Unit/Integration | `dotnet test Schnittstellenzentrale.slnx --filter "FullyQualifiedName!~Playwright" --logger "trx;LogFileName=inventory-baseline-unit.trx" --logger "console;verbosity=normal"` | Repo-Root | 0 | 654 | 0 | 0 | [Console-Log](test-results/unit-tests-console.log), [TRX](test-results/inventory-baseline-unit.trx) |
| Playwright | `CI=true dotnet test Schnittstellenzentrale.slnx --no-build --filter "FullyQualifiedName~Playwright" --logger "trx;LogFileName=inventory-baseline-playwright.trx" --logger "console;verbosity=normal"` | Repo-Root | 0 | 64 | 0 | 0 | [Console-Log](test-results/playwright-tests-console.log), [TRX](test-results/inventory-baseline-playwright.trx) |

Gesamtlaufzeit: ~23,6 s (Unit/Integration, Testzeit ohne Build) bzw. ~2,05 min (Playwright).

### Nachgewiesene bestehende Testfehler

Keine — beide Suiten liefen ohne einen einzigen Fehlschlag durch (654/654 bzw. 64/64 bestanden). Es wurden keine übersprungenen Tests gezählt.

### Testlücken und Ausführungsprobleme

- Keine Ausführungsprobleme; beide Läufe vollständig.
- Coverage wurde in dieser Bestandsaufnahme nicht erhoben (kein `--collect`); die CI erzwingt 70 % Line Coverage separat.
- Die Feature-Anforderung selbst (statische Page + Deploy-Workflow) erzeugt keine .NET-Testoberfläche; absichernde Checks (YAML-Validierung, Layout-Smoke-Test der Page) existieren nicht — es gibt keinen etablierten Testpfad für statische HTML/CSS-Artefakte im Repo.

## Testklassen (relevant für diese Anforderung)

Da keine App-Codeänderung ansteht, sind primär die Playwright-Klassen als **Infrastruktur-Vorlage für die Screenshot-Erzeugung** relevant:

### `LayoutSmokeTests` (`src/Schnittstellenzentrale.Tests/Playwright/LayoutSmokeTests.cs`)
- `AppShell_NimmtVollbildEin` — `BoundingBoxAsync` auf `.sz-app-shell` (> 200 px)
- `TopBar_IstSichtbarUndHatHoehe` — `.sz-topbar` Höhe ≥ 30 px
- `WorkspacesBereich_SidebarUndContentHabenFlaeche` — Klick `.sz-topbar-tab` „Workspaces", Sidebar/Content-Box
- `EnvironmentsBereich_SidebarHatFlaeche`, `HistoryBereich_ContentHatFlaeche` — weitere Bereichs-Boxen
- `ActivityLog_KlickAufToggleButton_ZeigtPanel` — Retry-Muster bis Blazor-Circuit interaktiv

### `HomePageTests` (`src/Schnittstellenzentrale.Tests/Playwright/HomePageTests.cs`)
- `StartPage_ShowsSystemGroup`, `StartPage_ShowsOwnApiEndpoints` — Basis-Navigation + Baum-Aufklappen (`.sz-tree-chevron-btn`)

### Weitere Suite (Auswahl)
- `ApplicationCrudTests`, `GroupCrudTests`, `EndpointGroupCrudTests` — CRUD-Flüsse über die UI (erzeugen Daten, die auch für Screenshots taugen)
- `EndpointExecutionTests` — Endpunkt-Ausführungsansicht
- `SwaggerImportTests`, `ODataImportTests` — Import-Dialoge/Diff-Ansicht
- `EnvironmentManagementTests`, `HistoryPage`-nahe Tests, `SignalRSyncTests` (Zwei-Kontext-Setup via `CreateAdditionalContextAsync`)

## Hilfsmethoden

### `TestDatabaseSeeder` (`Playwright/Infrastructure/TestDatabaseSeeder.cs`)
- `ResetAsync()` — DB neu anlegen + `SystemEntryInitializer` + `SeedSystemEndpointsAsync()`; liefert den einzigen reproduzierbaren Datenstand (Systemgruppe + `POST /authenticate`)

### `TestHelpers` / `TestMockFactory` (`src/Schnittstellenzentrale.Tests/Helpers/`)
- Helfer für bUnit-/Integrationstests (kein direkter Screenshot-Bezug; `ControllerTestFactory`, `CoverageTestFactory` für WebApplicationFactory-Szenarien)

### `PlaywrightTestBase`-Helfer
- `CreateAdditionalContextAsync(locale, acceptLanguage)` — zweiter Browser-Kontext
- `BaseUrl`, `Page`, `Context` als geschützte Zugriffspunkte
