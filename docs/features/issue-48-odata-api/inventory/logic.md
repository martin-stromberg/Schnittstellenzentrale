# Logik

## `ODataImportService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ODataImportService.cs`

Implementiert `IODataImportService`. Abhängigkeiten: `IHttpClientFactory`, `IEndpointRepository`, `ILogger<ODataImportService>`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `ImportAsync(Application)` | `public async` | Lädt `$metadata`-XML per HTTP, parst es via `CsdlReader.Parse`, erzeugt GET/POST-Endpunkte pro EntitySet und GET/POST-Endpunkte pro Operation, berechnet Diff gegen bestehende Endpunkte via `ImportDiffCalculator.Calculate` |
| `ApplyDiffAsync(ImportDiff)` | `public async` | Iteriert `NewEndpoints`, `ChangedEndpoints` und `RemovedEndpoints` und ruft die entsprechenden Repository-Methoden auf |

### Fehlerbehandlung in `ImportAsync`

- `HttpRequestException` beim HTTP-Abruf → gibt `ImportDiff { ErrorMessage = "HTTP-Fehler …" }` zurück
- `XmlException` beim Parsen → gibt `ImportDiff { ErrorMessage = "Ungültiges XML …" }` zurück
- Allgemeine `Exception` beim Parsen → gibt `ImportDiff { ErrorMessage = "Fehler beim Parsen …" }` zurück
- Leere `InterfaceUrl` → gibt leeres `ImportDiff` (ohne `ErrorMessage`) zurück

Abonnierte Events: keine
Publizierte Events: keine
