# Aufgabenliste – Anforderungsbearbeitung

Branch: `task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko-schritt-3-odata-edm`

Basisbranch: `task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko`

**Abweichungen dokumentiert:**

- `models.md` nicht anwendbar — nur ein Modell verfügbar (Vorgabe des Auftrags).
- Keine Unteragenten startbar — alle Schritte (Übersetzung, Bestandsaufnahme, Planung, Planprüfung, Implementierung, Reviews, Tests, Dokumentation) werden vom Orchestrator selbst ausgeführt (Vorgabe des Auftrags).

| Status | Schritt | Beschreibung | Artefakt |
|--------|---------|--------------|----------|
| [x] | 1 | Branch-Name ermitteln | – |
| [x] | 2 | Verzeichnisstruktur vorbereiten | `docs/features/{branchname}/` |
| [x] | – | Einstiegspunkt ermitteln | – |
| [ ] | 3 | Anforderung übersetzen | `requirement.md` |
| [ ] | 4 | Bestandsaufnahme | `inventory.md`, `inventory/` |
| [ ] | 5 | Umsetzungsplanung | `plan.md` |
| [ ] | 5a | Offene Punkte prüfen | `plan.md` (aktualisiert) |
| [ ] | 5b | Plan gegen Anforderung und Testbedarf prüfen | `plan-check.md` |
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
