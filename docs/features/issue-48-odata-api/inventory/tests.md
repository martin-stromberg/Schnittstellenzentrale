# Tests

## Testklassen

### `ODataImportTests`
Datei: `src/Schnittstellenzentrale.Tests/Playwright/ODataImportTests.cs`

Playwright-End-to-End-Tests. Beide Testmethoden navigieren zur Detailansicht einer neu erstellten OData-Anwendung und klicken auf den Button `"OData-Import"`. Sie setzen damit voraus, dass dieser Button in der aktuell angezeigten Ansicht sichtbar ist.

| Testmethode | Was wird getestet |
|-------------|-------------------|
| `ImportOData_RecognizesODataType_AndImportsEndpoints` | Erstellt eine OData-Anwendung, öffnet die Detailansicht, klickt den OData-Import-Button, prüft den Dialog-Titel und das Import-Ergebnis (`GET Applications`), wendet den Import an und prüft den Baum-Eintrag |
| `ImportOData_CrudOperation_PersistsChange` | Erstellt eine OData-Anwendung, führt den Import durch, navigiert neu und prüft, dass der importierte Endpunkt persistent gespeichert wurde |

**Wichtig:** Beide Tests suchen den Button per `GetByRole(AriaRole.Button, new() { Name = "OData-Import" })`. Da `ApplicationContentView` diesen Button noch nicht enthält, schlagen diese Tests aktuell fehl — bzw. sie treffen (sofern `ApplicationCard` noch im DOM sichtbar ist) die alte Komponente.

---

### `ODataImportServiceTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/ODataImportServiceTests.cs`

Unit-Tests für `ODataImportService` mit gemocktem `HttpMessageHandler`.

| Testmethode | Was wird getestet |
|-------------|-------------------|
| `Import_NewODataMetadata_ReturnsCorrectDiff` | Vollständiger Diff für neue Metadaten (2 Endpunkte erwartet) |
| `Import_ChangedODataMetadata_ReturnsChangedInDiff` | Geänderter Endpunkt erscheint in `ChangedEndpoints` |
| `Import_RemovedODataEndpoint_ReturnsRemovedInDiff` | Nicht mehr vorhandener Endpunkt erscheint in `RemovedEndpoints` |
| `Import_HttpError_ReturnsErrorMessage` | HTTP-Fehler führt zu `ImportDiff.ErrorMessage != null` |
| `Import_InvalidXml_ReturnsErrorMessage` | Ungültiges XML führt zu `ImportDiff.ErrorMessage != null` |
| `Import_EmptyInterfaceUrl_ReturnsEmptyDiff` | Leere `InterfaceUrl` → leerer Diff ohne Fehler |
| `ApplyDiff_NewChangedRemoved_CallsRepositoryMethods` | `ApplyDiffAsync` ruft `Add`, `Update` und `Delete` auf dem Repository auf |

### Hilfsmethoden

| Hilfsmethode | Beschreibung |
|--------------|--------------|
| `CreateService(string, Mock<IEndpointRepository>)` | Erstellt `ODataImportService` mit gemocktem `HttpMessageHandler`, der das übergebene Metadaten-XML zurückgibt |
| `CreateServiceWithErrorHandler(Mock<IEndpointRepository>)` | Erstellt `ODataImportService` mit einem Handler, der `HttpRequestException` wirft |

---

### `ODataImportServiceIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ODataImportServiceIntegrationTests.cs`

Integrationstest mit `ControllerTestFactory` (WebApplicationFactory) und realer In-Memory-Datenbank.

| Testmethode | Was wird getestet |
|-------------|-------------------|
| `Import_NewODataApplication_PersistsEndpoints` | Erstellt eine Anwendung in der DB, importiert OData-Metadaten, wendet den Diff an und prüft, dass 2 Endpunkte persistiert wurden |

---

## Fehlende Tests (bezogen auf Anforderung)

- Keine Unit-Tests oder Bunit-Tests für `ApplicationContentView` — weder für den OData-Button noch für den Swagger-Button
- Die Playwright-Tests in `ODataImportTests` testen noch gegen die alte `ApplicationCard` (oder fallen fehl), nicht gegen `ApplicationContentView`
