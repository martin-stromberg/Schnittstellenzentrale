# Code-Review: Branch `30-redesign`

Geprüfte Commits: alle Commits auf dem Branch seit Abzweigung von `main`, einschließlich der uncommitted Working-Tree-Änderungen und ungetrackte neue Dateien.

Methodik: 7 unabhängige Suchwinkel (line-by-line diff, entferntes Verhalten, cross-file-Tracing, Wiederverwendung, Vereinfachung, Effizienz, Altitude). Jeder Kandidat wurde anschließend verifiziert (CONFIRMED / PLAUSIBLE / REFUTED). Nur bestätigte und plausible Befunde sind aufgeführt.

## Findings

```json
[
  {
    "file": "src/Schnittstellenzentrale/Components/Shared/ContentHeader.razor",
    "line": 150,
    "summary": "TriggerFileInput verwendet document.querySelector statt der vorhandenen @ref-Referenz — bei mehreren ContentHeader-Instanzen auf einer Seite wird der Datei-Dialog des falschen Elements geöffnet.",
    "failure_scenario": "Die ContentHeader-Komponente hält bereits eine @ref-Referenz _inputFile auf das InputFile-Element. TriggerFileInput ignoriert diese und ruft stattdessen JSRuntime.InvokeVoidAsync('eval', \"document.querySelector('input[type=file].sz-hidden-file-input')?.click()\") auf. Falls eine Seite zwei ContentHeader-Instanzen rendert (z.B. Sammlung und Anwendung gleichzeitig sichtbar), klickt der querySelector auf das erste passende Element im DOM — unabhängig davon, welchen Upload-Button der Benutzer betätigt hat. Die korrekte Lösung wäre, _inputFile.Element per JSRuntime direkt anzusprechen."
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/HistoryService.cs",
    "line": 50,
    "summary": "GetPagedAsync validiert den page-Parameter nicht — page=0 ergibt Skip(-pageSize) und löst eine ArgumentOutOfRangeException aus.",
    "failure_scenario": "Ein Aufrufer übergibt page=0 (z.B. ein direkter Serviceaufruf in einem Test oder eine zukünftige Verwendung). Skip((0-1)*pageSize) = Skip(-pageSize) ist für EF Core kein gültiger Aufruf und wirft ArgumentOutOfRangeException zur Laufzeit. Die HistoryContentView initialisiert _page mit 1 und schützt die UI-Seite vor diesem Pfad; der Service selbst hat keine Validierung."
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs",
    "line": 182,
    "summary": "DurationMs (long) wird ohne Überlaufprüfung auf int gecastet — bei Anfragen mit mehr als ca. 24 Tagen Laufzeit wird ein falscher Wert in EndpointCallHistoryEntry.DurationMs gespeichert.",
    "failure_scenario": "result.DurationMs.Value ist vom Typ long. Der Cast (int)result.DurationMs.Value ist in C# ein unchecked Narrowing Cast. Bei einem Wert > 2.147.483.647 ms (~24,8 Tage) überläuft der Cast stillschweigend zu einem negativen oder fehlerhaften Wert. In der Praxis selten, da HTTP-Requests mit einem realistischen Timeout enden. Empfehlung: checked-Cast oder Clamping auf int.MaxValue."
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/EndpointScriptRunner.cs",
    "line": 209,
    "summary": "Task.Run(() => PersistVariableAsync(...)).GetAwaiter().GetResult() blockiert einen Thread-Pool-Thread synchron und kann bei Last zu Thread-Pool-Erschöpfung führen.",
    "failure_scenario": "ApplyEnvironmentSet wird aus einem synchronen Jint-Callback aufgerufen. GetAwaiter().GetResult() blockiert den aktuellen Thread-Pool-Thread, während Task.Run einen weiteren Thread für UpdateVariableAsync + NotifyEnvironmentChangedAsync benötigt. Bei vielen gleichzeitigen Skriptausführungen mit langsamer Datenbank blockieren alle verfügbaren Thread-Pool-Threads synchron — ASP.NET Core kann keine neuen Requests mehr bedienen. Bereits als bekannte Einschränkung dokumentiert."
  },
  {
    "file": "src/Schnittstellenzentrale/Components/Layout/MainLayout.razor",
    "line": 40,
    "summary": "MainLayout._activityLogPanelHeight wird nach einem JS-gesteuerten Drag-Resize nicht aktualisiert; padding-bottom auf <article> weicht dauerhaft von der tatsächlichen Panel-Höhe ab.",
    "failure_scenario": "Benutzer öffnet das Panel im Dock-Modus (Standardwert: 200 px) und zieht den Resize-Handle auf 400 px. JS setzt panelElement.style.height = '400px' und speichert den Wert in localStorage, ruft aber kein .NET-Callback auf. _activityLogPanelHeight bleibt 200. Das <article>-Element behält padding-bottom: 200 px, während das Panel 400 px hoch ist — der untere Seiteninhalt wird durch das Panel verdeckt. Bereits als bekannte Einschränkung dokumentiert."
  }
]
```
