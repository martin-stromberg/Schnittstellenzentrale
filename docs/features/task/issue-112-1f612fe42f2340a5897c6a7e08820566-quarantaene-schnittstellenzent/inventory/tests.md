# Tests

## Test-Ausgangszustand vor der Umsetzung

- **Zeitpunkt (mit Zeitzone):** 2026-11-24, ca. 19:58–20:02 UTC (lokale Zeit UTC+1)
- **Branch und Commit-ID:** `task/issue-112-1f612fe42f2340a5897c6a7e08820566-quarantaene-schnittstellenzent`, `d416395c2d7f0ae592c5aeeaf194229fc6aee5cc` (`.NET-11-Upgrade + risikoreiche NuGet-Befunde aus Paketprüfung 2026-KW48 (#111)`)
- **Uncommittete Änderungen im getesteten Stand:** Nur das untracked Feature-Dokumentationsverzeichnis `docs/features/task/issue-112-…/`. Produktiv-/Testcode unverändert gegenüber dem Commit. Nebeneffekt des OS-Laufs: `SiteScreenshotTests` schreiben `site/screenshots/*.png` neu (4 Dateien) — nach dem Lauf per `git checkout` zurückgesetzt; der getestete Stand war code-identisch mit dem Commit.
- **Testumgebung und Runtime-/SDK-Versionen:** Windows (Git Bash/MSYS), Rechner `DESKTOP-CM8OBSG`; .NET SDK 11.0.100 (`global.json`, net11.0, Debug-Konfiguration); xunit.v3 4.0.3 im Microsoft.Testing.Platform-Modus (`test.runner` in `global.json`); Microsoft.Playwright 1.65.1, lokale Chromium-Installation. **Abweichung:** Für den OS-Lauf war `CI=true` gesetzt → headless Chromium (lokaler Default wäre headed, `TestPaths.IsHeadless()`).
- **Ermittelte Testsuiten und Quellen der Testbefehle:** Zwei Lanes laut `CLAUDE.md` und `docs/help/stabilitaet/os-interface-tests.md`: reguläre Lane `Category!=OsInterface` (blockierend, 641 Tests) und OS-Schnittstellen-Lane `Category=OsInterface` (best-effort, 94 Tests). MTP-Syntax: `--filter` als Test-App-Argument hinter `--`; TRX via `--report-trx`. Build vorab: `dotnet build Schnittstellenzentrale.slnx` erfolgreich (0 Warnungen, 0 Fehler, 8,2 s, inkl. `InstallPlaywright`-Target).

### Testläufe

| Lauf | Befehl inkl. Filter | Arbeitsverzeichnis | Exit-Code | Erfolgreich | Fehlgeschlagen | Übersprungen | Nachweis |
|------|--------------------|--------------------|-----------|-------------|----------------|--------------|----------|
| Build | `dotnet build Schnittstellenzentrale.slnx --nologo` | Repo-Root | 0 | — | — | — | Konsolen-Output (0 Warn./0 Fehler) |
| Regulär | `dotnet test --project src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj --no-build --results-directory docs/features/task/issue-112-…/inventory/test-results -- --filter "Category!=OsInterface" --report-trx --report-trx-filename baseline-regular.trx` | Repo-Root | 0 | 641 | 0 | 0 | [baseline-regular.trx](test-results/baseline-regular.trx), [Konsole](test-results/baseline-regular-console.log) |
| OS-Interface | `dotnet test --project src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj --no-build --results-directory docs/features/task/issue-112-…/inventory/test-results -- --filter "Category=OsInterface" --report-trx --report-trx-filename baseline-osinterface.trx` (Env `CI=true`) | Repo-Root | 0 | 94 | 0 | 0 | [baseline-osinterface.trx](test-results/baseline-osinterface.trx), [Konsole](test-results/baseline-osinterface-console.log) |

TRX-Auswertung: regulär 641×`Passed`, OS 94×`Passed` (je 1 zusätzlicher `Completed`-Containereintrag). Alle sieben Methoden von `TreeCollapseTests` bestanden; `EndpointGroupInitiallyCollapsed` lief 12,1 s (TRX `startTime` 19:59:47 UTC, `endTime` 19:59:59 UTC).

### Nachgewiesene bestehende Testfehler

**Keine.** Beide Lanes liefen vollständig grün; kein Testfall ist im Ausgangslauf fehlgeschlagen. Der Quarantäne-Befund (1 Fehlschlag in 5 CI-Läufen) ist ein CI-/Timing-Phänomen — lokal trat er im dokumentierten Einzellauf nicht auf.

### Testlücken und Ausführungsprobleme

- **Konsolen-Warnungen ohne Testfehlschlag:** `TempDirectoryCleanup` meldete während des OS-Laufs zwei best-effort-Löschfehler gesperrter Temp-Dateien (`tempfile-cleanup-…txt`, `tempdir-cleanup-…/locked.txt` nach 5 Versuchen). Das ist dokumentiertes Verhalten (Restfehler nur geloggt); die zugehörigen Tests bestanden.
- **Nebeneffekt:** Der OS-Lauf regeneriert `site/screenshots/*.png` (4 veränderte Dateien durch `SiteScreenshotTests`) — nach dem Lauf zurückgesetzt.
- **Artefakte:** `playwright-traces/` (~33 MB Trace-ZIPs, gitignored) wurde nach dem Lauf gelöscht; TRX-Dateien liegen unter `test-results/`.
- **Umgebungsabweichung:** OS-Lauf mit `CI=true` (headless) statt lokalem headed-Default; CI-Läufe nutzen Release-Build + `windows-latest` — Konfigurationsunterschied zum lokalen Debug-Lauf.
- **Nicht ausgeführt:** Die CI-Historie (`Download-OsInterfaceTrxHistory.ps1`) und der Analyzer wurden lokal nicht gespiegelt; der Analyzer-Befund aus Issue #112 ist als bestehender CI-Befund dokumentiert, nicht lokal reproduziert.

## Testklassen

### `TreeCollapseTests` (`src/Schnittstellenzentrale.Tests/Playwright/TreeCollapseTests.cs`)

`[Collection("Playwright")] [OsInterface]` auf Klassenebene — alle 7 Methoden `[Fact]` ohne weitere Traits.

- `ClickApplicationGroupTitle_TogglesCollapse` — Titelklick auf Anwendungsgruppe klappt Kind-Elemente zu/auf (eigene Gruppe „Collapse-Test-Gruppe" + Anwendung)
- `ClickApplicationName_ExpandsApplication` — Klick auf Anwendungsnamen expandiert (Chevron `sz-icon-chevron-down` sichtbar)
- `ClickApplicationName_SelectsApplication` — Klick selektiert Anwendung (`.sz-content-title`)
- `ClickApplicationName_CollapsesExpandedApplication` — Zweiter Klick klappt Anwendung wieder zu (Chevron `sz-icon-chevron-right`)
- `ClickEndpointGroupName_TogglesCollapse` — Ordner anlegen + **Blindklick** `appBtn.ClickAsync()` (Zeile 160), dann Label-Klicks mit Chevron-Assertionen
- `ClickEndpointGroupChevron_TogglesCollapse` — identisches Muster (Blindklick Zeile 189), Chevron-Button statt Label
- `EndpointGroupInitiallyCollapsed` — **Quarantäne-Kandidat:** Ordner + Endpunkt anlegen, `Page.ReloadAsync()` (Zeile 222), `ExpandSystemGroupAsync`, **Blindklick** `appBtn.ClickAsync()` (Zeile 226), Assertion `sz-icon-chevron-right` an der Ordnerzeile (Zeile 229). Kein `[Flaky]`-Attribut.

### `FlakyConventionTests` (`src/Schnittstellenzentrale.Tests/Infrastructure/Testing/FlakyConventionTests.cs`)

- `FlakyMarkedTests_AlwaysCarryOsInterface` — erzwingt `Flaky` nur mit `OsInterface` (reguläre Lane)

### `TraitParityTests` (`src/Schnittstellenzentrale.Tests/Infrastructure/Testing/TraitParityTests.cs`)

- `PlaywrightTestClasses_AlwaysCarryOsInterface` — Trait-Parität aller `*Playwright*`-Testklassen (reguläre Lane)

Weitere 24 OS-getaggte Testklassen unter `src/Schnittstellenzentrale.Tests/Playwright/` (alle im OS-Lauf grün gelaufen).

## Hilfsmethoden

### `TreeCollapseTests` (klassenintern)

- `ExpandSystemGroupAsync` (Zeilen 118–138) — **bereits gehärteter** verifizierender Expand der Systemgruppe: wartet bis zu 3×5 s auf `SystemAppRow`-`sz-tree-item-btn`; bei Timeout klickt er den ersten `.collapsible-section .sz-tree-chevron-btn` (Gruppen-Chevron) erneut. Federt verlorene Klicks während des SignalR-Reconnects ab. Wird von `ClickEndpointGroupName_TogglesCollapse`, `ClickEndpointGroupChevron_TogglesCollapse` und `EndpointGroupInitiallyCollapsed` genutzt.
- `SystemAppRow` (Zeilen 140–141) — Locator: `.sz-tree-row` mit `.sz-tree-item-btn` `HasText = "Schnittstellenzentrale"` (Systemanwendung).

### `PlaywrightTestBase`

- `InitializeAsync` — DB-Reset (`TestDatabaseSeeder.ResetAsync`), Chromium-Launch, Kontext `de-DE`, Tracing, `NewPageAsync`
- `DisposeAsync` — Trace-Stop `{TestName}-HHmmssfff.zip` unter `playwright-traces/`, best-effort-Cleanup
- `ApplyDefaultTimeouts` — `SetDefaultTimeout(30 s)`/`SetDefaultNavigationTimeout(60 s)` pro Kontext

### `PlaywrightServer` / `TestDatabaseSeeder` / `TestPaths`

- `InitializeAsync` — Kestrel 127.0.0.1:5099, benannte In-Memory-SQLite, Test-Auth, API über In-Process-Handler, `ISwaggerImportService`-Mock, `SignalRNotificationProxy`
- `WaitForServerReadyAsync` — Socket-Readiness-Poll (500 ms / 60 s)
- `ResetAsync` — Schema neu + `SystemEntryInitializer` + System-Endpunkt-Seed
- `IsHeadless` / `FindRepoRoot` — Headless-Flag (`CI=true`) und Repo-Root-Auflösung
