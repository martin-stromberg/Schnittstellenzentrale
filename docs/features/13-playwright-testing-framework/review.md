# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

- [x] `PlaywrightTestFactory` (Klasse, erbt von `WebApplicationFactory<Program>`) — angelegt
- [x] Feld `BaseAddress` in `PlaywrightTestFactory` — vorhanden (public Property)
- [x] Methode `ConfigureWebHost` in `PlaywrightTestFactory` — vorhanden (`UseEnvironment("Testing")`, `UseUrls`, `ConfigureAppConfiguration`, `ConfigureTestServices`)
- [x] Methode `ConfigureTestServices` (virtual) in `PlaywrightTestFactory` — vorhanden (Auth-Bypass, Datei-SQLite, `IHostedService`-Entfernung, `ISignalRNotificationService`-Mock, `ICurrentUserService`-Mock)
- [x] Methode `CreateHost` in `PlaywrightTestFactory` — vorhanden (`EnsureCreated`, `SystemEntryInitializer.InitializeAsync`, `Api:BaseUrl`-Override)
- [x] `PlaywrightSignalRFactory` (Klasse, erbt von `PlaywrightTestFactory`) — angelegt
- [x] Methode `ConfigureTestServices` (override) in `PlaywrightSignalRFactory` — vorhanden (entfernt Mock, registriert echten `SignalRNotificationService<EndpointHub>`)
- [x] `TestDatabaseSeeder` (Klasse) — angelegt
- [x] Methode `ResetAsync` in `TestDatabaseSeeder` — vorhanden (`EnsureDeleted`, `EnsureCreated`, `SystemEntryInitializer.InitializeAsync`)
- [x] `PlaywrightCollection` (xUnit Collection-Definition) — angelegt (`[CollectionDefinition("Playwright")]`, `ICollectionFixture<PlaywrightTestFactory>`)
- [x] `PlaywrightTestBase` (abstrakte Klasse, implementiert `IAsyncLifetime`) — angelegt
- [x] Methode `InitializeAsync` in `PlaywrightTestBase` — vorhanden (`TestDatabaseSeeder.ResetAsync`, Playwright-Init, Chromium headless, `IBrowserContext`, Tracing-Start mit Screenshots/Snapshots/Sources)
- [x] Methode `DisposeAsync` in `PlaywrightTestBase` — vorhanden (Tracing-Stop mit Pfad, Context/Browser/Playwright-Dispose)
- [x] Property `Context` in `PlaywrightTestBase` — vorhanden
- [x] Property `Page` in `PlaywrightTestBase` — vorhanden
- [x] Property `BaseUrl` in `PlaywrightTestBase` — vorhanden
- [x] `HomePageTests` (Testklasse) — angelegt
- [x] Test `StartPage_ShowsSystemGroup` in `HomePageTests` — vorhanden
- [x] Test `StartPage_ShowsOwnApiEndpoints` in `HomePageTests` — vorhanden
- [x] `ApplicationCrudTests` (Testklasse) — angelegt
- [x] Test `CreateApplication_AppearsInTree` in `ApplicationCrudTests` — vorhanden
- [x] Test `EditApplication_UpdatesNameInTree` in `ApplicationCrudTests` — vorhanden
- [x] Test `DeleteApplication_DisappearsFromTree` in `ApplicationCrudTests` — vorhanden
- [x] `EndpointExecutionTests` (Testklasse) — angelegt
- [x] Test `ExecuteEndpoint_ReturnsSuccessResponse` in `EndpointExecutionTests` — vorhanden
- [x] `SwaggerImportTests` (Testklasse) — angelegt
- [x] Test `ImportSwagger_ImportsEndpointsIntoTree` in `SwaggerImportTests` — vorhanden
- [x] `HealthCheckTests` (Testklasse) — angelegt
- [x] Test `HealthCheck_ShowsReachableStatus` in `HealthCheckTests` — vorhanden
- [x] `StorageModeTests` (Testklasse) — angelegt
- [x] Test `SwitchToTeamMode_ShowsTeamData` in `StorageModeTests` — vorhanden
- [x] Test `SwitchBackToUserMode_ShowsUserData` in `StorageModeTests` — vorhanden
- [x] `SignalRSyncTests` (Testklasse) — angelegt
- [x] Test `BrowserA_CreatesApp_BrowserB_ReceivesViaSignalR` in `SignalRSyncTests` — vorhanden (zwei `IBrowserContext`-Instanzen, kein `Page.ReloadAsync`)
- [x] `PackageReference Microsoft.Playwright` in `Schnittstellenzentrale.Tests.csproj` — vorhanden (v1.52.0)
- [x] MSBuild-Target `InstallPlaywright` in `Schnittstellenzentrale.Tests.csproj` — vorhanden (`AfterTargets="Build"`, `playwright.ps1 install chromium`)
- [x] Konfigurationsoverride `ConnectionStrings:Default` in `PlaywrightTestFactory` — vorhanden
- [x] Konfigurationsoverride `DatabaseProvider` in `PlaywrightTestFactory` — vorhanden
- [x] Konfigurationsoverride `Api:BaseUrl` in `PlaywrightTestFactory.CreateHost` — vorhanden

## Offene Aufgaben

## Hinweise

- `PlaywrightTestBase.DisposeAsync` speichert den Trace **immer** (unabhängig vom Testergebnis), anstatt ihn wie im Hauptablauf beschrieben nur bei Fehler zu speichern. Dies entspricht der Empfehlung aus Offenem Punkt #8 des Plans und ist eine bewusste, im Plan begründete Abweichung.
- Die Implementierung enthält eine zusätzliche, im Plan nicht explizit gelistete Klasse `PlaywrightSignalRCollection` (`[CollectionDefinition("PlaywrightSignalR")]`), die `SignalRSyncTests` mit `PlaywrightSignalRFactory` als Fixture verbindet. Diese Klasse ist für die korrekte Funktion der SignalR-Tests notwendig und stellt keine Planabweichung dar.
- `Api:BaseUrl` wird in `CreateHost` durch direkte Zuweisung (`configuration["Api:BaseUrl"] = BaseAddress`) gesetzt, nicht über eine `ConfigureAppConfiguration`-Callback. Das Ergebnis ist funktional identisch zum Plan.
