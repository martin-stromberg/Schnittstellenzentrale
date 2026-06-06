# Umsetzungsplan: OData-Metadaten-Import – Fehlende Tests und ApplyDiffAsync-Entscheidung

## Übersicht

Der Kern des Features (IODataImportService, ODataImportService, ODataImportDialog, ApplicationCard-Button) ist auf dem Branch vollständig implementiert. Dieser Plan deckt ausschließlich die noch offenen Arbeiten ab: fehlende Unit-Tests für Fehlerfälle und ApplyDiffAsync, ein fehlender WebApplicationFactory-Integrationstest sowie die Entscheidung, ob ApplyDiffAsync analog zum SwaggerImportService um Ordnerzuweisung und Bearer-Token-Persistierung ergänzt werden soll.

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|-----------------|------------|
| `ApplyDiffAsync` — Ordnerzuweisung | Keine Ordnerzuweisung via `EndpointGroupHelper` | OData-Entity-Sets sind flache Ressourcennamen (z. B. `Products`, `Orders`) ohne hierarchische Pfad-Segmente. Eine Gruppenableitung aus dem Entity-Set-Namen ergibt fachlich keinen Mehrwert; Endpunkte landen ohne Gruppe direkt unter der Anwendung. Kann nachgelagert manuell oder per separatem Feature ergänzt werden. |
| `ApplyDiffAsync` — Bearer-Token-Persistierung | Keine Bearer-Token-Persistierung | Das CSDL-Format enthält kein proprietäres `x-sz-bearer-token`-Feld. OData-Endpunkte werden zunächst ohne Authentifizierungstyp importiert; Bearer-Token können manuell gesetzt werden. Entspricht dem aktuellen Implementierungsstand. |
| Integrationstest-Scope | Neue Testklasse `ODataImportServiceIntegrationTests` in `Schnittstellenzentrale.Tests/Integration/` | Folgt dem Muster der bestehenden Integrationstest-Klassen; die `ControllerTestFactory` (`WebApplicationFactory`) wird als `IClassFixture` wiederverwendet. |

## Programmabläufe

### Unit-Test: HTTP-Fehlerfall

1. `CreateServiceWithErrorHandler` erstellt einen `ODataImportService` mit einem `HttpMessageHandler`-Mock, der eine `HttpRequestException` wirft.
2. `ImportAsync` wird mit einer `Application` mit gültiger `InterfaceUrl` aufgerufen.
3. Die zurückgegebene `ImportDiff` wird auf nicht-null `ErrorMessage` geprüft.

Beteiligte Klassen/Komponenten: `ODataImportService`, `ODataImportServiceTests`

---

### Unit-Test: Ungültiges-XML-Fehlerfall

1. `CreateService` erstellt einen `ODataImportService` mit einem Handler, der HTTP 200 mit ungültigem XML-Inhalt (kein gültiges CSDL) zurückgibt.
2. `ImportAsync` wird aufgerufen.
3. Die zurückgegebene `ImportDiff` wird auf nicht-null `ErrorMessage` geprüft.

Beteiligte Klassen/Komponenten: `ODataImportService`, `ODataImportServiceTests`

---

### Unit-Test: Leere URL

1. Eine `Application` mit leerem oder null `InterfaceUrl` wird an `ImportAsync` übergeben.
2. Die zurückgegebene `ImportDiff` wird auf null `ErrorMessage` und leere Listen geprüft (laut Implementierung: leere `ImportDiff` ohne Fehler).

Beteiligte Klassen/Komponenten: `ODataImportService`, `ODataImportServiceTests`

---

### Unit-Test: ApplyDiffAsync

1. `ApplyDiffAsync` wird mit einem `ImportDiff` aufgerufen, der je einen Eintrag in `NewEndpoints`, `ChangedEndpoints` und `RemovedEndpoints` enthält.
2. Die Repository-Mocks (`AddEndpointAsync`, `UpdateEndpointAsync`, `DeleteEndpointAsync`) werden auf je genau einen Aufruf mit den korrekten Endpunkten geprüft.

Beteiligte Klassen/Komponenten: `ODataImportService`, `IEndpointRepository`, `ODataImportServiceTests`

---

### Integrationstest: Import-Endpunkt-Abgleich mit realer Datenbank

1. `ODataImportServiceIntegrationTests` erhält `ControllerTestFactory` als `IClassFixture`.
2. Im Test wird per `factory.Services` ein `IODataImportService` aus dem DI-Container aufgelöst.
3. Eine OData-Anwendung wird über `IApplicationRepository` oder den API-Client in der Test-Datenbank angelegt.
4. `ImportAsync` wird mit einem Mock-HTTP-Handler aufgerufen, der ein gültiges Minimal-CSDL zurückgibt (analog zum `ODataMetadata`-Fixture aus `ODataImportServiceTests`).
5. Die zurückgegebene `ImportDiff` wird auf mindestens einen `NewEndpoints`-Eintrag geprüft.
6. `ApplyDiffAsync` wird mit der zurückgegebenen `ImportDiff` aufgerufen.
7. Über `IEndpointRepository.GetEndpointsAsync` wird verifiziert, dass die Endpunkte in der Datenbank persistiert wurden.

Beteiligte Klassen/Komponenten: `ODataImportService`, `IODataImportService`, `IEndpointRepository`, `ControllerTestFactory`, `ODataImportServiceIntegrationTests`

---

## Neue Klassen

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `ODataImportServiceIntegrationTests` | Testklasse | `WebApplicationFactory`-Integrationstest für `ImportAsync` und `ApplyDiffAsync` mit realer In-Memory-Datenbank |

## Änderungen an bestehenden Klassen

### `ODataImportServiceTests` (Testklasse)

- **Neue Methoden (Testfälle):**
  - `Import_HttpError_ReturnsErrorMessage` — prüft HTTP-Fehlerfall (`HttpRequestException` → `ImportDiff.ErrorMessage` nicht null)
  - `Import_InvalidXml_ReturnsErrorMessage` — prüft ungültiges XML (`XmlException`-Pfad → `ImportDiff.ErrorMessage` nicht null)
  - `Import_EmptyInterfaceUrl_ReturnsEmptyDiff` — prüft leere URL → leere `ImportDiff` ohne Fehler
  - `ApplyDiff_NewChangedRemoved_CallsRepositoryMethods` — prüft, dass `AddEndpointAsync`, `UpdateEndpointAsync` und `DeleteEndpointAsync` je einmal aufgerufen werden
- **Neue Hilfsmethode:** `CreateServiceWithErrorHandler` — erstellt `ODataImportService` mit einem Handler, der eine `HttpRequestException` wirft (alternativ: Erweiterung von `CreateService` mit einem optionalen `HttpStatusCode`-Parameter, analog zum Swagger-Test-Pattern)

## Datenbankmigrationen

Keine.

## Validierungsregeln

Keine.

## Konfigurationsänderungen

Keine.

## Seiteneffekte und Risiken

- **`ApplyDiffAsync` ohne Ordnerzuweisung:** Importierte OData-Endpunkte werden ohne `EndpointGroupId` angelegt. Dies ist eine bewusste Entscheidung (s. Designentscheidungen) und entspricht dem aktuellen Stand. Kein Risiko für bestehende Features.
- **`ApplyDiffAsync` ohne Bearer-Token:** Kein `ICredentialService` wird in `ODataImportService` injiziert. Sollte Bearer-Token-Unterstützung nachgerüstet werden, muss der Konstruktor erweitert und die DI-Registrierung angepasst werden.

## Umsetzungsreihenfolge

1. Fehlende Unit-Tests in `ODataImportServiceTests` ergänzen (HTTP-Fehler, ungültiges XML, leere URL, `ApplyDiffAsync`).
2. Neue Testklasse `ODataImportServiceIntegrationTests` erstellen.

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `Import_HttpError_ReturnsErrorMessage` | `ODataImportServiceTests` | `HttpRequestException` führt zu `ImportDiff` mit nicht-null `ErrorMessage` |
| `Import_InvalidXml_ReturnsErrorMessage` | `ODataImportServiceTests` | Ungültiger XML-Content führt zu `ImportDiff` mit nicht-null `ErrorMessage` |
| `Import_EmptyInterfaceUrl_ReturnsEmptyDiff` | `ODataImportServiceTests` | Leere `InterfaceUrl` → leere `ImportDiff`, kein `ErrorMessage`, alle Listen leer |
| `ApplyDiff_NewChangedRemoved_CallsRepositoryMethods` | `ODataImportServiceTests` | `AddEndpointAsync`, `UpdateEndpointAsync`, `DeleteEndpointAsync` werden je genau einmal aufgerufen |
| `CreateServiceWithErrorHandler` (Hilfsmethode) | `ODataImportServiceTests` | Erstellt `ODataImportService` mit einem Handler, der `HttpRequestException` wirft |
| `Import_NewODataApplication_PersistsEndpoints` | `ODataImportServiceIntegrationTests` | Vollständiger Import-Zyklus (`ImportAsync` + `ApplyDiffAsync`) gegen reale In-Memory-Datenbank: Endpunkte sind nach `ApplyDiffAsync` per Repository abrufbar |

### Betroffene bestehende Tests

Keine.

### E2E-Tests (Pflicht)

Die Playwright-Tests `ODataImportTests` (`ImportOData_RecognizesODataType_AndImportsEndpoints` und `ImportOData_CrudOperation_PersistsChange`) sind bereits vorhanden und decken den Happy Path ab. Kein neuer E2E-Test erforderlich.

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| — | Keine bestehenden E2E-Tests betroffen. |

## Offene Punkte

Keine.
