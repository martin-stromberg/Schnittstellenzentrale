# Code-Review: Branch `30-redesign`

Geprüfte Commits: alle Commits auf dem Branch seit Abzweigung von `main`.

Methodik: 7 unabhängige Suchwinkel (line-by-line diff, entferntes Verhalten, cross-file-Tracing, Wiederverwendung, Vereinfachung, Effizienz, Altitude). Jeder Kandidat wurde anschließend verifiziert (CONFIRMED / PLAUSIBLE / REFUTED). Nur bestätigte und plausible Befunde sind aufgeführt.

## Findings

```json
[
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs",
    "line": 150,
    "summary": "Maskierte Variablenwerte können im message-Feld des ActivityLog-Eintrags im Klartext erscheinen, weil result.RequestDetails (die vollständig aufgelöste URL) ohne Maskierung als message übergeben wird.",
    "failure_scenario": "Eine maskierte Umgebungsvariable 'token=secret123' wird in einem Query-Parameter verwendet: ?token={{token}}. Die aufgelöste URL lautet 'GET https://host/api?token=secret123'. BuildMaskedDetails maskiert nur den details-Parameter (Request+Response-Body), nicht den message-Parameter. Das ActivityLog zeigt in der Eintrags-Überschrift 'GET https://host/api?token=secret123' im Klartext. Dasselbe gilt für Zeilen 157 und 164 (HttpError und Post-Script-Fehler-Eintrag)."
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs",
    "line": 137,
    "summary": "Wenn das Post-Script fehlschlägt (HTTP 200, Post-Script-Fehler), wird zuerst ein EndpointExecuted-Eintrag geloggt (result.HttpSuccess == true), dann ein InternalError-Eintrag — aber der EndpointExecuted-Eintrag enthält die ungemaskierte URL (siehe oben) und es gibt keinen Hinweis, dass der Aufruf letztlich fehlschlug.",
    "failure_scenario": "HTTP GET liefert 200 OK → result.HttpSuccess = true. Post-Script schlägt fehl → result.Success = false. Zeile 137: if (result.HttpSuccess) → true → EndpointExecuted geloggt mit 'GET https://... — 200'. Zeile 160: if (!result.Success && result.HttpSuccess) → InternalError geloggt. Das Protokoll enthält einen irreführenden EndpointExecuted-Eintrag, der dem Nutzer einen erfolgreichen Aufruf signalisiert, obwohl der Endpunkt als fehlgeschlagen zurückgegeben wird."
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/EndpointScriptRunner.cs",
    "line": 210,
    "summary": "Task.Run(PersistVariableAsync).GetAwaiter().GetResult() blockiert einen Thread-Pool-Thread synchron innerhalb eines synchronen Jint-Callbacks und riskiert Thread-Pool-Erschöpfung bei parallelen Skriptausführungen.",
    "failure_scenario": "ApplyEnvironmentSet wird aus einem synchronen Jint-Callback aufgerufen. GetAwaiter().GetResult() blockiert den aktuellen Thread-Pool-Thread, während Task.Run() einen weiteren Thread für PersistVariableAsync (DB-Schreibzugriff + SignalR-Benachrichtigung) benötigt. Bei vielen gleichzeitigen Skriptausführungen mit langsamer Datenbank oder hoher SignalR-Last können alle Thread-Pool-Threads blockiert sein — neue Requests können nicht bedient werden. Bereits als bekannte Einschränkung kommentiert."
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/ActivityLogService.cs",
    "line": 43,
    "summary": "ActivityLogService hat keine Obergrenze für die Anzahl der Einträge — die In-Memory-Liste wächst unbegrenzt über die Lebensdauer eines Blazor-Circuits.",
    "failure_scenario": "Bei einer langlebigen Sitzung mit häufigen Endpunkt-Aufrufen (z. B. automatisierter Test-Loop oder Stress-Test) wächst _entries kontinuierlich. Jeder Aufruf erzeugt mindestens einen Eintrag (EndpointExecuted oder HttpError), bei Skripten mehrere. Nach tausenden Aufrufen kann die Liste signifikant Speicher belegen. Da der Service Scoped (pro Circuit) ist, wird er erst beim Schließen der Verbindung freigegeben."
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/EndpointScriptRunner.cs",
    "line": 48,
    "summary": "ScriptExecuted-Log unterscheidet nicht zwischen Pre-Request- und Post-Request-Skript — der Eintrag 'Skript ausgeführt: <Name>' erscheint für beide, was die Diagnose bei Fehlern im Post-Script erschwert.",
    "failure_scenario": "Ein Endpunkt hat Pre- und Post-Request-Skript. Das Pre-Script läuft erfolgreich ('Skript ausgeführt: MeinEndpunkt'). Das Post-Script schlägt fehl ('InternalError: JavaScript-Fehler in Skript: MeinEndpunkt'). Im Protokoll ist nicht erkennbar, ob der ScriptExecuted-Eintrag vom Pre- oder Post-Script stammt, da context.EndpointName in beiden Fällen identisch ist und kein Skript-Typ-Hinweis übergeben wird."
  }
]
```
