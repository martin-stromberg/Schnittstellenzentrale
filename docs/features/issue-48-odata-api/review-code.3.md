# Code-Review: issue-48-odata-api

Diff-Basis: `git diff main...HEAD` (Branches: `issue-48-odata-api` vs `main`)

```json
[
  {
    "file": "src/Schnittstellenzentrale/Components/Shared/ODataImportDialog.razor",
    "line": 18,
    "summary": "Fehlermeldung bei ApplyDiffAsync-Fehler wird niemals angezeigt — der Dialog schließt sich vor dem Rendering.",
    "failure_scenario": "ODataImportDialog.ApplyAsync fängt eine Exception, setzt _errorMessage, wirft sie aber nicht weiter. ImportDialog.ApplyAsync (Aufrufer via OnApply-Callback) sieht keine Exception und ruft await OnClose.InvokeAsync() auf, das _showODataImport=false setzt. Die ODataImportDialog-Komponente wird aus dem DOM entfernt, bevor _errorMessage gerendert werden kann. Der Benutzer erhält keine Rückmeldung über den Fehler; der Dialog schließt sich still."
  },
  {
    "file": "src/Schnittstellenzentrale/Components/Shared/ImportDialog.razor",
    "line": 101,
    "summary": "BearerTokens werden beim Erstellen von selectedDiff nicht kopiert — Credentials werden beim OData-Import immer verworfen.",
    "failure_scenario": "ImportDialog erstellt selectedDiff = new ImportDiff { NewEndpoints = ..., ChangedEndpoints = ..., RemovedEndpoints = ... } ohne BearerTokens zu übernehmen. ODataImportService.ApplyDiffAsync ruft SaveBearerTokenIfPresent(endpoint, diff) auf und wertet diff.BearerTokens aus — findet aber das leere Default-Dictionary. Bearer-Token-Credentials werden silently verworfen, obwohl ImportAsync sie korrekt in der ImportDiff.BearerTokens befüllt hat. Das trifft auch SwaggerImportDialog."
  },
  {
    "file": "src/Schnittstellenzentrale/OData/ODataEndpointsController.cs",
    "line": 115,
    "summary": "PATCH für Endpoints validiert EndpointGroupId nicht gegen die Anwendungsgrenze — Endpoints können in Gruppen fremder Anwendungen verschoben werden.",
    "failure_scenario": "PUT (Zeilen 80-84) prüft, dass entity.EndpointGroupId zur selben Anwendung gehört, und gibt sonst 400 zurück. PATCH schreibt endpointgroupid direkt (Zeile 132: target.EndpointGroupId = prop.Value.GetInt32()) ohne diese Prüfung. Ein Aufrufer sendet PATCH /odatav4/Endpoints(5) mit {\"EndpointGroupId\": 99}, wobei Gruppe 99 zu einer anderen Anwendung gehört. Der Endpoint landet in einer Gruppe einer fremden (ggf. system-markierten) Anwendung."
  },
  {
    "file": "src/Schnittstellenzentrale/OData/ODataApplicationsController.cs",
    "line": 77,
    "summary": "PUT überschreibt existing.RowVersion mit dem client-seitigen Wert vor dem Repository-Update — optimistische Nebenläufigkeitskontrolle wird ausgehebelt.",
    "failure_scenario": "Zeilen 77-78: if (entity.RowVersion.Length > 0) existing.RowVersion = entity.RowVersion. Das Repository setzt den EF-OriginalValue auf existing.RowVersion — das ist jetzt der Wert des Clients, nicht der beim Laden gelesene DB-Wert. Client A besitzt einen veralteten RowVersion, Client B hat die Ressource zwischenzeitlich geändert. Client A sendet PUT und überschreibt existing.RowVersion mit dem eigenen (veralteten) Wert, der nach dem Überschreiben aber nicht mehr mit dem aktuellen DB-Wert verglichen wird: EF sieht keinen Konflikt und speichert blind."
  },
  {
    "file": "src/Schnittstellenzentrale/OData/ODataEndpointsController.cs",
    "line": 32,
    "summary": "GetAllEndpointsAsync materialisiert alle Endpunkte mit allen Includes in den Speicher; [EnableQuery] filtert erst danach — kein DB-seitiges Pushdown.",
    "failure_scenario": "Get() ruft GetAllEndpointsAsync() auf, das alle Endpoints inkl. Headers, QueryParameters, EndpointGroup und Application als IList<Endpoint> lädt. Ok(endpoints.AsQueryable()) erzeugt LINQ-to-Objects. Bei einer Instanz mit Tausenden Endpunkten werden alle vollständig in den Speicher geladen, auch wenn der Client $top=10&$filter=... setzt. Der Speicher- und Serialisierungsaufwand wächst linear mit der Datenmenge. Dasselbe gilt für ODataEndpointGroupsController.Get() via GetAllEndpointGroupsAsync."
  },
  {
    "file": "src/Schnittstellenzentrale/OData/ODataApplicationGroupsController.cs",
    "line": 94,
    "summary": "TryApplyPatch mit IconData-Validierung ist eine exakte Kopie der gleichnamigen Methode in ODataApplicationsController — Wartungsduplikat trotz vorhandenem ODataPatchHelper.",
    "failure_scenario": "Beide Controller enthalten eine fast identische private static bool TryApplyPatch-Methode, die über alle Felder iteriert und IconData via ODataPatchHelper delegiert. Das gemeinsame ODataPatchHelper.TryApplyIconData wurde für das Innere extrahiert, aber das äußere Muster (Schleife, switch, error-out) ist dupliziert. Erweiterungen (z.B. Beschränkung der IconData-Größe) müssen manuell in beiden Methoden gepflegt werden."
  }
]
```
