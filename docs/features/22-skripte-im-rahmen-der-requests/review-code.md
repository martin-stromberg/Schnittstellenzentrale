# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### SwaggerImportService.cs (SwaggerImportService)

- **God-Methode** — `ImportAsync` (Zeilen 31–100, ~70 Zeilen) erledigt fünf konzeptuell getrennte Aufgaben hintereinander: URL-Validierung, HTTP-Abruf, OpenAPI-Parsing mit Fehlerbehandlung, Endpoint-Mapping und Diff-Berechnung.

  Empfehlung: Methode in private Hilfsmethoden aufteilen, z. B. `FetchSwaggerStreamAsync`, `ParseSwaggerDocument` und `MapToEndpoints`, sodass `ImportAsync` nur noch orchestriert.

- **Doppelter Code / unnötige Objektkonstruktion** — Zeilen 92–99: `ImportDiffCalculator.Calculate` gibt ein vollständiges `ImportDiff` zurück. Anschließend wird ein neues `ImportDiff` erstellt, das alle vier Felder des berechneten Diffs einzeln kopiert und `BearerTokens` ergänzt. Da `ImportDiff.BearerTokens` ein `init`-Property ist, könnte `BearerTokens` direkt im `Calculate`-Ergebnis nicht gesetzt werden — aber das Kopieren aller anderen Felder ist redundant und fehleranfällig, wenn `ImportDiff` um weitere Properties erweitert wird.

  Empfehlung: `BearerTokens` nach der Berechnung in einem separaten Schritt befüllen, oder `ImportDiffCalculator.Calculate` so erweitern, dass es `BearerTokens` als Parameter entgegennimmt und direkt im Ergebnis setzt. Alternativ: `ImportDiff` um eine Methode `WithBearerTokens(IDictionary<string, string>)` ergänzen, die eine Kopie mit gesetzten Tokens zurückgibt.

### SwaggerImportServiceTests.cs (SwaggerImportServiceTests)

- **Doppelter Code** — Das `Application`-Objekt mit identischen Werten wird in 8 von 8 Testmethoden (Zeilen 59, 79, 97, 126, 155, 184, 200, 233) ohne Extraktion dupliziert.

  Empfehlung: Eine private statische Hilfsmethode oder Konstante `CreateTestApplication()` einführen, die das gemeinsame `Application`-Objekt zurückgibt, und in allen Testmethoden aufrufen.

## Geprüfte Dateien

- `src/Schnittstellenzentrale.Core/Models/ImportDiff.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/ImportDiffCalculator.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/SwaggerImportService.cs`
- `src/Schnittstellenzentrale.Tests/Services/SwaggerImportServiceTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/ImportDiffCalculatorTests.cs`
