# Detailanalyse: Bestehende Tests

## Testklassen

### `ApplicationsControllerIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ApplicationsControllerIntegrationTests.cs`

Verwendet `IClassFixture<ControllerTestFactory>`. Testet HTTP-Endpunkte des `ApplicationsController` via `HttpClient`.

- `PostApplication_WithValidTokenAndRequest_Returns201AndLocation` — POST mit Bearer-Token und `X-Storage-Mode: Team`; prüft HTTP 201, Location-Header, `X-New-Token`-Header und Response-Body
- `PostApplication_WithoutToken_Returns401` — POST ohne Token; prüft HTTP 401

### `ApplicationGroupsControllerIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ApplicationGroupsControllerIntegrationTests.cs`

Testet HTTP-Endpunkte des `ApplicationGroupsController` via `HttpClient`.

### `AuthControllerIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/AuthControllerIntegrationTests.cs`

Testet den `POST /authenticate`-Endpunkt.

### `EndpointExecutionIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/EndpointExecutionIntegrationTests.cs`

Implementiert `IAsyncLifetime`. Verwendet `ControllerTestFactory` mit eigenem Lifecycle (nicht `IClassFixture`). Testet `EndpointExecutionService` gegen den echten Test-Server.

- `ExecuteEndpoint_OwnApiWithBearerToken_ReturnsSuccess` — Legt Endpunkt auf die eigene API an, führt ihn mit Bearer-Token aus und prüft ein positives Ergebnis

### `ApplicationRepositoryIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ApplicationRepositoryIntegrationTests.cs`

Unit-/Integrationstests für `ApplicationRepository` direkt gegen SQLite In-Memory.

### `EndpointRepositoryIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/EndpointRepositoryIntegrationTests.cs`

Unit-/Integrationstests für `EndpointRepository` direkt gegen SQLite In-Memory.

### `SystemEntryInitializerTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/SystemEntryInitializerTests.cs`

Testet `SystemEntryInitializer.InitializeAsync` mit verschiedenen Konfigurationsszenarien.

- `InitializeAsync_WhenGroupAndApplicationMissing_CreatesBoth`
- `InitializeAsync_WhenGroupExistsButApplicationMissing_CreatesApplication`
- `InitializeAsync_WhenUrlDiffers_UpdatesBaseUrlAndInterfaceUrl`
- `InitializeAsync_WhenUrlMatches_MakesNoChanges`
- `InitializeAsync_IsIdempotent_OnRepeatedCall`
- `InitializeAsync_WhenDbThrows_DoesNotPropagateException`
- `InitializeAsync_WhenBaseUrlMissing_SkipsAndLogs`

Enthält innere Klasse `ThrowingApplicationRepository` (Stub, der bei jedem Aufruf wirft) und `CompositeDisposable` als Hilfsklasse.

---

## Service-Tests (Unit)

Datei-Verzeichnis: `src/Schnittstellenzentrale.Tests/Services/`

Vorhandene Testdateien:
- `ApplicationApiClientTests.cs`
- `DatabaseProviderFactoryTests.cs`
- `EndpointExecutionServiceTests.cs`
- `HealthCheckServiceTests.cs`
- `ODataImportServiceTests.cs`
- `SwaggerImportServiceTests.cs`
- `SystemEndpointSyncServiceTests.cs`
- `ThemeServiceTests.cs`
- `TokenStoreTests.cs`

Diese Tests sind reine Unit-Tests und testen Services isoliert. Für die Playwright-Implementierung sind sie als Referenz für Service-Verhalten relevant (z. B. `SwaggerImportService`, `HealthCheckService`).

---

## Hilfsmethoden

### `ControllerTestFactory`
- `ObtainTokenAsync(HttpClient)` — Ruft `POST /authenticate` auf und gibt den Bearer-Token zurück. Wird in Playwright-Tests nicht direkt benötigt, da `TestAuthHandler` den Authenticate-Endpunkt ohne Windows-Credentials bedienbar macht.

### `TestHelpers`
- `CreateInMemoryDbContext()` — Erzeugt SQLite In-Memory `IDbContextFactory<AppDbContext>`. Für Playwright irrelevant (Playwright braucht Datei-SQLite).
- `ExecuteWithTwoContextsAsync(...)` — Zwei `ApplicationRepository`-Instanzen über dieselbe Connection. Muster relevant für `SignalRSyncTests`, aber In-Memory ist für Playwright nicht nutzbar.

---

## Playwright-Tests

Es existieren **keine** Playwright-Testklassen. Das Verzeichnis `src/Schnittstellenzentrale.Tests/` enthält keinen `Playwright`-Unterordner. Weder `PlaywrightTestFactory`, `PlaywrightTestBase` noch `TestDatabaseSeeder` sind vorhanden.

---

## NuGet-Abhängigkeiten (Testprojekt)

Datei: `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj`

Aktuell vorhanden:

| Paket | Version |
|---|---|
| `bunit` | 2.7.2 |
| `coverlet.collector` | 10.0.0 |
| `Microsoft.AspNetCore.Mvc.Testing` | 9.0.* |
| `Microsoft.EntityFrameworkCore.Sqlite` | 9.0.16 |
| `Microsoft.EntityFrameworkCore.SqlServer` | 9.0.16 |
| `Microsoft.JSInterop` | 9.0.16 |
| `Microsoft.NET.Test.Sdk` | 18.5.1 |
| `Moq` | 4.20.72 |
| `xunit` | 2.9.3 |
| `xunit.runner.visualstudio` | 3.1.5 |

**Fehlend:** `Microsoft.Playwright` und ein Playwright xUnit-Adapter sind nicht referenziert.
