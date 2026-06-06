# Umsetzungsplan: OData-API (Issue #48)

## Übersicht

Die Schnittstellenzentrale wird um einen OData v4-Service unter dem Präfix `/odatav4` erweitert, der alle vier Kernobjekte (`Application`, `ApplicationGroup`, `Endpoint`, `EndpointGroup`) als Entity-Sets mit CRUD-Zugriff und CSDL-Metadaten-Dokument bereitstellt. Betroffen sind das Hauptprojekt (`Schnittstellenzentrale`) für Controller, EDM-Modell und Middleware-Registrierung sowie das Testprojekt für Unit-, Integrations- und Playwright-Tests.

---

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|---|---|---|
| `ODataControllerBase` — Authentifizierung | Eigene abstrakte Basisklasse `ODataControllerBase`, die von `ODataController` (aus `Microsoft.AspNetCore.OData`) erbt und `ValidateTokenAndSetResponseHeaderAsync` sowie `ParseRequestContextAsync` analog zu `ApiControllerBase` neu implementiert; `ITokenStore` wird per Konstruktor injiziert | OData-Controller müssen von `ODataController` erben, um OData-Routing und Query-Optionen nutzen zu können. Eine Mehrfachvererbung von `ApiControllerBase` ist in C# nicht möglich. Die Logik ist schlank genug, um sie in einer neuen Basisklasse zu duplizieren, ohne eine weitere Abstraktionsschicht einzuziehen. |
| `ODataEdmModelBuilder` — EDM-Aufbau | Statische Klasse mit einer einzigen Factory-Methode `Build()`, die ein `IEdmModel` zurückgibt | Ein Transaction Script / Gateway reicht hier aus. Das EDM-Modell wird einmalig beim Start gebaut und ist zustandslos; eine Klasse mit statischer Methode entspricht dem Muster von `AddSwaggerGen`-Konfigurationen im Projekt. |
| `X-Storage-Mode`-Semantik in OData | Die OData-API wertet `X-Storage-Mode` und `X-Owner` **nicht** aus; sie liefert immer alle Datensätze (äquivalent zu `StorageMode.Team`) | Der primäre Zweck der OData-API ist maschineller Zugriff für `IODataImportService` und Tests. Eine nutzermodusabhängige Filterung würde den Endpunkt für Integrationstests und generische OData-Clients unnötig komplex machen. Die Anforderung schweigt zu dieser Anforderung explizit. |
| Concurrency bei PUT/PATCH | `RowVersion` wird bei PUT/PATCH serverseitig ignoriert (aus dem gespeicherten Datensatz übernommen); kein OData-ETag-Mechanismus | Die Anforderung legt fest, dass `RowVersion` nicht schreibbar ist. Ein blindes Überschreiben entspricht dem einfachsten konformen Verhalten und vermeidet Abhängigkeit vom `If-Match`-Header in Integrationstests. |
| PATCH-Unterstützung | PATCH-Methode wird implementiert (partielles Update via `Delta<T>`) | OData v4 schreibt PATCH für partielle Updates vor; die `Microsoft.AspNetCore.OData`-Bibliothek liefert `Delta<T>` out of the box. Ohne PATCH ist der Service nicht OData-konform. |
| `EndpointGroup.ParentGroupId` im EDM | Self-Referenz-Navigationseigenschaft `ParentGroup` / `ChildGroups` wird im EDM-Modell deklariert | Die Eigenschaft ist im Domänenmodell vorhanden. Ohne Deklaration würde `$expand=ChildGroups` nicht funktionieren. Da `ODataImportService` die Self-Referenz nicht benötigt, ist sie optional für die Kernanforderung — sie wird dennoch deklariert, damit das Metadaten-Dokument vollständig ist. |
| `IsSystem`-Schutz für `Endpoint` und `EndpointGroup` | 403-Prüfung für alle vier Entity-Typen; bei `Endpoint` und `EndpointGroup` wird `IsSystem` über die zugehörige `Application` ermittelt (`Endpoint.Application.IsSystem` bzw. `EndpointGroup.Application.IsSystem`); dazu wird die `Application`-Navigationseigenschaft beim Laden per `Include()` mitgeladen | `Endpoint` und `EndpointGroup` haben kein eigenes `IsSystem`-Feld. Die Zugehörigkeit zu einer Systemanwendung ist die nächste sinnvolle Abgrenzung: Endpunkte und Gruppen einer Systemanwendung gelten als systemverwaltet und dürfen nicht über die OData-API geändert oder gelöscht werden. |

---

## Programmabläufe

### OData-Service-Registrierung beim Start

1. `Program.cs` ruft `builder.Services.AddControllers().AddOData(options => ...)` auf, um OData-Unterstützung in der Controller-Pipeline zu aktivieren.
2. `ODataEdmModelBuilder.Build()` wird aufgerufen und liefert das fertige `IEdmModel`.
3. Das Modell wird in `options.AddRouteComponents("odatav4", model)` eingetragen.
4. `app.MapControllers()` bleibt unverändert; OData-Routing wird durch die `AddRouteComponents`-Konfiguration aktiviert.

Beteiligte Klassen/Komponenten: `Program`, `ODataEdmModelBuilder`

---

### GET `/odatav4/$metadata`

1. Der OData-Middleware-Stack von `Microsoft.AspNetCore.OData` fängt die Anfrage ab.
2. Das registrierte `IEdmModel` wird als CSDL-XML serialisiert und zurückgegeben.
3. Kein Controller-Code ist beteiligt — der Endpunkt wird automatisch durch die OData-Bibliothek bereitgestellt.

Beteiligte Klassen/Komponenten: `ODataEdmModelBuilder`, OData-Middleware

---

### GET Collection (z. B. `GET /odatav4/Applications`)

1. `ODataApplicationsController.GetAsync()` wird aufgerufen.
2. `ValidateTokenAndSetResponseHeaderAsync()` prüft den Bearer-Token; bei Fehler wird 401 zurückgegeben.
3. `IApplicationRepository.GetApplicationsAsync(StorageMode.Team, string.Empty)` lädt alle Datensätze (kein Modus-Filter).
4. Der OData-Query-Stack (`[EnableQuery]`-Attribut) wendet `$filter`, `$select`, `$expand`, `$orderby`, `$top`, `$skip` auf die zurückgegebene `IQueryable`-Quelle an.
5. Das Ergebnis wird als OData-Collection-Response zurückgegeben.

Beteiligte Klassen/Komponenten: `ODataApplicationsController`, `ODataControllerBase`, `IApplicationRepository`

---

### GET by Key (z. B. `GET /odatav4/Applications(1)`)

1. `ODataApplicationsController.GetAsync(int key)` wird aufgerufen.
2. Token-Validierung wie oben.
3. `IApplicationRepository.GetApplicationByIdAsync(key)` lädt den Datensatz; bei `null` wird 404 zurückgegeben.
4. Das Objekt wird direkt zurückgegeben (OData serialisiert es).

Beteiligte Klassen/Komponenten: `ODataApplicationsController`, `ODataControllerBase`, `IApplicationRepository`

---

### POST (z. B. `POST /odatav4/Applications`)

1. `ODataApplicationsController.PostAsync(Application entity)` wird aufgerufen.
2. Token-Validierung.
3. `entity.Id` wird zurückgesetzt (serverseitig vergeben); `entity.RowVersion` wird ignoriert.
4. `Application.DetectInterfaceType(entity.InterfaceUrl)` setzt `InterfaceType` automatisch.
5. `IApplicationRepository.AddApplicationAsync(entity)` speichert den Datensatz.
6. Rückgabe: 201 Created mit Location-Header auf `GET /odatav4/Applications({newId})`.

Beteiligte Klassen/Komponenten: `ODataApplicationsController`, `ODataControllerBase`, `IApplicationRepository`

---

### PUT (z. B. `PUT /odatav4/Applications(1)`)

1. `ODataApplicationsController.PutAsync(int key, Application entity)` wird aufgerufen.
2. Token-Validierung.
3. Bestehender Datensatz wird per `IApplicationRepository.GetApplicationByIdAsync(key)` geladen; bei `null` → 404.
4. Bei Systemeinträgen (`existing.IsSystem == true`) → 403.
5. Felder des geladenen Datensatzes werden aus `entity` überschrieben; `RowVersion` wird vom gespeicherten Datensatz beibehalten.
6. `IApplicationRepository.UpdateApplicationAsync(existing)` speichert die Änderungen.
7. Rückgabe: 200 OK mit dem aktualisierten Objekt.

Beteiligte Klassen/Komponenten: `ODataApplicationsController`, `ODataControllerBase`, `IApplicationRepository`

---

### PATCH (z. B. `PATCH /odatav4/Applications(1)`)

1. `ODataApplicationsController.PatchAsync(int key, Delta<Application> delta)` wird aufgerufen.
2. Token-Validierung.
3. Bestehender Datensatz wird geladen; bei `null` → 404.
4. Bei Systemeinträgen → 403.
5. `delta.Patch(existing)` übernimmt nur die geänderten Felder; `Id` und `RowVersion` werden aus dem Patch-Delta herausgenommen, bevor `Patch` aufgerufen wird.
6. `IApplicationRepository.UpdateApplicationAsync(existing)` speichert die Änderungen.
7. Rückgabe: 200 OK.

Beteiligte Klassen/Komponenten: `ODataApplicationsController`, `ODataControllerBase`, `IApplicationRepository`

---

### DELETE (z. B. `DELETE /odatav4/Applications(1)`)

1. `ODataApplicationsController.DeleteAsync(int key)` wird aufgerufen.
2. Token-Validierung.
3. Bestehender Datensatz wird geladen; bei `null` → 404.
4. Bei Systemeinträgen → 403.
5. `IApplicationRepository.DeleteApplicationAsync(key)` löscht den Datensatz.
6. Rückgabe: 204 No Content.

Beteiligte Klassen/Komponenten: `ODataApplicationsController`, `ODataControllerBase`, `IApplicationRepository`

---

### PUT/PATCH/DELETE auf `Endpoint` oder `EndpointGroup` (IsSystem-Prüfung via Application)

1. Controller-Methode (z. B. `ODataEndpointsController.PutAsync(int key, ...)`) wird aufgerufen.
2. Token-Validierung.
3. Bestehender Datensatz wird per `IEndpointRepository.GetEndpointByIdAsync(key)` geladen; dabei wird die `Application`-Navigationseigenschaft per `Include()` mitgeladen; bei `null` → 404.
4. `existing.Application.IsSystem == true` → 403.
5. Felder übernehmen, `RowVersion` aus DB beibehalten, persistieren.
6. Rückgabe: 200 OK (PUT/PATCH) oder 204 No Content (DELETE).

Beteiligte Klassen/Komponenten: `ODataEndpointsController`, `ODataEndpointGroupsController`, `ODataControllerBase`, `IEndpointRepository`

---

### OData-Import-Test (Playwright-Ablauf)

1. `ODataImportTests` navigiert zur App-Startseite.
2. Eine neue Testanwendung wird über die UI angelegt; `InterfaceUrl` wird auf `{BaseUrl}/odatav4/$metadata` gesetzt, wobei `BaseUrl` direkt aus `PlaywrightTestBase` bezogen wird — kein separater Konfigurationseintrag erforderlich.
3. Die UI-Komponente zur `InterfaceType`-Erkennung zeigt `OData` an (automatische Erkennung via `Application.DetectInterfaceType`).
4. Der OData-Import-Auslöse-Button in der UI wird geklickt.
5. `IODataImportService.ImportAsync` ruft intern `/odatav4/$metadata` über den `inProcessHandler` ab (kein externer HTTP-Aufruf nötig, da In-Process).
6. Die vier Entity-Set-Endpunkte (GET und POST je Set) erscheinen im Endpunkt-Baum.
7. Mindestens eine CRUD-Operation (z. B. POST auf `Applications`) wird über die UI ausgeführt und die Persistenz verifiziert.

Beteiligte Klassen/Komponenten: `ODataImportTests`, `PlaywrightServer`, `IODataImportService`, `ODataApplicationsController`

---

## Neue Klassen

| Klasse | Typ | Zweck |
|---|---|---|
| `ODataControllerBase` | Abstrakte Klasse | Basisklasse für alle OData-Controller; reimplementiert Token-Validierung aus `ApiControllerBase` auf Basis von `ODataController` |
| `ODataEdmModelBuilder` | Statische Klasse | Baut das `IEdmModel` mit den vier Entity-Types und Navigationseigenschaften |
| `ODataApplicationsController` | Controller (erbt `ODataControllerBase`) | OData-CRUD für Entity-Set `Applications` |
| `ODataApplicationGroupsController` | Controller (erbt `ODataControllerBase`) | OData-CRUD für Entity-Set `ApplicationGroups` |
| `ODataEndpointsController` | Controller (erbt `ODataControllerBase`) | OData-CRUD für Entity-Set `Endpoints` |
| `ODataEndpointGroupsController` | Controller (erbt `ODataControllerBase`) | OData-CRUD für Entity-Set `EndpointGroups` |
| `ODataImportServiceRealMetadataTests` | Testklasse (xUnit) | Unit-Test: parst reales `$metadata`-Dokument von `ODataEdmModelBuilder` mit gemocktem `HttpMessageHandler` |
| `ODataControllerIntegrationTests` | Testklasse (xUnit) | Integrationstest mit `ControllerTestFactory` für alle vier OData-Controller |
| `ODataImportTests` | Testklasse (Playwright) | E2E-Test: OData-Anwendung anlegen, Import auslösen, Endpunkte und CRUD prüfen |

---

## Änderungen an bestehenden Klassen

### `Program` (`Program.cs`)

- **Geänderte Methoden:** `BuildWebApplicationAsync` — `builder.Services.AddControllers()` wird zu `builder.Services.AddControllers().AddOData(options => options.AddRouteComponents("odatav4", ODataEdmModelBuilder.Build()).Select().Filter().Expand().OrderBy().Count().SetMaxTop(null))` erweitert; kein `MapODataRoute`-Aufruf nötig (wird durch `AddRouteComponents` ersetzt).

### `Schnittstellenzentrale.csproj`

- **Neue Paketabhängigkeit:** `Microsoft.AspNetCore.OData` Version **8.2.x** wird als `<PackageReference>` hinzugefügt. Die konkrete Patch-Version wird vor Implementierungsbeginn mit `dotnet add package Microsoft.AspNetCore.OData` ermittelt und festgenagelt.

---

## Datenbankmigrationen

Keine.

---

## Validierungsregeln

| Feld / Objekt | Regel | Fehlerfall |
|---|---|---|
| `Application.Id` (POST) | Wird serverseitig auf `0` zurückgesetzt, Eingabewert wird ignoriert | Kein Fehler — stille Ignorierung |
| `Application.RowVersion` (POST/PUT/PATCH) | Wird serverseitig ignoriert; bei PUT/PATCH aus DB-Datensatz übernommen | Kein Fehler — stille Ignorierung |
| `ApplicationGroup.IsSystem` (PUT/PATCH/DELETE) | Systemgruppen dürfen nicht geändert oder gelöscht werden | 403 Forbidden |
| `Application.IsSystem` (PUT/PATCH/DELETE) | Systemanwendungen dürfen nicht geändert oder gelöscht werden | 403 Forbidden |
| `Endpoint.Application.IsSystem` (POST/PUT/PATCH/DELETE) | Endpunkte einer Systemanwendung dürfen nicht angelegt, geändert oder gelöscht werden; Ermittlung über `Endpoint.Application` (per `Include()` beim Laden) | 403 Forbidden |
| `EndpointGroup.Application.IsSystem` (POST/PUT/PATCH/DELETE) | Endpunktgruppen einer Systemanwendung dürfen nicht angelegt, geändert oder gelöscht werden; Ermittlung über `EndpointGroup.Application` (per `Include()` beim Laden) | 403 Forbidden |
| Bearer-Token (alle Endpunkte) | Muss im `Authorization`-Header als `Bearer <token>` vorhanden und gültig sein | 401 Unauthorized |

---

## Konfigurationsänderungen

Keine.

---

## Seiteneffekte und Risiken

- **`AddOData()`-Erweiterung auf `AddControllers()`:** Das Hinzufügen von OData zum Controller-Stack kann das Routing bestehender REST-Controller beeinflussen, falls OData-Middleware Anfragen an `/api/...` fälschlicherweise abfängt. Das Risiko ist gering, da der OData-Präfix `/odatav4` klar abgetrennt ist — ist aber durch Regressionstests der bestehenden Controller zu verifizieren.
- **`ControllerTestFactory` und OData:** Die `WebApplicationFactory` für Integrationstests muss `AddOData` ebenfalls ausführen (da sie `Program` startet). Ohne Anpassung der Factory ist das automatisch der Fall. Es ist zu prüfen, ob der `TestAuthHandler` mit OData-Routing kompatibel ist.
- **`PlaywrightServer`:** `IODataImportService` läuft bereits unggemockt; sobald der OData-Controller registriert ist, kann er `/odatav4/$metadata` in-process aufrufen. Es ist sicherzustellen, dass die `PlaywrightApiFactory` (die `Program` startet) ebenfalls `AddOData` aktiviert.
- **Bestehende Tests:** `ApplicationsControllerIntegrationTests` und verwandte Tests dürfen nicht brechen. Da nur additive Änderungen an `Program.cs` vorgenommen werden (kein Entfernen, kein Umbenennen), ist das Risiko gering.
- **`IsSystem`-Prüfung für `Endpoint`/`EndpointGroup` erfordert Navigation-Load:** Die `IEndpointRepository`-Methoden `GetEndpointByIdAsync` und `GetEndpointGroupByIdAsync` müssen die `Application`-Navigationseigenschaft per `Include()` mitladen. Falls diese Methoden das bisher nicht tun, muss geprüft werden, ob das `Include()` direkt in der Repository-Implementierung ergänzt wird oder ob im Controller eine separate `GetApplicationByIdAsync`-Abfrage nachgezogen wird. Bevorzugt wird das Ergänzen des `Include()` in der Repository-Implementierung, um die Logik zentral zu halten.

---

## Umsetzungsreihenfolge

1. NuGet-Paket `Microsoft.AspNetCore.OData` Version 8.2.x zu `Schnittstellenzentrale.csproj` hinzufügen (konkrete Patch-Version via `dotnet add package Microsoft.AspNetCore.OData` bestimmen).
2. `ODataEdmModelBuilder` (statische Klasse) mit `Build()`-Methode anlegen — deklariert alle vier Entity-Types, Navigationseigenschaften und schreibgeschützte Felder (`Id`, `RowVersion`).
3. `ODataControllerBase` (abstrakte Klasse, erbt `ODataController`) anlegen — Token-Validierungs-Logik aus `ApiControllerBase` übernehmen.
4. Repository-Methoden `GetEndpointByIdAsync` und `GetEndpointGroupByIdAsync` prüfen: falls `Application`-Navigation dort nicht per `Include()` geladen wird, ergänzen.
5. `ODataApplicationsController` implementieren (GET Collection, GET by Key, POST, PUT, PATCH, DELETE; IsSystem-403 für PUT/PATCH/DELETE).
6. `ODataApplicationGroupsController` implementieren (GET Collection, GET by Key, POST, PUT, PATCH, DELETE; IsSystem-403 für PUT/PATCH/DELETE).
7. `ODataEndpointsController` implementieren (GET Collection, GET by Key, POST, PUT, PATCH, DELETE; IsSystem-403 via `Application.IsSystem` für alle Schreibzugriffe).
8. `ODataEndpointGroupsController` implementieren (GET Collection, GET by Key, POST, PUT, PATCH, DELETE; IsSystem-403 via `Application.IsSystem` für alle Schreibzugriffe).
9. `Program.cs` anpassen: `AddOData` + `AddRouteComponents("odatav4", ...)` eintragen.
10. Manueller Smoke-Test: `GET /odatav4/$metadata` und `GET /odatav4/Applications` prüfen.
11. `ODataImportServiceRealMetadataTests` schreiben: reales CSDL-Dokument (via `ODataEdmModelBuilder.Build()`) in `HttpMessageHandler` einbauen, `ODataImportService.ImportAsync` aufrufen, Diff prüfen.
12. `ODataControllerIntegrationTests` schreiben: `$metadata`, Collection-GET, POST/PUT/PATCH/DELETE auf mindestens einem Entity-Set, 401-Verhalten, `$filter`/`$expand`/`$select`, 403-Verhalten für Systemeinträge auf allen vier Entity-Sets.
13. `ODataImportTests` (Playwright) schreiben: Anwendung anlegen, OData-Erkennung, Import auslösen, Endpunkte im Baum prüfen, CRUD-Operation.

---

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|---|---|---|
| `GetMetadata_ReturnsValidCsdl` | `ODataControllerIntegrationTests` | `GET /odatav4/$metadata` liefert HTTP 200 mit CSDL-XML; enthält alle vier Entity-Sets |
| `GetApplications_WithValidToken_Returns200` | `ODataControllerIntegrationTests` | `GET /odatav4/Applications` liefert OData-Collection mit gültigem Token |
| `GetApplications_WithoutToken_Returns401` | `ODataControllerIntegrationTests` | `GET /odatav4/Applications` ohne Token → 401 |
| `PostApplication_WithValidToken_Returns201` | `ODataControllerIntegrationTests` | POST legt Anwendung an, persistiert, gibt 201 zurück |
| `PutApplication_WithValidToken_Returns200` | `ODataControllerIntegrationTests` | PUT aktualisiert Anwendung |
| `PatchApplication_WithValidToken_Returns200` | `ODataControllerIntegrationTests` | PATCH aktualisiert nur geänderte Felder |
| `DeleteApplication_WithValidToken_Returns204` | `ODataControllerIntegrationTests` | DELETE entfernt Anwendung |
| `PutApplication_WithSystemApplication_Returns403` | `ODataControllerIntegrationTests` | PUT auf Systemanwendung → 403 |
| `DeleteApplication_WithSystemApplication_Returns403` | `ODataControllerIntegrationTests` | DELETE auf Systemanwendung → 403 |
| `PutEndpoint_WithSystemApplication_Returns403` | `ODataControllerIntegrationTests` | PUT auf Endpunkt einer Systemanwendung → 403 |
| `DeleteEndpoint_WithSystemApplication_Returns403` | `ODataControllerIntegrationTests` | DELETE auf Endpunkt einer Systemanwendung → 403 |
| `PostEndpoint_WithSystemApplication_Returns403` | `ODataControllerIntegrationTests` | POST eines Endpunkts auf eine Systemanwendung → 403 |
| `PutEndpointGroup_WithSystemApplication_Returns403` | `ODataControllerIntegrationTests` | PUT auf Endpunktgruppe einer Systemanwendung → 403 |
| `DeleteEndpointGroup_WithSystemApplication_Returns403` | `ODataControllerIntegrationTests` | DELETE auf Endpunktgruppe einer Systemanwendung → 403 |
| `GetApplications_WithFilter_ReturnsFilteredResult` | `ODataControllerIntegrationTests` | `$filter=Name eq 'X'` liefert gefilterte Liste |
| `GetApplications_WithExpand_ReturnsRelatedEntities` | `ODataControllerIntegrationTests` | `$expand=Endpoints,EndpointGroups` liefert verknüpfte Objekte |
| `GetApplications_WithSelect_ReturnsSelectedFields` | `ODataControllerIntegrationTests` | `$select=Id,Name` liefert nur die gewählten Felder |
| `Import_RealMetadata_ReturnsCorrectEntitySetEndpoints` | `ODataImportServiceRealMetadataTests` | `ODataImportService.ImportAsync` mit realem CSDL von `ODataEdmModelBuilder.Build()`: alle vier Entity-Sets führen zu GET+POST-Endpunkten im Diff |
| `ImportOData_RecognizesODataType_AndImportsEndpoints` | `ODataImportTests` (Playwright) | Anwendung anlegen mit `$metadata`-URL, automatische `InterfaceType.OData`-Erkennung, Import, vier Entity-Sets im Baum sichtbar |
| `ImportOData_CrudOperation_PersistsChange` | `ODataImportTests` (Playwright) | CRUD-Operation über importierten OData-Endpunkt, Persistenz verifiziert |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|---|---|
| `ApplicationsControllerIntegrationTests` (und alle weiteren `*ControllerIntegrationTests`) | Die `AddOData`-Erweiterung auf `AddControllers()` kann das Routing der bestehenden `/api`-Controller beeinflussen. Kein inhaltlicher Änderungsbedarf erwartet, aber Testlauf nach OData-Integration zur Verifikation der Regressionssicherheit erforderlich. |

### E2E-Tests (Pflicht)

| Szenario | Testdatei / Testklasse | Abgedecktes Akzeptanzkriterium |
|---|---|---|
| Anwendung mit `$metadata`-URL anlegen, `InterfaceType.OData` wird automatisch erkannt | `ODataImportTests` | Playwright-Test legt Anwendung mit `/odatav4/$metadata` an, erkennt `InterfaceType.OData` automatisch |
| OData-Import auslösen, vier Entity-Set-Endpunkte erscheinen im Baum | `ODataImportTests` | `ODataImportService` erkennt Entity-Sets aus eigenem `$metadata`, importiert Endpunkte erfolgreich |
| CRUD-Operation über UI auf importiertem OData-Endpunkt | `ODataImportTests` | Playwright-Test verifiziert CRUD-Operationen über importierte OData-Endpunkte |

Welche bestehenden E2E-Tests müssen angepasst werden?

Keine.

---

## Offene Punkte

Keine.
