# Workflow-Konventionen (.github/workflows/)

10 Workflows unter `.github/workflows/`; dazu drei lokale Composite Actions unter `.github/actions/` (`security-scan`, `build-and-package`, `create-review-issue`).

## Übergreifende Konventionen

| Aspekt | Konvention |
|--------|-----------|
| Kopf | `name:`-Feld in Zeile 1, danach mehrzeiliger **englischer** `#`-Kommentarblock mit Zweck, Herkunft (Verweise auf `ci-target-schema.md`, z. B. „1:1 from Rezepte/Softwareschmiede/msTools.Updater") und Fallstricken |
| `permissions` | Immer explizit und minimal; `verify-pr-source.yml` nutzt `permissions: {}` (leer) |
| `concurrency` | Fast immer gesetzt; Gruppenname = Workflow-Name bzw. `workflow-ref`-Kombination; `cancel-in-progress: true` für CI/Scans, `false` für Release/Promotion/Issue-Erzeugung |
| `timeout-minutes` | Auf nahezu allen Jobs (5–30 min) |
| Runner | `ubuntu-latest` für Git-/Orchestrierungs-/Issue-Jobs; `windows-latest` für .NET-Build/Test/Security-Scan (App ist Windows-only: Negotiate/Windows-Auth, EventLog-Sink) |
| Shell | `shell: pwsh` explizit pro Step — auch auf `ubuntu-latest`; Bash (Default) nur in `detect-backmerge`-Jobs |
| Actions-Versionen | `actions/checkout@v7`, `actions/setup-dotnet@v6` (`dotnet-version: '10.0.x'`), `actions/setup-node@v7` (`node-version: 22`, `cache: npm`), `actions/upload-artifact@v7` (`retention-days: 14`) |
| Step-Kommentare | Umfangreiche Inline-`#`-Kommentare in englischer Sprache an komplexen Steps |
| Trigger-Muster | `push: branches: [staging]`/`[main]`, `pull_request: branches: [staging]`/`[main]`, `schedule: cron` (UTC), `workflow_run` (auf Display-Name!), `workflow_dispatch` bei allen Schedule-Workflows |

## Bestehende Workflows

| Datei | `name:` | Trigger | Runner | permissions | concurrency |
|-------|---------|---------|--------|-------------|-------------|
| `staging-ci.yml` | Pre-Release | `push` → `staging` | windows-latest (Build/Test), ubuntu-latest (detect-backmerge, version) | `contents: write`, `pull-requests: write` | `staging-ci`, cancel: true |
| `pr-staging-ci.yml` | PR CI for Staging | `pull_request` → `staging` | windows-latest | `contents: read`, `checks: write` | `pr-staging-${PR-Nr}`, cancel: true |
| `release.yml` | Release | `push` → `main`, `tags: v*.*.*` | windows-latest | `contents: write`, `issues: write`, `pull-requests: write` | `release`, cancel: false |
| `staging-to-main-promotion.yml` | Staging to Main Promotion | `workflow_run` „Pre-Release" completed auf `staging` | ubuntu-latest | `contents: read`, `pull-requests: write`, `issues: write` | `staging-to-main-promotion`, cancel: false |
| `sync-staging-with-main.yml` | Backmerge Main to Staging | `push` → `main` | ubuntu-latest | `contents: read`, `pull-requests: write`, `issues: write` | `sync-staging-with-main`, cancel: false |
| `verify-pr-source.yml` | Verify PR Source | `pull_request` → `main` | ubuntu-latest | `{}` (leer) | — (nicht gesetzt) |
| `security-scan.yml` | Security Scan | `schedule` Mo 04:00 UTC + `workflow_dispatch` | windows-latest | — (nicht gesetzt) | `security-scan-${workflow}-${ref}`, cancel: true |
| `monthly-architecture-security-review.yml` | Monthly Architecture & Security Review | `schedule` 1. d. Monats 05:00 UTC + `workflow_dispatch` | ubuntu-latest | `contents: read`, `issues: write` | `monthly-architecture-security-review`, cancel: false |
| `weekly-nuget-review.yml` | Weekly NuGet Package Review | `schedule` Mo 06:00 UTC + `workflow_dispatch` | ubuntu-latest | `contents: read`, `issues: write` | `weekly-nuget-review`, cancel: false |
| `yearly-compliance-review.yml` | Yearly Compliance & Copyright Review | `schedule` 1. Jan. 05:00 UTC + `workflow_dispatch` | ubuntu-latest | `contents: read`, `issues: write` | `yearly-compliance-review`, cancel: false |

## Relevante Einzelheiten

- **Review-Workflows** (`monthly-architecture-security-review.yml`, `weekly-nuget-review.yml`, `yearly-compliance-review.yml`): kleinstes Workflow-Muster im Repo — ein Job, `ubuntu-latest`, `timeout-minutes: 5`, Steps = `checkout` + lokale Composite Action `./.github/actions/create-review-issue`. `workflow_dispatch` ist hier Konvention (Kommentarzeile 11–12 in `monthly-architecture-security-review.yml`).
- **`staging-ci.yml` / `pr-staging-ci.yml`**: Test-Befehl `dotnet test Schnittstellenzentrale.slnx --filter "FullyQualifiedName!~Playwright"`; Playwright-Suite läuft als eigener Step mit `continue-on-error: true`; Coverage-Gate 70 % via ReportGenerator.
- **`release.yml`**: `concurrency: group: release, cancel-in-progress: false` — Kommentar begründet Serialisierung (Race um Tag/Version). Erzeugt GitHub Releases mit ZIP-Asset via `gh release create`/`semantic-release`.
- **Kein Pages-Deployment vorhanden:** `grep` über `.github/` findet keinerlei `actions/configure-pages`, `actions/upload-pages-artifact`, `actions/deploy-pages`, `github-pages`-Environment oder `pages:`-Permission. Es existiert kein `site/`-, `docs/page/`- oder sonstiger Seiten-Quellordner, keine `_config.yml`, `CNAME` oder `.nojekyll`.
- **Composite Actions** unter `.github/actions/`: `build-and-package` (self-contained win-x64 Publish + Update-Manifest), `security-scan` (`dotnet list package --vulnerable`), `create-review-issue` (Label + Issue mit Duplikat-Check).
- **Sonstiges:** `package.json` + `release.config.cjs` im Root verdrahten semantic-release (Release-Branch nur `main`); `scripts/` enthält Node-Helfer (`resolve-release-version.mjs`, `generate-update-manifest.mjs`, `verify-release-version.cjs`) inkl. `*.test.mjs`-Tests — Konvention für Repo-Tooling-Skripte.
