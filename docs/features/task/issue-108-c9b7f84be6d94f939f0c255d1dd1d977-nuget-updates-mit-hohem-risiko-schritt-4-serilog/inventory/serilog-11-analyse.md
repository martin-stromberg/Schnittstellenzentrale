# Detailanalyse: Serilog-Stack 10.x → 11.0.0

## Quellen

- GitHub-Release `serilog/serilog-aspnetcore` **11.0.0** (18.11.2026, Tag `a56507a`, PR #407 „11.0.0 Release")
- NuGet-Seite `Serilog.AspNetCore` 11.0.0 (deklarierte Abhängigkeiten)
- GitHub-Release `serilog/serilog-settings-configuration` **11.0.0** (18.11.2026, PR #481)
- NuGet-Seiten `Serilog.Sinks.File` 7.0.0, `Serilog.Sinks.EventLog` 4.0.0, `Serilog` 4.4.0

## Commits im 11.0.0-Release (serilog-aspnetcore)

| Commit | Inhalt | Bewertung für diese App |
|---|---|---|
| #404 `9393ac0` | „Suppress exceptions for aborted requests" | Einzige Verhaltensänderung: betrifft ausschließlich die Request-Logging-Middleware (`UseSerilogRequestLogging`). Die App nutzt diese Middleware **nicht** (Volltextsuche: kein Treffer) → ohne Wirkung. |
| #405 `e92507e` | „Updates for .NET 11.0": net11.0-TFM hinzu, net8.0/net9.0-TFMs entfernt, Serilog auf 4.4.0 | Projekt targetet net11.0 → nativer TFM wird jetzt getroffen statt net10.0-Fallback. Kein API-Bruch. |
| `c9787f4` / `22894a7` | v11-Serilog-Abhängigkeiten (`Extensions.Hosting`, `Extensions.Logging`, `Settings.Configuration` 11.0.0) | Transitive Hebung; `ReadFrom.Configuration`-Pfad API-stabil. |
| `7d5d8ec` / `dab7524` | Trusted-Publishing-Migration | Nur Release-Prozess. |

## Deklarierte Abhängigkeiten von Serilog.AspNetCore 11.0.0 (net10.0/net11.0-Gruppe)

```
Serilog                        >= 4.4.0
Serilog.Extensions.Hosting     >= 11.0.0
Serilog.Formatting.Compact     >= 3.0.0
Serilog.Settings.Configuration >= 11.0.0
Serilog.Sinks.Console          >= 6.1.1
Serilog.Sinks.Debug            >= 3.0.0
Serilog.Sinks.File             >= 7.0.0   ← die App referenziert genau 7.0.0
```

Damit ist `Serilog.Sinks.File` 7.0.0 die vom Maintaner selbst gebündelte Version — Kompatibilität ist per Paketdeklaration gegeben.

## Serilog.Settings.Configuration 11.0.0

- Microsoft.Extensions.*-Abhängigkeiten auf 11.0 gehoben, net11-TFM, net8/9 gedroppt.
- Keine Änderung am `ReadFrom.Configuration`-Verhalten oder an der `WriteTo`-Assembly-Auflösung (Sink-Namensauflösung über referenzierte Assemblys bleibt; die App hat keinen `Using`-Abschnitt nötig, da die Sink-Pakete direkt referenziert sind).

## Serilog.Sinks.EventLog 4.0.0

- Letzter stabiler Release; TFMs net6.0/netstandard2.0/net462 → unter net11.0 via net6.0-Asset auflösbar.
- Bekanntes Bestandsverhalten (seit v3.0): `manageEventSource` default `false`. Die appsettings-Konfiguration setzt kein `manageEventSource` → die Event-Source `Schnittstellenzentrale` muss auf dem Zielhost registriert sein (IIS-/Dienst-Betrieb); ist sie es nicht, schweigt der Sink mit SelfLog-Hinweis. Das ist unverändertes Verhalten vor/nach dem Update.

## Risiko-Einschätzung

- **Kompilierzeit:** kein API-Bruch auf den genutzten Pfaden (`LoggerConfiguration`, `ReadFrom.Configuration`, `UseSerilog`, statisches `Log`).
- **Laufzeit:** File-Sink-Version wird vom neuen Hostpaket selbst verlangt; EventLog unverändert. Der einzig denkbare stille Ausfall wäre ein Bruch in der Konfig-Assembly-Auflösung — nicht angekündigt und durch die reale E2E-Abnahme (Datei wird geschrieben) verifiziert.
- **Verhaltensänderung Request-Logging:** nicht anwendbar (Middleware nicht im Einsatz).
