# Code-Review: issue-48-odata-api

Diff-Basis: `git diff main...HEAD`
Branches: `issue-48-odata-api` vs `main`
Datum: 2026-06-07
Effort: high
Iteration: 2

```json
[
  {
    "file": "src/Schnittstellenzentrale/OData/ODataEndpointsController.cs",
    "line": 129,
    "summary": "PATCH für Endpoints prüft targetGroup.Application.IsSystem nicht — anders als PUT (Zeile 89), das diese Prüfung enthält.",
    "failure_scenario": "Client sendet PATCH /odatav4/Endpoints(5) mit {\"EndpointGroupId\": 99}, wobei Gruppe 99 zur selben Anwendung gehört, die Anwendung aber IsSystem=true ist. PATCH prüft nur targetGroup.ApplicationId != existing.ApplicationId (Zeile 132), nicht ob targetGroup.Application.IsSystem. Da ApplicationId übereinstimmt, gibt die Validierung kein 400 zurück. Der Endpoint wird in eine Gruppe einer System-Anwendung verschoben, obwohl PUT für denselben Endpoint 403 zurückgeben würde."
  },
  {
    "file": "src/Schnittstellenzentrale/Components/Shared/ApplicationContentView.razor",
    "line": 3,
    "summary": "IODataImportService direkt in Blazor-Komponente injiziert — API-First-Verletzung; CLAUDE.md-Ausnahme wurde vom Kunden abgelehnt.",
    "failure_scenario": "Bekannter offener Punkt (continue.md): Korrektur erfordert neuen ApiClient-Endpunkt (z.B. ImportODataMetadataAsync) und Controller-Endpunkt, damit die Komponente ausschließlich über IApplicationApiClient kommuniziert. Analog für ISwaggerImportService prüfen."
  },
  {
    "file": "src/Schnittstellenzentrale/OData/ODataApplicationsController.cs",
    "line": 75,
    "summary": "Concurrency-Schutz ist opt-in: fehlendes RowVersion im Request-Body umgeht den EF-Concurrency-Check.",
    "failure_scenario": "Bekannter offener Punkt (continue.md, bewusste Designentscheidung): var concurrencyRowVersion = entity.RowVersion.Length > 0 ? entity.RowVersion : existing.RowVersion — sendet ein Client kein RowVersion-Feld, greift der Fallback auf den DB-Wert; der EF-Check ist dann immer erfolgreich. Gleichzeitige Schreibkonflikte werden nicht erkannt. Gilt analog für ApplicationGroupsController, EndpointsController und EndpointGroupsController."
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/ODataImportService.cs",
    "line": 160,
    "summary": "ExtractEntitySetName wertet nur den ersten Leerzeichen-Index aus — bei Endpunkt-Namen ohne Leerzeichen (z.B. Operation-Namen) gibt die Methode null zurück und die Gruppe wird nicht zugewiesen.",
    "failure_scenario": "Operation-Endpunkte (z.B. OData-Actions und -Functions ohne Leerzeichen im Namen) erhalten keine EndpointGroup-Zuweisung in ApplyDiffAsync. Das ist für Actions/Functions korrekt (kein Gruppen-Mapping vorgesehen), jedoch: Sollte ein EntitySet-Name zufällig kein Leerzeichen haben, würde auch dessen Endpunkt keine Gruppe erhalten. In der Praxis betrifft das ausschließlich Operation-Endpunkte — kein Fehler, aber die Methode enthält keine Dokumentation dieser Annahme."
  }
]
```

## Hinweise

- Die Befunde unter `file: ODataApplicationsController.cs:75` und `file: ApplicationContentView.razor:3` sind bekannte offene Punkte aus `continue.md` und wurden gemäß Aufgabenstellung als solche erkannt und aufgenommen.
- Der Befund `ODataEndpointsController.cs:129` (PATCH ohne IsSystem-Prüfung auf targetGroup) ist neu — der analoge Fix wurde für PUT in Iteration 1 umgesetzt, aber für PATCH übersehen.
- `ODataImportDialog_Error_Apply`-Schlüssel: bereits in Iteration 1 entfernt (kein Match in resx). Nicht mehr relevant.
- `ExtractEntitySetName`-Befund (Zeile 160): PLAUSIBLE als Wartungshinweis, kein Laufzeitfehler unter aktuellen Bedingungen.
- `ImportDialog.razor`: `BearerTokens` wird korrekt aus `Diff.BearerTokens` in `selectedDiff` kopiert (Zeile 106). Dieser Befund aus früheren Reviews wurde behoben.
