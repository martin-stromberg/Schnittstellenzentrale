# Aufgabenliste – Anforderungsbearbeitung

Branch: `task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko-schritt-2-frameworkref`
Basisbranch: `task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko`

> **Abweichung (dokumentiert):** In dieser Umgebung können keine Unteragenten gestartet
> werden. Sämtliche als „(Unteragent)" markierten Schritte werden daher vom
> Lifecycle-Agenten selbst ausgeführt. Die Modellauswahl gemäß `models.md` ist
> ebenfalls nicht anwendbar (nur ein Agentenmodell verfügbar).

| Status | Schritt | Beschreibung | Artefakt |
|--------|---------|--------------|----------|
| [x] | 1 | Branch-Name ermitteln | – |
| [x] | 2 | Verzeichnisstruktur vorbereiten | `docs/features/{branchname}/` |
| [x] | – | Einstiegspunkt ermitteln | – |
| [ ] | 3 | Anforderung übersetzen (Unteragent → selbst ausgeführt) | `requirement.md` |
| [ ] | 4 | Bestandsaufnahme (Unteragent → selbst ausgeführt) | `inventory.md`, `inventory/` |
| [ ] | 5 | Umsetzungsplanung (Unteragent → selbst ausgeführt) | `plan.md` |
| [ ] | 5a | Offene Punkte prüfen und ggf. Planung wiederholen | `plan.md` (aktualisiert) |
| [ ] | 5b | Plan gegen Anforderung und Testbedarf prüfen (Unteragent → selbst ausgeführt) | `plan-check.md` |
| [ ] | 5c | Planungscommit | – |
| [ ] | 6 | Implementierung (Unteragent → selbst ausgeführt) | Codeänderungen |
| [ ] | 7 | Plan-Review (Unteragent → selbst ausgeführt) | `review.md` |
| [ ] | 8 | Usability-Review (keine UI-Änderungen erwartet) | `review-usability.md` |
| [ ] | 9 | Code-Review (Unteragent → selbst ausgeführt) | `review-code.md` |
| [ ] | 10 | Tests ausführen inkl. realer E2E-Abnahme (Unteragent → selbst ausgeführt) | `test-results.md` |
| [ ] | – | Iteration oder Abschluss entscheiden | – |
| [ ] | 11 | Folgeaufgaben dokumentieren (bei Schleifenabbruch) | `continue.md` |
| [ ] | 12 | Dokumentation erstellen (Unteragent → selbst ausgeführt) | `docs/help/` |
| [ ] | 12b | README aktualisieren (Unteragent → selbst ausgeführt) | `README.md` |
| [ ] | 12c | Release Notes aktualisieren (Unteragent → selbst ausgeführt) | `docs/RELEASE_NOTES.md` |
| [ ] | – | Feature-Verzeichnis löschen | – |
| [ ] | – | Commit durchführen | – |
