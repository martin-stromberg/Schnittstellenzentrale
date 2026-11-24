# Übersetzte Anforderung – F4: Serilog.AspNetCore → 11.0.0

## Ausgangslage

Entwicklungsschritt 4 des Projekts Issue #108 „NuGet-Updates mit hohem Risiko".
Schritte 1–3 sind bereits gemergt: net11.0, SDK 11.0.100, `FrameworkReference` in
Infrastructure, OData-Stack auf stabilen 9.0.0-Versionen (`AspNetCore.OData` bewusst
auf `10.0.0-preview.2` verharrend).

Aktueller Stand im Web-Projekt (`src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`):

- `Serilog.AspNetCore` **10.0.0**
- `Serilog.Sinks.EventLog` **4.0.0**
- `Serilog.Sinks.File` **7.0.0**

## Fachliches Ziel

`Serilog.AspNetCore` im Web-csproj von **10.0.0 auf 11.0.0** heben.

## Umfang

1. **Changelog-Prüfung 11.0.0:** Release Notes (GitHub/NuGet) auf Breaking Changes prüfen
   und das Ergebnis dokumentieren.
2. **Sink-Kompatibilität verifizieren:** `Serilog.Sinks.File` 7.0.0 und
   `Serilog.Sinks.EventLog` 4.0.0 gegen Serilog-11-Stack prüfen
   (Support-Matrizen/Release-Notes); Sinks ggf. mitheben, wenn erforderlich.
3. **Betroffene Codestellen (Prüfumfang):**
   - `src/Schnittstellenzentrale/Program.cs:43-47` – Bootstrap-Logger via
     `LoggerConfiguration().ReadFrom.Configuration(...)`
   - `src/Schnittstellenzentrale/Program.cs:59` – `builder.Host.UseSerilog()`
   - `appsettings.json:13-32` – Serilog-Abschnitt: EventLog-Source
     `Schnittstellenzentrale` (Log `Application`), File-Sink `logs/log-.txt`
     mit `rollingInterval: Day`, `retainedFileCountLimit: 7`
4. **Gezielte Log-Verifikation:** Da ein Logging-Ausfall still wäre, muss nach dem
   Update die tatsächliche Log-Ausgabe verifiziert werden — nicht nur Build/Tests.

## Akzeptanzkriterien

1. `dotnet test --filter "Category!=OsInterface"` vollständig grün — Baseline **641 Tests**.
2. Dokumentierte Log-Verifikation:
   - File-Sink schreibt tatsächlich `logs/log-*.txt` (kein stiller Ausfall).
   - EventLog-Sink-Konfiguration wird geladen — auf Windows direkt prüfbar,
     andernfalls erfolgreiches Laden der Konfiguration verifizieren.
3. `dotnet list package --vulnerable` ohne Befund.

## Randbedingungen / Hinweise

- OsInterface-Lane (Playwright, `Category=OsInterface`) ist **nicht** erforderlich.
- Reale E2E-Abnahme gefordert: App real unter Kestrel starten, Request ausführen,
  danach `logs/log-<datum>.txt` auf Existenz und Inhalt prüfen; Ergebnis in
  `test-results.md` dokumentieren. Bei Nicht-Durchführbarkeit begründet dokumentieren.
- `logs/` darf nicht committed werden (`.gitignore` bereits prüfen —
  Regel `[Ll]ogs/` vorhanden); Test-Artefakte aufräumen.
- Kein Issue-Kommentar, kein Merge/Push/PR. Endzustand: Lifecycle vollständig inkl.
  Abschlusscommit `feat: ...` auf dem Schritt-Branch.
- Keine Rückfragen: technisch begründbarste Entscheidung treffen und als Annahme
  dokumentieren; keine Scope-Erweiterung.

## Abgrenzung (Scope-Ausschlüsse)

- Keine Hebung der Sinks, sofern nicht zur Kompatibilität erforderlich (dokumentierte Annahme).
- Kein Umbau der Logging-Konfiguration (appsettings-Schema, Sinks, Pfade bleiben unverändert).
