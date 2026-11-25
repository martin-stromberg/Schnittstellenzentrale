# Testinfrastruktur und CI

## `PlaywrightTestBase` (`src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightTestBase.cs`)

Basisklasse aller Playwright-Tests (`IAsyncLifetime`).

| Member | Sichtbarkeit | Kurzbeschreibung |
|--------|-------------|------------------|
| `DefaultActionTimeoutMs = 30_000` | private const | `SetDefaultTimeout(30 s)` pro Kontext — gilt für Aktionen/Locator; `Expect().ToBeVisibleAsync()` nutzt allerdings den eigenen Assertion-Default (5 s), nicht diesen Wert |
| `DefaultNavigationTimeoutMs = 60_000` | private const | `SetDefaultNavigationTimeout(60 s)` (Blazor-Erstaufruf kann unter CI-Last >30 s dauern) |
| `TraceStopBudget = 30 s` | private static readonly | Deadline für `Tracing.StopAsync` beim Dispose |
| `TraceDirectory` | private static | `<RepoRoot>/playwright-traces/` über `TestPaths.FindRepoRoot()` — absolut verankert, damit der CI-Upload greift; gitignored |
| `Page`, `Context`, `BaseUrl`, `Server`, `TestName` | protected | Testzugriff: `Page`/`Context` (Chromium-Kontext, Locale `de-DE`), `BaseUrl` (aus `PlaywrightServer.BaseAddress`), `TestName` = Klassenname für Trace-Dateinamen |
| `InitializeAsync` | public | `TestDatabaseSeeder.ResetAsync` → `Playwright.CreateAsync` → Chromium (`Headless = TestPaths.IsHeadless()`) → Kontext + Tracing (Screenshots/Snapshots/Sources) → `NewPageAsync` |
| `CreateAdditionalContextAsync` | protected | Zusatzkontexte (z. B. SignalRSyncTests) mit eigenem Tracing |
| `DisposeAsync` | public | Komplett best-effort mit Fehlersammlung: Trace-Stop mit 30-s-Budget (`{TestName}-HHmmssfff.zip`, UTC-Suffix), Kontext/Browser/Playwright dispose — wirft nie, loggt via `ITestOutputHelper` in den TRX-`<Output>`-Block |
| `StopTraceBestEffortAsync` | protected | Für abgeleitete Klassen mit Zusatzkontexten |

## `PlaywrightServer` (`src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightServer.cs`)

Collection-Fixture (`ICollectionFixture<PlaywrightServer>` über `[CollectionDefinition("Playwright")]`, `PlaywrightCollection.cs`); startet echten Kestrel auf Port 5099 (Unterklassen 5100–5104).

| Member | Sichtbarkeit | Kurzbeschreibung |
|--------|-------------|------------------|
| `BindUrl` | protected virtual | `http://127.0.0.1:5099` (fester Port, bewusst; Collections serialisieren) |
| `BaseAddress`, `Services`, `ApiNotificationProxy` | public/protected | Zugriff aus Tests |
| `InitializeAsync` | public virtual | Phasen-markiert (Anchor-DB → `BuildWebApplicationAsync` → `StartAsync` → `WaitForServerReady` → `OnAfterStartAsync` → Swagger-Warmup); Fehler werden mit Phase/BindUrl/Ursache erneut geworfen. Benannte Shared-Cache-In-Memory-SQLite (`Data Source={guid};Mode=Memory;Cache=Shared`), de-DE-Culture, Test-Auth-Scheme, `IHostedService` entfernt, `ISwaggerImportService` gemockt, `ISignalRNotificationService` → `SignalRNotificationProxy`, API-Calls über In-Process-Handler (kein TCP) |
| `WaitForServerReadyAsync` | private | Pollt den echten Kestrel-Socket (500 ms, 60-s-Deadline; **jede** HTTP-Antwort = ready; `UseProxy = false`) |
| `DisposeAsync` | public virtual | Best-effort, wirft nie |

## `TestDatabaseSeeder` (`src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/TestDatabaseSeeder.cs`)

`ResetAsync`: `EnsureDeletedAsync` + `EnsureCreatedAsync`, `SystemEntryInitializer.InitializeAsync` (legt Systemgruppe/-anwendung „Schnittstellenzentrale" an — Voraussetzung für `SystemAppRow`-Lokator), `SeedSystemEndpointsAsync` (System-Endpunkt `POST /authenticate`). `SeedShowcaseDataAsync` nur für `SiteScreenshotTests`.

## `TestPaths` (`src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/TestPaths.cs`)

`IsHeadless()` → `true` nur wenn Env `CI=true` (lokal standardmäßig headed); `FindRepoRoot()` → Solution-Suche für Trace-Verzeichnis.

## Trait-Attribute (`src/Schnittstellenzentrale.Tests/Infrastructure/Testing/`)

| Klasse | Zweck |
|--------|-------|
| `OsInterfaceAttribute` | `Category=OsInterface` auf Klasse/Methode (`ITraitAttribute`) |
| `OsInterfaceFactAttribute` / `OsInterfaceTheoryAttribute` | Fact-/Theory-Variante mit OsInterface-Trait (Methodenebene; Theory bisher ohne Anwender) |
| `FlakyAttribute` | `Category=Flaky` auf Klasse/Methode — **aktuell keine Verwendung in `src/`** |
| `TestCategories` | Konstanten `OsInterface`, `Flaky` |
| `TestTraitReader` | Reflection-Leser für beide Schreibweisen (`[Trait]`-Konstruktorargumente + `ITraitAttribute.GetTraits()`), inkl. Vererbungskette |

## Meta-Tests (reguläre Lane)

| Klasse / Methode | Prüfung |
|------------------|---------|
| `FlakyConventionTests.FlakyMarkedTests_AlwaysCarryOsInterface` | Jeder `Flaky`-Träger (Klasse oder Methode) muss effektiv `OsInterface` liefern — klassenweite Traits vererben auf Methoden |
| `TraitParityTests.PlaywrightTestClasses_AlwaysCarryOsInterface` | Jede Testklasse mit `Playwright` im `FullName` muss `Category=OsInterface` tragen (Klasse oder jede Methode) |

## CI-Workflows

`.github/workflows/pr-staging-ci.yml` und `.github/workflows/staging-ci.yml` (nahezu identisch):

- Job `build & test` (blockierend): `dotnet test --no-build -- --filter "Category!=OsInterface" --coverage … --report-trx` — einzige Lane mit Coverage-Gate (70 %).
- Job `os-interface-tests` (best-effort: `continue-on-error: true`, `timeout-minutes: 45`, `windows-latest`, Permissions `contents:read`/`issues:write`/`actions:read`):
  1. `Test OS interface`: `dotnet test -c Release --no-build -- --filter "Category=OsInterface" --report-trx --report-trx-filename test-results-os-interface.trx`
  2. `Retry flaky tests once` (`if: failure()`): `scripts/Retry-FlakyTests.ps1` — TRX parsen, Schnittmenge mit `Category=Flaky`-Tests, genau **ein** gefilterter Re-Run → `<name>-flaky-retry.trx`; persistent → Exit 1 (Job bleibt nicht-blockierend)
  3. `Download OS-interface TRX history` (`if: always()`, best-effort): `scripts/Download-OsInterfaceTrxHistory.ps1 -MaxRuns 15`
  4. `Analyze flaky test results` (`if: always()`): `scripts/Analyze-FlakyTestResults.ps1` — Quarantäne-Kandidat ab ≥ 2 Flake-Ereignissen ODER Fehlrate ≥ 20 % bei ≥ 5 Ausführungen
  5. `Create quarantine issues` (`if: always()`): `scripts/New-FlakyQuarantineIssue.ps1` — dedupliziert über Titel/Marker `<!-- flaky-quarantine:<fqn> -->`
  6. Artefakte `test-results-os-interface-pr|-staging` (TRX inkl. `*-flaky-retry.trx`, 14 Tage) und `playwright-traces-pr|-staging` (`playwright-traces/`, 14 Tage)
- `version`/`prerelease` in `staging-ci.yml` hängen nicht am OS-Job.

## Skripte (`scripts/`)

| Skript | Rolle |
|--------|-------|
| `Retry-FlakyTests.ps1` | Einmal-Retry fehlgeschlagener `Flaky`-Tests; fehlendes/unauswertbares TRX → neutral Exit 0 |
| `Analyze-FlakyTestResults.ps1` | Flake-Statistik über TRX-Verzeichnisse (Namenskonvention `<name>.trx` / `<name>-flaky-retry.trx`); Klassifikation `Quarantaene-Kandidat`/`beobachten`/`stabil`; schreibt `flaky-quarantine-candidates.json` |
| `Download-OsInterfaceTrxHistory.ps1` | Lädt Artefakt-Kette der OS-Lane für die Analyzer-Historie |
| `New-FlakyQuarantineIssue.ps1` | Legt deduplizierte „Quarantaene: `<test-fqn>`"-Issues an (hat Issue #112 erzeugt) |

## Analyzer-Befund (Issue #112, abgerufen 2026-11-24 via `gh`)

- Issue #112 „Quarantaene: Schnittstellenzentrale.Tests.Playwright.TreeCollapseTests.EndpointGroupInitiallyCollapsed" — OPEN, erstellt 2026-11-24T05:44:41Z, Marker `<!-- flaky-quarantine:…EndpointGroupInitiallyCollapsed -->`.
- Kennzahlen: 5 Ausführungen, 1 Fehlschlag, 1 Flake-Ereignis, 20 % Fehlrate → Kandidatenstatus allein über die Fehlraten-Regel (≥ 2 Flake-Ereignisse nicht erreicht).
- CI-Artefakte (`test-results-os-interface-*`, `playwright-traces-*`): jüngste `pr-staging-ci.yml`-Runs vorhanden (z. B. Run 45336591664 vom 2026-11-24T05:54Z), Retention 14 Tage — Download der Fehlschlag-Artefakte im Umsetzungsschritt prinzipiell möglich (in der Bestandsaufnahme nicht durchgeführt).
