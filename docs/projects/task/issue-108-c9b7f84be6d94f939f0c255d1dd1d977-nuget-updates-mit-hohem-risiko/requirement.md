# Anforderung – .NET-11-Upgrade + aufgeschobene NuGet-Befunde (Issue #108)

**Quelle:** GitHub-Issue [martin-stromberg/Schnittstellenzentrale#108](https://github.com/martin-stromberg/Schnittstellenzentrale/issues/108) („NuGet-Updates mit hohem Risiko / Architektur-Entscheidungen, aus Paketprüfung 2026-KW48") + Issue-Kommentar von martin-stromberg (2026-11-24): „ich denke wir müssen für ein Upgrade auf .NET 11 machen. Dies und die gemeldeten Befunde sollten dann mit dem project-skill abgearbeitet werden."

**Interpretation des Auftrags:** Das .NET-11-Upgrade der Lösung und die im Issue gemeldeten, aufgeschobenen NuGet-/Architektur-Befunde werden gemeinsam als mehrteiliges Projekt abgearbeitet — nicht in einem einzelnen PR, sondern schrittweise in getrennten, eigenständig verifizierbaren Änderungen (Fehlerisolierung gemäß Issue-Vorgabe).

## Ziel

Die Lösung wird von .NET 10 auf .NET 11 gehoben und die in Issue #108 gebündelten Major-Updates, Migrationen und Architektur-/Prozessentscheidungen werden strukturiert abgearbeitet. Am Ende steht ein konsistenter Stand: .NET 11 als Ziel-Framework, aufgelöste Paket-Major-Sprünge ohne Versions-Splits, dokumentierte Entscheidungen für die offenen Architektur-/Prozessfragen und eine weiterhin grüne Testsuite.

## Fachliche Anforderungen

### A. .NET-11-Upgrade der Lösung (belegte Ausgangslage)

- Alle 4 Projekte stehen derzeit auf `net10.0`: `src/Schnittstellenzentrale` (Web), `.Core`, `.Infrastructure`, `.Tests`.
- `global.json` pinnt das SDK auf `10.0.301` mit `rollForward: latestFeature`.
- Installierte SDKs auf dem Entwicklungsstand: 10.0.404 und **11.0.100** — ein .NET-11-SDK ist also bereits verfügbar.
- 17 `10.0.*`-Floating-PackageReferences existieren; weder `packages.lock.json` noch `Directory.Packages.props` sind vorhanden.
- CI-Workflows pinnen `dotnet-version: '10.0.x'` an 9 Stellen (`.github/workflows` + `.github/actions/build-and-package`).

**Anforderung:** TargetFramework auf `net11.0` (alle Projekte), `global.json` auf das .NET-11-SDK, CI-Pins auf `11.0.x`. Der Umgang mit den `10.0.*`-Floatings (mitheben auf `11.0.*` oder pinnen) hängt von der Prozessentscheidung F7 ab — siehe Offene Fragen.

### B. Aufgeschobene Befunde aus Issue #108 (8 Punkte)

| # | Thema | Art | Issue-Risiko |
|---|-------|-----|--------------|
| F1 | `Microsoft.Playwright` 1.52.0 → 1.65.1 | Update mit dokumentierten Breaking Changes | mittel |
| F2 | `xunit` 2.9.3 → `xunit.v3`, `xunit.runner.visualstudio` 3.1.5 → 4.x | Migration | mittel–hoch |
| F3 | `Microsoft.OpenApi` 2.10.0 → 3.x + `Swashbuckle.AspNetCore` 10.1.7 → 11.x | koordiniertes Major-Update, **ein gemeinsamer PR** | hoch |
| F4 | `Serilog.AspNetCore` 10.0.0 → 11.0.0 | Major-Update (`UseSerilog` in `Program.cs`) | mittel |
| F5 | `Microsoft.OData.Edm` 8.4.3 → 9.0.0 + `Microsoft.AspNetCore.OData`-Strategie | Update + Entscheidung | mittel |
| F6 | `FrameworkReference Microsoft.AspNetCore.App` in Infrastructure bzw. Minimalvariante | Architektur-Entscheidung | mittel |
| F7 | Floating `10.0.*` vs. Lock-Dateien/CPM | Prozess-Entscheidung | niedrig |
| F8 | npm-`devDependencies`, GitHub-Actions-Pins | Hinweis, außerhalb NuGet-Scope | – |

Details und Besonderheiten je Punkt (aus dem Issue, verifiziert):

- **F1:** ~400 betroffene Locator-/Action-Aufrufe (`GetByRole`/`ClickAsync`/`FillAsync` u. a.) in OsInterface-Tests. Änderungen der Zwischenreleases 1.53–1.64 sind vor dem Update vollständig zu prüfen (bisher nur 1.65-Release-Notes dokumentiert). Browser-Binaries werden über das `InstallPlaywright`-Target (`playwright.ps1 install chromium`) beim Build nachinstalliert — CI-Caches ggf. invalidieren.
- **F2:** v2 ist feature-frozen/deprecated; Migration umfasst `OutputType Exe`, `IAsyncLifetime` → `ValueTask`, Wegfall `Xunit.Abstractions`; `xunit.runner.visualstudio` 4.x kann als risikoärmerer Zwischenschritt vorweggehen. Verhalten von `dotnet test`, `--filter`-Syntax (Trait `Category=OsInterface`) und Coverage via `coverlet.collector` unter Microsoft.Testing.Platform verifizieren.
- **F3:** Getrenntes Update erzeugt einen OpenApi-2/3-Mix im Swagger-Pfad (`SwaggerImportService`, `ODataImportService`; `Microsoft.OpenApi` in Core + Infrastructure) — daher zwingend ein PR. Eigenständige Swashbuckle-11-Breaking-Changes und die 3.x-API-Oberfläche in beiden Import-Services verifizieren; Zielversionen zum Umsetzungszeitpunkt konkretisieren.
- **F4:** Changelog von 11.0.0 prüfen; Sinks (`Serilog.Sinks.File` 7.0.0, `Serilog.Sinks.EventLog` 4.0.0) auf Kompatibilität prüfen; nach dem Update die Log-Ausgabe gezielt verifizieren (Logging-Ausfall ist still).
- **F5:** `Microsoft.AspNetCore.OData` hat keinen stabilen 10.x-Release (neueste: `10.0.0-preview.2`; neuester stabiler Strang 9.5.0 für net8.0). `Microsoft.OData.Edm` auf 9.0.0 hebt den binärfragilen Versions-Split auf (Infrastructure kompiliert gegen 8.4.3, läuft transitiv gegen 9.0.0-preview.4). `Edm` 8.4.4 als Minimal-Fallback. Regression: `ODataControllerIntegrationTests` (78 Tests).
- **F6 (Korrektur aus dem Issue beachten):** `Hub`/`IHubContext<THub>` sind vollständig im Shared Framework enthalten → `FrameworkReference` deckt `SignalRNotificationService<THub>` ab. `Microsoft.AspNetCore.SignalR.Client` ist **nicht** Teil des Shared Frameworks und wird produktiv im **Web-Projekt** genutzt (`AppShell.razor`, `ApplicationGroupTree.razor`, `TopBarTests`) — die Referenz ist daher aus Infrastructure ins Web-Projekt zu **verschieben**, nicht zu entfernen. `SignalR.Core` 1.2.10 → Shared-Framework-Version ist faktisch selbst ein Major-Upgrade. Minimalvariante: nur `SignalR.Client` verschieben.
- **F7:** Floating ist teilweise bewusst gewählt (Security-Patches fließen implizit, begründet im `System.Security.Cryptography.Pkcs`-Kommentar, GHSA-846f-f764-76mx / GHSA-2fmv-rv2c-59x7). Kombination „Floating + Lock" ist kein sinnvoller Mittelweg. Alternativen: a) Floating + regelmäßige Paketprüfung (Status quo), b) pinnen + `packages.lock.json` im Locked-Mode + botgestützte Update-PRs, c) `Directory.Packages.props` (CPM) als möglicher Zwischenschritt.
- **F8:** Außerhalb des NuGet-Scopes; ggf. separates Issue/eigene Kadenz.

## Betroffene Bereiche

- **Projektdateien:** alle 4 `.csproj` (`TargetFramework`, PackageReferences), `global.json`.
- **CI:** 9 `dotnet-version`-Pins in `.github/workflows` (`pr-staging-ci.yml`, `staging-ci.yml`, `release.yml`, `security-scan.yml`) und `.github/actions/build-and-package/action.yml`; ggf. CI-Cache-Invalidierung für Playwright-Browser.
- **Code:**
  - `Program.cs` (`UseSerilog`, Swagger/OpenApi-/OData-Registrierung).
  - `SwaggerImportService`, `ODataImportService` (Microsoft.OpenApi-API gegen 3.x-Modell).
  - `SignalRNotificationService<THub>` und `WindowsCurrentUserService` (Infrastructure, F6).
  - `AppShell.razor`, `ApplicationGroupTree.razor` (`HubConnection`-Nutzung, F6).
- **Tests:** gesamte Testsuite (F2-Migration), OsInterface-/Playwright-Tests (~400 Aufrufe, F1), `ODataControllerIntegrationTests` (78 Tests, F5), `TopBarTests` (F6), `PlaywrightServer`/Kestrel-Setup.
- **Dokumentation:** `CLAUDE.md` (dokumentierte `dotnet test --filter`-Kommandos unter xunit.v3/Testing.Platform), `docs/help/stabilitaet/os-interface-tests.md` (best-effort-Lane).

## Abläufe

1. **Entscheidungen zuerst** (Issue-Empfehlung): F7 (Update-Prozess) und die F5-`AspNetCore.OData`-Strategie festlegen — sie rahmen alle weiteren Updates. Entscheidungen dokumentieren (im Issue oder als Folge-Artefakt).
2. **.NET-11-Upgrade** als eigenständiger Schritt: TFM, `global.json`, CI-Pins; dabei klären, ob `10.0.*`-Floatings auf `11.0.*` folgen (Annahme: ja, sofern F7 Floating beibehält).
3. **F6-Minimalvariante** (`SignalR.Client` ins Web-Projekt) kann jederzeit klein umgesetzt werden; die volle `FrameworkReference`-Variante nur bei positiver Entscheidung inkl. Build-/Test-Check des bunit-Testprojekts gegen die transitive `FrameworkReference`.
4. **Testinfrastruktur getrennt und sequenziell** (damit die absichernden Tests nicht gleichzeitig aus zwei Richtungen verändert werden): erst Runner 3.1.5 → 4.x, dann Playwright F1, xunit.v3-Migration als eigenes Teilprojekt zuletzt.
5. **F3** und **F4** als getrennte PRs.
6. **F8**: auslagern oder separat dokumentieren.
7. Abschluss-PR gegen `staging` mit Closing-Tag `Closes #108` (Konvention: PRs gehen gegen staging; Issue schließt bei Promotion nach main).

Jeder Schritt ist ein eigener, eigenständig verifizierbarer PR — kein Sammel-PR.

## Konfiguration

- Ziel-SDK: .NET 11 (installiert: 11.0.100); `global.json` entsprechend anheben (`rollForward`-Verhalten beibehalten, sofern nicht anders entschieden).
- Ziel-Framework: `net11.0` in allen 4 Projekten.
- CI: `dotnet-version` auf `11.0.x` an allen 9 Fundstellen.
- OsInterface-Lane bleibt best-effort in der CI — für F1 ist ein verbindliches Absicherungs-Vorgehen zu definieren (siehe Offene Fragen).
- Keine `packages.lock.json`/`Directory.Packages.props` ohne ausdrückliche F7-Entscheidung einführen.

## Akzeptanzkriterien

- **Nach dem .NET-11-Upgrade:** Lösung baut und läuft unter `net11.0`/SDK 11.x; `dotnet test --filter "Category!=OsInterface"` vollständig grün (aktueller Baseline: 641 Tests); CI-Lanes grün mit `11.0.x`.
- **Pro umgesetztem Update/Migration (F1–F5):** `dotnet test --filter "Category!=OsInterface"` vollständig grün; bei Änderungen an der Test-Infrastruktur (F1, F2) zusätzlich `--filter "Category=OsInterface"` grün bzw. dokumentierte best-effort-Bewertung; `dotnet list package --vulnerable` ohne Befund.
- **F5 zusätzlich:** `ODataControllerIntegrationTests` (78) grün; kein Versions-Split bei `Microsoft.OData.Edm` über die Projekte hinweg.
- **F4 zusätzlich:** Log-Ausgabe nach dem Update gezielt verifiziert (nicht nur Build/Tests).
- **F6 (falls volle Variante):** Test-Projekt verträgt die transitive `FrameworkReference` (Build-/Test-Check); `SignalR.Client`-Referenz liegt im Web-Projekt.
- **Entscheidungen** zu F5 (OData-Strategie), F6 (Variante), F7 (Prozess) und F8 (Auslagerung) sind dokumentiert — im Issue oder in Folge-Artefakten/-Issues.
- **Abschluss:** PR(s) gegen `staging`; finaler PR trägt `Closes #108`.

## Explizite Nicht-Ziele

- **F8-Inhalte selbst umsetzen:** npm-`devDependencies` und GitHub-Actions-Pins werden nicht geprüft/geändert — nur die Entscheidung, ob ein separates Issue mit eigener Kadenz angelegt wird.
- **Kein Sammel-PR:** Die Punkte werden nicht in einer einzelnen Änderung gebündelt (Issue-Vorgabe: Fehlerisolierung, eigenständige Verifikation).
- **Keine Scope-Erweiterung:** keine weiteren Paket-Updates, Refactorings oder Architekturänderungen über .NET 11 + die 8 Issue-Punkte hinaus.
- **Keine Änderung der PR-Zielkonvention:** PRs gehen weiterhin gegen `staging` (Issue-Schließung erfolgt bei Promotion nach main).
- **`Microsoft.AspNetCore.OData`-Preview nicht eigenmächtig ersetzen:** kein Wechsel auf eine andere OData-Implementierung ohne Entscheidung.

## Offene Fragen

1. **F5/OData unter .NET 11:** Auf `Microsoft.AspNetCore.OData` 10.0.0-preview.2 verharren bis ein stabiler 10.x/11.x-Release existiert, oder nächstes Preview tracken, sobald verfügbar? Kriterium festlegen, wann auf eine stabile Linie umgeschwenkt wird. *Annahme zur Prüfung:* Ob die preview.2 überhaupt mit `net11.0` kompatibel ist, ist beim Upgrade zu verifizieren — das kann die Entscheidung zeitlich vorgeben.
2. **F6-Variante:** Volle `FrameworkReference Microsoft.AspNetCore.App` in Infrastructure (inkl. Bindung von Infrastructure ans ASP.NET-Core-Shared-Framework; `SignalR.Core`-Paketlinie → Shared-Framework-Version ist selbst ein Major-Upgrade) oder nur die Minimalvariante (`SignalR.Client` ins Web-Projekt verschieben)?
3. **F7-Prozessentscheidung:** Floating beibehalten + regelmäßige Paketprüfung (a), pinnen + `packages.lock.json` Locked-Mode + Bot-Updates (b) oder CPM via `Directory.Packages.props` (c)? Die Entscheidung bestimmt auch, ob `10.0.*`-Referenzen beim .NET-11-Upgrade auf `11.0.*` folgen oder gepinnt werden (Annahme: bei a) folgen sie auf `11.0.*`).
4. **F1-Absicherung:** Die OsInterface-Lane läuft in der CI nur best-effort — wie wird das Playwright-Update verbindlich abgesichert (z. B. lokales Gate als definierte Pflicht vor dem Merge)?
5. **F2-Tooling:** Verhalten von `dotnet test`, `--filter`-Syntax (`Category=OsInterface`-Traits) und `coverlet.collector`-Coverage unter xunit.v3/Microsoft.Testing.Platform verifizieren; Dokumentation der Test-Kommandos (`CLAUDE.md`) ggf. anpassen.
6. **F8:** Soll für npm-`devDependencies`/Actions-Pins ein separates Issue mit eigener Kadenz angelegt werden?
7. **Reihenfolge des .NET-11-Upgrades:** Soll das Framework-Upgrade als erster Schritt (alle Befunde darauf aufbauend) oder nach den risikoarmen Entscheidungen/F6-Minimalvariante erfolgen? *Annahme:* Das Issue empfiehlt Entscheidungen zuerst; das .NET-11-Upgrade als früher eigenständiger Schritt ist plausibel, aber nicht belegt vorgegeben.
