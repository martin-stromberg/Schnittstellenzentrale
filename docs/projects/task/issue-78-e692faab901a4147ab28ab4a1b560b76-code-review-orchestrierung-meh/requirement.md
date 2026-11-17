# Übersetzte Anforderung – Code-Review-Orchestrierung (Issues #87–#92)

## Fachliche Zusammenfassung

Die aus dem Code-Review in Issue #78 abgeleiteten Befunde 1–8 werden in einem gemeinsamen Pull Request gegen `staging` behoben. Kern der Maßnahme ist die Verlagerung mehrstufiger fachlicher Geschäftsvorfälle aus aufrufenden Komponenten (Blazor-UI, `BackgroundService`, Controller) in jeweils genau einen serverseitigen Ausführungspfad: Der Swagger-Import wird über `ISwaggerImportService.SyncFromDocumentAsync` und einen neuen API-Endpunkt `POST /api/applications/{id}/swagger-import/apply` vereinheitlicht, `WorkspacesLayout` greift nicht mehr direkt auf `IEndpointRepository` zu, das Löschen von Anwendungsgruppen mit Inhalt wird atomar über `DELETE /api/application-groups/{id}?content=delete|detach` abgewickelt, die SignalR-Benachrichtigungspolitik wird zentralisiert und auf die OData-Pfade ausgedehnt, und das Endpunkt-Aggregat-Update (Skalare + Header/QueryParameters) erhält eine gemeinsame — oder explizit dokumentiert abweichende — Semantik für REST, OData und Import.

## Fachlicher Scope

### Verarbeitungseinheit

Die Anforderung umfasst vier fachliche Verarbeitungseinheiten:

1. **Ein Swagger-Import-Durchlauf** (Dokument → `ImportDiff` → Apply) — sowohl der manuelle UI-Import als auch der automatische Startabgleich der Systemanwendung. Zum Scope gehört die gesamte Strecke vom `OpenApiDocument` bis zur persistierten Endpunktmenge; nicht dazu gehört der Abruf des Dokuments per HTTP (bleibt in `SwaggerImportService.ImportAsync` bzw. `ISwaggerProvider`).
2. **Ein Gruppen-Löschvorgang mit Inhalt** — das Löschen einer `ApplicationGroup` inklusive aller zugeordneten `Application`-Datensätze, wahlweise mit Löschen (`content=delete`) oder Umhängen in die ungruppierte Menge (`content=detach`) der enthaltenen Anwendungen, als eine atomare Einheit.
3. **Ein Endpunkt-Aggregat-Update** — das vollständige Ersetzen eines `Endpoint`-Aggregats bestehend aus Skalarwerten (Name, Methode, Pfad, Body, Auth, Skripte, `EndpointGroupId`, `RowVersion`) und den Kindern `Headers`/`QueryParameters`, als eine Operation.
4. **Eine Änderungs-Benachrichtigung** — genau eine `ISignalRNotificationService`-Nachricht pro mutierendem Vorgang, unabhängig vom Einstiegspfad (REST-API, OData-API, Blazor-UI, Import, Startabgleich).

### Auslösende Ereignisse

- App-Start: `SystemEndpointSyncService` führt den selektiven Endpunktabgleich der Systemanwendung durch.
- Nutzeraktion in der Blazor-UI: Bestätigen des `SwaggerImportDialog`, Bestätigen des `ConfirmDeleteGroupDialog` („Alles löschen" / „Nur Gruppe löschen"), Endpunkt-/Endpunktgruppen-CRUD über `WorkspacesLayout`, Speichern in `EndpointPage`.
- Externe API-Aufrufe: `PUT /api/endpoints/{id}`, `DELETE /api/application-groups/{id}?content=…`, `POST /api/applications/{id}/swagger-import/apply`, mutierende OData-Operationen (`POST`/`PUT`/`PATCH`/`DELETE` auf `/odatav4/Endpoints` und den übrigen Entity-Sets).

### Erwartete Wirkung

- Der Startabgleich aktualisiert geänderte System-Endpunkte (`ChangedEndpoints`: Name, Body, `AuthenticationType`, Pre-/Post-Request-Skripte) und erkennt Duplikatschlüssel — bisher wurden nur `New`/`Removed` verarbeitet.
- Der Swagger-Import-Apply läuft über denselben validierten API-Pfad wie der OData-Import (Token-Validierung, Anwendungs-Existenzprüfung, `InterfaceType`-Prüfung mit `422`).
- Es existiert nur noch ein produktiver Ausführungspfad für Endpunkt-/Endpunktgruppen-CRUD aus der UI: der Controller-Pfad mit Kontext-Headern, Team-Notify und konsistenten Nebenbedingungen.
- „Gruppe mit Inhalt löschen" ist serverseitig atomar: bei Fehlschlag bleibt der Bestand konsistent (kein Teilerfolg).
- Jede mutierende Änderung löst im Team-Modus genau eine Notification aus — auch über OData; die Doppelauslösung auf dem UI-Pfad (`EndpointsController.UpdateAsync` + `EndpointPage.OnSaveSuccessAsync`) entfällt.
- OData-`PUT`/`PATCH` auf `Endpoints` ignoriert mitgelieferte `Headers`/`QueryParameters` nicht mehr still — entweder werden Kinder über denselben Aggregat-Update-Pfad gepflegt oder die Abweichung ist explizit dokumentiert.

### Umgang mit fehlenden Objekten

- `POST …/swagger-import/apply` und `DELETE /api/application-groups/{id}?content=…`: Fehlt das Zielobjekt (Anwendung bzw. Gruppe), wird `404` zurückgegeben — es wird nichts erzeugt.
- `content`-Parameter: fehlender oder ungültiger Wert → bewusst zu definierendes Verhalten (Vorschlag: Default = bisheriges Verhalten ohne Parameter, ungültiger Wert = `400`); zu klären, siehe Offene Fragen.
- `ImportDiff` mit `ErrorMessage` beim Startabgleich: wird vom Service wie im bisherigen `ImportAsync`-Pfad behandelt (kein Apply), Fehler wird geloggt — Annahme, zu verifizieren.
- Fehlgeschlagener atomarer Gruppen-Löschvorgang: vollständiges Rollback, Bestand wird wiederverwendet (kein Neuaufbau).

### Scope-Ausschlüsse

- Kein Umbau des Notification-Transports (SignalR/`EndpointHub` bleibt unverändert); es wird nur die Auslösepolitik zentralisiert.
- Kein neuer `IQueryable`-fähiger Repository-Pfad für OData-Abfragen (bestehende In-Memory-Filterung bleibt, ist im Code dokumentiert).
- Keine Änderung an den bestehenden Contract-DTOs (`UpdateEndpointRequest` etc.) außer den für die neuen Endpunkte/Parameter erforderlichen Erweiterungen.
- OData-`PATCH`/`PUT` auf Nicht-`Endpoint`-Sets sowie die Einzel-Endpunkte für Header/QueryParameter (`POST/DELETE /api/endpoints/headers…`, `…/query-parameters…`) bleiben bestehen; ihre Zukunft hängt von der Entscheidung zu #92 ab (siehe Offene Fragen).
- Das UI-seitige `IActivityLogService`-Logging der Nutzeraktionen in `WorkspacesLayout` bleibt erhalten — ausgeschlossen ist nur die redundante Team-Notify-Entscheidung in der Komponente (Annahme auf Basis des Issue-Textes, zu verifizieren).

## Betroffene Klassen und Komponenten

### Interfaces (Schnittstellenzentrale.Core)

- `ISwaggerImportService` — neue Methode `SyncFromDocumentAsync(OpenApiDocument document, int applicationId)` als Pass-Einstieg (Map → Diff → Apply)
- `IApplicationApiClient` — neue Methode `ApplySwaggerDiffAsync(int applicationId, ImportDiff diff)`; `DeleteGroupAsync` um `content`-Variante (`delete`/`detach`) erweitern (Signatur: optionaler Parameter oder eigene Methode — Umsetzungsdetail)
- `IEndpointRepository` — neue Methode `UpdateAggregateAsync(Endpoint endpoint)` (Skalare + Ersetzung der Kinder in einer Operation)
- `IApplicationRepository` — neue Methode für das atomare Gruppen-Löschen mit Inhalt (z. B. `DeleteGroupWithContentAsync(int id, …)`)
- `ISignalRNotificationService` — unverändert; ggf. neuer zentraler Helper für die Notify-Bedingung (`StorageMode == Team`)

### Services / Logik (Schnittstellenzentrale.Infrastructure, Schnittstellenzentrale)

- `SwaggerImportService` — Implementierung von `SyncFromDocumentAsync` unter Wiederverwendung von `SwaggerOperationHelper.MapDocumentToEndpoints` und `ImportDiffCalculator.Calculate`; InterfaceType-/Aggregat-Validierung in `ApplyDiffAsync` verankern
- `SystemEndpointSyncService` — vereinfacht auf: Systemgruppe/Systemanwendung auflösen, `ISwaggerProvider.GetSwagger("v1")` liefern, `SyncFromDocumentAsync` aufrufen; eigener Diff-Aufbau und direkter `IEndpointRepository`-Aufruf entfallen
- `ApplicationRepository` — atomare Gruppenlöschung mit Inhalt (EF-Transaktion über `AppDbContext`)
- `EndpointRepository` — Implementierung von `UpdateAggregateAsync`
- `ApplicationApiClient` — `ApplySwaggerDiffAsync` (analog `ApplyODataDiffAsync`) und erweiterter Gruppen-Delete-Aufruf
- `ImportDiffCalculator` — bleibt `internal`; wird nun auch vom Startabgleich über den Service mitgenutzt (kein Duplikat mehr im Worker)
- Neuer zentraler Notify-Helper/-Service für die Bedingung „Notification nur bei `StorageMode.Team`" (Ablageort: Core-Helper oder Infrastructure-Service — Umsetzungsdetail)

### Controller (Schnittstellenzentrale)

- `ApplicationImportController` — neuer Endpunkt `POST /api/applications/{id:int}/swagger-import/apply` (analog `ApplyODataDiffAsync`: `ValidateTokenAndSetResponseHeaderAsync`, `404` bei fehlender Anwendung, `422` bei `InterfaceType != Rest`)
- `ApplicationGroupsController` — `DeleteAsync` um Query-Parameter `content` (`delete`/`detach`) erweitern, Aufruf der atomaren Repository-Operation, Notify unverändert im Controller
- `EndpointsController` — `UpdateAsync` auf `UpdateAggregateAsync` umstellen (Komposition Skalar-Update + Kinder-Ersetzung + Reload wandert in die Repository-/Service-Schicht)
- `ODataEndpointsController` — `Put`/`Patch` an den gemeinsamen Aggregat-Pfad anschließen **oder** mitgelieferte Kinder explizit ablehnen/dokumentieren; Team-Notify für mutierende Operationen ergänzen
- `ODataApplicationsController`, `ODataApplicationGroupsController`, `ODataEndpointGroupsController` — Team-Notify für mutierende Operationen ergänzen (Umfang siehe Offene Fragen)
- `EndpointGroupsController` — nur insofern betroffen, als `WorkspacesLayout` künftig über den ApiClient hierauf zugreift; kein Verhaltensumbau geplant

### UI-Komponenten (Schnittstellenzentrale)

- `WorkspacesLayout.razor` — `@inject IEndpointRepository` und `@inject ISignalRNotificationService` entfernen; alle Endpunkt-/Endpunktgruppen-Operationen (Zeilen ~272–399) auf `IApplicationApiClient` umstellen; `OnDeleteGroupConfirmedAll`/`OnDeleteGroupConfirmedGroupOnly` durch einen einzigen Aufruf des neuen `content`-Delete ersetzen; eigene `StorageMode`-Notify-Prüfungen entfallen
- `SwaggerImportDialog.razor` — `ISwaggerImportService`-Injection durch `IApplicationApiClient.ApplySwaggerDiffAsync` ersetzen
- `EndpointPage.razor` — Doppel-Notify in `OnSaveSuccessAsync` (Zeilen 481–482) entfernen; redundante Kinder-Pflege (Panel-Edits mit Sofort-Add/Delete vs. Vollmenge im PUT) prüfen und ggf. vereinfachen

### Enums / Contracts (Schnittstellenzentrale.Core)

- Neuer Enum für den `content`-Parameter (z. B. `GroupContentDeleteMode` mit `Delete`/`Detach`) oder stringbasierter Query-Parameter — Umsetzungsdetail
- Keine neuen Response-Contracts erforderlich (Delete: `204`, Apply: `204`)

### Tests (Schnittstellenzentrale.Tests)

- `SystemEndpointSyncServiceTests` — Anpassung auf neue Semantik (`ChangedEndpoints`, Service-Pass statt eigenem Diff)
- `SwaggerImportServiceTests` — Abdeckung `SyncFromDocumentAsync` und Validierung in `ApplyDiffAsync`
- `ApplicationApiClientTests` — gemockter `HttpMessageHandler` für `ApplySwaggerDiffAsync` und Gruppen-Delete mit `content`
- `ApplicationImportControllerIntegrationTests` — `WebApplicationFactory`-Tests für `swagger-import/apply` (inkl. `401`/`404`/`422`)
- `ApplicationGroupsControllerIntegrationTests` — `WebApplicationFactory`-Tests für `DELETE ?content=delete|detach` inkl. Atomarität
- `ODataControllerIntegrationTests` — Notify-/Aggregat-Verhalten der OData-Mutationen
- `EndpointRepositoryIntegrationTests` — `UpdateAggregateAsync`
- Playwright-Suite (`GroupCrudTests`, `EndpointGroupCrudTests`, `SwaggerImportTests`, `SignalRSyncTests`, `LayoutSmokeTests` u. a.) — muss ohne neue Tests grün bleiben; Layout-Kollaps wird weiterhin über `BoundingBoxAsync()` geprüft

### Dokumentation

- `docs/help/api/api.md` / `docs/help/api/odata-api.md` — neue Endpunkte (`swagger-import/apply`, `content`-Parameter) sowie die gewählte OData-Aggregat-Semantik dokumentieren (insbesondere falls #92 als „dokumentierte Absicht" umgesetzt wird)
- `CLAUDE.md` — bleibt korrekt: die explizite `IEndpointRepository`-Ausnahme gilt weiterhin nur für `SystemEndpointSyncService` bzw. wandert mit der Logik in den Service

## Implementierungsansatz

- **#87:** `SyncFromDocumentAsync` in `SwaggerImportService` implementieren (Dokument mappen via `SwaggerOperationHelper`, Bestand laden, `ImportDiffCalculator.Calculate`, `ApplyDiffAsync` inkl. `WithBearerTokens`). `SystemEndpointSyncService.SyncEndpointsAsync` reduziert sich auf Dokumentbeschaffung + Aufruf; die bisherige Hand-Diff-Bildung (nur `New`/`Removed`) und der `IEndpointRepository`-Bezug entfallen.
- **#88:** `ApplicationImportController` um `swagger-import/apply` ergänzen (Validierungskette analog OData-Pfad); `IApplicationApiClient`/`ApplicationApiClient` um `ApplySwaggerDiffAsync` ergänzen; `ApplyDiffAsync` validiert InterfaceType-/Aggregat-Zugehörigkeit des Diffs; `SwaggerImportDialog` ruft den ApiClient.
- **#89:** `WorkspacesLayout` vollständig auf `IApplicationApiClient` umstellen; `IEndpointRepository`- und `ISignalRNotificationService`-Injections sowie die `StorageModeService.CurrentMode == StorageMode.Team`-Prüfungen entfernen — Notify- und Kontext-Nebenbedingungen liegen einheitlich im Controller-Pfad.
- **#90:** `IApplicationRepository` um eine transaktionale Gruppen-Löschoperation erweitern (beide Varianten in einer `AppDbContext`-Transaktion: `delete` = Apps kaskadierend löschen, `detach` = `ApplicationGroupId = null` setzen, dann Gruppe löschen); `ApplicationGroupsController.DeleteAsync` wertet `content` aus; `WorkspacesLayout` ruft nur noch den einen Endpunkt.
- **#91:** Notify-Bedingung an einer zentralen Stelle definieren (z. B. Helper-Methode oder in `ISignalRNotificationService`-Aufrufstellen vereinheitlicht) und in allen mutierenden OData-Controllern anwenden; `EndpointPage.OnSaveSuccessAsync` feuert nicht mehr zusätzlich.
- **#92:** `IEndpointRepository.UpdateAggregateAsync` als gemeinsame Operation für `EndpointsController.UpdateAsync`, `ODataEndpointsController.Put`/`Patch` und Import-Pfade; Alternative: OData pflegt dokumentiert keine Kinder — dann muss `Put`/`Patch` mitgelieferte `Headers`/`QueryParameters` nicht mehr still ignorieren (z. B. `400` oder klar dokumentierte Ignorier-Semantik). `EndpointPage`-Kinder-Pflege anschließend auf Redundanz prüfen.
- Erweiterungspunkte: keine neuen Events/Hook-Mechanismen; Wiederverwendung bestehender Bausteine (`ApiControllerBase.ParseRequestContextAsync`, `ValidateTokenAndSetResponseHeaderAsync`, `RequiresContextHeadersAttribute`, `ImportDiff`, `EndpointKeyHelper`).
- Lokalisierung: neue UI-Texte (falls z. B. neue Fehlermeldungen) über `SharedResources`-Schlüsselschema `{KomponentenName}_{Rolle}` mit `Comment`-Eintrag.
- Ergebnis: **ein gemeinsamer Pull Request gegen `staging`**, der alle sechs Issues abdeckt; Commit-Messages enthalten Closing-Keywords (`Closes #N`).

## Konfiguration

Keine neue Konfiguration erforderlich. Das Verhalten ist fest verdrahtet: Notify-Bedingung `StorageMode == Team` (bestehende Konvention), `content`-Parameter mit den beiden Werten `delete`/`detach`.

## Offene Fragen

1. **#92 – Variantenwahl:** Soll `ODataEndpointsController.Put`/`Patch` Kinder (`Headers`/`QueryParameters`) über denselben `UpdateAggregateAsync`-Pfad pflegen, oder wird die Skalare-Semantik als dokumentierte Absicht festgelegt? Das Issue lässt beide Optionen zu („oder dokumentierte Absicht"); die Entscheidung bestimmt, ob OData-`PUT` mit Kindern `400` liefert, sie ignoriert (dokumentiert) oder anwendet.
2. **#91 – Notify-Umfang OData:** Gilt „mutierende OData-Operationen" nur für `ODataEndpointsController` (Post/Put/Patch/Delete) oder auch für `ODataApplicationsController`, `ODataApplicationGroupsController` und `ODataEndpointGroupsController`? Für letztere ist zusätzlich zu klären, welche Notify-Methode (`NotifyTreeChangedAsync` vs. entitätsspezifisch) greift.
3. **`content`-Parameter-Semantik:** Verhalten bei fehlendem/ungültigem Wert — Abwärtskompatibilität des bisherigen `DELETE` ohne Parameter (leere Gruppe löschen) oder Pflichtparameter? Vorschlag: ohne `content` nur leere Gruppen löschen bzw. `400` bei nicht-leerer Gruppe.
4. **`ApplyDiffAsync`-Validierung (#88):** Konkreter Umfang der „InterfaceType-/Aggregat-Validierung" im Service — validiert `ApplyDiffAsync(ImportDiff)` zusätzlich, dass alle Diff-Endpunkte zur selben `ApplicationId` gehören? Das `ImportDiff`-Modell trägt die Anwendungs-ID nur implizit über die Endpunkte.
5. **`UpdateEndpointRequest`-Vertrag (#92):** Bleibt die REST-PUT-Semantik „Vollmenge der Kinder ersetzen" unverändert, oder wird der Contract um Flags ergänzt (Kinder nur ersetzen, wenn gesendet)? Betrifft auch die Bewertung der redundanten Kinder-Pflege in `EndpointPage`.
6. **ActivityLog in `WorkspacesLayout` (#89):** Die `IActivityLogService.Log`-Aufrufe für UI-Aktionen (Ordner erstellt/umbenannt/gelöscht, Endpunkt erstellt/gelöscht) sind nicht Teil des Controller-Pfads — verbleiben sie in der Komponente (Annahme: ja)?
7. **Reihenfolge/Abhängigkeiten im PR:** #89 und #90 berühren beide `WorkspacesLayout`; #91 und #92 berühren beide `EndpointPage`/`EndpointsController`/`ODataEndpointsController`. Für den gemeinsamen PR ist eine sinnvolle Umsetzungsreihenfolge festzulegen (Vorschlag: #87 → #88 → #90 → #89 → #92 → #91).
