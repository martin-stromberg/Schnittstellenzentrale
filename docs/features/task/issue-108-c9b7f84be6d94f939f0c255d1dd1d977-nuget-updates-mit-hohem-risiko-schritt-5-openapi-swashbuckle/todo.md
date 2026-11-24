# Aufgabenliste – Anforderungsbearbeitung

Branch: `task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko-schritt-5-openapi-swashbuckle`

**Dokumentierte Abweichung vom Standardablauf:** In dieser Umgebung können keine Unteragenten gestartet werden; alle Schritte, die der Workflow an Unteragenten delegiert (Übersetzung, Bestandsaufnahme, Planung, Planprüfung, Implementierung, Reviews, Tests, Dokumentation), werden daher vom Agenten selbst ausgeführt. `models.md` ist nicht anwendbar (nur ein Modell verfügbar).

| Status | Schritt | Beschreibung | Artefakt |
|--------|---------|--------------|----------|
| [x] | 1 | Branch-Name ermitteln | – |
| [x] | 2 | Verzeichnisstruktur vorbereiten | `docs/features/{branchname}/` |
| [x] | – | Einstiegspunkt ermitteln (keine Artefakte vorhanden → Schritt 3) | – |
| [x] | 3 | Anforderung übersetzen | `requirement.md` |
| [x] | 4 | Bestandsaufnahme | `inventory.md`, `inventory/` |
| [x] | 5 | Umsetzungsplanung | `plan.md` |
| [x] | 5a | Offene Punkte prüfen (keine offenen Punkte, Ausschlüsse bestätigt) | `plan.md` |
| [x] | 5b | Plan gegen Anforderung und Testbedarf prüfen | `plan-check.md` |
| [ ] | 5c | Planungscommit | – |
| [ ] | 6 | Implementierung | Codeänderungen |
| [ ] | 7 | Plan-Review | `review.md` |
| [ ] | 8 | Usability-Review (bei UI-Änderungen) | `review-usability.md` |
| [ ] | 9 | Code-Review | `review-code.md` |
| [ ] | 10 | Tests ausführen inkl. realer E2E-Abnahme | `test-results.md` |
| [ ] | – | Iteration oder Abschluss entscheiden | – |
| [ ] | 11 | Folgeaufgaben dokumentieren (bei Schleifenabbruch) | `continue.md` |
| [ ] | 12 | Dokumentation erstellen | `docs/help/` |
| [ ] | 12b | README aktualisieren | `README.md` |
| [ ] | 12c | Release Notes aktualisieren | `docs/RELEASE_NOTES.md` |
| [ ] | – | Feature-Verzeichnis löschen | – |
| [ ] | – | Commit durchführen | – |
