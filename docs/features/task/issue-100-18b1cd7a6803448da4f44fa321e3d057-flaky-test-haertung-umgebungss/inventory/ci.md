# CI-Bestand

## `.github/workflows/pr-staging-ci.yml` („PR CI for Staging")

- **Trigger:** `pull_request` auf `staging` (opened/synchronize/reopened), Zeilen 11–18.
- **Concurrency:** `group: pr-staging-${{ github.event.pull_request.number }}`, `cancel-in-progress: true` (Z. 20–22).
- **Permissions:** `contents: read`, `checks: write` (Z. 24–26).

### Job-Struktur

| Job | runs-on | timeout-min | needs / if | Rolle |
|---|---|---:|---|---|
| `detect-backmerge` | ubuntu-latest | – | – | Erkennt reine Back-Merges von main (Merge-Parent + Tree-Diff), Output `is_backmerge` (Z. 29–61) |
| `static-checks` | windows-latest | 15 (Z. 73) | `detect-backmerge`; `if: is_backmerge != 'true'` | dotnet format --severity error (Gate 1, Z. 92), security-scan Action (Gate 2, Z. 97), Release-Build mit TreatWarningsAsErrors (Gate 3, Z. 110) |
| `build-and-test` | windows-latest | 30 (Z. 118) | `detect-backmerge`; `if: is_backmerge != 'true'` | Build + **beide** Test-Steps + Coverage-Gate |
| `back-merge-skip` | ubuntu-latest | – | `is_backmerge == 'true'` | Echo-Platzhalter (Z. 210–216) |

`static-checks` und `build-and-test` laufen parallel (kein `needs:` zwischen ihnen).

### Test-Steps im `build-and-test`-Job

| Step | Zeile | Kommando | Blockierend? |
|---|---:|---|---|
| Build | 137 | `dotnet build Schnittstellenzentrale.slnx -c Release --no-restore` (triggert `InstallPlaywright`-Target → Chromium-Install) | ja |
| Test | 144 | `dotnet test ... --filter "FullyQualifiedName!~Playwright" --collect:"XPlat Code Coverage" --logger "trx;LogFileName=test-results.trx"` | **ja (Gate 4)** |
| Test Playwright | 147–157 | `dotnet test ... --filter "FullyQualifiedName~Playwright" --collect:"XPlat Code Coverage" --logger "trx;LogFileName=test-results-playwright.trx"` | `continue-on-error: true` (Z. 155) — **Step**-Ebene, im selben Job |
| Generate coverage report | 164 | `reportgenerator "-reports:src/Schnittstellenzentrale.Tests/TestResults/**/coverage.cobertura.xml" ...` | merge­t Cobertura-XMLs **beider** Steps aus demselben `TestResults`-Verzeichnis |
| Enforce coverage threshold | 167–182 | pwsh: `Line coverage` aus `coverage-report/Summary.txt` parsen, `< $COVERAGE_THRESHOLD` → throw | **ja (Gate 5), 70 %** (`COVERAGE_THRESHOLD: '70'`, Z. 170) |

Wichtig: Da der Playwright-Step `continue-on-error` ist, fließt seine Coverage **nur bei Erfolg** des Steps in den Merge — schlägt der Step fehl (aber Cobertura-Dateien wurden dennoch geschrieben), läuft `reportgenerator` trotzdem weiter und mergt, was vorliegt. Der Schwellwert hängt damit faktisch von der Best-Effort-Lane ab.

### Artefakte (alle `if: always()`, `actions/upload-artifact@v7`, retention 14 d)

| Artefakt | Pfad | Zeilen |
|---|---|---|
| `coverage-report-pr` | `coverage-report/` | 184–190 |
| `test-results-pr` | `src/Schnittstellenzentrale.Tests/TestResults/*.trx` | 192–199 |
| `playwright-traces-pr` | `playwright-traces/` | 201–208 |

## `.github/workflows/staging-ci.yml` („Pre-Release")

- **Trigger:** `push` auf `staging` (Z. 13–16).
- **Concurrency:** `group: staging-ci`, `cancel-in-progress: true` (Z. 18–20).
- **Permissions:** `contents: write`, `pull-requests: write` (Z. 22–24).

| Job | runs-on | timeout-min | Rolle |
|---|---|---:|---|
| `detect-backmerge` | ubuntu-latest | – | identisch zu pr-staging-ci (Z. 27–59) |
| `static-checks` | windows-latest | 15 (Z. 66) | identische Gates 1–3 |
| `build-and-test` | windows-latest | 30 (Z. 99) | identische Test-Steps: `!~Playwright` (Z. 118), Playwright `continue-on-error` (Z. 122–123), ReportGenerator (Z. 131), 70-%-Gate (Z. 134–148) |
| `version` | ubuntu-latest | 10 (Z. 185) | needs: [detect-backmerge, static-checks, build-and-test]; semantic-release Dry-Run + RC-Nummer |
| `prerelease` | windows-latest | 30 (Z. 258) | needs: [detect-backmerge, version]; Release-Notes + `build-and-package` Action + `gh release create --prerelease` |

Artefakte analog mit `-staging`-Suffix (`coverage-report-staging`, `test-results-staging`, `playwright-traces-staging`, Z. 150–178). Kommentar Z. 167–172 dokumentiert, dass Traces **immer** hochgeladen werden (`if: always()`), damit Fehlschläge Diagnosematerial hinterlassen.

**Blockierende Kette:** `version`/`prerelease` hängen via `needs: build-and-test` am Gesamtjob — da `continue-on-error` nur Step-Ebene betrifft, lässt ein Playwright-Fehlschlag den Job grün; das Gate bleibt die reguläre Lane + Coverage.

## `.github/workflows/release.yml`

- „Release gate - tests" (Z. 94–106): `dotnet test Schnittstellenzentrale.slnx -c Release --filter "FullyQualifiedName!~Playwright" -p:SkipPlaywrightInstall=true` — **dritte Stelle** mit FQN-Namensfilter (kein Trait). Läuft nur bei `released == 'true' && release_action == 'create'`.

## `.github/actions/*`

| Action | Relevanz |
|---|---|
| `actions/security-scan/action.yml` | Composite: `dotnet list package --vulnerable --include-transitive`, failt bei Treffern; Artefakt-Upload des Scan-Logs. Gate 2 beider CI-Jobs. Keine Test-Bezüge. |
| `actions/build-and-package/action.yml` | Nur vom `prerelease`-Job/`release.yml` genutzt (Paketierung, update-Manifest). |
| `actions/create-review-issue/action.yml` | Review-Automatisierung, keine Test-Bezüge. |

## Fakten zur Lane-Trennung heute

1. Trennung erfolgt **rein namensbasiert** über `FullyQualifiedName~Playwright` (trifft über den *Namespace* `Schnittstellenzentrale.Tests.Playwright` — alle 24 Playwright-Testklassen inkl. `SitePageTests`; `ImpressumServiceTests` fällt **nicht** darunter und läuft blockierend).
2. Playwright ist `continue-on-error` auf **Step**-Ebene innerhalb `build-and-test` — kein eigener Job, kein eigenes `timeout-minutes`, Artefakt-Uploads sind Job-Ebene.
3. Der Coverage-Merge greift beide `coverage.cobertura.xml` aus einem gemeinsamen `TestResults`-Baum (`src/Schnittstellenzentrale.Tests/TestResults/**`) — ein separater Job müsste die OS-Coverage per Artefakt übergeben oder aus dem Gate ausnehmen.
4. TRX-Dateien (`test-results.trx`, `test-results-playwright.trx`) liegen bereits als Artefakte vor; es gibt **keine** TRX-Auswertung, kein Re-Run-Step, keine Retry-Logik in den Workflows.
