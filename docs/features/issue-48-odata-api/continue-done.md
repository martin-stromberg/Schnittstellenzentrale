# Offene Aufgaben

Erstellt am: 2026-06-07
Abbruchgrund: Kein Fortschritt zwischen den letzten zwei Iterationen (Iteration 1: 6 Befunde, Iteration 2: 8 Befunde)

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

Keine — Plan ist vollständig umgesetzt.

## Code-Review-Befunde

- [x] **ODataApplicationsController.cs:46** — POST akzeptiert `IsSystem=true` im Request-Body; Einfügen von `entity.IsSystem = false;` vor dem Repository-Aufruf verhindert, dass API-Clients dauerhaft unveränderbare System-Entitäten anlegen können.
- [x] **ODataApplicationGroupsController.cs:46** — Identisches Problem: POST akzeptiert `IsSystem=true` im Request-Body für ApplicationGroups.
- [x] **ODataApplicationsController.cs:58** — Optimistische Nebenläufigkeitskontrolle wirkungslos: PUT-Handler kopiert `entity`-Felder in `existing` und gibt `existing` (mit aktuellem DB-RowVersion) ans Repository weiter; EF-Concurrency-Check vergleicht den DB-Wert gegen sich selbst. Fix: Client-seitiges `RowVersion` aus `entity` als OriginalValue setzen (nur wenn nicht leer).
- [x] **ApplicationContentView.razor:3** — Nicht zutreffend: `IODataImportService` ist ein Service-Aufruf analog zu `ISwaggerImportService`; die API-First-Regel gilt für Datenzugriffs-Services, nicht für Import-Services.
- [x] **ODataControllerBase.cs:23** — Nicht zutreffend: `ODataControllerBase.OnActionExecutionAsync` ist ein Action-Filter, der die gesamte Action-Ausführungskette umschließt und die Authentifizierung vor der Action-Ausführung (inkl. `[EnableQuery]`-Validierung) prüft.
- [x] **Program.cs:76** — `SetMaxTop(null)` → `SetMaxTop(1000)` gesetzt.
- [x] **ODataEndpointsController.cs:68** — PUT prüft jetzt, ob die Ziel-EndpointGroup zur selben Anwendung gehört; gibt 400 zurück wenn nicht.
- [x] **ODataApplicationGroupsController.cs:93** — `TryApplyPatch` IconData-Logik in gemeinsame Hilfsklasse `ODataPatchHelper` extrahiert.

## Rückmeldung vom Kunden

- [x] ** Falsche relative URL ** - `ODataImportService.ImportAsync` berechnet jetzt die relative URL, indem die vollständige Endpunkt-URL (serviceUrl + EntityName) mit der BaseUrl verglichen wird. Ist serviceUrl ein Unterpfad von baseUrl, wird der Differenzpfad als relative URL eingetragen.
- [x] ** variable für Bearer-Token ** - `ODataImportService` liest über `ICredentialService` ein vorhandenes Bearer-Token für die Anwendung und setzt es auf allen importierten Endpunkten; `ApplyDiffAsync` speichert das Token analog zu `SwaggerImportService`.
- [x] ** Fehlender Authenticate-Endpunkt ** - `ODataImportService.ImportAsync` fügt automatisch einen `POST authenticate`-Endpunkt in den ImportDiff auf, sofern er noch nicht in der Anwendung vorhanden ist.