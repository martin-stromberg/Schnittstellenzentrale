# Umsetzungsplan: Aktivitätsprotokoll

## Übersicht

Es wird ein benutzerspezifischer, In-Memory-Aktivitätsprotokoll-Dienst (`IActivityLogService` / `ActivityLogService`) als Scoped-Service eingeführt, der schreibende und ausführende Aktionen des aktuellen Windows-Benutzers chronologisch aufzeichnet. Eine neue Blazor-Komponente `ActivityLogPanel` wird in `MainLayout` eingebettet und zeigt Protokolleinträge als Overlay oder angedocktes, höhenverstellbares Panel an. Protokollaufrufe werden in `EndpointExecutionService`, `EndpointScriptRunner`, `ApplicationGroupTree`, `Home` und `MainLayout` ergänzt.

---

## Programmabläufe

### Protokolleintrag hinzufügen

1. Eine aufrufende Komponente oder ein Service ruft `IActivityLogService.Log(category, message, details?)` auf.
2. `ActivityLogService.Log` erstellt intern einen neuen `ActivityLogEntry` mit `DateTime.Now`, der übergebenen `ActivityLogCategory`, `message` und optionalem `details`-String.
3. Der Eintrag wird der internen `List<ActivityLogEntry>` hinzugefügt.
4. Das `OnEntryAdded`-Event wird gefeuert; Fehler beim Feuern des Events werden still geschluckt.
5. `ActivityLogPanel` empfängt das Event und ruft `InvokeAsync(StateHasChanged)` auf, um die Anzeige zu aktualisieren.

Beteiligte Klassen/Komponenten: `ActivityLogService`, `IActivityLogService`, `ActivityLogEntry`, `ActivityLogPanel`

---

### Protokollierung eines HTTP-Requests in `EndpointExecutionService`

1. `EndpointExecutionService.ExecuteAsync` führt den HTTP-Request aus.
2. Nach erfolgreichem Request (`response.IsSuccessStatusCode == true`): Aus `EndpointExecutionResult` und dem aktuellen Umgebungskontext wird ein komprimierter Message-Text (Methode + URL + Statuscode) aufgebaut. Aus Request- und Response-Details wird ein `details`-String zusammengesetzt (je max. 10.240 Zeichen). Maskierte Variablen (`IsValueMasked == true`) werden im `details`-String durch `***` ersetzt. Dann: `IActivityLogService.Log(ActivityLogCategory.EndpointExecuted, message, details)`.
3. Bei HTTP-Fehlerantwort (`!response.IsSuccessStatusCode`): `IActivityLogService.Log(ActivityLogCategory.HttpError, message)` mit Statuscode und `response.ReasonPhrase`, ohne Body.
4. Bei unbehandelter Exception im eigenen Code: `IActivityLogService.Log(ActivityLogCategory.InternalError, message, stackTrace)` vor dem Rückgabe-`return`.

Beteiligte Klassen/Komponenten: `EndpointExecutionService`, `IActivityLogService`, `IActiveEnvironmentService`, `EnvironmentVariable`, `EndpointExecutionResult`

---

### Protokollierung in `EndpointScriptRunner`

1. Vor Ausführung des Skripts: `IActivityLogService.Log(ActivityLogCategory.ScriptExecuted, message)` mit dem Skriptkontext (Endpunktname, sofern über `ScriptContext.EndpointName` verfügbar, sonst generischer Text).
2. Während der Skriptausführung: Wenn das Skript `sz.console.write(text)` aufruft, wird das im `sz`-Objekt registrierte Lambda aufgerufen, welches `IActivityLogService.Log(ActivityLogCategory.ScriptConsoleOutput, text)` aufruft.
3. Bei `JavaScriptException` oder allgemeiner `Exception` in `ExecuteAsync`: `IActivityLogService.Log(ActivityLogCategory.InternalError, message, stackTrace)` aufrufen.

Beteiligte Klassen/Komponenten: `EndpointScriptRunner`, `IActivityLogService`, `ScriptContext`

---

### Protokollierung von Entitätsoperationen in `ApplicationGroupTree` und `Home`

1. Nach erfolgreicher Persistierung (Anlage oder Änderung) in `Home.razor` oder `ApplicationGroupTree.razor` wird die jeweilige `IActivityLogService.Log`-Methode mit der passenden `ActivityLogCategory` aufgerufen.
2. Anlageoperationen (`OnGroupSaved`, `OnApplicationSaved`, `HandleCreateEndpointRequested`, `OnCreateEndpointGroupConfirmed`) verwenden `ActivityLogCategory.EntityCreated` mit Name des Objekts.
3. Umbenennung-/Aktualisierungsoperationen (`OnGroupRenamed`, `OnEndpointGroupRenamed`) verwenden `ActivityLogCategory.EntityModified` mit Typ und Name.
4. Verschiebeoperationen (Drag & Drop in `ApplicationGroupTree`, `OnDrop`) verwenden `ActivityLogCategory.EntityMoved` mit Name sowie Quell- und Zielordner.

Beteiligte Klassen/Komponenten: `Home.razor`, `ApplicationGroupTree.razor`, `IActivityLogService`

---

### Protokollierung von Kontext-Wechseln in `MainLayout`

1. In `OnStorageModeChanged` nach `StorageModeService.SetMode(mode)`: `IActivityLogService.Log(ActivityLogCategory.ContextSwitched, message)` mit dem neuen Modus-Namen.
2. Wenn der Benutzer aktiv eine Umgebung wechselt (nicht beim initialen Restore in `RestoreEnvironmentFromLocalStorageAsync`): nach `ActiveEnvironmentService.SetActiveEnvironment(environment)` ein `IActivityLogService.Log(ActivityLogCategory.ContextSwitched, message)` mit dem Namen der neuen Umgebung.

Beteiligte Klassen/Komponenten: `MainLayout.razor`, `IActivityLogService`, `IStorageModeService`, `IActiveEnvironmentService`

---

### Panel-Anzeige und Interaktion

1. `MainLayout` rendert einen Protokoll-Symbolbutton (`bi-list-ul`) in der `.top-row`; Klick öffnet/schließt `ActivityLogPanel`.
2. `ActivityLogPanel` lädt beim ersten Render (`OnAfterRenderAsync(firstRender: true)`) Anzeigemodus (`activityLogDisplayMode`) und Panelhöhe (`activityLogPanelHeight`) aus `localStorage` via `activity-log-panel.js`.
3. Im Dock-Modus setzt `MainLayout` die CSS-Variable `--activity-log-height`, sodass `<article>` entsprechend verkleinert wird.
4. Im Overlay-Modus wird `ActivityLogPanel` mit `position: fixed` ohne Layout-Verschiebung gerendert.
5. Der Resize-Handle am oberen Rand des Panels ruft `initializePanelResize(handleElement, panelElement)` aus `activity-log-panel.js` auf; neue Höhe wird automatisch in `localStorage` gespeichert.
6. Der Symbolbutton „Protokoll leeren" ruft `IActivityLogService.Clear()` auf und löst `StateHasChanged` aus.
7. Der Symbolbutton „Overlay/Angedockt" schaltet den Anzeigemodus um und speichert ihn via `activity-log-panel.js`.

Beteiligte Klassen/Komponenten: `ActivityLogPanel`, `MainLayout.razor`, `IActivityLogService`, `activity-log-panel.js`, `LocalStorageKeys`

---

## Neue Klassen

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `ActivityLogEntry` | Datenmodellklasse | Datensatz eines Protokolleintrags mit `Timestamp`, `Category`, `Message`, `Details` |
| `ActivityLogCategory` | Enum | Kategorisierung von Protokolleinträgen |
| `IActivityLogService` | Interface | Schnittstelle für den Protokolldienst mit `Entries`, `OnEntryAdded`, `Log` und `Clear` |
| `ActivityLogService` | Klasse | Scoped-Implementierung von `IActivityLogService`; hält interne `List<ActivityLogEntry>` |
| `ActivityLogPanel` | Blazor-Komponente | Anzeige und Interaktion mit dem Aktivitätsprotokoll im UI |
| `activity-log-panel.js` | JavaScript-Modul | Panel-Resize, `localStorage`-Persistierung für Anzeigemodus und Panelhöhe |

---

## Änderungen an bestehenden Klassen

### `LocalStorageKeys` (Hilfsklasse)

- **Neue Eigenschaften:** `ActivityLogDisplayMode` (`string`, Konstante) → `"activityLogDisplayMode"` — localStorage-Schlüssel für den Anzeigemodus des Panels
- **Neue Eigenschaften:** `ActivityLogPanelHeight` (`string`, Konstante) → `"activityLogPanelHeight"` — localStorage-Schlüssel für die Panelhöhe

---

### `ScriptContext` (Datenmodellklasse)

- **Neue Eigenschaften:** `EndpointName` (`string?`) — optionaler Endpunktname für die Protokollierung in `EndpointScriptRunner`; wird von `EndpointExecutionService.BuildScriptContext` befüllt

---

### `EndpointExecutionService` (Klasse)

- **Neue Konstruktorabhängigkeit:** `IActivityLogService` per Konstruktor-Injektion
- **Geänderte Methoden:** `ExecuteAsync(Endpoint, Dictionary<int,int>)` — nach erfolgreichem HTTP-Request `Log(ActivityLogCategory.EndpointExecuted, ...)` aufrufen; bei Fehlerantwort `Log(ActivityLogCategory.HttpError, ...)`; bei unbehandelter Exception `Log(ActivityLogCategory.InternalError, ...)`
- **Geänderte Methoden:** `BuildScriptContext(Endpoint, Dictionary<int,int>, ScriptResponseData?)` — `ScriptContext.EndpointName` mit dem Endpunktnamen befüllen
- **Neue Hilfsmethode:** `BuildMaskedDetails(string, IEnumerable<EnvironmentVariable>)` (private static) — ersetzt Klartext-Werte maskierter Variablen im Detail-String durch `***`

---

### `EndpointScriptRunner` (Klasse)

- **Neue Konstruktorabhängigkeit:** `IActivityLogService` per Konstruktor-Injektion
- **Geänderte Methoden:** `ExecuteAsync(string, ScriptContext)` — vor Ausführung `Log(ActivityLogCategory.ScriptExecuted, ...)`; bei `JavaScriptException` und `Exception` zusätzlich `Log(ActivityLogCategory.InternalError, ...)`
- **Geänderte Methoden:** `RegisterSzObject(Engine, ScriptContext)` — `sz.console.write`-Lambda registrieren, das `IActivityLogService.Log(ActivityLogCategory.ScriptConsoleOutput, text)` aufruft

---

### `ApplicationGroupTree.razor` (Blazor-Komponente)

- **Neue Abhängigkeit:** `IActivityLogService` via `@inject`
- **Geänderte Methoden:** `OnDrop(int?)` — nach erfolgreichem Verschieben `Log(ActivityLogCategory.EntityMoved, ...)` aufrufen

---

### `Home.razor` (Blazor-Komponente)

- **Neue Abhängigkeit:** `IActivityLogService` via `@inject`
- **Geänderte Methoden:** `OnGroupSaved()` — nach Persistierung `Log(ActivityLogCategory.EntityCreated, ...)` aufrufen
- **Geänderte Methoden:** `OnApplicationSaved()` — nach Persistierung `Log(ActivityLogCategory.EntityCreated, ...)` aufrufen
- **Geänderte Methoden:** `OnGroupRenamed(ApplicationGroup)` — nach `UpdateGroupAsync` `Log(ActivityLogCategory.EntityModified, ...)` aufrufen
- **Geänderte Methoden:** `HandleCreateEndpointRequested(...)` — nach `AddEndpointAsync` `Log(ActivityLogCategory.EntityCreated, ...)` aufrufen
- **Geänderte Methoden:** `OnCreateEndpointGroupConfirmed(string)` — nach `AddEndpointGroupAsync` `Log(ActivityLogCategory.EntityCreated, ...)` aufrufen
- **Geänderte Methoden:** `OnEndpointGroupRenamed(EndpointGroup)` — nach `UpdateEndpointGroupAsync` `Log(ActivityLogCategory.EntityModified, ...)` aufrufen

---

### `MainLayout.razor` (Blazor-Komponente)

- **Neue Abhängigkeit:** `IActivityLogService` via `@inject`
- **Neue Eigenschaften:** `_activityLogOpen` (`bool`) — Zustand des geöffneten/geschlossenen Panels
- **Geänderte Methoden:** `OnAfterRenderAsync(bool)` — beim ersten Render Anzeigemodus und Panelhöhe aus `localStorage` laden (via `activity-log-panel.js`)
- **Geänderte Methoden:** `OnStorageModeChanged(ChangeEventArgs)` — nach `StorageModeService.SetMode(mode)` `Log(ActivityLogCategory.ContextSwitched, ...)` aufrufen
- **Neue Methoden:** `OnEnvironmentSelectedByUser(SystemEnvironment?)` — Wrapper für benutzerinitiiertes Umgebungswechsel-Ereignis mit anschließendem `Log(ActivityLogCategory.ContextSwitched, ...)`; wird von der Environment-Auswahl im UI aufgerufen (nicht vom initialen Restore)
- **Neue Event-Handler:** Protokoll-Symbolbutton-Klick → `_activityLogOpen` toggeln, `StateHasChanged`

---

## Datenbankmigrationen

Keine. Das Aktivitätsprotokoll wird ausschließlich In-Memory gehalten; es gibt keine Persistierung in der Datenbank.

---

## Validierungsregeln

Keine. Der `Log`-Aufruf akzeptiert beliebige Strings; Fehler beim Erstellen des Eintrags werden intern still abgefangen (`ActivityLogCategory.InternalError`-Platzhalter).

---

## Konfigurationsänderungen

Keine. Das Feature ist nicht serverseitig konfigurierbar. Die UI-Präferenzen (Anzeigemodus, Panelhöhe) werden ausschließlich im `localStorage` des Browsers gespeichert.

---

## Seiteneffekte und Risiken

- **`EndpointExecutionService` — Konstruktorsignatur:** Die neue `IActivityLogService`-Abhängigkeit ändert den Konstruktor. Alle Tests, die `EndpointExecutionService` direkt instanziieren (über `CreateService`), müssen einen `IActivityLogService`-Mock mitgeben.
- **`EndpointScriptRunner` — Konstruktorsignatur:** Analog zu `EndpointExecutionService` müssen Tests, die `EndpointScriptRunner` direkt instanziieren (über `CreateRunner`), einen `IActivityLogService`-Mock mitgeben.
- **`ScriptContext` — neues Feld `EndpointName`:** Tests, die `ScriptContext` direkt erstellen (via `CreateContext`), sind nicht gebrochen, da das Feld optional ist (`string?`).
- **Scoped-Service in `MainLayout`:** `MainLayout` ist selbst eine Blazor-Komponente, die im Circuit-Scope lebt. Da `IActivityLogService` als Scoped registriert ist, gibt es keine DI-Scope-Konflikte.
- **Event-Abonnement in `ActivityLogPanel`:** Wenn `ActivityLogPanel` nicht korrekt disposed wird, besteht ein Memory-Leak-Risiko. Das Dispose-Muster muss analog zu `ApplicationGroupTree` (Kündigung in `DisposeAsync`) umgesetzt werden.
- **`sz.console.write` in Fehlerfall:** Da `Log` synchron und direkt im Lambda aufgerufen wird, erscheinen `ScriptConsoleOutput`-Einträge auch dann im Protokoll, wenn das Skript danach abbricht — dies ist das erwartete Verhalten.

---

## Umsetzungsreihenfolge

1. `ActivityLogCategory` (Enum) anlegen — Voraussetzung für `ActivityLogEntry` und `IActivityLogService`
2. `ActivityLogEntry` (Datenmodellklasse) anlegen — Voraussetzung für `IActivityLogService`
3. `IActivityLogService` (Interface) anlegen — Voraussetzung für `ActivityLogService` und alle injizierenden Klassen
4. `ActivityLogService` (Implementierung) anlegen — Voraussetzung für DI-Registrierung
5. `ActivityLogService` als Scoped-Service in der DI registrieren (z. B. in `Program.cs`)
6. `LocalStorageKeys` um `ActivityLogDisplayMode` und `ActivityLogPanelHeight` erweitern
7. `ScriptContext` um `EndpointName` (`string?`) erweitern
8. `EndpointExecutionService` anpassen: `IActivityLogService` injizieren, `BuildScriptContext` erweitern, `BuildMaskedDetails` hinzufügen, Logging-Aufrufe in `ExecuteAsync` einbauen
9. `EndpointScriptRunner` anpassen: `IActivityLogService` injizieren, `sz.console.write` registrieren, Logging-Aufrufe in `ExecuteAsync` und `RegisterSzObject` einbauen
10. `Home.razor` anpassen: `IActivityLogService` injizieren, Logging-Aufrufe nach Persistierungsoperationen hinzufügen
11. `ApplicationGroupTree.razor` anpassen: `IActivityLogService` injizieren, Logging-Aufruf in `OnDrop` hinzufügen
12. `activity-log-panel.js` anlegen (Resize-Logik, `localStorage`-Zugriff)
13. `ActivityLogPanel` (Blazor-Komponente) anlegen: Rendering, Event-Abonnement, Dispose, JS-Interop
14. `MainLayout.razor` anpassen: `IActivityLogService` injizieren, Protokoll-Symbolbutton einbauen, `ActivityLogPanel` einbetten, CSS-Variable für Dock-Modus setzen, Logging-Aufrufe für Modus- und Umgebungswechsel ergänzen
15. Bestehende Tests anpassen (Mocks für `IActivityLogService` ergänzen)
16. Neue Tests anlegen

---

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `CreateActivityLogServiceMock()` | `EndpointExecutionServiceTests` | Stellt einen Mock für `IActivityLogService` bereit |
| `CreateActivityLogServiceMock()` | `EndpointScriptRunnerTests` | Stellt einen Mock für `IActivityLogService` bereit |
| `Log_ErstelltEintragMitKorrektenFeldern` | `ActivityLogServiceTests` (neu) | `ActivityLogService.Log` erstellt `ActivityLogEntry` mit korrektem Timestamp, Category, Message, Details |
| `Log_FeuertOnEntryAdded` | `ActivityLogServiceTests` (neu) | `OnEntryAdded`-Event wird nach `Log`-Aufruf gefeuert |
| `Log_EventFehler_WirdIgnoriert` | `ActivityLogServiceTests` (neu) | Exception im Event-Handler unterbricht `Log` nicht |
| `Clear_LeertEintraege` | `ActivityLogServiceTests` (neu) | `Clear` entfernt alle Einträge aus `Entries` |
| `Execute_ErfolgreichRequest_ProtokolliertEndpointExecuted` | `EndpointExecutionServiceTests` | Nach erfolgreichem Request wird `Log(EndpointExecuted)` aufgerufen |
| `Execute_HttpFehler_ProtokolliertHttpError` | `EndpointExecutionServiceTests` | Bei 4xx/5xx-Antwort wird `Log(HttpError)` aufgerufen |
| `Execute_Exception_ProtokolliertInternalError` | `EndpointExecutionServiceTests` | Bei unbehandelter Exception wird `Log(InternalError)` aufgerufen |
| `Execute_MaskiertVariablen_ImDetailString` | `EndpointExecutionServiceTests` | Werte maskierter Variablen erscheinen nicht im Klartext im Details-String |
| `ExecuteAsync_ProtokolliertScriptExecuted` | `EndpointScriptRunnerTests` | Vor Skriptausführung wird `Log(ScriptExecuted)` aufgerufen |
| `SzConsoleWrite_ProtokolliertScriptConsoleOutput` | `EndpointScriptRunnerTests` | `sz.console.write(text)` erzeugt `Log(ScriptConsoleOutput, text)` |
| `ExecuteAsync_JavaScriptException_ProtokolliertInternalError` | `EndpointScriptRunnerTests` | Bei `JavaScriptException` wird `Log(InternalError)` aufgerufen |

---

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| `CreateService(...)` (Hilfsmethode in `EndpointExecutionServiceTests`) | Muss `IActivityLogService`-Mock als Parameter oder intern als leeren Mock entgegennehmen, da der Konstruktor von `EndpointExecutionService` die neue Abhängigkeit erhält |
| `CreateRunner(...)` (Hilfsmethode in `EndpointScriptRunnerTests`) | Muss `IActivityLogService`-Mock als Parameter oder intern als leeren Mock entgegennehmen, da der Konstruktor von `EndpointScriptRunner` die neue Abhängigkeit erhält |
| Alle Tests in `EndpointExecutionServiceTests` | Durch die Anpassung von `CreateService` transitiv betroffen; keine inhaltlichen Änderungen, sofern `CreateService` den Mock intern als leeres Mock erstellt |
| Alle Tests in `EndpointScriptRunnerTests` | Durch die Anpassung von `CreateRunner` transitiv betroffen; keine inhaltlichen Änderungen, sofern `CreateRunner` den Mock intern als leeres Mock erstellt |

---

## Offene Punkte

| # | Offener Punkt | Empfohlener Vorschlag |
|---|---------------|----------------------|
| 1 | **Scoping-Strategie:** Ein Scoped-Service entspricht einem Blazor-Circuit. Zwei Browser-Tabs desselben Benutzers haben separate Protokoll-Instanzen, obwohl die Anforderung „pro Benutzer, nicht pro Tab" spezifiziert. | Scoped-Service (ein Protokoll pro Circuit/Tab) akzeptieren. Der Implementierungsaufwand für einen Singleton mit `ConcurrentDictionary<string, List<ActivityLogEntry>>` ist deutlich höher und bringt bei typischen Einzelbenutzer-Szenarien keinen Mehrwert. Anforderungstext als weichen Wunsch interpretieren. |
| 2 | **Maskierungsstrategie:** Soll der maskierte Variablenwert per String-Suche im fertigen Detail-String ersetzt werden, oder sollen Details separat ohne aufgelöste Werte aufgebaut werden? | String-Suche im fertigen Detail-String (Methode `BuildMaskedDetails`): Nach der Platzhalterauflösung werden alle Vorkommen der Klartext-Werte maskierter Variablen durch `***` ersetzt. Dies ist einfacher zu implementieren und deckt den Hauptfall ab. |
| 3 | **Endpunktname im Skript-Kontext:** `ScriptContext` enthält kein `EndpointName`-Feld; `EndpointScriptRunner` kennt den Endpunktnamen daher nicht. | `ScriptContext` um ein optionales `string? EndpointName`-Feld erweitern. `EndpointExecutionService.BuildScriptContext` befüllt dieses Feld mit dem Endpunktnamen. So kann `EndpointScriptRunner` den Namen im `ScriptExecuted`-Protokolleintrag ausgeben. |
| 4 | **`sz.console.write` — Sichtbarkeit im Fehlerfall:** Erscheinen `ScriptConsoleOutput`-Einträge auch, wenn das Skript danach abbricht? | Ja — der natürliche Ablauf ist korrekt und gewünscht. Da `Log` synchron im Lambda aufgerufen wird, erscheint der Eintrag immer, auch bei nachfolgendem Skriptabbruch. Keine besondere Behandlung erforderlich. |
| 5 | **Maximale Eintragsanzahl:** Gibt es eine Obergrenze für Protokolleinträge im Arbeitsspeicher? | Keine Obergrenze implementieren, da die Anforderung keine nennt. Die Liste wächst bis zum manuellen Leeren oder Circuit-Ende. Falls dies später ein Problem wird, kann ein konfigurierbares Maximum nachgerüstet werden. |
| 6 | **`sz.environment.set` — `ContextSwitched`-Protokolleintrag:** Soll `sz.environment.set` im Skript ebenfalls einen `ContextSwitched`-Eintrag erzeugen? | Nein — die Anforderung nennt `ContextSwitched` explizit nur für manuelle Benutzerinteraktionen (Dropdown-Wechsel in `MainLayout`). `sz.environment.set` ist eine interne Skriptoperation und erzeugt keinen `ContextSwitched`-Eintrag. |
