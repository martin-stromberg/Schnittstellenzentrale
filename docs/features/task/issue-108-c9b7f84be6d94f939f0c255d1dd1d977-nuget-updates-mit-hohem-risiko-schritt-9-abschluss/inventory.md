# Bestandsaufnahme – Schritt 9: F7-Prozessentscheidung + F8-Auslagerung

Erstellt gegen HEAD `9c698c9` (Schritt-Branch, Stand nach Merge der Schritte 1–8 in den
Basisbranch + Schritt-9-Abzweig).

## 1. Ausgangslage `decisions.md`

Datei: `docs/projects/task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko/decisions.md`

Vorhandene Einträge (alle mit Status „Entschieden und umgesetzt", Begründung,
Alternativen, Konsequenzen):

| Entscheidung | Inhalt | Schritt | Vollständig? |
|---|---|---|---|
| F3 | Microsoft.OpenApi 3.11.1 + Swashbuckle.AspNetCore 11.0.1 (koordiniert) | 5 | ja |
| F5 | AspNetCore.OData auf `10.0.0-preview.2` verharren + Edm/Core/Spatial 9.0.0 + ModelBuilder 3.0.0; **Exit-Kriterium dokumentiert** (Umstieg auf stabile Linie sobald stabiler 10.x-/11.x-Release verfügbar) | 3 | ja |
| F6 | Volle `FrameworkReference Microsoft.AspNetCore.App`-Variante in Infrastructure; `SignalR.Client` ins Web-Projekt verschoben; NU1510-Folgebereinigung inkl. Pkcs | 2 | ja |
| F4 | Serilog.AspNetCore 11.0.0, Sinks unverändert | 4 | ja |
| F1 | Microsoft.Playwright 1.65.1 + lokales OsInterface-Gate | 7 | ja |
| F2 | xunit → xunit.v3 4.0.3 (MTP) | 8 | ja |
| **F7** | **fehlt** — Prozessentscheidung Floating vs. Lock/CPM | 9 | **einzutragen** |
| **F8** | **fehlt** — Auslagerung npm/Actions | 9 | **einzutragen** |

Reihenfolge im Dokument ist nicht numerisch sortiert (F3, F5, F6, F4, F1, F2) — neue
Einträge F7/F8 werden hinten angehängt (Konsistent mit der bisherigen Praxis, in der
jeder Schritt seinen Eintrag anfügt).

## 2. F7-Faktenlage (Paketversions-Strategie)

- **Floating-Referenzen `11.0.*`: 11 Stück** (exakte Zählung via grep über alle csproj):
  - `Schnittstellenzentrale.csproj` (3): `Microsoft.AspNetCore.Authentication.Negotiate`,
    `Microsoft.AspNetCore.SignalR.Client`, `Microsoft.EntityFrameworkCore.Design`
  - `Schnittstellenzentrale.Infrastructure.csproj` (4): `Microsoft.EntityFrameworkCore`,
    `…Design`, `…Sqlite`, `…SqlServer`
  - `Schnittstellenzentrale.Tests.csproj` (4): `Microsoft.AspNetCore.Mvc.Testing`,
    `Microsoft.EntityFrameworkCore.Sqlite`, `…SqlServer`, `Microsoft.JSInterop`
  - `Schnittstellenzentrale.Core.csproj`: keine Floating-Referenz (nur `Microsoft.OpenApi 3.11.1` gepinnt)
- **Keine** `packages.lock.json` und **kein** `Directory.Packages.props` im Repo
  (verifiziert via `find`).
- **Pkcs-Kommentar-Lage:** Die ursprüngliche `System.Security.Cryptography.Pkcs`-Floating-
  Begründung (GHSA-846f-f764-76mx, GHSA-2fmv-rv2c-59x7 — Security-Patches sollen implizit
  einfließen) wurde in Schritt 2 (F6) überführt: Pkcs kommt jetzt aus der
  Runtime-Installation; der Kommentar in `Infrastructure.csproj:7-23` dokumentiert
  diesen Übergang. Die **Floating-Strategie selbst** bleibt für die 11 verbliebenen
  `11.0.*`-Referenzen die bewusste Wahl (Framework-Patches fließen implizit mit).
- **Etablierte Prüfpraxis:** `.github/workflows/weekly-nuget-review.yml` erzeugt
  wöchentlich (Mo 06:00 UTC) ein Review-Issue (Label `automated-nuget-review`,
  Duplikat-Schutz via `./.github/actions/create-review-issue`); die Bearbeitungskette
  Issue #107 → #108 ist die konkrete Ausprägung dieser Praxis. Doku:
  `docs/help/review-automatisierung/`.

## 3. F8-Faktenlage (Nicht-NuGet-Abhängigkeiten)

- **npm `devDependencies`** in `./package.json` (Root, „Release-Automatisierung",
  private): `semantic-release ^25.0.5`, `@semantic-release/commit-analyzer ^13.0.1`,
  `@semantic-release/github ^11.0.6`, `@semantic-release/release-notes-generator ^14.0.3`,
  `conventional-changelog-conventionalcommits ^9.1.0` — Caret-Ranges, kein separater
  Review-Rhythmus; Skripte `release`, `test:release-version`
  (`scripts/resolve-release-version.test.mjs`, `scripts/generate-update-manifest.test.mjs`).
- **GitHub-Actions-Pins:** 11 Workflows + 3 Composite Actions unter `.github/`.
  Alle externen `uses:` sind **Major-Tag-Pins** (`actions/checkout@v7`,
  `actions/setup-dotnet@v6`, `actions/setup-node@v7`, `actions/upload-artifact@v7`,
  `actions/configure-pages@v6`, `actions/upload-pages-artifact@v5`,
  `actions/deploy-pages@v5`) — keine SHA-Pins. Eine Dependabot-/Renovate-Konfiguration
  (`.github/dependabot.yml` o. ä.) existiert **nicht** — weder für `github-actions`
  noch für npm oder NuGet.
- `gh` ist authentifiziert (`GH_TOKEN`, Account `softwarewerkstatt`) — Issue-Anlage und
  Kommentar auf `martin-stromberg/Schnittstellenzentrale` möglich (Berechtigung wird im
  Implementierungsschritt empirisch bestätigt; Fallback = Titel+Skizze in decisions.md).

## 4. Verifikations-Baseline

- Reguläre Lane erwartet: **641 Tests grün** (`Category!=OsInterface`,
  MTP-Modus: `dotnet test … -- --filter "Category!=OsInterface"` — Kommandoform aus
  Schritt 8; der in der Anforderung genannte Filter ist identisch, die `--`-Trennung
  wird von `dotnet test` unter MTP benötigt bzw. von `global.json` `test.runner`
  gesteuert — empirisch in Schritt 8 verifiziert: `--filter` funktioniert auch direkt).
- Kein Produktcode-Diff erwartet → Testzahl unverändert.

## 5. Konventionen/Randbedingungen

- Commit-Konvention der Schritte: `plan:` (Planungscommit), `docs:`/`feat:`
  (Abschlusscommit). Kein Closing-Keyword (Abschluss-PR folgt orchestrator-seitig).
- `steps.md` bleibt auf diesem Branch „In Arbeit" (Status „Fertig" wird auf dem
  Projekt-Basisbranch nach dem Merge gesetzt — beobachtet in Schritt 8).
- Dokumentationssprache: Deutsch; GitHub-Inhalte deutsch.
