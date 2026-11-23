# Test-Ausgangszustand und testabdeckende Klassen

## Test-Ausgangszustand vor der Umsetzung

- **Zeitpunkt:** 2026-11-23, ~22:09–22:12 Uhr +01:00 (W. Europe Standard Time)
- **Branch / Commit:** `task/issue-107-1156a1d229fc480b8c0fe11dd1f2c325-review-nuget-paketpruefung-202` @ `5188970306044127368645c79984fad510d38d15`
- **Uncommittete Änderungen im getesteten Stand:** nur untracked `docs/features/task/issue-107-…/` (dieses Feature-Verzeichnis); keine Änderungen an Produktiv- oder Testcode
- **Testumgebung:** Windows, .NET SDK 10.0.404 (`global.json`: 10.0.301 + rollForward latestFeature), NuGet-Quellen nuget.org + lokaler Fallback-Ordner
- **Ermittelte Testsuiten / Quellen der Befehle:** CLAUDE.md + `pr-staging-ci.yml`: reguläre Lane `dotnet test --filter "Category!=OsInterface"` (blockierend); OsInterface-Lane `--filter "Category=OsInterface"` (Playwright, echte Kestrel-Bindung Ports 5099–5104, Temp-FS — in CI best-effort). Trait-Infrastruktur: `src/Schnittstellenzentrale.Tests/Infrastructure/Testing/` (`OsInterfaceAttribute`, `OsInterfaceFactAttribute`, `TestCategories`).

### Testläufe

| Lauf | Befehl inkl. Filter | Arbeitsverzeichnis | Exit-Code | Erfolgreich | Fehlgeschlagen | Übersprungen | Nachweis |
|------|--------------------|--------------------|-----------|-------------|----------------|--------------|----------|
| Baseline (Debug, impliziter Build) | `dotnet test Schnittstellenzentrale.slnx --filter "Category!=OsInterface" --logger "console;verbosity=normal"` | Repo-Root | 0 | 641 | 0 | 0 | [dotnet-test-baseline.log](test-results/dotnet-test-baseline.log) |

Gesamtzeit laut Log: 22,35 s. Hinweis: Der Build triggerte dabei das MSBuild-Target `InstallPlaywright` (`playwright.ps1 install chromium`) — Chromium war bereits installiert bzw. wurde ohne Fehler durchlaufen.

### Nachgewiesene bestehende Testfehler

Keine — im Baseline-Lauf sind alle 641 ausgeführten Tests bestanden.

### Testlücken und Ausführungsprobleme

- **OsInterface-Lane nicht ausgeführt** (`Category=OsInterface`: Playwright-Browsertests, Kestrel-Integrationstests auf Ports 5099–5104, Temp-Verzeichnis-Tests). In der CI ist diese Lane bewusst best-effort (`continue-on-error: true`); für den Update-PR ist sie nicht blockierend. Umfang der nicht ausgeführten Tests: unbekannt (ohne Filterlauf nicht gezählt).
- Anzahl übersprungener Tests im Baseline-Lauf: keine im Log ausgewiesen.

## Testklassen (paketrelevante Auswahl)

Für die NuGet-Prüfung relevant: keine neuen Tests erforderlich — der Nachweis erfolgt über die bestehende Suite. Besonders paketberührte Klassen:

### OData (deckt `Microsoft.AspNetCore.OData` + `Microsoft.OData.*` ab)

- `Integration/ODataControllerIntegrationTests` — 78 Testmethoden: CRUD auf allen vier Entity-Sets, `$metadata`/CSDL, Token-Auth (`authenticate`), `[EnableQuery]`-Filter, Concurrency (RowVersion), 401/403-Pfade
- `Integration/ODataControllerNullApplicationTests` — OData-Verhalten bei null-Anwendung
- `Integration/ODataImportServiceIntegrationTests` + `Services/ODataImportServiceTests` + `Services/ODataImportServiceRealMetadataTests` — `Microsoft.OData.Edm`/`CsdlReader`-Pfad
- `Integration/ODataNotificationIntegrationTests` — OData-CRUD → SignalR-Benachrichtigungen
- Playwright (OsInterface): `ODataEndpointExecutionTests`, `ODataImportTests`

### SignalR (deckt `SignalR.Core`/`SignalR.Client` ab)

- `Integration/ImportApplyNotificationTests`, `Integration/RestNotificationIntegrationTests` — Notification-Pfade
- `Services/NotificationPolicyTests`, `Services/EndpointScriptRunnerTests`, `Services/EndpointExecutionServiceTests` — `ISignalRNotificationService`-Mocks
- Playwright (OsInterface): `SignalRSyncTests` (`PlaywrightSignalRServer` nutzt echten `IHubContext<EndpointHub>`)

### Http.Abstractions / Windows-Auth

- `Services/ApplicationApiClientTests` — `Mock<IHttpContextAccessor>`
- `Integration/AuthControllerIntegrationTests`, `Integration/LocalizationTests`

### Test-Infrastruktur-Pakete (bunit/xunit/Moq/Playwright/coverlet)

- bunit: alle `Components/*Tests.cs` (11 Klassen)
- `Infrastructure/Testing/TraitParityTests`, `FlakyConventionTests` — prüfen die Trait-Konventionen selbst
- `AngleSharp`: nur ungenutzte `using`-Direktive in `Playwright/Infrastructure/PlaywrightServer.cs` (kein API-Aufruf; der direkte Override dient der Pin-Funktion)

## Hilfsmethoden / Test-Infrastruktur

- `Playwright/Infrastructure/PlaywrightServer.cs` — Kestrel-basierter Test-Server (Port 5099), `WebApplicationFactory`-Variante
- `Playwright/Infrastructure/PlaywrightSignalRServer.cs` + `SignalRNotificationProxy.cs` — echter `IHubContext<EndpointHub>` für Sync-Tests
- `Infrastructure/Testing/CategoryTraitDiscoverer.cs`, `OsInterface*Attribute`, `TestCategories` — Trait-System für die zwei CI-Lanes
- `scripts/Retry-FlakyTests.ps1`, `Analyze-FlakyTestResults.ps1`, `New-FlakyQuarantineIssue.ps1` — CI-Flake-Handling (nur OsInterface-Lane)
