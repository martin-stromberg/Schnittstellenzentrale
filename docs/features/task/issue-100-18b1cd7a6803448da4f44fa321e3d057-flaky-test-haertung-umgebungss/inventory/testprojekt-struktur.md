# Testprojekt-Struktur

## Projektdatei

`src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj`:

- `TargetFramework`: `net10.0` (Solution-SDK: .NET 10.0.301 per `global.json`, `rollForward: latestFeature`)
- `ImplicitUsings` + `Nullable` aktiviert; `WarningsAsErrors` enthält `CS1591` (fehlende XML-Doku)
- Projekt-Referenzen: `Schnittstellenzentrale.Core`, `Schnittstellenzentrale.Infrastructure`, `Schnittstellenzentrale`
- `<Using Include="Xunit" />` (globaler using, Zeile 45–47)

### Paketversionen (csproj-Zeilen)

| Paket | Version | Zeile |
|---|---|---|
| `xunit` | **2.9.3** (xUnit **v2**, nicht v3) | 36 |
| `xunit.runner.visualstudio` | 3.1.5 | 37 |
| `Microsoft.NET.Test.Sdk` | 18.5.1 | 29 |
| `Microsoft.Playwright` | 1.52.0 | 30 |
| `Microsoft.AspNetCore.Mvc.Testing` | 10.0.* | 25 |
| `Microsoft.EntityFrameworkCore.Sqlite` | 10.0.* | 26 |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.* | 27 |
| `Microsoft.JSInterop` | 10.0.* | 28 |
| `bunit` | 2.7.2 | 19 |
| `AngleSharp` | 1.5.0 (direkter Override wegen GHSA-pgww-w46g-26qg, Kommentar Zeile 15–18) | 20 |
| `Moq` | 4.20.72 | 31 |
| `coverlet.collector` | 10.0.0 | 21 |
| `Jint` | 4.9.2 | 24 |
| `SQLitePCLRaw.lib.e_sqlite3` | 3.53.3 | 32 |

### MSBuild-Target `InstallPlaywright` (csproj Zeilen 41–43)

```xml
<Target Name="InstallPlaywright" AfterTargets="Build" Condition="'$(SkipPlaywrightInstall)' != 'true'">
  <Exec Command="pwsh -NoProfile -File &quot;$(MSBuildProjectDirectory)\bin\$(Configuration)\$(TargetFramework)\playwright.ps1&quot; install chromium" />
</Target>
```

Läuft nach **jedem** Build, sofern nicht `-p:SkipPlaywrightInstall=true` gesetzt ist (wird in `release.yml:106` genutzt; in den CI-Build-Steps nicht).

## Dateimenge je Verzeichnis

105 `.cs`-Dateien (ohne `obj/`):

| Verzeichnis | Dateien | Davon mit `[Fact]`/`[Theory]` | Inhalt |
|---|---:|---:|---|
| `Components/` | 21 | 21 | bUnit-Tests (`Bunit.TestContext`), reine Komponententests |
| `Helpers/` | 7 | 2 | `LocalStorageKeysTests`, `SwaggerOperationHelperTests` (Tests); `ControllerTestFactory`, `CoverageTestFactory`, `TestAuthHandler`, `TestHelpers`, `TestMockFactory` (Helfer) |
| `Hubs/` | 1 | 1 | `EndpointHubTests_Subscriptions` (Moq-basiert) |
| `Integration/` | 17 | 17 | WebApplicationFactory-/Repository-Integrationstests |
| `Playwright/` (ohne Infrastructure) | 22 | 22 | E2E-Testdateien, 24 Testklassen (2 Dateien enthalten je 2 Klassen) |
| `Playwright/Infrastructure/` | 17 | 0 | Server-Fixtures, Collections, Basisklasse, Seeder, Proxy, Pfade |
| `Services/` | 20 | 20 | Service-Unit-Tests (Moq, In-Memory-SQLite) |
| **Summe** | **105** | **83** | 703 `[Fact]`/`[Theory]`-Attribute gesamt |

## Vorhandene Attribute / Traits

- **`[Trait]`**: kein einziges Vorkommen im gesamten Testprojekt (Volltextsuche `Trait|CollectionDefinition|ICollectionFixture|\[Collection` über alle `.cs`-Dateien — alle Treffer sind Collection-Attribute).
- **Testklassen-Attribute:** ausschließlich `[Fact]`, `[Theory]` (mit `[InlineData]`), `[Collection("…")]` (nur Playwright), `IClassFixture<T>` (nur Integration), `IAsyncLifetime`, `IDisposable`.
- Der bisherige CI-Filter `FullyQualifiedName~Playwright` trifft alle Playwright-Tests **über den Namespace** `Schnittstellenzentrale.Tests.Playwright.*` — nicht über Datei- oder Klassennamen. `SitePageTests` hat kein `[Collection]`, liegt aber im selben Namespace und wird vom Filter erfasst.

## Test-Collections (alle in `Playwright/Infrastructure/`)

| Collection-Name | Definition-Datei | Fixture | Port (`BindUrl`) |
|---|---|---|---|
| `Playwright` | `PlaywrightCollection.cs:4` | `PlaywrightServer` | `http://127.0.0.1:5099` (`PlaywrightServer.cs:39`) |
| `PlaywrightSignalR` | `PlaywrightSignalRCollection.cs:4` | `PlaywrightSignalRServer` | `http://127.0.0.1:5100` (`PlaywrightSignalRServer.cs:14`) |
| `PlaywrightImpressum` | `PlaywrightImpressumCollection.cs:4` | `PlaywrightImpressumServer` | `http://127.0.0.1:5101` (`PlaywrightImpressumServer.cs:15`) |
| `PlaywrightImpressumLanguage` | `PlaywrightImpressumLanguageCollection.cs:4` | `PlaywrightImpressumLanguageServer` | `http://127.0.0.1:5102` (`PlaywrightImpressumLanguageServer.cs:16`) |
| `PlaywrightODataEndpoint` | `PlaywrightODataEndpointCollection.cs:4` | `PlaywrightODataEndpointServer` | `http://127.0.0.1:5103` (`PlaywrightODataEndpointServer.cs:20`) |
| `PlaywrightImpressumFallback` | `PlaywrightImpressumFallbackCollection.cs:4` | `PlaywrightImpressumFallbackServer` | `http://127.0.0.1:5104` (`PlaywrightImpressumFallbackServer.cs:14`) |

- Jede Collection hält einen eigenen `ICollectionFixture<TServer>` — xUnit serialisiert Tests **innerhalb** einer Collection; verschiedene Collections können parallel laufen (daher die Port-Staffelung 5099–5104).
- `SitePageTests` (`Playwright/SitePageTests.cs:12`) steht **bewusst in keiner Collection** (Klassenkommentar Zeile 6–11): ohne Kestrel, statische Dateien via Route-Interception unter Pseudo-Origin `http://site.test`.
- Keine Collections außerhalb von `Playwright/`; Integration-Tests verwenden `IClassFixture<ControllerTestFactory>` bzw. `IClassFixture<ApplicationImportControllerTestFactory>` (Klassen-Fixture pro Klasse, kein Collection-Sharing).

## Helfer-Bestand (nicht-Test-Dateien)

| Datei | Rolle |
|---|---|
| `Helpers/ControllerTestFactory.cs` | `WebApplicationFactory<Program>` mit Test-Auth (`TestAuthHandler`), `Data Source=:memory:`-SQLite (Zeile 35), `SignalRNotificationMock` (Zeile 30), optionale `TokenLifetime` (Zeile 26), `ObtainTokenAsync` (Zeile 90–96) |
| `Helpers/CoverageTestFactory.cs` | Reine Datenklasse für gemeinsame Mocks/Fixtures |
| `Helpers/TestAuthHandler.cs` | Auth-Handler, liefert immer `TEST\testuser` |
| `Helpers/TestHelpers.cs` | `CreateInMemoryDbContext()` → `Data Source=:memory:` (Zeilen 20, 80), Shared-Connection-Helper für Concurrency-Tests |
| `Helpers/TestMockFactory.cs` | Mock-Fabriken inkl. `CreateFakeLocalizer()` und `CreateCoverageScenarioDependencies()` |
| `Playwright/Infrastructure/PlaywrightApiFactory.cs` | `WebApplicationFactory<Program>` als In-Process-TestServer für API-Calls des Blazor-Servers (Zeilen 18–24: bewusst kein TCP, kein Kestrel-Deadlock) |
| `Playwright/Infrastructure/TestDatabaseSeeder.cs` | `ResetAsync()` (EnsureDeleted+EnsureCreated+SystemEntryInitializer+SeedSystemEndpoints, Zeilen 28–39), `SeedShowcaseDataAsync()` für Screenshot-Tests (Zeilen 51–63) |
| `Playwright/Infrastructure/SignalRNotificationProxy.cs` | Volatile Inner-Service-Delegation; Null-Objekt bis `SetInner` nach App-Start |
| `Playwright/Infrastructure/TestPaths.cs` | `IsHeadless()` (CI-Umgebungsvariable, Zeilen 14–18), `FindRepoRoot()` (sucht `Schnittstellenzentrale.slnx` aufwärts, Zeilen 26–37) |

## Umgebungskonfiguration der App

`src/Schnittstellenzentrale/appsettings.Playwright.json` (wird beim Build ins Test-bin kopiert):

```json
{
  "Api": { "BaseUrl": "http://127.0.0.1:5099" },
  "ConnectionStrings": { "Default": "Data Source=schnittstellenzentrale-playwright.db" },
  "DatabaseProvider": "SQLite",
  "Urls": "http://127.0.0.1:5099"
}
```

Fakten: `Urls` und `Api:BaseUrl` zeigen auf Port 5099; `PlaywrightServer.InitializeAsync` löscht `_app.Urls` (Zeile 133) und setzt `BindUrl` neu, sodass Unterklassen-Ports (5100–5104) die konfigurierte URL überschreiben. Die Datei-Connection-String `schnittstellenzentrale-playwright.db` wird in den Tests durch die benannte In-Memory-SQLite (`Data Source={Guid};Mode=Memory;Cache=Shared`, `PlaywrightServer.cs:64`) ersetzt. `Program.BuildWebApplicationAsync` enthält `Playwright`-Environment-Verzweigungen (`Program.cs:53, 179, 190, 193, 209`). `ControllerTestFactory` nutzt `builder.UseEnvironment("Testing")` (Zeile 42); `Program.cs:179` hat einen `Testing`-Zweig.
