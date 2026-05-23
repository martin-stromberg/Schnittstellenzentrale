# Bestandsaufnahme: Automatische Endpunktregistrierung der eigenen API beim Start

Analysiert wurde die Infrastruktur für den geplanten `SystemEndpointSyncService`, der nach dem App-Start einen Abgleich zwischen Datenbank-Endpunkten und der eigenen Swagger-Definition durchführt. Der Fokus lag auf den Klassen und Interfaces, die der neue Service direkt verwendet oder auf die er sich stützt.

## Zusammenfassung

- `ISwaggerImportService` / `SwaggerImportService` sind vollständig vorhanden: `ImportAsync` und `ApplyDiffAsync` sind implementiert; `ApplyDiffAsync` verarbeitet alle drei Diff-Kategorien inkl. `ChangedEndpoints`.
- `ImportDiffCalculator` ist vorhanden und berechnet den Diff anhand des Schlüssels `Method:RelativePath` (`BuildKey`); `HasChanged` vergleicht `Name`, `Body` und `AuthenticationType`.
- `ImportDiff` mit den drei Listen `NewEndpoints`, `ChangedEndpoints`, `RemovedEndpoints` und einem optionalen `ErrorMessage`-Feld ist vorhanden.
- `IEndpointRepository.AddEndpointAsync` und `DeleteEndpointAsync` sind vorhanden und einsatzbereit.
- `IApplicationRepository.GetSystemGroupAsync` ist vorhanden und liefert die Systemgruppe mit ihren Anwendungen.
- `SystemEntryInitializer` ist vorhanden und setzt `InterfaceUrl` der Systemanwendung auf `{Api:BaseUrl}/swagger/v1/swagger.json`.
- `Program.cs` registriert alle benötigten Scoped-Dienste; kein `AddHostedService`-Aufruf ist vorhanden.
- `IServiceScopeFactory` als Konvention fur Singleton-zu-Scoped-Auflosung ist im Projekt noch nicht etabliert — weder `SystemEntryInitializer` noch ein anderer Hosted Service nutzt diesen Ansatz (nur `SystemEntryInitializer` als statische Methode mit explizit übergebenem `IServiceProvider`).
- `SystemEndpointSyncService` existiert noch nicht.
- Keine Tests für `SystemEndpointSyncService` vorhanden.

## Details

- [Datenmodell](inventory/models.md)
- [Logik](inventory/logic.md)
- [Interfaces](inventory/interfaces.md)
- [Tests](inventory/tests.md)
