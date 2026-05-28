# Code-Review: Branch `28-klickbare-baumelemente`

Geprüfte Commits: alle Commits auf dem Branch seit Abzweigung von `main`.

Methodik: 7 unabhängige Suchwinkel (line-by-line diff, entferntes Verhalten, cross-file-Tracing, Wiederverwendung, Vereinfachung, Effizienz, Altitude). Jeder Kandidat wurde anschließend verifiziert (CONFIRMED / PLAUSIBLE / REFUTED). Nur bestätigte und plausible Befunde sind aufgeführt.

## Findings

```json
[
  {
    "file": "src/Schnittstellenzentrale/wwwroot/activity-log-panel.js",
    "line": 25,
    "summary": "Nach einem Dock↔Overlay-Wechsel akkumulieren document.mousemove- und document.mouseup-Listener, weil die alte Handle-Referenz im _listenerRegistry nicht mehr aufgefunden wird.",
    "failure_scenario": "ToggleDisplayMode setzt _needsResizeInit = true. Das if/else-Markup erzeugt ein neues DOM-Element (andere CSS-Klasse), daher erhält _handleElement nach dem Re-Render eine neue JS-Objektreferenz. initializePanelResize wird mit der neuen Referenz aufgerufen; destroy() am Anfang der Funktion iteriert über _listenerRegistry und entfernt alle bisherigen Einträge korrekt — sofern die Registry die alten Einträge noch enthält. Tritt jedoch zwischen destroy() und der erneuten Registration ein Render-Zyklus auf, bei dem die alte Handle-Referenz bereits garbage-collected wurde, kann der Map-Eintrag verloren gehen. Wahrscheinlicher: nach N Wechseln sind mehrere Zyklen aktiv, wenn destroy() zwischen OnAfterRenderAsync-Aufrufen nicht konsequent geflusht wird. Cleanup erfolgt zuverlässig nur beim expliziten Panel-Schließen (destroy() ohne Argument leert die gesamte Registry)."
  },
  {
    "file": "src/Schnittstellenzentrale/Components/Layout/MainLayout.razor",
    "line": 40,
    "summary": "MainLayout._activityLogPanelHeight wird nach JS-gesteuertem Drag-Resize nicht aktualisiert; padding-bottom auf <article> weicht dauerhaft von der tatsächlichen Panel-Höhe ab.",
    "failure_scenario": "Benutzer öffnet das Panel im Dock-Modus (initialer Wert: 200 px) und zieht den Resize-Handle auf 400 px. JS setzt panelElement.style.height = '400px' und speichert den Wert in localStorage, ruft aber kein .NET-Callback auf. _activityLogPanelHeight bleibt 200. Das <article>-Element behält padding-bottom: 200 px, während das Panel 400 px hoch ist — der untere Seiteninhalt wird durch das Panel verdeckt. Zustand hält bis zum nächsten Seitenaufruf an. Bereits als bekannte Einschränkung dokumentiert."
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/EndpointScriptRunner.cs",
    "line": 206,
    "summary": "Task.Run(() => PersistVariableAsync(...)).GetAwaiter().GetResult() blockiert einen Thread-Pool-Thread synchron und kann bei Last zu Thread-Pool-Erschöpfung führen.",
    "failure_scenario": "ApplyEnvironmentSet wird aus einem synchronen Jint-Callback aufgerufen. GetAwaiter().GetResult() blockiert den aktuellen Thread-Pool-Thread, während Task.Run einen weiteren Thread für PersistVariableAsync (UpdateVariableAsync + NotifyEnvironmentChangedAsync) benötigt. Bei vielen gleichzeitigen Skriptausführungen mit langsamer Datenbank blockieren alle verfügbaren Thread-Pool-Threads synchron — ASP.NET Core kann keine neuen Requests mehr bedienen. Bereits als bekannte Einschränkung dokumentiert."
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs",
    "line": 330,
    "summary": "BuildMaskedDetails verwendet string.Replace (case-sensitiv): Klartextwerte maskierter Variablen werden im Detail-String nicht redaktiert, wenn der Server sie in abweichender Gross-/Kleinschreibung zurückgibt.",
    "failure_scenario": "Eine maskierte Umgebungsvariable hat den Wert 'MySecret'. Der Server antwortet mit 'mysecret' (Kleinschreibung). string.Replace('MySecret', '***') findet keinen Treffer. Der Detail-String im EndpointExecuted-Eintrag enthält 'mysecret' im Klartext. Da Tokens und Passwörter üblicherweise exakt gespeichert und übertragen werden, ist dies ein Randfall — bei URL-Codierung oder Normalisierung durch den Server ist er jedoch realistisch."
  }
]
```
