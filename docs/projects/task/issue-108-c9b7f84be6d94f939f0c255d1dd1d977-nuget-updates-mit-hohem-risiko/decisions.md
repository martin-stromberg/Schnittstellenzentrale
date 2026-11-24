# Architektur- und Prozessentscheidungen – Projekt Issue #108

Dieses Dokument sammelt die im Projektverlauf getroffenen Entscheidungen mit Begründung. Jede Entscheidung wird im zugehörigen Entwicklungsschritt ergänzt.

## F3 – OpenAPI-Stack: Microsoft.OpenApi 3.11.1 + Swashbuckle.AspNetCore 11.0.1 (Schritt 5)

**Status:** Entschieden und umgesetzt (Schritt-Branch `…-schritt-5-openapi-swashbuckle`).

**Entscheidung:**

1. **`Microsoft.OpenApi` 2.10.0 → 3.11.1** in `Schnittstellenzentrale.Core.csproj` und `Schnittstellenzentrale.Infrastructure.csproj` sowie **`Swashbuckle.AspNetCore` 10.1.7 → 11.0.1** in `Schnittstellenzentrale.csproj` — zwingend in einem gemeinsamen Schritt.
   - **Begründung:** Swashbuckle 11 hängt an OpenAPI.NET 3.x; `OpenApiDocument` steht in der öffentlichen Core-Signatur `ISwaggerImportService.SyncFromDocumentAsync` und wird vom `SystemEndpointSyncService` via `ISwaggerProvider.GetSwagger("v1")` bezogen. Ein getrenntes Update hätte einen OpenApi-2/3-Mix erzeugt (v2-Referenz im Importpfad vs. v3-Dokument aus Swashbuckle 11 — binär inkompatible Major-Versionen).
   - **Zielversionen zum Umsetzungszeitpunkt konkretisiert:** 3.11.1 (16.11.2026) bzw. 11.0.1 (12.11.2026) sind die jeweils neuesten stabilen Releases — keine neuere stabile Linie vorhanden. Swashbuckle 11.0.1 deklariert seine OpenAPI-Abhängigkeit auf der 3.11-Linie; ältere 3.x-Stände (≤ 3.10.x) tragen dokumentierte High-CVSS-Befunde — 3.11.1 ist zusätzlich die sauberste Sicherheitswahl.
   - **`OpenApiVersion` bleibt Default (OpenAPI 3.0):** Swashbuckle 11 erzeugt standardmäßig weiter OpenAPI-3.0-Dokumente (3.1/3.2 sind Opt-in via `options.OpenApiVersion`). Kein Opt-in gesetzt — das Akzeptanzkriterium fordert ein gültiges OpenAPI-3-Dokument; verifiziert: `openapi: 3.0.4`.

2. **Einzige erforderliche Code-Migration: `SecurityOperationFilter`.**
   - **Befund aus der realen Abnahme:** `new OpenApiSecuritySchemeReference(schemeId, null)` serialisierte unter 3.x die Security-Requirement als leeres Objekt (`security: [{}]`) statt `{"Negotiate": []}` — Referenzen brauchen in 3.x das Host-Dokument zur Auflösung (dokumentiertes „by design"-Verhalten, dotnet/aspnetcore#64524).
   - **Fix:** `new OpenApiSecuritySchemeReference(schemeId, context.Document)` — `OperationFilterContext.Document` ist die von Swashbuckle 11 neu bereitgestellte Eigenschaft (vgl. Swashbuckle-Beispielfilter `SecurityRequirementsOperationFilter`). Ohne den Fix hätte `SwaggerOperationHelper.DetectAuthenticationType` beim Startup-Abgleich die Negotiate-Erkennung verloren.
   - **Übrige Stellen quellkompatibel:** `OpenApiJsonReader`/`ReadAsync`/`OpenApiReaderSettings` (`SwaggerImportService`), `JsonNodeExtension`, `IOpenApiExtension`, `IOpenApiParameter`, `OpenApiPaths`/`OpenApiPathItem.AddOperation`, `OpenApiSchema` (`JsonSchemaType`, `JsonValue`-Enum/Default), `OpenApiInfo`/`OpenApiSecurityScheme` und die Swashbuckle-Filter-/Provider-Schnittstellen (`IOperationFilter`, `ISwaggerProvider`) sind in 3.x unverändert. Die im Plan antizipierte größere Reader-Migration entfiel — der 2.x→3.x-Umbruch ist laut offiziellem Upgrade-Guide klein; der Bestand nutzte bereits die interface-basierte 2.x-API.
   - **Test-Fixture:** `SystemEntryInitializerTests` setzt die Referenz jetzt gegen das Host-Dokument (produktive Form) statt `null`.
   - **Swashbuckle-11-Breaking-Changes gesondert geprüft:** `anyOf`-Kombinierung mehrfacher Response-Typen pro Statuscode nicht relevant (kein Action mit doppeltem `ProducesResponseType`-Statuscode — Volltextprüfung über alle Controller), Redoc ungenutzt, net11.0-TFM vorhanden, `net8.0`/`net9.0`-Drop irrelevant.

**Betrachtete Alternativen (verworfen):**

- OpenApi-2.x in Core/Infrastructure pinnen und nur Swashbuckle heben: verworfen — erzeugt genau den zu vermeidenden 2/3-Mix (`OpenApiDocument`-Typ inkompatibel zwischen den Major-Versionen).
- Opt-in auf OpenAPI 3.1/3.2-Dokumenterzeugung: verworfen als Scope-Erweiterung — kein Nutzerbedarf; Default 3.0 erfüllt das Akzeptanzkriterium.
- Security-Requirement ohne Referenz als `$ref`-String bauen: verworfen — `context.Document` ist der dokumentierte v11-Weg und hält die Referenz semantisch auflösbar.

**Konsequenzen:**

- `dotnet list package --include-transitive` zeigt `Microsoft.OpenApi` einheitlich 3.11.1 über alle Projekte; `Swashbuckle.AspNetCore.*` einheitlich 11.0.1.
- Reguläre Test-Lane unverändert 641/641 grün; reale Kestrel-Abnahme: `swagger.json` (200, `openapi: 3.0.4`, `x-sz-*`-Extensions, korrekte Security-Requirements) und Swagger-UI (200) verifiziert; der Startup-Abgleich importierte 34 Systemendpunkte inkl. korrekter Negotiate-/BearerToken-Erkennung.
- Bei künftigen OpenAPI.NET-Updates ist zu beachten: Referenztypen brauchen ihr Host-Dokument — Test-Fixtures, die Dokumente im Speicher bauen, müssen Referenzen mit dem Dokument verknüpfen, sonst testen sie ein unproduktives Modell.

## F5 – OData-Stack: Edm 9.0.0 + AspNetCore.OData-Strategie (Schritt 3)

**Status:** Entschieden und umgesetzt (Schritt-Branch `…-schritt-3-odata-edm`).

**Entscheidung:**

1. **Microsoft.AspNetCore.OData: auf `10.0.0-preview.2` verharren** (dokumentierte Annahme).
   - **Begründung:** Es existiert kein stabiler 10.x-/11.x-Release von `Microsoft.AspNetCore.OData` (neueste Version zum Entscheidungszeitpunkt: `10.0.0-preview.2`; letzter stabiler Strang ist 9.x für net8.0). `10.0.0-preview.2` läuft unter net11 bereits seit Schritt 1 verifiziert (78/78 `ODataControllerIntegrationTests` grün). Ein Preview-Tracking auf neue Preview-Stände ohne Zielrelease (unbekannter Stabilisierungszeitpunkt) würde das Risiko nur erhöhen, ohne einen Zielzustand zu erreichen.
   - **Exit-Kriterium:** Umstieg auf die stabile Linie, sobald ein stabiler 10.x- oder 11.x-Release von `Microsoft.AspNetCore.OData` verfügbar ist — dann als eigener Schritt mit voller OData-Testverifikation.

2. **Microsoft.OData.Edm 8.4.3 → 9.0.0** (direkte Referenz in `Schnittstellenzentrale.Infrastructure.csproj`). Damit ist die Paketanforderung von `Microsoft.AspNetCore.OData 10.0.0-preview.2` (`>= 9.0.0-preview.4`) erfüllt und der binärfragile Versions-Split aufgehoben (vorher: kompiliert gegen 8.4.3, gelaufen transitiv gegen 9.0.0-preview.4). `dotnet list package --include-transitive` zeigt `Microsoft.OData.Edm` in allen Projekten einheitlich `9.0.0`.

3. **Ergänzend (dokumentierte Annahme, technisch erforderlich):** direkte Referenzen `Microsoft.OData.Core 9.0.0` und `Microsoft.OData.ModelBuilder 3.0.0` in `Schnittstellenzentrale.csproj`.
   - **Begründung:** `Microsoft.OData.Core 9.0.0-preview.4` pinnt `Microsoft.OData.Edm` **exakt** auf `= 9.0.0-preview.4` — ohne die Core-Hebung erzeugte die Edm-9.0.0-Referenz NU1608-Warnungen (Web- und Testprojekt) und einen Rest-Versions-Split im ODL-Stack. Die stabilen Releases erfüllen die von `AspNetCore.OData 10.0.0-preview.2` deklarierten Mindestbereiche (`Core`/`Edm`/`Spatial` `[9.0.0-preview.4, 10.0.0)`, `ModelBuilder` `[3.0.0-preview.1, 4.0.0)`). `ModelBuilder` wird außerdem direkt im Code genutzt (`OData/ODataEdmModelBuilder.cs`) — eine direkte Referenz ist ohnehin semantisch korrekt. Ergebnis: kompletter ODL-Stack (`Edm`, `Core`, `Spatial` 9.0.0, `ModelBuilder` 3.0.0) auf der stabilen Linie; einzig `AspNetCore.OData` bleibt Preview (Strategie siehe Punkt 1).

**Betrachtete Alternativen (verworfen):**

- Edm auf `9.0.0-preview.4` pinnen (Deck-Gleichheit mit der Core-Anforderung): würde das Issue-Ziel „stabile 9.0.0" verfehlen und den Preview-Status in den direkt referenzierten Code tragen.
- Fallback `8.4.4`: nur vorgesehen, wenn 9.0.0 konkrete Probleme verursacht — nicht eingetreten (Build sauber, Edm-9-API kompatibel: `CsdlReader.Parse`, `IEdmModel`/`IEdmEntitySet`/`IEdmOperation`/`IEdmAction`, Vocabulary-Annotations-API unverändert nutzbar).
- AspNetCore.OData-Downgrade auf stabilen 9.x-Strang (net8.0): verworfen — würde den seit Schritt 1 verifizierten net11-Pfad verlassen und OData-API-Downgrades riskieren.

**Konsequenzen:**

- `ODataImportService` (Infrastructure) und `ODataEdmModelBuilder` (Web) kompilieren und laufen gegen dieselbe Edm-Version — kein Laufzeit-/Kompilierzeit-Split mehr.
- Der ODL-Stack ist bis auf das Host-Paket vollständig stabil; Versionsfolge von `Core`/`Edm`/`Spatial`/`ModelBuilder` liegt jetzt bei direkten, prüfbaren Referenzen statt versteckt in transitiven Preview-Auflösungen.
- Beim Erreichen des Exit-Kriteriums ist zu prüfen, ob die ODL-Direktreferenzen weiter gepflegt oder dem `AspNetCore.OData`-Release folgen.
- `Microsoft.Spatial` bleibt transitiv (kein direkter Code-Bezug) und löst als `9.0.0` auf.

## F6 – ASP.NET-Core-Bezug in `Schnittstellenzentrale.Infrastructure` (Schritt 2)

**Status:** Entschieden und umgesetzt (Schritt-Branch `…-schritt-2-frameworkref`).

**Entscheidung:** Volle `FrameworkReference`-Variante. `Schnittstellenzentrale.Infrastructure.csproj` erhält `<FrameworkReference Include="Microsoft.AspNetCore.App" />`; die PackageReferences `Microsoft.AspNetCore.Http.Abstractions 2.3.0` und `Microsoft.AspNetCore.SignalR.Core 1.2.10` entfallen. Zusätzlich – unabhängig von der Variante verbindlich – wird `Microsoft.AspNetCore.SignalR.Client 11.0.*` aus `Infrastructure` ins Web-Projekt `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj` verschoben.

**Begründung:**

- Die in `Infrastructure` tatsächlich genutzten ASP.NET-Core-APIs liegen vollständig im Shared Framework: `Hub` und `IHubContext<THub>` (`SignalRNotificationService<THub>`, einzige `Microsoft.AspNetCore.SignalR`-Nutzung) sowie `IHttpContextAccessor` (`WindowsCurrentUserService`, einzige `Microsoft.AspNetCore.Http`-Nutzung). Per Volltextsuche verifiziert: keine weiteren `Microsoft.AspNetCore.*`-usings in `Infrastructure`, keine in `Core`.
- Zwei Legacy-Paketlinien entfallen: `Http.Abstractions 2.3.0` (.NET-Core-2.x-Linie, wird nicht mehr separat gepflegt) und `SignalR.Core 1.2.10` (1.x-Linie). Der ohnehin fällige faktische Major-Sprung `SignalR.Core` 1.2.10 → Shared-Framework-Version (11.0.x) wird sauber auf die Framework-Version vollzogen statt als verwaiste Paketlinie weiterverwaltet.
- `Microsoft.AspNetCore.SignalR.Client` ist **nicht** Teil des Shared Frameworks und wird produktiv ausschließlich im Web-Projekt genutzt (`Components/Layout/AppShell.razor`, `Components/Shared/ApplicationGroupTree.razor`). Die Referenz gehört daher dorthin, wo sie gebraucht wird — `Infrastructure` hatte sie nur historisch mitgeschleppt.
- Das Testprojekt (`Microsoft.NET.Sdk`, bunit) erhält `Microsoft.AspNetCore.App` transitiv über die `ProjectReference`n (FrameworkReference ist transitiv; das Web-SDK-Projekt liefert sie ohnehin implizit mit) und `SignalR.Client` transitiv über die Web-Referenz — bunit-Verträglichkeit wurde per Build und kompletter regulärer Testlane verifiziert.

**Betrachtete Alternative (verworfen):** Minimalvariante — nur `SignalR.Client` verschieben und die beiden Legacy-Pakete in `Infrastructure` belassen. Verworfen, weil sie die verwaisten Paketlinien (`Http.Abstractions 2.3.0`, `SignalR.Core 1.2.10`) konserviert und den fälligen SignalR-Major-Sprung weiter aufschiebt; das Restrisiko der vollen Variante (bunit-/Testprojekt-Verträglichkeit) ist gering und wurde im Schritt verifiziert.

**Konsequenzen:**

- `Infrastructure` hängt zur Laufzeit an der ASP.NET-Core-Shared-Framework-Installation (bei Blazor-Server-Einsatz ohnehin immer gegeben); als class library ändert sich am Kompilat nichts.
- Die SignalR-Server-Version in `Infrastructure` folgt künftig der Runtime statt einer pinnenbaren Paketversion — gewollt, da das Hosting-Projekt dieselbe Runtime vorgibt.
- Security-Update-Pfad für diese APIs läuft über Runtime-/SDK-Patches statt über NuGet-Paketupdates — passt zur Floating-/Patch-Strategie der übrigen Framework-Pakete.
- **Folgebereinigung (NU1510):** Mit der `FrameworkReference` wurden außerdem die bis dahin direkt referenzierten, nun vollständig im Shared Framework enthaltenen Pakete `Microsoft.Extensions.Configuration.Abstractions`, `Microsoft.Extensions.Configuration.Binder`, `Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.Http`, `Microsoft.JSInterop` und `System.Security.Cryptography.Pkcs` als redundant gemeldet und entfernt — sonst blieben dauerhaft NU1510-Warnungen im Build. Zur Pkcs-Sicherheitslage (GHSA-846f-f764-76mx, GHSA-2fmv-rv2c-59x7): Die ursprüngliche Pinning-Begründung entfällt — Pkcs kommt jetzt aus der Runtime-Installation und bleibt über deren Patches auf der gepatchten Linie; die transitive `Microsoft.EntityFrameworkCore.SqlServer`-Anforderung wird auf das Framework-Asset gekürzt.

## F4 – Serilog-Stack: `Serilog.AspNetCore` 11.0.0, Sinks unverändert (Schritt 4)

**Status:** Entschieden und umgesetzt (Schritt-Branch `…-schritt-4-serilog`).

**Entscheidung:**

1. **`Serilog.AspNetCore` 10.0.0 → 11.0.0** in `Schnittstellenzentrale.csproj`.
   - **Changelog-Bewertung 11.0.0** (GitHub-Release vom 18.11.2026): Der Release fügt den net11.0-TFM hinzu (net8.0/net9.0 entfallen — irrelevant, das Projekt läuft auf net11.0), hebt den Serilog-Kern auf 4.4.0 und alle `Serilog.*`-Satelliten auf die v11-Linie (`Extensions.Hosting`, `Extensions.Logging`, `Settings.Configuration` je 11.0.0). Die einzige Verhaltensänderung — das Request-Logging (`UseSerilogRequestLogging`) unterdrückt Exceptions bei client-seitig abgebrochenen Requests — ist für diese Anwendung **ohne Wirkung**, weil die Middleware nicht genutzt wird (per Volltextsuche verifiziert). Kein API-Bruch auf den genutzten Pfaden (`LoggerConfiguration().ReadFrom.Configuration`, `builder.Host.UseSerilog()`, statisches `Log.Logger`); der Bootstrap-Logger-Pfad in `Program.cs` und der `Serilog`-Abschnitt in `appsettings.json` bleiben unverändert.

2. **Sinks bleiben unverändert** (dokumentierte Annahme): `Serilog.Sinks.File` 7.0.0 und `Serilog.Sinks.EventLog` 4.0.0.
   - **Begründung:** `Sinks.File` 7.0.0 ist die von `Serilog.AspNetCore` 11.0.0 selbst deklarierte Abhängigkeit (`>= 7.0.0`) — Kompatibilität per Paketdeklaration; kein neuerer stabiler Stand verfügbar (nur Prerelease). `Sinks.EventLog` 4.0.0 ist letzter stabiler Stand, targetet net6.0/netstandard2.0 und löst unter net11.0 via Nearest-TFM auf; die Serilog-4.x-Abhängigkeit ist major-kompatibel zu 4.4.0. Ein Mitheben entfällt — kein technischer Anlass.
   - **Verifiziert:** Reale E2E-Abnahme unter Kestrel — `logs/log-20261124.txt` schreibt Request-/Hub-Events, und der EventLog-Sink schreibt auf diesem Windows-Host tatsächlich Einträge unter Provider `Schnittstellenzentrale` ins Anwendungsprotokoll (Aussage stärker als das geforderte „Konfiguration wird geladen").

**Betrachtete Alternativen (verworfen):**

- Sinks vorsorglich auf Prerelease-Stände heben: verworfen — ohne konkreten Kompatibilitätsbefund würde das Prerelease-Risiko ohne Gegenwert eingeführt.
- `manageEventSource: true` in der EventLog-Konfiguration ergänzen: verworfen als Scope-Erweiterung — seit Sink-v3.0 Default `false` (Source muss auf dem Zielhost registriert sein; auf dem Abnahme-Host ist sie es), das Bestandsverhalten bleibt bewusst unverändert.

**Konsequenzen:**

- Der Serilog-Stack folgt jetzt der net11-Linie des Hostings; künftige Sink-Updates können unabhängig vom Hostpaket erfolgen (Sinks hängen nur am Serilog-Kern).
- Nachweisbarkeit des Loggings ist jetzt Teil der Schritt-Abnahme dokumentiert: File-Sink und EventLog-Sink schreiben real — bei künftigen Serilog-Updates ist die reale Kestrel-Abnahme (Datei- und EventLog-Prüfung) zu wiederholen.

## F1 – Playwright-Stack: `Microsoft.Playwright` 1.65.1 (Schritt 7)

**Status:** Entschieden und umgesetzt (Schritt-Branch `…-schritt-7-playwright`).

**Entscheidung:**

1. **`Microsoft.Playwright` 1.52.0 → 1.65.1** in `Schnittstellenzentrale.Tests.csproj` — einzige Code-Änderung des Schritts.
   - **Release-Notes-Sichtung 1.53–1.65.1 vollständig:** Breaking Changes in 1.55 (Chromium-Extension-MV2), 1.56 (`BrowserContext.BackgroundPage`), 1.57 (`Page.Accessibility` entfernt; Wechsel Chromium → Chrome for Testing), 1.58 (`_react`/`_vue`/`:light`-Selektoren, `Devtools`-Option), 1.59/1.60 (macOS-WebKit, `Handle`-Option `ExposeBindingAsync`, Docker-Image), 1.64 (Elemente in versteckten iframes gelten als hidden), 1.65 (Aktionen warten auf nicht-inerte Elemente; Cookie-Matching RFC 6265; ungerenderter Text fällt aus Accessible Names; `ParamsString` angehängt statt ersetzt; Proxy-Credentials nur an Proxy-Challenges).
   - **Befund: keine der entfernten/geänderten APIs oder Verhaltensmuster wird im Repo verwendet** (`BackgroundPage`, `Accessibility`, `_react`, `_vue`, `:light`, `Devtools`, `ExposeBinding`, `APIRequestContext`, `ParamsString`, `AriaSnapshot`, `Cookie`, `iframe`, `inert` → jeweils 0 Treffer; ~60 `GetByRole`-Locators mit `Name` matchen gerenderten Text; einziges `<details>` in `ActivityLogPanel.razor` wird nicht per Role-Name adressiert). Die vollständige Release-Tabelle liegt im Schritt-`inventory/release-notes-befund.md` (in der Git-Historie des Planungscommits erhalten).
   - **Empirisch bestätigt:** OsInterface-Lane 94/94 grün unter Chromium 159.0.8154.6 (Chrome for Testing, Revision 1255) — keine Test-/Infrastrukturanpassung nötig (`PlaywrightTestBase`, `PlaywrightServer`, `PlaywrightApiFactory`, `PlaywrightSignalRServer` unverändert).

2. **Browser-Installation läuft weiterhin über das `InstallPlaywright`-MSBuild-Target** — kein dedizierter Install-Schritt nötig; die paketkonforme Revision landet in `%LOCALAPPDATA%/ms-playwright/`. **CI-Cache-Befund:** Es existiert kein `actions/cache` für `ms-playwright` in den Workflows — nichts zu invalidieren; jeder CI-Build installiert die paketkonforme Revision frisch.

3. **Lokales Gate als Merge-Pflicht (Prozessentscheidung):** Da die OsInterface-CI-Lane best-effort (`continue-on-error`) läuft, gilt für Playwright-Updates verbindlich: `dotnet test --filter "Category=OsInterface"` muss **lokal vollständig grün** gelaufen sein, bevor der PR gemergt wird; Ergebnis/TRX werden im PR verlinkt. Für diesen Schritt nachgewiesen: 94/94 grün (`test-results-os-interface.trx`).

**Betrachtete Alternativen (verworfen):**

- Testcode vorsorglich auf neue APIs (z. B. `Locator.Visible`, `FrameLocator()` ohne Selektor) umstellen: verworfen als Scope-Erweiterung — die bestehenden Aufrufe sind quell- und laufzeitkompatibel.
- `[Flaky]`-Markierungen vorsorglich vergeben: verworfen — Repo-Konvention erlaubt `Flaky` nur bei dokumentiertem Flake-Ereignis; der Lauf war ohne Einzelfehlschlag.

**Konsequenzen:**

- Die E2E-Suite läuft auf der aktuellen Chrome-for-Testing-Linie; künftige Playwright-Major-Updates folgen demselben Verfahren (Release-Notes-Sichtung + lokaler OsInterface-Gate-Lauf vor Merge).
- Bei künftigem Einsatz von `inert`, Cookies oder iframes in der App sind die 1.64/1.65-Verhaltensänderungen zu berücksichtigen.

## F2 – Testprojekt: `xunit` 2.9.3 → `xunit.v3` 4.0.3 (Schritt 8)

**Status:** Entschieden und umgesetzt (Schritt-Branch `…-schritt-8-xunit-v3`).

**Entscheidung:**

1. **`xunit` → `xunit.v3` 4.0.3** (letztes stabiles Release; installiert transitiv `xunit.v3.mtp-v2` = Microsoft.Testing.Platform-Runner). Das Testprojekt wird zur selbst laufenden MTP-App: `OutputType=Exe`, `xunit.abstractions` entfällt. `xunit.runner.visualstudio` 4.0.1 und `Microsoft.NET.Test.Sdk` 18.5.1 bleiben bewusst referenziert (IDE-Test-Discovery).
   - **MTP-CLI:** `dotnet test` läuft über `global.json` (`test.runner: Microsoft.Testing.Platform`) im MTP-Modus; Filter/Reporter werden als Test-App-Argumente hinter `--` übergeben. `--filter` behält die VSTest-Syntax unverändert (empirisch: 641/94-Selektion und Compound-Filter identisch). TRX via `Microsoft.Testing.Extensions.TrxReport` (`--report-trx`), Coverage via `Microsoft.Testing.Extensions.CodeCoverage` 18.12.0 (`--coverage --coverage-output-format cobertura`) — ersetzt `coverlet.collector`/`--collect:"XPlat Code Coverage"` (VSTest-Collector läuft unter MTP nicht).
   - **API-Migration:** `IAsyncLifetime` nutzt `ValueTask` (betraf `PlaywrightServer` + Overrides, `PlaywrightTestBase`, `EndpointExecutionIntegrationTests`); `ITestOutputHelper` liegt im `Xunit`-Namespace (`using Xunit.Abstractions;` entfernt).
   - **Traits:** v2-`ITraitDiscoverer`/`Xunit.Sdk` existieren nicht mehr → `CategoryTraitDiscoverer.cs` gelöscht; `[OsInterface]`/`[Flaky]`/`[OsInterfaceFact]`/`[OsInterfaceTheory]` implementieren `Xunit.v3.ITraitAttribute` und liefern `("Category", wert)` selbst via `GetTraits()`. `TestTraitReader` liest rohe `[Trait]`-Attribute per `CustomAttributeData` (inkl. Vererbungskette) und `ITraitAttribute`-Instanzen — beide dokumentierten Schreibweisen funktionieren weiter.
   - **CI/Skripte:** Workflows `staging-ci.yml`, `pr-staging-ci.yml`, `release.yml` sowie `TestDiscovery.ps1`/`Retry-FlakyTests.ps1` auf die `--`-Argumenttrennung umgestellt; `--results-directory` überall explizit, wo TRX-Pfade konsumiert werden. MTP-Exitcode 8 bei leerer Filterauswahl ist für die Discovery belanglos.

2. **Empirischer Nachweis:** Reguläre Lane 641/641 grün, OS-Schnittstellen-Lane 94/94 grün (lokales Gate), Cobertura-Coverage 72,5 % ≥ 70-%-Gate, `dotnet list package --vulnerable` sauber — siehe Schritt-`test-results.md`.

**Betrachtete Alternativen (verworfen):**

- Auf xunit 2.9.3 bleiben und nur MTP-Extensions nachrüsten: verworfen — F2 fordert die v3-Migration, und die v2-Trait-Discoverer hätten ohnehin ersetzt werden müssen, sobald v3 folgt.
- Testfilter auf xunit.v3-eigene Selektoren (`-filter`-Query-Language) umstellen: verworfen — die VSTest-Syntax läuft unter xunit.v3 unverändert und hält CI/Skripte/Doku diff-minimal.
- `Microsoft.NET.Test.Sdk`/`xunit.runner.visualstudio` entfernen: verworfen — beide sichern die IDE-Test-Discovery (VS/Rider/Test Explorer); der MTP-Modus bricht damit nicht.

**Konsequenzen:**

- Alle `dotnet test`-Aufrufe (lokal, CI, Skripte, Doku) nutzen die `--`-Trennung; Reporter-Artefakte brauchen `--report-trx`/`--coverage`-Argumente plus explizites `--results-directory`.
- Neue Traits/Kategorien werden künftig als `Xunit.v3.ITraitAttribute` implementiert — kein Discoverer-Muster mehr.
- Künftige xunit-Updates folgen der v3-Linie; für MTP-Erweiterungen (Reporter) gelten eigene Versionslinien (`Microsoft.Testing.Extensions.*`).

## F7 – Update-Prozessstrategie: Floating `11.0.*` beibehalten + regelmäßige Paketprüfung (Schritt 9)

**Status:** Entschieden (Schritt-Branch `…-schritt-9-abschluss`) — reine Prozessentscheidung, keine Code-/Manifeständerung.

**Entscheidung:** Variante (a) — die 11 Floating-Referenzen `11.0.*` (3 × Web, 4 × Infrastructure, 4 × Tests; `Core` hat keine Floating-Referenz) bleiben unverändert; die Update-Disziplin läuft weiter über die etablierte regelmäßige Paketprüfung. Es werden **keine** `packages.lock.json` und **kein** `Directory.Packages.props` eingeführt.

**Begründung:**

- **Floating ist bewusst gewählt.** Security-Patches sollen implizit einfließen — ursprünglich im `System.Security.Cryptography.Pkcs`-Kommentar der `Infrastructure.csproj` begründet (`10.0.*` wegen GHSA-846f-f764-76mx / GHSA-2fmv-rv2c-59x7). Seit F6 kommt Pkcs aus der Runtime-Installation (NU1510-Folgebereinigung); die Strategie gilt unverändert für die verbliebenen `11.0.*`-Referenzen: Framework-/EF-Patches fließen ohne Manifeständerung beim nächsten Restore mit.
- **„Floating + Lock" ist kein sinnvoller Mittelweg.** Bei Floating-Versionen löst eine Lock-Datei bei jedem Restore neu auf — sie liefert dann keine Reproduzierbarkeit, sondern nur Pflegeoverhead. Wer Lock-Reproduzierbarkeit will, muss zuerst pinnen — das ist Variante (b), die verworfen ist.
- **Die Paketprüfungs-Praxis trägt das Risiko bereits.** Der automatisierte `weekly-nuget-review`-Workflow erzeugt wöchentlich ein Review-Issue (Label `automated-nuget-review`, Duplikat-Schutz); die Issue-#107/#108-Kette ist die konkrete Ausprägung dieser Praxis — Major-/Risiko-Updates werden gebündelt, entschieden und schrittweise mit eigener Verifikation umgesetzt. Ein zusätzlicher Lock-/Bot-Mechanismus adressiert ein Problem, das die bestehende Kadenz bereits abdeckt.

**Betrachtete Alternativen (verworfen):**

- **(b) Pinnen + `packages.lock.json` im Locked-Mode + botgestützte Update-PRs** (Dependabot/Renovate): verworfen — Reproduzierbarkeit und nachverfolgbare Update-Diffs werden mit neuer Bot-/Review-Infrastruktur erkauft; das konterkariert die bewusst gewählte implizite Security-Patch-Zufuhr und liegt außerhalb des Issue-Scopes. Bei künftigem Reproduzierbarkeits-Bedarf (z. B. Audit-/Compliance-Anforderung) ist der Umstieg jederzeit nachrüstbar.
- **(c) Central Package Management via `Directory.Packages.props`**: verworfen — zentrale Versionswahrheit bringt bei 4 Projekten und überschaubarer Referenzmenge keinen Reproduzierbarkeits-Gewinn; als „Zwischenschritt" ohne Zielentscheidung wäre es Mehraufwand ohne Gegenwert.
- **Floating + Lock-Kombination**: verworfen — kein sinnvoller Mittelweg (siehe Begründung).

**Konsequenzen:**

- Der Restore löst die `11.0.*`-Referenzen auf die jeweils neuesten 11.0.x-Patches auf; Builds auf unterschiedlichen Ständen können unterschiedliche Patch-Stände einspielen — akzeptiert, da Framework-Patches kompatibel sind und die wöchentliche Prüfung Abweichungen sichtbar macht.
- `dotnet list package --vulnerable` bleibt Teil der Update-/Schritt-Abnahmen; Security-relevante Patches werden weiterhin explizit bewertet (Kette #107/#108), zusätzlich zur impliziten Patch-Zufuhr.
- Re-Evaluation zulässig und billig: Bei verändertem Risikobild (Reproduzierbarkeits-Anforderung, Bot-Infrastruktur ohnehin geplant) kann die Entscheidung ohne Altlasten revidiert werden — es wurde nichts eingeführt, was zurückgebaut werden müsste.

## F8 – Auslagerung: npm-devDependencies + GitHub-Actions-Pins (Schritt 9)

**Status:** Entschieden und umgesetzt (Schritt-Branch `…-schritt-9-abschluss`) — Folge-Issue angelegt.

**Entscheidung:** Die beiden Nicht-NuGet-Abhängigkeitsbereiche werden **nicht** in Issue #108 bearbeitet, sondern in das separate Folge-Issue **[#110](https://github.com/martin-stromberg/Schnittstellenzentrale/issues/110)** ausgelagert — mit empfohlener eigener Review-Kadenz (Vorschlag: monatlich, analog zum `monthly-architecture-security-review`-Muster, statt der wöchentlichen NuGet-Kadenz).

**Scope des Folge-Issues:**

- **npm-`devDependencies`** der Release-Automatisierung (`package.json`, Root): `semantic-release` ^25.0.5, `@semantic-release/commit-analyzer` ^13.0.1, `@semantic-release/github` ^11.0.6, `@semantic-release/release-notes-generator` ^14.0.3, `conventional-changelog-conventionalcommits` ^9.1.0.
- **GitHub-Actions-Pins:** 11 Workflows + 3 Composite Actions unter `.github/` — alle externen `uses:` als Major-Tag-Pins (`@v5`–`@v7`), keine SHA-Pins, keine Dependabot-/Renovate-Konfiguration.

**Begründung:**

- Beide Bereiche liegen **außerhalb des NuGet-Scopes** von Issue #108 (NuGet-Paketprüfung); die Aufnahme hier wäre Scope-Creep.
- Anderer Technologie-/Werkzeugkreis (npm-Ökosystem, Actions-Marketplace) mit eigenen Prüfwerkzeugen und eigenem Änderungsrhythmus — ein separater Review-Zyklus ist fachlich sauberer und verhindert, dass die wöchentliche NuGet-Kadenz auf einen kleinen, selten ändernden Scope überdimensioniert wirkt.
- Issue #108 wird dadurch entscheidungsseitig abschließbar, ohne die Befunde zu verlieren.

**Konsequenzen:**

- Issue #108 ist mit diesem Schritt fachlich abgeschlossen; die Umsetzung von #110 ist eigenständig und blockiert den #108-Abschluss nicht.
- In #110 ist zu entscheiden, ob die Prüfung manuell-periodisch bleibt oder per Bot/Workflow automatisiert wird (Dependabot-/Renovate-Frage für `github-actions` + `npm` gehört dorthin — nicht zu F7, das nur NuGet betrifft).
