# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### SignalRSyncTests.cs (SignalRSyncTests)

- **Doppelter Code / fehlende Kapselung** — `SignalRSyncTests` implementiert `IAsyncLifetime` direkt und dupliziert dabei die gesamte Playwright-Initialisierungs- und Teardown-Logik aus `PlaywrightTestBase`: Browser-Launch, Tracing-Start/-Stop, `TestDatabaseSeeder`-Aufruf und Ressourcen-Dispose (Zeilen 33–73). Die Klasse nutzt `PlaywrightTestBase` nicht, obwohl diese Basisklasse genau für diesen Zweck existiert.

  Empfehlung: `PlaywrightTestBase` so erweitern, dass sie mehrere Browser-Kontexte pro Test unterstützt (z. B. durch eine geschützte `CreateAdditionalContextAsync()`-Methode), oder die duplizierte Initialisierungslogik in eine gemeinsam genutzte Hilfsklasse auslagern. Alternativ reicht es, `PlaywrightTestBase` so zu erweitern, dass Unterklassen einen zweiten `IBrowserContext` anlegen können, ohne die Basis-Initialisierung zu wiederholen.

### ApplicationCrudTests.cs / EndpointExecutionTests.cs

- **Robustheit — instabiler Selektor** — Der Kontext-Menü-Toggle wird via `appRow.GetByText("⚙️")` gesucht (ApplicationCrudTests.cs Zeilen 43, 69; EndpointExecutionTests.cs Zeile 21). Ein Text-Selektor auf einem Emoji ist fragil: Ändert sich das Icon oder wird es durch ein CSS-Klassen-Icon ersetzt, schlagen die Tests ohne klare Fehlermeldung fehl.

  Empfehlung: Den Toggle über ein stabiles Attribut selektieren, z. B. `data-testid="context-menu-toggle"` oder die CSS-Klasse `.context-menu-toggle` (die in den bUnit-Tests bereits verwendet wird): `appRow.Locator(".context-menu-toggle")`.

### EndpointExecutionTests.cs (EndpointExecutionTests)

- **Fehlerbehandlung / Fehlermeldung** — Zeile 33: `int.Parse(statusText ?? "0")` wirft bei unerwarteten Inhalten (z. B. Leerzeichen, HTML-Fragment) eine `FormatException` statt einer aussagekräftigen Assertion-Fehlermeldung. Der Test würde dann mit einem kryptischen Stack-Trace statt einem Playwright-Assertionsfehler scheitern.

  Empfehlung: Parsen defensiv absichern und mit einer expliziten Assertion scheitern lassen:
  ```csharp
  Assert.True(int.TryParse(statusText?.Trim(), out var statusCode),
      $"Statuscode konnte nicht geparst werden: '{statusText}'");
  Assert.InRange(statusCode, 200, 299);
  ```

## Geprüfte Dateien

- `src/Schnittstellenzentrale.Tests/Components/ApplicationContextMenuTests.cs`
- `src/Schnittstellenzentrale.Tests/Components/EndpointContextMenuTests.cs`
- `src/Schnittstellenzentrale.Tests/Components/EndpointGroupContextMenuTests.cs`
- `src/Schnittstellenzentrale.Tests/Components/EndpointPageTests.cs`
- `src/Schnittstellenzentrale.Tests/Helpers/ControllerTestFactory.cs`
- `src/Schnittstellenzentrale.Tests/Helpers/TestAuthHandler.cs`
- `src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs`
- `src/Schnittstellenzentrale.Tests/Integration/ApplicationGroupsControllerIntegrationTests.cs`
- `src/Schnittstellenzentrale.Tests/Integration/ApplicationRepositoryIntegrationTests.cs`
- `src/Schnittstellenzentrale.Tests/Integration/ApplicationsControllerIntegrationTests.cs`
- `src/Schnittstellenzentrale.Tests/Integration/AuthControllerIntegrationTests.cs`
- `src/Schnittstellenzentrale.Tests/Integration/EndpointRepositoryIntegrationTests.cs`
- `src/Schnittstellenzentrale.Tests/Integration/SystemEntryInitializerTests.cs`
- `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj`
- `src/Schnittstellenzentrale.Tests/Services/ApplicationApiClientTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/DatabaseProviderFactoryTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/EndpointExecutionServiceTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/HealthCheckServiceTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/ODataImportServiceTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/SwaggerImportServiceTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/SystemEndpointSyncServiceTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/ThemeServiceTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/TokenStoreTests.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/HomePageTests.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/ApplicationCrudTests.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/EndpointExecutionTests.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/SwaggerImportTests.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/HealthCheckTests.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/StorageModeTests.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/SignalRSyncTests.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightTestFactory.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightTestBase.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightCollection.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightSignalRFactory.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightSignalRCollection.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/TestDatabaseSeeder.cs`
