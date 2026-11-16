# Tasks: Automatisierung wiederkehrender Architektur-, Sicherheits- und Compliance-Reviews

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---------|---------|--------|--------------|
| 1 | Composite Action | `.github/actions/create-review-issue/action.yml` anlegen: `description:`-Block, Inputs `label`, `label-color`, `label-description`, `issue-title-prefix`, `issue-body`, `cadence` (alle `required`) | Offen | — |
| 2 | Composite Action | Schritt „Ensure label exists" in der Action implementieren (`gh label create <label> --color <hex> --description <text> --force`, `env: GH_TOKEN`, `shell: pwsh`) | Offen | — |
| 3 | Composite Action | Schritt „Create issue if none open" implementieren: `gh issue list --label <label> --state open --json number`, Titelbildung `issue-title-prefix` + Suffix per `switch` auf `cadence` (`yyyy-MM` / ISO `yyyy-KWww` / `yyyy`), `gh issue create` mit `issue-body` via `env: ISSUE_BODY`, Skip-Pfad mit `Write-Host` | Offen | — |
| 4 | Workflows | `.github/workflows/monthly-architecture-security-review.yml` anlegen (Kommentarblock, `permissions`, `concurrency`, Cron `0 5 1 * *` + `workflow_dispatch`, `ubuntu-latest`, `timeout-minutes: 5`, `actions/checkout@v7`, Action-Aufruf mit `cadence: monthly`, Label `automated-architecture-review`, Titel-Präfix `[Review] Architektur- und Sicherheitsreview`, Body wörtlich aus `issue.md` Abschnitt 1) | Offen | — |
| 5 | Workflows | `.github/workflows/weekly-nuget-review.yml` anlegen (gleiche Struktur, Cron `0 6 * * 1`, `cadence: weekly`, Label `automated-nuget-review`, Titel-Präfix `[Review] NuGet-Paketprüfung`, Body wörtlich aus `issue.md` Abschnitt 2) | Offen | — |
| 6 | Workflows | `.github/workflows/yearly-compliance-review.yml` anlegen (gleiche Struktur, Cron `0 5 1 1 *`, `cadence: yearly`, Label `automated-compliance-review`, Titel-Präfix `[Review] Compliance- und Copyright-Review`, Body wörtlich aus `issue.md` Abschnitt 3) | Offen | — |
| 7 | Verifikation | YAML-Parse-Check aller vier neuen Dateien (z. B. Python `yaml.safe_load`) | Offen | — |
| 8 | Verifikation | Struktur-Check der Workflows: `permissions` (`contents: read`, `issues: write`), `concurrency`, `timeout-minutes`, `workflow_dispatch`, korrekte Cron-Ausdrücke, alle Pflicht-Inputs der Action gesetzt | Offen | — |
| 9 | Verifikation | Lokaler Trockenlauf der Duplikat-Logik via `gh`: `gh label create automated-nuget-review --force` (idempotent) und `gh issue list --label automated-nuget-review --state open --json number` (leeres Ergebnis) — ohne Issue-Anlage | Offen | — |
| 10 | E2E-Test | Temporären `push`-Trigger auf den Feature-Branch in den drei Workflows ergänzen, pushen und je Workflow die Issue-Erzeugung verifizieren (Titel mit Präfix + Periode, Label, `@softwarewerkstatt` als erste Zeile, vollständige Prüfliste) | Offen | — |
| 11 | E2E-Test | Zweiten Lauf pro Workflow auslösen und Duplikat-Vermeidung verifizieren (kein zweites Issue, Skip-Meldung im Job-Log) | Offen | — |
| 12 | E2E-Test | Test-Issues schließen, dritten Lauf auslösen und Neuanlage verifizieren (Label-Logik greift nur auf offene Issues); danach Test-Issues schließen und temporären `push`-Trigger entfernen | Offen | — |
| 13 | Dokumentation | `docs/help/review-automatisierung/index.md` anlegen (Muster `git-hooks/index.md`) | Offen | — |
| 14 | Dokumentation | `docs/help/review-automatisierung/beschreibung.md` anlegen (Zweck, drei Workflows mit Intervallen, `@softwarewerkstatt`, Labels, manuelle Auslösung, Cron-Hinweis) | Offen | — |
| 15 | Dokumentation | `docs/help/review-automatisierung/ablauf-technisch.md` anlegen (Composite Action, `GH_TOKEN`, `gh issue list`/`create`, Permissions, Abgrenzung zu `security-scan.yml`, Erstlauf-Hinweis) | Offen | — |
| 16 | Dokumentation | Eintrag in `docs/help/index.md` im Abschnitt „Qualität und Tests" ergänzen | Offen | — |
| 17 | Dokumentation | Kurzabschnitt „Automatisierte Review-Workflows" in `README.md` ergänzen mit Link auf das Hilfethema | Offen | — |
