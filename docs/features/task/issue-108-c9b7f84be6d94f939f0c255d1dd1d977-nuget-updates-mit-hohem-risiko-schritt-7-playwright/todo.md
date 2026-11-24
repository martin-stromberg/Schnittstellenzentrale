# Aufgabenliste – Anforderungsbearbeitung

Branch: `task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko-schritt-7-playwright`

**Abweichungen vom Standardworkflow (dokumentiert):**
- Keine Unteragenten in dieser Umgebung startbar — alle Unteragenten-Schritte (Übersetzung, Bestandsaufnahme, Planung, Planprüfung, Implementierung, Reviews, Tests, Dokumentation) werden vom Lifecycle-Agenten selbst ausgeführt.
- `models.md` nicht anwendbar (nur ein Modell verfügbar).

| Status | Schritt | Beschreibung | Artefakt |
|--------|---------|--------------|----------|
| [x] | 1 | Branch-Name ermitteln | – |
| [x] | 2 | Verzeichnisstruktur vorbereiten | `docs/features/task/issue-108-...-schritt-7-playwright/` |
| [x] | – | Einstiegspunkt ermitteln (neuer Lauf, Einstieg bei Schritt 3) | – |
| [x] | 3 | Anforderung übersetzen (selbst, keine Unteragenten) | `requirement.md` |
| [x] | 4 | Bestandsaufnahme (selbst) | `inventory.md`, `inventory/` |
| [x] | 5 | Umsetzungsplanung (selbst) | `plan.md` |
| [x] | 5a | Offene Punkte prüfen (keine offenen Punkte, keine Scope-Ausschlüsse mit Status offen) | `plan.md` |
| [x] | 5b | Plan gegen Anforderung und Testbedarf prüfen (selbst) | `plan-check.md` |
| [x] | 5c | Planungscommit | – |
| [ ] | 6 | Implementierung (selbst) | Codeänderungen |
| [ ] | 7 | Plan-Review (selbst) | `review.md` |
| [ ] | 8 | Usability-Review (bei UI-Änderungen) | `review-usability.md` |
| [ ] | 9 | Code-Review (selbst) | `review-code.md` |
| [ ] | 10 | Tests ausführen inkl. OsInterface-Lane (selbst) | `test-results.md` |
| [ ] | – | Iteration oder Abschluss entscheiden | – |
| [ ] | 11 | Folgeaufgaben dokumentieren (bei Schleifenabbruch) | `continue.md` |
| [ ] | 12 | Dokumentation erstellen (selbst) | `docs/help/` |
| [ ] | 12b | README aktualisieren (selbst) | `README.md` |
| [ ] | 12c | Release Notes aktualisieren (selbst) | `docs/RELEASE_NOTES.md` |
| [ ] | – | Feature-Verzeichnis löschen | – |
| [ ] | – | Commit durchführen | – |
