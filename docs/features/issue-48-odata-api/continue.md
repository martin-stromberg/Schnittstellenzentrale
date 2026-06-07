# Offene Aufgaben

Erstellt am: 2026-06-07
Abbruchgrund: Maximale Iterationsanzahl (3) erreicht

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

Keine — Plan ist vollständig umgesetzt.

## Code-Review-Befunde

- [ ] **ODataApplicationsController.cs:72** — Concurrency-Schutz ist opt-in: leeres/fehlendes RowVersion im Request umgeht den EF-Concurrency-Check (Fallback auf DB-Wert). Gilt analog für alle vier Controller. Bewusste Designentscheidung — evaluieren ob akzeptabel oder ob leeres RowVersion als 400/409 gewertet werden soll.

- [ ] **ApplicationContentView.razor:136** — `OpenSwaggerImportAsync` prüft `diff.ErrorMessage` nicht: Der Swagger-Dialog öffnet sich auch bei einem Import-Fehler, statt die Fehlermeldung im Hero anzuzeigen. Der OData-Pfad (`OpenODataImportAsync`) macht dies korrekt. Fix: analog zu `OpenODataImportAsync` eine `if (diff.ErrorMessage != null)` Prüfung einbauen.

- [ ] **ApplicationImportController.cs:47** — Beide Import-Endpunkte (`/import/swagger`, `/import/odata`) geben HTTP 200 zurück, auch wenn `diff.ErrorMessage != null`. Client kann Fehler nur am `ErrorMessage`-Feld erkennen, nicht am Status-Code. Erwägen: bei `ErrorMessage != null` HTTP 422 (Unprocessable) oder 200 mit Fehlerfeld beibehalten (konsistent mit existierendem Swagger-Import-Muster prüfen).

- [ ] **ApplicationContentView.razor:138** — `OpenSwaggerImportAsync` setzt `_errorMessage` nicht auf `null` zurück vor dem Import-Aufruf. Eine vorherige OData-Fehlermeldung bleibt sichtbar, wenn anschließend der Swagger-Dialog geöffnet wird. Fix: `_errorMessage = null;` als erste Zeile in `OpenSwaggerImportAsync` ergänzen (analog `OpenODataImportAsync`).

## Rückmeldung vom Kunden

- [ ] **ApplicationImportController — Endpunkte zusammenführen** — `/import/swagger` und `/import/odata` sollen zu einem einzigen Endpunkt `POST /api/applications/{id}/import` zusammengeführt werden. Begründung: Jede Anwendung hat genau einen Interface-Typ (Swagger oder OData); der Controller kann den Typ aus der Anwendung lesen und intern den richtigen Import-Service auswählen. `IApplicationApiClient` erhält eine einzige Methode `ImportMetadataAsync(int applicationId)` statt zwei getrennter Methoden.
