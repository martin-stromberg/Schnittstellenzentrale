# Outdated-/Vulnerable-/Deprecated-Auswertung

Befehle (jeweils auf `Schnittstellenzentrale.slnx`, Exit 0; slnx-Format wird vom SDK 10.0.404 unterstützt, kein Fehler):

```
dotnet list Schnittstellenzentrale.slnx package --outdated --include-transitive
dotnet list Schnittstellenzentrale.slnx package --vulnerable --include-transitive
dotnet list Schnittstellenzentrale.slnx package --deprecated --include-transitive
```

Rohdaten: [dotnet-list-outdated.txt](dotnet-list-outdated.txt) · [dotnet-list-vulnerable.txt](dotnet-list-vulnerable.txt) · [dotnet-list-deprecated.txt](dotnet-list-deprecated.txt)

## Vulnerable

**0 Pakete** — alle vier Projekte melden „keine anfälligen Pakete" (direkt + transitiv). Die beiden bekannten Advisories sind durch die Direkt-Overrides abgedeckt: AngleSharp 1.5.0 (statt transitiver 1.4.0 via bunit) und System.Security.Cryptography.Pkcs 10.0.15 (≥ 10.0.13 gepatcht).

## Deprecated

Nur im Testprojekt:

| Paket | Aufgelöst | Grund | Alternative |
|-------|-----------|-------|-------------|
| `xunit` (Top-Level) | 2.9.3 | Legacy | `xunit.v3` |
| `xunit.assert` (transitiv) | 2.9.3 | Legacy | `xunit.v3.assert` |
| `xunit.core` (transitiv) | 2.9.3 | Legacy | `xunit.v3.core` |
| `xunit.extensibility.core` (transitiv) | 2.9.3 | Legacy | `xunit.v3.extensibility.core` |
| `xunit.extensibility.execution` (transitiv) | 2.9.3 | Legacy | `xunit.v3.extensibility.core` |

## Outdated — Top-Level

„Angefordert" = csproj-Angabe, „Aufgelöst" = tatsächliche Version, „Neueste" = neueste auf nuget.org. Bei den `10.0.*`-Floats zeigt „Neueste" auf die **.NET-11-Linie** (11.0.0); der Wechsel hängt am TFM `net10.0` (Scope-Ausschluss — als Faktum festgehalten, keine Bewertung).

### Schnittstellenzentrale.Core (1)

| Paket | Angefordert | Aufgelöst | Neueste |
|-------|------------|-----------|---------|
| Microsoft.OpenApi | 2.10.0 | 2.10.0 | 3.11.1 |

### Schnittstellenzentrale.Infrastructure (17)

| Paket | Angefordert | Aufgelöst | Neueste | Sprung |
|-------|------------|-----------|---------|--------|
| Jint | 4.9.2 | 4.9.2 | 4.18.1 | Minor (4.x) |
| Markdig | 1.2.0 | 1.2.0 | 1.6.0 | Minor (1.x) |
| Microsoft.AspNetCore.Http.Abstractions | 2.3.0 | 2.3.0 | 2.3.14 | Patch (2.3.x) |
| Microsoft.AspNetCore.SignalR.Client | 10.0.* | 10.0.15 | 11.0.0 | Major-Linie |
| Microsoft.AspNetCore.SignalR.Core | 1.2.10 | 1.2.10 | 1.2.14 | Patch (1.2.x) |
| Microsoft.EntityFrameworkCore | 10.0.* | 10.0.15 | 11.0.0 | Major-Linie |
| Microsoft.EntityFrameworkCore.Design | 10.0.* | 10.0.15 | 11.0.0 | Major-Linie |
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.* | 10.0.15 | 11.0.0 | Major-Linie |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.* | 10.0.15 | 11.0.0 | Major-Linie |
| Microsoft.Extensions.Configuration.Abstractions | 10.0.* | 10.0.15 | 11.0.0 | Major-Linie |
| Microsoft.Extensions.Configuration.Binder | 10.0.* | 10.0.15 | 11.0.0 | Major-Linie |
| Microsoft.Extensions.DependencyInjection.Abstractions | 10.0.* | 10.0.15 | 11.0.0 | Major-Linie |
| Microsoft.Extensions.Http | 10.0.* | 10.0.15 | 11.0.0 | Major-Linie |
| Microsoft.JSInterop | 10.0.* | 10.0.15 | 11.0.0 | Major-Linie |
| Microsoft.OData.Edm | 8.4.3 | 8.4.3 | 9.0.0 | Major (8→9) |
| Microsoft.OpenApi | 2.10.0 | 2.10.0 | 3.11.1 | Major (2→3) |
| System.Security.Cryptography.Pkcs | 10.0.* | 10.0.15 | 11.0.0 | Major-Linie (Override beachten) |

### Schnittstellenzentrale.Tests (12)

| Paket | Angefordert | Aufgelöst | Neueste | Sprung |
|-------|------------|-----------|---------|--------|
| AngleSharp | 1.5.0 | 1.5.0 | 1.10.5 | Minor (1.x) — Override-Kommentar beachten |
| bunit | 2.7.2 | 2.7.2 | 2.15.2 | Minor (2.x) |
| coverlet.collector | 10.0.0 | 10.0.0 | 10.1.1 | Minor (10.x) |
| Jint | 4.9.2 | 4.9.2 | 4.18.1 | Minor (4.x) |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.* | 10.0.15 | 11.0.0 | Major-Linie |
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.* | 10.0.15 | 11.0.0 | Major-Linie |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.* | 10.0.15 | 11.0.0 | Major-Linie |
| Microsoft.JSInterop | 10.0.* | 10.0.15 | 11.0.0 | Major-Linie |
| Microsoft.NET.Test.Sdk | 18.5.1 | 18.5.1 | 18.12.0 | Minor (18.x) |
| Microsoft.Playwright | 1.52.0 | 1.52.0 | 1.65.1 | Minor (1.x) |
| Moq | 4.20.72 | 4.20.72 | 4.21.0 | Minor (4.x) |
| xunit.runner.visualstudio | 3.1.5 | 3.1.5 | 4.0.1 | Major (3→4) |

Nicht gelistet (aktuell bzw. nur deprecated statt outdated): `xunit` 2.9.3 (deprecated → xunit.v3), `SQLitePCLRaw.lib.e_sqlite3` 3.53.3, `Microsoft.OData.Edm` in Tests (nur transitiv), `Microsoft.AspNetCore.OData` 10.0.0-preview.2 (Preview → nicht im outdated-Report gelistet; Faktum: kein Eintrag, ob stabile 10.x existiert ist dort nicht ersichtlich).

### Schnittstellenzentrale (5)

| Paket | Angefordert | Aufgelöst | Neueste | Sprung |
|-------|------------|-----------|---------|--------|
| Microsoft.AspNetCore.Authentication.Negotiate | 10.0.* | 10.0.15 | 11.0.0 | Major-Linie |
| Microsoft.EntityFrameworkCore.Design | 10.0.* | 10.0.15 | 11.0.0 | Major-Linie |
| Serilog.AspNetCore | 10.0.0 | 10.0.0 | 11.0.0 | Major (10→11) |
| ShadcnBlazor | 1.0.14 | 1.0.14 | 1.0.22 | Patch (1.0.x) |
| Swashbuckle.AspNetCore | 10.1.7 | 10.1.7 | 11.0.1 | Major (10→11) |

Nicht gelistet (aktuell): `Serilog.Sinks.EventLog` 4.0.0, `Serilog.Sinks.File` 7.0.0, `Microsoft.AspNetCore.OData` 10.0.0-preview.2.

## Outdated — transitive Highlights

Vollständige Liste in [dotnet-list-outdated.txt](dotnet-list-outdated.txt). Auffällige transitive Einträge (Auflösung → Neueste):

- `Microsoft.AspNetCore.Authorization` **2.3.9** → 11.0.0 und `Microsoft.AspNetCore.Http.Features` **2.3.0** → 5.0.17 (Legacy-Transitive via Http.Abstractions bzw. andere 2.x-Pakete)
- `Microsoft.OData.Core`/`Edm`/`Spatial` **9.0.0-preview.4** → 9.0.0 (stabile 9.0.0 existiert bereits; via AspNetCore.OData preview)
- `Microsoft.OData.ModelBuilder` **3.0.0-preview.1** → 3.0.0 (stabile 3.0.0 existiert)
- `AngleSharp.Css` **1.0.0-beta.157** → 1.5.0 (via bunit)
- `Microsoft.Data.SqlClient` 6.1.7 → 7.1.2; `Azure.Identity` 1.17.1 → 1.21.0; `Microsoft.IdentityModel.*`/`System.IdentityModel.Tokens.Jwt` 7.7.1 → 8.23.0 (Major-Linie 7→8); `Microsoft.Identity.Client*` 4.84.2 → 4.92.0
- `SQLitePCLRaw.bundle_e_sqlite3`/`core`/`provider.e_sqlite3` 2.1.13 → 3.0.5 (transitiv; Top-Level `lib.e_sqlite3` 3.53.3 ist separat)
- `Humanizer.Core` 2.14.1 → 3.0.10; `Castle.Core` 5.1.1 → 5.2.1; `Newtonsoft.Json` 13.0.3 → 13.0.4; `Serilog` 4.3.0 → 4.4.0
- `Microsoft.CodeAnalysis.*` 5.0.0 → 5.12.0, `Microsoft.Build.Framework` 18.0.2 → 18.12.2 (via EFCore.Design), `xunit.analyzers` 1.18.0 → 2.3.0
- `System.Composition.*` 9.0.0 → 11.0.0, `System.Configuration.ConfigurationManager`/`System.Diagnostics.EventLog`/`System.Security.Cryptography.ProtectedData` 9.0.11 → 11.0.0 (via EF/SqlClient/Serilog-EventLog)
- `TailwindMerge.NET` 1.2.0 → 1.5.0, `LucideBlazor` 1.0.4 → 1.0.7 (via ShadcnBlazor)
