# Bestandsaufnahme – F4: Serilog.AspNetCore 10.0.0 → 11.0.0

## Paketbestand (Ist)

Direkte Referenzen in `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj` (Zeilen 37–39):

- `Serilog.AspNetCore` **10.0.0**
- `Serilog.Sinks.EventLog` **4.0.0**
- `Serilog.Sinks.File` **7.0.0**

Aktueller transitiver Graph (`dotnet list package --include-transitive`):

| Paket | Aufgelöst (vor Update) | Erwartet nach Update (deklarierte Abhängigkeit von AspNetCore 11.0.0) |
|---|---|---|
| Serilog | 4.3.0 | ≥ 4.4.0 |
| Serilog.Extensions.Hosting | 10.0.0 | ≥ 11.0.0 |
| Serilog.Extensions.Logging | 10.0.0 | ≥ 11.0.0 (via Hosting) |
| Serilog.Formatting.Compact | 3.0.0 | ≥ 3.0.0 |
| Serilog.Settings.Configuration | 10.0.0 | ≥ 11.0.0 |
| Serilog.Sinks.Console | 6.1.1 | ≥ 6.1.1 |
| Serilog.Sinks.Debug | 3.0.0 | ≥ 3.0.0 |
| Serilog.Sinks.File | 7.0.0 | ≥ 7.0.0 (AspNetCore 11 deklariert File 7.0.0 selbst als Abhängigkeit) |
| Serilog.Sinks.EventLog | 4.0.0 (direkt) | keine AspNetCore-Abhängigkeit; net6.0/netstandard2.0 → unter net11 kompatibel |

Keine Serilog-Referenzen in Core/Infrastructure/Tests-csproj.

## Betroffene Codestellen (verifiziert)

- `src/Schnittstellenzentrale/Program.cs`
  - Zeile 3: `using Serilog;`
  - Zeilen 43–47: Bootstrap-Logger `Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()).CreateLogger();`
  - Zeile 59: `builder.Host.UseSerilog();`
  - `BuildWebApplicationAsync` ist `public` und wird aus Tests (`WebApplicationFactory`/`Program`) aufgerufen — Bootstrap-Pfad läuft also auch in Integrationstests.
- `src/Schnittstellenzentrale/SystemEntryInitializer.cs` (Zeilen 5, 28, 67): statische `Log.Warning`/`Log.Error`-API — Version-agnostisch.
- `src/Schnittstellenzentrale/appsettings.json` (Zeilen 13–32): `Serilog`-Abschnitt, `MinimumLevel: Information`, `WriteTo: [EventLog(source "Schnittstellenzentrale", logName "Application"), File(path "logs/log-.txt", rollingInterval Day, retainedFileCountLimit 7)]`. Kein `Using`-Abschnitt — Sinks werden über referenzierte Assemblys aufgelöst.
- Infrastruktur-Kommentare verweisen auf den EventLog-Sink (`EndpointExecutionService.cs:291`, `WindowsCurrentUserService.cs:33`) — nur Kommentare.
- **Kein** `UseSerilogRequestLogging()` im Code — die einzige dokumentierte Verhaltensänderung von 11.0.0 (Request-Logging unterdrückt Exceptions bei client-seitig abgebrochenen Requests) ist daher nicht wirksam.

## Changelog 11.0.0 (GitHub serilog/serilog-aspnetcore, Release 11.0.0 vom 18.11.2026)

Änderungen gegenüber 10.0.0:
- **.NET-11-TFM hinzugefügt, .NET 8/9-TFMs entfernt** — net10.0/net11.0 bleiben; Projekt läuft auf net11.0 → passt.
- **Serilog-Kernabhängigkeit 4.3.x → 4.4.0**; alle `Serilog.*`-Satelliten auf v11-Linie (`Extensions.Hosting`, `Extensions.Logging`, `Settings.Configuration` je ≥ 11.0.0).
- **Verhaltensänderung:** Request-Logging (`UseSerilogRequestLogging`) unterdrückt jetzt Exceptions bei vom Client abgebrochenen Requests. → Für diese App ohne Request-Logging-Middleware ohne Wirkung.
- `Serilog.Settings.Configuration` 11.0.0: Microsoft.Extensions.*-Abhängigkeiten auf 11.0, net11-TFM, net8/9 gedroppt — `ReadFrom.Configuration`-API und `WriteTo`-Assembly-Auflösung unverändert.
- Keine API-Entfernungen, die den genutzten Pfad betreffen (`LoggerConfiguration().ReadFrom.Configuration`, `Host.UseSerilog()`, statisches `Log.Logger`).

Details: [inventory/serilog-11-analyse.md](inventory/serilog-11-analyse.md)

## Sink-Kompatibilität

- **Serilog.Sinks.File 7.0.0:** von `Serilog.AspNetCore` 11.0.0 selbst als `>= 7.0.0`-Abhängigkeit deklariert → offiziell kompatibel. Kein stabiler neuerer Release (nur Prerelease). **Verbleibt auf 7.0.0.**
- **Serilog.Sinks.EventLog 4.0.0:** targetet net6.0/netstandard2.0, unter net11 via Nearest-TFM auflösbar; hängt an Serilog ≥ 4.x (Major-Kompatibilität zu 4.4.0 gegeben). Letzter stabiler Stand; v3.0-Änderung (`manageEventSource` Default `false`) bereits bekannt — Konfiguration setzt `manageEventSource` nicht, d. h. Source `Schnittstellenzentrale` muss im EventLog registriert sein, sonst schweigt der Sink (Bestandsverhalten, unverändert). **Verbleibt auf 4.0.0.**

## Test-/Infrastruktur-Bezug

- Reguläre Lane: `dotnet test --filter "Category!=OsInterface"` — Baseline **641 Tests** (letzter Schritt: 641/641 grün).
- `logs/` ist per `.gitignore` Regel `[Ll]ogs/` (Zeile 48) ignoriert — verifiziert via `git check-ignore` auf `src/Schnittstellenzentrale/logs/log-*.txt` (Bestandsdateien vorhanden).
- Reale Abnahme: App unter Kestrel starten (`dotnet run` / kompilierte DLL), HTTP-Request ausführen, `logs/log-<datum>.txt` prüfen.
- EventLog-Verifikation auf diesem Windows-Host: `Get-EventLog`/`Get-WinEvent` auf Log `Application`, Source `Schnittstellenzentrale` — prüfbar, ob Einträge ankommen bzw. ob die Konfiguration fehlerfrei lädt (SelfLog/Start ohne Exception ist Mindestnachweis).

## Relevante Konventionen

- Projekt-Artefakte des Elternprojekts: `docs/projects/task/issue-108-...-nuget-updates-mit-hohem-risiko/` — `decisions.md` erhält je Schritt einen Entscheidungsabschnitt (F5/F6 als Vorbild), `docs/RELEASE_NOTES.md` wird EN+DE gepflegt.
- Feature-Artefakte werden am Ende gelöscht (Lifecycle); Planungscommit sichert sie in der Historie.
