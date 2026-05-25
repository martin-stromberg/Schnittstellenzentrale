# Umsetzungsplan: Skripte für Endpunkte (Pre/Post-Request)

## Übersicht

Das `Endpoint`-Modell wird um zwei optionale JavaScript-Skriptfelder erweitert, die vor bzw. nach dem HTTP-Request ausgeführt werden. Zur Ausführung wird ein neues Interface `IEndpointScriptRunner` mit der Implementierungsklasse `EndpointScriptRunner` eingeführt, die einen isolierten Jint-basierten JavaScript-Kontext aufbaut und ein `sz`-API-Objekt exponiert. `EndpointExecutionService`, `EndpointPage` und das Datenbankschema werden entsprechend ergänzt.

---

## Programmabläufe

### Pre-Request-Skript-Ausführung

1. `EndpointExecutionService.ExecuteAsync` wird aufgerufen mit einem `Endpoint`, der `PreRequestScript` enthält.
2. Ein `ScriptContext` wird erstellt: `IActiveEnvironmentService` (für `sz.environment`), `ScriptRequestData` mit den Rohwerten (URL, Method, Headers, Body vor Platzhalterauflösung), einem `Func<string, Task<EndpointExecutionResult>>`-Callback für `sz.execute()` und einem neu initialisierten `Dictionary<int, int> CallDepth` (Rekursionsschutz).
3. `CallDepth[endpoint.Id]` wird inkrementiert; liegt der Wert `>= 2`, wird sofort ein Fehler-`EndpointExecutionResult` zurückgegeben.
4. `IEndpointScriptRunner.ExecuteAsync(endpoint.PreRequestScript, context)` wird aufgerufen.
5. Bei `ScriptExecutionResult.Success == false`: `EndpointExecutionResult` mit `ErrorMessage` zurückgeben, Ablauf abbrechen (kein HTTP-Request).
6. Bei Erfolg: Ablauf fortsetzen.

Beteiligte Klassen/Komponenten: `EndpointExecutionService`, `IEndpointScriptRunner`, `EndpointScriptRunner`, `ScriptContext`, `ScriptRequestData`, `ScriptExecutionResult`, `IActiveEnvironmentService`

### Platzhalterauflösung und HTTP-Request

1. `BuildRequest(endpoint)` wird aufgerufen — löst `{{...}}`-Platzhalter und `{...}`-Pfad-Platzhalter auf (wie bisher). Da das Pre-Skript ggf. Umgebungsvariablen via `sz.environment.set()` geändert hat, stehen die neuen Werte über `IActiveEnvironmentService.ActiveVariables` zur Verfügung.
2. HTTP-Request wird abgesendet.
3. `EndpointExecutionResult` wird aus der HTTP-Antwort aufgebaut.

Beteiligte Klassen/Komponenten: `EndpointExecutionService`, `IActiveEnvironmentService`

### Post-Request-Skript-Ausführung

1. `ScriptResponseData` wird aus der HTTP-Antwort befüllt: `Body`, `Headers`.
2. `ScriptContext` wird um `ScriptResponseData` ergänzt (für `sz.response`); `ScriptRequestData` enthält die aufgelösten Werte (nach Platzhalterauflösung — die tatsächlich gesendeten Werte).
3. `IEndpointScriptRunner.ExecuteAsync(endpoint.PostRequestScript, context)` wird aufgerufen.
4. Bei `ScriptExecutionResult.Success == false`: `EndpointExecutionResult.ErrorMessage` wird gesetzt oder ergänzt; das Ergebnis wird dennoch zurückgegeben (HTTP-Ergebnis bleibt erhalten).
5. `CallDepth[endpoint.Id]` wird dekrementiert.

Beteiligte Klassen/Komponenten: `EndpointExecutionService`, `IEndpointScriptRunner`, `EndpointScriptRunner`, `ScriptContext`, `ScriptResponseData`, `ScriptExecutionResult`

### `sz.execute()`-Callback mit Rekursionsschutz

1. Das Skript ruft `sz.execute(name)` auf.
2. `EndpointScriptRunner` ruft den im `ScriptContext` hinterlegten Callback `Func<string, Task<EndpointExecutionResult>>` auf. Da Jint single-threaded ist, erfolgt der Aufruf über `Task.Run(() => callback(name)).GetAwaiter().GetResult()`, um den Blazor-Server-`SynchronizationContext` nicht zu blockieren.
3. Der Callback in `EndpointExecutionService` sucht via `IEndpointRepository.GetEndpointsAsync` den Endpunkt nach Name in der gleichen Anwendung. Sind mehrere Treffer vorhanden, wird ein Fehler zurückgegeben (kein „erster Treffer"-Verhalten).
4. `ExecuteAsync` wird rekursiv für den gefundenen Endpunkt aufgerufen; dabei wird der bestehende `ScriptContext` (inkl. `CallDepth`) weitergereicht.
5. Vor jedem rekursiven Aufruf prüft `ExecuteAsync`: Wenn `CallDepth[id] >= 2`, wird die Ausführung mit einem Fehler abgebrochen.
6. Nach Rückkehr gibt der Callback das `EndpointExecutionResult` zurück.

Beteiligte Klassen/Komponenten: `EndpointExecutionService`, `IEndpointRepository`, `ScriptContext`

### `sz.environment.get/set` im Skript

1. Das Skript ruft `sz.environment.get(name)` auf.
2. `EndpointScriptRunner` liest `IActiveEnvironmentService.ActiveVariables[name]` und gibt den Wert zurück (oder `null`, falls der Schlüssel fehlt).
3. Das Skript ruft `sz.environment.set(name, value)` auf.
4. `EndpointScriptRunner` erstellt eine aktualisierte Kopie der `ActiveVariables`-Collection (bestehende Variable überschreiben oder neue hinzufügen) und ruft `IActiveEnvironmentService.SetActiveEnvironment` mit einer modifizierten `SystemEnvironment`-Kopie auf, sodass die geänderten Variablen im In-Memory-Zustand verfügbar sind. Die Änderung wird nicht in der Datenbank persistiert.

Beteiligte Klassen/Komponenten: `EndpointScriptRunner`, `IActiveEnvironmentService`, `ActiveEnvironmentService`

### Skripteingabe in der UI

1. Benutzer navigiert zur `EndpointPage` und wählt die Registerkarte „Pre-Request-Skript" oder „Post-Request-Skript".
2. Ein `<textarea>`-Element zeigt den aktuellen Skriptinhalt aus `_model.PreRequestScript` bzw. `_model.PostRequestScript`.
3. Änderungen am Textarea-Inhalt binden an das `_model`-Feld und rufen `MarkDirty()` auf.
4. Beim Speichern (`SaveAsync`) werden die Skriptfelder wie alle anderen Felder in das `Endpoint`-Objekt übernommen und persistiert.

Beteiligte Klassen/Komponenten: `EndpointPage`, `Endpoint`, `ModelUpdateExtensions`

---

## Neue Klassen

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `IEndpointScriptRunner` | Interface | Contract für die Skriptausführung: `ExecuteAsync(string script, ScriptContext context)` → `Task<ScriptExecutionResult>` |
| `EndpointScriptRunner` | Klasse | Implementierung von `IEndpointScriptRunner`; baut den JavaScript-Interpreter auf, registriert das `sz`-Objekt und führt das Skript aus |
| `ScriptContext` | Datenklasse | Kapselt alle Eingaben für eine Skriptausführung: `IActiveEnvironmentService`, `ScriptRequestData`, `ScriptResponseData?`, `Func<string, Task<EndpointExecutionResult>>`-Callback, `Dictionary<int, int> CallDepth` (Rekursionsschutz) |
| `ScriptRequestData` | Datenklasse | Snapshot der Request-Felder: `Url` (`string`), `Method` (`string`), `Headers` (`IDictionary<string, string>`), `Body` (`string?`); stellt `asJson()` (via `System.Text.Json`) und `asXml()` (XML via `XDocument`, dann JSON-Serialisierung als verschachteltes JS-Objekt) bereit |
| `ScriptResponseData` | Datenklasse | Snapshot der HTTP-Antwort: `Body` (`string?`), `Headers` (`IDictionary<string, string>`); stellt `asJson()` und `asXml()` bereit (analog zu `ScriptRequestData`) |
| `ScriptExecutionResult` | Datenklasse | Ergebnis einer Skriptausführung: `bool Success`, `string? ErrorMessage` |

---

## Änderungen an bestehenden Klassen

### `Endpoint` (Datenmodellklasse)

- **Neue Eigenschaften:** `PreRequestScript` (`string?`) — optionales JavaScript-Skript, das vor dem HTTP-Request ausgeführt wird
- **Neue Eigenschaften:** `PostRequestScript` (`string?`) — optionales JavaScript-Skript, das nach dem HTTP-Request ausgeführt wird

### `EndpointExecutionService` (Logikklasse)

- **Neue Abhängigkeiten:** `IEndpointScriptRunner` (per Konstruktor-Injektion)
- **Neue Abhängigkeiten:** `IEndpointRepository` (per Konstruktor-Injektion; für Endpunkt-Lookup bei `sz.execute()`)
- **Geänderte Methoden:** `ExecuteAsync` — Ausführungsreihenfolge wird um Pre-/Post-Skript-Aufrufe erweitert (siehe Programmabläufe); `ScriptContext` mit `CallDepth` wird erstellt und weitergereicht; bei Pre-Skript-Fehler Abbruch; bei Post-Skript-Fehler Fehlertext an `EndpointExecutionResult.ErrorMessage` anhängen. Kein `_callDepth`-Instanzfeld — der Zähler lebt ausschließlich im `ScriptContext`.

### `ModelUpdateExtensions` (Logikklasse)

- **Geänderte Methoden:** `ApplyUpdate(this Endpoint, Endpoint)` — `PreRequestScript` und `PostRequestScript` werden in die Kopier-Logik aufgenommen

### `EndpointPage` (Blazor-Komponente)

- **Neue Eigenschaften (im lokalen `_model`):** `PreRequestScript` (`string?`) und `PostRequestScript` (`string?`)
- **Geänderte Methoden:** `LoadModelFromParameter` — kopiert `PreRequestScript` und `PostRequestScript` aus dem `Endpoint`-Parameter
- **Geänderte Methoden:** `SaveAsync` — überträgt `PreRequestScript` und `PostRequestScript` in das zu speichernde `Endpoint`-Objekt
- **Neue UI-Elemente:** Zwei zusätzliche Registerkarten in der `<ul class="nav nav-tabs">`-Liste: „Pre-Request-Skript" (Tab-Key `"pre-script"`) und „Post-Request-Skript" (Tab-Key `"post-script"`), jeweils mit einem `<textarea>`-Element

### `IEndpointRepository` / `EndpointRepository` (Interface und Implementierung)

- **Neue Methoden:** `GetEndpointByNameAsync(int applicationId, string name)` — sucht einen Endpunkt nach Name innerhalb einer Anwendung, benötigt für den `sz.execute()`-Lookup

---

## Datenbankmigrationen

| Migrationsname | Betroffene Tabellen/Spalten | Beschreibung der Änderung |
|----------------|----------------------------|---------------------------|
| `AddScriptFieldsToEndpoint` | `Endpoints.PreRequestScript`, `Endpoints.PostRequestScript` | Zwei neue nullable TEXT-Spalten (`nvarchar(max)` / SQLite `TEXT`) werden zur `Endpoints`-Tabelle hinzugefügt |

---

## Validierungsregeln

Keine. Die Skriptfelder sind optional und werden ohne serverseitige Syntaxvalidierung gespeichert. Syntaxfehler werden erst zur Laufzeit durch den `EndpointScriptRunner` erkannt und als `ScriptExecutionResult.ErrorMessage` zurückgegeben.

---

## Konfigurationsänderungen

| Eintrag | Typ | Standardwert | Zweck |
|---------|-----|--------------|-------|
| `ScriptTimeoutMs` | `private const int` (in `EndpointScriptRunner`) | `5000` | Maximale Ausführungsdauer eines JavaScript-Skripts in Millisekunden |

Kein Eintrag in `appsettings.json` erforderlich.

---

## Seiteneffekte und Risiken

- **`ModelUpdateExtensions.ApplyUpdate`:** Wird die Methode nicht um `PreRequestScript`/`PostRequestScript` ergänzt, gehen Skriptänderungen beim Speichern eines Endpunkts stillschweigend verloren.
- **`sz.environment.set()` und `SetActiveEnvironment`:** Das Ersetzen der gesamten Umgebung durch eine modifizierte Kopie löst `OnActiveEnvironmentChanged` aus — alle Blazor-Komponenten, die auf dieses Event reagieren, werden neu gerendert. Dieses Verhalten ist erwünscht, stellt aber einen bestehenden Mechanismus unter Last.
- **`sz.execute()` und Rekursionsschutz:** `CallDepth` wird per `ScriptContext` weitergereicht (kein Instanzfeld), da jeder Request-Kontext seinen eigenen Aufrufzähler benötigt und parallele Ausführungen sich nicht gegenseitig beeinflussen dürfen.
- **`sz.execute()` Synchronizität (Risiko):** `Task.Run(...).GetAwaiter().GetResult()` aus Jint heraus kann im Blazor-Server-Kontext zu Deadlocks führen. Vor der Implementierung ist ein minimaler Prototyp zu erstellen und zu verifizieren. Schlägt der Prototyp fehl, muss eine alternative Synchronisierungsstrategie gewählt werden.
- **`EndpointExecutionServiceTests`:** Bestehende Tests instanziieren `EndpointExecutionService` direkt mit einem festen Satz von Abhängigkeiten. Durch die zwei neuen Pflichtabhängigkeiten (`IEndpointScriptRunner`, `IEndpointRepository`) müssen alle `CreateService`-Hilfsmethoden entsprechende Mocks erhalten.

---

## Umsetzungsreihenfolge

1. NuGet-Paket `Jint` in `Schnittstellenzentrale.Infrastructure` und `Schnittstellenzentrale.Tests` hinzufügen.
2. **Prototyp `sz.execute()` synchron:** Minimalen Jint-Prototyp erstellen, der `Task.Run(...).GetAwaiter().GetResult()` aus einem Jint-Delegate-Aufruf im Blazor-Server-Kontext ausführt — verifizieren, dass kein Deadlock entsteht. Schlägt der Prototyp fehl, vor Schritt 9 eine alternative Strategie festlegen.
3. Datenmodellklassen anlegen (`Schnittstellenzentrale.Core`): `ScriptExecutionResult`, `ScriptRequestData`, `ScriptResponseData` (abhängigkeitslos — werden von `ScriptContext` referenziert).
4. `ScriptContext`-Klasse anlegen (`Schnittstellenzentrale.Core`): referenziert `ScriptRequestData`, `ScriptResponseData`, `IActiveEnvironmentService`; enthält `Dictionary<int, int> CallDepth`.
5. `IEndpointScriptRunner`-Interface anlegen (`Schnittstellenzentrale.Core`): referenziert `ScriptContext`, `ScriptExecutionResult`.
6. `Endpoint`-Datenmodellklasse um `PreRequestScript` und `PostRequestScript` erweitern.
7. EF-Core-Migration `AddScriptFieldsToEndpoint` erstellen und anwenden.
8. `ModelUpdateExtensions.ApplyUpdate` um die zwei neuen Skriptfelder erweitern.
9. `EndpointScriptRunner`-Klasse implementieren (`Schnittstellenzentrale.Infrastructure`): Jint-Engine-Setup mit `ScriptTimeoutMs = 5000`, `sz`-Objekt-Registrierung (`sz.environment`, `sz.request`, `sz.response`, `sz.execute`), Fehlerbehandlung.
10. `EndpointExecutionService` erweitern: neue Konstruktorparameter `IEndpointScriptRunner` und `IEndpointRepository`; `ExecuteAsync` mit `ScriptContext`-Erstellung, `CallDepth`-Verwaltung, Pre-/Post-Skript-Aufrufen und `sz.execute`-Callback (inkl. Mehrdeutigkeitsprüfung).
11. DI-Registrierung: `IEndpointScriptRunner` → `EndpointScriptRunner` in der Infrastruktur-Konfiguration eintragen.
12. `EndpointPage` erweitern: `_model`-Felder, `LoadModelFromParameter`, `SaveAsync`, zwei neue Registerkarten mit `<textarea>`.
13. `EndpointScriptRunnerTests` neu erstellen.
14. `EndpointExecutionServiceTests` erweitern: `CreateService`-Hilfsmethoden um Mocks für `IEndpointScriptRunner` und `IEndpointRepository` ergänzen; neue Skript-Testfälle hinzufügen.
15. `EndpointPageTests` erweitern: Testfälle für die zwei neuen Registerkarten.

---

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `PreScript_SetsEnvironmentVariable_VariableAvailableInRequest` | `EndpointExecutionServiceTests` | Pre-Skript setzt Variable via `sz.environment.set()`; Wert steht nach `{{...}}`-Auflösung im Request zur Verfügung |
| `PreScript_Fehler_BlockiertHttpRequest_FehlerMeldungImErgebnis` | `EndpointExecutionServiceTests` | Pre-Skript-Fehler (Syntax oder Runtime) verhindert HTTP-Request; `ErrorMessage` ist gesetzt, `Success = false` |
| `PostScript_LiestResponseBody_SetzUmgebungsvariable` | `EndpointExecutionServiceTests` | Post-Skript liest `sz.response.body.asJson()` und setzt Umgebungsvariable via `sz.environment.set()` |
| `PostScript_Fehler_HttpErgebnisVorhanden_FehlerMeldungAngehaengt` | `EndpointExecutionServiceTests` | Post-Skript-Fehler: HTTP-Response ist vorhanden, `ErrorMessage` ist ergänzt |
| `SzExecute_LoesteAusfuehrungDesZweitenEndpunktsAus` | `EndpointExecutionServiceTests` | `sz.execute(name)` führt zweiten Endpunkt aus; Ergebnis wird zurückgegeben |
| `SzExecute_RekursionsschutzGreiftBeimDrittenAufruf` | `EndpointExecutionServiceTests` | Dritter Aufruf desselben Endpunkts im Aufrufbaum ergibt Fehler |
| `EndpunktOhneSkript_VerhaeltSichWieBisher` | `EndpointExecutionServiceTests` | Endpunkt ohne Skriptfelder: kein Aufruf von `IEndpointScriptRunner`, Ablauf unverändert |
| `SzExecute_MehrdeutigerName_GibtFehlerZurueck` | `EndpointExecutionServiceTests` | Mehrere Endpunkte mit gleichem Namen: `sz.execute()` gibt Fehler zurück |
| `Syntaxfehler_GibtScriptExecutionResultMitErrorMessage` | `EndpointScriptRunnerTests` | JavaScript-Syntaxfehler wird abgefangen; `Success = false`, `ErrorMessage` gefüllt |
| `RuntimeException_GibtScriptExecutionResultMitErrorMessage` | `EndpointScriptRunnerTests` | JavaScript-Runtime-Exception wird abgefangen |
| `SzEnvironmentGet_LiestVariableAusActiveVariables` | `EndpointScriptRunnerTests` | `sz.environment.get(name)` gibt korrekten Wert zurück |
| `SzEnvironmentSet_AktualisiertActiveVariables` | `EndpointScriptRunnerTests` | `sz.environment.set(name, value)` aktualisiert `IActiveEnvironmentService` |
| `SzRequestUrl_GibtKorrekteUrlZurueck` | `EndpointScriptRunnerTests` | `sz.request.url` gibt die URL aus `ScriptRequestData` zurück |
| `SzResponseBodyAsJson_ParstJsonKorrekt` | `EndpointScriptRunnerTests` | `sz.response.body.asJson()` parst JSON-Body und macht Felder im Skript zugreifbar |
| `SzResponseBodyAsXml_ParstXmlKorrekt` | `EndpointScriptRunnerTests` | `sz.response.body.asXml()` parst XML-Body und macht Struktur im Skript zugreifbar |
| `PreRequestSkript_RegistorkarteWirdGerendert` | `EndpointPageTests` | Registerkarte „Pre-Request-Skript" ist im DOM vorhanden |
| `PostRequestSkript_RegistorkarteWirdGerendert` | `EndpointPageTests` | Registerkarte „Post-Request-Skript" ist im DOM vorhanden |
| `PreRequestSkript_AenderungLoestMarkDirtyAus` | `EndpointPageTests` | Textänderung im Pre-Skript-Textarea ruft `MarkDirty()` auf |
| `PostRequestSkript_AenderungLoestMarkDirtyAus` | `EndpointPageTests` | Textänderung im Post-Skript-Textarea ruft `MarkDirty()` auf |
| `CreateScriptRunnerMock(ScriptExecutionResult)` | `EndpointExecutionServiceTests` | Hilfsmethode: erstellt einen `IEndpointScriptRunner`-Mock mit konfiguriertem Rückgabewert |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| Alle Tests in `EndpointExecutionServiceTests` (via `CreateService`/`CreateServiceCapturingUri`) | `EndpointExecutionService` erhält zwei neue Pflichtabhängigkeiten (`IEndpointScriptRunner`, `IEndpointRepository`); alle Hilfsmethoden `CreateService` und `CreateServiceCapturingUri` müssen Standard-Mocks (Skript-Ausführung liefert immer `Success = true`; Repository liefert leere Listen) intern erzeugen oder als Parameter entgegennehmen |

---

## Offene Punkte

Keine.
