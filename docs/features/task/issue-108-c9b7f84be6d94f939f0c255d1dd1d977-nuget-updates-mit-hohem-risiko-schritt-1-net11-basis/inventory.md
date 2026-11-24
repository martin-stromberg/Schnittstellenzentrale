# Bestandsaufnahme – .NET-11-Basisupgrade (Schritt 1)

Stand: Branch `task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko-schritt-1-net11-basis` (sauberer Working Tree, abgezweigt vom Basisbranch `...-nuget-updates-mit-hohem-risiko`).

Übergeordnete Projektdokumente (Kontext, nicht Teil dieses Schritts): `docs/projects/task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko/` (`requirement.md`, `inventory.md`, `project-plan.md`, `project-plan-check.md`).

## Umgebung

- Installierte SDKs: `10.0.404`, `11.0.100` (Verifikation: `dotnet --list-sdks`).
- `global.json`: `"version": "10.0.301"`, `"rollForward": "latestFeature"` → aktive SDK-Auflösung derzeit `10.0.404`.
- Solution: `Schnittstellenzentrale.slnx` (Repo-Root), enthält alle 4 Projekte.
- Kein `Directory.Packages.props`, kein `Directory.Build.props/.targets`, kein `nuget.config`, kein `packages.lock.json` im Repo.

## Projektdateien (4 × TFM `net10.0`, 17 Floating `10.0.*`)

### `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`
- TFM `net10.0` (Zeile 38).
- Floating: `Microsoft.AspNetCore.Authentication.Negotiate 10.0.*` (Z15), `Microsoft.EntityFrameworkCore.Design 10.0.*` (Z17, PrivateAssets=all).
- Gepinnt unverändert: `Microsoft.AspNetCore.OData 10.0.0-preview.2` (net10-only — bewusst), `Serilog.AspNetCore 10.0.0` (F4, späterer Schritt), `Serilog.Sinks.EventLog 4.0.0`, `Serilog.Sinks.File 7.0.0`, `ShadcnBlazor 1.0.14`, `Swashbuckle.AspNetCore 10.1.7` (F3, späterer Schritt).

### `src/Schnittstellenzentrale.Core/Schnittstellenzentrale.Core.csproj`
- TFM `net10.0` (Z8). Keine Floating-Referenzen; `Microsoft.OpenApi 2.10.0` gepinnt (F3).

### `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj`
- TFM `net10.0` (Z41).
- 11 Floating: `SignalR.Client`, `EntityFrameworkCore`, `EntityFrameworkCore.Design`, `EntityFrameworkCore.Sqlite`, `EntityFrameworkCore.SqlServer`, `Extensions.Configuration.Abstractions`, `Extensions.Configuration.Binder`, `Extensions.DependencyInjection.Abstractions`, `Extensions.Http`, `JSInterop`, `System.Security.Cryptography.Pkcs` (je `10.0.*`).
- **Pkcs-Pin-Kommentar (Z28–36)** ist wörtlich an 10.x gebunden („>= 10.0.13", „every 10.x release >= 10.0.13 is patched", „10.0.* float") — muss auf 11.x-Begründung umgeschrieben werden (GHSA-846f-f764-76mx high, GHSA-2fmv-rv2c-59x7 moderate bleiben die Begründung; unter 11.x gilt analog: gepatchte Linie ≥ der ersten 11.x-Version, die den Fix enthält).
- Gepinnt unverändert: `Jint 4.9.2`, `Markdig 1.2.0`, `Microsoft.AspNetCore.Http.Abstractions 2.3.0` (F6), `Microsoft.AspNetCore.SignalR.Core 1.2.10` (F6), `Microsoft.OData.Edm 8.4.3` (F5), `Microsoft.OpenApi 2.10.0` (F3), `SQLitePCLRaw.lib.e_sqlite3 3.53.3`.

### `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj`
- TFM `net10.0` (Z4).
- 4 Floating: `Microsoft.AspNetCore.Mvc.Testing`, `EntityFrameworkCore.Sqlite`, `EntityFrameworkCore.SqlServer`, `Microsoft.JSInterop` (je `10.0.*`).
- **AngleSharp-Override-Kommentar (Z20–23):** version-neutral (GHSA-pgww-w46g-26qg, bunit-Transitivauflösung) — keine 10.x-Bindung, keine Anpassung nötig.
- Gepinnt unverändert: `AngleSharp 1.5.0`, `bunit 2.7.2`, `coverlet.collector 10.0.0`, `Jint 4.9.2`, `Microsoft.NET.Test.Sdk 18.5.1`, `Microsoft.Playwright 1.52.0` (F1), `Moq 4.20.72`, `SQLitePCLRaw.lib.e_sqlite3 3.53.3`, `xunit 2.9.3` (F2), `xunit.runner.visualstudio 3.1.5` (F2).
- `InstallPlaywright`-Target nutzt `$(TargetFramework)` dynamisch — keine harte net10-Nennung.

## CI/CD (`10.0.x`-Pins und net10-Nennungen)

9 `dotnet-version: '10.0.x'`-Fundstellen:
- `.github/workflows/pr-staging-ci.yml`: Z81, Z126, Z226 (dazu Schrittnamen `Setup .NET 10` Z78/123/223)
- `.github/workflows/staging-ci.yml`: Z74, Z107, Z202 (dazu `Setup .NET 10` Z71/104/199)
- `.github/workflows/security-scan.yml`: Z31 (dazu `Setup .NET 10` Z28)
- `.github/workflows/release.yml`: Z52 (unquoted `10.0.x`)
- `.github/actions/build-and-package/action.yml`: Z41 (dazu `Setup .NET 10` Z38, Description Z4 „Provisions the .NET 10 SDK")

Harter TFM im Publish: `action.yml:57` — `dotnet publish ... --framework net10.0 --runtime win-x64 --self-contained true`.

## Code-/Dokumentations-Nennungen von net10 / .NET 10

- `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightTestBase.cs:27` — Kommentar `bin/<cfg>/net10.0/` (reine Kommentarzeile).
- `README.md:4` — Badge `.NET-10.0`; `README.md:6` — Badge `EF_Core-10`; `README.md:34` — `.NET 10.0 Runtime (ASP.NET Core)`.
- `docs/help/schnittstellenzentrale/installation.md:6` — `.NET 10.0 Runtime (ASP.NET Core)`.
- `docs/help/playwright-tests/installation.md:5` — `.NET 10.0 SDK`.
- `issue.md` — historische Aufgabenbeschreibung, bleibt unverändert.
- `docs/projects/task/issue-108-*/` — Projektplandokumente (historisch), bleiben unverändert.
- `.softwareschmiede-scm-exchange/` — gitignore'd, irrelevant.
- `obj/`/`bin/` — Build-Artefakte, regenerieren sich.

## Anwendungsstart (für E2E-Abnahme)

- `Program.cs`: Negotiate-Auth außerhalb IIS, aber **keine** Fallback-Authorization-Policy; REST-/OData-Basisklassen tragen `[AllowAnonymous]` (`ApiControllerBase.cs:12`, `ODataControllerBase.cs:10`) → `GET /` und `GET /odatav4/...` sind anonym erreichbar.
- `PathBase: /` in `appsettings.json`; HTTPS-Redirect aktiv außerhalb Playwright-Umgebung → für E2E HTTP-Port oder `-k` bei HTTPS verwenden, bzw. `ASPNETCORE_URLS` setzen.
- SQLite als Default-Provider; DB-Initialisierung via `MigrateAsync` beim Start.

## Testbaseline

- `dotnet test --filter "Category!=OsInterface"`: Baseline **641 Tests** (aus Issue-/Projektkontext; verifizieren).
- `ODataControllerIntegrationTests`: `src/Schnittstellenzentrale.Tests/Integration/ODataControllerIntegrationTests.cs` — 78 `[Fact]`/`[Theory]`-Tests (Projekt-Inventar, verifiziert: 83 Vorkommen Fact/Theory im File, davon 78 Tests laut Requirement).
- `coverlet.collector 10.0.0` — Paketversion (nicht TFM-gebunden), bleibt unverändert; ist keine `10.0.*`-Floating-Referenz.
