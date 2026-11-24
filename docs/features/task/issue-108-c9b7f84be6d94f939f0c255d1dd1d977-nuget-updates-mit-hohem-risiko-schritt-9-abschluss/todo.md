# Aufgabenliste – Anforderungsbearbeitung

Branch: `task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko-schritt-9-abschluss`

**Abweichungen (dokumentiert):**
- Keine Unteragenten in dieser Umgebung startbar — alle als „(Unteragent)" markierten Schritte werden vom Lifecycle-Agenten in Eigenregie mit frischem Blick je Schritt ausgeführt.
- `models.md` nicht anwendbar (nur ein Modell verfügbar).
- Anweisung des Orchestrators: KEIN Merge, KEIN Push, KEIN PR, KEIN Closing-Keyword — der Abschluss-PR mit `Closes #108` wird vom Orchestrator nach dem Merge erstellt. Der Abschlusscommit auf dem Schritt-Branch erfolgt ohne Closing-Keyword.
- Schritt 12/12b/12c (Doku/README/Release Notes): geprüft und als nicht anwendbar bewertet — reine Prozess-/Entscheidungsdokumentation ohne nutzerseitige Verhaltensänderung; `decisions.md` ist das vorgesehene Dokumentationsziel der Anforderung. RELEASE_NOTES.md wird bewusst nicht ergänzt (Prozessentscheidung, kein produktiver Release-Punkt).

| Status | Schritt | Beschreibung | Artefakt |
|--------|---------|--------------|----------|
| [x] | 1 | Branch-Name ermitteln | – |
| [x] | 2 | Verzeichnisstruktur vorbereiten | `docs/features/{branchname}/` |
| [x] | – | Einstiegspunkt ermitteln (keine Vorgänger-Artefakte → Schritt 3) | – |
| [ ] | 3 | Anforderung übersetzen (Eigenregie, keine Unteragenten) | `requirement.md` |
| [ ] | 4 | Bestandsaufnahme (Eigenregie) | `inventory.md` |
| [ ] | 5 | Umsetzungsplanung (Eigenregie) | `plan.md` |
| [ ] | 5a | Offene Punkte prüfen und ggf. Planung wiederholen | `plan.md` (aktualisiert) |
| [ ] | 5b | Plan gegen Anforderung und Testbedarf prüfen (Eigenregie) | `plan-check.md` |
| [ ] | 5c | Planungscommit | – |
| [ ] | 6 | Implementierung: F7+F8 in `decisions.md`, F8-Issue via `gh`, F7-Kommentar auf #108 (Eigenregie) | `decisions.md`, Issue, Kommentar |
| [ ] | 7 | Plan-Review (Eigenregie) | `review.md` |
| [ ] | 8 | Usability-Review (keine UI-Änderungen → nicht anwendbar) | `review-usability.md` |
| [ ] | 9 | Code-Review (Eigenregie) | `review-code.md` |
| [ ] | 10 | Tests ausführen: `dotnet test --filter "Category!=OsInterface"` (641 erwartet) | `test-results.md` |
| [ ] | – | Iteration oder Abschluss entscheiden | – |
| [ ] | 11 | Folgeaufgaben dokumentieren (bei Schleifenabbruch) | `continue.md` |
| [ ] | 12 | Dokumentation — geprüft: nicht anwendbar (Prozessentscheidung, Ziel ist `decisions.md`) | – |
| [ ] | 12b | README aktualisieren — geprüft: nicht anwendbar | – |
| [ ] | 12c | Release Notes — geprüft: bewusst kein Eintrag (kein produktiver Release-Punkt) | – |
| [ ] | – | Feature-Verzeichnis löschen | – |
| [ ] | – | Commit durchführen (docs:, ohne Closing-Keyword) | – |
