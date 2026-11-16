# Dokumentationsstruktur und Konventionen

## `docs/help/`

Übersichtsseite `docs/help/index.md` gliedert die Themen in vier Abschnitte: „Kernfunktionen", „Oberfläche und Benutzerführung", „APIs und Integration", „Qualität und Tests". Jeder Eintrag ist eine Zeile `- [Titel](<ordner>/index.md) — <Ein-Satz-Beschreibung>`.

Vorhandene Themenordner:

| Ordner | Dateien |
|--------|---------|
| `aktivitaetsprotokoll/` | index.md, beschreibung.md, ablauf-anwender.md, ablauf-technisch.md, business-rules.md |
| `anwendungen/` | index.md, beschreibung.md, ablauf-anwender.md, ablauf-technisch.md, business-rules.md, datenmodell.md |
| `api/` | index.md, beschreibung.md, ablauf-technisch.md, business-rules.md, api.md, odata-api.md, installation.md |
| `dark-mode/` | index.md, beschreibung.md, ablauf-anwender.md, ablauf-technisch.md, business-rules.md, api.md |
| `endpunkte/` | index.md, beschreibung.md, ablauf-anwender.md, ablauf-technisch.md, business-rules.md, api.md |
| `git-hooks/` | index.md, beschreibung.md (**kleinstes Beispiel**: nur zwei Dateien) |
| `impressum/` | index.md, beschreibung.md, ablauf-anwender.md, ablauf-technisch.md, installation.md |
| `lokalisierung/` | index.md, beschreibung.md, ablauf-technisch.md, business-rules.md, installation.md |
| `playwright-tests/` | index.md, beschreibung.md, ablauf-technisch.md, business-rules.md, installation.md |
| `schnittstellenzentrale/` | index.md, beschreibung.md, ablauf-anwender.md, ablauf-technisch.md, business-rules.md, api.md, datenmodell.md, installation.md |
| `speichermodus/` | index.md, beschreibung.md, ablauf-anwender.md, ablauf-technisch.md, business-rules.md, api.md |
| `systemumgebungen/` | index.md, beschreibung.md, ablauf-anwender.md, ablauf-technisch.md, business-rules.md, datenmodell.md |

Muster pro Thema:

- `index.md`: `# <Titel>` + 1–2 Sätze + „## Inhalt"-Liste mit Links auf die Unterseiten (siehe `git-hooks/index.md`).
- `beschreibung.md`: `# <Titel> - Beschreibung`, dann `## Zweck` und weitere Abschnitte (siehe `git-hooks/beschreibung.md`).
- Sprache der Hilfedateien: **Deutsch**.

## CI-/Workflow-Dokumentation

- **Es gibt kein dediziertes Hilfethema zu CI/Workflows.** Der einzige CI-Bezug in `docs/help/` ist der Abschnitt „CI-Pipeline (GitHub Actions)" in `playwright-tests/installation.md` (Zeilen 85–97, Artefakt-Upload-Beispiel).
- `ci-target-schema.md` wird in Workflow-Kommentaren häufig zitiert, existiert aber **nicht in diesem Repository** (Repo-weite Suche ohne Treffer); es ist offenbar ein externes Referenzdokument des Softwareschmiede-Template-Projekts.
- `README.md` dokumentiert CI-bezogen: Workflow-Badge für `release.yml` (Zeile 3), Abschnitt „Releases" (Zeilen 99–101), Abschnitt „Git-Hooks" (Zeilen 198–211). Kein Abschnitt zu geplanten Automatisierungen.

## Compliance-relevante Bestände (für das jährliche Review)

- `LEGAL.md` (englisch, 37 Zeilen): MIT-Lizenz-Verweis auf eine `LICENSE`-Datei, die **nicht** im Repo-Stamm liegt (README Zeile 215 bestätigt: „Im Repository-Stamm ist keine separate `LICENSE`-Datei hinterlegt"); Haftungsausschluss, Betreiber-Verantwortung für Impressum/Datenschutz.
- `docs/help/impressum/`: Hilfethema zur optionalen Impressumsseite (`impressum.md` im Programmverzeichnis, `/impressum`-Route).
- Keine Datenschutzerklärung, keine About-Dialog-Datei, keine App-Store-Metadaten im Repo.

## `CLAUDE.md` / weitere Konventionen

- `CLAUDE.md` betrifft ausschließlich den C#-Stack (CSS `.sz-*`, API-First, Tests, resx-Lokalisierung) — für Workflow-Änderungen ohne direkte Vorgaben.
- `.githooks/` enthält versionierte Python-Checks + `pre-commit`/`pre-push`; dokumentiert in `docs/help/git-hooks/`.
- `scripts/` enthält Node-Skripte der Release-Automatisierung (`resolve-release-version.mjs`, `generate-update-manifest.mjs` + jeweils `.test.mjs`, `verify-release-version.cjs`); Tests laufen über `npm run test:release-version` (`package.json`).
- `task.json` ist veraltet (verweist auf issue-48/„OData API", nicht auf #79).
- `.softwareschmiede-scm-exchange/` enthält einen GUID-Unterordner `50eb01d0-a0dd-413a-8d4e-2688a6edc802` (Inhalt: nicht weiter untersucht, kein dokumentierter Zweck im Repo).
