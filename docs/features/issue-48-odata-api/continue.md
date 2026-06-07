# Offene Aufgaben

Erstellt am: 2026-06-07
Abbruchgrund: Kein Fortschritt zwischen den letzten zwei Iterationen (Iteration 1: 6 Befunde, Iteration 2: 8 Befunde)

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

Keine — Plan ist vollständig umgesetzt.

## Code-Review-Befunde

- [ ] **ODataApplicationGroupsController.cs:72** — PUT ignoriert Client-seitiges RowVersion; EF-Concurrency-Check ist wirkungslos (analog zum bereits korrigierten `ODataApplicationsController`). Fix: `concurrencyRowVersion`-Muster aus `ODataApplicationsController.Put` übernehmen.
- [ ] **ODataEndpointsController.cs:100** — Identisches RowVersion-Problem für Endpoints PUT.
- [ ] **ODataEndpointGroupsController.cs:83** — Identisches RowVersion-Problem für EndpointGroups PUT.
- [ ] **ODataApplicationsController.cs:72** — Concurrency-Schutz ist opt-in: leeres/fehlendes RowVersion im Request umgeht den Check (Fallback auf DB-Wert). Erwägen, ob das gewünschtes Verhalten ist, oder ob leeres RowVersion als Fehler gewertet werden soll.
- [ ] **ApplicationContentView.razor:3** — `IODataImportService` direkt injiziert (API-First-Frage, mehrfach flagged). Klären ob Import-Services von der Regel ausgenommen sind; ggf. in CLAUDE.md dokumentieren.
- [ ] **ODataEndpointsController.cs:84** — PUT-EndpointGroupId-Validierung prüft nicht `targetGroup.Application.IsSystem`; Endpoint kann in eine System-Gruppe derselben Anwendung verschoben werden.
- [ ] **ODataImportDialog.razor:13** — `ODataImportDialog_Error_Apply`-Schlüssel in beiden resx-Dateien ist verwaist (toter Code nach Fehlerbehandlungs-Refaktor). Entfernen.
- [ ] **ODataImportService.cs:127** — authenticate-Endpunkt-Duplikatprüfung ist case-sensitiv; `RelativePath`-Vergleich sollte `StringComparison.OrdinalIgnoreCase` verwenden.

## Rückmeldung vom Kunden (Design-Überdenken)

- [ ] **Authenticate-Endpunkt — Design falsch** — Die aktuelle Implementierung fügt beim OData-Import immer automatisch einen `POST authenticate`-Endpunkt hinzu. Das ist falsch: Nicht jede OData-API hat einen Authenticate-Endpunkt. Der Grund, warum er nicht in `$metadata` enthalten ist: OData `$metadata` beschreibt nur das Datenmodell (Entity-Typen, Entity-Sets, Aktionen/Funktionen des Datenmodells) — keine beliebigen HTTP-Endpunkte wie Authentifizierung. Die automatische Einfügung rückgängig machen und alternative Lösung klären (z.B. nur für die eigene Schnittstellenzentrale-API einfügen, oder als konfigurierbares Verhalten).
