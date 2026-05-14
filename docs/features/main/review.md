# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

### Enums (`Schnittstellenzentrale.Core`)

- [x] `StorageMode` (Enum) — angelegt; Werte `Team` und `User` vorhanden
- [x] `HttpMethod` (Enum) — angelegt; alle 7 Werte (`GET`, `POST`, `PUT`, `DELETE`, `PATCH`, `HEAD`, `OPTIONS`) vorhanden
- [x] `AuthenticationType` (Enum) — angelegt; alle 5 Werte (`None`, `Basic`, `Negotiate`, `BearerToken`, `NegotiateWithImpersonation`) vorhanden

### Datenmodellklassen (`Schnittstellenzentrale.Core`)

- [x] `ApplicationGroup` (Datenmodellklasse) — angelegt
- [x] `Application` (Datenmodellklasse) — angelegt; enthält Name, Beschreibung, Basis-URL, optionale Swagger-URL, optionale Metadata-URL, Zugehörigkeit zu `ApplicationGroup`
- [x] `EndpointGroup` (Datenmodellklasse) — angelegt
- [x] `Endpoint` (Datenmodellklasse) — angelegt; enthält Methode, relativen Pfad, Header, Query-Parameter, Body, Authentifizierungstyp, Zugehörigkeit zu `Application` und optionaler `EndpointGroup`
- [x] `EndpointHeader` (Datenmodellklasse) — angelegt
- [x] `EndpointQueryParameter` (Datenmodellklasse) — angelegt
- [x] `ImportDiff` (Datenmodellklasse) — angelegt; enthält `NewEndpoints`, `ChangedEndpoints`, `RemovedEndpoints`

### Interfaces (`Schnittstellenzentrale.Core`)

- [x] `IApplicationRepository` (Interface) — angelegt; CRUD für `Application` und `ApplicationGroup`, storageMode-bewusst
- [x] `IEndpointRepository` (Interface) — angelegt; CRUD für `Endpoint`, `EndpointGroup`, `EndpointHeader`, `EndpointQueryParameter`
- [x] `IEndpointExecutionService` (Interface) — angelegt
- [x] `ISwaggerImportService` (Interface) — angelegt
- [x] `IODataImportService` (Interface) — angelegt
- [x] `IHealthCheckService` (Interface) — angelegt
- [x] `ICredentialService` (Interface) — angelegt
- [x] `IStorageModeService` (Interface) — angelegt
- [x] `ISignalRNotificationService` (Interface) — angelegt

### EF Core / Infrastruktur (`Schnittstellenzentrale.Infrastructure`)

- [x] `AppDbContext` (Klasse) — angelegt; alle Entitäten konfiguriert, `RowVersion`-Felder als Concurrency-Token gesetzt, Fluent API für Beziehungen vorhanden
- [x] `DatabaseProviderFactory` (Klasse) — angelegt; liest `DatabaseProvider` aus Konfiguration, registriert SQLite oder SQL Server
- [x] EF-Core-Migration SQLite — vorhanden (`Data/Migrations/20260514070438_InitialCreate`)
- [x] EF-Core-Migration SQL Server — vorhanden (`Data/SqlServerMigrations/20260514000000_InitialCreate`)
- [x] `ApplicationRepository` (Klasse) — angelegt; implementiert `IApplicationRepository` mit StorageMode-Logik
- [x] `EndpointRepository` (Klasse) — angelegt; implementiert `IEndpointRepository` vollständig
- [x] `EndpointExecutionService` (Klasse) — angelegt; implementiert `IEndpointExecutionService`; alle Authentifizierungsstrategien je `AuthenticationType` vorhanden; `WindowsIdentity.RunImpersonated` für `NegotiateWithImpersonation` vorhanden
- [x] `SwaggerImportService` (Klasse) — angelegt; nutzt `Microsoft.OpenApi`; liefert `ImportDiff`
- [x] `ODataImportService` (Klasse) — angelegt; parst `$metadata` via `Microsoft.OData.Edm`; liefert `ImportDiff`
- [x] `HealthCheckService` (Klasse) — angelegt; In-Memory-Cooldown-Tracking per Anwendungs-ID; `HealthCheck:CooldownSeconds` konfigurierbar
- [x] `WindowsCredentialService` (Klasse) — angelegt; implementiert `ICredentialService` via Windows Credential Manager (native DPAPI)
- [x] `StorageModeService` (Klasse) — angelegt; Scoped DI; implementiert `IStorageModeService`
- [x] `SignalRNotificationService` (Klasse) — angelegt; generische Implementierung über `IHubContext<THub>`; implementiert `ISignalRNotificationService`

### SignalR (`Schnittstellenzentrale`)

- [x] `EndpointHub` (Klasse) — angelegt; Gruppen-Abonnement-Mechanismus für Anwendungs-ID und Gruppen-ID vorhanden

### Anwendungskonfiguration (`Schnittstellenzentrale`)

- [x] `appsettings.json` — angelegt; enthält `DatabaseProvider`, `ConnectionStrings:Default`, `Serilog`, `HealthCheck:CooldownSeconds`
- [x] `Program.cs` — angelegt; alle DI-Registrierungen vorhanden; Serilog registriert; Windows-Authentifizierung (Negotiate) konfiguriert; SignalR, HttpClient und Hub-Mapping vorhanden

### UI-Komponenten (`Schnittstellenzentrale`)

- [x] `MainLayout` (Blazor-Komponente) — angelegt; `StorageMode`-Umschalter vorhanden
- [x] `ApplicationGroupTree` (Blazor-Komponente) — angelegt; Baumansicht mit `CollapsibleSection`, standardmäßig zugeklappt
- [x] `ApplicationCard` (Blazor-Komponente) — angelegt; Aktionen für Swagger-Import, OData-Import und Health-Check vorhanden
- [x] `EndpointList` (Blazor-Komponente) — angelegt; Endpunkte in `EndpointGroup`s gruppiert, standardmäßig zugeklappt
- [x] `EndpointEditor` (Blazor-Komponente) — angelegt; Formular mit Methode, Pfad, Header, Query-Parametern, Body, Auth; `ConcurrencyWarningDialog` bei Schreibkonflikt
- [x] `EndpointExecutionPanel` (Blazor-Komponente) — angelegt; Ausführung mit Request/Response-Anzeige; Health-Check-Aufruf bei Verbindungsfehler vorhanden
- [x] `SwaggerImportDialog` (Blazor-Komponente) — angelegt; zeigt `ImportDiff`-Vorschau
- [x] `ODataImportDialog` (Blazor-Komponente) — angelegt; analog zu `SwaggerImportDialog`
- [x] `HealthCheckDialog` (Blazor-Komponente) — angelegt; zeigt Health-Check-Ergebnis; Option zum Entfernen der Anwendung vorhanden
- [x] `ConcurrencyWarningDialog` (Blazor-Komponente) — angelegt; Warnung bei Schreibkonflikt mit Force-Save und Abbrechen

### Tests (`Schnittstellenzentrale.Tests`)

- [x] `Execute_WithAuthTypeNone_SendsRequestWithoutCredentials` in `EndpointExecutionServiceTests` — vorhanden
- [x] `Execute_WithAuthTypeBasic_SendsBasicAuthHeader` in `EndpointExecutionServiceTests` — vorhanden
- [x] `Execute_WithAuthTypeNegotiate_UsesNegotiateHandler` in `EndpointExecutionServiceTests` — vorhanden
- [x] `Execute_WithAuthTypeBearerToken_SendsBearerHeader` in `EndpointExecutionServiceTests` — vorhanden
- [x] `Execute_WithAuthTypeNegotiateWithImpersonation_RunsImpersonated` in `EndpointExecutionServiceTests` — vorhanden
- [x] `Execute_OnConnectionError_DoesNotCallHealthCheck` in `EndpointExecutionServiceTests` — vorhanden; verifiziert, dass `IHealthCheckService.CheckAsync` bei Verbindungsfehler nicht aufgerufen wird (Verantwortung liegt in der UI)
- [x] `Import_NewSwaggerDefinition_ReturnsCorrectDiff` in `SwaggerImportServiceTests` — vorhanden
- [x] `Import_ChangedSwaggerOperation_ReturnsChangedInDiff` in `SwaggerImportServiceTests` — vorhanden
- [x] `Import_RemovedSwaggerOperation_ReturnsRemovedInDiff` in `SwaggerImportServiceTests` — vorhanden
- [x] `Import_NewODataMetadata_ReturnsCorrectDiff` in `ODataImportServiceTests` — vorhanden
- [x] `Import_ChangedODataMetadata_ReturnsChangedInDiff` in `ODataImportServiceTests` — vorhanden
- [x] `CheckAsync_WithinCooldown_DoesNotSendRequest` in `HealthCheckServiceTests` — vorhanden
- [x] `CheckAsync_AfterCooldownExpired_SendsRequest` in `HealthCheckServiceTests` — vorhanden
- [x] `CheckAsync_UnreachableUrl_ReturnsFalse` in `HealthCheckServiceTests` — vorhanden
- [x] `CreateSqliteContext_ReturnsSqliteDbContext` in `DatabaseProviderFactoryTests` — vorhanden
- [x] `CreateSqlServerContext_ReturnsSqlServerDbContext` in `DatabaseProviderFactoryTests` — vorhanden
- [x] `GetApplications_WithStorageModeUser_ReturnsOnlyUserData` in `ApplicationRepositoryIntegrationTests` — vorhanden
- [x] `GetApplications_WithStorageModeTeam_ReturnsTeamData` in `ApplicationRepositoryIntegrationTests` — vorhanden
- [x] `SaveEndpoint_ConcurrentWrite_DetectsConflict` in `EndpointRepositoryIntegrationTests` — vorhanden
- [x] `CreateInMemoryDbContext` in `TestHelpers` — vorhanden

## Offene Aufgaben

Keine.

## Hinweise

- Die Verantwortung für den Health-Check-Aufruf bei Verbindungsfehlern liegt in der UI-Komponente `EndpointExecutionPanel`, nicht im `EndpointExecutionService`. Der Plan beschreibt dieses Verhalten explizit im Testfall `Execute_OnConnectionError_DoesNotCallHealthCheck` so: „Verantwortung liegt in der UI". Die Implementierung ist konsistent mit dem Plan.
- `SwaggerImportDialog` und `ODataImportDialog` delegieren ihre Darstellungslogik an eine gemeinsame `ImportDialog`-Komponente. Die Planspezifikation beschreibt beide als eigenständige Komponenten; die Umsetzung als Thin-Wrapper erfüllt die geforderte Funktionalität vollständig.
- `SignalRNotificationService` ist als generische Klasse `SignalRNotificationService<THub>` implementiert und wird in `Program.cs` explizit als `SignalRNotificationService<EndpointHub>` registriert. Das entspricht der Planabsicht.
- `DatabaseProviderFactory` ist als `static class` implementiert; die Planspezifikation macht dazu keine Einschränkung.
- Die SQL-Server-Migration liegt in einem separaten Verzeichnis (`SqlServerMigrations`). Für produktiven SQL-Server-Betrieb ist sicherzustellen, dass `DatabaseProviderFactory` die korrekte Migrationsbaugruppe konfiguriert.
