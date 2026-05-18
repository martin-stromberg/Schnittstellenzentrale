# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

### Neue Klassen und Interfaces

- [x] `AuthToken` (Datenklasse, `Schnittstellenzentrale.Core/Models`) — angelegt; enthält `TokenValue` (GUID-String), `ExpiresAt` (DateTime), `WindowsUsername`
- [x] `ITokenStore` (Interface, `Schnittstellenzentrale.Core/Interfaces`) — angelegt; definiert `CreateTokenAsync(username)` und `ValidateAndRotateAsync(tokenString)`
- [x] `TokenStore` (Singleton-Klasse, `Schnittstellenzentrale/Services`) — angelegt; implementiert `ITokenStore` mit `ConcurrentDictionary`, bereinigt abgelaufene Token bei Zugriff
- [x] `AuthenticateResponse` (DTO, `Schnittstellenzentrale.Core/Contracts`) — angelegt; enthält `Token` als String
- [x] `CreateApplicationGroupRequest` (DTO, `Schnittstellenzentrale.Core/Contracts`) — angelegt; enthält `Name` mit `[Required]` und `[MaxLength(200)]`
- [x] `UpdateApplicationGroupRequest` (DTO, `Schnittstellenzentrale.Core/Contracts`) — angelegt; enthält `Name` mit `[Required]` und `[MaxLength(200)]`
- [x] `ApplicationGroupResponse` (DTO, `Schnittstellenzentrale.Core/Contracts`) — angelegt; enthält `Id`, `Name` und `IList<ApplicationResponse> Applications`
- [x] `CreateApplicationRequest` (DTO, `Schnittstellenzentrale.Core/Contracts`) — angelegt; enthält `Name` (`[Required]`, `[MaxLength(200)]`), `BaseUrl` (`[Required]`, `[MaxLength(500)]`), `Description`, `InterfaceUrl` (`[MaxLength(500)]`), `ApplicationGroupId`, `Owner` (`[MaxLength(256)]`)
- [x] `UpdateApplicationRequest` (DTO, `Schnittstellenzentrale.Core/Contracts`) — angelegt; enthält `Name` (`[Required]`, `[MaxLength(200)]`), `BaseUrl` (`[Required]`, `[MaxLength(500)]`), `Description`, `InterfaceUrl` (`[MaxLength(500)]`), `ApplicationGroupId`, `Owner` (`[MaxLength(256)]`)
- [x] `ApplicationResponse` (DTO, `Schnittstellenzentrale.Core/Contracts`) — angelegt; enthält alle geplanten Felder: `Id`, `Name`, `BaseUrl`, `ApplicationGroupId`, `Description`, `InterfaceUrl`, `InterfaceType` (int), `Owner`
- [x] `IApplicationApiClient` (Interface, `Schnittstellenzentrale.Core/Interfaces`) — vollständig neu gestaltet; spiegelt `IApplicationRepository` mit allen zehn domänenmodell-basierten Methoden
- [x] `AuthController` (ASP.NET Core Controller, `Schnittstellenzentrale/Controllers`) — angelegt; exponiert `POST /authenticate`, liest Windows-Identität aus `HttpContext.User.Identity.Name`, ruft `ITokenStore.CreateTokenAsync` auf, gibt `AuthenticateResponse` zurück
- [x] `ApplicationGroupsController` (ASP.NET Core Controller, `Schnittstellenzentrale/Controllers`) — vollständig implementiert; exponiert `GET`, `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}` unter `/api/application-groups`; validiert Token, delegiert an `IApplicationRepository`, setzt `X-New-Token`-Header, ruft `ISignalRNotificationService` bei `StorageMode.Team`
- [x] `ApplicationsController` (ASP.NET Core Controller, `Schnittstellenzentrale/Controllers`) — vollständig implementiert; exponiert `GET`, `GET /ungrouped`, `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}` unter `/api/applications`; validiert Token, delegiert an `IApplicationRepository`, setzt `X-New-Token`-Header, ruft `ISignalRNotificationService` bei `StorageMode.Team`
- [x] `ApplicationApiClient` (HTTP-Client, `Schnittstellenzentrale/Services`) — vollständig implementiert; injiziert `IStorageModeService` und `ICurrentUserService`; implementiert alle zehn Methoden; internes Mapping zwischen Domänenobjekten und DTOs; Token-Rotation; Retry-Logik bei 401

### Felder in DTOs und Datenklassen

- [x] Felder `TokenValue`, `ExpiresAt`, `WindowsUsername` in `AuthToken` — vorhanden
- [x] Feld `Token` in `AuthenticateResponse` — vorhanden
- [x] Feld `Name` (`[Required]`, `[MaxLength(200)]`) in `CreateApplicationGroupRequest` — vorhanden
- [x] Feld `Name` (`[Required]`, `[MaxLength(200)]`) in `UpdateApplicationGroupRequest` — vorhanden
- [x] Felder `Id`, `Name`, `IList<ApplicationResponse> Applications` in `ApplicationGroupResponse` — vorhanden
- [x] Felder `Name`, `BaseUrl`, `Description`, `InterfaceUrl`, `ApplicationGroupId`, `Owner` mit Validierungsattributen in `CreateApplicationRequest` — vorhanden
- [x] Felder `Name`, `BaseUrl`, `Description`, `InterfaceUrl`, `ApplicationGroupId`, `Owner` mit Validierungsattributen in `UpdateApplicationRequest` — vorhanden
- [x] Felder `Id`, `Name`, `BaseUrl`, `ApplicationGroupId`, `Description`, `InterfaceUrl`, `InterfaceType`, `Owner` in `ApplicationResponse` — vorhanden

### Methoden in Controllern

- [x] `GetAllAsync` in `ApplicationGroupsController` — vorhanden; liest `X-Storage-Mode` und `X-Owner`, ruft `IApplicationRepository.GetGroupsAsync`, mappt auf `IList<ApplicationGroupResponse>` inkl. `Applications`, gibt `200 OK` mit `X-New-Token` zurück
- [x] `GetByIdAsync(int id)` in `ApplicationGroupsController` — vorhanden; gibt `404` bei fehlendem Datensatz, `200` mit `ApplicationGroupResponse` inkl. `Applications` sonst
- [x] `CreateAsync` in `ApplicationGroupsController` — vorhanden
- [x] `UpdateAsync(int id)` in `ApplicationGroupsController` — vorhanden; gibt `404` bei fehlendem Datensatz, überschreibt `Name`, ruft `UpdateGroupAsync`, `200 OK` mit geänderter `ApplicationGroupResponse`
- [x] `DeleteAsync(int id)` in `ApplicationGroupsController` — vorhanden; gibt `404` bei fehlendem Datensatz, ruft `DeleteGroupAsync`, `204 No Content`
- [x] `GetAllAsync` in `ApplicationsController` — vorhanden; liest `X-Storage-Mode` und `X-Owner`, gibt `200 OK` mit `IList<ApplicationResponse>`
- [x] `GetUngroupedAsync` in `ApplicationsController` — vorhanden; ruft `GetUngroupedApplicationsAsync`, gibt `200 OK`
- [x] `GetByIdAsync(int id)` in `ApplicationsController` — vorhanden; gibt `404` oder `200` mit vollständiger `ApplicationResponse`
- [x] `CreateAsync` in `ApplicationsController` — vorhanden
- [x] `UpdateAsync(int id)` in `ApplicationsController` — vorhanden; überschreibt alle änderbaren Felder inkl. `InterfaceType` via `Application.DetectInterfaceType`
- [x] `DeleteAsync(int id)` in `ApplicationsController` — vorhanden

### Methoden in IApplicationApiClient und ApplicationApiClient

- [x] `GetGroupsAsync(StorageMode, string)` — vorhanden; setzt `X-Storage-Mode`- und `X-Owner`-Header, mappt Response-Liste auf `IList<ApplicationGroup>`
- [x] `GetGroupByIdAsync(int)` — vorhanden; gibt `null` bei 404
- [x] `AddGroupAsync(ApplicationGroup)` — vorhanden; liest `StorageMode` intern aus `IStorageModeService`
- [x] `UpdateGroupAsync(ApplicationGroup)` — vorhanden; sendet `PUT` mit `UpdateApplicationGroupRequest`-Body
- [x] `DeleteGroupAsync(int)` — vorhanden; sendet `DELETE`, wirft keine Ausnahme bei 204
- [x] `GetUngroupedApplicationsAsync(StorageMode, string)` — vorhanden
- [x] `GetApplicationByIdAsync(int)` — vorhanden; gibt `null` bei 404
- [x] `AddApplicationAsync(Application)` — vorhanden
- [x] `UpdateApplicationAsync(Application)` — vorhanden; sendet `PUT` mit `UpdateApplicationRequest`-Body, mappt alle Felder
- [x] `DeleteApplicationAsync(int)` — vorhanden

### Änderungen an bestehenden Klassen

- [x] `ApplicationGroupEditor` — injiziert `IApplicationApiClient`; `SaveAsync` ruft `IApplicationApiClient.AddGroupAsync(new ApplicationGroup { Name = _model.Name })`; kein `IApplicationRepository`; kein `ISignalRNotificationService`; kein `IStorageModeService`
- [x] `ApplicationEditor` — injiziert `IApplicationApiClient`, `IStorageModeService`, `ICurrentUserService`; kein `IApplicationRepository`; kein `ISignalRNotificationService`; `OnInitializedAsync` ruft `IApplicationApiClient.GetGroupsAsync`; `SaveAsync` (Anlage) ruft `IApplicationApiClient.AddApplicationAsync`; `SaveAsync` (Bearbeitung) ruft `IApplicationApiClient.UpdateApplicationAsync`
- [x] `ApplicationGroupTree` — injiziert `IApplicationApiClient`, `IStorageModeService`, `ICurrentUserService`; kein `IApplicationRepository`; kein `ISignalRNotificationService`; `LoadDataAsync` ruft `GetGroupsAsync` und `GetUngroupedApplicationsAsync`; `OnRemoveFromGroupRequested` ruft `UpdateApplicationAsync`; `OnDrop` ruft `UpdateApplicationAsync`
- [x] `ApplicationCard` — injiziert `IApplicationApiClient`; kein `IApplicationRepository`; `OnParametersSetAsync` ruft `IApplicationApiClient.GetApplicationByIdAsync`; `RemoveApplication` ruft `IApplicationApiClient.DeleteApplicationAsync`
- [x] `Home` — injiziert `IApplicationApiClient`; kein `IApplicationRepository`; kein `ISignalRNotificationService`; `OnGroupRenamed` ruft `IApplicationApiClient.UpdateGroupAsync`; `OnDeleteGroupConfirmedAll` ruft `DeleteApplicationAsync` je Anwendung, dann `DeleteGroupAsync`; `OnDeleteGroupConfirmedGroupOnly` ruft `UpdateApplicationAsync` je Anwendung, dann `DeleteGroupAsync`; `OnDeleteApplicationConfirmed` ruft `DeleteApplicationAsync`
- [x] `Program.cs` — `AddControllers()`, `MapControllers()`, `AddSingleton<ITokenStore, TokenStore>()`, `AddHttpClient<IApplicationApiClient, ApplicationApiClient>` mit `BaseAddress` aus `Api:BaseUrl` vorhanden; `IStorageModeService` und `ICurrentUserService` als Scoped/Singleton registriert und werden korrekt in `ApplicationApiClient` bereitgestellt
- [x] `appsettings.json` — Konfigurationsabschnitt `Api:BaseUrl` vorhanden

### Tests

- [x] `AuthControllerIntegrationTests` — Testklasse vorhanden
- [x] `Authenticate_WithValidWindowsIdentity_Returns200WithToken` — vorhanden
- [x] `Authenticate_CreatesTokenInTokenStore` — vorhanden
- [x] `ApplicationGroupsControllerIntegrationTests` — Testklasse vorhanden
- [x] `PostApplicationGroup_WithValidTokenAndRequest_Returns201AndLocation` — vorhanden
- [x] `PostApplicationGroup_WithoutToken_Returns401` — vorhanden
- [x] `PostApplicationGroup_WithExpiredToken_Returns401` — vorhanden
- [x] `PostApplicationGroup_WithMissingName_Returns400` — vorhanden
- [x] `PostApplicationGroup_RotatesToken_OldTokenIsInvalid` — vorhanden
- [x] `GetApplicationGroups_WithValidToken_Returns200WithList` — vorhanden; prüft auch `g.Applications != null`
- [x] `GetApplicationGroups_WithoutToken_Returns401` — vorhanden
- [x] `GetApplicationGroupById_WithValidId_Returns200` — vorhanden
- [x] `GetApplicationGroupById_WithInvalidId_Returns404` — vorhanden
- [x] `PutApplicationGroup_WithValidRequest_Returns200AndRotatesToken` — vorhanden
- [x] `PutApplicationGroup_WithInvalidId_Returns404` — vorhanden
- [x] `PutApplicationGroup_WithMissingName_Returns400` — vorhanden
- [x] `DeleteApplicationGroup_WithValidId_Returns204AndRotatesToken` — vorhanden
- [x] `DeleteApplicationGroup_WithInvalidId_Returns404` — vorhanden
- [x] `ApplicationsControllerIntegrationTests` — Testklasse vorhanden
- [x] `PostApplication_WithValidTokenAndRequest_Returns201AndLocation` — vorhanden
- [x] `PostApplication_WithoutToken_Returns401` — vorhanden
- [x] `PostApplication_WithMissingName_Returns400` — vorhanden
- [x] `PostApplication_WithMissingBaseUrl_Returns400` — vorhanden
- [x] `GetApplications_WithValidToken_Returns200WithList` — vorhanden
- [x] `GetApplications_WithoutToken_Returns401` — vorhanden
- [x] `GetUngroupedApplications_WithValidToken_Returns200WithList` — vorhanden
- [x] `GetApplicationById_WithValidId_Returns200WithAllFields` — vorhanden; prüft `Description`, `InterfaceUrl`, `InterfaceType`, `Owner`
- [x] `GetApplicationById_WithInvalidId_Returns404` — vorhanden
- [x] `PutApplication_WithValidRequest_Returns200AndRotatesToken` — vorhanden
- [x] `PutApplication_WithInvalidId_Returns404` — vorhanden
- [x] `PutApplication_WithMissingBaseUrl_Returns400` — vorhanden
- [x] `DeleteApplication_WithValidId_Returns204AndRotatesToken` — vorhanden
- [x] `DeleteApplication_WithInvalidId_Returns404` — vorhanden
- [x] `TokenStoreTests` — Testklasse vorhanden
- [x] `CreateTokenAsync_ReturnsValidToken` — vorhanden
- [x] `ValidateAndRotateAsync_WithValidToken_ReturnsNewToken` — vorhanden
- [x] `ValidateAndRotateAsync_WithExpiredToken_ReturnsNull` — vorhanden
- [x] `ValidateAndRotateAsync_WithUnknownToken_ReturnsNull` — vorhanden
- [x] `ApplicationApiClientTests` — Testklasse vorhanden
- [x] `AddGroupAsync_AuthenticatesAndSendsCorrectRequest_ReturnsResponse` — vorhanden
- [x] `AddGroupAsync_RotatesTokenAfterSuccessfulCall` — vorhanden
- [x] `AddApplicationAsync_AuthenticatesAndSendsCorrectRequest_ReturnsResponse` — vorhanden
- [x] `GetGroupsAsync_SendsCorrectHeadersAndReturnsMappedList` — vorhanden
- [x] `GetUngroupedApplicationsAsync_SendsCorrectHeadersAndReturnsMappedList` — vorhanden
- [x] `GetApplicationByIdAsync_ReturnsNullOn404` — vorhanden
- [x] `UpdateGroupAsync_SendsCorrectPutRequestAndReturnsMappedGroup` — vorhanden
- [x] `DeleteGroupAsync_SendsCorrectDeleteRequest` — vorhanden
- [x] `UpdateApplicationAsync_SendsCorrectPutRequestAndReturnsMappedApplication` — vorhanden
- [x] `DeleteApplicationAsync_SendsCorrectDeleteRequest` — vorhanden

## Offene Aufgaben

Keine.

## Hinweise

- `ApplicationApiClient` implementiert zusätzlich zur im Plan beschriebenen Basis-Logik eine Retry-Logik bei HTTP 401: Der Client löscht den gespeicherten Token, ruft `/authenticate` erneut auf und wiederholt den Datenaufruf. Dies entspricht dem im Plan unter „Seiteneffekte und Risiken" erwähnten empfehlenswerten Verhalten.
- `ICurrentUserService` ist als Singleton registriert (`WindowsCurrentUserService`), `IStorageModeService` als Scoped. Da `ApplicationApiClient` als typisierter HTTP-Client registriert ist (Lebenszyklus wird von `IHttpClientFactory` verwaltet), ist die Injektion beider Dienste ohne Captive-Dependency-Problem korrekt.
- Keine Blazor-Komponente injiziert mehr `IApplicationRepository` oder `ISignalRNotificationService` direkt.
- Die Testinfrastruktur `ControllerTestFactory` und `TestAuthHandler` sind als notwendige Voraussetzung für die Controller-Integrationstests angelegt, obwohl der Plan sie nicht als explizite Planelemente listet.
