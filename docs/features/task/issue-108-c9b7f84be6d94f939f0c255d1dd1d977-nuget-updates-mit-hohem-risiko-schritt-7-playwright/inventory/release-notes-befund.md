# Release-Notes-Sichtung Playwright for .NET 1.53 → 1.65.1

Quelle: `gh release view v{ver} --repo microsoft/playwright-dotnet` (alle Releases 1.53.0–1.65.1 vollständig gesichtet, Stand 2026; Zwischenstand 1.52.0 ist installiert).

## Befund je Release

| Release | Breaking Changes / relevante Ankündigungen | Betroffen im Repo? |
|---------|---------------------------------------------|--------------------|
| 1.53.0 | Keine BC. Neu: Trace-Viewer-Steps, `Locator.Describe()`, `playwright.ps1 install --list`. Chromium 138. | Nein |
| 1.54.0 | Keine BC. Neu: Cookie-`PartitionKey`, `--user-data-dir`, `playwright.ps1 open` öffnet keinen Recorder mehr (nur CLI-Verhalten). Chromium 139. | Nein (kein `pwsh open`-Aufruf im Repo) |
| 1.55.0 | BC: Chromium-Extension-Manifest-v2-Support entfernt. Sonst: Debian-13-Support, Xunit-v3-/MSTest-v4-Pakete (neu, nicht genutzt). Chromium 140. | Nein (keine Browser-Extensions) |
| 1.56.0 | BC: `BrowserContext.BackgroundPage`-Event entfernt/wird nicht emittiert; `BackgroundPages` liefert leere Liste. Misc: Aria-Snapshots rendern `input`-`placeholder`. Chromium 141. | Nein — `grep BackgroundPage`: 0 Treffer |
| 1.57.0 | BC: `Page.Accessibility` entfernt (seit 3 Jahren deprecated). Wechsel Chromium → **Chrome for Testing**-Builds (headed + headless; keine funktionalen Änderungen erwartet). Chromium 143. | `Page.Accessibility`: 0 Treffer. Chrome-for-Testing: beobachten — headless Tests sollten unverändert laufen; Verifikation über lokale OsInterface-Lane. |
| 1.58.0 | BC: `_react`-/`_vue`-Selektoren entfernt; `:light`-Selektor-Suffix entfernt; `Devtools`-Option aus `BrowserType.LaunchAsync` entfernt; macOS-13-WebKit-Support entfernt. Chromium 145. | Nein — alle entfernten APIs: 0 Treffer (`_react`, `_vue`, `:light`, `Devtools`) |
| 1.59.0 | BC: macOS-14-WebKit-Support entfernt. Neu: `Page.Screencast`-API. Chromium 147. | Nein (Windows-Runner, kein WebKit, Screencast nicht genutzt) |
| 1.60.0 | BC: Docker-Image auf .NET SDK 10 (Repo nutzt kein Playwright-Docker-Image); `Handle`-Option auf `ExposeBindingAsync` entfernt. Chromium 148. | Nein — `ExposeBinding`: 0 Treffer; CI nutzt `windows-latest` + `setup-dotnet` |
| 1.61.0 | Keine BC. Neu: WebAuthn-`Credentials`, `WebStorage`-API, `APIResponse.SecurityDetailsAsync`/`ServerAddrAsync`, `ArtifactsDir` (CDP), `Cursor` (Screencast), Ubuntu-26.04-Support, WS-Requests in HAR/Trace. | Nein |
| 1.62.0 | Keine BC. Announcements: Debian 11 nicht mehr unterstützt; **Clipboard im Headless-Modus vom OS isoliert**. Neu: WebP-Screenshots, `Scroll`-Option, `Locator.WaitForFunctionAsync`, `APIResponse.TimingAsync`. Chromium 151. | Nein — `clipboard`/`Clipboard`: 0 Treffer |
| 1.63.0 | Keine BC. Announcements: Ubuntu 20.04 nicht mehr unterstützt; Linux-arm64 nutzt Chrome for Testing. Neu: `FrameLocator()` ohne Selektor (über Frames suchen), `Locator.Visible`, `AriaSnapshots`/`ScreenSnapshots`-Tracing-Optionen, `HttpCredentialsList` (Array), `Opfs` (StorageState), `DialogClosed`-Events, `install --no-remove`, `codegen --http-credentials`. Chromium 153. | Nein |
| 1.64.0 | BC: Elemente in **versteckten iframes** (z. B. `visibility: hidden`) gelten als hidden — betrifft Actions, `IsVisibleAsync`, `ToBeVisibleAsync`. Chromium 156. | Nein — `iframe`/`IFrame`/`FrameLocator`: 0 Treffer |
| 1.64.1 | Nur `chore(driver): bump bundled Node.js to 24.22.1`. | Nein |
| 1.65.0 | BC (5 Punkte): siehe Detail unten. Announcement: gebündeltes **Node.js 26** statt 24 (auf Linux braucht Node 26 `libatomic1` — Windows-CI nicht betroffen). Chromium 158. | Detailprüfung unten |
| 1.65.1 | Nur `chore(driver): roll to 1.65.1`. Chromium 159.0.8154.6. | Nein |

## Detailprüfung 1.65.0 Breaking Changes gegen Repo

1. **Locator-Aktionen warten auf nicht-inerte Elemente** (`ClickAsync`, `CheckAsync`, `TapAsync`, `FillAsync`, `ClearAsync`, `SelectOptionAsync` warten auf *not inert*; Timeout wenn Element dauerhaft `inert`).
   - Repo-Befund: `inert`-Attribut kommt im App-Code **nicht** vor (`grep -rn inert src/` → nur Doc-Kommentar-/Binär-Fehl-Positive). Dialoge sind eigene Blazor-Overlays ohne `inert`. → Kein Code-Impact; Laufzeitverifikation über OsInterface-Lane.
2. **Cookie-Matching nach RFC 6265** (`BrowserContext.CookiesAsync`, `APIRequestContext.CookiesAsync`; Host-only-Cookies, Path-Matching, namenlose Cookies).
   - Repo-Befund: `Cookie`: 0 Treffer in `src/Schnittstellenzentrale.Tests/`, App setzt/liest keine Cookies. → Kein Impact.
3. **Ungerenderter Text fällt aus Accessible Names / Aria-Snapshots** (Text ohne Slot-Zuweisung, Text in geschlossenem `<details>`; betrifft `GetByRole` mit `Name`, `AriaSnapshotAsync`, `ToMatchAriaSnapshotAsync`).
   - Repo-Befund: `AriaSnapshot`/`ToMatchAriaSnapshot`: 0 Treffer. `GetByRole` mit `Name`: ~60 Vorkommen — alle matchen gerenderten Button-/Link-Text (z. B. „Speichern", „Löschen"). Einziges `<details>`: `ActivityLogPanel.razor:148` — kein Test adressiert dessen Inhalt per Role-Name (nur Panel-Sichtbarkeit in `LayoutSmokeTests`). → Kein Code-Impact erwartet; Restrisiko über Lane-Lauf gedeckt.
4. **`ParamsString` wird angehängt statt ersetzt** (`APIRequestContext`-Requests).
   - Repo-Befund: `APIRequestContext`/`ParamsString`: 0 Treffer. → Kein Impact.
5. **Proxy-Credentials nur an Proxy-Challenges** (nicht mehr an Websites).
   - Repo-Befund: Kein Playwright-`Proxy`-Launch-Option-Einsatz; `UseProxy=false` in `PlaywrightServer.cs:188` betrifft nur den internen Readiness-HttpClient. → Kein Impact.

## API-Nutzungsinventar `src/Schnittstellenzentrale.Tests/Playwright/` (grep-Zählung, Vorkommen)

Locator( 316 · ClickAsync 212 · Expect( 150 · ToBeVisibleAsync 117 · GetByRole 106 · WaitFor 80 · FillAsync 62 · GetByLabel 44 · GetByText 31 · Screenshot 19 · SelectOptionAsync 11 · PressAsync 8 · RouteAsync 1 (SitePageTests.cs:40, Route-Interception mit `route.FulfillAsync` — API unverändert) · EvaluateAsync 4 · SetInputFiles 3 · GetByPlaceholder 5 · HoverAsync 0 · CheckAsync 0 · AriaSnapshot 0 · Cookie(s) 0 · APIRequestContext 0

## Infrastruktur-Dateien

- `Playwright/Infrastructure/PlaywrightTestBase.cs` — `Playwright.CreateAsync()`, `Chromium.LaunchAsync(Headless=TestPaths.IsHeadless())`, `NewContextAsync` (Locale de-DE), `Tracing.StartAsync(Screenshots/Snapshots/Sources)`, `SetDefaultTimeout(30s)`/`SetDefaultNavigationTimeout(60s)`, best-effort-Dispose mit 30-s-Trace-Stop-Budget. Keine betroffene API — Tracing-Start-Optionen unverändert (1.63-Neuerungen optional).
- `Playwright/Infrastructure/PlaywrightServer.cs` — Kestrel 5099, `WebApplicationFactory`-Ableitung (`PlaywrightApiFactory`), Readiness-Poll via HttpClient `UseProxy=false`.
- `Playwright/Infrastructure/PlaywrightApiFactory.cs`, `PlaywrightSignalRServer.cs`, `PlaywrightImpressum{,Language,Fallback}Server.cs`, `PlaywrightODataEndpointServer.cs` — Ports 5100–5104, Collections serialisieren.
- `TestPaths.IsHeadless()` — headless-Steuerung.

## Browser-Installation / CI-Cache

- `InstallPlaywright`-Target im Test-csproj (AfterTargets=Build, abschaltbar via `-p:SkipPlaywrightInstall=true`) führt `pwsh bin/<cfg>/net11.0/playwright.ps1 install chromium` aus → installiert die zum Paket passende Browser-Revision nach `%LOCALAPPDATA%/ms-playwright/`.
- CI (`.github/workflows/pr-staging-ci.yml`, `staging-ci.yml`): Browser-Installation läuft ebenfalls über den Build-Schritt (Kommentar im Workflow verweist explizit darauf). **Kein `actions/cache` für `ms-playwright` konfiguriert** (grep `cache` → nur `cache: npm` in staging-ci Docs-Job). → Es existiert kein CI-Browser-Cache, der invalidiert werden müsste; jeder CI-Build installiert die paketkonforme Revision frisch.
- Lokal sind Alt-Revisionen chromium-1169…1255 vorhanden (`%LOCALAPPDATA%/ms-playwright`); `playwright.ps1 install` entfernt standardmäßig fremde Revisionen anderer Playwright-Installationen nicht automatisch, aber versionierte Verzeichnisse koexistieren — kein Konflikt.

## Flaky-/Retry-Konvention

- Aktuell **kein** Test trägt `[Flaky]` (nur Infrastruktur-Attribute vorhanden).
- Bei Flake-Verdacht: `scripts/Retry-FlakyTests.ps1` (einmaliger gefilterter Re-Run) + `scripts/Analyze-FlakyTestResults.ps1`; `[Flaky]` nur mit dokumentiertem Ereignis vergeben.
