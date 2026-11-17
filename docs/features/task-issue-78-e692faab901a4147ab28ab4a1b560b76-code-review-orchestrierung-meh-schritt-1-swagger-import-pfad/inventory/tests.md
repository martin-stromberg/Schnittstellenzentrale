# Bestehende Testabdeckung

## `SystemEndpointSyncServiceTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/SystemEndpointSyncServiceTests.cs` (380 Zeilen, 13 Tests)

- `TestableSyncService` (ruft `ExecuteAsync` via `RunAsync`), `CreateService()` baut `ServiceCollection` mit Mocks für `IApplicationRepository`, `ISwaggerProvider`, `IEndpointRepository`, `ISwaggerImportService`.
- Charakterisiert das **Ist-Verhalten**: verifiziert `NewEndpoints`/`RemovedEndpoints`/Inhalte des an `ApplyDiffAsync` übergebenen Diffs (Negotiate-Auth, Bearer-Token, Pre-/Post-Skripte). `ChangedEndpoints` wird nirgends geprüft.
- **Anpassungsbedarf:** `IEndpointRepository` aus der DI entfernen (Nachweis, dass der Dienst die Persistenz nicht mehr direkt nutzt); statt Diff-Inhalten wird `SyncFromDocumentAsync(document, appId)` verifiziert. Die inhaltlichen Mapping-/Diff-Assertions wandern zu `SwaggerImportServiceTests`.

## `SwaggerImportServiceTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/SwaggerImportServiceTests.cs` (424 Zeilen, 13 Tests)

- Deckt `ImportAsync` (New/Changed/Removed, Skripte, Bearer-Token, Re-Import-Reset) und `ApplyDiffAsync` (Token-Save, Gruppen-Auflösung aus Pfadsegmenten, Wiederverwendung bestehender Gruppen) ab.
- **Ergänzungsbedarf:** `SyncFromDocumentAsync` — End-to-End im Service: Dokument-Objekt → `AddEndpointAsync`/`UpdateEndpointAsync`/`DeleteEndpointAsync`-Aufrufe, Bearer-Token → `SavePassword`, Fehler-Diff (Duplikatschlüssel im Bestand) → keine Repository-Schreibzugriffe. `OpenApiDocument` kann wie in `SystemEndpointSyncServiceTests` direkt als Objektgraph aufgebaut werden (kein HTTP-Mock nötig).

## `ImportDiffCalculatorTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/ImportDiffCalculatorTests.cs` (112 Zeilen, 5 Tests)

- Deckt `ChangedEndpoints`-Semantik (Skript-Diff, Reset auf null) und Serialisierbarkeit ab.
- **Lücke:** Duplikatschlüssel-Erkennung (`ErrorMessage`-Zweige) ist nicht getestet — ergänzen.

## Test-Infrastruktur

- Unit-Tests: `dotnet test --filter "FullyQualifiedName!~Playwright"`.
- Playwright läuft über `PlaywrightServer` (Kestrel, Port 5099); `WebApplicationFactory`-Tests entfernen alle `IHostedService` — `SystemEndpointSyncService` läuft in Integrationstests bewusst nicht.
- Kein UI-Bestandteil dieser Anforderung → keine neuen Playwright-Tests nötig; bestehende müssen grün bleiben.
