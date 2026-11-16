# Bestehende Issue-/Label-Automatisierung

## Issue-Erzeugung

- **Es gibt keinen Workflow und keine Action, die GitHub-Issues erzeugt.** `gh issue create` und `actions/github-script` kommen in `.github/` nicht vor (Repo-weite Suche bestätigt: nur Treffer in `docs/features/.../requirement.md`).
- Automatisiert werden ausschließlich **Pull Requests** (`gh pr create`) und **Releases** (`gh release create/upload`, `gh api`) erzeugt.
- Keine `.github/ISSUE_TEMPLATE`-Dateien, kein `dependabot.yml`, kein `CODEOWNERS` — `.github/` enthält nur `actions/` und `workflows/`.

## Etabliertes gh-CLI-Muster (aus `sync-staging-with-main.yml`, `staging-to-main-promotion.yml`)

1. **Token:** `env: GH_TOKEN: ${{ github.token }}` auf Step-Ebene gesetzt (`sync-staging-with-main.yml` Zeilen 55–56, 62–63; `staging-to-main-promotion.yml` Zeilen 61–62, 68–69; `staging-ci.yml` Zeilen 268, 292; `release.yml` Zeilen 112, 157, 168).
2. **Label vorab sicherstellen:** `gh label create <name> --color "<hex>" --description "<text>" --force` in einem eigenen Step „Ensure <name> label exists" — Labels werden idempotent per `--force` angelegt:
   - `automated-backmerge` (Farbe `1D76DB`, `sync-staging-with-main.yml` Zeile 57)
   - `automated-promotion` (Farbe `0E8A16`, `staging-to-main-promotion.yml` Zeile 63)
3. **Duplikat-Vermeidung:** vor `gh pr create` wird per `gh pr list --base <ziel> --head <quelle> --state open --json number | ConvertFrom-Json` geprüft, ob bereits ein offenes PR desselben Typs existiert; nur bei leerem Ergebnis wird angelegt, sonst `Write-Host`-Meldung (`sync-staging-with-main.yml` Zeilen 65–75, `staging-to-main-promotion.yml` Zeilen 71–82).
4. **Permissions:** `issues: write` ist bereits in drei Workflows gesetzt (`sync-staging-with-main.yml`, `staging-to-main-promotion.yml`, `release.yml`), obwohl dort keine Issues erzeugt werden — vermutlich aus dem Vorlagen-Schema übernommen.

## `@softwarewerkstatt`-Erwähnung

- Keine Stelle im Repository außer `issue.md`/`requirement.md` reagiert auf oder dokumentiert `@softwarewerkstatt`. Kein Workflow, keine Doku, kein Script erwähnt den Account.
- Es existiert keine beschriebene Automatisierung, die Issue-Erwähnungen verarbeitet (die Erwähnung soll offenbar einen externen Agenten-Prozess anstoßen — außerhalb dieses Repos nicht dokumentiert).

## Runner-/Shell-Wahl für gh-Aufrufe

- `gh`-aufrufende Jobs laufen auf **`ubuntu-latest`** (`backmerge`, `promote`, `version` in `staging-ci.yml`) oder `windows-latest` (Release-Kontext). `gh` ist auf GitHub-Hosted-Runners vorinstalliert.
- `shell: pwsh` wird auch auf ubuntu-latest explizit gesetzt; einfache Git-/echo-Schritte verwenden teils die bash-Default-Shell (`detect-backmerge`, `verify-pr-source.yml`).
