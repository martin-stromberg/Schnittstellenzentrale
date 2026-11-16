# Bestandsaufnahme: Automatisierung wiederkehrender Architektur-, Sicherheits- und Compliance-Reviews

Analysiert wurde die CI-/Automatisierungs-Infrastruktur des Repositories (`.github/workflows/`, `.github/actions/`) sowie die Dokumentationsstruktur (`docs/help/`, `README.md`, `LEGAL.md`, `CLAUDE.md`), bezogen auf die Anforderung aus `requirement.md` (drei zeitgesteuerte Workflows, die Issues mit `@softwarewerkstatt`-Erwähnung und Duplikat-Vermeidung erzeugen). C#-Artefakte sind nicht betroffen.

## Zusammenfassung

- **7 Workflows** existieren unter `.github/workflows/`; davon hat nur `security-scan.yml` einen `schedule`-Trigger (`0 4 * * 1`, Montag 04:00 UTC) — zugleich der einzige Workflow mit `workflow_dispatch`.
- **Kein Workflow erzeugt Issues.** `gh issue create` und `actions/github-script` kommen nirgends vor; automatisiert werden bisher nur Pull Requests (`gh pr create`) und Releases erzeugt.
- Ein etabliertes **gh-CLI-Muster** existiert bereits: `env: GH_TOKEN: ${{ github.token }}` pro Step, `gh label create <name> --force` zur Label-Sicherstellung, `gh pr list --state open` zur Duplikat-Prüfung vor dem Anlegen — aus `sync-staging-with-main.yml` und `staging-to-main-promotion.yml`.
- **Zwei automatisierte Labels** existieren per Konvention: `automated-backmerge` (1D76DB), `automated-promotion` (0E8A16). `issues: write` steht bereits in drei Workflows, obwohl dort keine Issues erzeugt werden.
- `security-scan.yml` führt **bereits wöchentlich** einen NuGet-Vulnerability-Scan durch (direkte + transitive Pakete, `dotnet list package --vulnerable`), der bei Befund fehlschlägt und ein Log-Artefakt hochlädt — **thematische Überschneidung** mit der geforderten wöchentlichen NuGet-Prüfung; erzeugt aber kein Issue und setzt keine Update-Empfehlungen um.
- **Keine `@softwarewerkstatt`-Bezüge** außerhalb von `issue.md`/`requirement.md`; keine dokumentierte Automatisierung, die auf Erwähnungen reagiert.
- `ci-target-schema.md` wird in Workflow-Kommentaren zitiert, **liegt aber nicht im Repo** (externes Referenzdokument des Vorlagen-Projekts).
- `docs/help/` hat eine feste Struktur (Themenordner mit `index.md`, `beschreibung.md` etc.; kleinstes Beispiel `git-hooks/` mit zwei Dateien); **kein CI-/Workflow-Hilfethema** vorhanden — einzige CI-Erwähnung ist der Abschnitt „CI-Pipeline" in `playwright-tests/installation.md`.
- `LEGAL.md` referenziert eine `LICENSE`-Datei, die nicht im Repo-Stamm existiert (laut README bewusst); kein `ISSUE_TEMPLATE`, kein `dependabot.yml`, kein `CODEOWNERS` unter `.github/`.
- Workflow-Konventionen: englischer Zweck-Kommentarblock unter `name:`, expliziter `permissions:`-Block (bis auf `security-scan.yml`), `concurrency`-Gruppe, `timeout-minutes`, Runner `ubuntu-latest` für reine `gh`-Aufrufe vs. `windows-latest` für .NET-Builds, `shell: pwsh` für pwsh-Skripte (teilweise bash-Default bei trivialen Schritten).

## Details

- [Bestehende Workflows](inventory/workflows.md) — alle sieben Workflows mit Triggern, Permissions, Runnern, Concurrency und Kommentar-Stil
- [Composite Actions](inventory/composite-actions.md) — `security-scan` und `build-and-package` mit Inputs/Outputs und Verwendungsorten
- [Issue-/Label-Automatisierung](inventory/issue-automation.md) — etabliertes gh-CLI-Muster, Label-Konventionen, `GH_TOKEN`-Verwendung, `@softwarewerkstatt`-Befund
- [Dokumentationsstruktur und Konventionen](inventory/docs-structure.md) — `docs/help/`-Aufbau, fehlende CI-Doku, compliance-relevante Bestände, `CLAUDE.md`/`README.md`/`LEGAL.md`
