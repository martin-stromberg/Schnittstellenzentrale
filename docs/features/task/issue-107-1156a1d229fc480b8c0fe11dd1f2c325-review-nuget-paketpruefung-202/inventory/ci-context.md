# CI-Kontext, npm und Build-Pins

Stand: 2026-11-23. Nur Hinweis-Daten — npm devDependencies und Actions-Pins sind laut Anforderung Scope-Ausschluss.

## global.json (SDK-Pin)

```json
{ "sdk": { "version": "10.0.301", "rollForward": "latestFeature" } }
```

Installiertes/aktives SDK: **10.0.404** (rollForward latestFeature → 10.0.4xx-Featureband wird verwendet). CI installiert `10.0.x` via `actions/setup-dotnet@v6`.

## Relevante Gates für einen Update-PR gegen `staging`

### `.github/workflows/pr-staging-ci.yml` (PR → staging, blockierend)

| Gate | Schritt | Befehl / Action | Blockierend? |
|------|---------|-----------------|--------------|
| 1 Format | `static-checks` | `dotnet format Schnittstellenzentrale.slnx --verify-no-changes --no-restore --severity error` | ja |
| 2 Security-Scan | `static-checks` | `./.github/actions/security-scan` → `dotnet list Schnittstellenzentrale.slnx package --vulnerable --include-transitive`; schlägt fehl bei Match auf „has the following vulnerable packages" oder „Severity" | ja |
| 3 Statische Analyse | `static-checks` | `dotnet build ... -c Release -p:TreatWarningsAsErrors=true` | ja |
| 4 Unit-Tests | `build-and-test` | `dotnet test ... -c Release --no-build --filter "Category!=OsInterface"` + Coverage | ja |
| 5 Coverage | `build-and-test` | ReportGenerator, Line-Coverage ≥ **70 %** | ja |
| — OS-Interface | `os-interface-tests` | `dotnet test --filter "Category=OsInterface"` (Playwright, Kestrel 5099–5104, Temp-FS) | **nein** (`continue-on-error: true`, best-effort) |
| — Backmerge-Check | `detect-backmerge` | reine main-Backmerges überspringen alle Checks | — |

### `.github/workflows/security-scan.yml`

Wöchentlicher Standalone-Scan (Mo 04:00 UTC) + `workflow_dispatch`; nutzt dieselbe Composite Action — findet ein Update neue transitive Vulnerabilities, würde auch dieser Workflow rot.

### `.github/workflows/staging-ci.yml` („Pre-Release", Push auf staging)

Identische Gate-Struktur: static-checks (Format, Security-Scan `vulnerable-packages-staging`, TreatWarningsAsErrors-Build), build & test (gleicher Filter + 70 %-Coverage), os-interface-tests (best-effort). Danach Paketierung via `./.github/actions/build-and-package`.

### `.github/actions/security-scan/action.yml` (Kernlogik)

```pwsh
dotnet list <sln> package --vulnerable --include-transitive
# exit 1 wenn Output "has the following vulnerable packages" oder "Severity" matcht
```

Hinweis: Der Matcher reagiert auf den englischen CLI-Output; die Action läuft in der CI (en-US). Lokal lieferte derselbe Befehl deutsche Ausgabe („keine anfälligen Pakete").

## npm-Seite (Hinweis, kein Scope)

`package.json` — Release-Automatisierung, `private: true`, keine `dependencies`, nur `devDependencies`:

| Paket | Deklariert |
|-------|-----------|
| `semantic-release` | ^25.0.5 |
| `@semantic-release/commit-analyzer` | ^13.0.1 |
| `@semantic-release/github` | ^11.0.6 |
| `@semantic-release/release-notes-generator` | ^14.0.3 |
| `conventional-changelog-conventionalcommits` | ^9.1.0 |

Scripts: `release` → `semantic-release`; `test:release-version` → `node --test` auf zwei Skript-Tests.

## GitHub-Actions-Pins (Hinweis, kein Scope)

Verwendete externe Actions (eindeutige Pins über alle Workflows/Composite Actions):

- `actions/checkout@v7`
- `actions/setup-dotnet@v6` (`dotnet-version: 10.0.x`)
- `actions/setup-node@v7`
- `actions/upload-artifact@v7`
- `actions/configure-pages@v6`, `actions/upload-pages-artifact@v5`, `actions/deploy-pages@v5`
- Lokal: `./.github/actions/build-and-package`, `./.github/actions/security-scan`, `./.github/actions/create-review-issue`
- Tool-Pin: `dotnet-reportgenerator-globaltool --version 5.5.11`

## Wöchentlicher Review-Workflow (Kontext)

`.github/workflows/weekly-nuget-review.yml` legt montags 06:00 UTC das Review-Issue neu an (Duplikatsprüfung + Label `automated-nuget-review` via `./.github/actions/create-review-issue`). Bleibt laut Scope unverändert.
