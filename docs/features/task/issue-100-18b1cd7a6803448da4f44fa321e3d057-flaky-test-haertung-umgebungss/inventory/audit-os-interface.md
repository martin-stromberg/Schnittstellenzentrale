# Audit: Umgebungssensitive Tests (OsInterface-Kandidaten)

Klassifikationskriterium (aus `requirement.md`, FR 1): Ein Test gilt als **OsInterface**,
wenn seine Assertion am echten OS-Verhalten hängt. Repo-Heuristik:

- Playwright/Browser-Automation (echtes Chromium)
- echte Kestrel-Bindung inkl. festem Port (5099–5104)
- `WebApplicationFactory` mit echtem HTTP-Roundtrip (Socket)
- Temp-Dateisystem (`Path.GetTempPath`, `sz-impressum-*`, `playwright-traces`, Repo-Arbeitsverzeichnis)
- SignalR-Live-Roundtrips über die gebundene Adresse
- Upload-Tests mit echten Dateien

**Nicht** umgebungssensitiv: `Data Source=:memory:`-SQLite, bUnit, gemockte
`HttpMessageHandler`, `WebApplicationFactory`/`TestServer` ohne echten Socket.

## Ergebnis-Übersicht

| Klassifikation | Dateien | Anteil |
|---|---:|---|
| **OsInterface** | 40 | 22 Playwright-Testdateien (24 Testklassen) + 17 Infrastructure-Dateien + `Services/ImpressumServiceTests.cs` |
| **regulär** | 65 | alle Components-, Helpers-, Hubs-, Integration- und übrigen Services-Dateien |
| **Summe** | 105 | |

Randbeobachtungen (keine Umklassifizierung, aber festgehalten):

- `Playwright/IconUploadTests.cs` schreibt Temp-Dateien mit **festen Namen** (`test-icon.png`, `test-icon.txt`, `Path.GetTempPath`, Zeilen 45/68) — Kollisionsrisiko bei parallelen `dotnet test`-Prozessen auf derselben Maschine. Der zweite Test schreibt die Datei, lädt aber ein In-Memory-`FilePayload` hoch (Zeilen 74–79); die Datei ist für den Upload ungenutzt.
- `Services/TokenStoreTests.cs:53–61` und `Integration/ApplicationGroupsControllerIntegrationTests.cs:65–80` verwenden `TokenLifetime = 1 ms` + `Task.Delay(10)` — zeitabhängig, aber ohne OS-Ressource; regulär.
- `Services/EndpointExecutionServiceTests.cs:280–284` nutzt `Task.Delay(20)` im gemockten Handler; regulär.
- `Integration/EndpointExecutionIntegrationTests.cs:82` nutzt `_factory.Server.CreateHandler()` — In-Process-TestServer-Handler, kein echter Socket; regulär.
- `Playwright/SiteScreenshotTests.cs` schreibt PNGs ins **Repo-Arbeitsverzeichnis** (`site/screenshots/`, Zeilen 150–157) — zusätzlicher OS-Bezug (Dateisystem im Checkout); ohnehin OsInterface.
- `Playwright/SitePageTests.cs` startet echtes Chromium, aber **keinen** Kestrel und kein echtes Netzwerk (Route-Interception, externe Requests werden abgebrochen, Zeilen 135–144) — OsInterface wegen Browser-Automation.

## Tabelle: Components/ (21 Dateien — alle regulär)

Alle 21 Dateien sind bUnit-Tests (`Bunit.TestContext`, verifiziert per `using Bunit`/`TestContext`-Vorkommen in jeder Datei). Kein Dateisystem, kein Netzwerk, keine Prozesse.

| Datei | Klassifikation | Begründung |
|---|---|---|
| `Components/AppShellTests.cs` | regulär | bUnit; gemockte Services/JSInterop |
| `Components/ApplicationContentViewTests.cs` | regulär | bUnit; CoverageTestFactory-Mocks |
| `Components/ApplicationContextMenuTests.cs` | regulär | bUnit |
| `Components/ApplicationEditorTests_State.cs` | regulär | bUnit |
| `Components/ConfirmDeleteApplicationDialogTests_Dialog.cs` | regulär | bUnit |
| `Components/CoverageGapComponentTests.cs` | regulär | bUnit |
| `Components/CreateEndpointGroupDialogTests_State.cs` | regulär | bUnit |
| `Components/EndpointContextMenuTests.cs` | regulär | bUnit |
| `Components/EndpointGroupContextMenuTests.cs` | regulär | bUnit |
| `Components/EndpointPageTests.cs` | regulär | bUnit |
| `Components/EnvironmentContentViewTests_State.cs` | regulär | bUnit |
| `Components/EnvironmentEditorTests_State.cs` | regulär | bUnit |
| `Components/EnvironmentSelectorTests.cs` | regulär | bUnit |
| `Components/EnvironmentsSidebarTests_State.cs` | regulär | bUnit |
| `Components/ImportDialogTests.cs` | regulär | bUnit |
| `Components/ImpressumPageTests_State.cs` | regulär | bUnit (ImpressumService gemockt — kein echtes Dateisystem) |
| `Components/RenameEndpointGroupDialogTests_State.cs` | regulär | bUnit |
| `Components/RenameGroupDialogTests_State.cs` | regulär | bUnit |
| `Components/RequestBodyPanelTests.cs` | regulär | bUnit |
| `Components/RequestHeadersPanelTests_State.cs` | regulär | bUnit |
| `Components/TopBarTests.cs` | regulär | bUnit |

## Tabelle: Helpers/ (7 Dateien)

| Datei | Klassifikation | Begründung |
|---|---|---|
| `Helpers/LocalStorageKeysTests.cs` | regulär | Reine String-Formatierungstests |
| `Helpers/SwaggerOperationHelperTests.cs` | regulär | Reine Enum-Mapping-Tests |
| `Helpers/ControllerTestFactory.cs` | regulär (Helfer) | `WebApplicationFactory<Program>` mit `TestServer` (In-Memory-Handler), `Data Source=:memory:` (Z. 35) — kein echter Socket, kein Dateisystem |
| `Helpers/CoverageTestFactory.cs` | regulär (Helfer) | Reine Datenklasse |
| `Helpers/TestAuthHandler.cs` | regulär (Helfer) | Auth-Stub |
| `Helpers/TestHelpers.cs` | regulär (Helfer) | `Data Source=:memory:`-SQLite (Z. 20, 80) |
| `Helpers/TestMockFactory.cs` | regulär (Helfer) | Mock-Fabriken |

## Tabelle: Hubs/ (1 Datei)

| Datei | Klassifikation | Begründung |
|---|---|---|
| `Hubs/EndpointHubTests_Subscriptions.cs` | regulär | Hub direkt mit `Mock<HubCallerContext>`/`Mock<IGroupManager>` instanziiert (Z. 22–37); kein SignalR-Transport |

## Tabelle: Integration/ (17 Dateien — alle regulär)

Alle Integration-Tests nutzen entweder `ControllerTestFactory` (`WebApplicationFactory` mit `TestServer` = In-Memory-Pipeline, **kein** echter Socket) oder `Data Source=:memory:`-SQLite. Keine Datei verwendet `Path.GetTempPath`, echte Kestrel-Bindung oder echtes Netzwerk.

| Datei | Klassifikation | Begründung |
|---|---|---|
| `Integration/ApplicationApiClientIntegrationTests.cs` | regulär | `IClassFixture<ControllerTestFactory>` (Z. 12), TestServer-Client |
| `Integration/ApplicationGroupsControllerIntegrationTests.cs` | regulär | `IClassFixture<ControllerTestFactory>` (Z. 12); `Task.Delay(10)` bei 1-ms-Token (Z. 65–71) — zeitabhängig, aber in-process |
| `Integration/ApplicationImportControllerIntegrationTests.cs` | regulär | `IClassFixture<ApplicationImportControllerTestFactory>` (Z. 18); Import-Services gemockt (Z. 442–446); Uploads via `JsonContent`, keine echten Dateien |
| `Integration/ApplicationRepositoryIntegrationTests.cs` | regulär | `TestHelpers.CreateInMemoryDbContext()` (Z. 16 u. a.) — `:memory:`-SQLite |
| `Integration/ApplicationsControllerIntegrationTests.cs` | regulär | `IClassFixture<ControllerTestFactory>` (Z. 14) |
| `Integration/AuthControllerIntegrationTests.cs` | regulär | `IClassFixture<ControllerTestFactory>` (Z. 10) |
| `Integration/EndpointExecutionIntegrationTests.cs` | regulär | Eigene `ControllerTestFactory`-Instanz (Z. 23); `EndpointExecutionService` erhält `new HttpClient(_factory.Server.CreateHandler())` (Z. 82) — In-Process-Handler, kein Socket |
| `Integration/EndpointRepositoryIntegrationTests.cs` | regulär | `CreateInMemoryDbContext()` (Z. 16 u. a.) — `:memory:`-SQLite |
| `Integration/ImportApplyNotificationTests.cs` | regulär | `IClassFixture<ApplicationImportControllerTestFactory>` (Z. 15) |
| `Integration/LocalizationTests.cs` | regulär | `IClassFixture<ControllerTestFactory>` (Z. 7) |
| `Integration/ODataControllerIntegrationTests.cs` | regulär | `IClassFixture<ControllerTestFactory>` (Z. 13) |
| `Integration/ODataControllerNullApplicationTests.cs` | regulär | Controller direkt mit Moq + `DefaultHttpContext` (Z. 14–26) — reine Unit-Tests trotz Integration-Ordner |
| `Integration/ODataImportServiceIntegrationTests.cs` | regulär | `ControllerTestFactory` (Z. 16) nur für Repository-DI; HTTP über `Mock<HttpMessageHandler>` (Z. 47–59) |
| `Integration/ODataNotificationIntegrationTests.cs` | regulär | `IClassFixture<ControllerTestFactory>` (Z. 16); SignalR über `SignalRNotificationMock` verifiziert |
| `Integration/RestNotificationIntegrationTests.cs` | regulär | `IClassFixture<ControllerTestFactory>` (Z. 18); SignalR gemockt |
| `Integration/SystemEntryInitializerTests.cs` | regulär | `CreateInMemoryDbContext()` (Z. 24, 218) — `:memory:`-SQLite |
| `Integration/SystemEnvironmentRepositoryIntegrationTests.cs` | regulär | `CreateInMemoryDbContext()` (Z. 15, 26) — `:memory:`-SQLite |

## Tabelle: Playwright/ Testdateien (22 Dateien — alle OsInterface)

Alle fahren echtes Chromium (und außer `SitePageTests` einen echten Kestrel-Server auf festem Port) hoch. `Page`/`Context` werden über `PlaywrightTestBase` verwaltet.

| Datei | Klasse(n) | Collection | Port | Klassifikation | Begründung |
|---|---|---|---|---:|---|
| `Playwright/ApplicationCrudTests.cs` | `ApplicationCrudTests` | `Playwright` | 5099 | OsInterface | Chromium + Kestrel |
| `Playwright/CoverageGapPlaywrightTests.cs` | `CoverageGapPlaywrightTests` | `Playwright` | 5099 | OsInterface | Chromium + Kestrel |
| `Playwright/EndpointExecutionTests.cs` | `EndpointExecutionTests` | `Playwright` | 5099 | OsInterface | Chromium + Kestrel |
| `Playwright/EndpointGroupCrudTests.cs` | `EndpointGroupCrudTests` | `Playwright` | 5099 | OsInterface | Chromium + Kestrel |
| `Playwright/EnvironmentManagementTests.cs` | `EnvironmentManagementTests` | `Playwright` | 5099 | OsInterface | Chromium + Kestrel |
| `Playwright/GroupCrudTests.cs` | `GroupCrudTests` | `Playwright` | 5099 | OsInterface | Chromium + Kestrel |
| `Playwright/HealthCheckTests.cs` | `HealthCheckTests` | `Playwright` | 5099 | OsInterface | Chromium + Kestrel |
| `Playwright/HomePageTests.cs` | `HomePageTests` | `Playwright` | 5099 | OsInterface | Chromium + Kestrel |
| `Playwright/IconUploadTests.cs` | `IconUploadTests` | `Playwright` | 5099 | OsInterface | Chromium + Kestrel + Temp-Dateien (`Path.GetTempPath`, Z. 45/68) |
| `Playwright/ImpressumPageTests.cs` | `ImpressumPageTests`, `ImpressumPageWithFileTests` | `Playwright`, `PlaywrightImpressum` | 5099, 5101 | OsInterface | Chromium + Kestrel + `sz-impressum-*`-Tempdir |
| `Playwright/ImpressumPageWithLanguageTests.cs` | `ImpressumPageWithLanguageTests`, `ImpressumPageWithFallbackTests` | `PlaywrightImpressumLanguage`, `PlaywrightImpressumFallback` | 5102, 5104 | OsInterface | Chromium + Kestrel + Tempdir |
| `Playwright/InplaceEditingTests.cs` | `InplaceEditingTests` | `Playwright` | 5099 | OsInterface | Chromium + Kestrel |
| `Playwright/LayoutSmokeTests.cs` | `LayoutSmokeTests` | `Playwright` | 5099 | OsInterface | Chromium + Kestrel; manueller 15-s-Deadline-Loop (Z. 110–121) |
| `Playwright/NavigationTests.cs` | `NavigationTests` | `Playwright` | 5099 | OsInterface | Chromium + Kestrel |
| `Playwright/ODataEndpointExecutionTests.cs` | `ODataEndpointExecutionTests` | `PlaywrightODataEndpoint` | 5103 | OsInterface | Chromium + Kestrel (Credential-Mock-Server) |
| `Playwright/ODataImportTests.cs` | `ODataImportTests` | `Playwright` | 5099 | OsInterface | Chromium + Kestrel |
| `Playwright/SignalRSyncTests.cs` | `SignalRSyncTests` | `PlaywrightSignalR` | 5100 | OsInterface | 2 Browser-Kontexte + echter SignalR-Roundtrip über Kestrel (WebSocket) |
| `Playwright/SitePageTests.cs` | `SitePageTests` | – (keine) | – | OsInterface | Echtes Chromium, kein Kestrel/kein echtes Netz (Route-Interception `site.test`, Z. 34, 135–144) |
| `Playwright/SiteScreenshotTests.cs` | `SiteScreenshotTests` | `Playwright` | 5099 | OsInterface | Chromium + Kestrel; schreibt PNGs ins Repo-Checkout (`site/screenshots`, Z. 150–157); 20-s-Deadline-Loop (Z. 135) |
| `Playwright/StorageModeTests.cs` | `StorageModeTests` | `Playwright` | 5099 | OsInterface | Chromium + Kestrel; `localStorage` im Browser |
| `Playwright/SwaggerImportTests.cs` | `SwaggerImportTests` | `Playwright` | 5099 | OsInterface | Chromium + Kestrel |
| `Playwright/TreeCollapseTests.cs` | `TreeCollapseTests` | `Playwright` | 5099 | OsInterface | Chromium + Kestrel |

## Tabelle: Playwright/Infrastructure/ (17 Dateien — alle OsInterface-Kontext)

Keine Testdateien im xUnit-Sinn (keine `[Fact]`/`[Theory]`), aber sie liegen im
Namespace `Schnittstellenzentrale.Tests.Playwright.Infrastructure` und enthalten den
gesamten OS-Bezug (Ports, Tempdirs, Chromium-Lebenszyklus).

| Datei | Klassifikation | OS-Bezug |
|---|---|---|
| `Infrastructure/PlaywrightServer.cs` | OsInterface (Infra) | Kestrel-Bindung `127.0.0.1:5099` (Z. 39, 134–135), `Path.GetTempPath` für `no-impressum-*.md` (Z. 127) |
| `Infrastructure/PlaywrightImpressumServer.cs` | OsInterface (Infra) | Port 5101; `sz-impressum-{guid}`-Tempdir + `Directory.Delete` (Z. 23, 61–65) |
| `Infrastructure/PlaywrightImpressumLanguageServer.cs` | OsInterface (Infra) | Port 5102; `sz-impressum-lang-{guid}`-Tempdir (Z. 24, 66–70) |
| `Infrastructure/PlaywrightODataEndpointServer.cs` | OsInterface (Infra) | Port 5103 |
| `Infrastructure/PlaywrightImpressumFallbackServer.cs` | OsInterface (Infra) | Port 5104; `sz-impressum-fb-{guid}`-Tempdir (Z. 22, 60–64) |
| `Infrastructure/PlaywrightSignalRServer.cs` | OsInterface (Infra) | Port 5100; echter `IHubContext<EndpointHub>` ab Z. 27–32 |
| `Infrastructure/PlaywrightTestBase.cs` | OsInterface (Infra) | Chromium-Launch (Z. 49), `playwright-traces/`-Verzeichnis im CWD (Z. 99–103) |
| `Infrastructure/PlaywrightApiFactory.cs` | OsInterface (Infra) | `WebApplicationFactory`-TestServer (In-Process) — selbst kein Socket, gehört aber zur Kestrel-Fixture |
| `Infrastructure/PlaywrightCollection.cs` | OsInterface (Infra) | Fixture-Verdrahtung |
| `Infrastructure/PlaywrightSignalRCollection.cs` | OsInterface (Infra) | Fixture-Verdrahtung |
| `Infrastructure/PlaywrightImpressumCollection.cs` | OsInterface (Infra) | Fixture-Verdrahtung |
| `Infrastructure/PlaywrightImpressumLanguageCollection.cs` | OsInterface (Infra) | Fixture-Verdrahtung |
| `Infrastructure/PlaywrightImpressumFallbackCollection.cs` | OsInterface (Infra) | Fixture-Verdrahtung |
| `Infrastructure/PlaywrightODataEndpointCollection.cs` | OsInterface (Infra) | Fixture-Verdrahtung |
| `Infrastructure/SignalRNotificationProxy.cs` | OsInterface (Infra) | Proxy-Baustein der SignalR-Fixture |
| `Infrastructure/TestDatabaseSeeder.cs` | OsInterface (Infra) | DB-Reset/Seeding im Server-Scope |
| `Infrastructure/TestPaths.cs` | OsInterface (Infra) | `CI`-Umgebungsvariable (Z. 16), Dateisystem-Suche nach `*.slnx` (Z. 26–37) |

## Tabelle: Services/ (20 Dateien)

| Datei | Klassifikation | Begründung |
|---|---|---|
| `Services/ActivityLogServiceTests.cs` | regulär | `:memory:`-SQLite via `TestHelpers`; Zeitvergleiche über `DateTime.UtcNow` (Z. 17–21) |
| `Services/ApplicationApiClientTests.cs` | regulär | `Mock<HttpMessageHandler>` (Z. 73 ff.), TokenStore-Mock |
| `Services/ApplicationGroupServiceTests.cs` | regulär | `:memory:`-SQLite; Icon-Bytes als `byte[]` im Speicher (Z. 34) — kein echtes Datei-Upload |
| `Services/ApplicationLinkServiceTests.cs` | regulär | `:memory:`-SQLite |
| `Services/DatabaseProviderFactoryTests.cs` | regulär | In-Memory-`IConfiguration`; `ProviderName`-Assertion (Z. 54), keine DB-Verbindung |
| `Services/EndpointExecutionServiceTests.cs` | regulär | `Mock<HttpMessageHandler>`; `Task.Delay(20)` im Mock (Z. 282) |
| `Services/EndpointScriptRunnerTests.cs` | regulär | Jint in-process; SignalR-Mock |
| `Services/HealthCheckServiceTests.cs` | regulär | `Mock<HttpMessageHandler>` (Z. 34) |
| `Services/HistoryServiceTests.cs` | regulär | `:memory:`-SQLite |
| `Services/ImportDiffCalculatorTests.cs` | regulär | Reine Modellvergleiche |
| `Services/ImpressumServiceTests.cs` | **OsInterface** | Echtes Temp-Verzeichnis `Path.Combine(Path.GetTempPath(), Guid)` (Z. 15–16) mit `Directory.Delete(recursive: true)` in `Dispose` (Z. 22–23); **zwei Tests schreiben Dateien ins `AppContext.BaseDirectory`** (`impressum.md`, `mein_impressum.md` — Z. 118, 127, 143, 151) d. h. in das Build-Ausgabeverzeichnis des Testprojekts, mit `File.Delete` im `finally` (Z. 133–134, 157–158); thread-globale `CultureInfo.CurrentUICulture`-Mutation pro Test (z. B. Z. 43–52) |
| `Services/NavigationStateServiceTests.cs` | regulär | Reine Zustandslogik |
| `Services/NotificationPolicyTests.cs` | regulär | `Mock<ISignalRNotificationService>` (Z. 11) |
| `Services/ODataImportServiceTests.cs` | regulär | `Mock<HttpMessageHandler>` (z. B. Z. 477, 681); `localhost`-URLs nur als Testdaten, kein echter Request |
| `Services/ODataImportServiceRealMetadataTests.cs` | regulär | `Mock<HttpMessageHandler>` (Z. 31–43); CSDL in-memory via `CsdlWriter` |
| `Services/StorageModeServiceTests.cs` | regulär | `Mock<IJSRuntime>`/`Mock<IJSObjectReference>` (Z. 14–25) — kein echter Browser/localStorage |
| `Services/SwaggerImportServiceTests.cs` | regulär | `Mock<IHttpClientFactory>` (Z. 52); OpenAPI-Dokumente als Testdaten |
| `Services/SystemEndpointSyncServiceTests.cs` | regulär | `ServiceCollection` mit gemockten Repos/Services (Z. 37–42); `localhost`-URLs nur Testdaten |
| `Services/ThemeServiceTests.cs` | regulär | `Mock<IJSRuntime>` |
| `Services/TokenStoreTests.cs` | regulär | Reiner In-Memory-Store; `TokenLifetime=1 ms` + `Task.Delay(10)` (Z. 53–57) |

## Abgrenzungsliste (Audit-Ergebnis)

**OsInterface-Testklassen (Trait-Kandidaten):** alle 24 Klassen unter
`Schnittstellenzentrale.Tests.Playwright` (inkl. `SitePageTests` ohne Collection)
sowie `Schnittstellenzentrale.Tests.Services.ImpressumServiceTests`.

**Ausdrücklich regulär (bleiben blockierend):** alle bUnit-Tests (Components/), alle
`ControllerTestFactory`-Integration-Tests (TestServer ohne echten Socket), alle
`CreateInMemoryDbContext`-Tests (`Data Source=:memory:`), alle Moq-`HttpMessageHandler`-Tests,
`EndpointHubTests_Subscriptions`, `ODataControllerNullApplicationTests`.

**Offene Beobachtung für die Planung:** `ImpressumServiceTests` ist die einzige
OsInterface-Datei außerhalb des `Playwright`-Namensraums — ein reiner
FQN→Trait-Paritäts-Meta-Test würde sie nicht erfassen; die Zuordnung muss manuell
erfolgen (das Issue nennt sie explizit als Beispiel).
