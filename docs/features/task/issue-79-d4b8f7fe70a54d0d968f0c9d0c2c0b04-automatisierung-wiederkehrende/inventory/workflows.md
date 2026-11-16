# Bestehende Workflows unter `.github/workflows/`

Sieben Workflow-Dateien. Übergreifende Konventionen:

- Englischer Kommentarblock direkt unter `name:` am Dateianfang mit Zweckbeschreibung und Verweisen auf `ci-target-schema.md` (diese Datei liegt **nicht** in diesem Repository — sie wird in den Kommentaren als externes Referenzdokument des Softwareschmiede-Templates zitiert, z. B. „1:1 from Rezepte/Softwareschmiede/msTools.Updater").
- Inline-Kommentare (englisch) erläutern nicht-offensichtliche Entscheidungen.
- Actions-Versionen: `actions/checkout@v7`, `actions/setup-dotnet@v6` (.NET `10.0.x`), `actions/setup-node@v7` (Node 22), `actions/upload-artifact@v7` (retention-days: 14).
- Shell: überwiegend `shell: pwsh` pro Step (auch auf ubuntu-latest); `detect-backmerge`-Jobs und `verify-pr-source.yml` verwenden die Default-Shell `bash`.
- `timeout-minutes` ist an Jobs gesetzt, wo ein Job nennenswert läuft (10–30 min); fehlt bei reinen Check-Jobs (`detect-backmerge`, `verify-source`).

## `security-scan.yml` — „Security Scan"

Datei: `.github/workflows/security-scan.yml` (41 Zeilen)

| Aspekt | Wert |
|--------|------|
| Trigger | `schedule: cron '0 4 * * 1'` (wöchentlich Montag 04:00 UTC, Kommentar Zeile 10–11) + `workflow_dispatch` |
| `permissions:` | **kein Block vorhanden** |
| `concurrency` | `security-scan-${{ github.workflow }}-${{ github.ref }}`, `cancel-in-progress: true` |
| Job `security-scan` | `runs-on: windows-latest`, `timeout-minutes: 20` |
| Schritte | `actions/checkout@v7` → `actions/setup-dotnet@v6` (10.0.x) → `dotnet restore Schnittstellenzentrale.slnx` (pwsh) → Composite Action `./.github/actions/security-scan` mit `solution-path`/`artifact-name: vulnerable-packages-scan` |

Fachlich relevant: Führt bereits **wöchentlich** einen NuGet-Vulnerability-Scan durch (`dotnet list package --vulnerable --include-transitive`), schlägt bei Befund fehl (Exit 1) und lädt ein Log-Artefakt hoch. Erzeugt **kein** Issue. Dasselbe Composite läuft als „Gate 2" in `static-checks` von `pr-staging-ci.yml` und `staging-ci.yml`.

## `sync-staging-with-main.yml` — „Backmerge Main to Staging"

Datei: `.github/workflows/sync-staging-with-main.yml` (76 Zeilen)

| Aspekt | Wert |
|--------|------|
| Trigger | `push` auf `main` |
| `permissions:` | `contents: read`, `pull-requests: write`, `issues: write` (Zeilen 21–24) |
| `concurrency` | `sync-staging-with-main`, `cancel-in-progress: false` |
| Job `backmerge` | `runs-on: ubuntu-latest`, `timeout-minutes: 10` |
| gh-CLI-Muster | `env: GH_TOKEN: ${{ github.token }}` pro Step (Zeilen 55–56, 62–63) |
| Label-Muster | `gh label create automated-backmerge --color "1D76DB" --description "..." --force` (Zeile 57) |
| Duplikat-Muster | `gh pr list --base staging --head main --state open --json number \| ConvertFrom-Json`; nur bei `Count -eq 0` wird `gh pr create` ausgeführt (Zeilen 65–75) |

## `staging-to-main-promotion.yml` — „Staging to Main Promotion"

Datei: `.github/workflows/staging-to-main-promotion.yml` (83 Zeilen)

| Aspekt | Wert |
|--------|------|
| Trigger | `workflow_run` auf `workflows: ["Pre-Release"]`, `types: [completed]`, `branches: [staging]` — referenziert den **Display-Namen** von `staging-ci.yml` (Kommentar Zeilen 7–10) |
| `permissions:` | `contents: read`, `pull-requests: write`, `issues: write` (Zeilen 20–23) |
| `concurrency` | `staging-to-main-promotion`, `cancel-in-progress: false` |
| Job `promote` | `if: github.event.workflow_run.conclusion == 'success'`, `runs-on: ubuntu-latest`, `timeout-minutes: 15` |
| Label-Muster | `gh label create automated-promotion --color "0E8A16" --description "..." --force` (Zeile 63) |
| Duplikat-Muster | `gh pr list --base main --head staging --state open --json number`, dann `gh pr create ... --draft --label "automated-promotion"` (Zeilen 71–82) |

## `staging-ci.yml` — „Pre-Release"

Datei: `.github/workflows/staging-ci.yml` (303 Zeilen)

| Aspekt | Wert |
|--------|------|
| Trigger | `push` auf `staging` |
| `permissions:` | `contents: write`, `pull-requests: write` (Zeilen 24–26) |
| `concurrency` | `staging-ci`, `cancel-in-progress: true` |
| Jobs | `detect-backmerge` (ubuntu-latest, bash-Merge-Parent-/Tree-Diff-Check, liefert Output `is_backmerge`) → `static-checks` (windows-latest, 15 min: `dotnet format`, Security-Scan-Gate 2, Static Analysis mit `TreatWarningsAsErrors`) → `build-and-test` (windows-latest, 30 min: Build, `dotnet test` ohne Playwright, Playwright best-effort `continue-on-error: true`, Coverage-Report + 70-%-Gate, drei Artefakt-Uploads) → `version` (ubuntu-latest, 10 min, `npx semantic-release --dry-run` pwsh, RC-Tag-Ermittlung) → `prerelease` (windows-latest, 30 min) |
| gh-CLI | `GH_TOKEN` im `prerelease`-Job: `gh api repos/.../releases/generate-notes` (Zeile 270) und `gh release create <tag> --prerelease` (Zeile 298) |

## `pr-staging-ci.yml` — „PR CI for Staging"

Datei: `.github/workflows/pr-staging-ci.yml` (216 Zeilen)

| Aspekt | Wert |
|--------|------|
| Trigger | `pull_request` auf `staging`, `types: [opened, synchronize, reopened]` |
| `permissions:` | `contents: read`, `checks: write` (Zeilen 24–26) |
| `concurrency` | `pr-staging-${{ github.event.pull_request.number }}`, `cancel-in-progress: true` |
| Jobs | `detect-backmerge` (ubuntu-latest, bash), `static-checks` (windows-latest, 15 min), `build-and-test` (windows-latest, 30 min), `back-merge-skip` (ubuntu-latest, Platzhalter-Echo) |

## `release.yml` — „Release"

Datei: `.github/workflows/release.yml` (174 Zeilen)

| Aspekt | Wert |
|--------|------|
| Trigger | `push` auf `main` und Tags `v*.*.*` |
| `permissions:` | `contents: write`, `issues: write`, `pull-requests: write` (Zeilen 18–21) |
| `concurrency` | `release`, `cancel-in-progress: false` (Kommentar Zeilen 23–26 begründet Serialisierung) |
| Job `release` | `runs-on: windows-latest`, `timeout-minutes: 30` |
| gh-CLI | `GH_TOKEN` für `gh api .../generate-notes` (Zeile 115), `gh release create --verify-tag` (Zeile 163, manueller Tag-Pfad), `gh release upload --clobber` (Zeile 174, Asset-Repair-Pfad); automatisierter Pfad via `npm run release` (semantic-release) mit `GITHUB_TOKEN` (Zeilen 141–146) |

## `verify-pr-source.yml` — „Verify PR Source"

Datei: `.github/workflows/verify-pr-source.yml` (22 Zeilen)

| Aspekt | Wert |
|--------|------|
| Trigger | `pull_request` auf `main` (alle Typen) |
| `permissions:` | `permissions: {}` — explizit leer (Zeile 9) |
| `concurrency` | nicht gesetzt |
| Job `verify-source` | `runs-on: ubuntu-latest`, bash-Einzeiler; schlägt fehl, wenn `github.head_ref != "staging"` |

## Zeitgesteuerte Trigger — Übersicht

Nur **ein** Workflow hat aktuell einen `schedule`-Trigger: `security-scan.yml` (`0 4 * * 1`). `workflow_dispatch` existiert ebenfalls nur dort.
