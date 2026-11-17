# Umsetzungsplan — Swagger-Import-Apply über validierten REST-Endpunkt

Eingaben: `requirement.md`, `inventory.md`. Issue-Bezug: #88.
Basisbranch für Diffs: `task/issue-78-e692faab901a4147ab28ab4a1b560b76-code-review-orchestrierung-meh`.

## Scope-Klärung

- **Verarbeitungseinheit:** Ein `ImportDiff` (Listen `NewEndpoints`/`ChangedEndpoints`/
  `RemovedEndpoints` + `BearerTokens`) pro HTTP-Request bzw. Dialog-Bestätigung.
- **Auslösende Ereignisse:**
  1. Externer/interner HTTP-Aufruf `POST /api/applications/{id}/swagger-import/apply`.
  2. Klick auf „Übernehmen" im `SwaggerImportDialog` (via `ImportDialog` → `OnApply`).
  3. Interner Aufruf `ISwaggerImportService.ApplyDiffAsync` durch
     `SyncFromDocumentAsync` (Startabgleich der Systemanwendung).
- **Erwartete Wirkung:** Endpunkte des Diffs werden über `IEndpointRepository`
  eingefügt/aktualisiert/gelöscht; Bearer-Tokens landen im Credential Store. Bei
  ungültigem Token/fehlender Anwendung/falschem InterfaceType/Aggregatverletzung ändert
  sich nichts am Bestand.
- **Umgang mit fehlenden Objekten:** Anwendung nicht vorhanden → `404`. Endpunkte des
  Diffs mit abweichender `ApplicationId` → Diff wird komplett verworfen, Fehler statt
  Teilanwendung. Noch nicht vorhandene Endpunkte des Diffs werden angelegt
  (unverändertes Bestandsverhalten).
- **Wiederaufnahme/Async:** Der Apply ist synchron; bei `401` stellt der API-Client
  automatisch ein neues Token aus und wiederholt einmal (Bestandsverhalten von
  `ExecuteWithTokenAsync`).

## Scope-Ausschlüsse

Keine — die Anforderung ist vollständig im Scope.

## Offene Punkte

Keine.

## Design-Entscheidungen

1. **Aggregat-Validierung sitzt im Service:** `ISwaggerImportService.ApplyDiffAsync`
   erhält die Zielanwendungs-ID als neuen Parameter:
   `Task ApplyDiffAsync(ImportDiff diff, int applicationId)`. Der Service prüft vor
   jeder Persistierung, dass alle Endpunkte in den drei Listen
   `ApplicationId == applicationId` haben; Verletzung ⇒ `InvalidOperationException`
   mit verständlicher deutscher Meldung
   („Der Import-Diff enthält Endpunkte, die nicht zur Anwendung {id} gehören."). Der
   `ErrorMessage`-Guard bleibt bestehen und greift weiter zuerst.
   Begründung: Die Anforderung verlangt die Absicherung „der Apply-Logik des
   Swagger-Imports" selbst — damit sind alle Aufrufer (REST-Endpunkt,
   `SyncFromDocumentAsync`, künftige) geschützt, nicht nur der HTTP-Pfad.
2. **Controller mappt die Exception auf `422`:** `ApplySwaggerDiffAsync` fängt
   `InvalidOperationException` aus dem Service und antwortet
   `UnprocessableEntity(new { errorMessage = ex.Message })`. So bleibt der
   Fehlervertrag identisch zum OData-Apply (`{ "errorMessage": … }`) und der Client
   kann die Meldung unverändert durchreichen.
3. **Signatur-Bruch ist bewusst:** Die alte Ein-Parameter-Überladung entfällt —
   interne Schnittstelle, alle Aufrufstellen (`SyncFromDocumentAsync`, Dialog,
   Tests) werden angepasst. Kein Mehr-API zur Kompatibilität nötig.

## Umsetzungsschritte

### S1: Service-Validierung (`Infrastructure` + `Core`-Interface)

- `ISwaggerImportService.ApplyDiffAsync(ImportDiff diff)` →
  `ApplyDiffAsync(ImportDiff diff, int applicationId)` (XML-Doc ergänzen:
  wirft `InvalidOperationException` bei Aggregatverletzung).
- `SwaggerImportService.ApplyDiffAsync`: nach dem `ErrorMessage`-Guard alle
  Endpunkte aus den drei Listen auf `ApplicationId != applicationId` prüfen;
  bei Treffer `InvalidOperationException` werfen (keine Persistierung).
- `SyncFromDocumentAsync`: `ApplyDiffAsync(diff, applicationId)`.

### S2: REST-Endpunkt (`ApplicationImportController`)

- Neues Action `ApplySwaggerDiffAsync(int id, [FromBody][ValidateNever] ImportDiff diff)`:
  - Route `[HttpPost("{id:int}/swagger-import/apply")]`, Response-Typen
    204/401/404/422 dokumentieren.
  - Token → `401`; `GetApplicationByIdAsync` → `404`;
    `InterfaceType != InterfaceType.Rest` → `422`
    `{ errorMessage = "Die Anwendung unterstützt keinen Swagger-Import." }`.
  - `try { await _swaggerImportService.ApplyDiffAsync(diff, id); }`
    `catch (InvalidOperationException ex)` → `422 { errorMessage = ex.Message }`;
    Erfolg → `NoContent()`.

### S3: API-Client

- `IApplicationApiClient`: `Task ApplySwaggerDiffAsync(int applicationId, ImportDiff diff)`.
- `ApplicationApiClient.ApplySwaggerDiffAsync`: analog `ApplyODataDiffAsync` —
  POST `{baseUrl}/api/applications/{applicationId}/swagger-import/apply` via
  `ExecuteWithTokenAsync`/`BuildRequestWithBody`; bei Fehler
  `InvalidOperationException(errorBody ?? $"HTTP-Fehler: …")`.

### S4: Dialog-Umstellung

- `SwaggerImportDialog.razor`: `@inject IApplicationApiClient ApiClient` statt
  `ISwaggerImportService`; `ApplyAsync` →
  `await ApiClient.ApplySwaggerDiffAsync(Application.Id, diff);`
- Fehleranzeige entsteht bereits über `ImportDialog_Error_Apply` (vorhandener
  resx-Schlüssel) — **keine neuen Lokalisierungs-Schlüssel erforderlich**.

### S5: Tests

**Unit-Tests (gemockter `HttpMessageHandler`), `ApplicationApiClientTests.cs`:**

- `ApplySwaggerDiffAsync_SendsCorrectPostRequest` — URL-Pfad
  `/api/applications/{id}/swagger-import/apply`, POST, Bearer-Token + `X-Storage-Mode`
  gesetzt; `204` ⇒ kein Wurf.
- `ApplySwaggerDiffAsync_OnErrorResponse_ThrowsWithServerMessage` — `422` mit
  `{ "errorMessage": "…" }` ⇒ `InvalidOperationException` mit Servermeldung.

**Service-Tests, `SwaggerImportServiceTests.cs`:**

- Bestehende 5 `ApplyDiffAsync`-Aufrufe auf neue Signatur umstellen (Ziel-ID 1,
  alle Test-Endpunkte tragen `ApplicationId = 1`).
- `ApplyDiff_MismatchedApplicationId_ThrowsAndDoesNotApply` — Diff mit Endpunkt
  `ApplicationId = 99`, Ziel-ID 1 ⇒ `InvalidOperationException`, kein
  Add/Update/Delete am Repository.

**Integrationstests (`WebApplicationFactory`), `ApplicationImportControllerIntegrationTests.cs`:**

- `ApplySwaggerDiff_WithRestApplication_Returns204AndCallsService` — Rest-App,
  `SwaggerImportMock.ApplyDiffAsync(diff, app.Id)` verifizieren.
- `ApplySwaggerDiff_WithoutToken_Returns401`
- `ApplySwaggerDiff_WithUnknownApplicationId_Returns404`
- `ApplySwaggerDiff_WithNonRestApplication_Returns422` — OData-App ⇒ 422 mit
  `errorMessage`.
- `ApplySwaggerDiff_WhenServiceRejectsAggregate_Returns422` — Mock wirft
  `InvalidOperationException("…gehören.")` ⇒ 422 mit `errorMessage`-Body.

**Komponententest, `ImportDialogTests.cs`:**

- `SwaggerImportDialog_ApplyAsync_CallsApiClient` — bUnit rendern, Übernehmen-Klick,
  `ApplySwaggerDiffAsync(app.Id, …)` verifizieren (Mock `IApplicationApiClient`
  ist bereits registriert).

### S6: Dokumentation

- `docs/help/api/api.md`: Abschnitt `POST /api/applications/{id}/swagger-import/apply`
  analog zum OData-Abschnitt (inkl. Hinweis auf Aggregat-Validierung und
  `IApplicationApiClient.ApplySwaggerDiffAsync`).
- `docs/help/endpunkte/ablauf-technisch.md`: Apply-Fluss aktualisieren
  (`SwaggerImportDialog` → `IApplicationApiClient.ApplySwaggerDiffAsync` → REST-Endpunkt
  → `SwaggerImportService.ApplyDiffAsync(diff, applicationId)` inkl. Validierung);
  OData-Passage Z. 407 faktisch korrigieren (Dialog ruft ApiClient, nicht den Service).
- `docs/help/schnittstellenzentrale/ablauf-technisch.md` Z. 40: `ApplyDiffAsync`-Signatur
  nachziehen, falls nötig.
- README (Schritt 12b): Feature-Bullet für den neuen Endpunkt ergänzen.
- `docs/RELEASE_NOTES.md` existiert nicht (semantic-release) — Schritt 12c entfällt
  mit Vermerk.

## Akzeptanzszenarien

**A1 (Happy Path, externer Auslöser):** Anwendung mit `InterfaceType = Rest`
existiert; Aufrufer besitzt gültiges Token.
`POST /api/applications/7/swagger-import/apply` mit Diff (alle Endpunkte
`ApplicationId = 7`) → `204`; neue Endpunkte sind persistiert, geänderte aktualisiert,
entfernte gelöscht; `X-New-Token`-Header gesetzt.

**A2 (Happy Path, UI):** REST-Anwendung geöffnet → „Swagger-Import" → Vorschau →
„Übernehmen" → Dialog schließt, Endpunkte erscheinen im Baum
(abgedeckt durch Playwright `ImportSwagger_ImportsEndpointsIntoTree`, der den
realen HTTP-Pfad mitläuft).

**A3 (Fehlerfälle):** Ohne/ungültigem Token → `401`; unbekannte ID → `404`;
Anwendung mit `InterfaceType = OData`/`Unknown` → `422` mit `errorMessage`;
Diff mit fremder `ApplicationId` → `422` mit verständlicher `errorMessage`,
Bestand unverändert.

**A4 (UI-Fehleranzeige):** Server lehnt Apply ab (z. B. Aggregatverletzung) →
Dialog bleibt offen, zeigt `ImportDialog_Error_Apply` mit der Servermeldung
(bUnit-abgedeckt über das Exception-Verhalten, identisch zum OData-Dialog).

## Test- und Nachweisstrategie

- `dotnet test --filter "FullyQualifiedName!~Playwright"` — Unit/Integration.
- Playwright (Port 5099, `PlaywrightServer` startet selbst):
  mindestens `SwaggerImportTests` + `LayoutSmokeTests` als E2E-Nachweis des
  UI-Flusses; bei ausreichender Laufzeit ggf. Gesamtsuite.
- Reale End-to-End-Abnahme: durch den Playwright-Lauf real abgedeckt (echte App,
  echter HTTP-Roundtrip, sichtbare Wirkung im Baum).
