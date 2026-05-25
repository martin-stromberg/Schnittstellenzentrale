# Offene Aufgaben

Erstellt am: 2026-05-25
Abbruchgrund: Maximale Iterationsanzahl erreicht

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

Keine.

## Code-Review-Befunde

- [x] **SwaggerImportService.cs – God-Methode:** `ImportAsync` (~70 Zeilen) erledigt fünf konzeptuell getrennte Aufgaben: URL-Validierung, HTTP-Abruf, OpenAPI-Parsing, Endpoint-Mapping und Diff-Berechnung. Empfehlung: in private Hilfsmethoden `FetchSwaggerStreamAsync`, `ParseSwaggerDocument` und `MapToEndpoints` aufteilen.
- [x] **SwaggerImportService.cs – Redundante Objektkonstruktion:** In `ImportAsync` (Z. 92–99) wird ein neues `ImportDiff` erstellt, das alle Felder des berechneten Diffs einzeln kopiert, nur um `BearerTokens` ergänzen zu können (da `init`-Property). Empfehlung: `ImportDiff` um eine Methode `WithBearerTokens(IDictionary<string, string>)` ergänzen, oder `ImportDiffCalculator.Calculate` um `BearerTokens`-Parameter erweitern.
- [x] **SwaggerImportServiceTests.cs – Doppelter Code:** Das `Application`-Objekt mit identischen Werten wird in 8 Testmethoden dupliziert. Empfehlung: private statische Hilfsmethode `CreateTestApplication()` einführen.
