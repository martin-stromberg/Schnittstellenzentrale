# Aktivitätsprotokoll — Business Rules

## Maskierung von Umgebungsvariablen in Details

**Beschreibung:** Werte von als sensibel markierten Umgebungsvariablen dürfen im Protokoll nicht im Klartext erscheinen.

**Bedingungen:**
- Betrifft ausschließlich den `details`-String von `EndpointExecuted`-Einträgen.
- Eine Variable gilt als sensibel, wenn `EnvironmentVariable.IsValueMasked == true`.

**Verhalten:**
- Nach Aufbau des Detail-Strings (Request- und Response-Inhalte, je max. 10.240 Zeichen) werden alle Vorkommen der Klartextwerte maskierter Variablen durch `***` ersetzt.
- Nicht maskierte Variablen erscheinen unverändert.

**Umsetzung:** `EndpointExecutionService.BuildMaskedDetails(string details, IEnumerable<EnvironmentVariable> variables)` — iteriert über alle maskierten Variablen mit nicht leerem Wert und führt `string.Replace` auf dem fertigen Detail-String aus.

---

## Keine Details bei HTTP-Fehlern

**Beschreibung:** Bei HTTP-Fehlerantworten (Statuscodes 4xx und 5xx) wird kein Response-Body protokolliert.

**Bedingungen:**
- `!response.IsSuccessStatusCode && result.StatusCode.HasValue`

**Verhalten:**
- Der `HttpError`-Eintrag enthält nur Methode, URL und Statuscode in der Nachricht.
- Kein `details`-String; insbesondere kein Response-Body und kein StackTrace.

**Umsetzung:** `EndpointExecutionService.ExecuteAsync` — der `Log(HttpError, ...)` -Aufruf ohne `details`-Parameter.

---

## Fehlertoleranz des Log-Aufrufs

**Beschreibung:** Ein fehlgeschlagener Protokollaufruf darf die aufrufende Methode nicht unterbrechen.

**Bedingungen:**
- Gilt für alle Aufrufer: `EndpointExecutionService`, `EndpointScriptRunner`, `Home.razor`, `ApplicationGroupTree.razor`, `MainLayout.razor`.

**Verhalten:**
- `ActivityLogService.Log` fängt Fehler beim Erstellen des `ActivityLogEntry` intern ab und legt einen `InternalError`-Platzhalter an.
- Fehler im `OnEntryAdded`-Handler werden still ignoriert.
- Die aufrufenden Methoden betten `Log`-Aufrufe daher nicht in eigene `try/catch`-Blöcke ein.

**Umsetzung:** `ActivityLogService.Log` — zwei `try/catch`-Blöcke: einer um die Eintragserstellung, einer um das Event-Feuern.

---

## Kein ContextSwitched beim initialen Restore

**Beschreibung:** Das automatische Wiederherstellen der zuletzt aktiven Umgebung beim Laden der Seite soll keinen `ContextSwitched`-Eintrag erzeugen.

**Bedingungen:**
- `RestoreEnvironmentFromLocalStorageAsync` wird beim ersten Render in `MainLayout.OnAfterRenderAsync(firstRender: true)` aufgerufen.

**Verhalten:**
- `RestoreEnvironmentFromLocalStorageAsync` ruft `ActiveEnvironmentService.SetActiveEnvironment` direkt auf — ohne `Log`-Aufruf.
- Nur wenn der Benutzer aktiv eine Umgebung über den `EnvironmentSelector` auswählt, wird `OnEnvironmentSelectedByUser` aufgerufen und ein `ContextSwitched`-Eintrag erzeugt.

**Umsetzung:** `MainLayout.OnEnvironmentSelectedByUser` ist ein separater Event-Handler, der nur aus dem `EnvironmentSelector.OnEnvironmentSelectedByUser`-Callback aufgerufen wird — nicht aus `RestoreEnvironmentFromLocalStorageAsync`.

---

## sz.console.write erscheint auch bei Skriptabbruch

**Beschreibung:** Ausgaben von `sz.console.write(text)` erscheinen im Protokoll auch dann, wenn das Skript danach mit einem Fehler abbricht.

**Bedingungen:**
- `sz.console.write` wird im Skript vor der fehlerhaften Stelle aufgerufen.

**Verhalten:**
- `Log(ScriptConsoleOutput, text)` wird synchron im Lambda ausgeführt, sobald `sz.console.write` aufgerufen wird.
- Der Eintrag ist bereits in `_entries` eingetragen, bevor die Exception ausgelöst wird.

**Umsetzung:** `EndpointScriptRunner.RegisterSzObject` — das `sz.console.write`-Lambda ruft `_activityLogService.Log` direkt (synchron) auf.
