# Projektplan – .NET-11-Upgrade + NuGet-Updates mit hohem Risiko (Issue #108)

## Übersicht

**Projekttitel:** .NET-11-Upgrade der Lösung und strukturierte Abarbeitung der aufgeschobenen NuGet-/Architektur-Befunde aus GitHub-Issue #108 (martin-stromberg/Schnittstellenzentrale).

**Gesamtziel:** Die Lösung (4 Projekte: `Schnittstellenzentrale` Web, `.Core`, `.Infrastructure`, `.Tests`) wird von `net10.0` auf `net11.0` gehoben (SDK 11.0.100 ist installiert; alle benötigten Microsoft.*-11.0.0-Pakete existieren). Danach werden die Issue-Befunde F1–F8 in getrennten, eigenständig verifizierbaren Schritten abgearbeitet: Playwright 1.52→1.65.1 (F1), xunit.v3-Migration (F2), koordiniertes Microsoft.OpenApi-3.x-/Swashbuckle-11-Update (F3), Serilog.AspNetCore 11.0.0 (F4), Microsoft.OData.Edm 9.0.0 + AspNetCore.OData-Strategie (F5), FrameworkReference-/SignalR.Client-Entscheidung (F6), Floating-vs.-Lock/CPM-Prozessentscheidung (F7) und Auslagerung der Nicht-NuGet-Punkte (F8). Jeder Schritt ist ein eigener PR gegen `staging`; der letzte PR trägt `Closes #108`.

**Betroffene Bereiche:** alle 4 `.csproj` (TFM + 41 PackageReferences, davon 17 Floating `10.0.*`), `global.json`, 9 CI-Pins in `.github/workflows` + `.github/actions/build-and-package`, OpenApi-/OData-/Serilog-/SignalR-Codepfade, die komplette Testinfrastruktur (Trait-System, Playwright, xunit.v3/MTP), `CLAUDE.md` und Hilfsdokumentation. Entscheidungen werden in `docs/projects/task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko/decisions.md` (neu anzulegen) und als Kommentar im Issue #108 dokumentiert.

## Grobe Vorgehensentscheidungen

1. **.NET-11-Basisupgrade zuerst (Schritt 1).** TFM `net11.0`, `global.json` → SDK 11.x, alle 9 CI-Pins → `11.0.x`, die 17 Floating-Referenzen `10.0.*` → `11.0.*` und harte net10-Nennungen in Workflows/Docs/Kommentaren werden in einem einzigen, eigenständig verifizierbaren Schritt gehoben. Begründung: Alle Folgebefunde sollen direkt auf der neuen Baseline angepasst und getestet werden; ein späterer TFM-Wechsel würde jede vorher erledigte Migration doppelt verifizieren müssen. Dabei gilt die **Arbeitsannahme „Floating beibehalten"** für F7 (begründeter Default: Das Floating-Modell ist im `System.Security.Cryptography.Pkcs`-Kommentar bereits bewusst begründet — GHSA-846f-f764-76mx / GHSA-2fmv-rv2c-59x7; Security-Patches sollen weiter implizit einfließen). F7 muss die Float-Strategie daher *nicht* vorher festlegen und läuft als reine Entscheidungsdokumentation nachgelagert in Schritt 9 — sollte entgegen dem Default gepinnt/CPM gewählt werden, ist die Nacharbeit dort zu leisten.
2. **`Microsoft.AspNetCore.OData` bleibt fürs Erste gepinnt auf `10.0.0-preview.2`** (net10-only-Paket; kein stabiles 10.x/11.x-Release existiert). NuGet löst es unter `net11.0` per TFM-Kompatibilität auf; die Lauffähigkeit wird bereits in Schritt 1 über die 78 `ODataControllerIntegrationTests` verifiziert. Die förmliche F5-Strategieentscheidung (inkl. Exit-Kriterium) folgt dokumentiert in Schritt 3 zusammen mit dem Edm-Update.
3. **Kleine Entscheidungs-+Umsetzungsschritte früh (Schritte 2–5).** F6 (SignalR.Client verschieben + FrameworkReference-Entscheidung), F5 (Edm 9.0.0 + OData-Strategie), F4 (Serilog, klein und unabhängig) und F3 (OpenApi 3.x + Swashbuckle 11 — **zwingend ein gemeinsamer Schritt**, da `Microsoft.OpenApi` in Core *und* Infrastructure referenziert ist und `OpenApiDocument` in der öffentlichen Signatur `ISwaggerImportService.SyncFromDocumentAsync` steht; ein 2/3-Mix ist nicht baubar) laufen auf der net11-Baseline als getrennte PRs, bevor die Testinfrastruktur angefasst wird.
4. **Testinfrastruktur strikt sequenziell und zuletzt (Schritte 6–8).** Damit die absichernden Tests nicht gleichzeitig aus zwei Richtungen verändert werden: erst `xunit.runner.visualstudio` 3.1.5 → 4.0.1 als risikoarmer Zwischenschritt, dann F1 (Playwright 1.65.1, ~400 Locator-/Action-Aufrufe), zuletzt F2 (xunit.v3-Migration als größter Hebel: `OutputType Exe`, `IAsyncLifetime`→`ValueTask`, v2-spezifische `Xunit.Sdk`-Trait-Infrastruktur neu bauen). Die dokumentierte `Category=OsInterface`-Filterung (`dotnet test --filter`, CLAUDE.md, CI) darf zu keinem Zeitpunkt brechen.
5. **Entscheidungen als dokumentierte Schritte mit begründetem Default.** F5/F6/F7 werden jeweils als „Entscheidung mit Begründung dokumentiert in `decisions.md` (+ Issue-Kommentar) + umgesetzte Minimaländerung" formuliert. Antwortet der Anwender nicht, gilt der im jeweiligen Schritt genannte Default. F8 wird nicht implementiert, sondern als separates-Issue-Entscheidung dokumentiert (Schritt 9).

## Entwicklungsschritte

### Schritt 1: .NET-11-Basisupgrade (TFM, SDK, CI, Floating-Baseline)

**Ziel/Umfang:** Alle 4 Projekte (`src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`, `src/Schnittstellenzentrale.Core/`, `src/Schnittstellenzentrale.Infrastructure/`, `src/Schnittstellenzentrale.Tests/`) von `net10.0` auf `net11.0` heben; `global.json` SDK `10.0.301` → `11.0.100` (`rollForward: latestFeature` beibehalten); alle 9 `dotnet-version: '10.0.x'`-Pins auf `'11.0.x'` (`.github/workflows/pr-staging-ci.yml`, `staging-ci.yml`, `security-scan.yml`, `release.yml` und `.github/actions/build-and-package/action.yml` — Fundstellen siehe `docs/projects/task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko/inventory.md`); den harten `--framework net10.0`-Parameter in `build-and-package/action.yml` auf `net11.0` anpassen. Alle 17 Floating-Referenzen `10.0.*` explizit auf `11.0.*` heben (Arbeitsannahme F7 = Floating beibehalten; Floatings lösen unter net11 *nicht* automatisch auf 11.x auf) und dabei den `System.Security.Cryptography.Pkcs`-Pin-Kommentar (`Infrastructure.csproj:28-37`, GHSA-Begründung) auf die 11.x-Linie anpassen sowie den AngleSharp-Override-Kommentar prüfen. Harte net10-Nennungen korrigieren: `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightTestBase.cs:27` (Kommentar `bin/<cfg>/net10.0/`), `README.md`, `docs/help/schnittstellenzentrale/installation.md`, `docs/help/playwright-tests/installation.md`. `Microsoft.AspNetCore.OData 10.0.0-preview.2` bleibt gepinnt — es ist net10-only, wird unter net11.0 per TFM-Kompatibilität aufgelöst; seine Lauffähigkeit ist Teil der Verifikation dieses Schritts.

**Akzeptanzkriterien:** `dotnet build` der `Schnittstellenzentrale.slnx` unter SDK 11.0.100 fehlerfrei; `dotnet test --filter "Category!=OsInterface"` vollständig grün (Baseline: 641 Tests); die 78 `ODataControllerIntegrationTests` grün (Verifikation preview.2 unter net11); `dotnet list package --vulnerable` ohne Befund; keine verbleibenden `10.0.*`-/`net10.0`-Nennungen außerhalb historischer Dokumente; PR gegen `staging`.

**Abhängigkeiten:** keine.

**Betroffene Bereiche:** alle `.csproj`, `global.json`, `.github/workflows/*`, `.github/actions/build-and-package`, Testprojekt-Kommentar, README, Hilfsdoku.

### Schritt 2: F6 – SignalR.Client ins Web-Projekt verschieben + FrameworkReference-Entscheidung

**Ziel/Umfang:** Architektur-Entscheidung mit Begründung in `docs/projects/task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko/decisions.md` dokumentiert (+ kurzer Kommentar in Issue #108) und die gewählte Variante umgesetzt. **Default, falls der Anwender nicht antwortet: volle `FrameworkReference`-Variante** — `<FrameworkReference Include="Microsoft.AspNetCore.App" />` in `Schnittstellenzentrale.Infrastructure.csproj` aufnehmen und die beiden Legacy-Paketreferenzen `Microsoft.AspNetCore.Http.Abstractions 2.3.0` und `Microsoft.AspNetCore.SignalR.Core 1.2.10` entfernen (Begründung: `Hub`/`IHubContext<THub>` in `SignalRNotificationService` und `IHttpContextAccessor` in `WindowsCurrentUserService` liegen vollständig im Shared Framework; zwei Legacy-Paketlinien entfallen; der faktische Major-Sprung SignalR.Core 1.2.10 → Shared-Framework-Version wird sauber vollzogen statt verwaltet). In beiden Varianten verbindlich: `Microsoft.AspNetCore.SignalR.Client` (`10.0.*` bzw. `11.0.*` nach Schritt 1) aus `Infrastructure.csproj:11` **ins Web-csproj verschieben** — das Paket ist nicht Teil des Shared Frameworks und wird faktisch nur im Web-Projekt genutzt (`Components/Layout/AppShell.razor`, `Components/Shared/ApplicationGroupTree.razor`). Bei der vollen Variante ist die Verträglichkeit des bunit-Testprojekts mit der transitiven `FrameworkReference` per Build-/Test-Check zu verifizieren; die csproj-Hook-Anforderungen (GenerateDocumentationFile etc.) nicht aushebeln.

**Akzeptanzkriterien:** `dotnet build` fehlerfrei; `dotnet test --filter "Category!=OsInterface"` vollständig grün (inkl. `TopBarTests`); `SignalR.Client`-Referenz liegt im Web-csproj, nicht mehr in Infrastructure; bei voller Variante: keine `Http.Abstractions`-/`SignalR.Core`-PackageReference mehr in Infrastructure, Testprojekt baut und läuft gegen die transitive `FrameworkReference`; Entscheidung mit Begründung in `decisions.md` dokumentiert; PR gegen `staging`.

**Abhängigkeiten:** 1.

**Betroffene Bereiche:** `Schnittstellenzentrale.Infrastructure.csproj`, `Schnittstellenzentrale.csproj`, `SignalRNotificationService`, `WindowsCurrentUserService`, Testprojekt-Verträglichkeit, `decisions.md`.

### Schritt 3: F5 – Microsoft.OData.Edm → 9.0.0 + dokumentierte AspNetCore.OData-Strategie

**Ziel/Umfang:** `Microsoft.OData.Edm` in `Schnittstellenzentrale.Infrastructure.csproj` von `8.4.3` auf die stabile `9.0.0` heben — erfüllt die Paketanforderung von `Microsoft.AspNetCore.OData 10.0.0-preview.2` (`>= 9.0.0-preview.4`) und hebt den binärfragilen Versions-Split auf (kompiliert gegen 8.4.3, läuft transitiv gegen 9.0.0-preview.4); Minimal-Fallback bei Problemen: `8.4.4`. `Infrastructure/Services/ODataImportService.cs` (`CsdlReader.Parse`, `IEdmModel`/`IEdmEntitySet`/`IEdmOperation`/`IEdmAction`) gegen die Edm-9-API prüfen. Zusammenhang dokumentierte Entscheidung zur `Microsoft.AspNetCore.OData`-Strategie in `decisions.md` (+ Issue-Kommentar): **Default, falls der Anwender nicht antwortet: auf `10.0.0-preview.2` verharren** (Begründung: kein stabiler 10.x-/11.x-Release existiert; preview.2 läuft unter net11 bereits seit Schritt 1 verifiziert; ein Preview-Tracking ohne Zielrelease erhöht nur das Risiko) mit festgehaltenem **Exit-Kriterium: Umstieg auf die stabile Linie, sobald ein stabiler 10.x- oder 11.x-Release verfügbar ist**; bis dahin wird der Preview-Stand nicht weiter getrackt.

**Akzeptanzkriterien:** kein Versions-Split bei `Microsoft.OData.Edm` über die Projekte hinweg (`dotnet list package --include-transitive` einheitlich); `ODataControllerIntegrationTests` (78 Tests) grün; `dotnet test --filter "Category!=OsInterface"` vollständig grün; OData-Funktionsverifikation (Import-Pfad über `ODataImportServiceIntegrationTests`, `[EnableQuery]`-Endpunkte via `ODataControllerIntegrationTests`); `dotnet list package --vulnerable` ohne Befund; Strategie + Exit-Kriterium in `decisions.md` dokumentiert; PR gegen `staging`.

**Abhängigkeiten:** 1 (baut auf der unter net11 verifizierten preview.2-Lauffähigkeit aus Schritt 1 auf).

**Betroffene Bereiche:** `Schnittstellenzentrale.Infrastructure.csproj`, `ODataImportService`, `src/Schnittstellenzentrale/OData/*` (Edm-Modellaufbau, Controller), `decisions.md`.

### Schritt 4: F4 – Serilog.AspNetCore → 11.0.0

**Ziel/Umfang:** `Serilog.AspNetCore` im Web-csproj von `10.0.0` auf `11.0.0` heben und das Changelog von 11.0.0 auf Breaking Changes prüfen. Kompatibilität der Sinks `Serilog.Sinks.File 7.0.0` und `Serilog.Sinks.EventLog 4.0.0` verifizieren (Paket-Support-Matrizen/Release-Notes; ggf. Sinks mitheben). Betroffene Codestellen: `Program.cs:43-47` (Bootstrap-Logger via `LoggerConfiguration().ReadFrom.Configuration`) und `Program.cs:59` (`builder.Host.UseSerilog()`) sowie der Serilog-Abschnitt in `appsettings.json:13-32` (EventLog-Source `Schnittstellenzentrale`, File-Sink `logs/log-.txt` Daily-Rolling). Da ein Logging-Ausfall still wäre, ist die Log-Ausgabe nach dem Update gezielt zu verifizieren (Anwendung starten, `logs/log-*.txt` entsteht und füllt sich, EventLog-Sink-Konfiguration geladen).

**Akzeptanzkriterien:** `dotnet test --filter "Category!=OsInterface"` vollständig grün; dokumentierte Log-Verifikation (File-Sink schreibt tatsächlich, kein stiller Ausfall); `dotnet list package --vulnerable` ohne Befund; PR gegen `staging`.

**Abhängigkeiten:** 1.

**Betroffene Bereiche:** `Schnittstellenzentrale.csproj` (Web), `Program.cs`, `appsettings.json`, Serilog-Sink-Pakete.

### Schritt 5: F3 – Koordiniertes Update Microsoft.OpenApi → 3.11.1 + Swashbuckle.AspNetCore → 11.0.1

**Ziel/Umfang:** `Microsoft.OpenApi` (referenziert in `.Core` **und** `.Infrastructure`) von `2.10.0` auf `3.11.1` und `Swashbuckle.AspNetCore` (Web) von `10.1.7` auf `11.0.1` **in einem gemeinsamen Schritt** heben — getrennte Updates würden einen OpenApi-2/3-Mix erzeugen, da `OpenApiDocument` in der öffentlichen Core-Signatur `ISwaggerImportService.SyncFromDocumentAsync` steht und `SystemEndpointSyncService` das Dokument via Swashbuckle-`ISwaggerProvider` (`GetSwagger("v1")`) bezieht. Zielversionen zum Umsetzungszeitpunkt erneut konkretisieren (Stand der Planung: 3.11.1 / 11.0.1). API-Migration auf das 3.x-Modell in: `Infrastructure/Services/SwaggerImportService.cs` (Reader-API `OpenApiJsonReader`/`ReadAsync` → 3.x-Äquivalent), `Infrastructure/Helpers/SwaggerOperationHelper.cs` (`Paths`, `Operations`, `IOpenApiExtension`, `OpenApiSecuritySchemeReference`, `IOpenApiParameter`), Web-Schicht: `Filters/SecurityOperationFilter.cs`, `Filters/ContextHeadersOperationFilter.cs`, `Filters/SzExtensionsOperationFilter.cs` (x-sz-*-Erweiterungen), `Program.cs` (`AddSwaggerGen`, `UseSwagger`/`UseSwaggerUI`, `IncludeXmlComments`) und den Test-Fixtures (`SwaggerImportServiceTests`, `SystemEndpointSyncServiceTests`, `SystemEntryInitializerTests`). Eigenständige Swashbuckle-11-Breaking-Changes sind gesondert zu prüfen.

**Akzeptanzkriterien:** `dotnet test --filter "Category!=OsInterface"` vollständig grün; Swagger-Funktionsverifikation (`/swagger/v1/swagger.json` liefert ein gültiges OpenAPI-3-Dokument, Swagger-UI erreichbar); Startup-Abgleich des `SystemEndpointSyncService` über die vorhandenen Tests verifiziert; kein OpenApi-2/3-Mix (einheitliche `Microsoft.OpenApi`-Major-Version über alle Projekte/transitiv); `dotnet list package --vulnerable` ohne Befund; PR gegen `staging`.

**Abhängigkeiten:** 1.

**Betroffene Bereiche:** `.Core`- und `.Infrastructure`-csproj (`Microsoft.OpenApi`), Web-csproj (`Swashbuckle.AspNetCore`), `SwaggerImportService`, `SwaggerOperationHelper`, Web-Filters, `Program.cs`, `SystemEndpointSyncService`, Test-Fixtures.

### Schritt 6: xunit.runner.visualstudio → 4.0.1 (risikoarmer Zwischenschritt)

**Ziel/Umfang:** In `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj` nur `xunit.runner.visualstudio` von `3.1.5` auf `4.0.1` heben; `xunit 2.9.3` bleibt unverändert. Der Schritt prüft vor der eigentlichen v3-Migration risikoarm, ob `dotnet test` mit der dokumentierten Filter-Syntax `--filter "Category!=OsInterface"` und `--filter "Category=OsInterface"` weiterhin korrekt selektiert (die `[OsInterface]`-/`[Flaky]`-Trait-Infrastruktur unter `Infrastructure/Testing/` basiert noch auf der xunit-v2-`Xunit.Sdk`-Extensibilität) und ob die Coverage-Lane (`--collect:"XPlat Code Coverage"` mit `coverlet.collector`, 70-%-Gate in der CI) unverändert funktioniert.

**Akzeptanzkriterien:** `dotnet test --filter "Category!=OsInterface"` vollständig grün (641-Test-Baseline); `dotnet test --filter "Category=OsInterface"` selektiert weiterhin die Trait-markierten Tests (Ausführung selbst best-effort, Selektion muss stimmen); Coverage-Collection läuft ohne Fehler; PR gegen `staging`.

**Abhängigkeiten:** 1.

**Betroffene Bereiche:** `Schnittstellenzentrale.Tests.csproj`, Test-Selektion/Coverage-Verhalten.

### Schritt 7: F1 – Microsoft.Playwright → 1.65.1

**Ziel/Umfang:** `Microsoft.Playwright` im Test-csproj von `1.52.0` auf `1.65.1` heben. Vor der Umsetzung sind die Release-Notes/Breaking Changes **aller** Zwischenreleases 1.53–1.64 vollständig zu sichten (bisher nur 1.65 dokumentiert) und gegen die ~400 Locator-/Action-Aufrufe in `src/Schnittstellenzentrale.Tests/Playwright/` zu prüfen (`Locator` 334, `ClickAsync` 212, `Expect` 150, `ToBeVisibleAsync` 117, `GetByRole` 106, `FillAsync` 62 u. a. — Zählung siehe `inventory.md`); betroffene Aufrufe und die Infrastruktur (`PlaywrightTestBase`, `PlaywrightServer`, `PlaywrightApiFactory`, `PlaywrightSignalRServer`) anpassen. Das `InstallPlaywright`-Target (`playwright.ps1 install chromium`, abschaltbar via `-p:SkipPlaywrightInstall=true`) installiert automatisch die zum neuen Paket passenden Browser — lokale Browser-Installation erneuern und ggf. den CI-Cache invalidieren. Da die OsInterface-CI-Lane nur best-effort läuft (`continue-on-error`), ist als verbindliches Absicherungs-Vorgehen ein **lokales Gate als Merge-Pflicht** zu definieren und im PR festzuhalten: `dotnet test --filter "Category=OsInterface"` muss lokal vollständig grün gelaufen sein, bevor der PR gemergt wird (Ergebnis/TRX im PR verlinken).

**Akzeptanzkriterien:** `dotnet test --filter "Category!=OsInterface"` vollständig grün; **lokaler** Lauf `dotnet test --filter "Category=OsInterface"` grün (bzw. dokumentierte best-effort-Bewertung mit Begründung je Befund); neue Chromium-Binaries über das Build-Target installiert und lauffähig; CI-Cache-Befund beurteilt/dokumentiert; `dotnet list package --vulnerable` ohne Befund; PR gegen `staging`.

**Abhängigkeiten:** 6 (Testinfrastruktur-Schritte laufen strikt sequenziell; der Runner-Tausch geht voran).

**Betroffene Bereiche:** `Schnittstellenzentrale.Tests.csproj`, gesamtes Verzeichnis `src/Schnittstellenzentrale.Tests/Playwright/` (Tests + Infrastruktur), `InstallPlaywright`-Target, CI-Cache, PR-/Gate-Dokumentation.

### Schritt 8: F2 – Migration auf xunit.v3 (4.0.3)

**Ziel/Umfang:** Vollständige Migration des Testprojekts von `xunit 2.9.3` auf `xunit.v3 4.0.3` (Zielversion zum Umsetzungszeitpunkt konkretisieren): `OutputType Exe` im Test-csproj setzen, `Microsoft.NET.Test.Sdk`-/MTP-Konfiguration anpassen, `IAsyncLifetime`-Signaturen auf `ValueTask` umstellen (4 Dateien: `Integration/EndpointExecutionIntegrationTests.cs`, `Playwright/Infrastructure/PlaywrightServer.cs`, `Playwright/Infrastructure/PlaywrightTestBase.cs`, `Playwright/SitePageTests.cs`), `Xunit.Abstractions`/`ITestOutputHelper` (22 Dateien) auf die v3-Pendants migrieren und `<Using Include="Xunit" />` prüfen. Die v2-spezifische `Xunit.Sdk`-Trait-Infrastruktur unter `Infrastructure/Testing/` (`OsInterfaceAttribute` als `ITraitAttribute`, `OsInterfaceFact`/`OsInterfaceTheory`, `FlakyAttribute`, `CategoryTraitDiscoverer` als `ITraitDiscoverer`, `TestTraitReader` plus Meta-Tests `TraitParityTests`/`FlakyConventionTests`) ist gegen die v3-Extensibilität **neu zu bauen** — die Semantik bleibt identisch: `dotnet test --filter "Category=OsInterface"` und `"Category!=OsInterface"` müssen weiterhin korrekt selektieren (dokumentierte Konvention in `CLAUDE.md`, CI-Filtern und `scripts/Retry-FlakyTests.ps1`, das `--list-tests`/`--filter` nutzt). Zusätzlich zu verifizieren: Verhalten von `coverlet.collector`/Coverage-Gate unter Microsoft.Testing.Platform; abschließend `CLAUDE.md` und `docs/help/stabilitaet/os-interface-tests.md` auf die migrierten Kommandos aktualisieren. 704 `[Fact]`, 8 `[Theory]`, 23 `[Collection]`-Nutzungen sind umfangmäßig betroffen — die Migration erfolgt mechanisch komplett in diesem einen Schritt.

**Akzeptanzkriterien:** `dotnet test --filter "Category!=OsInterface"` vollständig grün; `dotnet test --filter "Category=OsInterface"` liefert exakt die Trait-markierte Teilmenge (Trait-Selektion intakt, Ausführung best-effort dokumentiert bzw. lokaler Lauf wie in Schritt 7 vereinbart); Coverage-Collection funktioniert unter MTP (70-%-Gate erhalten); `Retry-FlakyTests.ps1`-Kommandos verifiziert/angepasst; `CLAUDE.md` + `docs/help/stabilitaet/os-interface-tests.md` aktualisiert; PR gegen `staging`.

**Abhängigkeiten:** 7 (strikt sequenziell nach Runner-Update und Playwright-Update; letzte Testinfrastruktur-Änderung).

**Betroffene Bereiche:** `Schnittstellenzentrale.Tests.csproj`, `Infrastructure/Testing/*` (Trait-System komplett), gesamte Testsuite, CI-Testkommandos, `scripts/Retry-FlakyTests.ps1`, `CLAUDE.md`, Hilfsdoku.

### Schritt 9: F7-Prozessentscheidung + F8-Auslagerung + Abschluss-PR

**Ziel/Umfang:** Abschließender Dokumentationsschritt ohne Produktivcode-Änderung (im Default). **F7:** Entscheidung zur Update-Prozessstrategie mit Begründung in `docs/projects/task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko/decisions.md` dokumentieren (+ Kommentar in Issue #108). **Default, falls der Anwender nicht antwortet: Variante (a) Floating beibehalten + regelmäßige Paketprüfung** — Begründung: Floating ist bewusst gewählt (Security-Patches fließen implizit, dokumentiert im `System.Security.Cryptography.Pkcs`-Kommentar); „Floating + Lock" ist kein sinnvoller Mittelweg; die bisherige Praxis hat den Paketprüfungs-Prozess bereits etabliert. Bei Default ist die Minimaländerung = reine Dokumentation (keine `packages.lock.json`, kein `Directory.Packages.props` einführen); wird stattdessen (b) Pinnen+Lock oder (c) CPM gewählt, ist die entsprechende Umsetzung in diesem Schritt zu erweitern. **F8:** Entscheidung dokumentieren und umsetzen — **Default: separates Issue für npm-`devDependencies` und GitHub-Actions-Pins mit eigener Kadenz vorschlagen/anlegen** (Begründung: außerhalb des NuGet-Scopes, verdient eigenen Rhythmus); falls keine Issue-Berechtigung: Titel + Skizze in `decisions.md` festhalten. **Abschluss:** finaler PR gegen `staging` mit Closing-Tag `Closes #108` in der PR-/Commit-Beschreibung (Issue schließt bei Promotion nach main — Staging-Konvention).

**Akzeptanzkriterien:** `decisions.md` enthält vollständig die Entscheidungen F5 (OData-Strategie + Exit-Kriterium), F6 (Variante), F7 (Prozess) und F8 (Auslagerung) mit Begründungen; F8-Issue angelegt oder begründeter Verzicht dokumentiert; `dotnet test --filter "Category!=OsInterface"` grün (unverändert); finaler PR gegen `staging` trägt `Closes #108`.

**Abhängigkeiten:** 2, 3, 4, 5, 6, 7, 8 (letzter Schritt; konsolidiert alle Entscheidungen und trägt den Closing-Tag).

**Betroffene Bereiche:** `decisions.md`, Issue #108 (Kommentare/Closing), ggf. neues Folge-Issue für F8; im Nicht-Default-Fall zusätzlich `packages.lock.json`/`Directory.Packages.props` und csproj-Pins.

## Übersichtstabelle

| # | Titel | Abhängigkeiten | Betroffene Bereiche |
|---|-------|----------------|---------------------|
| 1 | .NET-11-Basisupgrade (TFM, SDK, CI, Floating-Baseline) | – | Alle `.csproj`, `global.json`, CI-Workflows/-Actions, README/Docs, Test-Kommentar |
| 2 | F6: SignalR.Client ins Web-Projekt + FrameworkReference-Entscheidung | 1 | Infrastructure-/Web-csproj, SignalR-/HttpContext-Services, `decisions.md` |
| 3 | F5: Microsoft.OData.Edm 9.0.0 + AspNetCore.OData-Strategie | 1 | Infrastructure-csproj, `ODataImportService`, Web-OData-Controller, `decisions.md` |
| 4 | F4: Serilog.AspNetCore 11.0.0 | 1 | Web-csproj, `Program.cs`, `appsettings.json`, Sinks |
| 5 | F3: Microsoft.OpenApi 3.11.1 + Swashbuckle.AspNetCore 11.0.1 | 1 | Core-/Infrastructure-/Web-csproj, Import-Services, Filters, `Program.cs`, Tests |
| 6 | xunit.runner.visualstudio → 4.0.1 | 1 | Test-csproj, Test-Selektion/Coverage |
| 7 | F1: Microsoft.Playwright → 1.65.1 | 6 | Test-csproj, `Playwright/`-Suite + Infrastruktur, CI-Cache, Merge-Gate |
| 8 | F2: xunit.v3-Migration (4.0.3) | 7 | Test-csproj, `Infrastructure/Testing/` (Trait-System), gesamte Suite, `CLAUDE.md`, Skripte, Doku |
| 9 | F7-Prozessentscheidung + F8-Auslagerung + Abschluss (`Closes #108`) | 2, 3, 4, 5, 6, 7, 8 | `decisions.md`, Issue #108, ggf. Folge-Issue, ggf. Lock/CPM-Artefakte |

## Offene Punkte

> **Behandlungsstand (Projektleitung, 2026-11-24):** Alle fünf Punkte wurden mit den
> unten genannten Vorschlägen als **dokumentierte Annahmen** übernommen und gelten
> damit als behandelt. Begründung: Die Vorschläge entsprechen den Empfehlungen aus
> Issue #108 bzw. sind erzwungen (Punkt 1) bzw. status-quo-wahrend (Punkt 3); der
> Auftrag delegiert die Abarbeitung ausdrücklich. Die getroffenen Annahmen werden im
> Abschluss-PR transparent gemacht und können vom Anwender nachträglich übersteuert
> werden. Kein Punkt betrifft Scope-Ausschlüsse oder Abnahmeverzichte (diese wären
> nutzerpflichtig).

1. **OData-Preview-Strategie unter net11 (F5):** Auf `Microsoft.AspNetCore.OData 10.0.0-preview.2` verharren oder nächste Previews tracken? *Vorschlag:* Auf preview.2 verharren mit Exit-Kriterium „Umstieg sobald ein stabiler 10.x-/11.x-Release existiert". *Begründung:* Es gibt keinen stabilen 10.x-/11.x-Release; preview.2 ist unter net11 ab Schritt 1 über die 78 `ODataControllerIntegrationTests` verifiziert; blindes Preview-Tracking erhöht das Risiko ohne Gegenwert.
2. **F6-Variante:** Volle `FrameworkReference Microsoft.AspNetCore.App` in Infrastructure oder nur Minimalvariante (SignalR.Client verschieben)? *Vorschlag:* Volle FrameworkReference-Variante. *Begründung:* `Hub`/`IHubContext` und `IHttpContextAccessor` sind vollständig im Shared Framework; zwei Legacy-Paketlinien (`Http.Abstractions 2.3.0`, `SignalR.Core 1.2.10`) entfallen; der ohnehin fällige Major-Sprung von SignalR.Core wird sauber auf die Framework-Version vollzogen — Restrisiko (bunit-Verträglichkeit) wird im Schritt verifiziert.
3. **F7-Prozesswahl:** Floating + Paketprüfung (a), Pinnen + `packages.lock.json` Locked-Mode + Bot-Updates (b) oder CPM via `Directory.Packages.props` (c)? *Vorschlag:* (a) Floating beibehalten. *Begründung:* Floating ist bewusst gewählt und im Pkcs-Security-Kommentar begründet; „Floating + Lock" ist kein sinnvoller Mittelweg; Bot-basiertes Pinnen führt neue Infrastruktur ein, die außerhalb des Issue-Scopes liegt.
4. **F8-Auslagerung:** Separates Issue für npm-`devDependencies`/GitHub-Actions-Pins anlegen? *Vorschlag:* Ja, separates Issue mit eigener Kadenz anlegen (bzw. Skizze dokumentieren). *Begründung:* Liegt außerhalb des NuGet-Scopes von Issue #108; ein eigenes Issue verhindert Scope-Creep und gibt den Punkten einen eigenen Prüfrhythmus.
5. **F1-Absicherung:** Wie wird das Playwright-Update verbindlich abgesichert, wo die OsInterface-CI-Lane nur best-effort läuft? *Vorschlag:* Lokales Gate als Merge-Pflicht — `dotnet test --filter "Category=OsInterface"` muss lokal grün gelaufen sein, Ergebnis (TRX/Log) wird im PR verlinkt. *Begründung:* Die CI kann den Nachweis nicht erzwingen (`continue-on-error`); ein definiertes lokales Gate ist die schlankste verbindliche Absicherung ohne CI-Umbau.
