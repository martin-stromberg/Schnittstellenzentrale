# Code-Review: issue-48-odata-api

Diff-Basis: `git diff main...HEAD` + uncommittete Änderungen im Working Tree
Branches: `issue-48-odata-api` vs `main`
Datum: 2026-06-07
Effort: high

```json
[
  {
    "file": "src/Schnittstellenzentrale/OData/ODataApplicationGroupsController.cs",
    "line": 72,
    "summary": "PUT für ApplicationGroups ignoriert das Client-seitige RowVersion — optimistische Nebenläufigkeitskontrolle ist wirkungslos.",
    "failure_scenario": "Client A liest ApplicationGroup(1) mit RowVersion=[1,2,3]. Client B ändert dieselbe Gruppe (RowVersion=[4,5,6]). Client A sendet PUT mit RowVersion=[1,2,3]. Der Controller lädt 'existing' (RowVersion=[4,5,6]), kopiert Name/Description/Subtitle/IconData, übergibt aber 'existing' (mit RowVersion=[4,5,6]) an UpdateGroupAsync. Das Repository setzt OriginalValue=[4,5,6] — der EF-Concurrency-Check vergleicht [4,5,6] mit [4,5,6] und sieht keinen Konflikt. Client A überschreibt stillschweigend die Änderungen von Client B. Gegensatz: ODataApplicationsController.Put wurde bereits mit concurrencyRowVersion-Logik gefixt — diese Korrektur fehlt hier."
  },
  {
    "file": "src/Schnittstellenzentrale/OData/ODataEndpointsController.cs",
    "line": 100,
    "summary": "PUT für Endpoints ignoriert das Client-seitige RowVersion — optimistische Nebenläufigkeitskontrolle ist wirkungslos.",
    "failure_scenario": "Analog zu ODataApplicationGroupsController.Put: Der Controller übergibt 'existing' (mit DB-RowVersion) an UpdateEndpointAsync. Das Repository setzt OriginalValue = existing.RowVersion = DB-Wert → Concurrency-Check vergleicht DB-Wert gegen sich selbst → immer erfolgreich. Zwei gleichzeitige Clients können denselben Endpoint überschreiben ohne DbUpdateConcurrencyException."
  },
  {
    "file": "src/Schnittstellenzentrale/OData/ODataEndpointGroupsController.cs",
    "line": 83,
    "summary": "PUT für EndpointGroups ignoriert das Client-seitige RowVersion — optimistische Nebenläufigkeitskontrolle ist wirkungslos.",
    "failure_scenario": "Analog zu den anderen Controllern: 'existing' wird mit unveränderter DB-RowVersion an UpdateEndpointGroupAsync übergeben. UpdateEndpointGroupAsync setzt OriginalValue = group.RowVersion (= DB-Wert). EF-Concurrency-Check ist wirkungslos — lost updates werden nicht erkannt."
  },
  {
    "file": "src/Schnittstellenzentrale/OData/ODataApplicationsController.cs",
    "line": 72,
    "summary": "Concurrency-Fallback akzeptiert leeres RowVersion vom Client und verwendet dann den DB-Wert — Schutz kann durch Weglassen des RowVersion im Request-Body umgangen werden.",
    "failure_scenario": "var concurrencyRowVersion = entity.RowVersion.Length > 0 ? entity.RowVersion : existing.RowVersion — ein Client sendet PUT ohne RowVersion-Feld (oder mit leerem Array, was durch JSON-Deserialisierung Standard ist). entity.RowVersion.Length == 0 → Fallback auf existing.RowVersion (DB-Wert) → Concurrency-Check immer erfolgreich. Die Schutzfunktion ist opt-in statt opt-out: nur Clients, die explizit ein RowVersion mitsenden, profitieren davon."
  },
  {
    "file": "src/Schnittstellenzentrale/Components/Shared/ApplicationContentView.razor",
    "line": 3,
    "summary": "Blazor-Komponente injiziert IODataImportService direkt — verletzt die API-First-Architekturvorgabe aus CLAUDE.md.",
    "failure_scenario": "CLAUDE.md: 'Alle Datenentitäten werden aus UI-Komponenten ausschließlich über IApplicationApiClient abgerufen. Direktzugriffe auf Repository-Interfaces aus Blazor-Komponenten sind nicht erlaubt.' IODataImportService greift direkt auf IEndpointRepository zu (in ApplyDiffAsync). Die Komponente bypässt die IApplicationApiClient-Abstraktionsschicht, was bei einer Umstellung auf eine verteilte Architektur (z. B. separater Backend-Service) bricht, während alle IApplicationApiClient-Aufrufe transparent umgeleitet werden können. Hinweis: ISwaggerImportService folgt demselben Muster — ggf. gilt die Regel nur für reine Datenzugriffs-Interfaces; klären."
  },
  {
    "file": "src/Schnittstellenzentrale/OData/ODataEndpointsController.cs",
    "line": 84,
    "summary": "EndpointGroupId-Validierung in PUT prüft nicht, ob die Zielanwendung IsSystem=true ist — ein Endpoint kann in eine System-Gruppe verschoben werden.",
    "failure_scenario": "Client sendet PUT /odatav4/Endpoints(5) mit EndpointGroupId=99, wobei Gruppe 99 zu einer System-Anwendung gehört. Der Validator prüft nur targetGroup.ApplicationId != existing.ApplicationId — nicht ob die Zielanwendung IsSystem ist. Da bestehende Endpoint-Anwendung != Ziel-Anwendung, schlägt bereits die ApplicationId-Prüfung an (BadRequest). Aber: falls ein Endpoint aus einer normalen Anwendung zu einer Gruppe derselben Anwendung (die IsSystem=false ist, aber die Gruppe wurde nachträglich zu IsSystem befördert) verschoben wird, greift kein Schutz. Konkret: EndpointGroup.Application.IsSystem wird nicht geprüft — nur existing.Application.IsSystem."
  },
  {
    "file": "src/Schnittstellenzentrale/Components/Shared/ODataImportDialog.razor",
    "line": 13,
    "summary": "ApplyAsync delegiert jetzt direkt ohne eigene Fehlerbehandlung — Fehlertext kommt aus ImportDialog_Error_Apply statt ODataImportDialog_Error_Apply; der resx-Schlüssel ODataImportDialog_Error_Apply ist verwaist.",
    "failure_scenario": "Kein Absturz, aber: Die Fehlermeldung bei einem Repository-Fehler während des OData-Imports wird jetzt aus ImportDialog_Error_Apply ('Fehler beim Übernehmen: {0}') formatiert statt aus dem spezifischeren ODataImportDialog_Error_Apply-Schlüssel. Der ODataImportDialog_Error_Apply-Schlüssel in beiden resx-Dateien ist toter Code, wird aber nicht entfernt. Wartungsrisiko: spätere Übersetzer pflegen zwei Schlüssel, von denen einer nie angezeigt wird."
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/ODataImportService.cs",
    "line": 127,
    "summary": "authenticate-Endpunkt wird nur hinzugefügt wenn er noch nicht existiert, aber der Pfad-Vergleich ist case-sensitiv und normalisiert nicht trailing-Slashes.",
    "failure_scenario": "existingEndpoints.Any(e => e.Method == POST && e.RelativePath == authenticateEndpoint.RelativePath) — RelativePath aus BuildRelativePath verwendet StringComparison.OrdinalIgnoreCase für den Präfix-Check, aber der Ergebnis-String hat beliebige Groß-/Kleinschreibung je nach entityName. Der anschließende Vergleich e.RelativePath == authenticateEndpoint.RelativePath ist case-sensitiv (standard string ==). Falls ein bestehender Endpoint 'Authenticate' statt 'authenticate' heißt (oder trailing Slash abweicht), erkennt der Check keine Übereinstimmung → authenticate-Endpunkt wird doppelt importiert."
  }
]
```
