# Plan-Prüfung – F4: Serilog.AspNetCore 10.0.0 → 11.0.0

**Status: Plan vollständig**

Geprüft: `plan.md` gegen `requirement.md` und `inventory.md` (inkl. `inventory/serilog-11-analyse.md`).

## Abdeckung der Anforderung

| Anforderung | Plan-Abdeckung | Bewertung |
|---|---|---|
| `Serilog.AspNetCore` 10.0.0 → 11.0.0 im Web-csproj | Umsetzungsschritt 1 (Zeile 37 exakt benannt) | ✅ vollständig |
| Changelog 11.0.0 auf Breaking Changes prüfen | inventory/serilog-11-analyse.md: einzige Verhaltensänderung (Request-Logging unterdrückt Exceptions bei abgebrochenen Requests) analysiert und als nicht anwendbar bewertet, weil `UseSerilogRequestLogging` nicht genutzt wird — Verzicht per Volltextsuche verifiziert | ✅ vollständig |
| Sink-Kompatibilität `File` 7.0.0 / `EventLog` 4.0.0 verifizieren, ggf. mitheben | Beide begründet auf Stand belassen: File 7.0.0 ist die von AspNetCore 11.0.0 selbst deklarierte Abhängigkeit (stärkste denkbare Kompatibilitätsaussage); EventLog 4.0.0 letzter stabiler Stand, net6.0-Asset unter net11 auflösbar | ✅ vollständig |
| Betroffene Codestellen prüfen (Program.cs:43-47, :59; appsettings.json:13-32) | Inventory verifiziert exakt diese Zeilen; Plan schließt Code-Umbau begründet aus (API-unverändert) | ✅ vollständig |
| Gezielte Log-Verifikation (still möglicher Ausfall) | AS-3 (reale Kestrel-Abnahme, `logs/log-<datum>.txt`) + AS-4 (EventLog-Konfigladen) als verpflichtende Szenarien | ✅ vollständig |
| `dotnet test --filter "Category!=OsInterface"` grün, Baseline 641 | AS-2 mit exakter Baseline-Zahl | ✅ vollständig |
| `dotnet list package --vulnerable` ohne Befund | AS-5 | ✅ vollständig |

## Testbedarf

- Volle Bestandslane (641) + reale E2E-Abnahme sind verankert — angemessen, da der
  kritische Pfad (Bootstrap-Logger aus `appsettings.json`, Kestrel-Hosting) erst
  in realer Ausführung nachweisbar ist.
- Verzicht auf neue automatisierte Tests ist begründet: reine Versionshebung ohne
  Codeänderung; ein Versions-Assertions-Test hätte keinen fachlichen Mehrwert.

## Konsistenz/Plausibilität

- Akzeptanzszenarien sind vom Auslöser bis zur sichtbaren Wirkung durchgehend
  (AS-3 beschreibt kompletten Happy Path inkl. Ausgangszustand und Aufräumen).
- Risikotabelle adressiert die realistischen Fehlerbilder (still, NU1608,
  Artefakt-Verschmutzung, nicht registrierte EventLog-Source).
- `logs/`+`*.db*`-gitignore-Abdeckung ist im Inventory verifiziert (Zeilen 48/438/445/446
  der `.gitignore`), nicht nur angenommen.

## Festgestellte Lücken

Keine.
