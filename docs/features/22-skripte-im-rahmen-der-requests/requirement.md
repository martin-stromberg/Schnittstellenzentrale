# Anforderung: Skripte für Endpunkte (Pre/Post-Request)

## Fachliche Zusammenfassung

Das `Endpoint`-Modell wird um zwei optionale JavaScript-Skriptfelder (`PreRequestScript`, `PostRequestScript`) erweitert, die wie alle anderen Endpunkteigenschaften persistiert werden. `EndpointExecutionService.ExecuteAsync` wird so erweitert, dass das Pre-Request-Skript vor der `{{...}}`-Platzhalterauflösung und das Post-Request-Skript nach dem HTTP-Request ausgeführt werden. Die Skriptausführung erfolgt über eine neue Klasse `EndpointScriptRunner`, die einen isolierten JavaScript-Kontext (z. B. via `Jint` oder `Microsoft.ClearScript`) aufbaut und ein fest definiertes `sz`-API-Objekt exponiert. Dieses API erlaubt lesenden Zugriff auf Request-Daten (`sz.request`), lesenden und schreibenden Zugriff auf Umgebungsvariablen (`sz.environment`), Response-Daten im Post-Skript (`sz.response`) sowie die synchrone Ausführung anderer Endpunkte der gleichen Anwendung (`sz.execute()`). `EndpointPage` erhält zwei neue Registerkarten für die Skripteingabe. Endpunkte ohne Skript verhalten sich wie bisher.

---

## Betroffene Klassen und Komponenten

### Datenmodellklassen — zu erweitern (`Schnittstellenzentrale.Core`)

| Klasse | Änderung |
|---|---|
| `Endpoint` | Zwei neue Felder: `PreRequestScript` (`string?`) und `PostRequestScript` (`string?`). |

### Datenbankschicht — zu erweitern (`Schnittstellenzentrale.Infrastructure`)

| Artefakt | Änderung |
|---|---|
| EF-Core-Migration | Neue Spalten `PreRequestScript` (`nvarchar(max)`, nullable) und `PostRequestScript` (`nvarchar(max)`, nullable) auf der `Endpoint`-Tabelle. |

### Interfaces — neu (`Schnittstellenzentrale.Core`)

| Interface | Beschreibung |
|---|---|
| `IEndpointScriptRunner` | Definiert die Ausführung eines Skripts: `ExecuteAsync(string script, ScriptContext context)` → `ScriptExecutionResult`. `ScriptContext` kapselt Zugriff auf Request-Daten, Umgebungsvariablen und den `sz.execute()`-Mechanismus. |

### Logikklassen / Services — neu (`Schnittstellenzentrale.Infrastructure`)

| Klasse | Beschreibung |
|---|---|
| `EndpointScriptRunner` | Implementierung von `IEndpointScriptRunner`. Richtet den JavaScript-Interpreter ein, registriert das `sz`-Objekt mit seinen Teilobjekten (`sz.environment`, `sz.request`, `sz.response`, `sz.execute`) und führt das übergebene Skript aus. Fängt Interpreter-Fehler (Syntaxfehler, Runtime-Exception) und gibt sie als `ScriptExecutionResult.Error` zurück. |
| `ScriptContext` | Datenobjekt (oder Klasse): enthält `IActiveEnvironmentService` (für `sz.environment`-Zugriff), `ScriptRequestData` (für `sz.request`), `ScriptResponseData?` (für `sz.response`, nur im Post-Skript) und einen Callback `Func<string, Task<EndpointExecutionResult>>` für `sz.execute()`. |
| `ScriptRequestData` | Snapshot der Request-Felder vor Platzhalterauflösung (Pre-Skript) bzw. nach Platzhalterauflösung (Post-Skript — Annahme): `Url` (`string`), `Method` (`string`), `Headers` (`IDictionary<string, string>`), `Body` (`string?`). Stellt `asJson()` und `asXml()` als Methoden bereit, die das Body-Feld parsen. |
| `ScriptResponseData` | Snapshot der HTTP-Antwort: `Body` (`string?`), `Headers` (`IDictionary<string, string>`). Stellt ebenfalls `body.asJson()` und `body.asXml()` bereit. |
| `ScriptExecutionResult` | Ergebnisklasse: `bool Success`, `string? ErrorMessage`. |

### Logikklassen / Services — zu erweitern

| Klasse | Änderung |
|---|---|
| `EndpointExecutionService` | Neue Abhängigkeit: `IEndpointScriptRunner`. Geänderte Methode `ExecuteAsync`: (1) Pre-Request-Skript ausführen (falls vorhanden); bei Fehler `EndpointExecutionResult` mit `ErrorMessage` zurückgeben, HTTP-Request nicht absenden. (2) `{{...}}`-Platzhalterauflösung und Request-Aufbau wie bisher. (3) HTTP-Request absenden. (4) Post-Request-Skript ausführen (falls vorhanden); bei Fehler `ErrorMessage` an bestehendes `EndpointExecutionResult` anhängen, Ergebnis trotzdem zurückgeben. Die `sz.execute()`-Callback-Implementierung wird in `EndpointExecutionService` verankert (rekursiver Aufruf von `ExecuteAsync` für den Zielendpunkt). |

### UI-Komponenten (Blazor) — zu erweitern (`Schnittstellenzentrale`)

| Komponente | Änderung |
|---|---|
| `EndpointPage` | Zwei neue Registerkarten im Anfrage-Panel: „Pre-Request-Skript" und „Post-Request-Skript" (analog zu den bestehenden Reitern „Autorisierung", „Headers", „Query-Parameter", „Body"). Jede Registerkarte enthält ein mehrzeiliges Texteingabefeld (`<textarea>`) für JavaScript-Code. Die Skriptfelder sind im lokalen `_model`-Objekt (`PreRequestScript`, `PostRequestScript`) abgebildet und werden in `LoadModelFromParameter` und `SaveAsync` wie die übrigen Felder behandelt. |

### Tests — zu erweitern/neu zu erstellen (`Schnittstellenzentrale.Tests`)

| Artefakt | Beschreibung |
|---|---|
| `EndpointExecutionServiceTests` (Erweiterung) | Szenarien: (1) Pre-Skript setzt Variable via `sz.environment.set()`; Wert steht nach `{{...}}`-Auflösung im Request zur Verfügung. (2) Pre-Skript-Fehler blockiert HTTP-Request; `ErrorMessage` im Ergebnis. (3) Post-Skript liest `sz.response.body.asJson()`; setzt Umgebungsvariable. (4) Post-Skript-Fehler: HTTP-Ergebnis vorhanden, `ErrorMessage` angehängt. (5) `sz.execute()` löst Ausführung eines zweiten Endpunkts aus. (6) Rekursionsschutz: dritter Aufruf desselben Endpunkts im Aufrufbaum ergibt Fehler. (7) Endpunkt ohne Skript verhält sich wie bisher. |
| `EndpointScriptRunnerTests` | Unit-Tests des `EndpointScriptRunner` in Isolation: Syntaxfehler, Runtime-Exception, `sz.environment.get/set`, `sz.request.url`, `sz.response.body.asJson()`, `sz.response.body.asXml()`. |
| `EndpointPageTests` (Erweiterung) | Prüfen, dass die zwei neuen Registerkarten „Pre-Request-Skript" und „Post-Request-Skript" gerendert werden und Änderungen `MarkDirty()` auslösen. |

---

## Implementierungsansatz

### JavaScript-Interpreter

*Annahme: Der Interpreter wird als .NET-Bibliothek eingebettet (z. B. `Jint`, da bereits in ähnlichen .NET-Projekten verbreitet). Die konkrete Bibliothekswahl ist vor der Implementierung festzulegen (siehe Offene Fragen).* Der Interpreter wird pro Skriptaufruf neu instanziiert oder zurückgesetzt, um Zustandslecks zwischen Ausführungen zu vermeiden. Ein Ausführungs-Timeout begrenzt endlose Schleifen.

### Das `sz`-Objekt

`EndpointScriptRunner` registriert im JavaScript-Kontext ein Objekt `sz` mit drei Teilobjekten:

- **`sz.environment`**: Delegiert `get(name)` und `set(name, value)` an `IActiveEnvironmentService`. `set()` ruft `IActiveEnvironmentService.SetActiveEnvironment` mit einer aktualisierten Variablenliste auf, sodass nachfolgende Requests die geänderten Werte sehen.
- **`sz.request`**: Read-only-Proxy auf `ScriptRequestData`. `body.asJson()` parst `Body` via `System.Text.Json`; `body.asXml()` parst via `System.Xml.Linq.XDocument` und gibt ein DOM-ähnliches Objekt zurück, das im JavaScript-Kontext traversierbar ist.
- **`sz.response`** (nur Post-Skript): Analog zu `sz.request`, basierend auf `ScriptResponseData`.
- **`sz.execute(name)`**: Synchroner Callback (via `Task.Run(...).GetAwaiter().GetResult()` oder Interpreter-spezifische Async-Unterstützung — Annahme), der `EndpointExecutionService.ExecuteAsync` für den Endpunkt mit dem angegebenen Namen in der gleichen Anwendung aufruft.

### Rekursionsschutz für `sz.execute()`

`EndpointExecutionService` führt einen Aufrufzähler pro Endpunkt-ID in einem `Dictionary<int, int> _callDepth` (Thread-lokal oder per `ScriptContext` weitergereicht) mit. Bevor ein Endpunkt ausgeführt wird, wird geprüft, ob `_callDepth[id] >= 2`. Ist dies der Fall, wird die Ausführung mit einem Fehler abgebrochen. Nach der Ausführung wird der Zähler dekrementiert.

### Ausführungsreihenfolge in `EndpointExecutionService.ExecuteAsync`

1. `_callDepth[endpoint.Id]++`
2. `EndpointScriptRunner.ExecuteAsync(endpoint.PreRequestScript, context)` — bei Fehler: Ergebnis mit `ErrorMessage` zurückgeben, Ausführung abbrechen.
3. `BuildRequest(endpoint)` (enthält `{{...}}`-Auflösung wie bisher).
4. HTTP-Request absenden.
5. `EndpointScriptRunner.ExecuteAsync(endpoint.PostRequestScript, context)` — bei Fehler: `EndpointExecutionResult.ErrorMessage` setzen/ergänzen, Ergebnis trotzdem zurückgeben.
6. `_callDepth[endpoint.Id]--`

### `EndpointPage`-Erweiterung

Die neuen Registerkarten werden in der bestehenden `<ul class="nav nav-tabs">`-Liste ergänzt. Das Modell-Objekt `_model` wird um `PreRequestScript` und `PostRequestScript` erweitert; `LoadModelFromParameter` und `SaveAsync` werden entsprechend angepasst (analog zur Behandlung der übrigen Felder).

---

## Konfiguration

Kein zusätzlicher Konfigurationsbedarf in `appsettings.json`. Ein optionales Interpreter-Timeout (z. B. 5 Sekunden) könnte als Konstante in `EndpointScriptRunner` definiert oder in `appsettings.json` konfigurierbar gemacht werden (Annahme: Konstante genügt zunächst).

---

## Offene Fragen

1. **JavaScript-Interpreter-Bibliothek:** Welche Bibliothek soll verwendet werden? Kandidaten: `Jint` (reines .NET, kein nativer Code), `Microsoft.ClearScript` (V8-basiert, nativer Code, höhere Leistung). Die Wahl beeinflusst, wie `sz.execute()` synchron aus dem JavaScript-Kontext heraus aufgerufen werden kann und wie `asXml()` ein traversierbares DOM-Objekt zurückgibt.

2. **`sz.environment.set()` — Persistierung:** Soll `sz.environment.set()` nur den In-Memory-Zustand von `IActiveEnvironmentService` ändern (flüchtig, bis zur nächsten Seiteninitialisierung), oder soll der geänderte Wert auch in der Datenbank persistiert werden (analog zu einer manuellen Bearbeitung in `EnvironmentEditor`)? Die Anforderung schreibt vor, dass der Wert „nachfolgenden Requests sofort zur Verfügung steht" — das spricht für In-Memory; über Session-Grenzen hinaus ist nicht spezifiziert.

3. **`sz.execute()` — Synchronizität im JavaScript-Kontext:** .NET-Bibliotheken für JavaScript-Ausführung sind oft single-threaded. Wie wird `sz.execute()` synchron aus dem Skript heraus realisiert, ohne einen Deadlock zu erzeugen? Möglichkeit: `Task.Run(() => ExecuteAsync(...)).GetAwaiter().GetResult()` in einem separaten Thread-Pool-Thread — muss auf Kompatibilität mit Blazor-Server-Kontext (SynchronizationContext) geprüft werden.

4. **`sz.request` im Post-Skript:** Die Anforderung spricht davon, dass `sz.request` im Post-Skript die aufgelösten Werte enthält (URL nach Platzhalterauflösung) oder weiterhin die Rohwerte? *Annahme: Im Post-Skript enthält `sz.request.url` die vollständig aufgelöste URL — da die Platzhalterauflösung vor dem HTTP-Request stattfindet, ist dies der natürlichere Zustand.*

5. **`asXml()`-Rückgabe:** Wie wird ein „DOM-ähnliches Objekt" aus C# in den JavaScript-Kontext übergeben? Optionen: (a) JSON-Serialisierung des geparsten XML als verschachteltes Objekt; (b) Wrapper-Objekt mit Methoden wie `getElementsByTagName()`. Dies ist bibliotheksabhängig.

6. **Fehleranzeige bei Post-Skript-Fehler:** Soll der Post-Skript-Fehler in einem separaten Abschnitt im Response-Bereich angezeigt werden, oder als Erweiterung des bestehenden `alert-danger`-Bereichs? *Annahme: Ergänzung des bestehenden `ErrorMessage`-Feldes in `EndpointExecutionResult` — das bestehende UI zeigt `ErrorMessage` bereits an.*

7. **Endpunkt-Lookup für `sz.execute()`:** Der Endpunktname ist nicht zwingend eindeutig pro Anwendung. Soll nach dem ersten Treffer gesucht werden, oder soll `sz.execute()` einen Fehler liefern, wenn der Name mehrdeutig ist?

8. **Interpreter-Timeout:** Welches Timeout soll für die Skriptausführung gelten? Bei `sz.execute()` muss das Timeout des äußeren Skripts und der rekursiven Aufrufe koordiniert werden.

9. **Skript-Editor-Komfort:** Soll das Texteingabefeld ein einfaches `<textarea>` sein, oder wird ein Code-Editor (z. B. Monaco-Editor via JavaScript-Interop) gewünscht? *Annahme: Einfaches `<textarea>` gemäß Anforderungstext.*
