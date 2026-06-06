# Tests

## Testklassen

### `ODataImportServiceTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/ODataImportServiceTests.cs`

Unit-Tests für `ODataImportService`. Alle Tests verwenden einen gemockten `HttpMessageHandler`, der ein synthetisches CSDL-Dokument mit einem Entity-Set `Products` zurückliefert.

- `Import_NewODataMetadata_ReturnsCorrectDiff` — Prüft, dass bei leerer Bestandsliste mindestens ein neuer Endpunkt im Diff erscheint
- `Import_ChangedODataMetadata_ReturnsChangedInDiff` — Prüft, dass ein bestehender Endpunkt mit falschem Namen als geändert erkannt wird
- `Import_RemovedODataEndpoint_ReturnsRemovedInDiff` — Prüft, dass ein nicht mehr im Metadatum vorhandener Endpunkt als zu entfernen markiert wird

Fehlende Tests: Kein Test mit dem realen `$metadata`-Dokument des eigenen OData-Controllers (`ODataImportServiceRealMetadataTests`); keine Tests für OData-Controller-CRUD; kein Integrationstest für `/odatav4/$metadata`.

---

### `ApplicationsControllerIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ApplicationsControllerIntegrationTests.cs`

Integrationstest für `ApplicationsController` via `ControllerTestFactory`. Vorlage für `ODataControllerIntegrationTests`.

- `PostApplication_WithValidTokenAndRequest_Returns201AndLocation`
- `PostApplication_WithoutToken_Returns401`
- `PostApplication_WithMissingName_Returns400`
- `PostApplication_WithMissingBaseUrl_Returns400`
- `GetApplications_WithValidToken_Returns200WithList`
- `GetApplications_WithoutToken_Returns401`
- `GetUngroupedApplications_WithValidToken_Returns200WithList`
- `GetApplicationById_WithValidId_Returns200WithAllFields`
- `GetApplicationById_WithInvalidId_Returns404`
- `PutApplication_WithValidRequest_Returns200AndRotatesToken`
- `PutApplication_WithInvalidId_Returns404`
- `PutApplication_WithMissingBaseUrl_Returns400`
- `DeleteApplication_WithValidId_Returns204AndRotatesToken`
- `DeleteApplication_WithInvalidId_Returns404`
- `DeleteApplication_WithSystemApplication_Returns403`
- `PutApplication_WithSystemApplication_Returns403`

---

### `SwaggerImportTests`
Datei: `src/Schnittstellenzentrale.Tests/Playwright/SwaggerImportTests.cs`

Playwright-Test in `[Collection("Playwright")]`. Vorlage für `ODataImportTests`.

- `ImportSwagger_ImportsEndpointsIntoTree` — Löst den Swagger-Import über die UI aus und verifiziert, dass die Endpunkte im Baum erscheinen

---

## Hilfsmethoden

### `ControllerTestFactory`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/ControllerTestFactory.cs`

`WebApplicationFactory<Program>` für Controller-Integrationstests. Ersetzt Authentifizierung durch `TestAuthHandler`, nutzt SQLite In-Memory, entfernt `IHostedService`-Registrierungen.

- `ObtainTokenAsync(HttpClient)` — Ruft `POST /authenticate` auf und gibt den Bearer-Token zurück
- `TokenLifetime` — Eigenschaft zum Überschreiben der Token-Lebenszeit in Tests

Wird für alle bestehenden Controller-Integrationstests verwendet und ist der Einstiegspunkt für `ODataControllerIntegrationTests`.

---

### `PlaywrightServer`
Datei: `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightServer.cs`

Startet einen echten Kestrel-Server auf Port 5099. Konfiguriert `ISwaggerImportService` als Mock (gibt einen synthetischen Diff zurück). `IODataImportService` wird **nicht** gemockt — die Implementierung läuft über den `inProcessHandler` und kann die In-Process-URL `/odatav4/$metadata` direkt aufrufen, sobald der OData-Controller registriert ist.

- `ConfigureTestServices(IServiceCollection)` — Überschreibbare Methode zum Hinzufügen weiterer Service-Overrides in Unterklassen
- `OnAfterStartAsync()` — Hook nach dem Serverstart

---

### `TestMockFactory`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/TestMockFactory.cs`

- `CreateActivityLogServiceMock()` — Leerer `IActivityLogService`-Mock
- `CreateEnv(int id, string name)` — `SystemEnvironment`-Testinstanz
- `CreateFakeLocalizer()` — Passthrough-Localizer (gibt Schlüssel als Wert zurück)

---

### `PlaywrightApiFactory`
Datei: `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightApiFactory.cs`

In-Process-`WebApplicationFactory` für API-Calls aus dem Kestrel-Server. Enthält `PermissiveTokenStore`, der jeden Bearer-Token als gültig akzeptiert.
