# Aufgabenliste – Anforderungsbearbeitung

Branch: `task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko-schritt-8-xunit-v3`

**Abweichungen vom Standardworkflow:**
- Keine Unteragenten startbar (Subagent-Umgebung) — alle Schritte werden vom ausführenden Agenten selbst bearbeitet.
- `models.md` nicht anwendbar (nur ein Modell verfügbar).

| Status | Schritt | Beschreibung | Artefakt |
|--------|---------|--------------|----------|
| [x] | 1 | Branch-Name ermitteln | – |
| [x] | 2 | Verzeichnisstruktur vorbereiten | `docs/features/{branchname}/` |
| [x] | – | Einstiegspunkt ermitteln (keine Artefakte → Schritt 3) | – |
| [x] | 3 | Anforderung übersetzen (selbst, kein Unteragent) | `requirement.md` |
| [x] | 4 | Bestandsaufnahme (selbst) | `inventory.md` |
| [x] | 5 | Umsetzungsplanung (selbst) | `plan.md` |
| [x] | 5a | Offene Punkte prüfen — keine offenen Punkte, keine Scope-Ausschlüsse | `plan.md` |
| [x] | 5b | Plan gegen Anforderung und Testbedarf prüfen (selbst) | `plan-check.md` |
| [x] | 5c | Planungscommit | – |
| [ ] | 6 | Implementierung (selbst) | Codeänderungen |
| [ ] | 7 | Plan-Review (selbst) | `review.md` |
| [ ] | 8 | Usability-Review (keine UI-Änderungen erwartet) | `review-usability.md` |
| [ ] | 9 | Code-Review (selbst) | `review-code.md` |
| [ ] | 10 | Tests ausführen (selbst) | `test-results.md` |
| [ ] | – | Iteration oder Abschluss entscheiden | – |
| [ ] | 11 | Folgeaufgaben dokumentieren (bei Schleifenabbruch) | `continue.md` |
| [ ] | 12 | Dokumentation erstellen (CLAUDE.md + os-interface-tests.md gehören zur Anforderung) | `docs/help/` |
| [ ] | 12b | README aktualisieren | `README.md` |
| [ ] | 12c | Release Notes aktualisieren | `docs/RELEASE_NOTES.md` |
| [ ] | – | Feature-Verzeichnis löschen | – |
| [ ] | – | Commit durchführen | – |
