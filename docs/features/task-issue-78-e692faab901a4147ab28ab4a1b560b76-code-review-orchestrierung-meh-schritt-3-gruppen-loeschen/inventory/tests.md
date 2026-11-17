# Bestandsaufnahme: Tests

## Test-Ausgangszustand vor der Umsetzung

- Zeitpunkt (mit Zeitzone): 2026-11-17, ~06:45 UTC+01:00
- Branch und Commit-ID: `task/issue-78-e692faab901a4147ab28ab4a1b560b76-code-review-orchestrierung-meh-schritt-3-gruppen-loeschen` @ `900f304`
- Uncommittete Änderungen im getesteten Stand: keine (nur Lifecycle-Artefakte unter `docs/features/`, kein Quellcode)
- Testumgebung und Runtime-/SDK-Versionen: .NET SDK 10.0.403, xUnit, SQLite In-Memory, `WebApplicationFactory<Program>` (`ControllerTestFactory`), Moq
- Ermittelte Testsuiten und Quellen der Testbefehle: CLAUDE.md — `dotnet test --filter "FullyQualifiedName!~Playwright"` (Playwright via `PlaywrightServer` Port 5099 separat)

### Testläufe

| Lauf | Befehl inkl. Filter | Arbeitsverzeichnis | Exit-Code | Erfolgreich | Fehlgeschlagen | Übersprungen | Nachweis |
|------|--------------------|--------------------|-----------|-------------|----------------|--------------|----------|
| Baseline | `dotnet test --filter "FullyQualifiedName!~Playwright" --logger "console;verbosity=normal"` | Repo-Wurzel | 0 | 577 | 0 | 0 | [baseline.log](test-results/baseline.log) |

### Nachgewiesene bestehende Testfehler

Keine — der Baseline-Lauf war vollständig erfolgreich (577/577 bestanden).

### Testlücken und Ausführungsprobleme

- Playwright-Suite wurde per Filter ausgeschlossen (projektüblicher Unit-/Integrationstest-Befehl; kein manueller App-Start nötig, `PlaywrightServer` Port 5099).

## Testklassen

### `ApplicationGroupsControllerIntegrationTests` (`src/Schnittstellenzentrale.Tests/Integration/`)
- `DeleteApplicationGroup_WithValidId_Returns204AndRotatesToken` — DELETE ohne `content` → 204 + `X-New-Token`
- `DeleteApplicationGroup_WithInvalidId_Returns404` — 404
- `DeleteApplicationGroup_WithSystemGroup_Returns403` — IsSystem → 403 (seeding via `IApplicationRepository`)
- weitere: POST/GET/PUT-Tests als Muster für Token-Rotation (`X-New-Token` aus Response-Header extrahieren)

### `ApplicationRepositoryIntegrationTests` (`src/Schnittstellenzentrale.Tests/Integration/`)
- `DeleteGroup_SetsMemberApplicationsGroupless` — bisheriges Default-Verhalten: Mitglieder werden ungruppiert
- `DeleteGroup_WithApplicationsDeletedFirst_RemovesGroupAndApplications` — manuelle Mehrschritt-Löschung
- `ExecuteWithContextAsync` / `TestHelpers.ExecuteWithTwoContextsAsync` — Repository-Test-Scaffolding auf SQLite In-Memory

### `ApplicationApiClientTests` (`src/Schnittstellenzentrale.Tests/Services/`)
- `DeleteGroupAsync_SendsCorrectDeleteRequest` — prüft Methode + `AbsolutePath` via Moq `HttpMessageHandler` (MockBehavior.Strict); Query-String wird über `RequestUri` prüfbar sein
- `CreateClient`/`CreateHandlerMock` — Fixture-Muster für Client-Unit-Tests

### `ApplicationApiClientIntegrationTests` (`src/Schnittstellenzentrale.Tests/Integration/`)
- `BuildRequest`-Helfer + `ExtractNewToken` — Muster für Endpunkt-Integrationstests (Endpoint-/EndpointGroup-Routen)

### `SystemEntryInitializerTests` (`src/Schnittstellenzentrale.Tests/Integration/`, Z. 320)
- Enthält Fake-Implementierung von `IApplicationRepository` mit `DeleteGroupAsync(int id)` — Signaturänderung muss hier nachgezogen werden, falls das Interface geändert wird (bei Overload oder Default-Parameter nicht zwingend nötig).

## Hilfsmethoden

### `TestHelpers` (`src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs`)
- `CreateInMemoryDbContext()` — `(IDbContextFactory<AppDbContext>, SqliteConnection)`; SQLite `EnsureCreated` erzeugt FKs mit konfiguriertem `OnDelete`-Verhalten
- `ExecuteWithTwoContextsAsync` — zwei Repos über geteilter Connection (Concurrency-Tests)

### `ControllerTestFactory` (`src/Schnittstellenzentrale.Tests/Helpers/ControllerTestFactory.cs`)
- `ObtainTokenAsync(client)` — Bearer-Token holen
- `Services` — Zugriff auf `IApplicationRepository` zum Seeden (Systemgruppe)
