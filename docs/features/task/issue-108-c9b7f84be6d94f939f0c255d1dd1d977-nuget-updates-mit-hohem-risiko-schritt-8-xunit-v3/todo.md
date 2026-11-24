# Aufgabenliste – Anforderungsbearbeitung

Branch: `task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko-schritt-8-xunit-v3`

**Abweichungen vom Standardworkflow:**
- Keine Unteragenten startbar (Subagent-Umgebung) — alle Schritte wurden vom ausführenden Agenten selbst bearbeitet.
- `models.md` nicht anwendbar (nur ein Modell verfügbar).
- E2E-Abnahme nicht durchführbar/entbehrlich (reine Test-Infrastruktur-Änderung, keine UI) — Ersatznachweis über vollständige Lane-Läufe in `test-results.md` dokumentiert.

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
| [x] | 5c | Planungscommit | `ff1e7d3` |
| [x] | 6 | Implementierung (selbst) | Codeänderungen |
| [x] | 7 | Plan-Review (selbst) | `review.md` |
| [x] | 8 | Usability-Review — nicht anwendbar (keine UI-Änderungen) | `review-usability.md` |
| [x] | 9 | Code-Review (selbst) | `review-code.md` |
| [x] | 10 | Tests ausführen (selbst): 641/641 regulär + 94/94 OS-Lane grün, Coverage 72,5 %, kein Vuln-Befund | `test-results.md` |
| [x] | – | Iteration oder Abschluss entscheiden — Abschluss (alle Kriterien erfüllt) | – |
| [x] | 11 | Folgeaufgaben dokumentieren — keine Folgeaufgaben (kein Schleifenabbruch) | – |
| [x] | 12 | Dokumentation erstellen (CLAUDE.md + os-interface-tests.md + playwright-docs) | `docs/help/` |
| [x] | 12b | README aktualisiert (MTP-`--`-Form, Coverage-Extension) | `README.md` |
| [x] | 12c | Release Notes aktualisiert (EN + DE) | `docs/RELEASE_NOTES.md` |
| [ ] | – | Feature-Verzeichnis löschen (im feat-Commit) | – |
| [ ] | – | Commit durchführen | – |
