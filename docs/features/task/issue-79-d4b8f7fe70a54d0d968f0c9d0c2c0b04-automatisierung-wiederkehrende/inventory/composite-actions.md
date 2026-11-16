# Composite Actions unter `.github/actions/`

Zwei Composite Actions (`runs.using: composite`). Beide tragen einen ausführlichen englischen `description:`-Block, der Zweck und Wiederverwendung („so this logic only exists once") beschreibt und auf `ci-target-schema.md` sowie das Vorlagen-Projekt (FinanceManager/Rezepte) verweist.

## `security-scan` — „Security scan"

Datei: `.github/actions/security-scan/action.yml` (44 Zeilen)

| Aspekt | Wert |
|--------|------|
| Inputs | `solution-path` (optional, default `Schnittstellenzentrale.slnx`), `artifact-name` (optional, default `vulnerable-packages-scan`) |
| Schritt 1 | `dotnet list <sln> package --vulnerable --include-transitive` (pwsh), Ausgabe in `vulnerable-packages-scan.log`; bei Match auf „has the following vulnerable packages" oder „Severity": `::error::` + `exit 1` |
| Schritt 2 | `actions/upload-artifact@v7` mit `if: always()`, `retention-days: 14` |
| Verwendung | `security-scan.yml` (wöchentlicher Lauf), `pr-staging-ci.yml` und `staging-ci.yml` als „Gate 2" von `static-checks` |

## `build-and-package` — „Build and package"

Datei: `.github/actions/build-and-package/action.yml` (130 Zeilen)

| Aspekt | Wert |
|--------|------|
| Inputs | `release-version` (erforderlich), `release-tag` (erforderlich), `release-notes` (optional) |
| Outputs | `asset-paths`, `manifest-path` |
| Schritte | `setup-dotnet@v6` (10.0.x) → `dotnet restore` → `dotnet publish` win-x64 self-contained Release → ZIP-Packaging via `System.IO.Compression` → Update-Manifest `update.json` via `node scripts/generate-update-manifest.mjs` |
| Verwendung | `staging-ci.yml` (`prerelease`-Job), `release.yml` (`release`-Job) |
| Bemerkung | Beschreibung nennt Windows-only-Produktion (Negotiate/Windows Auth, Serilog EventLog) und offenes Issue `42-iis-kompatibilität` |

## Muster

- Keine Action nutzt `actions/github-script` oder REST-Calls gegen die Issues-API.
- Shell in Composite-Steps ist durchgehend `pwsh` (ein Node-Aufruf eingebettet).
- Verzeichniskonvention: `.github/actions/<name>/action.yml`.
