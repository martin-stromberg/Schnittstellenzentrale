# Empfehlungsreport: NuGet-Paketprüfung 2026-KW48 (Issue #107)

Risikobewertete Update-Empfehlungen für alle `PackageReference`-Einträge der Solution
`Schnittstellenzentrale.slnx` (net10.0). Umgesetzt wurden Empfehlungen mit Risiko
**niedrig** und **mittel**; **hoch**-Empfehlungen sind dokumentiert und für ein gebündeltes
Folge-Issue aufbereitet (Entwurf am Ende). Recherche- und Verifikationsstand: lokale
Release-Notes-Prüfung je Kandidat, danach `dotnet build -c Release -p:TreatWarningsAsErrors=true`,
`dotnet test --filter "Category!=OsInterface"` und
`dotnet list package --vulnerable --include-transitive` pro Batch.

## Verifikationsergebnis

| Batch | Inhalt | Build (Release + TWAE) | Tests (`Category!=OsInterface`) | Vulnerable-Scan (direkt + transitiv) |
|-------|--------|------------------------|----------------------------------|--------------------------------------|
| A | 3 Patch-Updates | 0 Warnungen, 0 Fehler | **641/641 bestanden** | **0 Befunde** |
| B | 7 Minor-Updates (8 Referenzen) | 0 Warnungen, 0 Fehler | **641/641 bestanden** | **0 Befunde** |

## Umgesetzte Updates

### Batch A — Patches (niedrig)

| Paket | Aktuell → Ziel | Update-Art | Risiko + Begründung |
|-------|----------------|------------|----------------------|
| `ShadcnBlazor` | 1.0.14 → 1.0.22 | Patch | **niedrig** — Patch-Linie 1.0.x; Dependency-Anhebungen (LucideBlazor ≥ 1.0.6, TailwindMerge.NET ≥ 1.4.0) ohne Breaking-Hinweis; Rendering-Risiko über `LayoutSmokeTests` abgedeckt. |
| `Microsoft.AspNetCore.Http.Abstractions` | 2.3.0 → 2.3.14 | Patch | **niedrig** — Servicing-Patches der gewarteten 2.3.x-Legacy-Linie, keine API-Änderung; liefert `IHttpContextAccessor` für `WindowsCurrentUserService`. |
| `Microsoft.AspNetCore.SignalR.Core` | 1.2.10 → 1.2.14 | Patch | **niedrig** — Servicing-Patches der letzten SignalR-Paketlinie 1.2.x, keine API-Änderung; liefert `Hub`/`IHubContext<THub>` für `SignalRNotificationService<THub>`. |

### Batch B — Minoren (mittel)

| Paket | Aktuell → Ziel | Update-Art | Risiko + Begründung |
|-------|----------------|------------|----------------------|
| `Markdig` | 1.2.0 → 1.6.0 | Minor | **mittel** — Der als „Breaking" markierte Rename `ParseSetexHeadings`→`ParseSetextHeadings` (1.5.1) wurde vom Maintainer per `[Obsolete]`-Alias entschärft; Nutzung beschränkt sich auf `Markdown.ToHtml()` (`ImpressumService`) — weder Property noch `DisableHeadings()` im Einsatz. |
| `Jint` | 4.9.2 → 4.18.1 | Minor | **mittel** — 4.x-Wartungsbranch: laut Branch-Policy nur nicht-brechende Correctness-/Conformance-Fixes, keine API-/Default-Änderungen. Neue Durchsetzung von `MaxArraySize`/`RegexTimeout` betrifft `EndpointScriptRunner` nicht (setzt nur `TimeoutInterval`/`LimitMemory`); `continue`-in-Label-Loop-Fix ist Korrektur. `EndpointScriptRunnerTests` grün. |
| `bunit` | 2.7.2 → 2.15.2 | Minor | **mittel** — Changelog 2.7.2→2.15.2 nur Added/Changed/Fixed, kein Breaking-Abschnitt; hebt transitive Anforderungen auf AngleSharp ≥ 1.10.1 und AngleSharp.Css ≥ 1.4.1. Alle 11 bunit-Testklassen grün. |
| `AngleSharp` | 1.5.0 → 1.10.5 | Minor | **mittel** — Changelog nur Fixes/Additions (Interface-Erweiterungen wie `BeforeToggle`, die nur Implementierer betreffen — Konsumseite unberührt). Direkt-Override beibehalten und Kommentar aktualisiert (bunit zieht selbst ≥ 1.10.1; Pin bleibt nötig, um die Auflösung auf 1.10.5 über dem transitiven Minimum ≥ 1.10.1 aus bunit 2.15.2 festzuhalten). |
| `coverlet.collector` | 10.0.0 → 10.1.1 | Minor | **mittel** — Changelog 10.1.0/10.1.1 nur Improvements und Regression-Fixes; Coverage-Gate (≥ 70 %) wird in der CI verifiziert. |
| `Microsoft.NET.Test.Sdk` | 18.5.1 → 18.12.0 | Minor | **mittel** — 18.12.0 enthält den Security-Fix CVE-2026-105544 (VSTest-Artefakt-Postprozessor) sowie Fehlerkorrekturen; die in .NET 11 dokumentierte Newtonsoft.Json-Entfernung trifft uns nicht (kein `Newtonsoft.Json`-Verweis im Code). |
| `Moq` | 4.20.72 → 4.21.0 | Minor | **mittel** — Release-Notes: nur Enhancements (It.Is-Compilation, Nullable-ctor-Args, Performance) und Fixes; die SponsorLink-Entfernung lag bereits in 4.20.69 (in 4.20.72 enthalten). |

## Aufgeschoben / hoch (nicht umgesetzt — Folge-Issue-Entwurf unten)

| Paket | Aktuell → Ziel | Update-Art | Risiko + Begründung |
|-------|----------------|------------|----------------------|
| `Microsoft.Playwright` | 1.52.0 → 1.65.1 | Minor mit dokumentierten Breaking Changes | **hoch** — Release Notes 1.65 listen explizit „Breaking Changes": Locator-Aktionen warten auf nicht-inerte Elemente, Cookie-Matching nach RFC 6265 (`BrowserContext.CookiesAsync`/`ApiRequestContext.CookiesAsync`), ungerenderter Text fällt aus Accessible Names/Aria-Snapshots (`GetByRole` mit Name betroffen), `ParamsString` wird angehängt statt ersetzt, Proxy-Credentials nur noch für Proxy-Challenges. Unsere OsInterface-Tests nutzen `GetByRole`/`ClickAsync`/`FillAsync` extensiv — Treffer plausibel; die Lane läuft lokal nicht (best-effort CI), ein Bruch wäre lokal nicht nachweisbar → gezielte Anpassung im Folge-Issue. |
| `xunit` | 2.9.3 → `xunit.v3` | Deprecation / Nachfolgepaket | **hoch** — Paket als „Legacy" deprecated, Alternative `xunit.v3`; Migration = neues Ausführungsmodell (Testprojekt wird Executable, `IAsyncLifetime` → `ValueTask`, `Xunit.Abstractions` entfällt) — strukturelle Umstellung, kein Versions-Bump. |
| `xunit.runner.visualstudio` | 3.1.5 → 4.0.1 | Major | **hoch** — Major-Sprung per Schema. Fakt aus Recherche: Adapter 4.x führt weiterhin xUnit ≥ 1.9.2 aus — kompatibel mit xunit 2.9.3, kann daher gebündelt mit oder vor der v3-Migration erfolgen. |
| `Microsoft.OpenApi` | 2.10.0 → 3.11.1 | Major | **hoch** — OpenAPI-3.x-Modell mit API-Umbau; in Core + Infrastructure im Swagger-Import-/Export-Pfad produktiv. Koordiniert mit `Swashbuckle.AspNetCore` 11 (erwartet OpenApi 3.x) — gebündelter Major-Block. |
| `Swashbuckle.AspNetCore` | 10.1.7 → 11.0.1 | Major | **hoch** — siehe oben; getrenntes Update würde OpenApi-2/3-Mix erzeugen. |
| `Serilog.AspNetCore` | 10.0.0 → 11.0.0 | Major | **hoch** — Major-Linienwechsel des Logging-Hosts (Request-Logging, `UseSerilog`-Konfiguration). |
| `Microsoft.AspNetCore.OData` | 10.0.0-preview.2 → — | Preview-Strategie | **hoch** — Recherche-Fakt: kein stabiler 10.x-Release existiert, preview.2 ist das neueste 10.x-Preview; neuester stabiler Strang ist 9.5.0 (net8.0). Jede Bewegung ist Preview-Wechsel → nur kohärent entscheiden, kein unkoordinierter Sprung. 78 `ODataControllerIntegrationTests` als Regressionssuite vorhanden. |
| `Microsoft.OData.Edm` | 8.4.3 → 9.0.0 (Empfehlung) | Major | **hoch** — Empfehlung: stabile 9.0.0 angleichen; erfüllt die transitive Anforderung `>= 9.0.0-preview.4` des AspNetCore.OData-Previews (SemVer 9.0.0 > 9.0.0-preview.4) und beseitigt den Versions-Split Infrastructure (8.4.3) vs. Web/Tests (9.0.0-preview.4). Recherche-Fakt: mit **8.4.4** existiert auch ein 8.x-Patch (nur Build-/Pipeline-Wartung) — Fallback-Option, wenn 9.0.0 zu groß; bleibt hier per Plan unverändert. |
| `Microsoft.AspNetCore.Http.Abstractions` / `SignalR.Core` (Ablösung) | Paket → `FrameworkReference Microsoft.AspNetCore.App` | Architektur | **hoch** — Infrastructure hat keinen Framework-Bezug; Ablösung bezieht ASP.NET-Typen aus dem Shared Framework 10.x, ändert API-Oberfläche/Verhalten. Patch-Updates (oben) halten die Legacy-Linien bis zur Entscheidung auf Fixstand. |
| `Microsoft.AspNetCore.SignalR.Client` | 10.0.* → 11.0.0 | Major (TFM-gebunden) | **hoch** — nur mit .NET-11-TFM-Wechsel sinnvoll (Scope-Ausschluss); im Infrastructure-Code ohnehin keine `HubConnection`-Nutzung (Nutzung transitiv im Web-Projekt) — Prüfkandidat auf Entfernbarkeit im Folge-Issue. |
| `System.Composition.*`, `Microsoft.IdentityModel.*`, `Microsoft.Data.SqlClient` u. a. (transitiv) | diverse → Major-Linien | Major (transitiv) | **hoch** — nur über Parent-Pakete erreichbar; fließt mit den gebündelten Major-Updates (EF 11-Linie bei TFM-Wechsel, IdentityModel 7→8 via SqlClient 7.x) mit. Kein Direkt-Pin. |

## Keine Aktion

| Paket / Gruppe | Stand | Begründung |
|----------------|-------|------------|
| 13 Floating-`10.0.*`-Pakete (17 Referenzen, u. a. `Microsoft.EntityFrameworkCore*`, `Microsoft.Extensions.*`, `Microsoft.JSInterop`, `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.AspNetCore.Authentication.Negotiate`) | lösen auf 10.0.15 auf | Patch-Auflösung fließt implizit bei jedem `dotnet restore`; „Neueste" = 11.0.0 ist TFM-gebunden (Ausschluss: kein TFM-Upgrade). |
| `System.Security.Cryptography.Pkcs` | 10.0.* (Override) | Direkt-Override unverändert — floating, gepatchte Linie ≥ 10.0.13 (GHSA-846f-f764-76mx, GHSA-2fmv-rv2c-59x7), Kommentar bereits korrekt. |
| `Microsoft.OData.Edm` | 8.4.3 | Per Plan unverändert — jede OData-Bewegung ist hoch (s. Tabelle oben). |
| `Microsoft.AspNetCore.OData` | 10.0.0-preview.2 | Kein stabiler/neuerer 10.x-Release verfügbar (Recherche-Fakt). |
| `Serilog.Sinks.EventLog` 4.0.0, `Serilog.Sinks.File` 7.0.0, `SQLitePCLRaw.lib.e_sqlite3` 3.53.3 | aktuell | Kein Update verfügbar laut outdated-Report. |
| `xunit` 2.9.3 | aktuellste 2.x | Kein neueres v2-Release; Ablösung = xunit.v3-Migration (hoch). |

## Transitive Highlights (nur beobachtet, nicht angefasst)

- `AngleSharp.Css`: durch bunit 2.15.2 von `1.0.0-beta.157` auf stabile **1.4.1** gehoben — Beta-Abhängigkeit beseitigt; `AngleSharp.Diffing` 1.1.1.
- `Microsoft.OData.Core`/`Edm`/`Spatial` 9.0.0-preview.4 und `Microsoft.OData.ModelBuilder` 3.0.0-preview.1 (via AspNetCore.OData-Preview): stabile 9.0.0 bzw. 3.0.0 existieren — Teil der OData-Kohärenz-Empfehlung.
- `Microsoft.AspNetCore.Http.Features` (transitiv via Http.Abstractions) wird durch das 2.3.14-Update mitgehoben; `Microsoft.AspNetCore.Authorization` 2.3.9 bleibt auf der 2.3.x-Legacy-Linie.
- Major-Linien unverändert: `Microsoft.IdentityModel.*`/`System.IdentityModel.Tokens.Jwt` 7.7.1 (→ 8.23.0), `Microsoft.Data.SqlClient` 6.1.7 (→ 7.1.2), `Humanizer.Core` 2.14.1 (→ 3.0.10), `SQLitePCLRaw.bundle_e_sqlite3`-Stack 2.1.13 (→ 3.0.5) — folgen nur mit Parent-Updates (hoch).

## Hinweise außerhalb des NuGet-Scopes (nicht umgesetzt, Scope-Ausschluss)

- **npm `devDependencies`** (`package.json`, Release-Automatisierung): `semantic-release` ^25.0.5, `@semantic-release/commit-analyzer` ^13.0.1, `@semantic-release/github` ^11.0.6, `@semantic-release/release-notes-generator` ^14.0.3, `conventional-changelog-conventionalcommits` ^9.1.0 — keine Auffälligkeit im Rahmen dieser Prüfung festgestellt; bei Bedarf eigener Review.
- **GitHub-Actions-Pins**: `actions/checkout@v7`, `actions/setup-dotnet@v6`, `actions/setup-node@v7`, `actions/upload-artifact@v7`, `actions/configure-pages@v6`, `actions/upload-pages-artifact@v5`, `actions/deploy-pages@v5`, `dotnet-reportgenerator-globaltool` 5.5.11 — unverändert; keine Auffälligkeit.

---

## Entwurf: gebündeltes Folge-Issue (ein Issue, ohne Label `automated-nuget-review`)

> **Titel:** NuGet-Updates mit hohem Risiko / Architektur-Entscheidungen (aus Paketprüfung 2026-KW48)
>
> **Kontext:** Die wöchentliche NuGet-Paketprüfung (Issue #107) hat alle niedrig/mittel-Updates
> umgesetzt und verifiziert (641/641 Tests, 0 Vulnerabilities). Folgende Empfehlungen wurden
> wegen Major-Sprüngen, dokumentierter Breaking Changes oder Architektur-Tragweite gebündelt
> aufgeschoben.
>
> ### 1. `Microsoft.Playwright` 1.52.0 → 1.65.1 (dokumentierte Breaking Changes)
>
> Release Notes 1.65: Locator-Aktionen (`ClickAsync`/`CheckAsync`/`TapAsync`/`FillAsync`/`ClearAsync`/`SelectOptionAsync`)
> warten auf nicht-inerte Elemente; Cookie-Matching folgt RFC 6265; ungerenderter Text fällt aus
> Accessible Names und Aria-Snapshots (betrifft `GetByRole` mit Name und `ToMatchAriaSnapshotAsync`);
> `ParamsString` wird an vorhandene Query angehängt statt ersetzt; Proxy-Credentials gehen nur noch
> an Proxy-Challenges. Unsere OsInterface-Tests nutzen `GetByRole`/`ClickAsync`/`FillAsync` extensiv.
> Vorgehen: OsInterface-Tests unter 1.65.1 lokal/CI gezielt laufen lassen, betroffene Aufrufe
> anpassen, dann Update.
>
> ### 2. xunit → `xunit.v3` (Deprecation)
>
> `xunit` 2.9.3 ist als „Legacy" deprecated; v2 ist feature-frozen. Migration: `OutputType Exe`,
> `IAsyncLifetime` → `ValueTask`, `Xunit.Abstractions` entfällt, `dotnet test --filter`-Syntax
> prüfen. `xunit.runner.visualstudio` 4.0.1 führt weiterhin xUnit ≥ 1.9.2 aus — kann als
> Zwischenschritt vor oder mit der Migration erfolgen (Major 3→4).
>
> ### 3. Koordinierter Major-Block: `Microsoft.OpenApi` 2.10.0 → 3.11.1 + `Swashbuckle.AspNetCore` 10.1.7 → 11.0.1
>
> Swashbuckle 11 erwartet OpenApi 3.x; getrenntes Update erzeugt einen 2/3-Mix im Swagger-Pfad
> (`SwaggerImportService`, `ODataImportService`, `Microsoft.OpenApi` in Core + Infrastructure).
>
> ### 4. `Serilog.AspNetCore` 10.0.0 → 11.0.0
>
> Major-Linienwechsel des Logging-Hosts; `UseSerilog`-/Request-Logging-Konfiguration prüfen.
>
> ### 5. OData-Kohärenz
>
> Fakten: `Microsoft.AspNetCore.OData` hat keinen stabilen 10.x-Release (preview.2 ist neueste
> 10.x-Version; neuester Stabler Strang 9.5.0 für net8.0). `Microsoft.OData.Edm` 9.0.0 ist stabil;
> 8.4.4 existiert als reiner Wartungs-Patch. Empfehlung: `Microsoft.OData.Edm` (Infrastructure,
> `CsdlReader` im `ODataImportService`) auf 9.0.0 anheben — erfüllt die transitive Anforderung
> `>= 9.0.0-preview.4` des AspNetCore.OData-Previews und beseitigt den Versions-Split
> (Infrastructure 8.4.3 vs. Web/Tests 9.0.0-preview.4). Daneben Entscheidung über
> `Microsoft.AspNetCore.OData`: auf preview.2 verharren bis stabiler 10.x-Release, oder nächstes
> Preview tracken, sobald verfügbar. `Microsoft.OData.Edm` 8.4.4 als Minimal-Fallback. Regression:
> `ODataControllerIntegrationTests` (78 Tests).
>
> ### 6. Legacy-Paket-Ablösung via `FrameworkReference Microsoft.AspNetCore.App`
>
> `Schnittstellenzentrale.Infrastructure` hat keinen Framework-Bezug;
> `Microsoft.AspNetCore.Http.Abstractions` (liefert `IHttpContextAccessor` für
> `WindowsCurrentUserService`) und `Microsoft.AspNetCore.SignalR.Core` (`Hub`/`IHubContext<THub>`
> für `SignalRNotificationService<THub>`) könnten durch eine `FrameworkReference` ersetzt werden —
> ändert API-Oberfläche und Verhalten (Architekturänderung). Prüfen: `SignalRNotificationService<THub>`
> ist generisch und braucht `IHubContext<THub>` ggf. weiterhin aus dem Paket — Machbarkeitsnachweis
> vor Umstellung. Ebenfalls prüfen: `Microsoft.AspNetCore.SignalR.Client` wird im
> Infrastructure-Code nicht genutzt (Nutzung transitiv im Web-Projekt) — Referenz ggf. entfernbar.
>
> ### 7. Prozessentscheidung: Floating `10.0.*` vs. `packages.lock.json`/`Directory.Packages.props`
>
> 17 Floating-Referenzen lösen aktuell auf 10.0.15 — Patches fließen implizit. Vorteile Lock-Dateien:
> reproduzierbare Builds, nachverfolgbare Update-Diffs; Nachteil: zusätzlicher Pflegeaufwand.
>
> ### 8. Hinweise (außerhalb NuGet-Scope)
>
> npm `devDependencies` der Release-Automatisierung und GitHub-Actions-Pins wurden nicht geprüft/
> geändert — bei Bedarf eigene Review-Zyklen etablieren.
