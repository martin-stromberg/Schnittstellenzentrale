# Umsetzungsplan: Einheitliches Aggregat-Update für Endpunkte

Basisbranch: `task/issue-78-e692faab901a4147ab28ab4a1b560b76-code-review-orchestrierung-meh` · Issue: #92

## Übersicht

`EndpointRepository.UpdateEndpointAsync` ist bereits die eine serverseitige Aggregat-Operation (Skalare + Vollmengen-Ersetzung der Kinder Header/Query-Parameter + Optimistic Concurrency via `RowVersion`). Dieser Plan stellt alle mutierenden Pfade einheitlich darauf um: `EndpointsController.UpdateAsync` verliert seine redundante Einzel-Kind-Pflege, `ODataEndpointsController.Put`/`Patch` pflegen mitgelieferte Kinder über denselben Pfad statt sie still zu ignorieren, und in `EndpointPage.razor` entfällt die parallele Sofort-Persistenz pro Panel-Änderung. Die Import-Apply-Pfade nutzen die Aggregat-Operation bereits unverändert weiter.

## Scope-Klärung

|| Feld | Festlegung |
||------|------------|
|| Verarbeitungseinheit | Ein Endpunkt-Aggregat: `Endpoint`-Datensatz mit Skalarwerten + Vollmengen `Headers` und `QueryParameters` |
|| Auslösende Ereignisse | `PUT /api/endpoints/{id}`; `PUT/PATCH /odatav4/Endpoints({key})`; `ApplyDiffAsync` in `SwaggerImportService`/`ODataImportService`; Speichern auf der Endpunkt-Detailseite |
|| Erwartete Wirkung | Identische Persistenzsemantik auf allen Pfaden: Skalare aktualisiert, mitgelieferte Kind-Mengen ersetzen vollständig die Bestands-Mengen; Versionskonflikt-Verhalten unverändert; Kinder werden nicht mehr still ignoriert |
|| Umgang mit fehlenden Objekten | Ziel-Endpunkt fehlt: REST → 404 (Vorprüfung), OData Put/Patch → 404, Import → nur `ChangedEndpoints` betroffen (entstehen nur bei vorhandenem Bestand). Im Diff-Objekt fehlt `ApplicationId` der `ChangedEndpoints`: `MergeExistingIdentity` liefert sie aus dem Import-Objekt — Validierung in `SwaggerImportService.ApplyDiffAsync` bleibt. OData: `headers`/`queryparameters` nicht im Body → Bestands-Kinder bleiben unverändert (EDM blendet sie aus — sonst verlöre jeder konforme OData-Client die Kinder) |

### Scope-Ausschlüsse

|| # | Einschränkung / Ausschluss | Begründung | Status |
||---|----------------------------|------------|--------|
|| 1 | Keine REST-Vertragsänderung: keine Flags für optionale Kind-Pflege; Vollmengen-Ersetzung bleibt wie bisher Vertragsbestandteil (`UpdateEndpointRequest.Headers`/`QueryParameters` Defaults `[]` → fehlende Kinder im REST-PUT leeren die Mengen weiterhin) | Ausdrücklich in der Anforderung festgelegt | bestätigt (Anforderungstext, 2026-11-17) |
|| 2 | Keine Änderung des Optimistic-Concurrency-Verhaltens über `RowVersion` (OData Put ohne 409-Catch, Patch mit 409-Catch — bleibt; REST-Verhalten bleibt) | Ausdrücklich in der Anforderung festgelegt | bestätigt (Anforderungstext, 2026-11-17) |
|| 3 | Kinder von `ChangedEndpoints` im Import bleiben unverändert: `ImportDiffCalculator.MergeExistingIdentity` übernimmt weiterhin die Bestands-Kinder; importierte `x-sz-header-*`-Annotationen wirken nur bei `NewEndpoints` | Anforderung verlangt nur die einheitliche Aggregat-Operation auf dem Apply-Pfad, keine Änderung der Diff-Semantik | bestätigt (Anforderungstext, 2026-11-17) |
|| 4 | EDM-Modell unverändert: `Headers`/`QueryParameters` bleiben per `Ignore` aus dem `$metadata` ausgeblendet; mitgelieferte Kinder sind ein JSON-Vertrags-Detail auf Put/Patch, kein EDM-Feature | Anforderung verlangt keine Vertragsänderung; Aufblenden würde `$metadata` und Navigationseigenschaften ändern | bestätigt (Anforderungstext, 2026-11-17) |
|| 5 | OData `Post` und `DELETE` unverändert; Einzel-Kind-Endpunkte (`/api/endpoints/headers`, `/api/endpoints/query-parameters`) bleiben bestehen (für UI-Neuanlage nötig, da `CreateEndpointRequest` keine Kinder enthält) | Anforderung betrifft nur Update-Operationen | bestätigt (Anforderungstext, 2026-11-17) |
|| 6 | Entfall der Sofort-Kind-Persistenz in `EndpointPage.razor`: Panel-Änderungen an Headern/Query-Parametern persistieren erst beim Speichern (Dirty-Flag + Vollmengen-PUT bleiben unverändert). Verhalten vorher: sofortige Persistenz zusätzlich zum Vollmengen-Save — redundant | Anforderung erlaubt explizit den Entfall („darf entfallen, solange das Speichern weiterhin die Vollmenge korrekt übermittelt") | bestätigt (Anforderungstext, 2026-11-17) |

## Designentscheidungen

|| Komponente / Bereich | Gewählter Ansatz | Begründung |
||----------------------|-----------------|------------|
|| Aggregat-Operation | Bestehende `IEndpointRepository.UpdateEndpointAsync` bleibt die eine gemeinsame Operation; XML-Doc präzisiert die Aggregat-Semantik | Semantik ist bereits vollständig implementiert (Skalare via `ApplyUpdate`, Vollmengen-Ersetzung der Kinder, `RowVersion` als `OriginalValue`); keine neue Methode nötig — alle Pfade nutzen dieselbe Operation |
|| OData `Put`-Body-Bindung | Signaturwechsel `[FromBody] Endpoint` → `[FromBody] JsonElement`, manuelle Deserialisierung via `JsonSerializer` mit `PropertyNameCaseInsensitive` + `JsonStringEnumConverter`; Kind-Vorhandensein via `EnumerateObject` erkennen | EDM blendet `Headers`/`QueryParameters` aus — der OData-Formatter bindet mitgelieferte Kinder nie; nur über das rohe JSON sind „mitgelieferte" Kinder erkennbar. `JsonElement`-Body hat Präzedenz im selben Controller (`Patch`) |
|| OData-Deserialisierungs-/Präsenz-Helfer | Neuer interner statischer Helfer `ODataEndpointUpdateHelper` in `src/Schnittstellenzentrale/OData/` | Kapselt `TryDeserializeEndpoint` + `ContainsProperty` + `TryReadChildren` — wiederverwendbar für Put und Patch; `ApplyPatch` wird zu `TryApplyPatch` (gleiches Muster wie `ODataPatchHelper`) |
|| REST-Kind-Mapping | `UpdateEndpointKeyValueItem` → `EndpointHeader`/`EndpointQueryParameter` direkt im `Endpoint`-Objekt vor dem einen `UpdateEndpointAsync`-Aufruf | Entspricht dem bestehenden Mapping-Muster; vermeidet alle Einzel-Repository-Aufrufe |

## Programmabläufe

### REST: `PUT /api/endpoints/{id}`

1. `ParseRequestContextAsync` → 401 bei Fehlschlag.
2. `GetEndpointByIdAsync(id)` → 404 wenn `null`.
3. Neues `Endpoint`-Objekt aus `UpdateEndpointRequest`: Skalare + `RowVersion` **plus** `Headers`/`QueryParameters` als `EndpointHeader`/`EndpointQueryParameter`-Liste (Key/Value gemappt).
4. Einmalig `UpdateEndpointAsync(endpoint)` → Aggregat-Update (Skalare + Vollmengen-Ersetzung beider Kind-Mengen + Versionsprüfung in einem `SaveChangesAsync`).
5. `GetEndpointByIdAsync(id)` → Response; Team-Modus → SignalR-Notify; `Ok(MapToResponse(updated))`.

Beteiligte Klassen/Komponenten: `EndpointsController`, `IEndpointRepository`, `Endpoint`, `EndpointHeader`, `EndpointQueryParameter`.

### OData: `PUT /odatav4/Endpoints({key})`

1. `GetEndpointByIdAsync(key)` → 404; `Application == null` → 404; `IsSystem` → 403 (unverändert).
2. `ODataEndpointUpdateHelper.TryDeserializeEndpoint(document, out entity, out error)` → 400 bei ungültigem JSON/Enum.
3. `entity.EndpointGroupId`-Validierung (Gruppe gehört zur selben Anwendung, kein System-Gruppe-Ziel) — unverändert.
4. `entity.RowVersion.Length == 0` → 400 — unverändert.
5. Skalare auf `existing` kopieren (wie bisher), `existing.RowVersion = entity.RowVersion`.
6. `ODataEndpointUpdateHelper.ContainsProperty(document, "headers")` → `existing.Headers = entity.Headers`; analog `"queryparameters"`. Nicht vorhandene Kind-Mengen bleiben die geladenen Bestands-Kinder.
7. `UpdateEndpointAsync(existing)` → `Ok(saved)`.

### OData: `PATCH /odatav4/Endpoints({key})`

1. Bestehender Ablauf (NotFound/IsSystem/`endpointgroupid`-Prüfung/`rowVersion`-Pflicht) unverändert.
2. `TryApplyPatch(patch, existing, out error)` statt `ApplyPatch` → 400 bei `error`. Neue Fälle `headers`/`queryparameters`: müssen JSON-Array sein (sonst `error` = Type-Guard); Elemente `{key, value}` werden als `EndpointHeader`/`EndpointQueryParameter` gemappt und ersetzen die jeweilige Menge auf `existing`.
3. `UpdateEndpointAsync(existing)` mit bestehendem `DbUpdateConcurrencyException` → 409.

Beteiligte Klassen/Komponenten: `ODataEndpointsController`, `ODataEndpointUpdateHelper`, `IEndpointRepository`.

### Import-Apply (unverändert, dokumentiert)

`SwaggerImportService.ApplyDiffAsync` und `ODataImportService.ApplyDiffAsync` rufen für `ChangedEndpoints` bereits `UpdateEndpointAsync` auf — die gemeinsame Aggregat-Operation. Kein Code-Change; Verhalten wird durch Integrationstests abgesichert.

### UI: `EndpointPage.razor`

1. Panel-Änderung (Header/Query-Parameter hinzufügen/entfernen/editieren) → nur noch `MarkDirty` über `OnChanged`; die EventCallbacks `OnHeaderAdded`/`OnHeaderRemoved`/`OnQueryParameterAdded`/`OnQueryParameterRemoved` werden nicht mehr verdrahtet.
2. Speichern (`SaveAsync` → `PersistAsync`): unverändert — Vollmenge via `ApplicationApiClient.UpdateEndpointAsync` (REST-PUT mit `Headers`/`QueryParameters`).
3. Neuanlage (`Id == 0`): `CreateEndpointWithDetailsAsync` bleibt unverändert (`CreateEndpointRequest` enthält keine Kinder → Einzel-POSTs weiterhin nötig).

## Neue Klassen

|| Klasse | Typ | Zweck |
||--------|-----|-------|
|| `ODataEndpointUpdateHelper` | interne statische Hilfsklasse | `TryDeserializeEndpoint(JsonElement, out Endpoint?, out string?)`, `ContainsProperty(JsonElement, string)`, `TryReadChildren` für `headers`/`queryparameters` (Vorhandensein + Mapping zu `EndpointHeader`/`EndpointQueryParameter`) |

## Änderungen an bestehenden Klassen

### `EndpointsController` (Controller)

- **Geänderte Methoden:** `UpdateAsync` — `endpoint.Headers`/`endpoint.QueryParameters` aus `request.Headers`/`request.QueryParameters` mappen; die manuellen `DeleteHeaderAsync`/`DeleteQueryParameterAsync`-/`AddHeaderAsync`/`AddQueryParameterAsync`-Schleifen entfallen.

### `ODataEndpointsController` (OData-Controller)

- **Geänderte Methoden:** `Put` — Parameter `[FromBody] Endpoint entity` → `[FromBody] JsonElement document`; Deserialisierung + Kind-Präsenz-Erkennung via `ODataEndpointUpdateHelper`; Skalar-Kopie auf `existing` unverändert; `existing.Headers`/`existing.QueryParameters` werden nur bei mitgelieferter Kind-Menge ersetzt.
- **Geänderte Methoden:** `Patch` — `ApplyPatch` → `TryApplyPatch` mit `out string? error`; bei Fehler `BadRequest(error)`; Rest unverändert.
- **Geänderte Methoden:** `ApplyPatch` → umbenannt zu `TryApplyPatch` mit `error`-Out-Parameter; neue Fälle `headers` und `queryparameters`.

### `EndpointPage.razor` (Blazor-Komponente)

- **Entfernte Methoden:** `AddHeaderAsync(HeaderEntry)`, `DeleteHeaderAsync(HeaderEntry)`, `AddQueryParameterAsync(QueryParamEntry)`, `DeleteQueryParameterAsync(QueryParamEntry)` (Z.444–474).
- **Entfernte Verdrahtung:** `OnHeaderAdded`/`OnHeaderRemoved` (Z.75–76) und `OnQueryParameterAdded`/`OnQueryParameterRemoved` (Z.81–82) — `OnChanged="MarkDirty"` bleibt.

### `EndpointRepository` (Repository)

- **Geänderte Methoden:** `UpdateEndpointAsync` — nur XML-Doc präzisieren (Aggregat-Semantik: Skalare + Vollmengen-Ersetzung der Kinder + Versionsprüfung). Kein Verhaltens-Change.

### `IEndpointRepository` (Interface)

- **Geänderte Methoden:** `UpdateEndpointAsync` — XML-Doc präzisieren (Aggregat-Semantik). Kein Vertrags-Change.

## Datenbankmigrationen

Keine.

## Validierungsregeln

|| Feld / Objekt | Regel | Fehlerfall |
||---------------|-------|------------|
|| OData Put-Body | muss als `Endpoint` deserialisierbar sein (inkl. String-Enums) | 400 Bad Request |
|| OData Patch `headers`/`queryparameters` | muss JSON-Array von `{key, value}`-Objekten sein | 400 Bad Request (`error`-Meldung aus `TryApplyPatch`) |
|| OData Put `headers`/`queryparameters` | falls vorhanden: JSON-Array von `{key, value}`-Objekten | 400 Bad Request |
|| `UpdateEndpointRequest.Headers`/`QueryParameters` | unverändert (Vollmengen-Listen, Defaults `[]`) | — |

## Konfigurationsänderungen

Keine.

## Seiteneffekte und Risiken

- **OData Put-Verhalten:** Clients, die `headers`/`queryparameters` im Body mitschicken, bewirken nun eine Vollmengen-Ersetzung der jeweiligen Menge — bisher wurden sie ignoriert. Gewünschte Semantikänderung; für EDM-konforme Clients (Kinder nicht sichtbar) bleibt alles unverändert.
- **EndpointPage:** Panel-Änderungen sind vor dem Speichern nicht mehr serverseitig persistiert — Verlassen ohne Speichern geht über den bestehenden Dirty-Guard (`_isDirty` + Navigations-/Before-Unload-Guards). Risiko minimal: parallele REST-Einzel-Updates anderer Clients auf demselben Endpunkt werden beim nächsten Vollmengen-Save überschrieben — war aber auch vorher so (Vollmengen-Ersetzung).
- **Performance:** REST-Update macht künftig 2 Queries (`Get` + `Update`) + 1 Reload statt 2 + 2×n Einzel-Statements — reine Verbesserung.
- **`TryApplyPatch`-Refactoring:** Signaturwechsel private → kein API-Bruch.

## Umsetzungsreihenfolge

1. **`ODataEndpointUpdateHelper` anlegen**
   - Voraussetzungen: keine
   - Beschreibung: Neue interne statische Klasse in `src/Schnittstellenzentrale/OData/` mit `TryDeserializeEndpoint` (JsonSerializer: `PropertyNameCaseInsensitive`, `JsonStringEnumConverter`), `ContainsProperty(JsonElement, string)` (case-insensitive Property-Suche), `TryReadChildren(JsonElement, string propertyName, out IList<EndpointHeader>?/IList<EndpointQueryParameter>? , out string? error)` — `null` wenn Property fehlt, Liste bei Vorhandensein; Fehler wenn kein Array/ungültige Elemente.
2. **`EndpointsController.UpdateAsync` auf reine Aggregat-Operation umstellen**
   - Voraussetzungen: keine
   - Beschreibung: Kinder in das übergebene `Endpoint`-Objekt mappen, einen `UpdateEndpointAsync`-Aufruf, manuelle Loops entfernen.
3. **`ODataEndpointsController` Put/Patch umstellen**
   - Voraussetzungen: Schritt 1
   - Beschreibung: `Put` auf `JsonElement`-Signatur umstellen (Deserialisierung via Helfer, bestehende Prüfungen beibehalten, Kinder bei Vorhandensein ersetzen); `ApplyPatch` → `TryApplyPatch` mit Fehlerkanal + neuen `headers`/`queryparameters`-Fällen.
4. **`EndpointPage.razor` bereinigen**
   - Voraussetzungen: keine
   - Beschreibung: Vier Sofort-Persist-Methoden und ihre Callback-Verdrahtungen entfernen.
5. **XML-Docs präzisieren** (`EndpointRepository.UpdateEndpointAsync`, `IEndpointRepository.UpdateEndpointAsync`)
   - Voraussetzungen: keine
6. **Neue Tests schreiben** (siehe Tests-Abschnitt)
   - Voraussetzungen: Schritte 1–4 (Tests verifizieren das neue Verhalten)
7. **Dokumentation**: `docs/help/api/api.md` (PUT-Abschnitt: Vollmengen-Ersetzung präzisieren) und `docs/help/api/odata-api.md` (Put/Patch: Kind-Mengen-Semantik) — erfolgt in Lifecycle-Schritt 12.

## Tests

### Neue Tests

|| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
||--------------------|------------|-------------------------------------|
|| `UpdateEndpointAsync_ReplacesScalarsAndChildren` | `EndpointRepositoryIntegrationTests` | Endpunkt mit 2 Headern + 1 Param → Update mit 1 Header + 2 Params → persistierter Stand = neue Mengen (Aggregate in einem Schritt) |
|| `UpdateEndpointAsync_EmptyChildren_ClearsExisting` | `EndpointRepositoryIntegrationTests` | Update mit leeren `Headers`/`QueryParameters` → alle Kinder entfernt |
|| `UpdateEndpointAsync_ConcurrencyConflict_LeavesChildrenUnchanged` | `EndpointRepositoryIntegrationTests` | Stale `RowVersion` → `DbUpdateConcurrencyException`; Kinder im Bestand unverändert (Versions-Konflikt-Fall der Aggregat-Operation) |
|| `PutEndpoint_WithHeadersAndQueryParameters_ReplacesChildren` | `ApplicationApiClientIntegrationTests` | REST `PUT /api/endpoints/{id}` mit Vollmengen → `GET` zeigt exakt die mitgelieferten Mengen (einheitliche Semantik) |
|| `PutEndpoint_WithoutChildren_ClearsExisting` | `ApplicationApiClientIntegrationTests` | REST PUT ohne/leere `headers`/`queryParameters` → Kinder geleert (unveränderter Vertrag) |
|| `PutEndpoint_WithHeaders_ReplacesSuppliedSet` | `ODataControllerIntegrationTests` | OData PUT mit `"headers":[{key,value}]` → Header-Menge ersetzt, `queryParameters` unberührt |
|| `PutEndpoint_WithoutChildren_PreservesExisting` | `ODataControllerIntegrationTests` | OData PUT ohne Kinder-Felder → Bestands-Kinder unverändert |
|| `PatchEndpoint_WithQueryParameters_ReplacesSet` | `ODataControllerIntegrationTests` | OData PATCH mit `queryparameters`-Array → Menge ersetzt |
|| `PatchEndpoint_WithMalformedHeaders_Returns400` | `ODataControllerIntegrationTests` | PATCH mit `headers` als Nicht-Array → 400 |

### Betroffene bestehende Tests

|| Test / Testklasse | Grund der Anpassung |
||-------------------|---------------------|
|| `ODataControllerIntegrationTests.PutEndpoint_*` (bestehende) | Signaturwechsel auf `JsonElement` — bestehende Tests senden `JsonContent.Create(new Endpoint{...})`; Verhalten muss identisch bleiben (Nachweis durch unverändert grüne Suite) |
|| `EndpointPageTests` | Keine Anpassung nötig — kein Test verifiziert die entfallenen Einzel-API-Aufrufe; Save-Pfad unverändert |

### E2E-Tests (primärer Funktionsnachweis)

Begründung für keine neuen E2E-Tests: Die Anforderung ändert keinen sichtbaren Benutzerfluss — Speichern auf der Endpunktseite zeigt exakt dasselbe Verhalten (Dirty-Badge → Speichern-Button → Vollmengen-Persistenz). Die entfallene Sofort-Persistenz ist für Anwender nicht sichtbar (war ein redundanter Doppel-Pfad). Der Persistenz-Happy-Path wird durch die geplanten Integrations-Tests gegen `WebApplicationFactory` (echter HTTP-Stack) nachgewiesen; bestehende Playwright-Suiten (u. a. `EndpointExecutionTests`, `EndpointGroupCrudTests`) bleiben unverändert grün. Bestehende E2E-Tests müssen nicht angepasst werden.

## Akzeptanzszenarien

|| # | Szenario | Auslöser | Ausgangszustand | Schritte | Sichtbare Wirkung | Nachweis |
||---|----------|----------|------------------|----------|-------------------|----------|
|| 1 | Happy Path REST-Aggregat-Update | `PUT /api/endpoints/{id}` mit Skalaren + `headers` + `queryParameters` | Endpunkt mit abweichenden Skalaren und anderen Kind-Mengen existiert | Request → `UpdateEndpointAsync` → Response + Reload | Persistierter Endpunkt enthält exakt die mitgelieferten Skalare und Kind-Mengen; `RowVersion` erhöht | `PutEndpoint_WithHeadersAndQueryParameters_ReplacesChildren` + reale Abnahme |
|| 2 | Happy Path OData PUT mit Kindern | `PUT /odatav4/Endpoints({key})` mit `headers`-Array | Endpunkt mit Bestands-Headern | Request → Deserialisierung → `UpdateEndpointAsync` | Header-Menge entspricht der mitgelieferten; unberührte `queryParameters` bleiben | `PutEndpoint_WithHeaders_ReplacesSuppliedSet` + reale Abnahme |
|| 3 | OData PATCH mit Kind-Menge | `PATCH /odatav4/Endpoints({key})` mit `queryparameters` | Endpunkt mit Bestands-Query-Parametern | Request → `TryApplyPatch` → `UpdateEndpointAsync` | Query-Parameter-Menge ersetzt | `PatchEndpoint_WithQueryParameters_ReplacesSet` |
|| 4 | Versions-Konflikt | Update mit veralteter `RowVersion` auf beliebigem Pfad | Endpunkt wurde zwischenzeitlich geändert | `UpdateEndpointAsync` → `DbUpdateConcurrencyException` | Kein Stand verändert (weder Skalare noch Kinder); OData Patch → 409 | `UpdateEndpointAsync_ConcurrencyConflict_LeavesChildrenUnchanged` |
|| 5 | UI-Speichern ohne Sofort-Persistenz | Header im Panel hinzufügen → Speichern | Bestehender Endpunkt | Panel-Edit → Dirty-Badge → Save → `UpdateEndpointAsync` (Vollmenge) | Nach Reload sind die Kinder exakt wie bearbeitet; vor Speichern keine serverseitige Änderung | `EndpointPageTests` (Save-Pfad) + reale Abnahme (UI) |

## Offene Punkte

Keine.
