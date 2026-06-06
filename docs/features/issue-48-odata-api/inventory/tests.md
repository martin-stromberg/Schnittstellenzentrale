# Tests

## Testklassen

### `ODataImportServiceTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/ODataImportServiceTests.cs`

Unit-Tests für `ODataImportService` mit gemocktem `HttpMessageHandler`.

- `Import_NewODataMetadata_ReturnsCorrectDiff` — Prüft, dass bei leerem Bestand mindestens ein neuer Endpunkt im Diff erscheint
- `Import_ChangedODataMetadata_ReturnsChangedInDiff` — Prüft, dass ein vorhandener Endpunkt mit geändertem Namen als `ChangedEndpoints` erscheint
- `Import_RemovedODataEndpoint_ReturnsRemovedInDiff` — Prüft, dass ein im Import nicht mehr vorhandener Endpunkt als `RemovedEndpoints` erscheint

**Fehlende Tests** (analog zu `SwaggerImportServiceTests`):
- HTTP-Fehlerfall (`HttpRequestException` → `ImportDiff.ErrorMessage`)
- Ungültiges XML (`XmlException` → `ImportDiff.ErrorMessage`)
- Leere `InterfaceUrl` → leere `ImportDiff`
- `ApplyDiffAsync` mit `AddEndpointAsync`-, `UpdateEndpointAsync`- und `DeleteEndpointAsync`-Aufrufen

---

### `ODataImportServiceRealMetadataTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/ODataImportServiceRealMetadataTests.cs`

Unit-Tests mit dem realen CSDL-Dokument, das `ODataEdmModelBuilder.Build()` erzeugt.

- `Import_RealMetadata_ReturnsCorrectEntitySetEndpoints` — Prüft, dass aus dem eigenen CSDL-Dokument `GET`- und `POST`-Endpunkte für alle vier Entity-Sets (`Applications`, `ApplicationGroups`, `Endpoints`, `EndpointGroups`) erzeugt werden

---

### `ODataControllerIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ODataControllerIntegrationTests.cs`

Integrationstests für die vier OData-CRUD-Controller via `ControllerTestFactory` (`WebApplicationFactory`).

- `GetMetadata_ReturnsValidCsdl` — CSDL-Antwort enthält alle vier Entity-Set-Namen
- `GetApplications_WithValidToken_Returns200` — Authentifizierter Zugriff liefert 200
- `GetApplications_WithoutToken_Returns401` — Nicht-authentifizierter Zugriff liefert 401
- `PostApplication_WithValidToken_Returns201` — Anlegen liefert 201 mit Location-Header
- `PutApplication_WithValidToken_Returns200` — Vollständige Aktualisierung liefert 200
- `PatchApplication_WithValidToken_Returns200` — Partielle Aktualisierung liefert 200
- `DeleteApplication_WithValidToken_Returns204` — Löschen liefert 204
- `PutApplication_WithSystemApplication_Returns403` — System-Anwendungen können nicht per PUT geändert werden
- `DeleteApplication_WithSystemApplication_Returns403` — System-Anwendungen können nicht gelöscht werden
- `PutEndpoint_WithSystemApplication_Returns403` — Endpunkte von System-Anwendungen sind schreibgeschützt
- `DeleteEndpoint_WithSystemApplication_Returns403` — Endpunkte von System-Anwendungen sind löschgeschützt
- `PostEndpoint_WithSystemApplication_Returns403` — Endpunkte von System-Anwendungen können nicht angelegt werden
- `PutEndpointGroup_WithSystemApplication_Returns403` — Gruppen von System-Anwendungen sind schreibgeschützt
- `DeleteEndpointGroup_WithSystemApplication_Returns403` — Gruppen von System-Anwendungen sind löschgeschützt
- `PutApplication_IsSystem_CannotBeElevatedViaPut` — `IsSystem`-Flag kann nicht über PUT gesetzt werden
- `GetApplications_WithFilter_ReturnsFilteredResult` — OData `$filter`-Unterstützung
- `GetApplications_WithExpand_ReturnsRelatedEntities` — OData `$expand`-Unterstützung
- `GetApplications_WithSelect_ReturnsSelectedFields` — OData `$select`-Unterstützung
- `PatchApplication_WithInvalidBase64IconData_Returns400` — Ungültiges IconData liefert 400
- `PatchApplication_WithValidBase64IconData_Returns200` — Gültiges IconData wird akzeptiert
- `ODataAuthenticate_Get_ReturnsToken` — GET `/odatav4/authenticate` liefert Token
- `ODataAuthenticate_Post_ReturnsToken` — POST `/odatav4/authenticate` liefert Token
- `ODataAuthenticate_TokenCanBeUsedForODataRequests` — Erhaltener Token ist für OData-Requests gültig

**Fehlende Integrationstests** (analog zu Swagger-Import):
- `ODataImportService.ImportAsync` / `ApplyDiffAsync` mit realer Datenbank (`WebApplicationFactory`)

---

### `ODataImportTests` (Playwright)
Datei: `src/Schnittstellenzentrale.Tests/Playwright/ODataImportTests.cs`

End-to-End-Tests via Playwright.

- `ImportOData_RecognizesODataType_AndImportsEndpoints` — Legt OData-Anwendung an, klickt „OData-Import", prüft Vorschau-Dialog mit „GET Applications", übernimmt und verifiziert importierte Endpunkte im Baum
- `ImportOData_CrudOperation_PersistsChange` — Importiert OData-Endpunkte und prüft nach Reload, dass die Endpunkte persistiert wurden

---

### `ImportDiffCalculatorTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/ImportDiffCalculatorTests.cs`

Unit-Tests für `ImportDiffCalculator` (wird von `ODataImportService` aufgerufen).

- `Calculate_WhenPostRequestScriptDiffers_ReturnsChangedEndpoint` — Geändertes `PostRequestScript` → `ChangedEndpoints`
- `Calculate_WhenPreRequestScriptDiffers_ReturnsChangedEndpoint` — Geändertes `PreRequestScript` → `ChangedEndpoints`
- `Calculate_MergedEndpoint_ContainsScriptsFromImport` — Merged Endpunkt enthält Scripts aus Import
- `Calculate_WhenImportedEndpointHasNullScripts_OverwritesExistingScripts` — Null-Scripts aus Import überschreiben vorhandene Scripts

## Hilfsmethoden

### `ODataImportServiceTests`
- `CreateService(string metadata, Mock<IEndpointRepository> repoMock)` — Erstellt einen `ODataImportService` mit gemocktem HTTP-Handler, der das übergebene CSDL-XML zurückgibt

### `ODataImportServiceRealMetadataTests`
- `BuildRealMetadata()` — Erzeugt CSDL-XML aus `ODataEdmModelBuilder.Build()` via `CsdlWriter.TryWriteCsdl`
- `CreateService(string metadata, Mock<IEndpointRepository> repoMock)` — Analog zu `ODataImportServiceTests.CreateService`
