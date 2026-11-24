# Detail: Trait-Infrastruktur (`src/Schnittstellenzentrale.Tests/Infrastructure/Testing/`)

## Komponenten

- **`TestCategories`**: Konstanten `OsInterface`, `Flaky`.
- **`CategoryTraitDiscoverer : ITraitDiscoverer`**: liest die `Category`-Property
  des Attributs via `IAttributeInfo.GetNamedArgument<string>("Category")` und
  emittiert `KeyValuePair("Category", wert)` — xunit-v2-Extensibilität
  (`Xunit.Abstractions`/`Xunit.Sdk`).
- **`OsInterfaceAttribute`**: `[TraitDiscoverer("...CategoryTraitDiscoverer", "Schnittstellenzentrale.Tests")]`,
  `AttributeTargets.Class | Method`, implementiert `ITraitAttribute`
  (memberloses Marker-Interface, ohne das xunit 2.9.3 den Discoverer nie aufruft).
- **`OsInterfaceFactAttribute`/`OsInterfaceTheoryAttribute`**: Fact/Theory +
  `ITraitAttribute` für Methoden (aktuell ungenutzt außerhalb der Infrastruktur).
- **`FlakyAttribute`**: gleiche Mechanik für `Category=Flaky`.
- **`TestTraitReader`**: liest Traits aus Reflection (für Paritäts-/Konventionstests).
- **`TraitParityTests` / `FlakyConventionTests`**: reguläre-Lane-Tests, die die
  Markierungskonventionen erzwingen.

## Markierte Testklassen (25)

Playwright (23): ApplicationCrudTests, CoverageGapPlaywrightTests,
EndpointExecutionTests, EndpointGroupCrudTests, EnvironmentManagementTests,
GroupCrudTests, HealthCheckTests, HomePageTests, IconUploadTests,
ImpressumPageTests, ImpressumPageWithFileTests, ImpressumPageWithLanguageTests,
InplaceEditingTests, LayoutSmokeTests, NavigationTests,
ODataEndpointExecutionTests, ODataImportTests, SignalRSyncTests, SitePageTests,
SiteScreenshotTests, StorageModeTests, SwaggerImportTests, TreeCollapseTests

Nicht-Playwright (2): Helpers/TempDirectoryCleanupTests, Services/ImpressumServiceTests

## CI-Verdrahtung

- Reguläre (blockierende) Lane: `dotnet test ... --filter "Category!=OsInterface"
  --collect:"XPlat Code Coverage"` + Coverage-Threshold `COVERAGE_THRESHOLD` (70 %).
- Best-effort-Lane `os-interface-tests`: `--filter "Category=OsInterface"`,
  kein Coverage.
