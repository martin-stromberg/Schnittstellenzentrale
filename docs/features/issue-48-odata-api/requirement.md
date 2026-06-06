# Anforderungsübersetzung: OData-Metadaten-Import

## Fachliche Zusammenfassung

Die Detailansicht einer Anwendung (`ApplicationCard`) wird um eine Import-Funktion für OData-Schnittstellen erweitert. Analog zur bestehenden Swagger-Import-Funktion (`SwaggerImportService` / `SwaggerImportDialog`) soll für Anwendungen vom Typ `InterfaceType.OData` ein Schaltfläche „OData-Import" erscheinen. Ein Klick darauf ruft das CSDL-Metadaten-Dokument von der konfigurierten `InterfaceUrl` der Anwendung ab, leitet daraus Endpunkte ab und präsentiert einen Diff aus neuen, geänderten und entfernten Endpunkten. Nach Bestätigung durch den Anwender werden die Endpunkte automatisch in der Datenbank angelegt oder aktualisiert.

Die Umsetzung folgt dem bestehenden Pattern: `IODataImportService.ImportAsync` → `ImportDiff` → `ODataImportDialog` (wiederverwendet `ImportDialog`) → `IODataImportService.ApplyDiffAsync` → `IEndpointRepository`.

## Betroffene Klassen und Komponenten

### Neu zu erstellende Artefakte

| Artefakt | Typ | Beschreibung |
|----------|-----|--------------|
| `IODataImportService` | Interface (Core) | Analog zu `ISwaggerImportService`; Methoden `ImportAsync(Application)` und `ApplyDiffAsync(ImportDiff)` |
| `ODataImportService` | Klasse (Infrastructure) | Implementierung: HTTP-Abruf des CSDL-Dokuments via `IHttpClientFactory`, Parsing mit `Microsoft.OData.Edm.Csdl.CsdlReader`, Abbildung von Entity-Sets und Operationen auf `Endpoint`-Objekte, Diff-Berechnung via `ImportDiffCalculator.Calculate` |
| `ODataImportDialog` | Razor-Komponente | Analog zu `SwaggerImportDialog`; delegiert an `ImportDialog` und ruft `IODataImportService.ApplyDiffAsync` auf |

### Zu erweiternde bestehende Artefakte

| Artefakt | Änderung |
|----------|----------|
| `ApplicationCard.razor` | Conditional-Button „OData-Import" bei `InterfaceType.OData` (analog zum Swagger-Button bei `InterfaceType.Rest`); Aufruf von `IODataImportService.ImportAsync`; State-Felder `_showODataImport` und `_odataDiff` |
| `SharedResources.resx` / `SharedResources.de.resx` | Schlüssel `ApplicationCard_Button_ODataImport`, `ODataImportDialog_Title` |
| DI-Registrierung (`Program.cs` o. Ä.) | `IODataImportService` → `ODataImportService` als Scoped-Service registrieren |

### Tests

| Artefakt | Typ | Beschreibung |
|----------|-----|--------------|
| `ODataImportServiceTests` | Unit-Test | Gemockter `HttpMessageHandler`; prüft `ImportAsync` bei neuem, geändertem und entferntem CSDL-Inhalt sowie Fehlerfall (HTTP-Fehler, ungültiges XML) |
| Integrationstests | `WebApplicationFactory` | Import-Endpunkt-Abgleich mit realer Datenbank (analog zum Pattern der Swagger-Integrationstests) |
| `ODataImportTests` (Playwright) | E2E-Test | Anlegen einer OData-Anwendung, Klick auf „OData-Import", Vorschau prüfen, Übernehmen, importierte Endpunkte im Navigationsbaum verifizieren |

## Implementierungsansatz

1. **Metadatenabruf:** `ODataImportService.ImportAsync` lädt den CSDL-XML-Inhalt von `application.InterfaceUrl` per `IHttpClientFactory`. HTTP-Fehler führen zu `ImportDiff { ErrorMessage = ... }`.

2. **Parsing:** Das XML wird mit `CsdlReader.Parse(XmlReader)` aus `Microsoft.OData.Edm.Csdl` in ein `IEdmModel` überführt. XML-Fehler und allgemeine Ausnahmen werden als `ImportDiff { ErrorMessage = ... }` zurückgegeben.

3. **Endpunkt-Abbildung:** Für jedes `IEdmEntitySet` aus `model.EntityContainer.EntitySets()` werden mindestens GET- und POST-Endpunkte (`HttpMethod.GET` / `HttpMethod.POST`) erzeugt, mit `RelativePath = entitySet.Name` und `ApplicationId`. Zusätzlich können `IEdmOperation`-Elemente (Actions → POST, Functions → GET) abgebildet werden.

4. **Diff-Berechnung:** `ImportDiffCalculator.Calculate(existingEndpoints, importedEndpoints)` wird identisch zum Swagger-Pfad aufgerufen und liefert ein `ImportDiff`-Objekt.

5. **UI-Integration:** `ApplicationCard` injiziert `IODataImportService`. Der Button ist nur sichtbar, wenn `_application.InterfaceType == InterfaceType.OData`. Der Dialog-Fluss ist identisch zum Swagger-Import-Fluss.

6. **Anwenden:** `ApplyDiffAsync` iteriert über `diff.NewEndpoints`, `diff.ChangedEndpoints` und `diff.RemovedEndpoints` und delegiert an `IEndpointRepository.AddEndpointAsync`, `UpdateEndpointAsync` und `DeleteEndpointAsync`.

**Wiederverwendete Mechanismen:** `ImportDiff`, `ImportDiffCalculator`, `ImportDialog` (Razor), `IEndpointRepository`.

**Abhängigkeiten:** `Microsoft.OData.Edm` (NuGet, bereits im Projekt vorhanden via `Microsoft.AspNetCore.OData`).

## Konfiguration

Kein zusätzlicher Konfigurationsbedarf. Die Metadaten-URL wird aus dem bereits vorhandenen Feld `Application.InterfaceUrl` gelesen, das bei OData-Anwendungen auf das CSDL-Dokument zeigen soll (z. B. `https://host/service/$metadata`). Der Typ der Anwendung wird aus `Application.InterfaceType` ermittelt, das bereits automatisch bei der Anlage gesetzt wird.

## Offene Fragen

1. **Endpunkt-Vollständigkeit:** Der aktuelle Entwurf erzeugt für jeden Entity-Set GET- und POST-Endpunkte. Sollen auch PUT, PATCH und DELETE je Entity-Set generiert werden? Falls ja: mit Pfad-Parameter (`{key}`) in `RelativePath`?

2. **Key-Format in Pfaden:** OData-Schlüsselzugriffe verwenden die Notation `EntitySet({key})`. Soll diese Konvention im `RelativePath` abgebildet werden, oder wird zunächst nur der Collection-Pfad ohne Schlüssel erzeugt?

3. **Authentifizierung:** Der Swagger-Import unterstützt `x-sz-bearer-token`. Ist für den OData-Import eine vergleichbare Mechanik geplant, oder werden Authentifizierungseinstellungen manuell gepflegt?

4. **Ordnerstruktur:** Der Swagger-Import verwendet `EndpointGroupHelper.ResolveGroupIdAsync` zur Gruppenableitung aus dem Pfad. Sollen OData-Endpunkte ebenfalls in Ordner einsortiert werden (z. B. ein Ordner je Entity-Set)?

5. **Fehlerbehandlung im Dialog:** Bei einem Fehler in `ImportAsync` (z. B. nicht erreichbare Metadata-URL) — soll der Dialog gar nicht öffnen (nur eine Fehlermeldung in `ApplicationCard`), oder soll ein leerer Dialog mit Fehlermeldung angezeigt werden? (Analog zum Swagger-Import: `ImportDiff.ErrorMessage` wird im `ImportDialog` angezeigt.)
