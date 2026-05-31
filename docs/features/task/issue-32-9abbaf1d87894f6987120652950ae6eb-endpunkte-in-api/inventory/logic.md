# Logik

## `ApplicationApiClient`
Datei: `src/Schnittstellenzentrale/Services/ApplicationApiClient.cs`

Implementiert `IApplicationApiClient`. Nutzt `HttpClient` mit Token-Rotation (via `ITokenStore`) und `X-Storage-Mode`-Header.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `GetGroupsAsync(StorageMode, string)` | `public` | GET `/api/application-groups` mit Storage-Mode- und Owner-Header |
| `GetGroupByIdAsync(int)` | `public` | GET `/api/application-groups/{id}`, gibt `null` bei 404 |
| `AddGroupAsync(ApplicationGroup)` | `public` | POST `/api/application-groups`, mappt Response auf Domänenmodell |
| `UpdateGroupAsync(ApplicationGroup)` | `public` | PUT `/api/application-groups/{id}`, mappt Response auf Domänenmodell |
| `DeleteGroupAsync(int)` | `public` | DELETE `/api/application-groups/{id}` |
| `GetUngroupedApplicationsAsync(StorageMode, string)` | `public` | GET `/api/applications/ungrouped` mit Storage-Mode- und Owner-Header |
| `GetApplicationByIdAsync(int)` | `public` | GET `/api/applications/{id}`, gibt `null` bei 404 |
| `AddApplicationAsync(Application)` | `public` | POST `/api/applications`, mappt Response auf Domänenmodell |
| `UpdateApplicationAsync(Application)` | `public` | PUT `/api/applications/{id}`, mappt Response auf Domänenmodell |
| `DeleteApplicationAsync(int)` | `public` | DELETE `/api/applications/{id}` |
| `SendWithTokenAsync<TResponse>(Func<string, HttpRequestMessage>)` | `private` | Sendet Request, erwartet nicht-null Response |
| `SendWithTokenNullableAsync<TResponse>(Func<string, HttpRequestMessage>)` | `private` | Sendet Request, gibt `null` bei 404 zurück |
| `SendWithTokenNoContentAsync(Func<string, HttpRequestMessage>)` | `private` | Sendet Request ohne Response-Body |
| `ExecuteWithTokenAsync(Func<string, HttpRequestMessage>)` | `private` | Kernlogik: Token holen, Request senden, bei 401 Token erneuern, Token aus `X-New-Token` Header rotieren |
| `BuildGetRequest(string, StorageMode?, string?, string)` | `private static` | Baut GET-Request mit Authorization, X-Storage-Mode, X-Owner |
| `BuildRequestWithBody<TBody>(HttpMethod, string, TBody, StorageMode, string)` | `private static` | Baut POST/PUT-Request mit JSON-Body, Authorization, X-Storage-Mode |
| `BuildDeleteRequest(string, StorageMode, string)` | `private static` | Baut DELETE-Request mit Authorization, X-Storage-Mode |
| `EnsureTokenAsync()` | `private` | Holt Token über `ITokenStore.CreateTokenAsync` wenn kein Token vorhanden (thread-safe via `SemaphoreSlim`) |
| `MapToApplicationGroup(ApplicationGroupResponse)` | `private static` | Mappt DTO auf `ApplicationGroup`-Domänenmodell |
| `MapToApplication(ApplicationResponse)` | `private static` | Mappt DTO auf `Application`-Domänenmodell |
| `GetBaseUrl()` | `private` | Liest Basis-URL aus `IHttpContextAccessor` oder `IConfiguration["Api:BaseUrl"]` |

Abonnierte Events: keine
Publizierte Events: keine

---

## `EndpointRepository`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/EndpointRepository.cs`

Implementiert `IEndpointRepository`. Vollständige EF-Core-Implementierung mit `IDbContextFactory<AppDbContext>`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `GetEndpointsAsync(int)` | `public` | Lädt alle Endpunkte einer Anwendung inkl. Headers, QueryParameters, EndpointGroup |
| `GetEndpointsByApplicationIdsAsync(IEnumerable<int>)` | `public` | Lädt Endpunkte mehrerer Anwendungen |
| `GetEndpointByIdAsync(int)` | `public` | Lädt einen Endpunkt per ID inkl. Application, ApplicationGroup, Headers, QueryParameters, EndpointGroup |
| `GetEndpointByNameAsync(int, string)` | `public` | Lädt Endpunkte anhand ApplicationId und Name |
| `AddEndpointAsync(Endpoint)` | `public` | Fügt Endpunkt hinzu, gibt gespeichertes Objekt zurück |
| `UpdateEndpointAsync(Endpoint)` | `public` | Aktualisiert Endpunkt mit RowVersion-Check, ersetzt Headers und QueryParameters vollständig |
| `DeleteEndpointAsync(int)` | `public` | Löscht Endpunkt per ID |
| `GetEndpointGroupsAsync(int)` | `public` | Lädt alle Endpunktgruppen einer Anwendung inkl. Application und ApplicationGroup |
| `GetEndpointGroupByIdAsync(int)` | `public` | Lädt eine Endpunktgruppe per ID inkl. Endpoints |
| `AddEndpointGroupAsync(EndpointGroup)` | `public` | Fügt Endpunktgruppe hinzu |
| `UpdateEndpointGroupAsync(EndpointGroup)` | `public` | Aktualisiert Endpunktgruppe mit RowVersion-Check |
| `DeleteEndpointGroupAsync(int)` | `public` | Löscht Endpunktgruppe per ID |
| `AddHeaderAsync(EndpointHeader)` | `public` | Fügt einen einzelnen Header hinzu |
| `DeleteHeaderAsync(int)` | `public` | Löscht einen Header per ID |
| `AddQueryParameterAsync(EndpointQueryParameter)` | `public` | Fügt einen Query-Parameter hinzu |
| `DeleteQueryParameterAsync(int)` | `public` | Löscht einen Query-Parameter per ID |
| `DeleteByIdAsync<T>(int)` | `private` | Generische Hilfsmethode zum Löschen nach ID |

---

## `SystemEndpointSyncService`
Datei: `src/Schnittstellenzentrale/SystemEndpointSyncService.cs`

Erbt von `BackgroundService`. Führt nach App-Start einmalig den selektiven Endpunktabgleich für die Systemanwendung durch. Nutzt `IEndpointRepository` direkt (kein HTTP-Roundtrip).

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `ExecuteAsync(CancellationToken)` | `protected override` | Einstiegspunkt: holt Systemgruppe und -anwendung, delegiert an `SyncEndpointsAsync` |
| `SyncEndpointsAsync(ISwaggerProvider, IEndpointRepository, Application)` | `private` | Liest Swagger-Dokument, baut Import-Liste, holt bestehende Endpunkte/Gruppen, delegiert an `ApplyDiffAsync` |
| `BuildImportedEndpoints(OpenApiDocument, int)` | `private` | Erstellt Endpunkt-Objekte aus OpenAPI-Paths inkl. Auth-Erkennung, Pre/Post-Skripten, Bearer-Token-Extension, Default-Headers |
| `ApplyDiffAsync(...)` | `private` | Berechnet Diff (neue Endpunkte hinzufügen, entfernte löschen), ruft `ResolveGroupIdAsync` auf |
| `ResolveGroupIdAsync(string, int, IEndpointRepository, Dictionary<...>)` | `private static` | Erzeugt Gruppen aus URL-Segmenten, nutzt Lookup-Cache |
| `ParseGroupSegments(string)` | `private static` | Zerlegt Pfad in Gruppen-Segmente (überspringt `api` und `{param}`) |
| `DetectAuthenticationType(OpenApiOperation?)` | `private static` | Erkennt `Negotiate`-Authentifizierung aus OpenAPI-Security-Requirement |
| `BuildDefaultHeaders(OpenApiOperation?)` | `private static` | Erstellt Header-Einträge aus OpenAPI-Header-Parametern |
| `ExtractDefaultValue(IOpenApiParameter)` | `private static` | Extrahiert Standardwert aus OpenAPI-Schema |
| `SaveBearerTokenIfPresent(Endpoint, Dictionary<string,string>, int)` | `private` | Speichert Bearer-Token via `ICredentialService.SavePassword` |

---

## `SystemEntryInitializer`
Datei: `src/Schnittstellenzentrale/SystemEntryInitializer.cs`

Statische Klasse. Legt beim Programmstart Systemgruppe und -anwendung an oder aktualisiert URLs.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `InitializeAsync(IServiceProvider, IConfiguration)` | `public static` | Prüft/legt Systemgruppe und -anwendung an, aktualisiert BaseUrl/InterfaceUrl bei Abweichung. Legt **keine** Endpunkte an. |

---

## `ApiControllerBase`
Datei: `src/Schnittstellenzentrale/Controllers/ApiControllerBase.cs`

Abstrakte Basisklasse für alle API-Controller. Attribute: `[AllowAnonymous]`, `[ApiController]`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `ValidateTokenAndSetResponseHeaderAsync()` | `protected` | Prüft Bearer-Token via `ITokenStore.ValidateAndRotateAsync`, schreibt neues Token in `X-New-Token`-Response-Header |
| `ParseStorageMode()` | `protected` | Liest `X-Storage-Mode`-Header, gibt `StorageMode.Team` oder `StorageMode.User` zurück |
| `ParseRequestContextAsync()` | `protected` | Kombiniert Token-Validierung, StorageMode und Owner-Header zu `RequestContext` |
| `MapToResponse(Application)` | `protected static` | Mappt `Application` auf `ApplicationResponse` |
| `MapToResponse(ApplicationGroup)` | `protected static` | Mappt `ApplicationGroup` auf `ApplicationGroupResponse` inkl. verschachtelter Applications |

---

## `ApplicationGroupsController`
Datei: `src/Schnittstellenzentrale/Controllers/ApplicationGroupsController.cs`

Route: `[Route("api/application-groups")]`. Erbt von `ApiControllerBase`. Vollständiges CRUD-Muster mit SignalR-Benachrichtigungen.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `GetAllAsync()` | `public` | GET – liefert alle Gruppen für StorageMode/Owner; `[RequiresContextHeaders(includeOwner: true)]` |
| `GetByIdAsync(int)` | `public` | GET `{id}` – liefert eine Gruppe per ID; 404 wenn nicht gefunden |
| `CreateAsync(CreateApplicationGroupRequest)` | `public` | POST – legt neue Gruppe an; `[RequiresContextHeaders]`; SignalR bei Team-Mode |
| `UpdateAsync(int, UpdateApplicationGroupRequest)` | `public` | PUT `{id}` – aktualisiert Gruppe; 403 bei IsSystem; SignalR bei Team-Mode |
| `DeleteAsync(int)` | `public` | DELETE `{id}` – löscht Gruppe; 403 bei IsSystem; SignalR bei Team-Mode |

---

## `ApplicationsController`
Datei: `src/Schnittstellenzentrale/Controllers/ApplicationsController.cs`

Route: `[Route("api/applications")]`. Erbt von `ApiControllerBase`. Vollständiges CRUD-Muster mit zusätzlichem `ungrouped`-Endpunkt.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `GetAllAsync()` | `public` | GET – alle Anwendungen für StorageMode/Owner |
| `GetUngroupedAsync()` | `public` | GET `ungrouped` – ungrouped Anwendungen |
| `GetByIdAsync(int)` | `public` | GET `{id}` – eine Anwendung per ID |
| `CreateAsync(CreateApplicationRequest)` | `public` | POST – legt Anwendung an; SignalR bei Team-Mode |
| `UpdateAsync(int, UpdateApplicationRequest)` | `public` | PUT `{id}` – aktualisiert; 403 bei IsSystem; SignalR |
| `DeleteAsync(int)` | `public` | DELETE `{id}` – löscht; 403 bei IsSystem; SignalR |
| `ApplyRequestToApplication(Application, UpdateApplicationRequest)` | `private static` | Überträgt Request-Felder auf Domänenobjekt |
