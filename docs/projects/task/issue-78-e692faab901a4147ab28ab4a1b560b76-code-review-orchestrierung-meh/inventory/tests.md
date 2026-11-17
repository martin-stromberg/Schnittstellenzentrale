# Tests und Test-Ausgangszustand

## Test-Ausgangszustand vor der Umsetzung

- Zeitpunkt (mit Zeitzone): 2026-11-17, ca. 05:34–05:40 Uhr CET (UTC+01:00, „W. Europe Standard Time")
- Branch und Commit-ID: `task/issue-78-e692faab901a4147ab28ab4a1b560b76-code-review-orchestrierung-meh` @ `7d773019f85af0e2db0b250a3310306962feb213`
- Uncommittete Änderungen im getesteten Stand: staged `docs/projects/task/issue-78-e692faab901a4147ab28ab4a1b560b76-code-review-orchestrierung-meh/requirement.md` und `todo.md` (beides neue Dokumentationsdateien, ohne Einfluss auf Produktivcode oder Tests); keine weiteren Änderungen
- Testumgebung und Runtime-/SDK-Versionen: Windows, .NET SDK 10.0.403 (`global.json` verlangt 10.0.301, rollForward latestFeature), Test-Runtime .NET 10.0.15, xUnit.net VSTest Adapter 3.1.5, Playwright 1.52.0 mit lokal installierten Chromium-Browsern (`%LOCALAPPDATA%\ms-playwright`), PowerShell 7.6.10
- Build-Konfiguration: `Debug` (lokaler Default; CI verwendet `Release` — Abweichung beachten)
- Ermittelte Testsuiten und Quellen der Testbefehle:
  - Unit-/Integrationstests: `dotnet test --filter "FullyQualifiedName!~Playwright"` — Quelle: `CLAUDE.md` („Unit-Tests") und `.github/workflows/pr-staging-ci.yml` Zeile 144 (dort zusätzlich `--configuration Release` + Coverage); blockierendes Quality-Gate
  - Playwright-E2E: `dotnet test --filter "FullyQualifiedName~Playwright"` — Quelle: `pr-staging-ci.yml` Zeile 156 (`continue-on-error: true`, best-effort); `PlaywrightServer` (Kestrel, Port 5099) startet die App in-process — kein manueller App-Start nötig (`CLAUDE.md`)
  - `-p:SkipPlaywrightInstall=true` wurde ergänzt, um das `InstallPlaywright`-MSBuild-Target (csproj Z. 39–41) zu überspringen — Browser waren bereits installiert; keine Änderung an Tests/Code

### Testläufe

| Lauf | Befehl inkl. Filter | Arbeitsverzeichnis | Exit-Code | Erfolgreich | Fehlgeschlagen | Übersprungen | Nachweis |
|------|--------------------|--------------------|-----------|-------------|----------------|--------------|----------|
| Baseline Unit/Integration (Debug) | `dotnet test Schnittstellenzentrale.slnx --filter "FullyQualifiedName!~Playwright" -p:SkipPlaywrightInstall=true --logger "trx;LogFileName=inventory-baseline-unit.trx"` | Repo-Root | 0 | 565 | 0 | 0 | [Console-Log](test-results/baseline-unit-console.log), [TRX](test-results/inventory-baseline-unit.trx) |
| Baseline Playwright (Debug) | `dotnet test Schnittstellenzentrale.slnx --filter "FullyQualifiedName~Playwright" -p:SkipPlaywrightInstall=true --no-restore --logger "trx;LogFileName=inventory-baseline-playwright.trx"` | Repo-Root | 0 | 64 | 0 | 0 | [Console-Log](test-results/baseline-playwright-console.log), [TRX](test-results/inventory-baseline-playwright.trx) |

### Nachgewiesene bestehende Testfehler

Keine — beide Suiten liefen vollständig grün (TRX: 565/565 bzw. 64/64 `outcome="Passed"`, keine `failed`-/`skipped`-Einträge).

### Testlücken und Ausführungsprobleme

- Keine nicht ausgeführten, übersprungenen oder abgebrochenen Tests; keine Build-/Setup-/Infrastrukturfehler.
- Einschränkung: Der Lauf erfolgte in `Debug`-Konfiguration; das CI-Gate verwendet `Release`. Die Befehle unterscheiden sich zudem um Coverage-/Logger-Optionen — für spätere „preexisting"-Vergleiche gilt dieser `Debug`-Lauf als Referenz.
- Die Playwright-Suite ist in CI ausdrücklich `continue-on-error` (best-effort) — lokaler Grün-Status hier dokumentiert.

## Testklassen (relevant für die Anforderung)

### `SystemEndpointSyncServiceTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/SystemEndpointSyncServiceTests.cs`

- `ExecuteAsync_NewEndpoints_AreAdded` — verifiziert `ApplyDiffAsync` mit 2 `NewEndpoints`
- `ExecuteAsync_RemovedEndpoints_AreDeleted` — verifiziert `RemovedEndpoints.Count == 1`
- `ExecuteAsync_ExistingEndpoints_AreLeftUntouched` — leerer Diff bei identischem Schlüssel
- `ExecuteAsync_WhenSwaggerProviderThrows_LogsErrorAndDoesNotThrow` — Fehler wird geloggt, `ApplyDiffAsync` nie aufgerufen
- `ExecuteAsync_WhenApplyDiffThrows_DoesNotThrow` — Ausnahme schluckt der Service
- `ExecuteAsync_IsIdempotent_OnRepeatedCall` — idempotent
- `ExecuteAsync_WhenSystemGroupMissing_LogsWarningAndSkips` / `ExecuteAsync_WhenSystemAppMissing_LogsWarningAndSkips` — Warnung + Abbruch
- `ExecuteAsync_WithNegotiateSecurityScheme_SetsNegotiateAuthenticationType` / `…_WithoutNegotiateSecurityScheme_SetsNoneAuthenticationType` — Auth-Mapping
- `ExecuteAsync_WithBearerTokenExtension_IncludesBearerTokenInDiff` — `x-sz-bearer-token` in `BearerTokens`
- `ExecuteAsync_WithPreRequestScript_…` / `…_WithPostRequestScript_…` — `x-sz-*`-Script-Mapping

Charakterisiert das **Ist-Verhalten** des handgebauten Diffs: Die Tests verifizieren `NewEndpoints`/`RemovedEndpoints` des `ApplyDiffAsync`-Aufrufs — `ChangedEndpoints` wird nirgends geprüft (deckt sich mit Befund aus Issue #78). Hilfsklassen: internes `TestableSyncService` (ruft `ExecuteAsync` auf), `CreateService()` mit Moq-Mocks für `IApplicationRepository`/`ISwaggerProvider`/`IEndpointRepository`/`ISwaggerImportService`.

### `SwaggerImportServiceTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/SwaggerImportServiceTests.cs`

- `Import_*` (10 Tests) — Diff-Berechnung: New/Changed/Removed, `x-sz-*`-Scripts, Bearer-Token, Authenticate-Namensersetzung, Re-Import-Reset
- `ApplyDiff_WithBearerToken_CallsSavePassword` — Credential-Store
- `ApplyDiff_NewEndpoints_GroupsCreatedFromUrlSegments` / `ApplyDiff_PathParameterSegments_AreSkipped` / `ApplyDiff_ExistingGroups_AreReusedAndNotCreatedAgain` — EndpointGroup-Auflösung aus Pfadsegmenten

`SyncFromDocumentAsync` ist nicht vorhanden → keine Tests dafür; Validierung in `ApplyDiffAsync` nicht abgedeckt.

### `ImportDiffCalculatorTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/ImportDiffCalculatorTests.cs` (5 Tests: Script-Diff, Script-Overwrite mit `null`, Serialisierbarkeit ohne zirkuläre Referenz)

### `ApplicationApiClientTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/ApplicationApiClientTests.cs` (gemockter `HttpMessageHandler`)

- Pro Methode ein „sends correct request"-Test inkl. Header-/URL-Verifikation (`DeleteGroupAsync_SendsCorrectDeleteRequest` prüft `DELETE /api/application-groups/{id}` — ohne `content`-Parameter)
- `ImportMetadataAsync_*` (4 Tests) — inkl. 422/401/404 → `ErrorMessage`-Mapping
- **Keine** Tests für `ApplySwaggerDiffAsync` (existiert nicht) bzw. `ApplyODataDiffAsync` (kein Test vorhanden)

### `ODataImportServiceTests` / `ODataImportServiceRealMetadataTests` / `ODataImportServiceIntegrationTests`
Dateien: `src/Schnittstellenzentrale.Tests/Services/ODataImportService{,RealMetadata}Tests.cs`, `Integration/ODataImportServiceIntegrationTests.cs`

- `Import_*` — Diff aus CSDL (EntitySets, Operationen, Annotations `x-sz-*`, Auth-Type, BaseUrl-Auflösung)
- `ApplyDiff_*` — Gruppenbildung pro EntitySet, App-übergreifende Gruppen, Credential-Deduplizierung

### `ApplicationImportControllerIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ApplicationImportControllerIntegrationTests.cs` (`WebApplicationFactory` via `ControllerTestFactory`)

- `Import_*` (6 Tests) — 200+Diff für Rest/OData, 422 `Unknown`, 401 ohne Token, 404 unbekannte App, 422-Body
- `ApplyODataDiff_*` (5 Tests) — 204, 422 Nicht-OData, 401, 404, 204 bei `Application = null`-Navigation
- **Keine** Tests für `swagger-import/apply` (existiert nicht)

### `ApplicationGroupsControllerIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ApplicationGroupsControllerIntegrationTests.cs`

- POST/GET/PUT/DELETE-Basispfade inkl. Token-Rotation (`X-New-Token`), 401/403/404/400
- `DeleteApplicationGroup_WithSystemGroup_Returns403`, `PutApplicationGroup_WithSystemGroup_Returns403`
- **Keine** Tests für `?content=delete|detach` (existiert nicht)

### `ApplicationApiClientIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ApplicationApiClientIntegrationTests.cs`

- EndpointGroups-/Endpoints-/Header-/QueryParameter-CRUD über REST-API (201/401/404/200/204-Pfade); `GetEndpoints_WithGroupId_ReturnsFilteredList`

### `EndpointRepositoryIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/EndpointRepositoryIntegrationTests.cs`

- Tracking-Konflikt-freies Add→Update mit verschiedenen Instanzen (Endpunkt + Gruppe)
- `SaveEndpoint_ConcurrentWrite_DetectsConflict` (RowVersion)
- `DeleteEndpointGroup_WithEndpoints_CascadesDelete` / `…_WithoutEndpoints_DeletesGroup`
- **Keine** Tests für `UpdateAggregateAsync` (existiert nicht); das aktuelle kindersetzende Verhalten von `UpdateEndpointAsync` ist nicht explizit benannt getestet

### `ApplicationRepositoryIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ApplicationRepositoryIntegrationTests.cs`

- `DeleteGroup_SetsMemberApplicationsGroupless` — charakterisiert das `SetNull`-Verhalten („detach")
- `DeleteGroup_WithApplicationsDeletedFirst_RemovesGroupAndApplications` — manuelle Zwei-Schritt-Löschung (App zuerst, dann Gruppe), entspricht der aktuellen UI-Komposition
- StorageMode-/Owner-Filter, RowVersion-Concurrency

### `ODataControllerIntegrationTests` / `ODataControllerNullApplicationTests`
Dateien: `src/Schnittstellenzentrale.Tests/Integration/OData*Tests.cs`

- Metadaten/`$metadata`, Auth-Flow (`ODataAuthenticate_Post_*`)
- CRUD für alle 4 EntitySets inkl. `IsSystem`-/Owner-`403`, `RowVersion`-`400`, Concurrency-`409`, EndpointGroup-Fremd-`400`, Base64-`IconData`-`400`
- **Kein** Test verifiziert Notifications (Factory mockt `ISignalRNotificationService`, verifiziert aber keine Aufrufe) und **kein** Test prüft Kind-Handling in `Put`/`Patch` auf `Endpoints`

### `EndpointPageTests` / `ImportDialogTests` / sonstige `Components/*Tests`
bUnit-Tests für Komponenten-Rendering/State — decken nicht die Notify- oder Repository-Integration ab.

## Hilfsmethoden

### `TestHelpers`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs`

- `CreateInMemoryDbContext()` / `CreateInMemoryDbContextWithHistory()` — `IDbContextFactory<AppDbContext>` auf SQLite In-Memory (`Microsoft.Data.Sqlite`)
- `ExecuteWithTwoContextsAsync` / `ExecuteWithTwoSystemEnvironmentRepositoriesAsync` — Zweikontext-Szenarien für Concurrency-/Tracking-Tests

### `ControllerTestFactory`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/ControllerTestFactory.cs`

`WebApplicationFactory<Program>` mit: Test-Auth-Schema (`TestAuthHandler`), SQLite-In-Memory-DbContextFactory, Entfernen aller `IHostedService` (deaktiviert `SystemEndpointSyncService` in Tests), `ISignalRNotificationService` → `Mock` (Notify wird technisch abgefangen, nicht inhaltlich verifiziert), optionale `TokenLifetime`-Überschreibung, `ObtainTokenAsync` (`POST /authenticate`).

### `TestMockFactory` / `TestAuthHandler` / `CoverageTestFactory`
- `TestMockFactory.CreateActivityLogServiceMock`, `CreateFakeLocalizer`, `CreateEnv` — bUnit-/Service-Testdaten
- `TestAuthHandler` — Auth-Stub für `ControllerTestFactory`

### Playwright-Infrastruktur
Dateien: `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/`

- `PlaywrightServer` — Kestrel-Server (Port 5099), In-Process-API-Handler via `PlaywrightApiFactory`, geteilte SQLite-In-Memory-DB, `TestDatabaseSeeder`
- `PlaywrightSignalRServer` + `SignalRNotificationProxy` — SignalR-Beobachtung für `SignalRSyncTests`
- `PlaywrightTestBase` — Browser-Setup (Chromium)

### Relevante Playwright-Suiten
- `SwaggerImportTests.ImportSwagger_ImportsEndpointsIntoTree` — UI-Importpfad
- `GroupCrudTests` — inkl. `DeleteGroup_Mitloeschen_EntferntGruppeUndAnwendung`, `DeleteGroup_NurSammlungLoeschen_EntferntNurGruppe`, `DeleteGroup_Abbrechen_…` (deckt die aktuelle UI-Komposition ab)
- `EndpointGroupCrudTests` — Ordner-CRUD inkl. Warnhinweis bei Endpunkten
- `SignalRSyncTests.BrowserA_CreatesApp_BrowserB_ReceivesViaSignalR` — Team-Notify-End-to-End
- `LayoutSmokeTests` (6 Tests) — `BoundingBoxAsync`-Layoutprüfung
- `ODataImportTests`, `ODataEndpointExecutionTests`, `StorageModeTests`, `TreeCollapseTests` u. a.
