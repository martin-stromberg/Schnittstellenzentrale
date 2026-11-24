# Abnahmeprüfung – Entwicklungsschritt 4

## Ergebnis
**Status:** Anforderung vollständig erfüllt

## Abweichungen
- Keine.

## Hinweise

**Diff-Scope (`git diff <basis>...HEAD`):** Nur drei Dateien geändert, alle scope-konform:
- `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`: `Serilog.AspNetCore` 10.0.0 → 11.0.0, Sinks (`Serilog.Sinks.EventLog` 4.0.0, `Serilog.Sinks.File` 7.0.0) unverändert, erklärender Kommentar mit Verweis auf Entscheidung F4.
- `docs/projects/task/…/decisions.md`: F4-Eintrag (Changelog-Bewertung, Sink-Begründung, reale Kestrel-Verifikation, verworfene Alternativen, Konsequenzen).
- `docs/RELEASE_NOTES.md`: EN- und DE-Eintrag zum Logging-Stack.

**Build:** `dotnet build` — 0 Fehler, 35 Warnungen, ausschließlich BL0012/BL0016 (Blazor-Analyzer, Bestand). Keine NU-Warnungen (NU160x o. ä.).

**Tests:** `dotnet test --filter "Category!=OsInterface"` — 641 erfolgreich, 0 Fehler, 0 übersprungen; exakt Baseline 641.

**Vulnerability-Scan:** `dotnet list package --vulnerable --include-transitive` — kein Befund in allen vier Projekten.

**Transitiver Graph (`dotnet list package --include-transitive`, Web-Projekt):** Serilog-Kern 4.4.0; Satelliten auf v11-Linie (`Serilog.Extensions.Hosting` 11.0.0, `Serilog.Extensions.Logging` 11.0.0, `Serilog.Settings.Configuration` 11.0.0); `Serilog.Sinks.Console` 6.1.1, `Serilog.Sinks.Debug` 3.0.0, `Serilog.Formatting.Compact` 3.0.0. Keine 10.x-Reste.

**Sink-Entscheidung plausibilisiert (lokaler NuGet-Cache, nuspec/lib-Struktur):**
- `Serilog.AspNetCore` 11.0.0 deklariert über alle TFMs `Serilog.Sinks.File >= 7.0.0` als Abhängigkeit — die Behauptung „File 7.0.0 ist die vom Paket deklarierte Abhängigkeit" ist zutreffend.
- `Serilog.AspNetCore` 11.0.0 liefert einen echten `net11.0`-TFM (weitere: net462, netstandard2.0/2.1, net10.0) und hängt an Serilog 4.4.0 — bestätigt die net11-Linie.
- `Serilog.Sinks.EventLog` 4.0.0 enthält Lib-Assets für net462, net471, net6.0, net8.0 und netstandard2.0 — unter net11.0 per Nearest-TFM (net8.0) auflösbar. Die im decisions.md genannte TFM-Liste („net6.0/netstandard2.0") ist unvollständig, aber substanziell korrekt; die Kernaussage „net11-tauglich" trifft zu.
- `dotnet list package --outdated` meldet für beide Sinks kein verfügbares stabiles Update — „letzter stabiler Stand" verifiziert (es existieren lediglich neuere Stände für ShadcnBlazor und Swashbuckle, schrittfremd).

**Log-Verifikation (dokumentiert + am Dateisystem nachvollzogen):** `src/Schnittstellenzentrale/logs/log-20261124.txt` existiert (≈106 KB) mit frischen Einträgen bis 24.11.2026 05:25 (Request-/Hub-Events über Kestrel, Port 5199) — der File-Sink schreibt real. Die EventLog-Verifikation ist im decisions.md-F4-Eintrag dokumentiert (Provider `Schnittstellenzentrale`, Anwendungsprotokoll); unabhängige Nachprüfung des EventLog-Inhalts war nicht Teil dieser Abnahme, die Dokumentation ist jedoch konkret und plausibel.

**Repo-Hygiene:** `git status` sauber; `logs/` und `*.db(-shm/-wal)` liegen ausschließlich als ignorierte, nicht getrackte Laufzeit-Artefakte vor (`git ls-files` ohne Treffer).
