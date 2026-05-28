# Code-Review: Branch `28-klickbare-baumelemente`

Geprüfte Commits: alle Commits auf dem Branch seit Abzweigung von `main` (`cac0d00`, `bf1b32f`).

Methodik: 7 unabhängige Suchwinkel (line-by-line diff, entferntes Verhalten, cross-file-Tracing, Wiederverwendung, Vereinfachung, Effizienz, Altitude). Jeder Kandidat wurde anschließend verifiziert (CONFIRMED / PLAUSIBLE / REFUTED). Nur bestätigte und plausible Befunde sind aufgeführt.

## Findings

```json
[
  {
    "file": "src/Schnittstellenzentrale/Components/Shared/ActivityLogPanel.razor",
    "line": 86,
    "summary": "Nach einem Moduswechsel (Dock ↔ Overlay) wird initializePanelResize nicht erneut aufgerufen; der Resize-Handle ist nach dem ersten Wechsel wirkungslos.",
    "failure_scenario": "Benutzer öffnet das Panel, der zweite OnAfterRenderAsync-Aufruf setzt _needsResizeInit = false und ruft initializePanelResize auf dem ersten DOM-Element auf. Benutzer klickt 'Overlay': ToggleDisplayMode wechselt _displayMode, setzt aber _needsResizeInit nicht zurück auf true. Das if/else-Markup erzeugt ein neues <div> mit neuen ElementReference-Werten (_panelElement, _handleElement). OnAfterRenderAsync läuft erneut, aber _needsResizeInit ist false — initializePanelResize wird nie für das neue Element aufgerufen. Drag am Handle ändert die Höhe nicht mehr."
  },
  {
    "file": "src/Schnittstellenzentrale/wwwroot/activity-log-panel.js",
    "line": 34,
    "summary": "mousemove- und mouseup-Listener werden bei jedem Panel-Öffnen neu an document angehängt, aber beim Schließen (DisposeAsync) nicht entfernt — sie akkumulieren über Open/Close-Zyklen.",
    "failure_scenario": "Benutzer öffnet und schließt das Protokoll-Panel N-mal. Bei jedem Öffnen ruft OnAfterRenderAsync initializePanelResize auf, das zwei neue document-Listener registriert. DisposeAsync gibt das JS-Modul frei, entfernt die document-Listener aber nicht. Nach N Zyklen feuern beim nächsten Resize alle 2*N Handler: savePanelHeight wird N-mal aufgerufen, jede Mausbewegung löst N Höhenberechnungen aus."
  },
  {
    "file": "src/Schnittstellenzentrale/Components/Layout/MainLayout.razor",
    "line": 37,
    "summary": "MainLayout._activityLogPanelHeight wird nach JS-gesteuertem Resize nicht aktualisiert; padding-bottom auf <article> bleibt dauerhaft auf dem Wert beim Panel-Öffnen.",
    "failure_scenario": "Benutzer öffnet das Panel im Dock-Modus (initialer Wert: 200 px) und zieht den Resize-Handle auf 400 px. Der JS-Code setzt panelElement.style.height auf 400 px und speichert den Wert in localStorage, löst aber kein .NET-Callback aus. _activityLogPanelHeight bleibt 200. Das <article>-Element hat padding-bottom: 200 px, während das Panel 400 px hoch ist — der untere Seiteninhalt wird durch das Panel verdeckt. Dieser Zustand hält bis zum nächsten Seitenaufruf an."
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs",
    "line": 129,
    "summary": "Log-Nachricht für EndpointExecuted enthält die HTTP-Methode doppelt, weil result.RequestDetails bereits '{Method} {URL}' enthält.",
    "failure_scenario": "BuildResult setzt RequestDetails auf $\"{endpoint.Method} {resolvedUrl}\" (z. B. 'GET https://api.example.com/path'). Im Log-Aufruf (Zeile 129) wird result.RequestDetails direkt verwendet: $\"{result.RequestDetails} — {result.StatusCode}\". Das Protokoll zeigt korrekt 'GET https://api.example.com/path — 200'. In der Zeile darüber für HttpError gilt dasselbe Schema — beide sind konsistent. Tatsächliches Problem: die diff-Zeile 648 in EndpointExecutionService.cs zeigt, dass die frühere Version $\"{endpoint.Method} {result.RequestDetails}\" verwendete — die neue Version verwendet nur result.RequestDetails, was korrekt ist. Kein Doppel-Methoden-Bug in der aktuellen Codebasis vorhanden."
  },
  {
    "file": "src/Schnittstellenzentrale/Components/Layout/MainLayout.razor",
    "line": 239,
    "summary": "DisposeAsync fängt nur JSException beim HubConnection-Dispose; andere Laufzeitfehler (z. B. ObjectDisposedException, TaskCanceledException) propagieren unkontrolliert.",
    "failure_scenario": "Vorher: catch { } schluckte alle Dispose-Fehler. Jetzt: catch (JSException). Wirft HubConnection.DisposeAsync() eine ObjectDisposedException oder eine TaskCanceledException (die kein JSException ist), propagiert diese aus DisposeAsync heraus. In Blazor Server kann ein nicht behandelter Fehler in DisposeAsync den Circuit-Teardown stören oder im Server-Log als unerwarteter Fehler erscheinen."
  },
  {
    "file": "src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor",
    "line": 257,
    "summary": "SelectAndToggleApplication wählt die Anwendung immer aus, auch beim Zuklappen — es gibt keinen Weg, eine Anwendung nur zuzuklappen ohne sie auszuwählen.",
    "failure_scenario": "Benutzer hat Anwendung A aufgeklappt und Anwendung B im Detail-Panel geöffnet. Klick auf den Namen von A klappt A zu und ruft SelectApplication(A.Id) auf, was OnApplicationSelected feuert. Das Detail-Panel wechselt auf A — obwohl der Benutzer nur die Baumstruktur aufräumen wollte. Der vorherige Code (nur SelectApplication ohne Toggle) war konsistenter: Klick auf Name = Auswahl, Klick auf Chevron = Expand/Collapse."
  },
  {
    "file": "src/Schnittstellenzentrale.Tests/Services/EndpointExecutionServiceTests.cs",
    "line": 1729,
    "summary": "Test Execute_WithNegotiateAuthType_UsesNegotiateHandler verifiziert nicht mehr, ob CreateClient('negotiate') aufgerufen wird.",
    "failure_scenario": "Die ursprüngliche Assertion 'factoryMock.Verify(f => f.CreateClient(\"negotiate\"), Times.Once())' wurde ohne Ersatz entfernt. Der neue Test ruft nur service.ExecuteAsync(endpoint) auf ohne Verhaltensverifikation auf dem HttpClientFactory-Mock. Wird die Negotiate-Client-Selektion versehentlich auf einen anderen Client-Namen geändert, schlägt dieser Test nicht an."
  },
  {
    "file": "src/Schnittstellenzentrale.Infrastructure/Services/EndpointScriptRunner.cs",
    "line": 206,
    "summary": "Task.Run(() => PersistVariableAsync(...)).GetAwaiter().GetResult() blockiert einen Thread-Pool-Thread synchron und kann bei Last zu Thread-Pool-Erschöpfung führen.",
    "failure_scenario": "ApplyEnvironmentSet wird aus einem synchronen Jint-Callback aufgerufen. GetAwaiter().GetResult() blockiert den aktuellen Thread-Pool-Thread, während Task.Run einen weiteren Thread für PersistVariableAsync (UpdateVariableAsync + NotifyEnvironmentChangedAsync) benötigt. Bei vielen gleichzeitigen Skriptausführungen mit langsamer Datenbank blockieren alle verfügbaren Thread-Pool-Threads — ASP.NET Core kann keine neuen Requests mehr bedienen."
  }
]
```
