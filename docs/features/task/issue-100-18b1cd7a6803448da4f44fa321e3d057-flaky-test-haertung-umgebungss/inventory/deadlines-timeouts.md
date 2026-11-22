# Deadline-/Timeout-Bestand

## Server-Start / Readiness

`Playwright/Infrastructure/PlaywrightServer.cs` (Basis aller Server-Fixtures, `IAsyncLifetime`):

- **Fester Port:** `protected virtual string BindUrl => "http://127.0.0.1:5099"` (Zeile 39). Unterklassen überschreiben: `PlaywrightSignalRServer` 5100 (Z. 14), `PlaywrightImpressumServer` 5101 (Z. 15), `PlaywrightImpressumLanguageServer` 5102 (Z. 16), `PlaywrightODataEndpointServer` 5103 (Z. 20), `PlaywrightImpressumFallbackServer` 5104 (Z. 14).
- **Start ohne Readiness-Poll:** `InitializeAsync` ruft `_app.Urls.Clear()`, `_app.Urls.Add(BindUrl)`, `await _app.StartAsync()` auf (Zeilen 133–135). Es gibt **keinen** Poll/Healthcheck gegen die gebundene Adresse — `StartAsync()` kehrt zurück, sobald Kestrel die Bindung geöffnet hat, aber nicht verifiziert, dass Blazor/Statisches/SignalR-Endpunkte tatsächlich antworten. Kein explizites Timeout-Argument an `StartAsync` (Default-Hostverhalten).
- **„Warmup" geht nicht über den Port:** Der einzige Nachstart-Request ist `warmupClient.GetAsync("/swagger/v1/swagger.json")` über `_apiFactory.CreateClient()` (Zeilen 139–140) — ein In-Process-`TestServer`-Client (`PlaywrightApiFactory`), der **nicht** über 127.0.0.1:Port läuft. Die tatsächliche Erreichbarkeit der Bind-URL wird vor dem ersten `Page.GotoAsync` nie geprüft.
- `OnAfterStartAsync()`-Hook (Zeile 52): nur `PlaywrightSignalRServer` nutzt ihn (echten `IHubContext<EndpointHub>` in den `SignalRNotificationProxy` setzen, `PlaywrightSignalRServer.cs:27–32`).
- `FindWebProjectRoot()` (Zeilen 143–155): Dateisystem-Aufwärtssuche nach `src/Schnittstellenzentrale/wwwroot`; wirft `DirectoryNotFoundException`.
- `Program.BuildWebApplicationAsync` hat `Playwright`-Environment-Verzweigungen (`src/Schnittstellenzentrale/Program.cs:53, 179, 190, 193, 209`).

## Playwright-Browser / Page-Timeouts

`Playwright/Infrastructure/PlaywrightTestBase.cs`:

- `Chromium.LaunchAsync` mit nur `Headless = TestPaths.IsHeadless()` (Zeile 49) — **kein** explizites Launch-Timeout.
- `NewContextAsync` (Zeilen 50–54) — keine `DefaultTimeout`/`DefaultNavigationTimeout`-Setzung, weder am Context noch an `Page`. Es gelten die **Playwright-.NET-Defaults** (30 s für Actions/Assertions, 30 s Navigation).
- `Context.Tracing.StartAsync` (Zeilen 56–61) — kein Timeout.
- `Context.Tracing.StopAsync` in `DisposeAsync` (Zeilen 99–103) — **kein eigenes Budget/Timeout**; hängt unbegrenzt, wenn der Browser-Stream klemmt.
- `SitePageTests` (kein `PlaywrightTestBase`): identisches Muster — `LaunchAsync`/`NewContextAsync` ohne Timeout-Optionen (Zeilen 24–33).

## Explizite Timeouts/Deadlines im Testcode

| Stelle | Wert | Zeile |
|---|---|---|
| `Playwright/LayoutSmokeTests.cs` – Retry-Loop „Klick bis interaktiv" | Gesamt-Deadline `DateTime.UtcNow.AddSeconds(15)`, pro Iteration `ToBeVisibleAsync(Timeout = 1500)` ms | 110–121 |
| `Playwright/SiteScreenshotTests.cs` – `ClickUntilVisibleAsync` | Gesamt-Deadline `DateTime.UtcNow.AddSeconds(20)`, pro Iteration `Timeout = 1500` ms | 133–148 |
| `Services/TokenStoreTests.cs` – abgelaufenes Token | `TokenLifetime = 1 ms` + `await Task.Delay(10)` | 53–57 |
| `Integration/ApplicationGroupsControllerIntegrationTests.cs` – abgelaufenes Token | `TokenLifetime = TimeSpan.FromMilliseconds(1)` + `await Task.Delay(10)` | 65–71 |
| `Services/EndpointExecutionServiceTests.cs` – Duration-Messung | `Task.Delay(20)` im gemockten Handler | 280–284 |

Alle übrigen Playwright-Waits sind `Page.WaitForLoadStateAsync(LoadState.NetworkIdle)` bzw. `Assertions.Expect(...)` ohne Timeout-Argument — also Playwright-Default.

Diagnose-Ausgabe (Phase/State/Error im Fehlerfall) existiert **nicht** — kein strukturiertes Logging in den Server-/Test-Fixtures; einzig die Playwright-Traces (`playwright-traces/{TestName}.zip`, Screenshots+Snapshots+Sources) dienen als Diagnose-Artefakt.

## Job-/Step-Timeouts (CI)

| Workflow | Job | `timeout-minutes` |
|---|---|---:|
| `pr-staging-ci.yml` | `static-checks` | 15 (Z. 73) |
| `pr-staging-ci.yml` | `build-and-test` (enthält reguläre + Playwright-Tests + Coverage-Gate) | 30 (Z. 118) |
| `staging-ci.yml` | `static-checks` | 15 (Z. 66) |
| `staging-ci.yml` | `build-and-test` | 30 (Z. 99) |
| `staging-ci.yml` | `version` | 10 (Z. 185) |
| `staging-ci.yml` | `prerelease` | 30 (Z. 258) |
| `release.yml` | Release-Gate-Tests (`dotnet test -c Release --filter "FullyQualifiedName!~Playwright" -p:SkipPlaywrightInstall=true`, Z. 106) | kein Step-Timeout |

`dotnet test` wird ohne `--blame-hang`/Test-Timeout-Parameter aufgerufen (beide CI-Workflows und `release.yml`). Das 30-min-Job-Timeout ist das einzige äußere Budget für beide Test-Steps plus Coverage-Auswertung.

## Bezugspunkt Blaupause (requirement.md FR 4)

Referenzfehler aus `martin-stromberg/Softwareschmiede#428`: Host-Deadline (30 s) < Kind-Budget (90 s). Repo-Gegenstück zum Abgleich: Job-`timeout-minutes: 30` (Host) vs. unbegrenztes `Tracing.StopAsync` + Playwright-Default-30-s-Assertions + keine Server-Readiness-Deadline (Kind-Phasen) — es existiert **kein** explizites Server-Start-Budget, das mit dem Job-Timeout abgeglichen wäre.
