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

- [ ] **Authenticate-Endpunkt — Design falsch** — Die aktuelle Implementierung fügt beim OData-Import immer automatisch einen `POST authenticate`-Endpunkt hinzu. Das ist falsch: Nicht jede OData-API hat einen Authenticate-Endpunkt. Die automatische Einfügung rückgängig machen.

- [ ] **Authenticate als OData Unbound Action** — Den `ODataAuthController` als OData Unbound Action modellieren (`POST /odatav4/Authenticate()`), damit der Endpunkt in `$metadata` erscheint und vom Import automatisch erkannt wird. Die Action nimmt Username/Password entgegen und gibt ein Token zurück. Route ändert sich von `/odatav4/authenticate` zu `/odatav4/Authenticate()`.

- [ ] **PUT-Endpunkte fehlen beim Import** — Der OData-Import erfasst aktuell nur GET- und POST-Endpunkte. OData-Entitäten haben standardmäßig auch PUT (vollständige Aktualisierung) und PATCH (Teilaktualisierung) und DELETE-Endpunkte. Für jede Entity-Set aus `$metadata` müssen alle HTTP-Methoden (GET collection, GET by key, POST, PUT, PATCH, DELETE) als separate Endpunkte importiert werden, sofern die Entität schreibbar ist (`<EntitySet>` ohne `<Annotation Term="Org.OData.Capabilities.V1.InsertRestrictions" ...>`-Ausschluss).

- [ ] **Endpunktgruppen fehlen** — Wie beim Swagger-Import soll für jede Entität (Entity-Set) eine eigene Endpunktgruppe (`EndpointGroup`) angelegt werden. Alle Endpunkte einer Entität (GET, POST, PUT, PATCH, DELETE) kommen in dieselbe Gruppe. Der Gruppenname entspricht dem Entity-Set-Namen (z.B. `Applications`, `Endpoints`).

- [ ] **Authentifizierungsart immer „None"** — Die importierten Endpunkte sollen — falls möglich — die Authentifizierungsart aus der `$metadata`-Datei lesen. Vorschlag: `$metadata` um ein herstellerspezifisches Annotation-Attribut erweitern (z.B. `x-sz-auth-type` analog zu `x-sz-bearer-token` bei Swagger). Entitäts-Endpunkte der eigenen API sollen `BearerToken` vorgeben, der Authenticate-Endpunkt `Negotiate`. Für fremde OData-APIs ohne diese Annotation: Standardwert `None` beibehalten.

- [ ] **Skripte nicht erfasst** — Wie beim Swagger-Import soll die `$metadata`-Datei um herstellerspezifische Annotation-Attribute für Skripte erweiterbar sein (z.B. `x-sz-post-request-script` an der jeweiligen Action/EntitySet-Annotation). Der OData-Import-Service soll diese Attribute auslesen und den Endpunkten zuweisen — analog zur bestehenden Swagger-Implementierung mit `x-sz-post-request-script`.
