# Logik und Abläufe

## `EndpointRepository`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/EndpointRepository.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `GetEndpointByIdAsync` | public | Lädt Endpunkt inkl. `Application`, `Headers`, `QueryParameters`, `EndpointGroup` |
| `AddEndpointAsync` | public | Fügt Endpunkt ein (`context.Endpoints.Add`) |
| `UpdateEndpointAsync` | public | **Aggregat-Update:** lädt Bestand inkl. Kinder, setzt `RowVersion` als `OriginalValue`, `ApplyUpdate` für Skalare, entfernt alle Bestands-Kinder (`context.Remove`), fügt übergebene `Headers`/`QueryParameters` neu ein, ein `SaveChangesAsync`. Wirft `InvalidOperationException` wenn nicht gefunden; `DbUpdateConcurrencyException` bei Versionskonflikt. |
| `AddHeaderAsync` / `DeleteHeaderAsync` | public | Einzel-Header anlegen/löschen (eigener DbContext pro Aufruf) |
| `AddQueryParameterAsync` / `DeleteQueryParameterAsync` | public | Einzel-Query-Parameter anlegen/löschen |

## `EndpointsController`
Datei: `src/Schnittstellenzentrale/Controllers/EndpointsController.cs`

| Methode | Kurzbeschreibung |
|---------|------------------|
| `UpdateAsync` (Z.150–206) | `PUT /api/endpoints/{id}`: Lädt `existing` (404 wenn null), baut neues `Endpoint`-Objekt mit Skalaren + `RowVersion` aus `UpdateEndpointRequest` — **ohne** Kinder. Ruft `UpdateEndpointAsync` (löscht intern alle Kinder!), dann manuell: `DeleteHeaderAsync`/`DeleteQueryParameterAsync` pro Bestands-Kind (no-op, da bereits gelöscht) und `AddHeaderAsync`/`AddQueryParameterAsync` pro Request-Kind. Danach `GetEndpointByIdAsync` für Response, SignalR-Notify im Team-Modus. |
| `CreateAsync` (Z.106–132) | `POST /api/endpoints`: nur Skalare aus `CreateEndpointRequest` (keine Kinder im Vertrag) |
| `AddHeaderAsync` / `DeleteHeaderAsync` (Z.253–295) | `POST`/`DELETE /api/endpoints/headers` — Einzel-Kind-Endpunkte |
| `AddQueryParameterAsync` / `DeleteQueryParameterAsync` (Z.310–352) | `POST`/`DELETE /api/endpoints/query-parameters` — Einzel-Kind-Endpunkte |

## `ODataEndpointsController`
Datei: `src/Schnittstellenzentrale/OData/ODataEndpointsController.cs`

| Methode | Kurzbeschreibung |
|---------|------------------|
| `Put` (Z.97–135) | `PUT /odatav4/Endpoints({key})`: `[FromBody] Endpoint entity` (OData-Formatter bindet per EDM — `Headers`/`QueryParameters` sind per `Ignore` ausgeblendet, bleiben `[]`). Kopiert Skalare + `RowVersion` auf das geladene `existing` (dessen Kinder geladen sind) → `UpdateEndpointAsync(existing)` persistiert Kinder identisch neu. **Mitgelieferte `headers`/`queryparameters` im Body werden vom OData-Formatter still ignoriert.** Kein `DbUpdateConcurrencyException`-Catch (im Gegensatz zu `Patch`). Prüft `EndpointGroupId`-Zugehörigkeit + `IsSystem`-Schutz, `RowVersion` Pflicht. |
| `Patch` (Z.141–191) | `PATCH /odatav4/Endpoints({key})`: `[FromBody] JsonElement patch`, `ApplyPatch` für Skalare, `TryExtractRowVersion` (Pflicht), `DbUpdateConcurrencyException` → 409 Conflict. **`headers`/`queryparameters` im Patch-Dokument werden still ignoriert.** |
| `ApplyPatch` (Z.193–222, private static) | Switch über Property-Namen: `name`, `relativepath`, `body`, `prerequestscript`, `postrequestscript`, `endpointgroupid`, `method`, `bodymode`, `authenticationtype`. Keine `headers`/`queryparameters`-Fälle. |
| `Post` / `Delete` | Anlegen (nur Skalare relevant) bzw. Löschen mit `IsSystem`-Schutz |

## `ODataEdmModelBuilder`
Datei: `src/Schnittstellenzentrale/OData/ODataEdmModelBuilder.cs`

- `Endpoints`-EntityType: `Ignore(e => e.Headers)`, `Ignore(e => e.QueryParameters)` (Z.36–37) — Kinder sind nicht im `$metadata`/EDM sichtbar; OData-Deserialisierung bindet sie nicht.

## `ODataPatchHelper`
Datei: `src/Schnittstellenzentrale/OData/ODataPatchHelper.cs`

- `TryApplyPatch(JsonElement, Application, out error)` und `TryApplyPatch(JsonElement, ApplicationGroup, out error)` — generische Patch-Helfer für andere Entitäten, `Endpoint` hat eigenes `ApplyPatch` im Controller.
- `ContainsRowVersion`, `TryExtractRowVersion`, `TryApplyIconData`.

## `SwaggerImportService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/SwaggerImportService.cs`

| Methode | Kurzbeschreibung |
|---------|------------------|
| `ApplyDiffAsync` (Z.103–144) | `NewEndpoints` → `EndpointGroupHelper.ResolveGroupIdAsync` + `AddEndpointAsync`; `ChangedEndpoints` → **`UpdateEndpointAsync` (Aggregat-Op bereits genutzt)**; `RemovedEndpoints` → `DeleteEndpointAsync`. Validierung: Diff-Endpunkte müssen zur `applicationId` gehören. |
| `ImportAsync` / `SyncFromDocumentAsync` | Diff-Berechnung via `ImportDiffCalculator.Calculate` |

## `ODataImportService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ODataImportService.cs`

| Methode | Kurzbeschreibung |
|---------|------------------|
| `ApplyDiffAsync` (Z.126–190) | Analog: `NewEndpoints` → Gruppenauflösung + `AddEndpointAsync`; `ChangedEndpoints` → Gruppenauflösung + **`UpdateEndpointAsync`**; `RemovedEndpoints` → `DeleteEndpointAsync`. |
| `AddEndpoint` (Z.232–257, private static) | Baut importierte `Endpoint`-Objekte inkl. `Headers` aus `x-sz-header-*`-Annotationen |

## `ImportDiffCalculator`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ImportDiffCalculator.cs`

- `Calculate`: Diff über `EndpointKeyHelper.BuildKey`; `HasChanged` vergleicht nur Skalare (Name, Body, AuthenticationType, PreRequestScript, PostRequestScript) — reine Kinder-Änderungen erzeugen keinen `ChangedEndpoints`-Eintrag.
- `MergeExistingIdentity` (Z.70–88): Übernimmt `Id` + `RowVersion` des Bestands, Skalare des Imports, **`Headers`/`QueryParameters` des Bestands** (Kopien ohne `EndpointId`) — importierte Kinder gehen bei ChangedEndpoints verloren (Bestandsverhalten).

## `ApplicationApiClient`
Datei: `src/Schnittstellenzentrale/Services/ApplicationApiClient.cs`

| Methode | Kurzbeschreibung |
|---------|------------------|
| `UpdateEndpointAsync` (Z.315–339) | `PUT /api/endpoints/{id}` mit `UpdateEndpointRequest` inkl. Vollmengen `Headers`/`QueryParameters` (aus `endpoint.Headers`/`QueryParameters` gemappt) |
| `AddEndpointAsync` (Z.290–312) | `POST /api/endpoints` nur Skalare |
| `AddHeaderAsync` / `DeleteHeaderAsync` / `AddQueryParameterAsync` / `DeleteQueryParameterAsync` (Z.352–405) | Einzel-Kind-REST-Aufrufe — von `EndpointPage` pro Panel-Änderung und bei Neuanlage genutzt |

## `EndpointPage.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor`

| Element | Kurzbeschreibung |
|---------|------------------|
| `SaveAsync` (Z.366–388) | Setzt `_model.Headers`/`QueryParameters` aus den Panel-Listen (Vollmenge), ruft `PersistAsync`; `DbUpdateConcurrencyException` → `_showConcurrencyWarning` |
| `PersistAsync` (Z.414–423) | `Id == 0` → `CreateEndpointWithDetailsAsync`; sonst `ApplicationApiClient.UpdateEndpointAsync` + `RowVersion` übernehmen |
| `CreateEndpointWithDetailsAsync` (Z.425–442) | `AddEndpointAsync` (nur Skalare), dann pro Header/Param `AddHeaderAsync`/`AddQueryParameterAsync` — nötig, da `CreateEndpointRequest` keine Kinder enthält |
| `AddHeaderAsync`/`DeleteHeaderAsync`/`AddQueryParameterAsync`/`DeleteQueryParameterAsync` (Z.444–474) | **Sofort-Persistenz pro Panel-Änderung** bei `Id != 0` — verdrahtet über `OnHeaderAdded`/`OnHeaderRemoved`/`OnQueryParameterAdded`/`OnQueryParameterRemoved` (Z.74–83) |
| `MarkDirty` (Z.322–329) | Dirty-Flag + Navigationsguards; `OnChanged`-Callback der Panels |

## `ModelUpdateExtensions`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/ModelUpdateExtensions.cs`

- `ApplyUpdate(this Endpoint, Endpoint)` (Z.32–43): kopiert alle Skalare (`Name`, `Method`, `RelativePath`, `Body`, `BodyMode`, `AuthenticationType`, `EndpointGroupId`, `PreRequestScript`, `PostRequestScript`) — **nicht** `Id`, `ApplicationId`, `RowVersion`, `Headers`, `QueryParameters`.

## `RequestHeadersPanel.razor` / `RequestQueryParamsPanel.razor`
Dateien: `src/Schnittstellenzentrale/Components/Shared/`

- Parameter: `Headers`/`QueryParameters` (Liste), `OnChanged`, `OnHeaderAdded`/`OnHeaderRemoved` bzw. `OnQueryParameterAdded`/`OnQueryParameterRemoved`. `EventCallback` ohne Abonnent ist ein no-op — Callbacks können seitens `EndpointPage` entfallen, ohne die Panels anzupassen.
