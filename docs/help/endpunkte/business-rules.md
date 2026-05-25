# Endpunkte — Business Rules

## Platzhalter-Erkennung aus dem Pfad-Template

**Beschreibung:** Die Unterscheidung zwischen Pfad-Platzhaltern und regulären Query-Parametern wird nicht in der Datenbank gespeichert, sondern beim Laden und bei jeder Pfad-Bearbeitung dynamisch aus dem Template `RelativePath` abgeleitet.

**Bedingungen:**
- `RelativePath` enthält Ausdrücke der Form `{name}` (Regex: `\{([^}]+)\}`).

**Verhalten:**
- Wenn `{name}` im Pfad vorkommt: der zugehörige `QueryParamEntry` erhält `IsPathParameter = true` und kann nicht gelöscht werden.
- Wenn `{name}` nicht mehr im Pfad vorkommt: der zugehörige Eintrag mit `IsPathParameter = true` wird aus der Liste entfernt.
- Wird ein Platzhalter umbenannt, verbleibt der alte Parameterwert als regulärer (löschbarer) Query-Parameter in der Liste.

**Umsetzung:** `EndpointPage.SyncPathParameters()` — die dynamische Ableitung vermeidet ein persistiertes Flag und hält das Datenbankmodell (`EndpointQueryParameter`) frei von UI-spezifischen Metadaten.

---

## Keine Duplikate bei Query-String-Extraktion

**Beschreibung:** Wenn ein extrahierter Query-String-Key bereits als Parameter in der Liste vorhanden ist, wird kein Duplikat erzeugt.

**Bedingungen:**
- `_model.RelativePath` enthält ein `?`.
- Mindestens ein extrahierter Key ist bereits in `_queryParameters` vorhanden.

**Verhalten:**
- Wenn Key bereits vorhanden: vorhandener Eintrag bleibt unverändert; der extrahierte Wert wird verworfen.
- Wenn Key nicht vorhanden: neuer Eintrag mit `IsPathParameter = false` wird hinzugefügt.

**Umsetzung:** `EndpointPage.ExtractAndStripQueryString()` — der vorhandene Wert hat Vorrang, damit manuell eingetragene Werte nicht durch eine erneute Eingabe überschrieben werden.

---

## Leere Keys werden übersprungen

**Beschreibung:** Parameter mit leerem Key werden weder beim Aufbau des Query-Strings noch bei der Platzhalter-Ersetzung berücksichtigt.

**Bedingungen:**
- `string.IsNullOrWhiteSpace(param.Key)` ergibt `true`.

**Verhalten:**
- Der Eintrag wird in `BuildRequest()` vollständig ignoriert (kein Platzhalter-Ersatz, kein Query-String-Eintrag).
- In `ResolveDisplayUrl()` werden Einträge mit leerem Key ebenfalls übersprungen.

**Umsetzung:** `EndpointExecutionService.BuildRequest()` und `EndpointPage.ResolveDisplayUrl()` — verhindert ungültige URLs mit leeren `key=value`-Paaren.

---

## Pfad-Platzhalter-Werte werden URL-kodiert

**Beschreibung:** Werte, die Pfad-Platzhalter ersetzen, werden mit `Uri.EscapeDataString` kodiert — konsistent zum bestehenden Query-String-Encoding.

**Bedingungen:**
- Ein `QueryParameter.Key` entspricht einem `{Key}`-Platzhalter in `RelativePath`.

**Verhalten:**
- Der Wert wird vor dem Einsetzen in den Pfad via `Uri.EscapeDataString(Value)` kodiert.
- Leerzeichen werden zu `%20`, andere Sonderzeichen entsprechend kodiert.

**Umsetzung:** `EndpointExecutionService.BuildRequest()` — `Uri.EscapeDataString` ist für Pfad-Segmente semantisch korrekt (entspricht Prozent-Kodierung gemäß RFC 3986).

---

## Sortierung: Pfad-Platzhalter vor regulären Parametern

**Beschreibung:** Einträge mit `IsPathParameter = true` erscheinen in der Parameter-Liste immer vor den regulären Query-Parametern.

**Umsetzung:** `RequestQueryParamsPanel` — Sortierung via `QueryParameters.OrderByDescending(p => p.IsPathParameter)` im Razor-Template.

---

## RelativePath speichert ausschließlich das Pfad-Template

**Beschreibung:** In der Datenbank wird in `Endpoint.RelativePath` ausschließlich der Pfad-Anteil ohne Query-String gespeichert.

**Bedingungen:**
- Der Anwender gibt einen Pfad mit Query-String ein.

**Verhalten:**
- `ExtractAndStripQueryString()` bereinigt `RelativePath` beim Laden oder bei `onblur`.
- Beim Speichern wird der bereits bereinigte `_model.RelativePath` persistiert.
- Ein Endpunkt mit gespeichertem Query-String im Pfad wird beim nächsten Laden automatisch bereinigt; `_isDirty` wird dabei nicht gesetzt (keine Benutzeraktion nötig zum Speichern der Bereinigung).

**Umsetzung:** `EndpointPage.LoadModelFromParameter()` und `EndpointPage.OnPathBlur()`.

---

## Pre-Skript-Fehler verhindert HTTP-Request

**Beschreibung:** Wenn das Pre-Request-Skript fehlschlägt (Syntaxfehler, Runtime-Exception oder Timeout), wird der HTTP-Request nicht abgeschickt.

**Bedingungen:**
- `endpoint.PreRequestScript` ist nicht leer.
- `ScriptExecutionResult.Success == false`.

**Verhalten:**
- `EndpointExecutionService.ExecuteAsync` gibt sofort `EndpointExecutionResult { Success = false, ErrorMessage = ... }` zurück.
- Es wird keine HTTP-Anfrage gesendet.

**Umsetzung:** `EndpointExecutionService.ExecuteAsync()` — schützt vor unbeabsichtigten Requests bei fehlerhaften Skripten.

---

## Post-Skript-Fehler erhält das HTTP-Ergebnis

**Beschreibung:** Wenn das Post-Request-Skript fehlschlägt, bleibt das HTTP-Ergebnis vollständig erhalten; der Fehler wird nur als Ergänzung in `ErrorMessage` angezeigt.

**Bedingungen:**
- `endpoint.PostRequestScript` ist nicht leer.
- `ScriptExecutionResult.Success == false`.

**Verhalten:**
- `EndpointExecutionResult.ErrorMessage` wird gesetzt oder per `\n` ergänzt.
- `StatusCode`, `ResponseBody`, `ResponseHeaders`, `DurationMs` und `ResponseSizeBytes` bleiben unverändert.

**Umsetzung:** `EndpointExecutionService.ExecuteAsync()` — das HTTP-Ergebnis soll auch bei Skript-Problemen auswertbar bleiben.

---

## Rekursionsschutz für `sz.execute()`

**Beschreibung:** `sz.execute()` kann nicht beliebig tief rekursiv aufgerufen werden; ein Endpunkt darf im selben Aufrufbaum höchstens zweimal ausgeführt werden.

**Bedingungen:**
- Ein Skript ruft `sz.execute(name)` auf, und der Zielendpunkt ruft seinerseits `sz.execute()` mit demselben oder einem anderen Endpunkt auf.

**Verhalten:**
- Wenn `callDepth[id] >= 2`: Ausführung mit Fehler abgebrochen (`ErrorMessage` enthält Hinweis auf Rekursionsschutz).
- Der `CallDepth`-Zähler lebt im `ScriptContext` (nicht als Instanzfeld), damit parallele Requests sich nicht gegenseitig beeinflussen.

**Umsetzung:** `EndpointExecutionService.ExecuteAsync(endpoint, callDepth)` — verhindert Endlosrekursionen ohne globalen Zustand.

---

## Mehrdeutiger Endpunktname bei `sz.execute()` ergibt Fehler

**Beschreibung:** Gibt es innerhalb einer Anwendung mehrere Endpunkte mit demselben Namen, schlägt `sz.execute()` mit einem Fehler fehl — es wird kein „erster Treffer"-Verhalten angewendet.

**Bedingungen:**
- `IEndpointRepository.GetEndpointByNameAsync(applicationId, name)` gibt mehr als einen Eintrag zurück.

**Verhalten:**
- `EndpointExecutionResult { Success = false, ErrorMessage = "... Mehrdeutiger Endpunktname ..." }` wird zurückgegeben.

**Umsetzung:** `EndpointExecutionService.ExecuteEndpoint`-Callback — explizite Fehlerbehandlung vermeidet nicht-deterministisches Verhalten.

---

## `sz.environment.set()` ändert nur den In-Memory-Zustand

**Beschreibung:** `sz.environment.set(name, value)` ersetzt die aktive Umgebung im `IActiveEnvironmentService`-Scoped-Service, speichert aber nicht in der Datenbank.

**Bedingungen:**
- Das Skript ruft `sz.environment.set(name, value)` auf.

**Verhalten:**
- `IActiveEnvironmentService.SetActiveEnvironment` wird mit einer aktualisierten Kopie der `SystemEnvironment` aufgerufen.
- Alle Blazor-Komponenten, die auf `OnActiveEnvironmentChanged` reagieren, werden neu gerendert.
- Die Änderung ist nicht über Session-Grenzen hinaus persistent.

**Umsetzung:** `EndpointScriptRunner.BuildEnvironmentObject()` — der In-Memory-Ansatz ermöglicht temporäre Werte (z. B. OAuth-Tokens) ohne Datenbankschreibzugriff.
