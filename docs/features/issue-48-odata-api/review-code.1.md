# Code-Review: issue-48-odata-api

**Branch:** `issue-48-odata-api` vs. `main`
**Datum:** 2026-06-07
**Effort:** high

```json
[
  {
    "file": "src/Schnittstellenzentrale/OData/ODataEndpointsController.cs",
    "line": 74,
    "summary": "IsSystem-Guard verwendet Null-Conditional auf Application-Navigationseigenschaft; ein verwaister Endpoint (FK ohne passende Application-Zeile) wird als nicht-system behandelt.",
    "failure_scenario": "Endpoint-Zeile mit ApplicationId, die auf eine gelöschte Application zeigt → existing.Application ist null → existing.Application?.IsSystem == true ergibt false → PUT/PATCH/DELETE wird auf einer potenziell geschützten Ressource ausgeführt.",
    "status": "behoben — explizite null-Prüfung auf Application, gibt 404 zurück wenn null"
  },
  {
    "file": "src/Schnittstellenzentrale/OData/ODataEndpointGroupsController.cs",
    "line": 74,
    "summary": "Gleicher Null-Conditional-IsSystem-Guard auf Application-Navigationseigenschaft für EndpointGroup Put/Patch/Delete.",
    "failure_scenario": "EndpointGroup-Zeile mit verwaister ApplicationId → existing.Application ist null → IsSystem-Prüfung ergibt false → Mutation an einer Gruppe, deren Anwendung eine System-Anwendung war, wird zugelassen.",
    "status": "behoben — explizite null-Prüfung auf Application, gibt 404 zurück wenn null"
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/ODataImportService.cs",
    "line": 36,
    "summary": "OperationCanceledException und TaskCanceledException werden nicht abgefangen; nur HttpRequestException wird behandelt.",
    "failure_scenario": "HTTP-Client läuft in einen Timeout oder wird abgebrochen (z.B. Benutzer navigiert weg oder Server-seitiger Timeout) → OperationCanceledException wird nicht abgefangen → Ausnahme propagiert unkontrolliert aus ImportAsync heraus in den Blazor-Circuit statt ImportDiff{ErrorMessage} zurückzugeben.",
    "status": "behoben — catch(OperationCanceledException) ergänzt (fängt auch TaskCanceledException, da diese von OperationCanceledException erbt)"
  },
  {
    "file": "src/Schnittstellenzentrale/OData/ODataApplicationGroupsController.cs",
    "line": 97,
    "summary": "PATCH ApplyPatch für ApplicationGroups ignoriert 'icondata' stillschweigend — anders als PUT, das IconData mappt — PATCH kann Icon-Daten nicht aktualisieren.",
    "failure_scenario": "Client sendet PATCH /odatav4/ApplicationGroups(1) mit {\"IconData\": \"<base64>\"} → switch-Default greift → IconData wird nicht aktualisiert → 200 OK wird zurückgegeben ohne Hinweis, dass das Feld ignoriert wurde.",
    "status": "behoben — icondata-Case in TryApplyPatch ergänzt, analog zu ODataApplicationsController mit Base64-Validierung"
  },
  {
    "file": "src/Schnittstellenzentrale/OData/ODataEndpointsController.cs",
    "line": 32,
    "summary": "GetAllEndpointsAsync materialisiert die gesamte Endpoints-Tabelle in eine List<> bevor OData-Query-Optionen ($filter, $top, $skip) in-memory angewendet werden.",
    "failure_scenario": "Große Deployment mit Tausenden von Endpunkten → GET /odatav4/Endpoints?$top=10 lädt alle Zeilen aus der DB, legt sie im RAM ab, wendet dann $top=10 in-memory an → O(n)-DB-Abfrage für O(1)-Ergebnis, verschwendet Speicher und Zeit bei jeder Anfrage.",
    "status": "nicht umsetzbar — IEndpointRepository.GetAllEndpointsAsync() gibt Task<IList<Endpoint>> zurück, kein IQueryable<>. Eine IQueryable-Unterstützung würde eine Interface-Erweiterung und Repository-Änderung erfordern, die über den Scope dieser Korrektur hinausgeht."
  },
  {
    "file": "src/Schnittstellenzentrale/Components/Shared/ODataImportDialog.razor",
    "line": 13,
    "summary": "ApplyDiffAsync hat keine Fehlerbehandlung; eine Repository-Ausnahme mitten im Diff hinterlässt die Anwendung in einem teilweise importierten Zustand ohne Fehlermeldung.",
    "failure_scenario": "Drei neue Endpunkte hinzuzufügen → erster AddEndpointAsync gelingt und wird committed, zweiter wirft DbUpdateException → Blazor-Circuit-Ausnahme, Benutzer sieht generischen Fehler, ein Endpunkt ist dauerhaft persistiert während zwei es nicht sind, erneutes Ausführen des Imports kann Duplikate erzeugen.",
    "status": "behoben — try/catch in ApplyAsync ergänzt, Fehlermeldung wird im Dialog angezeigt"
  }
]
```
