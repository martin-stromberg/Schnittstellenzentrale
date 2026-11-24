# Umsetzungsplan – F4: Serilog.AspNetCore 10.0.0 → 11.0.0

## Scope-Klärung

**Verarbeitungseinheit:** Eine einzige Paketreferenz-Hebung im Web-Projekt
(`Serilog.AspNetCore` 10.0.0 → 11.0.0 in `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`, Zeile 37).

**Auslösendes Ereignis:** Paketupdate im Rahmen von Issue #108, Schritt 4.

**Erwartete Wirkung:** Der Serilog-Stack läuft auf der v11-Linie (Kern ≥ 4.4.0,
Satelliten ≥ 11.0.0); die Anwendung startet weiterhin und schreibt nachweisbar
Logdateien nach `logs/log-<datum>.txt`; die EventLog-Sink-Konfiguration lädt
fehlerfrei.

**Umgang mit noch nicht vorhandenen Objekten:** Keine neuen Objekte. Die Sinks
(`File` 7.0.0, `EventLog` 4.0.0) bleiben unverändert — ihre Kompatibilität ist
per Paketdeklaration verifiziert (AspNetCore 11.0.0 deklariert `Sinks.File >= 7.0.0`
selbst; EventLog 4.0.0 ist net6.0-kompatibel und letzter stabiler Stand).

**Scope-Ausschlüsse (bestätigt als dokumentierte Annahmen — keine Rückfragen laut Auftrag):**

| # | Ausschluss | Status | Begründung |
|---|-----------|--------|-----------|
| 1 | Sinks werden **nicht** mitgehoben | bestätigt (Annahme, 2026-11-24) | `Sinks.File` 7.0.0 ist die von AspNetCore 11.0.0 selbst geforderte Version; `Sinks.EventLog` 4.0.0 ist letzter stabiler Stand und net11-kompatibel. Kein technischer Anlass. |
| 2 | Keine Änderung an `appsettings.json`-Serilog-Konfiguration | bestätigt (Annahme, 2026-11-24) | Konfig-Schema und Assembly-Auflösung unverändert (Settings.Configuration 11 bringt keinen Schema-Bruch). |
| 3 | Kein Code-Umbau in `Program.cs` | bestätigt (Annahme, 2026-11-24) | `ReadFrom.Configuration`, `UseSerilog()`, statisches `Log.Logger` sind in v11 API-unverändert. |
| 4 | OsInterface-Lane wird nicht ausgeführt | bestätigt (Auftrag) | Explizit ausgenommen; Logging betrifft keine OS-Schnittstelle im Test-Sinn. |
| 5 | Keine Aktivierung von `UseSerilogRequestLogging` | bestätigt (Annahme, 2026-11-24) | Nicht Bestand des Bestandscodes; wäre Feature-Erweiterung. |

## Akzeptanzszenarien

**AS-1 (Happy Path, Paketupdate):**
Ausgang: csproj pinnt `Serilog.AspNetCore` 10.0.0.
Aktion: Version auf `11.0.0` heben, `dotnet restore` + `dotnet build`.
Wirkung: Build ohne Fehler; `dotnet list package --include-transitive` zeigt
`Serilog.AspNetCore` 11.0.0, `Serilog` ≥ 4.4.0, `Extensions.Hosting`/`Settings.Configuration` ≥ 11.0.0,
keine Serilog-10.x-Reste, keine NU160x-Warnungen.

**AS-2 (Regressionsfreiheit):**
`dotnet test --filter "Category!=OsInterface"` → 641/641 grün (Baseline aus Schritt 3),
0 Fehler, 0 neu übersprungen.

**AS-3 (Reale Log-Verifikation — E2E):**
Ausgang: `src/Schnittstellenzentrale/logs/` ohne Datei des aktuellen Datums
(bzw. Inhalt wird vorher erfasst).
Aktion: App real unter Kestrel starten (`dotnet run` auf das Web-Projekt oder
kompilierte DLL), mindestens einen HTTP-Request ausführen (z. B. `GET /` oder
Health-/Swagger-Endpunkt), App kontrolliert beenden.
Wirkung: `logs/log-<datum>.txt` existiert und enthält Logeinträge aus diesem
Lauf (Zeitstempel ≥ Startzeitpunkt; typische Startup-/Request-Events).

**AS-4 (EventLog-Konfigurationsladen, Windows):**
Beim App-Start tritt keine Exception aus der Serilog-Konfiguration auf (App läuft
hoch, kein `CreateLogger`-/Sink-Auflösungsfehler). Zusätzlich, sofern die Source
`Schnittstellenzentrale` auf dem Host registriert ist: `Get-WinEvent`-/`Get-EventLog`-Abfrage
zeigt Einträge. Ist die Source nicht registriert, gilt der erfolgreiche, fehlerfreie
Konfig-Ladevorgang plus unverändertem Bestandsverhalten (manageEventSource=false
unterdrückt Source-Anlage) als erfüllt — dokumentiert in `test-results.md`.

**AS-5 (Sicherheitscheck):**
`dotnet list package --vulnerable` (Lösung, inkl. transitiv) → kein Befund.

## Umsetzungsschritte

1. `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`:
   `<PackageReference Include="Serilog.AspNetCore" Version="10.0.0" />` → `11.0.0`.
   (Sinks unverändert lassen — siehe Scope-Ausschluss 1.)
2. `dotnet restore` + `dotnet build` — Auflösung und Warnungen prüfen (AS-1).
3. `dotnet test --filter "Category!=OsInterface"` — volle Lane (AS-2).
4. Reale E2E-Abnahme (AS-3, AS-4):
   - Vorab `logs/` bestandsaufnahmen (vorhandene Dateien notieren).
   - App unter Kestrel auf freiem Port starten (Hintergrundprozess), mit `curl`
     einen Request absetzen, Prozess beenden.
   - `logs/log-<datum>.txt` auf Existenz + frische Einträge prüfen.
   - EventLog: Source-Registrierung und Einträge via PowerShell prüfen
     (`Get-EventLog -LogName Application -Source Schnittstellenzentrale -Newest 5`
     bzw. `Get-WinEvent`); Ergebnis dokumentieren.
   - Test-Artefakte aufräumen: neu erzeugte `logs/`-Dateien sind gitignored —
     dennoch Laufzeit-Artefakte (DB `schnittstellenzentrale.db` im Projektverzeichnis
     o. ä.) prüfen und ggf. entfernen bzw. auf ignored-Status verifizieren.
5. `dotnet list package --vulnerable` (AS-5).
6. Dokumentation: `docs/projects/task/issue-108-...-nuget-updates-mit-hohem-risiko/decisions.md`
   um F4-Abschnitt ergänzen (Update + Sink-Verbleib begründet), `docs/RELEASE_NOTES.md` EN+DE pflegen.

## Teststrategie

- Keine neuen automatisierten Tests erforderlich — die Änderung ist eine reine
  Paketversion; der Funktionsnachweis läuft über die volle Bestandslane (641 Tests)
  plus reale E2E-Abnahme (AS-3/AS-4). Neue Tests für Paketversionen wären ohne
  fachliche Aussage.
- Die reale E2E-Abnahme ist verpflichtender Teil dieses Schritts (still möglicher
  Logging-Ausfall kann von Unit-/Integrationstests nicht ausgeschlossen werden,
  da der Bootstrap-Logger-Pfad unter `WebApplicationFactory` ggf. anders
  konfiguriert läuft — die reale Kestrel-Ausführung mit `appsettings.json`
  ist der belastbare Nachweis).

## Offene Punkte

Keine.

## Risiken und Mitigation

| Risiko | Mitigation |
|---|---|
| Stiller Logging-Ausfall trotz grünem Build/Tests | AS-3/AS-4: reale Kestrel-Abnahme mit Datei-/EventLog-Verifikation |
| NU1608- oder Rest-Versionssplits im Serilog-Graph | `dotnet list package --include-transitive` nach Restore auswerten |
| Neu erzeugte Laufzeit-Artefakte (logs/, DB) landen im Commit | `git status`-Prüfung vor Commit; `.gitignore`-Abdeckung verifiziert (`[Ll]ogs/`, `*.db` ggf. prüfen) |
| EventLog-Source nicht registriert → „still" | Als Bestandsverhalten dokumentieren; Konfigladen via fehlerfreiem Start nachweisen |
