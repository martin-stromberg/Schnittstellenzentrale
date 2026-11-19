# Playwright-Infrastruktur

Basis für die geforderte Screenshot-Erzeugung. Alle Dateien unter `src/Schnittstellenzentrale.Tests/Playwright/` (Infrastructure) bzw. `src/Schnittstellenzentrale.Tests/Helpers/` (TestAuthHandler).

## Infrastruktur-Klassen

### `PlaywrightServer`

Datei: `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightServer.cs` — `IAsyncLifetime`, Collection-Fixture.

| Member | Sichtbarkeit | Kurzbeschreibung |
|--------|-------------|------------------|
| `BindUrl` | `protected virtual string` | `"http://127.0.0.1:5099"` — in Unterklassen überschreibbar (mehrere Server-Varianten für Parallelvermeidung) |
| `BaseAddress` | `public string` | Bind-URL des gestarteten Kestrel-Servers |
| `Services` | `public IServiceProvider` | Service-Provider der laufenden App (Zugriff auf `IDbContextFactory<AppDbContext>` usw.) |
| `InitializeAsync()` | `public virtual` | Setzt Kultur `de-DE`, öffnet benannte In-Memory-SQLite-Anchor-Verbindung (`Mode=Memory;Cache=Shared`), erzeugt `PlaywrightApiFactory` + In-Process-Handler, baut App via `Program.BuildWebApplicationAsync` (Environment `"Playwright"`, `FindWebProjectRoot()`), Service-Overrides, `_app.Urls` = `BindUrl`, `StartAsync`, Swagger-Warmup |
| `DisposeAsync()` | `public virtual` | Stoppt App, disposed Factory + Anchor-Connection |
| `ConfigureTestServices(IServiceCollection)` | `protected virtual` | Hook für Unterklassen (zusätzliche Service-Ersetzungen) |
| `OnAfterStartAsync()` | `protected virtual` | Hook nach `StartAsync` |

Service-Overrides in `InitializeAsync` (Zeilen 84–131): `TestAuthHandler` statt Windows-Auth; alle `IHostedService` entfernt; `ISignalRNotificationService` → `SignalRNotificationProxy`; `ICurrentUserService` → Mock `TEST\testuser`; `AppDbContext` auf benannte In-Memory-SQLite; `HttpClientFactoryOptions.PrimaryHandler` → In-Process-Handler der `PlaywrightApiFactory` (kein TCP-Loopback); `ISwaggerImportService` → Mock liefert `ImportDiff` mit einem `GET /api/test`-Endpunkt; `UploadSettings.MaxIconSizeBytes = 524288`; `ImpressumSettings.FilePath` → nicht existierender Temp-Pfad (Impressum-Link ausgeblendet).

### `PlaywrightApiFactory`

Datei: `.../Infrastructure/PlaywrightApiFactory.cs` — `internal sealed`, `WebApplicationFactory<Program>` für die API-Calls des Blazor-Servers in-process; gleiche Overrides plus `ITokenStore` → `PermissiveTokenStore` (akzeptiert jeden Bearer-Token).

### `PlaywrightTestBase`

Datei: `.../Infrastructure/PlaywrightTestBase.cs` — abstrakte Basisklasse aller Playwright-Tests.

| Member | Beschreibung |
|--------|--------------|
| `Context`, `Page`, `BaseUrl` | `protected` — Browser-Kontext, aktive Page, Server-Basis-URL |
| `TestName` | `protected virtual` — Name der abgeleiteten Klasse (Trace-Dateiname) |
| `InitializeAsync()` | `TestDatabaseSeeder.ResetAsync()`, `Playwright.CreateAsync()`, `Chromium.LaunchAsync(Headless = IsHeadless())`, Kontext mit `Locale="de-DE"` + `Accept-Language: de-DE,de;q=0.9`, Tracing (Screenshots+Snapshots+Sources) starten, `Context.NewPageAsync()` |
| `IsHeadless()` | `private static` — headless nur wenn Env `CI` == `"true"`; **lokal läuft der Browser sichtbar** |
| `CreateAdditionalContextAsync(locale?, acceptLanguage?)` | `protected` — zweiter Kontext für SignalR-Multi-Session-Tests |
| `DisposeAsync()` | schreibt `playwright-traces/{TestName}.zip` relativ zum Arbeitsverzeichnis (landet unter `bin/Debug/net10.0/playwright-traces/`, gitignored via `bin/`) |

### `PlaywrightCollection`

Datei: `.../Infrastructure/PlaywrightCollection.cs` — `[CollectionDefinition("Playwright")]` mit `ICollectionFixture<PlaywrightServer>`; serialisiert alle Klassen der Collection gegen **einen** Server.

Weitere Collections/Servers (gleiche Struktur, eigene `BindUrl`/Konfiguration): `PlaywrightSignalRServer`/`PlaywrightSignalRCollection`, `PlaywrightODataEndpointServer`/`...Collection`, `PlaywrightImpressumServer`, `PlaywrightImpressumLanguageServer`, `PlaywrightImpressumFallbackServer` mit je eigenen Collections.

### `TestDatabaseSeeder`

Datei: `.../Infrastructure/TestDatabaseSeeder.cs`

| Methode | Beschreibung |
|---------|--------------|
| `ResetAsync()` | `EnsureDeletedAsync` + `EnsureCreatedAsync`, dann `SystemEntryInitializer.InitializeAsync(_services, _configuration)` (legt Systemgruppe/-anwendung an) und `SeedSystemEndpointsAsync()` |
| `SeedSystemEndpointsAsync()` | `private` — fügt `POST /authenticate` zur Systemanwendung hinzu |

**Datenstand nach Reset (Fakt):** Nur die Systemgruppe „Schnittstellenzentrale" mit der Systemanwendung und dem einen Systemendpunkt — **keine Beispiel-Anwendungen/-endpunkte**. Für repräsentative Screenshots (Workspaces mit Inhalt, Endpoint-Seite, History) müsste zusätzlich geseedet oder per UI angelegt werden; ein Helfer dafür existiert nicht.

### `TestAuthHandler`

Datei: `src/Schnittstellenzentrale.Tests/Helpers/TestAuthHandler.cs` — Test-Authentifizierungsschema für Server und API-Factory.

## Bestehende Playwright-Testklassen (23 Dateien, 64 Facts)

`HomePageTests`, `LayoutSmokeTests`, `ApplicationCrudTests`, `GroupCrudTests`, `EndpointGroupCrudTests`, `EndpointExecutionTests`, `ODataEndpointExecutionTests`, `EnvironmentManagementTests`, `HealthCheckTests`, `SwaggerImportTests`, `ODataImportTests`, `StorageModeTests`, `SignalRSyncTests`, `TreeCollapseTests`, `NavigationTests`, `InplaceEditingTests`, `IconUploadTests`, `ImpressumPageTests`, `ImpressumPageWithLanguageTests`, `CoverageGapPlaywrightTests` u. a.

Screenshot-relevante Muster:

- `LayoutSmokeTests` — `Page.GotoAsync(BaseUrl)` → `WaitForLoadStateAsync(LoadState.NetworkIdle)` → `Locator`/`BoundingBoxAsync`; Klick-Pfade: `.sz-topbar-tab` mit `HasText` („Workspaces"/„Environments"/„History"); Retry-Schleife für Blazor-Circuit-Interaktivität (Zeilen 109–122).
- `HomePageTests` — Baum-Aufklappen via `.sz-tree-chevron-btn`, `.sz-tree-children`.
- **`Page.ScreenshotAsync()` wird im gesamten `src/` nicht verwendet** — vorhandene „Screenshot"-Mechanik ist nur das Playwright-Tracing (`Screenshots = true` im `TracingStartOptions`).

## Dark-Mode-Abbildung für Screenshots

Theme-Status liegt in `localStorage` (`colorScheme`, siehe [design-system.md](design-system.md)); die `.dark`-Klasse hängt an `<html>`. Per Playwright erzeugbar z. B. über `Context.AddInitScriptAsync` (localStorage vorladen), `Page.EvaluateAsync` (`document.documentElement.classList.add('dark')`) oder Klick auf den `ThemeToggle`-Button.

## Build-/CI-Aspekte

- `Microsoft.Playwright` 1.52.0 (`Schnittstellenzentrale.Tests.csproj` Z. 29).
- MSBuild-Target `InstallPlaywright` (`AfterTargets="Build"`) installiert Chromium via `playwright.ps1 install chromium`; abschaltbar mit `-p:SkipPlaywrightInstall=true`.
- In CI (`pr-staging-ci.yml`, `staging-ci.yml`) läuft die Suite als eigener Step mit `continue-on-error: true`; Trace-Zips werden als Artifact `playwright-traces-{pr,staging}` hochgeladen.
- `dotnet test --filter "FullyQualifiedName~Playwright"` selektiert die Suite (Namenskonvention über Namespace).
