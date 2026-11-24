# Umsetzungsplan – .NET-11-Basisupgrade (TFM, SDK, CI, Floating-Baseline)

Eingaben: `requirement.md`, `inventory.md`. Basisbranch: `task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko`.

## Scope-Klärung

- **Verarbeitungseinheit:** Build-/CI-Baseline der Lösung (4 csproj, global.json, 5 CI-Dateien, Kommentare/Docs). Keine Anwendungslogik-Änderung.
- **Auslösendes Ereignis:** Projektschritt 1 aus `project-plan.md` (Issue #108).
- **Erwartete Wirkung:** Build, Tests und CI laufen unter .NET 11 / SDK 11.0.100; Paket-Baseline auf 11.x.
- **Scope-Ausschlüsse:** siehe `requirement.md` → „Explizite Nicht-Ziele". Bewusst unverändert bleiben: `Microsoft.AspNetCore.OData 10.0.0-preview.2`, `coverlet.collector 10.0.0`, `Serilog.AspNetCore 10.0.0`, `Swashbuckle.AspNetCore 10.1.7`, `Microsoft.OpenApi 2.10.0`, `Microsoft.OData.Edm 8.4.3`, `Http.Abstractions 2.3.0`, `SignalR.Core 1.2.10`, `Microsoft.AspNetCore.SignalR.Client`-Platzierung (F6), `xunit`/`Playwright`-Versionen (F1/F2). Alle gepinnten 10.x-Paketversionen sind *bewusste* Folgeschritt-Themen und keine „verbleibenden 10.0.*-Nennungen" im Sinne des Akzeptanzkriteriums (das Kriterium betrifft Floating-Strings `10.0.*` und TFM-Nennungen `net10.0`, nicht gepinnte Paketversionen — dokumentierte Annahme A1).

## Annahmen (keine Rückfragen möglich)

- **A1:** Das Akzeptanzkriterium „keine verbleibenden `10.0.*`-/`net10.0`-Nennungen" bezieht sich auf Floating-Version-Strings und TFM-/SDK-Nennungen, nicht auf bewusst gepinnte Paketversionen (`10.0.0-preview.2`, `10.0.0`, `10.1.7`), die in Folgeschritten behandelt werden. Historische Dokumente (`issue.md`, `docs/projects/task/issue-108-*`, git-Historie) bleiben unangetastet.
- **A2:** Die kosmetischen `Setup .NET 10`-Schrittnamen und die action.yml-Description werden auf `.NET 11` angehoben — sie sind keine historischen Dokumente und würden sonst falsche Versionsangaben enthalten.
- **A3:** Pkcs-Kommentar-Neufassung: Die GHSA-Begründung bleibt inhaltlich gleich; die Zahlen werden auf die 11.x-Linie übertragen („EF Core ≥ 11.x verlangt transitiv eine gepatchte Pkcs-Version; die 11.0.*-Float hält die Auflösung auf der gepatchten Linie"). Die konkrete transitive Mindestversion wird beim Restore beobachtet und im Kommentar generisch formuliert (keine erfundene Patch-Version).
- **A4:** `Microsoft.AspNetCore.OData 10.0.0-preview.2` löst unter `net11.0` per TFM-Kompatibilität auf (net10.0-Assets werden für net11.0 als kompatibel akzeptiert). Falls NuGet dennoch NU1202 wirft, wäre das ein Blocker für diesen Schritt — Eskalation statt eigenmächtigem Paketwechsel.
- **A5:** E2E-Abnahme: App wird lokal per `dotnet run`/kompilierter Ausgabe gestartet (Development-Umgebung, `ASPNETCORE_URLS=http://localhost:<port>`); `GET /` (Blazor, `[AllowAnonymous]`-Pipeline ohne Fallback-Policy) und `GET /odatav4/Applications` als OData-Nachweis.

## Umsetzungsschritte

### 1. `global.json`
`"version": "10.0.301"` → `"11.0.100"`. `rollForward: latestFeature` beibehalten.

### 2. TFM in 4 csproj
`<TargetFramework>net10.0</TargetFramework>` → `net11.0` in:
- `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`
- `src/Schnittstellenzentrale.Core/Schnittstellenzentrale.Core.csproj`
- `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj`
- `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj`

### 3. Floating-Referenzen `10.0.*` → `11.0.*` (17 Stück)
- Web: `Authentication.Negotiate`, `EntityFrameworkCore.Design` (2)
- Infrastructure: `SignalR.Client`, `EntityFrameworkCore`, `EntityFrameworkCore.Design`, `EF.Sqlite`, `EF.SqlServer`, `Extensions.Configuration.Abstractions`, `Extensions.Configuration.Binder`, `Extensions.DependencyInjection.Abstractions`, `Extensions.Http`, `JSInterop`, `System.Security.Cryptography.Pkcs` (11)
- Tests: `Mvc.Testing`, `EF.Sqlite`, `EF.SqlServer`, `JSInterop` (4)

### 4. Pkcs-Pin-Kommentar (`Infrastructure.csproj` Z28–36) umschreiben
GHSA-Begründung (GHSA-846f-f764-76mx high, GHSA-2fmv-rv2c-59x7 moderate) auf die 11.x-Linie übertragen; Formulierung nicht mehr wörtlich an `>= 10.0.13`/„10.x" binden. AngleSharp-Kommentar: geprüft, version-neutral → unverändert.

### 5. CI-Dateien
- 9 × `dotnet-version: '10.0.x'` → `'11.0.x'` (je Stil der Zeile: quoted/unquoted).
- `action.yml:57`: `--framework net10.0` → `--framework net11.0`.
- Kosmetik (A2): `Setup .NET 10` → `Setup .NET 11` (8 Stellen), action.yml-Description `.NET 10 SDK` → `.NET 11 SDK`.

### 6. Kommentar/Docs
- `PlaywrightTestBase.cs:27`: `bin/<cfg>/net10.0/` → `bin/<cfg>/net11.0/`.
- `README.md`: Badge `.NET-10.0` → `.NET-11.0`, Badge `EF_Core-10` → `EF_Core-11`, `.NET 10.0 Runtime` → `.NET 11.0 Runtime`.
- `docs/help/schnittstellenzentrale/installation.md:6`: `.NET 10.0 Runtime (ASP.NET Core)` → `.NET 11.0 …`.
- `docs/help/playwright-tests/installation.md:5`: `.NET 10.0 SDK` → `.NET 11.0 SDK`.

### 7. Verifikation
1. `dotnet restore` + `dotnet build Schnittstellenzentrale.slnx` unter SDK 11.0.100 → 0 Fehler. (Playwright-Install via `InstallPlaywright`-Target läuft mit.)
2. `dotnet test --filter "Category!=OsInterface"` → alle grün (Baseline 641; Abweichungen dokumentieren).
3. `dotnet test --filter "FullyQualifiedName~ODataControllerIntegrationTests"` → 78 grün (A4-Verifikation).
4. `dotnet list package --vulnerable` → kein Befund.
5. Repo-Grep: keine `10.0.*`-/`net10.0`-Nennungen außerhalb `issue.md`, `docs/projects/`, `docs/features/` und Build-Artefakten.
6. Reale E2E-Abnahme: App starten, `GET /` (HTTP 200) + `GET /odatav4/Applications` (HTTP 200 JSON) → `test-results.md`.

## Akzeptanzszenarien

- **S1 (Happy Path Build):** `dotnet build Schnittstellenzentrale.slnx` unter SDK 11.0.100 endet mit 0 Fehlern; Ausgabepfade enthalten `net11.0`.
- **S2 (Testsuite):** `dotnet test --filter "Category!=OsInterface"` — alle gefundenen Tests grün, Anzahl ≈ 641 (Baseline).
- **S3 (OData-Preview unter net11):** `ODataControllerIntegrationTests` 78/78 grün → Beweis, dass `Microsoft.AspNetCore.OData 10.0.0-preview.2` unter net11.0 lauffähig ist.
- **S4 (Security-Gate):** `dotnet list package --vulnerable` ohne vulnerable Pakete.
- **S5 (Sauberkeit):** Grep nach `10\.0\.\*` und `net10\.0` über dem Repo liefert nur Treffer in historischen Dokumenten/Build-Artefakten.
- **S6 (E2E):** Anwendung startet unter net11.0, `GET /` liefert die Blazor-App, `GET /odatav4/Applications` liefert OData-JSON.

## Offene Punkte

Keine — Rückfragen sind in diesem Lauf nicht möglich; alle Unklarheiten sind als Annahmen A1–A5 dokumentiert (vom Auftrag so vorgegeben).
