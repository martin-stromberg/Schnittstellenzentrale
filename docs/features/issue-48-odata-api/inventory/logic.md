# Logik

## `ApiControllerBase`
Datei: `src/Schnittstellenzentrale/Controllers/ApiControllerBase.cs`

Abstrakte Basisklasse für alle REST-Controller. Die neuen OData-Controller müssen entweder von dieser Klasse erben oder die Token-Validierungslogik in einer eigenen `ODataControllerBase` neu implementieren.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `ValidateTokenAndSetResponseHeaderAsync()` | `protected async` | Prüft Bearer-Token via `ITokenStore.ValidateAndRotateAsync`, schreibt neues Token in `X-New-Token`-Header |
| `ParseStorageMode()` | `protected` | Liest `X-Storage-Mode`-Header, gibt `StorageMode.Team` oder `StorageMode.User` zurück |
| `ParseRequestContextAsync()` | `protected async` | Kombiniert Token-Validierung, StorageMode und Owner in `RequestContext`; gibt `null` bei ungültigem Token |
| `MapToResponse(Application)` | `protected static` | Mappt `Application` auf `ApplicationResponse`-DTO |
| `MapToResponse(ApplicationGroup)` | `protected static` | Mappt `ApplicationGroup` auf `ApplicationGroupResponse`-DTO (inkl. `Applications`-Liste) |
| `MapToResponse(EndpointGroup)` | `protected static` | Mappt `EndpointGroup` auf `EndpointGroupResponse`-DTO |
| `MapToResponse(EndpointHeader)` | `protected static` | Mappt `EndpointHeader` auf `EndpointKeyValueResponse`-DTO |
| `MapToResponse(EndpointQueryParameter)` | `protected static` | Mappt `EndpointQueryParameter` auf `EndpointKeyValueResponse`-DTO |
| `MapToResponse(Endpoint)` | `protected static` | Mappt `Endpoint` auf `EndpointResponse`-DTO inkl. Headers und QueryParameters |

---

## `ApplicationsController`
Datei: `src/Schnittstellenzentrale/Controllers/ApplicationsController.cs`

Route: `api/applications`. Analogvorlage für `ODataApplicationsController`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `GetAllAsync()` | `public async` | GET alle Anwendungen; liest StorageMode+Owner aus Headern |
| `GetUngroupedAsync()` | `public async` | GET ungrupierte Anwendungen |
| `GetByIdAsync(int id)` | `public async` | GET einzelne Anwendung; 404 wenn nicht gefunden |
| `CreateAsync(CreateApplicationRequest)` | `public async` | POST Anwendung anlegen; löst SignalR-Benachrichtigung aus |
| `UpdateAsync(int id, UpdateApplicationRequest)` | `public async` | PUT Anwendung aktualisieren; 403 bei Systemanwendung |
| `DeleteAsync(int id)` | `public async` | DELETE Anwendung; 403 bei Systemanwendung |
| `ApplyRequestToApplication(Application, UpdateApplicationRequest)` | `private static` | Übernimmt Request-Felder in die Entity; ruft `DetectInterfaceType` auf |

---

## `ApplicationGroupsController`
Datei: `src/Schnittstellenzentrale/Controllers/ApplicationGroupsController.cs`

Route: `api/application-groups`. Analogvorlage für `ODataApplicationGroupsController`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `GetAllAsync()` | `public async` | GET alle Gruppen |
| `GetByIdAsync(int id)` | `public async` | GET einzelne Gruppe |
| `CreateAsync(CreateApplicationGroupRequest)` | `public async` | POST Gruppe anlegen |
| `UpdateAsync(int id, UpdateApplicationGroupRequest)` | `public async` | PUT Gruppe aktualisieren; 403 bei Systemgruppe |
| `DeleteAsync(int id)` | `public async` | DELETE Gruppe; 403 bei Systemgruppe |

---

## `EndpointsController`
Datei: `src/Schnittstellenzentrale/Controllers/EndpointsController.cs`

Route: `api/endpoints`. Analogvorlage für `ODataEndpointsController`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `GetAllAsync(int applicationId, int? groupId)` | `public async` | GET Endpunkte einer Anwendung, optional gefiltert nach Gruppe |
| `GetByIdAsync(int id)` | `public async` | GET einzelner Endpunkt |
| `CreateAsync(CreateEndpointRequest)` | `public async` | POST Endpunkt anlegen; löst SignalR aus |
| `UpdateAsync(int id, UpdateEndpointRequest)` | `public async` | PUT Endpunkt aktualisieren; ersetzt Header und Query-Parameter vollständig |
| `DeleteAsync(int id)` | `public async` | DELETE Endpunkt |
| `AddHeaderAsync(AddEndpointKeyValueRequest)` | `public async` | POST Header hinzufügen |
| `DeleteHeaderAsync(int id)` | `public async` | DELETE Header |
| `AddQueryParameterAsync(AddEndpointKeyValueRequest)` | `public async` | POST Query-Parameter hinzufügen |
| `DeleteQueryParameterAsync(int id)` | `public async` | DELETE Query-Parameter |

---

## `EndpointGroupsController`
Datei: `src/Schnittstellenzentrale/Controllers/EndpointGroupsController.cs`

Route: `api/endpoint-groups`. Analogvorlage für `ODataEndpointGroupsController`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `GetAllAsync(int applicationId)` | `public async` | GET Gruppen einer Anwendung |
| `GetByIdAsync(int id)` | `public async` | GET einzelne Gruppe |
| `CreateAsync(CreateEndpointGroupRequest)` | `public async` | POST Gruppe anlegen |
| `UpdateAsync(int id, UpdateEndpointGroupRequest)` | `public async` | PUT Gruppe aktualisieren |
| `DeleteAsync(int id)` | `public async` | DELETE Gruppe |

---

## `ODataImportService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ODataImportService.cs`

Implementierung von `IODataImportService`. Verwendet `Microsoft.OData.Edm` (bereits referenziert in `Schnittstellenzentrale.Infrastructure.csproj`).

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `ImportAsync(Application)` | `public async` | Ruft `InterfaceUrl` per HTTP ab, parst CSDL mit `CsdlReader.Parse`, erstellt GET+POST-Endpunkte je Entity-Set; gibt `ImportDiff` zurück |
| `ApplyDiffAsync(ImportDiff)` | `public async` | Fügt neue Endpunkte hinzu, aktualisiert geänderte, löscht entfernte |

Abhängigkeiten: `IHttpClientFactory`, `IEndpointRepository`, `ILogger<ODataImportService>`.

---

## `SystemEndpointSyncService`
Datei: `src/Schnittstellenzentrale/SystemEndpointSyncService.cs`

`BackgroundService`; synchronisiert beim Start die Endpunkte der Systemanwendung aus dem Swagger-Dokument. Verwendet **kein** `IODataImportService` und darf die `/odatav4`-Route nicht registrieren.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `ExecuteAsync(CancellationToken)` | `protected override async` | Hauptlogik: ermittelt Systemgruppe und -anwendung, ruft `SyncEndpointsAsync` auf |
| `SyncEndpointsAsync(...)` | `private static async` | Holt Swagger-Dokument via `ISwaggerProvider`, berechnet Diff, ruft `ISwaggerImportService.ApplyDiffAsync` auf |

---

## `Program` (`BuildWebApplicationAsync`)
Datei: `src/Schnittstellenzentrale/Program.cs`

Relevante bestehende Registrierungen:

- `builder.Services.AddControllers()` — ohne OData-Erweiterung; `Microsoft.AspNetCore.OData` ist noch nicht referenziert
- `builder.Services.AddScoped<IODataImportService, ODataImportService>()` — bereits registriert
- `app.MapControllers()` — ohne OData-Route-Mapping
- Authentifizierung: Negotiate (oder Windows bei IIS-App-Pool) + `app.UseAuthentication()` / `app.UseAuthorization()`
- Kein `app.MapODataRoute(...)` vorhanden
