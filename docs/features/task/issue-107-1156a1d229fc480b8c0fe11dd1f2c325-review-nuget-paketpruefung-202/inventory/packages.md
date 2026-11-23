# Paketreferenzen je Projekt

Stand: 2026-11-23. Alle Projekte targeten `net10.0`. Aufgelöste Versionen aus `dotnet list Schnittstellenzentrale.slnx package` (Restore aktuell, Quelle nuget.org + Fallback-Ordner). Floating `10.0.*` löst überall auf **10.0.15** auf.

## `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`

SDK: `Microsoft.NET.Sdk.Web`. Referenziert Core + Infrastructure via `ProjectReference`.

| Paket | Deklariert | Aufgelöst | Bemerkung |
|-------|-----------|-----------|-----------|
| `Microsoft.AspNetCore.App.Internal.Assets` | (A) `[10.0.15, )` | 10.0.15 | automatisch referenziert (Framework) |
| `Microsoft.AspNetCore.Authentication.Negotiate` | 10.0.* | 10.0.15 | Floating |
| `Microsoft.AspNetCore.OData` | 10.0.0-preview.2 | 10.0.0-preview.2 | einzige Preview-Referenz |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.* | 10.0.15 | `PrivateAssets=all` |
| `Serilog.AspNetCore` | 10.0.0 | 10.0.0 | |
| `Serilog.Sinks.EventLog` | 4.0.0 | 4.0.0 | nicht in outdated-Liste → aktuell |
| `Serilog.Sinks.File` | 7.0.0 | 7.0.0 | nicht in outdated-Liste → aktuell |
| `ShadcnBlazor` | 1.0.14 | 1.0.14 | |
| `Swashbuckle.AspNetCore` | 10.1.7 | 10.1.7 | |

## `src/Schnittstellenzentrale.Core/Schnittstellenzentrale.Core.csproj`

SDK: `Microsoft.NET.Sdk` (reine Library, kein ASP.NET-Framework-Bezug).

| Paket | Deklariert | Aufgelöst | Bemerkung |
|-------|-----------|-----------|-----------|
| `Microsoft.OpenApi` | 2.10.0 | 2.10.0 | |

## `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj`

SDK: `Microsoft.NET.Sdk`. Referenziert Core. **Keine `FrameworkReference` auf `Microsoft.AspNetCore.App`** — ASP.NET-Typen kommen ausschließlich aus NuGet-Paketen.

| Paket | Deklariert | Aufgelöst | Bemerkung |
|-------|-----------|-----------|-----------|
| `Jint` | 4.9.2 | 4.9.2 | |
| `Markdig` | 1.2.0 | 1.2.0 | |
| `Microsoft.AspNetCore.Http.Abstractions` | 2.3.0 | 2.3.0 | Legacy-Linie (zuletzt 2.3.x); liefert `Microsoft.AspNetCore.Http`-Typen |
| `Microsoft.AspNetCore.SignalR.Client` | 10.0.* | 10.0.15 | Floating; im Infrastructure-Code keine `HubConnection`-Verwendung (Nutzung über Projektverweis im Web-Projekt, s. code-usage.md) |
| `Microsoft.AspNetCore.SignalR.Core` | 1.2.10 | 1.2.10 | Legacy-Linie (letzte Paketgeneration vor Shared Framework); liefert `Hub`/`IHubContext<T>` |
| `Microsoft.EntityFrameworkCore` | 10.0.* | 10.0.15 | Floating |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.* | 10.0.15 | `PrivateAssets=all` |
| `Microsoft.EntityFrameworkCore.Sqlite` | 10.0.* | 10.0.15 | Floating |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.* | 10.0.15 | Floating |
| `Microsoft.Extensions.Configuration.Abstractions` | 10.0.* | 10.0.15 | Floating |
| `Microsoft.Extensions.Configuration.Binder` | 10.0.* | 10.0.15 | Floating |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 10.0.* | 10.0.15 | Floating |
| `Microsoft.Extensions.Http` | 10.0.* | 10.0.15 | Floating |
| `Microsoft.JSInterop` | 10.0.* | 10.0.15 | Floating |
| `Microsoft.OData.Edm` | 8.4.3 | 8.4.3 | direkt genutzt in `ODataImportService` (`CsdlReader`); abweichend von der transitiven 9.0.0-preview.4 in Web/Tests |
| `Microsoft.OpenApi` | 2.10.0 | 2.10.0 | |
| `SQLitePCLRaw.lib.e_sqlite3` | 3.53.3 | 3.53.3 | |
| `System.Security.Cryptography.Pkcs` | 10.0.* | 10.0.15 | **Direkt-Override mit Kommentar** (csproj Z. 28–36): hält Auflösung auf gepatchter 10.x-Linie ≥ 10.0.13 für GHSA-846f-f764-76mx (high) und GHSA-2fmv-rv2c-59x7 (moderate); transitiv auch von `Microsoft.EntityFrameworkCore.SqlServer` ≥ 10.0.15 gefordert; nearest-wins propagiert in referenzierende Projekte |

## `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj`

SDK: `Microsoft.NET.Sdk`. Referenziert alle drei anderen Projekte. MSBuild-Target `InstallPlaywright` (`AfterTargets="Build"`) führt `playwright.ps1 install chromium` aus (abschaltbar via `SkipPlaywrightInstall=true`).

| Paket | Deklariert | Aufgelöst | Bemerkung |
|-------|-----------|-----------|-----------|
| `AngleSharp` | 1.5.0 | 1.5.0 | **Direkt-Override mit Kommentar** (csproj Z. 13–16): bunit 2.7.2 löst transitiv vulnerable 1.4.0 auf (GHSA-pgww-w46g-26qg, moderate, mXSS); direkte Referenz gewinnt per nearest-wins; zusätzlich direkt genutzt in `PlaywrightServer.cs` |
| `bunit` | 2.7.2 | 2.7.2 | |
| `coverlet.collector` | 10.0.0 | 10.0.0 | `PrivateAssets=all` |
| `Jint` | 4.9.2 | 4.9.2 | |
| `Microsoft.AspNetCore.Mvc.Testing` | 10.0.* | 10.0.15 | Floating |
| `Microsoft.EntityFrameworkCore.Sqlite` | 10.0.* | 10.0.15 | Floating |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.* | 10.0.15 | Floating |
| `Microsoft.JSInterop` | 10.0.* | 10.0.15 | Floating |
| `Microsoft.NET.Test.Sdk` | 18.5.1 | 18.5.1 | |
| `Microsoft.Playwright` | 1.52.0 | 1.52.0 | |
| `Moq` | 4.20.72 | 4.20.72 | |
| `SQLitePCLRaw.lib.e_sqlite3` | 3.53.3 | 3.53.3 | |
| `xunit` | 2.9.3 | 2.9.3 | **deprecated** (Legacy → `xunit.v3`) |
| `xunit.runner.visualstudio` | 3.1.5 | 3.1.5 | `PrivateAssets=all` |

## Projektübergreifende Fakten

- **18 Floating-Referenzen** (`10.0.*`) auf 14 verschiedene Pakete — alle lösen aktuell auf **10.0.15** auf; Patch-Updates fließen bei jedem `dotnet restore` implizit ein.
- **Kein zentrales Paketmanagement**: weder `Directory.Packages.props` noch `packages.lock.json` noch `nuget.config` im Repo gefunden.
- **Versionssplit bei `Microsoft.OData.Edm`**: Infrastructure pinnt 8.4.3 direkt; im Web- und Testprojekt löst transitiv (via `Microsoft.AspNetCore.OData` 10.0.0-preview.2) **9.0.0-preview.4** auf — dort gewinnt die höhere transitive Anforderung des Preview-Pakets.
- **Preview-Abhängigkeiten** (nur transitiv in Web + Tests): `Microsoft.OData.Core`/`Edm`/`Spatial` 9.0.0-preview.4, `Microsoft.OData.ModelBuilder` 3.0.0-preview.1, `AngleSharp.Css` 1.0.0-beta.157 (via bunit).
