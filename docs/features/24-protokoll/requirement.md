# Anforderungsübersetzung: Aktivitätsprotokoll

## Fachliche Zusammenfassung

Die Anwendung erhält einen benutzerspezifischen, rein In-Memory gehaltenen Aktivitätsprotokoll-Dienst (`IActivityLogService`), der schreibende und ausführende Aktionen des aktuell angemeldeten Windows-Benutzers (via `ICurrentUserService`) chronologisch als `ActivityLogEntry`-Liste speichert. Der Dienst wird als Scoped-Service registriert, da er pro Blazor-Circuit (= pro Browser-Session) einen eigenen Zustand halten soll. Die Protokolleinträge werden nicht in der Datenbank persistiert und leben bis zum IIS-Recycling bzw. Circuit-Ende. Eine neue Blazor-Komponente `ActivityLogPanel` wird in `MainLayout` eingebettet und kann als halbtransparentes Overlay oder als am unteren Bildschirmrand angedocktes, in der Höhe per Drag & Drop verstellbares Panel angezeigt werden. Der Anzeigemodus und die Panelhöhe werden über `localStorage` (via `IJSRuntime`) gespeichert und beim Öffnen wiederhergestellt. Das Panel abonniert ein `OnEntryAdded`-Ereignis des Dienstes und aktualisiert sich in Echtzeit. Protokollereignisse werden an den relevanten Aufrufstellen in Services und Komponenten eingesetzt, insbesondere in `EndpointExecutionService`, `EndpointScriptRunner`, `ApplicationGroupTree`, `Home` sowie beim Moduswechsel in `MainLayout`.

---

## Betroffene Klassen und Komponenten

### Neu zu erstellen

**Datenmodell / Enums**

- `ActivityLogEntry` (Core/Models) — Datensatz eines Protokolleintrags mit den Feldern:
  - `DateTime Timestamp`
  - `ActivityLogCategory Category`
  - `string Message`
  - `string? Details` (aufklappbare Zusatzdaten, z. B. Request/Response-Text, StackTrace)
- `ActivityLogCategory` (Core/Enums) — Enum mit den Werten:
  - `EntityCreated`
  - `EntityModified`
  - `EntityMoved`
  - `ContextSwitched`
  - `EndpointExecuted`
  - `ScriptExecuted`
  - `ScriptConsoleOutput`
  - `HttpError`
  - `InternalError`

**Service / Interface**

- `IActivityLogService` (Core/Interfaces) — Schnittstelle mit:
  - `IReadOnlyList<ActivityLogEntry> Entries { get; }`
  - `event Action? OnEntryAdded`
  - `void Log(ActivityLogCategory category, string message, string? details = null)`
  - `void Clear()`
- `ActivityLogService` (Infrastructure/Services) — Implementierung von `IActivityLogService`:
  - Hält intern eine `List<ActivityLogEntry>`
  - Fängt Fehler beim Erstellen des `ActivityLogEntry` still ab; legt stattdessen einen `ActivityLogCategory.InternalError`-Platzhalter an
  - Fehler beim Feuern des `OnEntryAdded`-Events werden ignoriert (stilles Schlucken)

**UI-Komponenten**

- `ActivityLogPanel` (Components/Shared) — Blazor-Komponente:
  - Rendert alle `ActivityLogEntry`-Objekte in umgekehrt chronologischer Reihenfolge (neueste oben)
  - Zeigt Zeitstempel (lokal formatiert), Kategorie-Icon + Farbe, Nachrichtentext
  - Aufklappbare Detail-Sektion für `ActivityLogEntry.Details` (sofern vorhanden)
  - Symbolbutton „Overlay" / „Angedockt" zur Umschaltung des Anzeigemodus
  - Symbolbutton „Protokoll leeren" (`IActivityLogService.Clear()` aufrufen, dann re-rendern)
  - Abonniert `IActivityLogService.OnEntryAdded` → `InvokeAsync(StateHasChanged)`
  - Speichert/liest Anzeigemodus (`activityLogDisplayMode`) und Panelhöhe (`activityLogPanelHeight`) via `localStorage` (analog zu bestehenden `LocalStorageKeys`)
  - Drag-Handle am oberen Rand des angedockten Panels; Resize-Logik per JS-Modul (analog zu `endpoint-page.js` und der bestehenden Sidebar-Resize-Implementierung)

**JS-Modul**

- `activity-log-panel.js` (wwwroot/js oder analog zu bestehendem JS-Modul-Muster) — stellt bereit:
  - `initializePanelResize(handleElement, panelElement)` — `mousedown`/`pointermove`/`pointerup`-Listener
  - `savePanelHeight(height)` / `loadPanelHeight()` — `localStorage`-Zugriff
  - `saveDisplayMode(mode)` / `loadDisplayMode()` — `localStorage`-Zugriff

**LocalStorageKeys-Erweiterung**

- `LocalStorageKeys` (Core/Helpers) — zwei neue statische Schlüssel:
  - `ActivityLogDisplayMode` → `"activityLogDisplayMode"`
  - `ActivityLogPanelHeight` → `"activityLogPanelHeight"`

### Zu erweitern

**`MainLayout.razor`**

- Neues Protokoll-Symbol (z. B. Bootstrap Icons `bi-list-ul`) in der `.top-row`-Leiste neben den bestehenden Steuerelementen; Klick öffnet/schließt `ActivityLogPanel`
- `ActivityLogPanel`-Komponente einbetten; Anzeigemodus steuert, ob das Panel als Overlay oder als angedocktes Element gerendert wird
- Im Dock-Modus: CSS-Variable `--activity-log-height` setzen, damit `<article>` (der Inhaltsbereich) entsprechend verkleinert wird, ohne Überlappung

**`EndpointExecutionService`**

- `IActivityLogService` per Konstruktor-Injektion aufnehmen
- Nach erfolgreichem HTTP-Request: `Log(ActivityLogCategory.EndpointExecuted, ...)` mit komprimiertem Message-Text (Methode + URL + Statuscode) und gekürzten Request-/Response-Details (je max. 10.240 Zeichen / ~10 KB) als `Details`
- Bei HTTP-Fehlerantwort (4xx/5xx, `!response.IsSuccessStatusCode`): `Log(ActivityLogCategory.HttpError, ...)` nur mit Statuscode und `response.ReasonPhrase` — kein Body, kein StackTrace
- Bei unbehandelter Exception im eigenen Code: `Log(ActivityLogCategory.InternalError, message, stackTrace)` vor dem Rückgabe-`return`
- Maskierte Umgebungsvariablen (`IsValueMasked == true`) dürfen im Protokoll nicht im Klartext erscheinen — beim Aufbau der Details-Strings werden maskierte Variablenwerte durch `***` ersetzt (analog zur bestehenden `ResolvePlaceholders`-Logik, erweitert um eine Masken-Map)

**`EndpointScriptRunner`**

- `IActivityLogService` per Konstruktor-Injektion aufnehmen
- Vor Ausführung des Skripts: `Log(ActivityLogCategory.ScriptExecuted, ...)` mit Skriptkontext (Endpunktname o. Ä.)
- `sz.console.write(text)` als neues Lambda im `sz`-Objekt registrieren → ruft `IActivityLogService.Log(ActivityLogCategory.ScriptConsoleOutput, text)` auf
- Bei `JavaScriptException` und allgemeiner `Exception` in `ExecuteAsync`: zusätzlich `Log(ActivityLogCategory.InternalError, message, stackTrace)` aufrufen

**`ApplicationGroupTree.razor` / `Home.razor`**

- `IActivityLogService` via `@inject` einbinden
- Bei folgenden Aktionen `Log(ActivityLogCategory.EntityCreated, ...)` aufrufen (jeweils nach erfolgreicher Persistierung):
  - Anlage einer `ApplicationGroup` (Name)
  - Anlage einer `Application` (Name, Gruppe)
  - Anlage eines `EndpointGroup`-Ordners (Name)
  - Anlage eines `Endpoint` (Name, Methode, URL)
- Bei `UpdateApplicationAsync`, `UpdateGroupAsync`, `UpdateEndpointAsync`, `UpdateEndpointGroupAsync`: `Log(ActivityLogCategory.EntityModified, ...)` (Typ + Name)
- Bei Verschieben eines Endpunkts (Drag & Drop oder Kontextmenü): `Log(ActivityLogCategory.EntityMoved, ...)` (Name, Quell- → Zielordner)

**`MainLayout.razor` (Modus- und Umgebungswechsel)**

- Nach `StorageModeService.SetMode(mode)`: `Log(ActivityLogCategory.ContextSwitched, ...)` (neuer Modus-Name)
- Nach `ActiveEnvironmentService.SetActiveEnvironment(environment)` bei Benutzerinteraktion (nicht beim initialen Restore beim Laden): `Log(ActivityLogCategory.ContextSwitched, ...)` (Name der neuen Umgebung)

---

## Implementierungsansatz

### Protokoll-Dienst

`IActivityLogService` / `ActivityLogService` werden als **Scoped**-Service in der DI registriert. Scoped entspricht einem Blazor-Circuit (einer Browser-Verbindung), was dem gewünschten Verhalten „pro Benutzer, nicht pro Tab" nahekommt. Eine weitergehende Benutzeridentifikation per `ICurrentUserService` ist in der Implementierung nicht zwingend nötig, da das Scoping bereits die Isolation sicherstellt — kann aber im `Log`-Aufruf zur Diagnose mitgeführt werden.

Das `OnEntryAdded`-Event ersetzt eine vollständige Reactive-Pipeline. `ActivityLogPanel` abonniert es in `OnInitialized` und kündigt in `Dispose`/`DisposeAsync` — analog zum Muster in `ApplicationGroupTree` (`StorageModeService.OnModeChanged`).

### Anzeigemodi und Layout-Integration

Der Overlay-Modus rendert `ActivityLogPanel` mit `position: fixed; bottom: 0; width: ...; z-index: ...` ohne Layout-Verschiebung. Der Dock-Modus ergänzt `MainLayout` um eine CSS-Variable `--activity-log-height`, die `<article>` als `padding-bottom` oder `margin-bottom` übernimmt. Die Panelhöhe und der Anzeigemodus werden beim ersten Render in `OnAfterRenderAsync(firstRender: true)` aus `localStorage` geladen — analog zur bestehenden `RestoreEnvironmentFromLocalStorageAsync`-Logik in `MainLayout`.

Der Resize-Handle am oberen Rand des Panels verwendet JS-Pointer-Events analog zu `initializeSidebarResize` in `endpoint-page.js`.

### Protokollereignisse in bestehenden Services

Die Einbindung von `IActivityLogService` erfolgt ausschließlich per Konstruktor-Injektion (Services) bzw. `@inject` (Razor-Komponenten). Fehler beim Protokollaufruf dürfen die aufrufende Methode nicht unterbrechen — der `Log`-Aufruf wird daher in bestehenden Services nicht in `try/catch` eingebettet, da `ActivityLogService.Log` selbst exceptions schluckt.

### `sz.console.write` in `EndpointScriptRunner`

Das neue Lambda wird analog zu `sz.execute` und `sz.environment.set` mit `JsValue.FromObject` im `sz`-Objekt registriert. Da `IActivityLogService.Log` synchron ist, ist kein `Task.Run`-Wrapper nötig.

### Maskierung von Variablenwerten

Beim Aufbau der Detail-Strings für `EndpointExecutionService` wird aus `IActiveEnvironmentService.ActiveEnvironment?.Variables` eine Map der maskierten Variablen aufgebaut. Im Detail-String werden Vorkommen maskierter Variablenwerte durch `***` ersetzt. Dies ist eine Annahme — die genaue Maskierungsstrategie (ob Wert-Suche im Text oder nur Platzhalter-Ebene) sollte vor Implementierung abgestimmt werden.

---

## Konfiguration

Das Feature ist nicht konfigurierbar. Einzig die UI-Präferenzen (Anzeigemodus, Panelhöhe) werden benutzerseitig im `localStorage` des Browsers gespeichert. Es gibt keine Einstellung zum De-/Aktivieren des Protokolls.

---

## Offene Fragen

1. **Scoping-Strategie:** Ein Scoped-Service in Blazor Server entspricht einem Circuit. Wenn ein Benutzer zwei Browser-Tabs öffnet, hat er zwei separate Protokoll-Instanzen. Die Anforderung sagt „pro Benutzer, nicht pro Browser-Tab". Ist dieses Verhalten akzeptabel, oder soll der Dienst als Singleton mit Benutzer-Key-basierter Speicherung (`ConcurrentDictionary<string, List<ActivityLogEntry>>`) implementiert werden?

2. **Maskierungsstrategie für Request-/Response-Details:** Wie sollen maskierte Variablen im Protokoll behandelt werden — reicht es, die aufgelösten Werte aus dem Protokoll-String zu ersetzen (String-Suche), oder sollen bereits vor der Platzhalterauflösung separate „maskierte" Details aufgebaut werden?

3. **Endpunktname im Skript-Kontext:** `EndpointScriptRunner.ExecuteAsync` erhält nur das Skript und den `ScriptContext`. Der `ScriptContext` kennt keinen Endpunktnamen. Soll `ScriptContext` um einen `string? EndpointName`-Feld erweitert werden, oder reicht ein generischer Protokolleintrag wie „Skript ausgeführt"?

4. **Protokolleintrag für sz.console.write — Sichtbarkeit in Fehlerfall:** Wenn `sz.console.write` in einem Skript aufgerufen wird, das danach abbricht, soll der Console-Eintrag trotzdem erscheinen? (Da `Log` synchron und direkt im Lambda aufgerufen wird, wäre das der natürliche Ablauf — nur zur Klarstellung.)

5. **Maximale Eintragsanzahl:** Gibt es eine Obergrenze für die Anzahl der Protokolleinträge im Arbeitsspeicher, oder wächst die Liste unbegrenzt bis zum manuellen Leeren oder Circuit-Ende?

6. **Protokollierung von Umgebungswechsel durch sz.environment.set:** Soll `sz.environment.set` im Skript ebenfalls einen `ContextSwitched`-Eintrag erzeugen, oder ist das nur für manuelle Benutzerinteraktionen (Dropdown-Wechsel) vorgesehen?
