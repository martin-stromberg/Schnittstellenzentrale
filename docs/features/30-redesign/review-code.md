# Code-Review: Branch `30-redesign`

Geprüfte Commits: alle Commits auf dem Branch seit Abzweigung von `main`.

Methodik: 7 unabhängige Suchwinkel (line-by-line diff, entferntes Verhalten, cross-file-Tracing, Wiederverwendung, Vereinfachung, Effizienz, Altitude). Jeder Kandidat wurde anschließend verifiziert (CONFIRMED / PLAUSIBLE / REFUTED). Nur bestätigte und plausible Befunde sind aufgeführt.

## Findings

```json
[
  {
    "file": "src/Schnittstellenzentrale/Components/Shared/HistoryContentView.razor",
    "line": 141,
    "summary": "Filterfelder (_filterFrom, _filterTo) werden mit DateTimeKind.Unspecified an GetPagedAsync übergeben, während ExecutedAt als UTC gespeichert ist — der Filter liefert falsche Ergebnisse auf Servern, die nicht in UTC laufen.",
    "failure_scenario": "Blazor Server läuft auf einem Server in UTC+2. Nutzer gibt '2026-05-31 14:00' als Von-Filter ein. Das datetime-local-Eingabefeld liefert DateTimeKind.Unspecified. Die entfernte SpecifyKind+ToUniversalTime-Konvertierung hatte das korrekt auf 12:00 UTC umgerechnet. Ohne diese Konvertierung wird '14:00 Unspecified' direkt mit den als TEXT gespeicherten UTC-Werten (z. B. '2026-05-31T12:00:00') verglichen. SQLite vergleicht Strings lexikografisch: '14:00' > '12:00', sodass Einträge von 12:00–14:00 UTC aus dem Fenster fallen und Einträge von 14:00–16:00 UTC fälschlich einbezogen werden. Das Filterfenster ist um den UTC-Offset des Servers verschoben."
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/HistoryService.cs",
    "line": 66,
    "summary": "GroupJoin nach Take(count) in GetTopEndpointsAsync: EF Core kann die kombinierte Query (GroupBy+Take+GroupJoin über zwei Tabellen) nicht als einzelnes SQL übersetzen und führt den GroupJoin client-seitig aus — auf SQL Server ist die Take-Beschränkung als DB-seitiges LIMIT dadurch unwirksam.",
    "failure_scenario": "Auf SQL Server als Provider lädt EF Core die GroupBy+OrderBy+Take-Abfrage als Subquery in die Liste, wechselt dann für den GroupJoin zu client-seitiger Auswertung. Dabei werden alle Endpoint-Zeilen der Anwendung in memory geladen, nicht nur count viele. Bei Anwendungen mit vielen hundert Endpunkten und tausenden History-Einträgen entsteht N+1-ähnlicher Speicherdruck. SQLite maskiert das Problem, weil in-memory LINQ-Evaluation dort de facto ebenfalls client-seitig stattfindet. Die nachgelagerte In-Memory-Sortierung auf Zeile 76 ist korrekt als Sicherheitsnetz, ändert aber nichts am fehlenden DB-Level-Take."
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/EndpointScriptRunner.cs",
    "line": 214,
    "summary": "Task.Run(PersistVariableAsync).GetAwaiter().GetResult() blockiert einen Thread-Pool-Thread synchron innerhalb eines synchronen Jint-Callbacks und riskiert Thread-Pool-Erschöpfung bei vielen gleichzeitigen Skript-Ausführungen.",
    "failure_scenario": "ApplyEnvironmentSet wird aus einem synchronen Jint-Callback aufgerufen. GetAwaiter().GetResult() blockiert den aktuellen Thread-Pool-Thread für die gesamte Dauer von UpdateVariableAsync + NotifyEnvironmentChangedAsync (DB-Schreibzugriff + SignalR). Task.Run() belegt einen weiteren Thread. Bei N gleichzeitigen Skript-Ausführungen mit langsamer Datenbank können alle Thread-Pool-Threads blockiert sein. ASP.NET Core kann dann keine neuen Requests mehr bedienen (Thread-Pool-Erschöpfung). Bereits als bekannte Einschränkung im Code kommentiert; eine vollständige Behebung erfordert Jint-seitige Async-Callback-Unterstützung."
  },
  {
    "file": "src/Schnittstellenzentrale/Components/Shared/HistoryContentView.razor",
    "line": 141,
    "summary": "Redundante Ternary-Ausdrücke in LoadAsync: _filterFrom.HasValue ? _filterFrom.Value : (DateTime?)null ist äquivalent zu _filterFrom — Überbleibsel der entfernten SpecifyKind-Konvertierung.",
    "failure_scenario": "Kein Laufzeitfehler, aber der Code täuscht eine Transformation vor, die nicht stattfindet. Bei einem späteren Review könnte ein Entwickler annehmen, der Ausdruck sei bewusst gewählt. Die korrekte Vereinfachung lautet: var filterFrom = _filterFrom; var filterTo = _filterTo; (oder direkt im HistoryFilter-Konstruktor)."
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs",
    "line": 113,
    "summary": "Im catch-Block für HTTP-Ausführungsfehler (Zeile 113) wird ex.ToString() als details-Parameter an ActivityLogService.Log übergeben — das kann den vollständigen Stack-Trace inkl. interner Fehlermeldungen in den Activity-Log schreiben, der für alle Nutzer der Session sichtbar ist.",
    "failure_scenario": "Eine HttpRequestException enthält typischerweise Netzwerkdetails (IP, Port, Zertifikatsfehlertext). Ex.ToString() auf Zeile 116 schreibt den gesamten Stack-Trace als 'details' in den Activity-Log-Eintrag. In einem Multi-Nutzer-Szenario (SharedMode) würde der Stack-Trace für alle Nutzer der Anwendungssitzung sichtbar — je nach Infrastruktur können dabei interne Netzwerkadressen oder Zertifikatsinformationen preisgegeben werden. Für InternalError-Einträge des Pre-Request-Skripts (Zeile 115-116) besteht dieselbe Problematik."
  }
]
```
