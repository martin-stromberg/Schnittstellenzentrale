# Bestandsaufnahme: OData-Metadaten-Import

Analysiert wurde der Branch `issue-48-odata-api` bezogen auf die Anforderung „OData-Metadaten-Import": Erweiterung der `ApplicationCard` um eine Import-Funktion für OData-Schnittstellen analog zum bestehenden Swagger-Import-Pattern.

## Zusammenfassung

- `IODataImportService` ist vollständig vorhanden: zwei Methoden `ImportAsync` und `ApplyDiffAsync`, identische Signatur zu `ISwaggerImportService`.
- `ODataImportService` ist vollständig implementiert: HTTP-Abruf, CSDL-Parsing via `CsdlReader`, Entity-Set- und Operation-Abbildung, Diff-Berechnung via `ImportDiffCalculator`, Fehlerbehandlung für HTTP- und XML-Fehler, `ApplyDiffAsync` mit direkten Repository-Aufrufen.
- `ODataImportDialog.razor` ist vollständig vorhanden: delegiert an `ImportDialog` und ruft `IODataImportService.ApplyDiffAsync` auf.
- `ApplicationCard.razor` ist vollständig erweitert: OData-Import-Button bei `InterfaceType.OData`, State-Felder `_showODataImport` / `_odataDiff`, `OpenODataImport`- und `CloseODataImport`-Methoden.
- Lokalisierungsschlüssel `ODataImportDialog_Title` und `ApplicationCard_Button_ODataImport` sind in `SharedResources.resx` und `SharedResources.de.resx` vorhanden.
- DI-Registrierung `IODataImportService → ODataImportService` als Scoped-Service ist in `Program.cs` vorhanden.
- Die eigene OData-API (`/odatav4/…`) ist vollständig implementiert: vier CRUD-Controller (`Applications`, `ApplicationGroups`, `Endpoints`, `EndpointGroups`), Authentifizierungsendpunkt (`/odatav4/authenticate`), EDM-Modell-Builder, Token-Validierung in `ODataControllerBase`.
- Unit-Tests (`ODataImportServiceTests`, `ODataImportServiceRealMetadataTests`) und Playwright-Tests (`ODataImportTests`) sind vorhanden.
- Integrationstests für die OData-Controller (`ODataControllerIntegrationTests`) sind umfangreich vorhanden (23 Tests).
- **Lücke Unit-Tests:** Fehlerfälle (`HttpRequestException`, `XmlException`, leere URL) und `ApplyDiffAsync` sind in `ODataImportServiceTests` nicht abgedeckt (Swagger-Import-Tests haben diese Fälle).
- **Lücke Integrationstests:** Ein `WebApplicationFactory`-Integrationstest für den Import-Endpunkt-Abgleich (analog zum Swagger-Import-Pattern) fehlt.
- `ApplyDiffAsync` im `ODataImportService` weist im Vergleich zum `SwaggerImportService` zwei bewusste Vereinfachungen auf: keine Ordner-Zuweisung via `EndpointGroupHelper` und keine Bearer-Token-Persistierung — diese Aspekte sind laut Anforderung als offene Fragen eingestuft.

## Details

- [Datenmodell](inventory/models.md)
- [Logik](inventory/logic.md)
- [Enums](inventory/enums.md)
- [Interfaces](inventory/interfaces.md)
- [UI-Komponenten](inventory/ui.md)
- [Tests](inventory/tests.md)
