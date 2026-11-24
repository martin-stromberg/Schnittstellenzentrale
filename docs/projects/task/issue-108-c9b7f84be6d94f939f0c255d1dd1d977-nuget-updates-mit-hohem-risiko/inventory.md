# Bestandsaufnahme – .NET-11-Upgrade + NuGet-Befunde (Issue #108)

Stand: Repository-Klon `martin-stromberg/Schnittstellenzentrale`, alle Angaben verifiziert gegen den tatsächlichen Dateibestand. Vorgehensplan und F1–F8-Einordnung: [requirement.md](requirement.md).

## Zielbild kurz

Alle 4 Projekte von `net10.0` auf `net11.0` heben (SDK `global.json` → 11.x, 9 CI-Pins → `11.0.x`, 17 Floating-Referenzen `10.0.*` → `11.0.*` sofern F7 Floating beibehält) und danach schrittweise die Issue-#108-Befunde abarbeiten: Playwright 1.52→1.65 (F1), xunit.v3-Migration (F2), koordiniertes OpenApi-3.x/Swashbuckle-11-Update (F3), Serilog.AspNetCore 11 (F4), Edm 9 + OData-Strategie (F5), FrameworkReference-/SignalR.Client-Entscheidung (F6), Floating-vs.-Lock/CPM-Prozess (F7), F8 auslagern. Pro Schritt ein eigener PR gegen `staging`; finaler PR `Closes #108`.

## Paketinventar

41 PackageReference-Vorkommen über 4 Projekte (direkte Referenzen; `Version` wie im csproj):

| Paket | Version | Projekt(e) | Typ / Bemerkung |
|---|---|---|---|
| Microsoft.AspNetCore.Authentication.Negotiate | `10.0.*` | Web | Floating |
| Microsoft.AspNetCore.OData | `10.0.0-preview.2` | Web | Pinned Preview – net10-only-Paket, F5-Risiko (csproj:16) |
| Microsoft.EntityFrameworkCore.Design | `10.0.*` | Web, Infrastructure | Floating, `PrivateAssets=all` |
| Serilog.AspNetCore | `10.0.0` | Web | F4-Ziel (→ 11.0.0) |
| Serilog.Sinks.EventLog | `4.0.0` | Web | F4-Kompatibilität prüfen |
| Serilog.Sinks.File | `7.0.0` | Web | F4-Kompatibilität prüfen |
| ShadcnBlazor | `1.0.14` | Web | Pinned |
| Swashbuckle.AspNetCore | `10.1.7` | Web | F3-Ziel (→ 11.x, gemeinsamer PR mit OpenApi) |
| Microsoft.OpenApi | `2.10.0` | Core, Infrastructure | F3-Ziel (→ 3.x) – Referenz in **beiden** Projekten |
| Jint | `4.9.2` | Infrastructure, Tests | Pinned |
| Markdig | `1.2.0` | Infrastructure | Pinned |
| Microsoft.AspNetCore.Http.Abstractions | `2.3.0` | Infrastructure | F6: Legacy-Paket, wäre durch `FrameworkReference Microsoft.AspNetCore.App` abgedeckt |
| Microsoft.AspNetCore.SignalR.Client | `10.0.*` | Infrastructure | F6: im Code **nur** Web-Projekt genutzt → ins Web-Projekt verschieben |
| Microsoft.AspNetCore.SignalR.Core | `1.2.10` | Infrastructure | F6: liefert `Hub`/`IHubContext`; Shared-Framework-Wechsel = faktischer Major-Sprung |
| Microsoft.EntityFrameworkCore | `10.0.*` | Infrastructure | Floating |
| Microsoft.EntityFrameworkCore.Sqlite | `10.0.*` | Infrastructure, Tests | Floating |
| Microsoft.EntityFrameworkCore.SqlServer | `10.0.*` | Infrastructure, Tests | Floating (Tests nutzen faktisch nur Sqlite – s. Risiken) |
| Microsoft.Extensions.Configuration.Abstractions | `10.0.*` | Infrastructure | Floating |
| Microsoft.Extensions.Configuration.Binder | `10.0.*` | Infrastructure | Floating |
| Microsoft.Extensions.DependencyInjection.Abstractions | `10.0.*` | Infrastructure | Floating |
| Microsoft.Extensions.Http | `10.0.*` | Infrastructure | Floating |
| Microsoft.JSInterop | `10.0.*` | Infrastructure, Tests | Floating |
| Microsoft.OData.Edm | `8.4.3` | Infrastructure | F5: Versions-Split (transitiv kommt 9.0.0-preview.4 via AspNetCore.OData) |
| SQLitePCLRaw.lib.e_sqlite3 | `3.53.3` | Infrastructure, Tests | Pinned |
| System.Security.Cryptography.Pkcs | `10.0.*` | Infrastructure | Floating **mit Security-Pin-Kommentar** (csproj:28–37): GHSA-846f-f764-76mx / GHSA-2fmv-rv2c-59x7 – begründet das bewusste Floating-Modell |
| AngleSharp | `1.5.0` | Tests | Pinned **mit Override-Kommentar** (csproj:13–16): bunit 2.7.2 zieht transitiv verwundbares AngleSharp 1.4.0 (GHSA-pgww-w46g-26qg) |
| bunit | `2.7.2` | Tests | Pinned – F6: verträgt transitive `FrameworkReference` zu prüfen |
| coverlet.collector | `10.0.0` | Tests | F2: Verhalten unter Microsoft.Testing.Platform verifizieren |
| Microsoft.AspNetCore.Mvc.Testing | `10.0.*` | Tests | Floating (`WebApplicationFactory`) |
| Microsoft.NET.Test.Sdk | `18.5.1` | Tests | F2: MTP-Umstellung betrifft dieses Paket |
| Microsoft.Playwright | `1.52.0` | Tests | F1-Ziel (→ 1.65.1) |
| Moq | `4.20.72` | Tests | Pinned |
| xunit | `2.9.3` | Tests | F2-Ziel (→ `xunit.v3`) |
| xunit.runner.visualstudio | `3.1.5` | Tests | F2: Zwischenschritt → 4.x vor xunit.v3 |

**Floating `10.0.*` exakt: 17** (Web 2, Core 0, Infrastructure 11, Tests 4) – deckt sich mit der Requirement-Angabe. Keine `packages.lock.json`, kein `Directory.Packages.props`, kein `NuGet.config`/`nuget.config` im Repo.

**Spezial-Kommentare/-Targets in csproj:**
- Pkcs-Security-Pin: `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj:28-37`
- AngleSharp-Override: `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj:13-16`
- `InstallPlaywright`-Target (führt `playwright.ps1 install chromium` nach Build aus, abschaltbar via `-p:SkipPlaywrightInstall=true`): `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj:39-41` – bei F1-Update wird die zum Paket gehörende neue `playwright.ps1` genutzt; CI nutzt keinen separaten Browser-Install-Schritt (`pr-staging-ci.yml:133-136`, `staging-ci.yml:209`).
- `<Using Include="Xunit" />`: `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj:44` (impliziter globaler `using Xunit` – für F2 relevant)

## TFM/SDK/CI-Befund

- **TFM `net10.0` in allen 4 Projekten:** Web `Schnittstellenzentrale.csproj:38`, Core `…Core.csproj:8`, Infrastructure `…Infrastructure.csproj:41`, Tests `…Tests.csproj:4`. Solution ist `Schnittstellenzentrale.slnx` (Repo-Root, alle 4 Projekte).
- **`global.json:3`:** SDK `10.0.301`, `rollForward: latestFeature`.
- **9 `dotnet-version`-Pins `10.0.x`:** `.github/workflows/pr-staging-ci.yml:81,126,226` · `staging-ci.yml:74,107,202` · `security-scan.yml:31` · `release.yml:52` · `.github/actions/build-and-package/action.yml:41`.
- **Weitere net10-Annahmen:**
  - `.github/actions/build-and-package/action.yml:57` – `dotnet publish … --framework net10.0` (harter TFM im Publish-Kommando).
  - `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightTestBase.cs:27` – Kommentar nennt `bin/<cfg>/net10.0/`.
  - `README.md:34` – „.NET 10.0 Runtime (ASP.NET Core)"; `docs/help/schnittstellenzentrale/installation.md:6` – „.NET 10.0 Runtime"; `docs/help/playwright-tests/installation.md:5` – „.NET 10.0 SDK".
  - Historischer Fund (kein Update-Ziel): `docs/features/task/issue-80-…/test-results.md:6`.
- **Kein Dockerfile** im Repository – Release-Verpackung läuft über `build-and-package` (self-contained win-x64 Publish).
- CI-Nutzung von `dotnet` nur in `pr-staging-ci.yml`, `staging-ci.yml`, `release.yml`, `security-scan.yml`; `pages.yml` (Site-Deploy) und die Review-/Promotion-Workflows ohne dotnet.

## Betroffene Codebereiche

### F1 – Playwright (`Microsoft.Playwright` 1.52.0)

- Infrastruktur: `Playwright/Infrastructure/PlaywrightServer.cs` (Kestrel, Port 5099; `IAsyncLifetime`, Readiness-Poll), `PlaywrightApiFactory.cs` (`WebApplicationFactory<Program>` mit In-Process-Handler), `PlaywrightSignalRServer.cs` (Port 5100, echter `IHubContext<EndpointHub>`), dazu Impressum-/OData-Server-Varianten (Ports 5101–5104) und `PlaywrightTestBase.cs` (`IAsyncLifetime`, `ITestOutputHelper`, Tracing, Default-Timeouts 30 s/60 s).
- Locator-/Action-API-Vorkommen im Verzeichnis `src/Schnittstellenzentrale.Tests/Playwright` (Substring-Zählung pro Token, inkl. Typnamen wie `ILocator`): `Locator` 334 · `ClickAsync` 212 · `Expect` 150 · `ToBeVisibleAsync` 117 · `GetByRole` 106 · `FillAsync` 62 · `GetByLabel` 44 · `GetByText` 31 · `SelectOptionAsync` 10 · `PressAsync` 8 · `ScreenshotAsync` 6 · `BoundingBoxAsync` 6 (u. a. `LayoutSmokeTests`) · `GetByPlaceholder` 5 · `SetInputFilesAsync` 3 · `ToHaveTextAsync` 2 · `WaitForAsync` 2 · **`ToMatchAriaSnapshotAsync` 0** (in der Requirement genannt – de facto nicht verwendet) · `CheckAsync`/`HoverAsync`/`GetByTitle`/`Keyboard` 0.
- Kategorie-Traits: keine rohen `[Trait("Category",…)]`-Vorkommen – stattdessen eigene Attribute `[OsInterface]`/`[OsInterfaceFact]`/`[OsInterfaceTheory]`/`[Flaky]` (`Infrastructure/Testing/`). `[OsInterface]` auf 24 Dateien: alle 22 Playwright-Testklassen plus `Helpers/TempDirectoryCleanupTests.cs:8` und `Services/ImpressumServiceTests.cs:12`. Playwright-Fact-Zählung: 73 `[Fact]`-Methoden in `Playwright/`.

### F2 – xunit (2.9.3, runner.visualstudio 3.1.5)

- 704× `[Fact]`, 8× `[Theory]`; `IClassFixture` 11×, `ICollectionFixture` 6×, `[Collection(…)]` 23×.
- `IAsyncLifetime` in 4 Dateien (Signatur `Task InitializeAsync()`/`Task DisposeAsync()` – wird unter v3 `ValueTask`): `Integration/EndpointExecutionIntegrationTests.cs:15`, `Playwright/Infrastructure/PlaywrightServer.cs:27`, `Playwright/Infrastructure/PlaywrightTestBase.cs:8`, `Playwright/SitePageTests.cs:14`.
- `Xunit.Abstractions`/`ITestOutputHelper`: 22 Dateien nutzen `ITestOutputHelper` (überwiegend Playwright-Tests); `Infrastructure/Testing/CategoryTraitDiscoverer.cs:1` importiert `Xunit.Abstractions` direkt.
- `Xunit.Sdk`-Extensibilität (v2-spezifisch, unter v3 neu aufzusetzen): `OsInterfaceAttribute.cs` (`ITraitAttribute`, `TraitDiscoverer`), `OsInterfaceFactAttribute.cs`/`OsInterfaceTheoryAttribute.cs` (erben `FactAttribute`/`TheoryAttribute`), `FlakyAttribute.cs`, `CategoryTraitDiscoverer.cs` (`ITraitDiscoverer`, `IAttributeInfo`), Meta-Tests `TraitParityTests.cs`/`FlakyConventionTests.cs` + `TestTraitReader.cs`.
- Test-Kommandos dokumentiert in `CLAUDE.md` und `docs/help/stabilitaet/os-interface-tests.md:75-107`; CI-Filter `Category!=OsInterface`/`Category=OsInterface` in beiden CI-Workflows; `scripts/Retry-FlakyTests.ps1:107-108` nutzt `--list-tests`/`--filter` – unter MTP/xunit.v3 zu verifizieren.

### F3 – OpenApi 2.10.0 + Swashbuckle 10.1.7

- Reader-API (2.x): `Infrastructure/Services/SwaggerImportService.cs:2` (`Microsoft.OpenApi.Reader`), :89-90 (`OpenApiJsonReader`, `ReadAsync(stream, uri, OpenApiReaderSettings, ct)` → `result.Diagnostic`/`result.Document`); Dokumentmodell :64 (`OpenApiDocument`-Signatur von `SyncFromDocumentAsync`).
- Modell-/Extension-API: `Infrastructure/Helpers/SwaggerOperationHelper.cs` (`OpenApiDocument`, `Paths`, `Operations`, `IOpenApiExtension`, `JsonNodeExtension`+`JsonValue` :20-29, `OpenApiSecuritySchemeReference` :150, `IOpenApiParameter` :172).
- Web-Schicht: `Program.cs:2,89-112` (`AddSwaggerGen`, `OpenApiInfo`, `OpenApiSecurityScheme`, `IncludeXmlComments`), `Filters/SecurityOperationFilter.cs` (`OpenApiSecurityRequirement`, `OpenApiSecuritySchemeReference`), `Filters/ContextHeadersOperationFilter.cs` (`OpenApiParameter`, `OpenApiSchema`, `JsonSchemaType`), `Filters/SzExtensionsOperationFilter.cs:21-37` (`IOpenApiExtension`, `JsonNodeExtension` – x-sz-*-Erweiterungen), `SystemEndpointSyncService.cs:5` (**`Swashbuckle.AspNetCore.Swagger`/`ISwaggerProvider`**, `GetSwagger("v1")` :47-48 – koppelt den Startup-Abgleich direkt an Swashbuckle), `Program.cs:202-203` (`UseSwagger`/`UseSwaggerUI`).
- `ApplicationImportController.cs` nutzt nur `ISwaggerImportService` (domäneneigen), keinen Swashbuckle-Typ.
- Core trägt `Microsoft.OpenApi` selbst: `Core/Interfaces/ISwaggerImportService.cs:1,18` (`OpenApiDocument` in öffentlicher Signatur – Grund für den Zwang „ein gemeinsamer PR").
- Test-Referenzen: `Tests/Services/SwaggerImportServiceTests.cs`, `Services/SystemEndpointSyncServiceTests.cs`, `Integration/SystemEntryInitializerTests.cs` (bauen `OpenApiDocument`/`OpenApiPaths`/`OpenApiPathItem`/`OpenApiOperation`/`OpenApiSecurityRequirement`/`IOpenApiExtension`-Fixtures).

### F4 – Serilog.AspNetCore 10.0.0

- `Program.cs:3` (`using Serilog`), :43-47 (Bootstrap-Logger via `LoggerConfiguration().ReadFrom.Configuration(appsettings.json)`), :59 (`builder.Host.UseSerilog()`).
- `appsettings.json:13-32` – Serilog-Abschnitt mit Sinks `EventLog` (Source `Schnittstellenzentrale`, Log `Application`) und `File` (`logs/log-.txt`, Daily-Rolling, 7 Dateien). `appsettings.Development.json` ohne Serilog-Abschnitt; `appsettings.Playwright.json` ohne Serilog.

### F5 – OData (`Microsoft.AspNetCore.OData` 10.0.0-preview.2 + `Microsoft.OData.Edm` 8.4.3)

- Edm-Lesen (Infrastructure, kompiliert gegen 8.4.3): `Infrastructure/Services/ODataImportService.cs:2-3` (`Microsoft.OData.Edm`, `Microsoft.OData.Edm.Csdl`), :64 (`CsdlReader.Parse`), `IEdmModel`/`IEdmEntitySet`/`IEdmOperation`/`IEdmAction` (:59,:87,:106-108).
- Edm-Modellaufbau (Web): `OData/ODataEdmModelBuilder.cs` – `ODataConventionModelBuilder` (aus `Microsoft.OData.ModelBuilder`, kommt transitiv via AspNetCore.OData), `EdmModel`-Cast :45, `EdmTerm`/`EdmVocabularyAnnotation`/`EdmStringConstant`/`EdmCoreModel` (:47-78).
- Registrierung: `Program.cs:80-87` (`AddOData` → `AddRouteComponents("odatav4", …)`, `Select/Filter/Expand/OrderBy/Count/SetMaxTop(1000)`).
- Controller: `src/Schnittstellenzentrale/OData/` – `ODataApplicationsController`, `ODataApplicationGroupsController`, `ODataEndpointsController`, `ODataEndpointGroupsController`, `ODataAuthController`, `ODataControllerBase`, `ODataEndpointUpdateHelper`, `ODataPatchHelper`; `[EnableQuery]`/`ODataRouteAttribute` aus `Microsoft.AspNetCore.OData.Query`/`.Routing.Attributes`.
- Tests: `Integration/ODataControllerIntegrationTests.cs` **= exakt 78 `[Fact]`** (Requirement-Zahl verifiziert), dazu `ODataControllerNullApplicationTests`, `ODataImportServiceIntegrationTests`, `ODataNotificationIntegrationTests`; OsInterface-Seite: `Playwright/ODataImportTests`, `ODataEndpointExecutionTests`, `PlaywrightODataEndpointServer`.

### F6 – FrameworkReference-Kandidaten

- `Infrastructure/Services/SignalRNotificationService.cs:1,11-13` – `Hub`/`IHubContext<THub>` (via `Microsoft.AspNetCore.SignalR.Core` 1.2.10). Im Shared Framework enthalten.
- `Infrastructure/Services/WindowsCurrentUserService.cs:2,12` – `IHttpContextAccessor` (via `Microsoft.AspNetCore.Http.Abstractions` 2.3.0). Im Shared Framework enthalten.
- `Infrastructure/Services/NotificationPolicy.cs` – keine ASP.NET-Core-Typen (nur Core-Interfaces).
- `Microsoft.AspNetCore.SignalR.Client` (`HubConnection`/`HubConnectionBuilder`) wird faktisch **nicht** in Infrastructure, sondern nur genutzt in:
  - `src/Schnittstellenzentrale/Components/Layout/AppShell.razor:11,55,135,261`
  - `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor:10,100,124,175,262,269,296`
  - Tests: `Components/TopBarTests.cs:126-128` (nur Verweis im Testnamen/Kommentar); `Playwright/Infrastructure/PlaywrightSignalRServer.cs` nutzt **serverseitig** `IHubContext<EndpointHub>` (:29), nicht den Client.
  - Die Referenz steht derzeit in `Schnittstellenzentrale.Infrastructure.csproj:11` und fließt transitiv ins Web-Projekt → Minimalvariante F6 = Verschieben der Zeile ins Web-csproj.

## Konventionen und Test-Infrastruktur

- Testbefehle (`CLAUDE.md`): `dotnet test --filter "Category!=OsInterface"` (blockierend) und `dotnet test --filter "Category=OsInterface"` (best-effort-CI-Job `os-interface-tests`, `continue-on-error`, 45 min). CI fügt `--collect:"XPlat Code Coverage"` nur in der regulären Lane hinzu (70-%-Gate, `pr-staging-ci.yml:146`, `staging-ci.yml:123`); `release.yml:108` nutzt `-p:SkipPlaywrightInstall=true`.
- Testprojekt-Struktur (`src/Schnittstellenzentrale.Tests/`): `Components/` (bUnit), `Integration/` (WebApplicationFactory/`ControllerTestFactory`, Sqlite `:memory:`), `Playwright/` + `Playwright/Infrastructure/`, `Services/` (Moq + gemockter `HttpMessageHandler`), `Helpers/`, `Hubs/`, `Infrastructure/Testing/` (Trait-Attribute + Meta-Tests). 86 Testdateien; Baseline laut Requirement: 641 Tests reguläre Lane.
- resx-Lokalisierung: je Projekt ein resx-Paket (`Resources/SharedResources.*` im Web-, `Resources/CoreResources.*` im Core-Projekt), Schlüsselschema `{Komponente}_{Rolle}` mit Comment; für das Upgrade **nicht funktional relevant**, nur Konvention beim Anfassen von Komponenten.
- Hooks (`.claude/settings.json:89-147`): PostToolUse auf `Edit|Write` laufen 8 Python-Checks (`check_al_labels`, `check_al_xml_comments`, `check_csproj_xmldoc`, `check_razor_usage`, `check_bunit_tests`, `check_ef_entitystate`, `check_css_classes`, `check_razor_l10n`). `check_csproj_xmldoc.py` verlangt bei jeder csproj-Änderung `GenerateDocumentationFile=true` + CS1591 als Fehler und verbietet XML-Doc-Codes in `NoWarn`/`WarningsNotAsErrors` – bei F6/F7-csproj-Änderungen nicht aushebeln. Stop-Hook `test-csharp-startup.ps1` prüft Projekt-Start.
- Datenbank in Tests: alle `WebApplicationFactory`-/Kestrel-Tests gegen **Sqlite In-Memory** (`ControllerTestFactory.cs:35,56`, `PlaywrightApiFactory.cs:63`, `PlaywrightServer.cs:70-72,111`); `EF.SqlServer` ist im Testprojekt referenziert, aber kein `UseSqlServer` im Testcode (produktiv in `Infrastructure/Data/DatabaseProviderFactory.cs:23` + `AppDbContextFactory.cs:24` für LocalDB/Design-Time).

## Wiederverwendbare Bausteine

- `Program.BuildWebApplicationAsync(args, options, configureServices)` (`Program.cs:38`) – einziger Konfigurationspunkt für DI/Middleware; Tests hängen Services über die `configureServices`-Callback ein (Playwright) bzw. `ConfigureTestServices` (Mvc.Testing).
- `Infrastructure/Testing/`: Trait-Attribute + `CategoryTraitDiscoverer` + `TestTraitReader` + Meta-Tests (`TraitParityTests`, `FlakyConventionTests`) – bei F2 komplett gegen die v3-Extensibilität neu zu bauen, aber die *Semantik* (`Category=OsInterface|Flaky`, Meta-Test-Absicherung) ist direkt übertragbar.
- `ControllerTestFactory`/`PlaywrightApiFactory` – WebApplicationFactory-Pattern mit Test-Auth (`TestAuthHandler`), Service-Ersetzung via `RemoveAll`+`AddScoped`.
- `PlaywrightServer`/`PlaywrightTestBase` – Kestrel-Testhost mit Readiness-Poll, benannter Shared-Cache-In-Memory-DB, Trace-Artefakten (`playwright-traces/`); Ports 5099–5104 über Collections serialisiert.
- CI-Composite-Actions: `.github/actions/security-scan` (`dotnet list package --vulnerable` Gate), `.github/actions/build-and-package` (SDK-Setup + win-x64-Publish), `.github/actions/create-review-issue`.
- `scripts/Retry-FlakyTests.ps1`, `Analyze-FlakyTestResults.ps1`, `Download-OsInterfaceTrxHistory.ps1`, `New-FlakyQuarantineIssue.ps1` – Flaky-Retry/Quarantäne-Toolchain der OsInterface-Lane.

## Einschränkungen und Risiken

1. **`Microsoft.AspNetCore.OData` 10.0.0-preview.2 ist net10-only** – unter `net11.0` würde NuGet per Kompatibilitätsregeln auflösen; ob das Paket unter net11 lauffähig ist, muss beim Upgrade verifiziert werden und kann die F5-Entscheidung zeitlich vorgeben (Offene Frage 1 im Requirement).
2. **Edm-Versions-Split:** Infrastructure pinnt `Microsoft.OData.Edm` 8.4.3, läuft transitiv gegen 9.0.0-preview.4 (via AspNetCore.OData preview.2) – binärfragil; Edm 8.4.4 als Minimal-Fallback notiert.
3. **OsInterface-Lane ist best-effort** (`continue-on-error`) – für F1 (Playwright-Update) reicht die CI allein nicht als Absicherung; verbindliches lokales Gate definieren (Offene Frage 4). `InstallPlaywright`-Target installiert die zum Paket passenden Browser – kein separater CI-Schritt, aber Paket-Update ⇒ neue Browser-Version.
4. **xunit.v3-Migration ist die größte Test-Hebel-Stelle:** `OutputType Exe`, `IAsyncLifetime`→`ValueTask`, Wegfall `Xunit.Abstractions`, `Xunit.Sdk`-Trait-Infrastruktur (Discoverer/Attribute) neu, `dotnet test`-/`--filter`-/`coverlet`-Verhalten unter Microsoft.Testing.Platform unklar; `<Using Include="Xunit" />` und 704 Facts/23 Collections betroffen. Empfohlene Sequenz: runner 3.1.5→4.x zuerst.
5. **F3 ist transitiv verzahnt:** `Microsoft.OpenApi` in **zwei** Projekten (Core+Infrastructure), `ISwaggerImportService.SyncFromDocumentAsync(OpenApiDocument)` in der öffentlichen Core-API, `ISwaggerProvider` (Swashbuckle) liefert das Dokument für den Startup-Abgleich (`SystemEndpointSyncService.cs:47`) – getrennte Updates erzeugen einen 2/3-Mix → zwingend ein gemeinsamer PR.
6. **Serilog-Ausfall wäre still:** nach F4-Update Log-Ausgabe gezielt verifizieren (EventLog+File-Sinks aus `appsettings.json`, Bootstrap-Logger in `Program.cs:43`).
7. **SignalR-Paketlinien:** `SignalR.Core` 1.2.10→Shared-Framework-Version ist selbst ein Major-Sprung; `SignalR.Client` fließt heute nur transitiv ins Web-Projekt; bunit-Testprojekt muss bei voller FrameworkReference-Variante auf Verträglichkeit geprüft werden.
8. **Floating-Versionen heben implizit mit:** Beim TFM-Wechsel lösen die 17 `10.0.*` unter net11 ggf. weiterhin 10.x-Pakete auf (kein automatischer Sprung auf 11.x) – die `10.0.*`→`11.0.*`-Erhöhung ist explizit vorzunehmen (F7-abhängig).
9. **SqlServer vs. Sqlite:** Tests laufen ausschließlich gegen Sqlite In-Memory; SqlServer-Pfade (`DatabaseProviderFactory`, `AppDbContextFactory`-LocalDB) werden durch die Suite nicht abgedeckt – EF-10→11-Verhalten dort nur indirekt abgesichert.
10. **Pkcs-Pin-Kommentar ist an `10.0.*` gebunden** – beim Heben auf `11.0.*` muss der Begründungstext (GHSA-Referenzen, „every 10.x release ≥ 10.0.13 is patched") angepasst werden, sonst dokumentiert der Kommentar eine falsche Annahme.

## Verlinkte Detaildateien

Keine separaten Detaildateien nötig – alle Fundstellen sind oben mit Datei:Zeile belegt. Referenzen außerhalb dieses Ordners: `docs/help/stabilitaet/os-interface-tests.md` (OsInterface-/Flaky-Konvention), `CLAUDE.md` (Testkommandos, resx-Konvention), `docs/RELEASE_NOTES.md` (Release-Notes-Format, wird pro Release per semantic-release gefüllt – kein fester Upgrade-Angriffspunkt).
