# Anforderung: OData-API (Issue #48)

## Fachliche Zusammenfassung

Die Schnittstellenzentrale wird um einen zweiten API-Endpunkt unter dem Präfix `/odatav4` erweitert, der einen vollständigen OData v4-Service bereitstellt. Der Service exponiert alle vier Kernobjekte (`Application`, `ApplicationGroup`, `Endpoint`, `EndpointGroup`) als Entity-Sets mit CRUD-Zugriff und veröffentlicht ein CSDL-Dokument unter `/odatav4/$metadata`. Die neue API teilt dieselbe Authentifizierungsinfrastruktur (Bearer-Token / Negotiate) wie die bestehende REST-API unter `/api`, wird aber nicht als Systemanwendung selbst-registriert. Der primäre Zweck ist die Bereitstellung eines realen OData-Ziels für die bestehende `IODataImportService`-Logik sowie für Playwright-Integrationstests, die OData-Erkennung und -Import durchlaufen.

---

## Betroffene Klassen und Komponenten

### Neu zu erstellende Artefakte

**OData-Controller (Hauptprojekt `Schnittstellenzentrale`)**

- `ODataApplicationsController` — OData-Controller für den Entity-Set `Applications`; exponiert GET (Collection + by Key), POST, PUT, PATCH, DELETE; nutzt `IApplicationRepository` direkt (analog zu `ApplicationsController`)
- `ODataApplicationGroupsController` — analoger Controller für den Entity-Set `ApplicationGroups`; nutzt `IApplicationRepository`
- `ODataEndpointsController` — analoger Controller für den Entity-Set `Endpoints`; nutzt `IEndpointRepository`
- `ODataEndpointGroupsController` — analoger Controller für den Entity-Set `EndpointGroups`; nutzt `IEndpointRepository`

**EDM-Modell**

- `ODataEdmModelBuilder` (neue statische Klasse oder Factory-Methode) — baut das `IEdmModel` mit den vier Entity-Types und ihren Navigationseigenschaften; steuert, welche Felder schreibgeschützt sind (`Id`, `RowVersion` werden als nicht schreibbar deklariert)

**OData-Konfiguration (Hauptprojekt)**

- Erweiterung in `Program.cs` (oder einem neuen `ODataServiceExtensions`): Registrierung von `Microsoft.AspNetCore.OData`, Routen-Konfiguration unter `/odatav4`, Einbindung des EDM-Modells; dieselbe Authentifizierungsmiddleware wie für `/api`

**Tests — Unit (Projekt `Schnittstellenzentrale.Tests`)**

- `ODataImportServiceRealMetadataTests` — neuer Unit-Test; parst ein reales `$metadata`-Dokument (erzeugt durch `ODataEdmModelBuilder`) mit gemocktem `HttpMessageHandler`; prüft, dass `ODataImportService.ImportAsync` aus den vier Entity-Sets korrekte GET- und POST-Endpunkte ableitet
- `ODataApplicationsServiceTests`, `ODataApplicationGroupsServiceTests`, `ODataEndpointsServiceTests`, `ODataEndpointGroupsServiceTests` (alternativ zusammengefasst in `ODataCrudServiceTests`) — Unit-Tests für die CRUD-Logik der Service-Schicht hinter den OData-Controllern ohne HTTP-Stack; testen Anlegen, Lesen, Ändern, Löschen je Entity-Typ
- `ODataControllerIntegrationTests` — Integrationstest mit `ControllerTestFactory` (`WebApplicationFactory`); prüft GET `/odatav4/$metadata`, GET `/odatav4/Applications`, sowie POST/PUT/PATCH/DELETE auf mindestens einem Entity-Set; prüft 401-Verhalten ohne Token; prüft `$filter`/`$expand`/`$select` auf `Applications`

**Tests — Playwright (Projekt `Schnittstellenzentrale.Tests`)**

- `ODataImportTests` — neuer Playwright-Test in `[Collection("Playwright")]`; legt eine `Application` über die UI an (InterfaceUrl = `{BaseUrl}/odatav4/$metadata`); verifiziert automatische `InterfaceType`-Erkennung als `OData`; löst OData-Import aus; prüft, dass die vier Entity-Set-Endpunkte im Baum erscheinen; führt exemplarische CRUD-Operationen über die UI aus und verifiziert Persistenz

### Betroffene bestehende Artefakte

- `Program.cs` — Registrierung der OData-Middleware, Route-Konfiguration
- `Schnittstellenzentrale.csproj` — neues NuGet-Paket `Microsoft.AspNetCore.OData` (Version kompatibel mit ASP.NET Core 9)
- `ODataImportService` (`Schnittstellenzentrale.Infrastructure`) — keine funktionale Änderung; wird durch den neuen Integrationstest mit dem realen eigenen `$metadata` getestet
- `PlaywrightServer` — Anpassung: der `ISwaggerImportService`-Mock muss nicht für OData-Tests gelten; `IODataImportService` soll im Playwright-Kontext gegen den echten Kestrel-Server laufen (kein Mock), da die URL `/odatav4/$metadata` in-process erreichbar ist; alternativ separater `PlaywrightODataServer`

---

## Implementierungsansatz

### OData-Framework-Wahl

Das vorhandene NuGet-Paket `Microsoft.OData.Edm` (bereits in `Schnittstellenzentrale.Infrastructure`) genügt für den `ODataImportService`. Für die Controller-Seite wird das Paket `Microsoft.AspNetCore.OData` (OData Web API library von Microsoft) benötigt, das EDM-Modell, Routing, Query-Validierung (`$filter`, `$expand`, `$select`, `$orderby`, `$top`, `$skip`) und `$metadata`-Serialisierung mitbringt.

### EDM-Modell und schreibgeschützte Felder

`ODataEdmModelBuilder` deklariert `Id` und `RowVersion` als serverseitig generiert / nicht schreibbar. Bei POST wird die `Id` ignoriert und serverseitig vergeben; bei PUT/PATCH wird `RowVersion` aus dem gespeicherten Datensatz übernommen (OData-Standard: Concurrency via ETag oder ignoriertes Feld). Da die REST-API bisher ein explizites `rowVersion`-Feld im Body für Optimistic Concurrency verwendet, muss für die OData-API entschieden werden, ob Concurrency über OData-ETag-Mechanismus oder per ignoriertem Feld gelöst wird (siehe Offene Fragen).

### Authentifizierung

Die bestehende `ApiControllerBase`-Authentifizierungslogik (Bearer-Token via `ITokenStore.ValidateAndRotateAsync`) wird für OData-Controller übernommen. Da OData-Controller typischerweise von `ODataController` (statt `ApiControllerBase`) erben, muss der Token-Validierungsansatz entweder in einer neuen `ODataControllerBase` reimplementiert oder via Middleware/Filter abstrahiert werden. Alternativ: OData-Controller erben von `ApiControllerBase`, falls die Vererbungskette es erlaubt.

### Routing und Middleware-Reihenfolge

`app.MapControllers()` bleibt unverändert. Zusätzlich wird `app.MapODataRoute("odatav4", "odatav4", model)` (oder die neuere `app.UseODataRouteDebug()` + `endpoints.MapODataRoute(...)`) konfiguriert. Der Präfix `/odatav4` darf nicht mit bestehenden Routen kollidieren.

### Navigationseigenschaften und `$expand`

Das EDM-Modell muss Navigationseigenschaften deklarieren (`Application.Endpoints`, `Application.EndpointGroups`, `Application.ApplicationGroup`), damit `$expand` funktioniert. Die Controller-Methoden müssen entsprechende `Include()`-Aufrufe in EF Core ausführen, wenn `$expand` angefordert wird.

### Selbst-Registrierung

`SystemEndpointSyncService` darf die neue OData-API nicht registrieren. Da die Anforderung explizit keine Selbstregistrierung fordert, ist sicherzustellen, dass kein neuer `IHostedService`-Code die `/odatav4`-Route als Systemanwendung einträgt.

### Playwright-Tests: ODataImportService nicht mocken

Im `PlaywrightServer` ist `ISwaggerImportService` gemockt, `IODataImportService` aber nicht. Da die `/odatav4/$metadata`-URL im In-Process-Kestrel-Server erreichbar ist, kann `ODataImportService` den HTTP-Client über den bereits konfigurierten `inProcessHandler` nutzen — kein separater Mock erforderlich. Der Playwright-Test legt die URL `http://127.0.0.1:5099/odatav4/$metadata` als `InterfaceUrl` der Testanwendung ein.

---

## Konfiguration

Keine neue Konfigurationsebene erforderlich. Die OData-Route `/odatav4` ist fest kodiert (wie `/api`). Kein Feature-Flag.

---

## Offene Fragen

1. **Concurrency bei PUT/PATCH:** Soll die OData-API `RowVersion` als OData-ETag-Wert (`If-Match`-Header) behandeln oder wie die REST-API als Pflichtfeld im Body? Der Anforderungstext sagt „`RowVersion` wird bei POST/PUT/PATCH serverseitig ignoriert bzw. ist nicht schreibbar" — das bedeutet, Optimistic Concurrency wird für die OData-API nicht unterstützt. Soll ein PUT ohne `RowVersion`-Prüfung blindlings überschreiben?

2. **`X-Storage-Mode`-Semantik in OData:** Die REST-API nutzt `X-Storage-Mode` und `X-Owner` für Benutzermodus-Filterung. Gelten diese Custom-Header auch für die OData-API, oder liefert die OData-API immer alle Datensätze (StorageMode.Team)?

3. **PATCH-Semantik:** OData-PATCH bedeutet partielles Update (nur geänderte Felder). Die aktuelle REST-API verwendet PUT (vollständiges Ersetzen). Sollen PATCH-Handler im OData-Controller implementiert werden, oder ist PUT ausreichend?

4. **NuGet-Version:** Welche Version von `Microsoft.AspNetCore.OData` ist zu verwenden (aktuell: 8.x für .NET 9)?

5. **`$metadata`-Endpunkt im Playwright-Test:** Kann der Playwright-Test die URL `{BaseUrl}/odatav4/$metadata` direkt verwenden, oder muss ein relativer Pfad aus der Konfiguration bezogen werden?

6. **EndpointGroups und ParentGroupId:** Das `EndpointGroup`-Modell hat eine `ParentGroupId` (Self-Referenz). Soll diese Navigationseigenschaft im OData-EDM-Modell deklariert werden?

---

## Akzeptanzkriterien (übertragen)

- [ ] `GET /odatav4/$metadata` liefert valides CSDL-XML mit allen vier Entity-Sets
- [ ] `GET /odatav4/Applications` liefert OData-Collection mit vorhandenen Anwendungen
- [ ] POST/PUT/PATCH/DELETE auf alle vier Entity-Sets funktionieren und persistieren Datenänderungen
- [ ] `$filter`, `$expand`, `$select` funktionieren auf mindestens `Applications`
- [ ] `$expand=Endpoints,EndpointGroups` auf `Applications` liefert verknüpfte Objekte
- [ ] Zugriff ohne gültige Authentifizierung wird mit 401 abgewiesen
- [ ] `Id` und `RowVersion` sind bei POST/PUT/PATCH nicht schreibbar (serverseitig ignoriert)
- [ ] `ODataImportService` erkennt Entity-Sets aus dem eigenen `$metadata`-Dokument korrekt (Unit-Test grün)
- [ ] Playwright-Test legt Anwendung mit `/odatav4/$metadata` an, erkennt `InterfaceType.OData` automatisch, importiert Endpunkte erfolgreich
- [ ] Playwright-Test verifiziert CRUD-Operationen über importierte OData-Endpunkte
