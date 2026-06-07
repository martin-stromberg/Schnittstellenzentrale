# Code-Review: issue-48-odata-api

Diff-Basis: `git diff main...HEAD` + uncommittete Änderungen im Working Tree
Branches: `issue-48-odata-api` vs `main`
Datum: 2026-06-07
Effort: high

```json
[
  {
    "file": "src/Schnittstellenzentrale/OData/ODataAuthController.cs",
    "line": 23,
    "summary": "GET /odatav4/authenticate ist dokumentiert, aber nicht implementiert — nur POST /odatav4/Authenticate() existiert.",
    "failure_scenario": "Die Dokumentation (docs/help/api/odata-api.md) und der OData-Metadaten-Import-Service erwarten beide GET /odatav4/authenticate. Der Controller registriert nur [HttpPost(\"Authenticate()\")]. Ein Client, der GET /odatav4/authenticate aufruft (wie in der Doku beschrieben), erhält 404. Der Test ODataAuthenticate_Get_ReturnsToken in den Integrationstests ist zudem eine 1:1-Kopie des POST-Tests (PostAsync statt GetAsync) — er testet faktisch nicht die GET-Route."
  },
  {
    "file": "src/Schnittstellenzentrale.Tests/Integration/ODataControllerIntegrationTests.cs",
    "line": 497,
    "summary": "Test ODataAuthenticate_Get_ReturnsToken ruft PostAsync statt GetAsync auf — GET-Route ist ungetestet.",
    "failure_scenario": "Beide Tests ODataAuthenticate_Get_ReturnsToken (Zeile 497) und ODataAuthenticate_Post_ReturnsToken (Zeile 509) enthalten identischen Rumpf mit PostAsync(\"/odatav4/Authenticate()\"). Der erste Test heißt \"Get\", testet aber POST. Da GET /odatav4/authenticate nicht implementiert ist (s. Finding 1), würde ein korrekter GetAsync-Aufruf 404 liefern — der Bug bleibt unentdeckt."
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/ODataImportService.cs",
    "line": 209,
    "summary": "ParseOperationAnnotations parst nur <Action>-Elemente, ignoriert <Function>-Elemente — Annotationen auf OData-Functions werden nicht gelesen.",
    "failure_scenario": "Ein OData-Service deklariert eine Function mit x-sz-auth-type=BearerToken. ParseOperationAnnotations(xmlContent) sucht nur nach Descendants(ns + \"Action\"). Das Function-Element wird nicht gefunden. operationAnnotations.TryGetValue(operation.Name, ...) liefert false → opAuthType = defaultAuthType statt BearerToken. Der importierte Endpoint bekommt die falsche Authentifizierung; ein POST-Request-Script aus x-sz-post-request-script wird ebenfalls nicht übernommen."
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/ODataImportService.cs",
    "line": 127,
    "summary": "GetEndpointGroupsAsync wird in ApplyDiffAsync pro Gruppenname einmal aufgerufen — N serielle DB-Abfragen statt einer.",
    "failure_scenario": "Ein OData-Service mit 10 Entity-Sets erzeugt 50 neue Endpunkte (5 HTTP-Methoden × 10 Sets). Für jeden noch nicht im groupLookup vorhandenen Gruppennamen wird GetEndpointGroupsAsync aufgerufen. Im schlechtesten Fall (alle Gruppen neu) sind das 10 separate DB-Queries. Ein einmaliger Prefetch aller EndpointGroups für die ApplicationId vor der Schleife würde die Anzahl auf 1 Query reduzieren."
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/ODataImportService.cs",
    "line": 110,
    "summary": "existingEndpoints wird nach dem Aufbau von importedEndpoints abgerufen — kein authentischer Snapshot-Zeitpunkt.",
    "failure_scenario": "Zwischen dem HTTP-Abruf der Metadaten und dem GetEndpointsAsync-Aufruf können parallel Endpunkte angelegt oder gelöscht worden sein. Das führt zu falschen ChangedEndpoints/NewEndpoints/RemovedEndpoints im Diff. Dieses Race-Condition-Risiko bestand schon vor dem Diff und wird durch das neue Code nicht verschlechtert, ist aber im neuen Service-Code ein PLAUSIBLE-Befund."
  },
  {
    "file": "src/Schnittstellenzentrale/OData/ODataApplicationsController.cs",
    "line": 75,
    "summary": "Concurrency-Fallback (entity.RowVersion.Length == 0) akzeptiert jeden Client ohne RowVersion-Unterstützung — optimistisches Locking kann vollständig umgangen werden.",
    "failure_scenario": "var concurrencyRowVersion = entity.RowVersion.Length > 0 ? entity.RowVersion : existing.RowVersion. Sendet ein Client kein RowVersion-Feld (oder ein leeres Array, was bei JSON-Deserialisierung der Default ist), wird existing.RowVersion (DB-Wert) verwendet. Das Repository setzt EF OriginalValue = DB-Wert → Concurrency-Check vergleicht DB-Wert gegen sich selbst → immer erfolgreich. Zwei gleichzeitige PUTs überschreiben sich stillschweigend. Das gleiche gilt analog für ODataEndpointsController, ODataEndpointGroupsController und ODataApplicationGroupsController, die dieselbe Logik verwenden."
  },
  {
    "file": "src/Schnittstellenzentrale/Components/Shared/ApplicationContentView.razor",
    "line": 3,
    "summary": "Blazor-Komponente injiziert IODataImportService direkt — verletzt die API-First-Architekturvorgabe (CLAUDE.md).",
    "failure_scenario": "CLAUDE.md schreibt vor: 'Alle Datenentitäten werden aus UI-Komponenten ausschließlich über IApplicationApiClient abgerufen.' IODataImportService ruft intern IEndpointRepository direkt auf (in ApplyDiffAsync). Die Komponente bypässt IApplicationApiClient; bei einer Umstellung auf eine verteilte Architektur müsste IODataImportService separat angepasst werden, während alle IApplicationApiClient-Aufrufe transparent umgeleitet werden könnten."
  }
]
```
