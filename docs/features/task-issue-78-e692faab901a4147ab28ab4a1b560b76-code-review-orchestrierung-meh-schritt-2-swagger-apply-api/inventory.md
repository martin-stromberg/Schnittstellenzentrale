# Bestandsaufnahme

Stand: Branch `task/…-schritt-2-swagger-apply-api`, ausgehend von Schritt 1
(einheitlicher Swagger-Import-Ausführungspfad, #87 — bereits gemergt auf den
Basisbranch `task/issue-78-…-code-review-orchestrierung-meh`).

## Referenz-Vorbild: OData-Apply-Pfad

### Controller

`src/Schnittstellenzentrale/Controllers/ApplicationImportController.cs`

- Route-Basis: `[Route("api/applications")]`, erbt `ApiControllerBase`.
- `ApplyODataDiffAsync(int id, [FromBody][ValidateNever] ImportDiff diff)` —
  `[HttpPost("{id:int}/odata-import/apply")]` (Z. 42–62):
  1. `ValidateTokenAndSetResponseHeaderAsync()` → `null` ⇒ `Unauthorized()` (401)
  2. `_applicationRepository.GetApplicationByIdAsync(id)` → `null` ⇒ `NotFound()` (404)
  3. `application.InterfaceType != InterfaceType.OData` ⇒
     `UnprocessableEntity(new { errorMessage = "Die Anwendung unterstützt keinen OData-Import." })` (422)
  4. `await _odataImportService.ApplyDiffAsync(diff)` → `NoContent()` (204)
- `ImportAsync` (Z. 71–99) wählt den Import-Service anhand `InterfaceType`:
  `Rest` → `ISwaggerImportService.ImportAsync`, `OData` → `IODataImportService.ImportAsync`,
  sonst 422 `{"errorMessage": "Interface-Typ nicht unterstützt"}`.
- DI: `ITokenStore`, `IApplicationRepository`, `ISwaggerImportService`,
  `IODataImportService` sind bereits im Konstruktor injiziert.

### Token-Validierung

`src/Schnittstellenzentrale/Controllers/ApiControllerBase.cs` —
`ValidateTokenAndSetResponseHeaderAsync()` (Z. 29–42): liest
`Authorization: Bearer <token>`, rotiert über `ITokenStore.ValidateAndRotateAsync`,
schreibt `X-New-Token`-Response-Header. `null` ⇒ ungültig.

## API-Client

`src/Schnittstellenzentrale/Services/ApplicationApiClient.cs` implementiert
`src/Schnittstellenzentrale.Core/Interfaces/IApplicationApiClient.cs`.

- `ApplyODataDiffAsync(int applicationId, ImportDiff diff)` (Z. 437–457):
  `ExecuteWithTokenAsync` + `BuildRequestWithBody(HttpMethod.Post,
  $"{baseUrl}/api/applications/{applicationId}/odata-import/apply", diff, storageMode, t)`;
  bei `!IsSuccessStatusCode` ⇒ `InvalidOperationException(errorBody ?? $"HTTP-Fehler: …")`.
- `TryReadErrorMessageAsync` (Z. 421–434) extrahiert `errorMessage` aus dem JSON-Body.
- `ImportMetadataAsync` (Z. 397–419): gleiche Fehlerbehandlung, gibt `ImportDiff`
  mit `ErrorMessage` zurück statt zu werfen.
- `ExecuteWithTokenAsync` (Z. 497–520): Token aus `ITokenStore`, bei 401 einmaliges
  Neu-Ausstellen und Retry; `X-New-Token`-Header wird übernommen.
- Interface: `Task ApplyODataDiffAsync(int applicationId, ImportDiff diff)`
  (`IApplicationApiClient.cs` Z. 139) — Muster für die neue Methode.

## Swagger-Import-Service

`src/Schnittstellenzentrale.Infrastructure/Services/SwaggerImportService.cs`,
Interface `src/Schnittstellenzentrale.Core/Interfaces/ISwaggerImportService.cs`.

- `Task ApplyDiffAsync(ImportDiff diff)` (Interface Z. 22; Impl Z. 103–134):
  - Guard: `diff.ErrorMessage` gesetzt ⇒ Warnung + Return, nichts wird angewendet.
  - `NewEndpoints`: `GetEndpointGroupsAsync(endpoint.ApplicationId)` → `groupLookup`,
    `EndpointGroupHelper.ResolveGroupIdAsync`, `AddEndpointAsync`,
    `SaveBearerTokenIfPresent`.
  - `ChangedEndpoints`: `UpdateEndpointAsync` + Token-Ablage.
  - `RemovedEndpoints`: `DeleteEndpointAsync(endpoint.Id)`.
  - **Keine** Prüfung der `Endpoint.ApplicationId` gegen eine Zielanwendung —
    Aggregat-Validierung fehlt vollständig.
- `SyncFromDocumentAsync(OpenApiDocument, int applicationId)` (Z. 64–79) ruft
  `ApplyDiffAsync(diff)` — hat die Ziel-ID bereits.
- Weitere Aufrufer von `ApplyDiffAsync`: nur `SwaggerImportDialog.razor` (UI)
  und Tests `SwaggerImportServiceTests.cs` (5 Aufrufe, alle Endpunkte mit
  `ApplicationId = 1`).
- `Endpoint.ApplicationId` ist `int` (nicht nullable), `Endpoint.cs` Z. 30.
- `InterfaceType`: `Unknown = 0`, `Rest = 1`, `OData = 2`.

## UI-Dialoge

`src/Schnittstellenzentrale/Components/Shared/`

- `SwaggerImportDialog.razor` (15 Zeilen): injiziert **`ISwaggerImportService`
  direkt** (Verstoß gegen API-First), `ApplyAsync` →
  `SwaggerImportService.ApplyDiffAsync(diff)`.
- `ODataImportDialog.razor`: Vorbild — injiziert `IApplicationApiClient`,
  `ApplyAsync` → `ApiClient.ApplyODataDiffAsync(Application.Id, diff)`.
- `ImportDialog.razor`: generischer Dialog; `ApplyAsync` (Z. 96–120) baut
  `selectedDiff` aus Checkbox-Auswahl, ruft `OnApply`-Callback; fängt
  `DbUpdateConcurrencyException` (`ImportDialog_Error_Concurrency`) und generische
  `Exception` (`ImportDialog_Error_Apply` mit `ex.Message`) — Fehlermeldungen aus dem
  API-Pfad werden also bereits verständlich angezeigt. Keine neuen resx-Schlüssel
  nötig, solange keine neuen Texte entstehen.
- `ApplicationContentView.razor` (Z. 136–160): `OpenSwaggerImportAsync`/
  `OpenODataImportAsync` rufen bereits `ApplicationApiClient.ImportMetadataAsync`;
  rendert `<SwaggerImportDialog Diff="_swaggerDiff" Application="Application"
  OnClose="CloseSwaggerImport" />` (Z. 45–47).

## Tests

- `src/Schnittstellenzentrale.Tests/Services/ApplicationApiClientTests.cs` —
  xUnit + Moq (`HttpMessageHandler` via `Moq.Protected`), Hilfsmethode `CreateClient`
  (Z. 26–71) mit gemocktem `ITokenStore`/`IConfiguration`/`IStorageModeService`/
  `ICurrentUserService`. Kein Test für `ApplyODataDiffAsync` vorhanden.
- `src/Schnittstellenzentrale.Tests/Integration/ApplicationImportControllerIntegrationTests.cs`
  — `IClassFixture<ApplicationImportControllerTestFactory>`; die Factory
  (`ControllerTestFactory`-Ableitung, Z. 278–317) ersetzt `ISwaggerImportService` und
  `IODataImportService` durch Mocks (`SwaggerImportMock`, `ODataImportMock`);
  `ObtainTokenAsync(client)` besorgt ein echtes Token. OData-Apply-Tests (Z. 159–274)
  sind direkte Vorlage: 204+Verify, 422, 401, 404, 204 mit Nav-Properties.
- `src/Schnittstellenzentrale.Tests/Services/SwaggerImportServiceTests.cs` —
  `CreateService(swaggerJson, repoMock, credentialMock?)`; `ApplyDiffAsync`-Tests
  Z. 290–447 (Bearer-Token, Gruppenanlage, bestehende Gruppen, ErrorMessage-Guard).
- `src/Schnittstellenzentrale.Tests/Components/ImportDialogTests.cs` — bUnit;
  registriert `Mock<IApplicationApiClient>`; testet ODataImportDialog-Fehleranzeige
  und BearerToken-Weitergabe.
- Playwright: `src/Schnittstellenzentrale.Tests/Playwright/SwaggerImportTests.cs`
  — `ImportSwagger_ImportsEndpointsIntoTree` spielt den kompletten UI-Fluss
  (Swagger-Import öffnen → Übernehmen → Endpunkte im Baum) gegen `PlaywrightServer`
  (Kestrel, Port 5099) — deckt den geänderten UI-Aufrufpfad real ab.

## Dokumentation / Lokalisierung

- `docs/help/api/api.md`: Endpunkt-Doku mit einheitlichem Schema
  (Authentifizierung, Header, Pfadparameter, Body, Response, Fehler-Tabelle);
  `POST /api/applications/{id}/odata-import/apply` (Z. 637–703) ist die direkte Vorlage.
- `docs/help/endpunkte/ablauf-technisch.md` Z. 279 und 407 beschreiben den alten
  Direktaufruf (`UI ruft ISwaggerImportService.ApplyDiffAsync(diff)` bzw.
  `ODataImportDialog → IODataImportService.ApplyDiffAsync`) — teils schon stale
  (ODataDialog ruft heute ApiClient), bei Schritt 12 nachziehen.
- `docs/help/schnittstellenzentrale/ablauf-technisch.md` Z. 40 beschreibt
  `SyncFromDocumentAsync`/`ApplyDiffAsync` — Signaturänderung ggf. nachziehen.
- `src/Schnittstellenzentrale/Resources/SharedResources(.de).resx`:
  `SwaggerImportDialog_Title` vorhanden (Z. 850–853); `ImportDialog_*`-Schlüssel
  inkl. `ImportDialog_Error_Apply` vorhanden.
- `docs/RELEASE_NOTES.md` existiert im Repo **nicht** (Releases laufen über
  `release.config.cjs`/semantic-release). Schritt 12c entfällt faktisch.
- README.md erwähnt den OData-Apply-Endpunkt im Feature-Block — für den neuen
  Endpunkt ggf. eine Zeile ergänzen (Schritt 12b).

## Nebenbedingungen

- `POST {id:int}`-Routing — gleiche Constraint-Syntax wie OData-Apply verwenden.
- `[ValidateNever]` am `ImportDiff`-Parameter wird im OData-Vorbild genutzt, um die
  rekursive Modellvalidierung (Navigation `Endpoint.Application` etc.) zu umgehen —
  für Swagger-Apply identisch nötig.
- Fehlertexte der API sind Deutsch (Vorbild OData: „Die Anwendung unterstützt
  keinen OData-Import.") — API-Fehlertexte werden **nicht** über resx lokalisiert.
