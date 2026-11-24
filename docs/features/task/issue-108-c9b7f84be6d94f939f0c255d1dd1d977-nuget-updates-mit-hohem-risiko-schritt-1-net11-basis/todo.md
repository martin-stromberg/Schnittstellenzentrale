# Aufgabenliste – Anforderungsbearbeitung

Branch: `task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko-schritt-1-net11-basis`
Basisbranch: `task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko`

**Abweichungen vom Standard-Workflow:**
- Keine Unteragenten startbar (Subagent-Umgebung) — alle Schritt-Inhalte werden vom ausführenden Agenten selbst erstellt.
- `models.md`-Modellauswahl nicht anwendbar — es steht nur ein Agentenmodell zur Verfügung.
- Keine Rückfragen möglich — offene Punkte werden als dokumentierte Annahmen in `plan.md` geführt (Auftrag: „Triff bei Unklarheiten die technisch begründbarste Entscheidung und dokumentiere sie als Annahme").

| Status | Schritt | Beschreibung | Artefakt |
|--------|---------|--------------|----------|
| [x] | 1 | Branch-Name ermitteln | – |
| [x] | 2 | Verzeichnisstruktur vorbereiten | `docs/features/{branchname}/` |
| [x] | – | Einstiegspunkt ermitteln | – |
| [x] | 3 | Anforderung übersetzen (selbst, keine Unteragenten) | `requirement.md` |
| [x] | 4 | Bestandsaufnahme (selbst) | `inventory.md`, `inventory/` |
| [x] | 5 | Umsetzungsplanung (selbst) | `plan.md` |
| [x] | 5a | Offene Punkte prüfen und ggf. Planung wiederholen | `plan.md` (aktualisiert) |
| [x] | 5b | Plan gegen Anforderung und Testbedarf prüfen (selbst) | `plan-check.md` |
| [ ] | 5c | Planungscommit | – |
| [ ] | 6 | Implementierung (selbst) | Codeänderungen |
| [ ] | 7 | Plan-Review (selbst) | `review.md` |
| [ ] | 8 | Usability-Review (bei UI-Änderungen) | `review-usability.md` |
| [ ] | 9 | Code-Review (selbst) | `review-code.md` |
| [ ] | 10 | Tests ausführen inkl. realer E2E-Abnahme (selbst) | `test-results.md` |
| [ ] | – | Iteration oder Abschluss entscheiden | – |
| [ ] | 11 | Folgeaufgaben dokumentieren (bei Schleifenabbruch) | `continue.md` |
| [ ] | 12 | Dokumentation erstellen (selbst) | `docs/help/` |
| [ ] | 12b | README aktualisieren (selbst) | `README.md` |
| [ ] | 12c | Release Notes aktualisieren (selbst) | `docs/RELEASE_NOTES.md` |
| [ ] | – | Feature-Verzeichnis löschen | – |
| [ ] | – | Commit durchführen | – |
