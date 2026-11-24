# Anforderung – .NET-11-Basisupgrade (TFM, SDK, CI, Floating-Baseline)

**Quelle:** Schritt 1 des Projekts „NuGet-Updates mit hohem Risiko / Architektur-Entscheidungen" (Issue #108), Projektplan `docs/projects/task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko/project-plan.md`. Kundenanforderung wörtlich übernommen.

**Kontext:** Die Lösung steht auf `net10.0` / SDK 10.0.301. SDK 11.0.100 ist installiert. Dieser Schritt hebt ausschließlich die .NET-Baseline — keine der Issue-Befunde F1–F8 wird hier fachlich umgesetzt (außer der impliziten F7-Arbeitsannahme „Floating beibehalten").

## Ziel

Alle 4 Projekte der `Schnittstellenzentrale.slnx` laufen unter `net11.0` mit SDK 11.0.100; CI-Pins, Floating-Referenzen und harte net10-Nennungen folgen auf die 11er-Linie. `Microsoft.AspNetCore.OData 10.0.0-preview.2` (net10-only) bleibt gepinnt und wird unter net11.0 per TFM-Kompatibilität aufgelöst — seine Lauffähigkeit ist Teil der Verifikation.

## Fachliche Anforderungen

1. **TFM:** `<TargetFramework>net10.0</TargetFramework>` → `net11.0` in allen 4 Projekten:
   - `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`
   - `src/Schnittstellenzentrale.Core/Schnittstellenzentrale.Core.csproj`
   - `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj`
   - `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj`
2. **SDK:** `global.json` `10.0.301` → `11.0.100`, `rollForward: latestFeature` beibehalten.
3. **CI:** alle 9 `dotnet-version: '10.0.x'`-Pins → `'11.0.x'` in `.github/workflows/pr-staging-ci.yml` (3×), `staging-ci.yml` (3×), `security-scan.yml` (1×), `release.yml` (1×) und `.github/actions/build-and-package/action.yml` (1×).
4. **Harter Publish-TFM:** `--framework net10.0` → `net11.0` in `build-and-package/action.yml` (Zeile 57).
5. **Floating-Baseline:** alle 17 `10.0.*`-PackageReferences → `11.0.*` (Floatings lösen unter net11 nicht automatisch auf 11.x auf). Dabei:
   - `System.Security.Cryptography.Pkcs`-Pin-Kommentar (`Infrastructure.csproj`, GHSA-846f-f764-76mx / GHSA-2fmv-rv2c-59x7, wörtlich an 10.x gebunden) auf die 11.x-Linie anpassen.
   - AngleSharp-Override-Kommentar (`Tests.csproj`) prüfen — er ist version-neutral formuliert (Pin `>= 1.5.0` wegen bunit-Transitivauflösung), keine 10.x-Bindung.
6. **Harte net10-Nennungen korrigieren:**
   - `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightTestBase.cs:27` (Kommentar `bin/<cfg>/net10.0/`)
   - `README.md` (Badge `.NET-10.0`, Voraussetzung `.NET 10.0 Runtime`, Badge `EF_Core-10`)
   - `docs/help/schnittstellenzentrale/installation.md` (`.NET 10.0 Runtime`)
   - `docs/help/playwright-tests/installation.md` (`.NET 10.0 SDK`)
   - Kosmetische `.NET 10`-Nennungen in CI-Schrittnamen/-Beschreibungen (`Setup .NET 10`, action.yml-Description) folgen ebenfalls — Akzeptanzkriterium „keine verbleibenden Nennungen".
7. **`Microsoft.AspNetCore.OData 10.0.0-preview.2` bleibt gepinnt** — net10-only-Paket, Auflösung unter net11.0 per TFM-Kompatibilität; Lauffähigkeit über die 78 `ODataControllerIntegrationTests` verifizieren.

## Akzeptanzkriterien

- `dotnet build` der `Schnittstellenzentrale.slnx` unter SDK 11.0.100 fehlerfrei.
- `dotnet test --filter "Category!=OsInterface"` vollständig grün (Baseline: 641 Tests).
- Die 78 `ODataControllerIntegrationTests` grün (Verifikation preview.2 unter net11).
- `dotnet list package --vulnerable` ohne Befund.
- Keine verbleibenden `10.0.*`-/`net10.0`-Nennungen außerhalb historischer Dokumente.
- Reale End-to-End-Abnahme: Anwendung real starten, mindestens `GET /` und falls schnell erreichbar ein OData-Endpunkt; Ergebnis in `test-results.md`.

## Explizite Nicht-Ziele / Scope-Ausschlüsse

- Keine Umsetzung der Issue-Befunde F1–F8 (Playwright-Update, xunit.v3, OpenApi/Swashbuckle, Serilog, OData.Edm, FrameworkReference/SignalR.Client-Verschiebung, Lock/CPM-Entscheidung, npm/Actions-Pins) — diese folgen in eigenen Schritten.
- Kein `packages.lock.json`/`Directory.Packages.props` (F7-Arbeitsannahme: Floating beibehalten).
- Keine OsInterface-/Playwright-Lane erforderlich — nur reguläre Lane.
- Kein Merge, kein Push, kein Pull Request, keine Issue-Kommentare.
- Keine inhaltliche Änderung an Paketen außer den 17 Floating-Versionsstrings `10.0.*` → `11.0.*`.

## Offene Fragen

Keine (Rückfragen sind in diesem Lauf nicht möglich; Unklarheiten werden als Annahmen in `plan.md` dokumentiert).
