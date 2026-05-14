# Tests – Bestandsaufnahme

## Testklassen

Für den Dark-Mode / Theme-Bereich existieren keine Testklassen.

Vorhandene Testklassen im Projekt betreffen:

| Testklasse | Datei |
|------------|-------|
| `DatabaseProviderFactoryTests` | `src/Schnittstellenzentrale.Tests/Services/DatabaseProviderFactoryTests.cs` |
| `EndpointExecutionServiceTests` | `src/Schnittstellenzentrale.Tests/Services/EndpointExecutionServiceTests.cs` |
| `HealthCheckServiceTests` | `src/Schnittstellenzentrale.Tests/Services/HealthCheckServiceTests.cs` |
| `ODataImportServiceTests` | `src/Schnittstellenzentrale.Tests/Services/ODataImportServiceTests.cs` |
| `SwaggerImportServiceTests` | `src/Schnittstellenzentrale.Tests/Services/SwaggerImportServiceTests.cs` |

Keine dieser Klassen deckt `StorageModeService`, `IStorageModeService`, Theme-Logik oder UI-Komponenten ab.

## Hilfsmethoden

### `TestHelpers`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs`

| Hilfsmethode | Zweck |
|--------------|-------|
| `CreateInMemoryDbContext()` | Erstellt einen `AppDbContext` mit SQLite In-Memory-Provider für Integrationstests. Liefert `(AppDbContext, SqliteConnection)`. |

Keine theme-spezifischen Hilfsmethoden vorhanden.
