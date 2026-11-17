# Logik, Services, Controller

## Import- und Sync-Logik

## `SwaggerImportService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/SwaggerImportService.cs`

Implementiert `ISwaggerImportService`. DI: `IHttpClientFactory`, `IEndpointRepository`, `ICredentialService`, `ILogger`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `ImportAsync` | public | InterfaceUrl leer → leerer Diff; `FetchSwaggerStreamAsync` → `ParseSwaggerDocumentAsync` (Fehler → `ImportDiff.ErrorMessage`); `SwaggerOperationHelper.MapDocumentToEndpoints`; Bestand via `_endpointRepository.GetEndpointsAsync`; `ImportDiffCalculator.Calculate(...).WithBearerTokens(...)` (Z. 33–61) |
| `ApplyDiffAsync` | public | Für `NewEndpoints`: EndpointGroup-Auflösung via `EndpointGroupHelper.ResolveGroupIdAsync` (Pfadsegmente), `AddEndpointAsync`, `SaveBearerTokenIfPresent`. Für `ChangedEndpoints`: `UpdateEndpointAsync` + Token. Für `RemovedEndpoints`: `DeleteEndpointAsync`. **Keine** InterfaceType-/Aggregat-Validierung (Z. 85–110) |
| `FetchSwaggerStreamAsync` | private | `HttpClient.GetStreamAsync` (Z. 63–67) |
| `ParseSwaggerDocumentAsync` | private static | `OpenApiJsonReader.ReadAsync`; Errors → `ImportDiff` mit `ErrorMessage` (Z. 69–82) |
| `SaveBearerTokenIfPresent` | private | `EndpointKeyHelper.BuildKey` → `CredentialTargetHelper.Build` → `ICredentialService.SavePassword` (Z. 112–130) |

Wird aufgerufen von: `ApplicationImportController.ImportAsync` (nur `ImportAsync`), `SwaggerImportDialog.razor` (nur `ApplyDiffAsync`), `SystemEndpointSyncService` (nur `ApplyDiffAsync`).

## `ImportDiffCalculator`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ImportDiffCalculator.cs`

`internal static`. Einzige Methode `Calculate(existing, imported)`:
- Duplikatschlüssel in Bestand oder Import → `ImportDiff` nur mit `ErrorMessage` (Z. 11–15)
- `New`/`Changed`/`Removed` über `EndpointKeyHelper.BuildKey` = `"{Method}:{RelativePath}"` (Z. 17–35)
- `HasChanged` vergleicht `Name`, `Body`, `AuthenticationType`, `PreRequestScript`, `PostRequestScript` (Z. 61–68)
- `MergeExistingIdentity` übernimmt `Id`, `EndpointGroupId`, `RowVersion` vom Bestand, **Headers/QueryParameters werden als Kopie vom Bestand übernommen** (nicht vom Import) (Z. 70–88)

Wird aufgerufen von: `SwaggerImportService.ImportAsync`, `ODataImportService.ImportAsync`. `SystemEndpointSyncService` nutzt ihn **nicht** (eigener Diff).

## `SystemEndpointSyncService`
Datei: `src/Schnittstellenzentrale/SystemEndpointSyncService.cs`

`BackgroundService`, DI: `IServiceScopeFactory`, `ILogger`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `ExecuteAsync` | protected override | Scope → `IApplicationRepository.GetSystemGroupAsync` → `IsSystem`-App suchen → `SyncEndpointsAsync`; Fehler werden geloggt, nie geworfen (Z. 28–65) |
| `SyncEndpointsAsync` | private static | `ISwaggerProvider.GetSwagger("v1")` → `SwaggerOperationHelper.MapDocumentToEndpoints` → **eigener Diff nur `New`/`Removed`** via `EndpointKeyHelper`-HashSets (Z. 76–84) → `swaggerImportService.ApplyDiffAsync(diff)` (Z. 67–87) |

Direkter `IEndpointRepository`-Bezug (Z. 52, 76) — laut CLAUDE.md die explizit dokumentierte Ausnahme vom API-First-Prinzip. `ChangedEndpoints` wird nicht erzeugt; Duplikatschlüssel-Erkennung fehlt.

## `SwaggerOperationHelper`
Datei: `src/Schnittstellenzentrale.Infrastructure/Helpers/SwaggerOperationHelper.cs`

`public static`. `MapDocumentToEndpoints(OpenApiDocument, int applicationId)` → `(List<Endpoint>, Dictionary<string,string> bearerTokens)` (Z. 51–89): mappt `x-sz-bearer-token`, `x-sz-pre-request-script`, `x-sz-post-request-script` (mit Authenticate-Endpoint-Namensersetzung), `Negotiate`-Security-Scheme-Erkennung, Header-Defaults aus `ParameterLocation.Header`. Weitere Helfer: `ReadExtensionString`, `MapHttpMethod`.

## `EndpointGroupHelper`
Datei: `src/Schnittstellenzentrale.Infrastructure/Helpers/EndpointGroupHelper.cs`

`internal static`. `ResolveGroupIdAsync` legt EndpointGroup-Hierarchie aus `RelativePath`-Segmenten an (überspringt `api` und `{param}`); `ParseGroupSegments` liefert die Segmente.

## `EndpointKeyHelper` / `CredentialTargetHelper`
Dateien: `src/Schnittstellenzentrale.Core/Helpers/EndpointKeyHelper.cs`, `CredentialTargetHelper.cs`

`BuildKey(Endpoint)` → `"{Method}:{RelativePath}"` — gemeinsamer Diff-/Token-Schlüssel für Swagger- und OData-Import.

## `ODataImportService` (Vergleichspfad)
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ODataImportService.cs`

`ImportAsync` (Z. 36–123): Fetch → `CsdlReader.Parse` → Endpunkte je EntitySet (GET/POST/PUT/PATCH/DELETE) + Operationen → `ImportDiffCalculator.Calculate`. `ApplyDiffAsync` (Z. 126–190): gruppiert neue/geänderte Endpunkte nach `ExtractEntitySetName` in flache EndpointGroups, `AddEndpointAsync`/`UpdateEndpointAsync`/`DeleteEndpointAsync`, `SaveBearerTokenOnce` (dedupliziert pro CredentialTarget). Ebenfalls **keine** InterfaceType-/Aggregat-Validierung im Service — die `InterfaceType`-Prüfung liegt im `ApplicationImportController`.

## Repository- und Persistenzlogik

## `EndpointRepository`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/EndpointRepository.cs`

Nutzt `IDbContextFactory<AppDbContext>` (pro Methode frischer Context).

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `UpdateEndpointAsync` | public | Lädt bestehenden Endpunkt inkl. Kinder; `RowVersion` als `OriginalValue` (Concurrency); `existing.ApplyUpdate(endpoint)` (Skalare via `ModelUpdateExtensions`); **löscht alle vorhandenen `Headers`/`QueryParameters` und fügt die übergebenen neu ein — in einer `SaveChangesAsync`** (Z. 104–130). Entspricht faktisch bereits einem Aggregat-Update |
| `AddEndpointAsync` / `DeleteEndpointAsync` | public | Anlegen/Löschen (Delete via generischem `DeleteByIdAsync<T>`) |
| `AddHeaderAsync` / `DeleteHeaderAsync` / `AddQueryParameterAsync` / `DeleteQueryParameterAsync` | public | Einzel-Kind-Operationen mit eigenem `SaveChangesAsync` je Aufruf |
| `Add/Update/DeleteEndpointGroupAsync` | public | `UpdateEndpointGroupAsync` setzt `RowVersion.OriginalValue` + `ApplyUpdate` (Name, ParentGroupId) |
| Get-Methoden | public | s. interfaces.md |

## `ApplicationRepository`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/ApplicationRepository.cs`

`DeleteGroupAsync` (Z. 79–88) lädt nur die Gruppe und entfernt sie; das Aushängen der Apps erledigt der FK-`SetNull`. **Keine** Transaktionsmethode für Löschen-mit-Inhalt. `Update*` setzen jeweils `RowVersion.OriginalValue` + `ModelUpdateExtensions.ApplyUpdate`.

## `ModelUpdateExtensions`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/ModelUpdateExtensions.cs`

`internal static`, `ApplyUpdate`-Überladungen für `ApplicationGroup` (Name/Description/Subtitle/IconData), `Application` (Name/Description/BaseUrl/InterfaceUrl/InterfaceType/Owner/ApplicationGroupId), `EndpointGroup` (Name/ParentGroupId), `Endpoint` (Name/Method/RelativePath/Body/BodyMode/AuthenticationType/EndpointGroupId/PreRequestScript/PostRequestScript — **keine** Kinder).

## `AppDbContext` (relevante Delete-Behaviors)
Datei: `src/Schnittstellenzentrale.Infrastructure/Data/AppDbContext.cs`

- `ApplicationGroup → Applications`: `OnDelete(SetNull)` (Z. 80–83)
- `Application → Endpoints`, `Application → EndpointGroups`: `Cascade` (Z. 95–102)
- `EndpointGroup → Endpoints`: `Cascade`; `EndpointGroup → ChildGroups`: `Restrict` (Z. 110–117)
- `Endpoint → Headers`/`QueryParameters`: `Cascade` (Z. 126–133)
- `SaveChangesAsync`/`SaveChanges` setzen `RowVersion` auf `Guid.NewGuid().ToByteArray()` für Added/Modified (Z. 44–67)

## `SignalRNotificationService<THub>`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/SignalRNotificationService.cs`

Generischer Dienst über `IHubContext<THub>`; registriert als `SignalRNotificationService<EndpointHub>` (Program.cs Z. 141). Sendet auf SignalR-Gruppen `workspace`, `application:{id}`, `group:{id}`, `environments` — **kein** StorageMode-Check im Service.

## `ApplicationApiClient`
Datei: `src/Schnittstellenzentrale/Services/ApplicationApiClient.cs`

HTTP-Client (`AddHttpClient`-Registrierung Program.cs Z. 144). Token-Lebenszyklus: `EnsureTokenAsync` (Token via `ITokenStore.CreateTokenAsync` + `ICurrentUserService`), Retry bei `401` mit Token-Rotation aus `X-New-Token`-Header (Z. 497–520). Alle Requests tragen `X-Storage-Mode`-Header aus `IStorageModeService.CurrentMode` (`BuildGetRequest`/`BuildRequestWithBody`/`BuildDeleteRequest`, Z. 522–554); `X-Owner` nur bei `GetGroupsAsync`/`GetUngroupedApplicationsAsync`.

| Methode | Kurzbeschreibung |
|---------|------------------|
| `DeleteGroupAsync` | `DELETE /api/application-groups/{id}` — kein Query-Parameter (Z. 105–112) |
| `ImportMetadataAsync` | `POST /api/applications/{id}/import`; `422` → `ImportDiff.ErrorMessage`, sonstige Fehler ebenfalls als `ErrorMessage` (Z. 397–419) |
| `ApplyODataDiffAsync` | `POST /api/applications/{id}/odata-import/apply` mit `ImportDiff`-Body; Fehler → `InvalidOperationException` mit `errorMessage`-Body (Z. 437–457). Enthält auskommentierten Header-Backlink-Code (Z. 442–447) |
| `UpdateEndpointAsync` | `PUT /api/endpoints/{id}` mit `UpdateEndpointRequest` inkl. Headers/QueryParameters-Vollmenge (Z. 304–328) |
| übrige CRUD | s. interfaces.md |

## Controller

## `ApiControllerBase`
Datei: `src/Schnittstellenzentrale/Controllers/ApiControllerBase.cs`

| Methode | Kurzbeschreibung |
|---------|------------------|
| `ValidateTokenAndSetResponseHeaderAsync` | Bearer-Token via `ITokenStore.ValidateAndRotateAsync`; schreibt `X-New-Token`-Response-Header; `null` bei ungültig (Z. 29–42) |
| `ParseStorageMode` | `X-Storage-Mode`-Header `"Team"` → `StorageMode.Team`, sonst `User` (Z. 46–50) |
| `ParseRequestContextAsync` | Token + StorageMode + `X-Owner` → `RequestContext` (Z. 54–63) |
| `MapToResponse`-Überladungen | DTO-Mapping für Application/ApplicationGroup/EndpointGroup/Endpoint/KeyValue (Z. 67–146) |

`[AllowAnonymous]` + `[ApiController]` auf Klassenebene; Auth läuft manuell über Token-Validierung.

## `ApplicationImportController`
Datei: `src/Schnittstellenzentrale/Controllers/ApplicationImportController.cs` — Route `api/applications`

| Aktion | Route | Verhalten |
|--------|-------|-----------|
| `ApplyODataDiffAsync` | `POST {id}/odata-import/apply` | Token → 401; `GetApplicationByIdAsync` → 404; `InterfaceType != OData` → 422 `{errorMessage}`; `_odataImportService.ApplyDiffAsync` → 204 (Z. 42–62) |
| `ImportAsync` | `POST {id}/import` | Token → 401; App → 404; `Rest` → `SwaggerImportService.ImportAsync`, `OData` → `ODataImportService.ImportAsync`, sonst 422 (Z. 71–99) |

`swagger-import/apply` **existiert nicht**.

## `ApplicationGroupsController`
Datei: `src/Schnittstellenzentrale/Controllers/ApplicationGroupsController.cs` — Route `api/application-groups`

| Aktion | Verhalten |
|--------|-----------|
| `GetAllAsync` | `RequiresContextHeaders(includeOwner: true)`; `GetGroupsAsync(context.StorageMode, context.Owner)` |
| `GetByIdAsync` | 404 bei fehlender Gruppe |
| `CreateAsync` | `Team` → `NotifyTreeChangedAsync` (Z. 105–106); 201 |
| `UpdateAsync` | 404; `IsSystem` → 403; `Team` → `NotifyTreeChangedAsync` (Z. 146–147) |
| `DeleteAsync` | 404; `IsSystem` → 403; `DeleteGroupAsync(id)` — **kein `content`-Parameter**; `Team` → `NotifyTreeChangedAsync` (Z. 165–186) |

## `EndpointsController`
Datei: `src/Schnittstellenzentrale/Controllers/EndpointsController.cs` — Route `api/endpoints`

| Aktion | Verhalten |
|--------|-----------|
| `GetAllAsync` | optionaler `groupId`-Filter mit App-Zugehörigkeitsprüfung → 404 |
| `CreateAsync` | `Team` → `NotifyEndpointChangedAsync` (Z. 127–128) |
| `UpdateAsync` | `GetEndpointByIdAsync` → 404; baut `Endpoint` **ohne** Headers/QueryParameters; `UpdateEndpointAsync` (löscht damit faktisch alle Kinder); **dann erneut manuell**: alle bestehenden Kinder einzeln `DeleteHeaderAsync`/`DeleteQueryParameterAsync` + `AddHeaderAsync`/`AddQueryParameterAsync` aus `request` (Z. 178–196); Reload; `Team` → `NotifyEndpointChangedAsync` (Z. 150–206) |
| `DeleteAsync` | `Team` → `NotifyEndpointChangedAsync` (Z. 234–235) |
| `AddHeaderAsync` / `DeleteHeaderAsync` / `AddQueryParameterAsync` / `DeleteQueryParameterAsync` | Einzel-Kind-Endpunkte (`POST/DELETE headers`, `…/query-parameters`) — **ohne** Notify (Z. 248–352) |

## `EndpointGroupsController`
Datei: `src/Schnittstellenzentrale/Controllers/EndpointGroupsController.cs` — Route `api/endpoint-groups`

Standard-CRUD mit `RequiresContextHeaders`; Create/Update/Delete feuern `NotifyEndpointGroupChangedAsync` nur bei `StorageMode.Team` (Z. 106, 149, 181). `DeleteAsync` löscht die Gruppe — enthaltene `Endpoints` kaskadieren (FK `Cascade`), `ChildGroups` sind `Restrict`.

## `ApplicationsController` (Referenz Notify-Muster)
`CreateAsync`/`UpdateAsync`/`DeleteAsync` feuern `NotifyTreeChangedAsync` bei `StorageMode.Team` (Z. 130, 172, 208).

## OData-Controller

## `ODataControllerBase`
Datei: `src/Schnittstellenzentrale/OData/ODataControllerBase.cs`

`IAsyncActionFilter`: validiert Bearer-Token (`ITokenStore.ValidateAndRotateAsync`), setzt `HttpContext.Items["ODataAuthenticatedUser"]` + `X-New-Token`-Header; `ParseStorageMode` analog `ApiControllerBase`. `[AllowAnonymous]`, `[ApiExplorerSettings(IgnoreApi = true)]`.

## `ODataEndpointsController`
Datei: `src/Schnittstellenzentrale/OData/ODataEndpointsController.cs` — Route `odatav4`

DI: `IEndpointRepository`, `IApplicationRepository` — **kein** `ISignalRNotificationService`.

| Aktion | Verhalten |
|--------|-----------|
| `Get` / `Get(key)` | `[EnableQuery]`; In-Memory-Filterung ist kommentiert dokumentiert (Z. 32–34); Owner-/App-Scope-Prüfung → 403 |
| `Post` | App → 404; `IsSystem` → 403; `Id=0`, `RowVersion=[]`; `AddEndpointAsync`; **kein Notify** |
| `Put` | 404/403; `EndpointGroupId`-Validierung (selbe App, System-Gruppe → 400/403); `RowVersion` Pflicht → 400; **nur Skalare** werden kopiert (Z. 122–131) — `Headers`/`QueryParameters` des Payloads werden **still ignoriert**; `UpdateEndpointAsync` — weil `existing.Headers` geladen bleibt, entsteht **kein** Datenverlust, aber Kinder werden nicht gepflegt |
| `Patch` | 404/403; `endpointgroupid`-Validierung; `rowVersion` Pflicht; `ApplyPatch` (nur Skalare: name/relativepath/body/scripts/endpointgroupid/method/bodymode/authenticationtype); `DbUpdateConcurrencyException` → `Conflict`; **kein Notify** |
| `Delete` | 404/403; `DeleteEndpointAsync`; **kein Notify** |

## `ODataApplicationsController` / `ODataApplicationGroupsController` / `ODataEndpointGroupsController`
Dateien: `src/Schnittstellenzentrale/OData/OData*Controller.cs`

Je `Get`/`Get(key)`/`Post`/`Put`/`Patch`/`Delete` mit Token-Filter, `IsSystem`-/Owner-Schutz, `RowVersion`-Pflicht für Put/Patch (`ODataPatchHelper`), `DbUpdateConcurrencyException` → `Conflict`. **Keiner** injiziert `ISignalRNotificationService`, **kein** Notify in irgendeiner mutierenden Aktion.

## `ODataPatchHelper`
Datei: `src/Schnittstellenzentrale/OData/ODataPatchHelper.cs`

`internal static`: `TryApplyPatch` für `Application`/`ApplicationGroup`, `ContainsRowVersion`, `TryExtractRowVersion`, `TryApplyIconData` (Base64-Validierung).

## `ODataEdmModelBuilder`
Datei: `src/Schnittstellenzentrale/OData/ODataEdmModelBuilder.cs`

EntitySets `Applications`, `ApplicationGroups`, `Endpoints`, `EndpointGroups`; setzt `x-sz-post-request-script`-Vocabulary-Annotation je EntitySet (Token-Refresh-Skript, Z. 18–65).

## `RequiresContextHeadersAttribute`
Datei: `src/Schnittstellenzentrale/Filters/RequiresContextHeadersAttribute.cs`

Reines Marker-Attribut (`IncludeOwner`); dient Swagger-Doku (`ContextHeadersOperationFilter`), erzwingt Header nicht zur Laufzeit.
