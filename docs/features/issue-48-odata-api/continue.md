# Offene Aufgaben

Erstellt am: 2026-06-07
Abbruchgrund: Kein Fortschritt zwischen den letzten zwei Iterationen (Iteration 1: 6 Befunde, Iteration 2: 8 Befunde)

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

Keine — Plan ist vollständig umgesetzt.

## Code-Review-Befunde

- [ ] **ODataApplicationsController.cs:46** — POST akzeptiert `IsSystem=true` im Request-Body; Einfügen von `entity.IsSystem = false;` vor dem Repository-Aufruf verhindert, dass API-Clients dauerhaft unveränderbare System-Entitäten anlegen können.
- [ ] **ODataApplicationGroupsController.cs:46** — Identisches Problem: POST akzeptiert `IsSystem=true` im Request-Body für ApplicationGroups.
- [ ] **ODataApplicationsController.cs:58** — Optimistische Nebenläufigkeitskontrolle wirkungslos: PUT-Handler kopiert `entity`-Felder in `existing` und gibt `existing` (mit aktuellem DB-RowVersion) ans Repository weiter; EF-Concurrency-Check vergleicht den DB-Wert gegen sich selbst. Fix: Client-seitiges `RowVersion` aus `entity` als OriginalValue setzen.
- [ ] **ApplicationContentView.razor:3** — Blazor-Komponente injiziert `IODataImportService` direkt, was gegen die API-First-Architekturvorgabe aus CLAUDE.md verstößt (analog zu `ISwaggerImportService`, der ebenfalls direkt injiziert ist — ggf. ist die Regel auf reine Datenzugriffs-Services beschränkt; klären).
- [ ] **ODataControllerBase.cs:23** — `[EnableQuery]`-Validierungsfehler können zurückgegeben werden, bevor `OnActionExecutionAsync` die Authentifizierung prüft; ein nicht-authentifizierter Client erhält ggf. Schema-Details.
- [ ] **Program.cs:76** — `SetMaxTop(null)` entfernt die serverseitige Obergrenze für `$top`; unbeschränkte Tabellen-Dumps sind möglich.
- [ ] **ODataEndpointsController.cs:68** — PUT für Endpoints erlaubt implizites Verschieben auf eine andere Anwendung durch Setzen von `EndpointGroupId` einer fremden Anwendungsgruppe ohne IsSystem-Prüfung der Zielanwendung.
- [ ] **ODataApplicationGroupsController.cs:93** — `TryApplyPatch` mit IconData-Base64-Validierung ist eine Kopie aus `ODataApplicationsController`; in gemeinsame Hilfsklasse extrahieren.
