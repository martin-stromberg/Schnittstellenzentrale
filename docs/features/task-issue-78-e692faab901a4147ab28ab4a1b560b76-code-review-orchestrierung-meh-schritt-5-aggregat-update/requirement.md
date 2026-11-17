# Übersetzte Anforderung: Einheitliches Aggregat-Update für Endpunkte

Issue-Bezug: #92 (Teilschritt 5 der Code-Review-Orchestrierung, übergeordnetes Issue #78)
Basisbranch: `task/issue-78-e692faab901a4147ab28ab4a1b560b76-code-review-orchestrierung-meh`

## Fachliche Zusammenfassung

Das vollständige Ersetzen eines Endpunkts — alle Skalarwerte (Name, Methode, Pfad, Body, Authentifizierung, Skripte, Gruppenzugehörigkeit, Version) sowie die Kind-Mengen Header und Query-Parameter — soll als genau eine gemeinsame serverseitige Operation mit eindeutiger Semantik bereitstehen und von allen mutierenden Pfaden einheitlich genutzt werden: dem REST-Update-Endpunkt (`EndpointsController`), den OData-Update-Operationen (`ODataEndpointsController`, Put/Patch) und den Import-Apply-Pfaden (`SwaggerImportService`, `ODataImportService`). Die heutige redundante Komposition im REST-Endpunkt (Skalar-Update plus manuelle Einzel-Lösch-/Anlege-Aufrufe pro Kind) entfällt zugunsten der einen Aggregat-Operation. Auf der OData-Seite werden mitgelieferte Header/Query-Parameter künftig über denselben Aggregat-Update-Pfad gepflegt statt still ignoriert zu werden. Der REST-Vertrag und das Optimistic-Concurrency-Verhalten über das Versionsfeld bleiben unverändert. Die redundante Sofort-Kinder-Pflege in der Endpunkt-Detailseite (`EndpointPage.razor`) darf entfallen, solange das Speichern weiterhin die Vollmenge korrekt übermittelt.

## Fachlicher Scope

- **Verarbeitungseinheit:** Ein Endpunkt-Aggregat = ein `Endpoint`-Datensatz mit seinen Skalarwerten und den vollständigen Kind-Mengen `Header` und `Query-Parameter`. Das Aggregat-Update ersetzt stets die Vollmenge der Kinder (Full-Replace-Semantik).
- **Auslösende Ereignisse:**
  - HTTP PUT auf den REST-Update-Endpunkt für Endpunkte (`EndpointsController.UpdateAsync`)
  - OData Put- und Patch-Operationen auf Endpunkte (`ODataEndpointsController`)
  - Import-Apply-Pfade in `SwaggerImportService` und `ODataImportService`, die bestehende Endpunkte aktualisieren
  - Speichern in der Endpunkt-Detailseite der Blazor-Oberfläche (`EndpointPage.razor`)
- **Erwartete Wirkung:**
  - Nach jedem Update-Pfad gilt identische Persistenzsemantik: Skalare werden aktualisiert, mitgelieferte Kinder ersetzen vollständig die bisherigen Kind-Mengen.
  - OData-Updates mit mitgelieferten Headern/Query-Parametern persistieren diese Kinder (werden nicht mehr still ignoriert).
  - Versionskonflikt (Optimistic Concurrency über das Versionsfeld) verhält sich auf allen Pfaden unverändert.
  - Einheitliche Aggregat-Semantik ist in der API- bzw. OData-API-Dokumentation festgehalten.
- **Umgang mit fehlenden Objekten:**
  - Ziel-Endpunkt existiert nicht: bestehendes Verhalten der jeweiligen Pfade bleibt (REST: 404/NotFound; Import: Anlegen statt Aktualisieren, sofern heute so implementiert).
  - Update-Payload enthält keine Kinder (leere/nicht gesetzte Kind-Mengen): Semantik muss im Plan eindeutig festgelegt werden — naheliegend: nicht gesetzte Kind-Mengen werden wie bisher behandelt (REST-Vertrag: Vollmengen-Ersetzung ist bereits Vertragsbestandteil); für OData ist das Verhalten bei fehlenden Kindern im Plan festzulegen (keine Flags für optionale Kind-Pflege).
- **Scope-Ausschlüsse:**
  1. Keine Änderung des REST-Vertrags — keine Flags für optionale Kind-Pflege, Vollmengen-Ersetzung der Kinder bleibt wie vereinbart Teil des bestehenden Vertrags. (Aus Anforderung ableitbar; als bestätigt anzusehen, da ausdrücklich im Auftrag festgelegt.)
  2. Keine Änderung am Optimistic-Concurrency-Verhalten über das Versionsfeld. (Ebenfalls ausdrücklich im Auftrag festgelegt.)
  3. Entfernung der Sofort-Kinder-Pflege in `EndpointPage.razor` ist optional („darf entfallen") — kein Zwang, sie zu entfernen; falls entfernt, muss das Speichern weiterhin die Vollmenge übermitteln.

## Betroffene Klassen und Komponenten

- `EndpointsController` (`src/Schnittstellenzentrale/Controllers/EndpointsController.cs`, `UpdateAsync` ~Z.150–206) — redundante Komposition aus Skalar-Update + Einzel-Kind-Pflege entfällt zugunsten der Aggregat-Operation.
- `ODataEndpointsController` (`src/Schnittstellenzentrale/OData/ODataEndpointsController.cs`, Put/Patch ~Z.97–191) — Kind-Pflege über denselben Aggregat-Update-Pfad.
- Import-Apply-Pfade: `SwaggerImportService`, `ODataImportService` — Nutzung derselben Aggregat-Operation.
- `EndpointPage.razor` (`src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor`, Sofort-Kinder-Pflege ~Z.444–474) — optionale Entfernung der parallelen Persistenz pro Panel-Änderung.
- Repository-/Service-Ebene (`IEndpointRepository` bzw. Endpoint-Service): Bereitstellung der einen Aggregat-Update-Operation (Skalare + Kinder in einem Schritt).
- `IApplicationApiClient` / API-Client-Schicht: ggf. betroffen, falls die UI über den Client die Vollmenge übermittelt.
- Dokumentation: `docs/help/api/api.md`, `docs/help/api/odata-api.md` (Festhalten der Aggregat-Semantik).
- Tests: Integrations-Tests der Aggregat-Operation (Skalare + Kinder in einem Schritt, Versions-Konflikt), OData- und REST-Update-Tests der einheitlichen Semantik.

## Implementierungsansatz

- Eine serverseitige Aggregat-Update-Operation auf Repository-/Service-Ebene, die Skalare und Kind-Vollmengen (Header, Query-Parameter) in einem Schritt ersetzt — inklusive Versionsprüfung (Optimistic Concurrency).
- Alle mutierenden Pfade (REST, OData, Import) delegieren auf diese eine Operation; die redundante Einzel-Kind-Pflege im REST-Endpunkt entfällt.
- Tests gemäß Projektkonvention: Unit-Tests mit gemocktem `HttpMessageHandler` bzw. gemockten Abhängigkeiten plus Integrationstests mit `WebApplicationFactory`.

## Konfiguration

Keine Konfigurationsänderungen erforderlich.

## Offene Fragen

Keine — die Anforderung legt Vertrag, Semantik und Constraints eindeutig fest. Einzige konkretisierungsbedürftige Detailentscheidung (für den Plan, nicht für den Anwender): exakte Semantik bei OData-Updates ohne mitgelieferte Kinder.
