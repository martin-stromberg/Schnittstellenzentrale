# Tests

## Testklassen

### `ApplicationApiClientTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/ApplicationApiClientTests.cs`

Unit-Tests mit gemocktem `HttpMessageHandler` (Moq). Muster: `CreateClient(...)` liefert einen konfigurierten `ApplicationApiClient` mit Mock-Abhängigkeiten.

- `AddGroupAsync_IssuesTokenViaTokenStoreAndSendsCorrectRequest_ReturnsResponse` — Prüft POST `/api/application-groups`, Token-Ausstellung, X-Storage-Mode-Header
- `AddGroupAsync_TokenIssuedOnlyOnceForMultipleCalls` — Prüft Token-Caching bei mehrfachen Aufrufen
- `AddApplicationAsync_IssuesTokenAndSendsCorrectRequest_ReturnsResponse` — Prüft POST `/api/applications`, Mapping aller Felder
- `GetGroupsAsync_SendsCorrectHeadersAndReturnsMappedList` — Prüft GET mit X-Storage-Mode und X-Owner, Mapping der Liste
- `GetUngroupedApplicationsAsync_SendsCorrectHeadersAndReturnsMappedList` — Prüft GET `/api/applications/ungrouped`
- `GetApplicationByIdAsync_ReturnsNullOn404` — Prüft null-Rückgabe bei 404
- `UpdateGroupAsync_SendsCorrectPutRequestAndReturnsMappedGroup` — Prüft PUT `/api/application-groups/{id}`
- `DeleteGroupAsync_SendsCorrectDeleteRequest` — Prüft DELETE `/api/application-groups/{id}`
- `UpdateApplicationAsync_SendsCorrectPutRequestAndReturnsMappedApplication` — Prüft PUT `/api/applications/{id}`, alle Felder
- `DeleteApplicationAsync_SendsCorrectDeleteRequest` — Prüft DELETE `/api/applications/{id}`

Fehlend: Alle Tests für `EndpointGroup`- und `Endpoint`-Methoden (GET by applicationId, GET by id, POST, PUT, DELETE jeweils für beide Ressourcentypen).

---

### `SystemEndpointSyncServiceTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/SystemEndpointSyncServiceTests.cs`

Unit-Tests mit gemocktem `IEndpointRepository`, `IApplicationRepository`, `ISwaggerProvider`, `ICredentialService`. Hilfsklasse `TestableSyncService` macht `ExecuteAsync` öffentlich zugänglich.

- `ExecuteAsync_NewEndpoints_AreAdded` — Prüft, dass neue Endpunkte aus Swagger angelegt werden
- `ExecuteAsync_RemovedEndpoints_AreDeleted` — Prüft, dass nicht mehr in Swagger enthaltene Endpunkte gelöscht werden
- `ExecuteAsync_ExistingEndpoints_AreLeftUntouched` — Prüft Idempotenz bei unveränderter Swagger-Definition
- `ExecuteAsync_WhenSwaggerProviderThrows_LogsErrorAndDoesNotThrow` — Prüft Fehlerbehandlung bei SwaggerProvider-Fehler
- `ExecuteAsync_WhenDbThrows_DoesNotThrow` — Prüft Fehlerbehandlung bei DB-Fehler
- `ExecuteAsync_IsIdempotent_OnRepeatedCall` — Prüft wiederholten Aufruf
- `ExecuteAsync_WhenSystemGroupMissing_LogsWarningAndSkips` — Prüft Verhalten ohne Systemgruppe
- `ExecuteAsync_WhenSystemAppMissing_LogsWarningAndSkips` — Prüft Verhalten ohne Systemanwendung
- `ExecuteAsync_NewEndpoint_GroupsAreCreatedFromUrlSegments` — Prüft hierarchische Gruppenanlage aus URL-Segmenten
- `ExecuteAsync_PathParameterSegments_AreSkipped` — Prüft, dass `{id}`-Segmente übersprungen werden
- `ExecuteAsync_ExistingGroups_AreReusedAndNotCreatedAgain` — Prüft Wiederverwendung bestehender Gruppen
- `ExecuteAsync_WithNegotiateSecurityScheme_SetsNegotiateAuthenticationType` — Prüft Negotiate-Auth-Erkennung
- `ExecuteAsync_WithoutNegotiateSecurityScheme_SetsNoneAuthenticationType` — Prüft None-Auth als Standard
- `ExecuteAsync_WithBearerTokenExtension_SavesBearerToken` — Prüft `ICredentialService.SavePassword` bei `x-sz-bearer-token`-Extension

Fehlend laut Anforderung: Tests für `x-sz-pre-request-script`- und `x-sz-post-request-script`-Extensions (Prüfung, ob Skripte im gespeicherten Endpunkt vorhanden sind).

---

### `SystemEntryInitializerTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/SystemEntryInitializerTests.cs`

Integrationstests mit In-Memory-SQLite-Datenbank (`TestHelpers.CreateInMemoryDbContext`). Prüft ausschließlich Gruppe- und Anwendungsanlage.

- `InitializeAsync_WhenGroupAndApplicationMissing_CreatesBoth` — Prüft vollständige Neuanlage von Gruppe und Anwendung
- `InitializeAsync_WhenGroupExistsButApplicationMissing_CreatesApplication` — Prüft Anlage der Anwendung bei vorhandener Gruppe
- `InitializeAsync_WhenUrlDiffers_UpdatesBaseUrlAndInterfaceUrl` — Prüft URL-Update
- `InitializeAsync_WhenUrlMatches_MakesNoChanges` — Prüft Idempotenz
- `InitializeAsync_IsIdempotent_OnRepeatedCall` — Prüft Idempotenz bei Dreifachaufruf
- `InitializeAsync_WhenDbThrows_DoesNotPropagateException` — Prüft Fehlerbehandlung via `ThrowingApplicationRepository`
- `InitializeAsync_WhenBaseUrlMissing_SkipsAndLogs` — Prüft Verhalten ohne `Api:BaseUrl`-Konfiguration

Fehlend: Kein Testfall prüft, ob nach `InitializeAsync` + `SystemEndpointSyncService.ExecuteAsync` Endpunkte vorhanden sind, welche Autorisierungstypen gesetzt sind, oder ob `ICredentialService` aufgerufen wurde.

---

### `ApplicationsControllerIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ApplicationsControllerIntegrationTests.cs`

Integrationstests mit `ControllerTestFactory` (WebApplicationFactory + SQLite In-Memory). Deckt vollständiges CRUD für `/api/applications` ab, inkl. 401/404/400/403.

Vorhanden als Muster für die noch fehlende `ApplicationApiClientIntegrationTests`-Klasse.

---

### `ApplicationGroupsControllerIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ApplicationGroupsControllerIntegrationTests.cs`

Analog zu `ApplicationsControllerIntegrationTests` für `/api/application-groups`.

Vorhanden als Muster für die noch fehlenden `EndpointGroupsController`- und `EndpointsController`-Integrationstests.

---

## Hilfsmethoden

### `ControllerTestFactory`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/ControllerTestFactory.cs`

- `ControllerTestFactory()` — Öffnet SQLite In-Memory-Verbindung
- `ConfigureWebHost(IWebHostBuilder)` — Ersetzt Auth, DbContext, ApplicationRepository; entfernt **alle `IHostedService`-Registrierungen** (inkl. `SystemEndpointSyncService`)
- `CreateHost(IHostBuilder)` — Führt `EnsureCreated()` für die DB aus
- `ObtainTokenAsync(HttpClient)` — POST `/authenticate`, gibt Token zurück

Hinweis: `ControllerTestFactory` entfernt `IHostedService`-Registrierungen. Für kombinierte Integrationstests (SystemEntryInitializer + SystemEndpointSyncService) wäre eine separate Factory nötig.

### `TestHelpers`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs`

Wird von `SystemEntryInitializerTests` für die In-Memory-Datenbank-Erstellung verwendet (`CreateInMemoryDbContext()`).
