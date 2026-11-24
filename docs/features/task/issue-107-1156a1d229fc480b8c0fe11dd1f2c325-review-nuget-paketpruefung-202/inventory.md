# Bestandsaufnahme: [Review] NuGet-Paketprüfung 2026-KW48 (Issue #107)

Faktenlage zu allen `PackageReference`-Einträgen der Solution `Schnittstellenzentrale.slnx` (Target `net10.0`) inklusive transitiver Abhängigkeiten, Paketverwendungen im Code, CI-Gates und Test-Ausgangszustand — als Grundlage für die spätere Risikobewertung und Update-Empfehlungsliste. Keine Bewertung, keine Empfehlungen.

## Zusammenfassung

- **4 Projekte**, insgesamt **45 direkte `PackageReference`-Einträge** (davon 17 mit Floating-Version `10.0.*`, die aktuell alle auf **10.0.15** auflösen). Kein `Directory.Packages.props`, keine `packages.lock.json`, kein `nuget.config` im Repo.
- **`--outdated`**: **35 Top-Level-Einträge** mit neuerer Version (davon 13 Floating-`10.0.*`-Referenzen, bei denen „Neueste" = 11.0.0 die .NET-11-Linie betrifft — TFM bleibt per Scope `net10.0`); dazu ~120 transitive Pakete mit Updates. Details: [inventory/outdated.md](inventory/outdated.md), Rohdaten [dotnet-list-outdated.txt](inventory/dotnet-list-outdated.txt).
- **`--vulnerable`**: **0 gefährdete Pakete** (direkt + transitiv, alle 4 Projekte). Rohdaten: [dotnet-list-vulnerable.txt](inventory/dotnet-list-vulnerable.txt).
- **`--deprecated`**: **xunit 2.9.3** (Top-Level, Grund „Legacy", Alternative `xunit.v3`) plus 4 transitive `xunit.*`-Pakete — nur im Testprojekt. Rohdaten: [dotnet-list-deprecated.txt](inventory/dotnet-list-deprecated.txt).
- **Zwei dokumentierte Security-Direkt-Overrides** existieren bereits und sind begründet: `AngleSharp` 1.5.0 in Tests (GHSA-pgww-w46g-26qg via bunit) und `System.Security.Cryptography.Pkcs` `10.0.*` in Infrastructure (GHSA-846f-f764-76mx, GHSA-2fmv-rv2c-59x7) — Kommentare stehen in den `.csproj`.
- **Legacy-Pakete aktiv genutzt**: `Microsoft.AspNetCore.Http.Abstractions` 2.3.0 liefert `IHttpContextAccessor` für `WindowsCurrentUserService` (Infrastructure hat **keine** `FrameworkReference` auf `Microsoft.AspNetCore.App`); `Microsoft.AspNetCore.SignalR.Core` 1.2.10 liefert `Hub`/`IHubContext<THub>` für `SignalRNotificationService<THub>`. Nachweise: [inventory/code-usage.md](inventory/code-usage.md).
- **OData**: `Microsoft.AspNetCore.OData` 10.0.0-preview.2 ist produktiv im Einsatz (Route `odatav4`, 4 CRUD-Controller + Auth-Controller + EDM-Model-Builder unter `src/Schnittstellenzentrale/OData/`); in Web/Tests zieht es transitiv `Microsoft.OData.Core/Edm/Spatial` 9.0.0-preview.4 und `Microsoft.OData.ModelBuilder` 3.0.0-preview.1, während Infrastructure `Microsoft.OData.Edm` **8.4.3** direkt pinnt.
- **Test-Ausgangszustand**: `dotnet test --filter "Category!=OsInterface"` — **641/641 bestanden**, 0 Fehlschläge, Exit 0 (Nachweis: [inventory/tests.md](inventory/tests.md)). Die OsInterface-Lane (Playwright/Kestrel) wurde nicht ausgeführt.

## Details

- [Paketreferenzen je Projekt (deklariert vs. aufgelöst, Overrides)](inventory/packages.md)
- [Outdated-Auswertung (Top-Level + transitive)](inventory/outdated.md)
- [Code-Verwendung: Legacy-Pakete, OData, weitere Nachweise](inventory/code-usage.md)
- [CI-Gates, npm devDependencies, Actions-Pins, global.json](inventory/ci-context.md)
- [Test-Ausgangszustand und testabdeckende Klassen](inventory/tests.md)

### Rohdaten / Nachweise

- [dotnet list package (Top-Level)](inventory/dotnet-list-toplevel.txt)
- [dotnet list package --outdated --include-transitive](inventory/dotnet-list-outdated.txt)
- [dotnet list package --vulnerable --include-transitive](inventory/dotnet-list-vulnerable.txt)
- [dotnet list package --deprecated --include-transitive](inventory/dotnet-list-deprecated.txt)
- [dotnet test Basislauf (Log)](inventory/test-results/dotnet-test-baseline.log)
