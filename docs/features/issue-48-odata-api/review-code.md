# Code-Review: issue-48-odata-api

Diff-Basis: `git diff main...HEAD` (Branches: `issue-48-odata-api` vs `main`)

```json
[
  {
    "file": "src/Schnittstellenzentrale/OData/ODataApplicationsController.cs",
    "line": 46,
    "summary": "POST akzeptiert IsSystem=true im Request-Body und legt eine system-markierte Anwendung an.",
    "failure_scenario": "Ein authentifizierter API-Client sendet POST /odatav4/Applications mit {\"Name\":\"x\",\"BaseUrl\":\"y\",\"IsSystem\":true}. Der Controller setzt nur Id und RowVersion zurück, nicht IsSystem. Die Anwendung wird mit IsSystem=true in der Datenbank gespeichert — danach kann sie weder via PUT/DELETE noch über die UI verändert werden, weil alle Write-Pfade IsSystem=true als Schutz werten."
  },
  {
    "file": "src/Schnittstellenzentrale/OData/ODataApplicationGroupsController.cs",
    "line": 46,
    "summary": "POST akzeptiert IsSystem=true im Request-Body und legt eine system-markierte Anwendungsgruppe an.",
    "failure_scenario": "Analog zu ODataApplicationsController.Post: Ein Aufrufer kann eine ApplicationGroup mit IsSystem=true erstellen. Danach ist die Gruppe unveränderbar und unlöschbar über alle Write-Endpunkte (PUT/PATCH/DELETE prüfen IsSystem und geben 403 zurück), ohne dass es einen legitimen Weg gibt, diesen Zustand rückgängig zu machen."
  },
  {
    "file": "src/Schnittstellenzentrale/OData/ODataApplicationsController.cs",
    "line": 58,
    "summary": "PUT ignoriert das Client-seitige RowVersion — optimistische Nebenläufigkeitskontrolle ist für alle OData-PUT/PATCH-Operationen wirkungslos.",
    "failure_scenario": "Client A liest Application(1) mit RowVersion=[1,2,3]. Client B liest und ändert dieselbe Ressource (RowVersion wird zu [4,5,6]). Client A sendet PUT mit RowVersion=[1,2,3] im Body. Der Controller lädt `existing` aus der DB (RowVersion=[4,5,6]), kopiert alle Felder von `entity` in `existing`, übergibt aber `existing` (nicht `entity`) an UpdateApplicationAsync. Im Repository wird `existing.RowVersion` (=[4,5,6]) als OriginalValue gesetzt — der EF-Concurrency-Check vergleicht [4,5,6] mit dem aktuellen DB-Wert [4,5,6] und sieht keinen Konflikt. Client A überschreibt stillschweigend die Änderungen von Client B."
  },
  {
    "file": "src/Schnittstellenzentrale/Components/Shared/ApplicationContentView.razor",
    "line": 3,
    "summary": "Blazor-Komponente injiziert IODataImportService direkt — verletzt die API-First-Architekturvorgabe aus CLAUDE.md.",
    "failure_scenario": "Die Komponente ruft ODataImportService.ImportAsync und ApplyDiffAsync direkt auf, statt diese Funktionalität über IApplicationApiClient zu leiten. Das erzeugt eine direkte Abhängigkeit zwischen UI-Schicht und Infrastruktur-Service. Bei einem späteren Wechsel auf eine verteilte Architektur (z.B. separater Backend-Service) bricht dieser Aufruf, während alle IApplicationApiClient-Aufrufe transparent umgeleitet werden können. Außerdem umgeht es den einheitlichen HTTP-Client-basierten Fehlerbehandlungs- und Retry-Mechanismus."
  },
  {
    "file": "src/Schnittstellenzentrale/OData/ODataControllerBase.cs",
    "line": 23,
    "summary": "Bei OData-Abfragevalidierungsfehlern ([EnableQuery]) kann die Authentifizierung umgangen werden — der Filter läuft ggf. nach dem OData-Framework.",
    "failure_scenario": "ODataControllerBase implementiert IAsyncActionFilter, das vom MVC-Filter-Pipeline nach der OData-Query-Validierung ausgeführt werden kann. Wenn [EnableQuery] eine ungültige $filter-Anfrage ablehnt, liefert das Framework möglicherweise ein 400-Fehlerdetail zurück, bevor OnActionExecutionAsync greift. In der Praxis hängt die Reihenfolge von der Registrierung der OData- vs. MVC-Middleware ab — ein nicht-authentifizierter Client erhält ggf. strukturierte Fehlerdetails über das Datenbankschema (z.B. \"Property 'X' not found\")."
  },
  {
    "file": "src/Schnittstellenzentrale/Program.cs",
    "line": 76,
    "summary": "SetMaxTop(null) entfernt jede serverseitige Obergrenze für OData-$top — unbeschränkte Tabellen-Dumps möglich.",
    "failure_scenario": "Ein authentifizierter Client kann GET /odatav4/Endpoints ohne $top-Parameter senden und erhält alle Endpunkte in einer einzigen Antwort. In einer Instanz mit Tausenden von Endpunkten führt das zu hohem Speicher- und Serialisierungsaufwand auf dem Server. Da SetMaxTop(null) explizit kein Limit setzt, gibt es keinen Fallback-Schutz."
  },
  {
    "file": "src/Schnittstellenzentrale/OData/ODataEndpointsController.cs",
    "line": 68,
    "summary": "PUT für Endpoints überträgt nicht ApplicationId — ein Endpoint kann aber implizit auf eine andere Anwendung verschoben werden, wenn der Aufrufer EndpointGroupId einer anderen Anwendung setzt.",
    "failure_scenario": "Der PUT-Handler lässt `existing.EndpointGroupId = entity.EndpointGroupId` zu. Wenn entity.EndpointGroupId auf eine Gruppe einer anderen (fremden) Anwendung zeigt, wird der Endpoint in eine andere Anwendung verschoben, ohne dass die IsSystem-Prüfung für die Zielanwendung ausgeführt wird. Ein Endpoint aus einer normalen Anwendung könnte so in eine System-Anwendungs-Gruppe verschoben werden."
  },
  {
    "file": "src/Schnittstellenzentrale/OData/ODataApplicationGroupsController.cs",
    "line": 93,
    "summary": "TryApplyPatch mit IconData-Base64-Validierung ist eine Kopie der entsprechenden Methode in ODataApplicationsController — Wartungsrisiko.",
    "failure_scenario": "Beide Klassen haben eine identische private TryApplyPatch-Methode mit identischer IconData-Validierungslogik. Wird die Validierung (z.B. MIME-Typ-Prüfung) in einem der Controller erweitert, muss die Änderung manuell in den anderen übertragen werden. Vergisst man dies, verhalten sich die Endpunkte inkonsistent."
  }
]
```
