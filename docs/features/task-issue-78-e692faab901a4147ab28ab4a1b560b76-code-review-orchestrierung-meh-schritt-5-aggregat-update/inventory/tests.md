# Tests — Bestandsaufnahme

## Test-Ausgangszustand vor der Umsetzung

- Zeitpunkt (mit Zeitzone): 2026-11-17, ca. 05:55–06:05 UTC
- Branch und Commit-ID: `task/issue-78-e692faab901a4147ab28ab4a1b560b76-code-review-orchestrierung-meh-schritt-5-aggregat-update` @ `2945630b17456d390c96b62caa7524c4854cd6c8`
- Uncommittete Änderungen im getesteten Stand: keine Quellcode-Änderungen (nur neue Lifecycle-Artefakte unter `docs/features/`)
- Testumgebung und Runtime-/SDK-Versionen: .NET SDK 10.0.403, xUnit.net VSTest Adapter v3.1.5, Windows
- Ermittelte Testsuiten und Quellen der Testbefehle: `Schnittstellenzentrale.Tests` (xUnit; Unit-, Komponenten-, Integrations- und Playwright-Tests). Befehl lt. `CLAUDE.md`: `dotnet test --filter "FullyQualifiedName!~Playwright"`. Playwright läuft über `PlaywrightServer` (Kestrel, Port 5099).

### Testläufe

| Lauf | Befehl inkl. Filter | Arbeitsverzeichnis | Exit-Code | Erfolgreich | Fehlgeschlagen | Übersprungen | Nachweis |
|------|--------------------|--------------------|-----------|-------------|----------------|--------------|----------|
| Baseline | `dotnet test --filter "FullyQualifiedName!~Playwright" --logger "console;verbosity=normal"` | Repo-Root | 0 | 588 | 0 | 0 | [baseline-dotnet-test.log](test-results/baseline-dotnet-test.log) |

### Nachgewiesene bestehende Testfehler

Keine — der Baseline-Lauf (588 Tests, ohne Playwright) war vollständig erfolgreich. Playwright-Tests wurden nicht ausgeführt (projektüblicher Filter); über sie wird keine Aussage getroffen. Bekannter pre-existierender Playwright-Flake laut Aufgabenkontext: `ODataEndpointExecutionTests.OdataApi_GetEndpoints_MitContainsFilter` (Strict-Mode-Verletzung, diff-unabhängig, isoliert grün) — nicht als Regression werten.

### Testlücken und Ausführungsprobleme

- Playwright-Suite wurde im Baseline-Lauf nicht ausgeführt (projektüblicher Filter `!~Playwright`).
- Keine weiteren Ausführungsprobleme.

## Testklassen

### `EndpointRepositoryIntegrationTests` (`src/Schnittstellenzentrale.Tests/Integration/EndpointRepositoryIntegrationTests.cs`)
- `AddThenUpdate_WithDifferentInstance_DoesNotThrowTrackingConflict` — Update mit anderer Instanz
- `AddThenUpdate_EndpointGroup_WithDifferentInstance_DoesNotThrowTrackingConflict`
- `SaveEndpoint_ConcurrentWrite_DetectsConflict` — `DbUpdateConcurrencyException` bei veralteter `RowVersion`
- `UpdateEndpoint_WithApplicationIncluded_CalledTwiceWithDifferentInstances_DoesNotThrowTrackingConflict`
- `DeleteEndpointGroup_WithEndpoints_CascadesDelete` / `DeleteEndpointGroup_WithoutEndpoints_DeletesGroup`
- **Kein Test, der die Kinder-Vollmengen-Ersetzung in `UpdateEndpointAsync` explizit prüft (Skalare + Kinder in einem Schritt).**

### `ApplicationApiClientIntegrationTests` (`src/Schnittstellenzentrale.Tests/Integration/ApplicationApiClientIntegrationTests.cs`)
- REST-Tests gegen `WebApplicationFactory`: `POST/GET/PUT/DELETE /api/endpoints`, `/api/endpoints/headers`, `/api/endpoints/query-parameters` (u. a. `PUT` um Z.400 mit `UpdateEndpointRequest` ohne Kinder)
- **Kein Test, der `PUT /api/endpoints/{id}` mit mitgelieferten `headers`/`queryParameters` (Vollmengen-Ersetzung) prüft.**

### `ODataControllerIntegrationTests` (`src/Schnittstellenzentrale.Tests/Integration/ODataControllerIntegrationTests.cs`, 1843 Zeilen)
- `PutEndpoint_*`: `WithEmptyRowVersion_Returns400`, `WithSystemApplication_Returns403`, `MoveToGroupOfSystemApplication_Returns403`, `WithEndpointGroupFromDifferentApplication_Returns400`, `WithEndpointGroupFromSameApplication_Returns200`, `WithRowVersion_AppliesConcurrencyCheck`
- `PatchEndpoint_*`: `WithoutRowVersion_Returns400`, `WithNonNumericEndpointGroupId_Returns400`, `WithEndpointGroupFromDifferent/SameApplication_*`, `BodyMode_AppliesChange`, `MoveToSystemGroupSameApp_Returns403`
- **Keine Tests zu `headers`/`queryparameters` in OData Put/Patch.**

### `EndpointPageTests` (`src/Schnittstellenzentrale.Tests/Components/EndpointPageTests.cs`)
- bUnit-Komponententests; verifiziert `UpdateEndpointAsync`-Aufruf, kein Verify der Einzel-Kind-API-Aufrufe (`AddHeaderAsync` o. ä. wird nicht abgesichert)

### `ApplicationApiClientTests` (`src/Schnittstellenzentrale.Tests/Services/ApplicationApiClientTests.cs`)
- Unit-Tests mit gemocktem `HttpMessageHandler` für den API-Client

### `SwaggerImportServiceTests` / `ODataImportServiceTests` / `ODataImportServiceIntegrationTests`
- Diff- und Apply-Verhalten der Import-Pfade

## Hilfsmethoden

### `TestHelpers` (`src/Schnittstellenzentrale.Tests/Helpers/`)
- `CreateInMemoryDbContext` — `(IDbContextFactory, SqliteConnection)` für In-Memory-SQLite-Tests der Repositories

### `WebApplicationFactory`-Infrastruktur
- Integrationstests nutzen `WebApplicationFactory<Program>`-basierte Fixtures mit Token-Erzeugung (`CreateRequest`-Hilfsmethode, Bearer-Token, optional `X-Storage-Mode`)
