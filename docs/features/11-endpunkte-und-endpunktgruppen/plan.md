# Umsetzungsplan: Endpunkte und Ordner für Anwendungen

## Übersicht

Der `ApplicationGroupTree` wird zu einem vollständigen Navigationsbaum erweitert, der `EndpointGroup`- und `Endpoint`-Knoten hierarchisch unterhalb jeder `Application` darstellt; alle Endpunkt- und Gruppendaten werden beim Initialisieren des Baums eager für alle Anwendungen geladen. Die bestehenden Komponenten `EndpointList`, `EndpointExecutionPanel` und `EndpointEditor` werden entfernt; ihre Funktionalität übernimmt die neue `EndpointPage`, die dynamisch im rechten Bereich von `Home` gerendert wird (analog zur bisherigen `ApplicationCard`). Zusätzlich werden `EndpointExecutionResult` um Antwort-Header und Laufzeitmetriken erweitert, SignalR-Benachrichtigungen für `Endpoint`- und `EndpointGroup`-Schreiboperationen eingeführt, die Datenbankbeziehung `EndpointGroup → Endpoint` auf Kaskadenlöschung umgestellt und die Sidebar mit einem Resize-Handle ausgestattet.

---

## Programmabläufe

### Navigationsbaum laden mit Endpunkten und Gruppen (Eager Loading)

1. `ApplicationGroupTree` ruft bei `OnInitializedAsync` und bei `IStorageModeService.OnModeChanged` zusätzlich zu den bestehenden Anwendungsdaten für jede geladene `Application` `IEndpointRepository.GetEndpointGroupsAsync(applicationId)` und `IEndpointRepository.GetEndpointsAsync(applicationId)` auf.
2. Alle Aufrufe erfolgen beim Start auf einmal (eager) — nicht erst beim Aufklappen eines Knotens.
3. Die Ergebnisse werden in zwei Wörterbüchern gehalten: `Dictionary<int, IList<EndpointGroup>>` (Schlüssel: `ApplicationId`) und `Dictionary<int, IList<Endpoint>>` (Schlüssel: `ApplicationId`).
4. Beim Rendern eines aufgeklappten `Application`-Knotens erscheinen zuerst die `EndpointGroup`-Knoten (mit ihren `Endpoint`-Kindknoten), dann die ungrouped `Endpoint`-Einträge (bei denen `EndpointGroupId == null`).
5. Jeder Knotentyp erhält ein typspezifisches Bootstrap-Icon: `bi-collection` für `ApplicationGroup`, `bi-window` für `Application`, `bi-folder` für `EndpointGroup`, `bi-lightning` für `Endpoint`.

Beteiligte Klassen/Komponenten: `ApplicationGroupTree`, `IEndpointRepository`, `IStorageModeService`.

---

### Endpunkt anlegen (über Anwendungs-Kontextmenü oder Ordner-Kontextmenü)

1. Der Benutzer klickt im `ApplicationContextMenu` auf „Endpunkt anlegen" oder im `EndpointGroupContextMenu` auf „Endpunkt anlegen".
2. Das jeweilige Kontextmenü löst `OnCreateEndpointRequested(Application, EndpointGroup?)` aus.
3. `Home` empfängt den Callback, legt einen neuen `Endpoint`-Datensatz mit Standardwerten (`Name = "Neuer Endpunkt"`, `Method = GET`, `RelativePath = ""`, `BodyMode = None`) an und speichert ihn via `IEndpointRepository.AddEndpointAsync`.
4. Bei `StorageMode.Team` wird `ISignalRNotificationService.NotifyEndpointChangedAsync(endpoint.Id, applicationId)` aufgerufen.
5. `Home` fügt den neuen Endpunkt in das lokale `_endpoints`-Dictionary von `ApplicationGroupTree` ein (via Refresh oder direkter Aktualisierung).
6. `Home` öffnet die `EndpointPage` für den neu angelegten Endpunkt.

Beteiligte Klassen/Komponenten: `ApplicationContextMenu`, `EndpointGroupContextMenu`, `Home`, `IEndpointRepository`, `ISignalRNotificationService`, `ApplicationGroupTree`, `EndpointPage`.

---

### Endpunkt bearbeiten und speichern

1. Der Benutzer klickt im Baum auf einen `Endpoint`-Knoten; `ApplicationGroupTree` löst `OnEndpointSelected(Endpoint)` aus.
2. `Home` empfängt den Callback, setzt `_selectedEndpoint` und rendert `EndpointPage` im rechten Bereich (dynamisch, kein eigenes Routing).
3. Der Benutzer ändert Felder in `EndpointPage`; der `_isDirty`-Zustand wird gesetzt.
4. Solange `_isDirty == true`, registriert `EndpointPage` einen `NavigationManager.RegisterLocationChangingHandler` (für interne Blazor-Navigation) und setzt per `IJSRuntime` einen `window.onbeforeunload`-Handler (für Browser-Refresh und Tab-Close). Beide werden bei `_isDirty = false` wieder deregistriert.
5. Speichern wird ausgelöst durch: Klick auf Speichern-Schaltfläche, Strg+S (JavaScript-Interop `keydown`-Listener auf `document`) oder implizit vor „Anfrage senden".
6. `EndpointPage` ruft `IEndpointRepository.UpdateEndpointAsync` auf; bei Concurrency-Konflikt wird `ConcurrencyWarningDialog` angezeigt.
7. Nach erfolgreichem Speichern wird `_isDirty` zurückgesetzt und beide Navigation-Guards werden deregistriert.
8. Bei `StorageMode.Team` wird `ISignalRNotificationService.NotifyEndpointChangedAsync(endpoint.Id, applicationId)` aufgerufen.

Beteiligte Klassen/Komponenten: `EndpointPage`, `Home`, `IEndpointRepository`, `ISignalRNotificationService`, `ConcurrencyWarningDialog`, `IJSRuntime`, `NavigationManager`.

---

### Anfrage senden

1. Der Benutzer klickt in `EndpointPage` auf „Anfrage senden".
2. `EndpointPage` ruft zunächst `SaveAsync()` auf; bei Speicherfehler wird die Ausführung abgebrochen.
3. `EndpointPage` ruft `IEndpointExecutionService.ExecuteAsync(endpoint)` auf.
4. `EndpointExecutionService.ExecuteAsync` delegiert an `ExecuteWithAuthAsync` oder `ExecuteImpersonatedAsync`.
5. `SendAndBuildResultAsync` startet eine `Stopwatch`, sendet die HTTP-Anfrage und stoppt die Messung nach Eingang der Antwort.
6. `BuildResult` liest `StatusCode`, `ResponseBody`, `ResponseHeaders` (aus `HttpResponseMessage.Headers` und `HttpResponseMessage.Content.Headers`), `DurationMs` (aus der Stoppuhr) und `ResponseSizeBytes` (aus der Body-Länge) aus und befüllt `EndpointExecutionResult`.
7. `EndpointPage` zeigt das Ergebnis in `ResponseBodyPanel` und `ResponseHeadersPanel` an.

Beteiligte Klassen/Komponenten: `EndpointPage`, `IEndpointExecutionService`, `EndpointExecutionService`, `EndpointExecutionResult`, `ResponseBodyPanel`, `ResponseHeadersPanel`.

---

### `BodyMode`-Automatik für `Content-Type`

1. Der Benutzer wählt in `RequestBodyPanel` einen `BodyMode`-Wert aus (`Json`, `Xml`, `PlainText`, `None`).
2. `RequestBodyPanel` löst ein Event aus; `EndpointPage` aktualisiert den `Content-Type`-Eintrag in der lokalen Header-Liste.
3. Der `Content-Type`-Wert wird anhand des `BodyMode` gesetzt (`application/json`, `application/xml`, `text/plain`); für `None` wird kein `Content-Type` automatisch gesetzt.
4. In `RequestHeadersPanel` wird der automatisch gesetzte `Content-Type`-Eintrag ausgegraut dargestellt; ein gesondertes Flag `IsAutoContentType` (nur im UI-State, nicht persistiert) markiert diesen Eintrag.
5. Ändert der Benutzer den `Content-Type`-Wert manuell, wird `IsAutoContentType` auf `false` gesetzt und die Ausgrauung entfernt.
6. `_isDirty` wird gesetzt.

Beteiligte Klassen/Komponenten: `EndpointPage`, `RequestBodyPanel`, `RequestHeadersPanel`.

---

### Body formatieren

1. Der Benutzer klickt in `RequestBodyPanel` auf „Formatieren".
2. `RequestBodyPanel` versucht, den Body-Text je nach `BodyMode` zu parsen: JSON via `System.Text.Json.JsonSerializer`, XML via `System.Xml.Linq.XDocument`.
3. Bei erfolgreichem Parsen wird der Text eingerückt und im Body-Feld angezeigt.
4. Bei Parse-Fehler wird eine Fehlermeldung im Body-Register angezeigt.
5. `None` und `PlainText` deaktivieren die Schaltfläche oder lösen keine Aktion aus.

Beteiligte Klassen/Komponenten: `RequestBodyPanel`.

---

### Ordner (`EndpointGroup`) anlegen

1. Der Benutzer klickt im `ApplicationContextMenu` auf „Ordner anlegen".
2. `ApplicationContextMenu` löst `OnCreateEndpointGroupRequested(Application)` aus.
3. `Home` empfängt den Callback, legt eine neue `EndpointGroup` mit Standardname (`"Neuer Ordner"`) an und speichert sie via `IEndpointRepository.AddEndpointGroupAsync`.
4. Bei `StorageMode.Team` wird `ISignalRNotificationService.NotifyEndpointGroupChangedAsync(group.Id, applicationId)` aufgerufen.
5. `Home` fügt die neue Gruppe in das lokale `_endpointGroups`-Dictionary von `ApplicationGroupTree` ein.

Beteiligte Klassen/Komponenten: `ApplicationContextMenu`, `Home`, `IEndpointRepository`, `ISignalRNotificationService`, `ApplicationGroupTree`.

---

### Ordner (`EndpointGroup`) umbenennen

1. Der Benutzer klickt im `EndpointGroupContextMenu` auf „Ordner umbenennen".
2. `EndpointGroupContextMenu` löst `OnRenameEndpointGroupRequested(EndpointGroup)` aus.
3. `Home` öffnet `RenameEndpointGroupDialog` mit dem aktuellen Namen.
4. Der Benutzer bestätigt; `Home` ruft `IEndpointRepository.UpdateEndpointGroupAsync` auf.
5. Bei `StorageMode.Team` wird `ISignalRNotificationService.NotifyEndpointGroupChangedAsync(group.Id, applicationId)` aufgerufen.
6. `Home` aktualisiert den lokalen Eintrag im `_endpointGroups`-Dictionary.

Beteiligte Klassen/Komponenten: `EndpointGroupContextMenu`, `RenameEndpointGroupDialog`, `Home`, `IEndpointRepository`, `ISignalRNotificationService`, `ApplicationGroupTree`.

---

### Ordner (`EndpointGroup`) löschen

1. Der Benutzer klickt im `EndpointGroupContextMenu` auf „Ordner löschen".
2. `EndpointGroupContextMenu` löst `OnDeleteEndpointGroupRequested(EndpointGroup)` aus.
3. `Home` prüft anhand des lokalen `_endpoints`-Dictionary, ob die Gruppe Endpunkte enthält.
4. Falls Endpunkte enthalten sind, öffnet `Home` den `ConfirmDeleteEndpointGroupDialog` mit einem Warnhinweis auf die kaskadierende Löschung aller enthaltenen Endpunkte. Falls keine Endpunkte enthalten sind, wird eine einfachere Bestätigung angezeigt.
5. Der Benutzer bestätigt; `Home` ruft `IEndpointRepository.DeleteEndpointGroupAsync` auf.
6. Durch das neue `DeleteBehavior.Cascade` (aus der EF-Migration `CascadeDeleteEndpointGroup`) werden alle enthaltenen Endpunkte automatisch in der Datenbank mitgelöscht.
7. `Home` entfernt Gruppe und ihre Endpunkte aus den lokalen Dictionaries.
8. Bei `StorageMode.Team` wird `ISignalRNotificationService.NotifyEndpointGroupChangedAsync(group.Id, applicationId)` aufgerufen.

Beteiligte Klassen/Komponenten: `EndpointGroupContextMenu`, `ConfirmDeleteEndpointGroupDialog`, `Home`, `IEndpointRepository`, `AppDbContext`, `ISignalRNotificationService`, `ApplicationGroupTree`.

---

### Endpunkt löschen

1. Der Benutzer klickt im `EndpointContextMenu` auf „Endpunkt löschen".
2. `EndpointContextMenu` löst `OnDeleteEndpointRequested(Endpoint)` aus.
3. `Home` zeigt eine einfache Bestätigungsabfrage (kein separater Dialog erforderlich, da kein Kaskadeneffekt).
4. Der Benutzer bestätigt; `Home` ruft `IEndpointRepository.DeleteEndpointAsync` auf.
5. Wenn der gelöschte Endpunkt gerade in `EndpointPage` angezeigt wird, setzt `Home` `_selectedEndpoint` auf `null` und schließt damit die `EndpointPage`.
6. `Home` entfernt den Endpunkt aus dem lokalen `_endpoints`-Dictionary.
7. Bei `StorageMode.Team` wird `ISignalRNotificationService.NotifyEndpointChangedAsync(endpoint.Id, applicationId)` aufgerufen.

Beteiligte Klassen/Komponenten: `EndpointContextMenu`, `Home`, `IEndpointRepository`, `ISignalRNotificationService`, `ApplicationGroupTree`, `EndpointPage`.

---

### Sidebar-Resize

1. JavaScript-Interop registriert `mousedown`- und `pointermove`-Listener auf dem Resize-Handle am rechten Rand der Sidebar.
2. Beim Ziehen wird die Breite der Sidebar als CSS-Variable (`--sidebar-width`) auf dem umschließenden Container aktualisiert.
3. Nach `mouseup` wird die aktuelle Breite im `localStorage` gespeichert (client-seitig, kein Server-Roundtrip).
4. Beim Initialisieren liest die Komponente den gespeicherten Wert aus dem `localStorage` und setzt die CSS-Variable initial.

Beteiligte Klassen/Komponenten: `ApplicationGroupTree` (Resize-Handle-Markup), `IJSRuntime`, Browser `localStorage`.

---

## Neue Klassen

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `BodyMode` | Enum | Steuert Body-Format und automatischen `Content-Type`-Header (`None`, `Json`, `Xml`, `PlainText`) |
| `EndpointPage` | Blazor-Komponente | Vollständige Endpunkt-Bearbeitungsseite mit Anfrage- und Ausgabe-Panel |
| `EndpointContextMenu` | Blazor-Komponente | Kontextmenü für `Endpoint`-Knoten im Navigationsbaum (Eintrag: Löschen) |
| `EndpointGroupContextMenu` | Blazor-Komponente | Kontextmenü für `EndpointGroup`-Knoten (Einträge: Endpunkt anlegen, Umbenennen, Löschen) |
| `ConfirmDeleteEndpointGroupDialog` | Blazor-Komponente | Bestätigungsdialog für das Löschen einer `EndpointGroup` mit Warnhinweis auf enthaltene Endpunkte |
| `RenameEndpointGroupDialog` | Blazor-Komponente | Formular/Modal zum Umbenennen einer `EndpointGroup` |
| `RequestAuthPanel` | Blazor-Komponente | Unterkomponente für `AuthenticationType`-Auswahl mit kontextsensitiven Eingabefeldern |
| `RequestHeadersPanel` | Blazor-Komponente | Unterkomponente für editierbare Header-Tabelle |
| `RequestQueryParamsPanel` | Blazor-Komponente | Unterkomponente für editierbare Query-Parameter-Tabelle |
| `RequestBodyPanel` | Blazor-Komponente | Unterkomponente für Body-Freitextfeld mit `BodyMode`-Auswahl und Formatieren-Schaltfläche |
| `ResponseBodyPanel` | Blazor-Komponente | Unterkomponente für Pretty/Raw-Anzeige des Antwort-Body |
| `ResponseHeadersPanel` | Blazor-Komponente | Unterkomponente für schreibgeschützte Tabelle der Antwort-Header |

---

## Änderungen an bestehenden Klassen

### `BodyMode` (Enum, neu — vor allen anderen Änderungen)

Wird als Voraussetzung für `Endpoint` und `RequestBodyPanel` zuerst angelegt.

---

### `EndpointExecutionResult` (Datenmodellklasse)

- **Neue Eigenschaften:**
  - `ResponseHeaders` (`IDictionary<string, string>?`) — Antwort-Header aus `HttpResponseMessage.Headers` und `Content.Headers`
  - `DurationMs` (`long?`) — Anfragedauer in Millisekunden, gemessen via `Stopwatch`
  - `ResponseSizeBytes` (`long?`) — Byte-Länge des gelesenen Antwort-Body

---

### `Endpoint` (Datenmodellklasse)

- **Neue Eigenschaften:**
  - `BodyMode` (`BodyMode`) — Steuert `Content-Type`-Automatik und Formatierungsfunktion; Default `None`

---

### `ISignalRNotificationService` (Interface)

- **Neue Methoden:**
  - `NotifyEndpointChangedAsync(int endpointId, int applicationId)` — Benachrichtigt Clients über Änderungen an einem Endpunkt; `applicationId` bestimmt die SignalR-Gruppe
  - `NotifyEndpointGroupChangedAsync(int endpointGroupId, int applicationId)` — Benachrichtigt Clients über Änderungen an einer Endpunktgruppe; `applicationId` bestimmt die SignalR-Gruppe

---

### `SignalRNotificationService<THub>` (Service)

- **Neue Methoden:**
  - `NotifyEndpointChangedAsync(int endpointId, int applicationId)` — Sendet `EndpointChanged`-Event an SignalR-Gruppe `application:{applicationId}`; die `applicationId` wird benötigt, da Abonnements über Anwendungsknoten laufen
  - `NotifyEndpointGroupChangedAsync(int endpointGroupId, int applicationId)` — Sendet `EndpointGroupChanged`-Event an SignalR-Gruppe `application:{applicationId}`

---

### `EndpointHub` (SignalR Hub)

Keine strukturellen Änderungen erforderlich. Da Endpunkt- und Gruppenbenachrichtigungen über die bestehenden `application:{applicationId}`-Gruppen verteilt werden und `ApplicationGroupTree` diese Gruppen beim Aufklappen von Anwendungsknoten bereits abonniert (`SubscribeToApplication`/`UnsubscribeFromApplication`), sind keine neuen Hub-Methoden nötig.

- **Neue Event-Handler in `ApplicationGroupTree`:** Reaktion auf `EndpointChanged`- und `EndpointGroupChanged`-Events über die bestehende SignalR-Verbindung; beim Eingang werden die betroffenen Einträge im lokalen Dictionary für die jeweilige Anwendung neu geladen.

---

### `EndpointExecutionService` (Service)

- **Geänderte Methoden:**
  - `SendAndBuildResultAsync` — Startet `Stopwatch` vor dem HTTP-Aufruf, stoppt nach Eingang der Antwort; übergibt Laufzeitwert und Header an `BuildResult`
  - `BuildResult` — Befüllt zusätzlich `ResponseHeaders` (zusammengeführt aus `HttpResponseMessage.Headers` und `HttpResponseMessage.Content.Headers`), `DurationMs` (aus der Stoppuhr) und `ResponseSizeBytes` (aus der gelesenen Body-Länge)

---

### `AppDbContext` (EF Core DbContext)

- **Geänderte Konfiguration:**
  - `EndpointGroup → Endpoint`-Beziehung: `OnDelete(DeleteBehavior.SetNull)` wird auf `OnDelete(DeleteBehavior.Cascade)` geändert, damit das Löschen einer Gruppe alle enthaltenen Endpunkte kaskadierend mitlöscht

---

### `ApplicationContextMenu` (Blazor-Komponente)

- **Neue Parameter:**
  - `OnCreateEndpointGroupRequested` (`EventCallback<Application>`) — Wird ausgelöst, wenn der Benutzer „Ordner anlegen" wählt
  - `OnCreateEndpointRequested` (`EventCallback<Application>`) — Wird ausgelöst, wenn der Benutzer „Endpunkt anlegen" wählt
- **Neue Menüeinträge:** „Ordner anlegen" und „Endpunkt anlegen" im Kontextmenü-Markup

---

### `ApplicationGroupTree` (Blazor-Komponente)

- **Neue Eigenschaften (intern):**
  - `_endpointGroups` (`Dictionary<int, IList<EndpointGroup>>`) — Geladene Gruppen pro Anwendung
  - `_endpoints` (`Dictionary<int, IList<Endpoint>>`) — Geladene Endpunkte pro Anwendung
  - `_expandedApplicationIds` (`HashSet<int>`) — Welche Anwendungsknoten aufgeklappt sind (steuert SignalR-Abonnements)
- **Neue Parameter (EventCallbacks):**
  - `OnCreateEndpointGroupRequested` (`EventCallback<Application>`)
  - `OnCreateEndpointRequested` (`EventCallback<(Application, EndpointGroup?)>`)
  - `OnRenameEndpointGroupRequested` (`EventCallback<EndpointGroup>`)
  - `OnDeleteEndpointGroupRequested` (`EventCallback<EndpointGroup>`)
  - `OnDeleteEndpointRequested` (`EventCallback<Endpoint>`)
  - `OnEndpointSelected` (`EventCallback<Endpoint>`)
- **Geänderte Methoden:**
  - `OnInitializedAsync` / Reload-Methode — Lädt beim Start eager alle `EndpointGroup`- und `Endpoint`-Daten für jede Anwendung via `IEndpointRepository`
  - Aufklapp-/Zuklapp-Handler — Bei Aufklappen eines `Application`-Knotens: Eintrag zu `_expandedApplicationIds` hinzufügen und `EndpointHub.SubscribeToApplication(applicationId)` aufrufen. Bei Zuklappen: Entfernen aus `_expandedApplicationIds` und `EndpointHub.UnsubscribeFromApplication(applicationId)` aufrufen.
- **Neue Event-Handler:** Reaktion auf `EndpointChanged`- und `EndpointGroupChanged`-SignalR-Events über die bestehende Hub-Verbindung; beim Eingang werden die Einträge für die betroffene Anwendung über `IEndpointRepository` neu geladen.
- **Neue Injektion:** `IEndpointRepository`
- **Neue Render-Abschnitte:** `EndpointGroup`-Knoten und `Endpoint`-Knoten mit Kontextmenüs und Icons; Resize-Handle am rechten Rand

---

### `Home` (Seite)

- **Neue Zustandsvariablen:**
  - `_selectedEndpoint` (`Endpoint?`) — Aktuell in `EndpointPage` angezeigter Endpunkt
- **Neue Methoden (Event-Handler):**
  - `HandleEndpointSelected(Endpoint endpoint)` — Öffnet `EndpointPage`
  - `HandleCreateEndpointGroupRequested(Application application)` — Legt neue Gruppe an und aktualisiert Baum
  - `HandleCreateEndpointRequested((Application, EndpointGroup?))` — Legt neuen Endpunkt an und öffnet `EndpointPage`
  - `HandleRenameEndpointGroupRequested(EndpointGroup group)` — Öffnet `RenameEndpointGroupDialog`
  - `HandleDeleteEndpointGroupRequested(EndpointGroup group)` — Öffnet `ConfirmDeleteEndpointGroupDialog`
  - `HandleDeleteEndpointRequested(Endpoint endpoint)` — Löscht Endpunkt nach Bestätigung
- **Rendering-Änderung:** Wenn `_selectedEndpoint != null`, wird `EndpointPage` anstelle von `ApplicationCard` im rechten Bereich gerendert

---

### `ApplicationCard` (Blazor-Komponente)

- **Geänderte Methoden:**
  - Entfernung des `<EndpointList>`-Aufrufs aus dem Markup; Import- und Health-Check-Schaltflächen bleiben erhalten

---

### `EndpointList` (Blazor-Komponente)

- **Maßnahme:** Komponente wird entfernt. Vor der Entfernung sicherstellen, dass keine anderen Aufrufstellen außer `ApplicationCard` bestehen.

---

### `EndpointExecutionPanel` (Blazor-Komponente)

- **Maßnahme:** Komponente wird entfernt. Vor der Entfernung sicherstellen, dass keine anderen Aufrufstellen außer `EndpointList` bestehen.

---

### `EndpointEditor` (Blazor-Komponente)

- **Maßnahme:** Komponente wird entfernt. Die Formularlogik wird vollständig in die neuen Panel-Unterkomponenten (`RequestAuthPanel`, `RequestHeadersPanel`, `RequestQueryParamsPanel`, `RequestBodyPanel`) verlagert. `ConcurrencyWarningDialog` wird direkt in `EndpointPage` eingebunden.

---

## Datenbankmigrationen

| Migrationsname | Betroffene Tabellen/Spalten | Beschreibung der Änderung |
|----------------|----------------------------|---------------------------|
| `AddBodyModeToEndpoint` | `Endpoints.BodyMode` | Neue `integer`-Spalte `BodyMode` mit Default `0` (`None`) für alle vorhandenen Zeilen |
| `CascadeDeleteEndpointGroup` | FK `Endpoints.EndpointGroupId` | Ändert `ON DELETE SET NULL` auf `ON DELETE CASCADE` für die Fremdschlüsselbeziehung `EndpointGroup → Endpoint` |

---

## Validierungsregeln

| Feld / Objekt | Regel | Fehlerfall |
|---------------|-------|------------|
| `Endpoint.Name` | Pflichtfeld, nicht leer oder nur Whitespace | Speichern wird verhindert; Hinweistext im Kopfbereich von `EndpointPage` |
| `Endpoint.RelativePath` | Kein Pflichtfeld; leerer Pfad ist erlaubt (Standardwert bei Neuanlage) | — |
| `EndpointGroup.Name` | Pflichtfeld, nicht leer oder nur Whitespace | Speichern in `RenameEndpointGroupDialog` wird verhindert |
| `RequestBodyPanel` Formatieren | Body-Text muss dem gewählten `BodyMode` entsprechen (valides JSON / valides XML) | Fehlermeldung im Body-Register; Speichern wird nicht blockiert |

---

## Konfigurationsänderungen

Keine.

---

## Seiteneffekte und Risiken

- **`AppDbContext`-Migrationspfad:** Die Änderung von `SetNull` auf `Cascade` für `EndpointGroup → Endpoint` erfordert zwei Migrationen (SQLite und SQL Server). Bei SQLite muss die Tabelle neu erstellt werden, da SQLite keine `ALTER CONSTRAINT`-Unterstützung hat — dies geschieht automatisch durch EF Core, birgt aber Risiko bei großen Datenbeständen.
- **N+1-Datenladen im Navigationsbaum:** Das eager Laden aller Endpunkt- und Gruppendaten beim Start erzeugt N Anwendungen × 2 Repository-Aufrufe. Bei kleinen bis mittelgroßen Installationen unkritisch; bei sehr vielen Anwendungen kann die Startlatenz spürbar werden.
- **`EndpointEditor`-Entfernung:** `EndpointEditor` wird zusammen mit `EndpointExecutionPanel` entfernt. `ConcurrencyWarningDialog` (bisher in `EndpointExecutionPanel` genutzt) wird direkt in `EndpointPage` eingebunden. Vor der Entfernung müssen alle Aufrufstellen verifiziert sein.
- **`window.onbeforeunload`-Handler:** Der JS-Handler muss bei Speichern, bei Wechsel zu einem anderen Endpunkt und beim Dispose der `EndpointPage`-Komponente zuverlässig deregistriert werden, um unerwünschte Browser-Dialoge zu vermeiden.
- **SignalR-Abonnement-Verwaltung:** Das aufklappbasierte Abonnement erfordert ein zuverlässiges Tracking von `_expandedApplicationIds`. Ein Fehler im Tracking (z. B. durch Dispose ohne Zuklapp-Event) kann zu verwaisten Abonnements führen. `IAsyncDisposable` in `ApplicationGroupTree` muss alle offenen Abonnements kündigen.
- **`EndpointList`- und `EndpointExecutionPanel`-Entfernung:** Beide Komponenten müssen sorgfältig auf alle Aufrufstellen geprüft werden, bevor sie entfernt werden.

---

## Umsetzungsreihenfolge

1. Enum `BodyMode` anlegen (`Schnittstellenzentrale.Core`)
2. `EndpointExecutionResult` um `ResponseHeaders`, `DurationMs`, `ResponseSizeBytes` erweitern (`Schnittstellenzentrale.Core`)
3. `Endpoint` um `BodyMode` erweitern (`Schnittstellenzentrale.Core`)
4. `ISignalRNotificationService` um `NotifyEndpointChangedAsync(int endpointId, int applicationId)` und `NotifyEndpointGroupChangedAsync(int endpointGroupId, int applicationId)` erweitern (`Schnittstellenzentrale.Core`)
5. `SignalRNotificationService` — neue Methoden implementieren; Events an `application:{applicationId}`-Gruppen senden (`Schnittstellenzentrale.Infrastructure`)
6. `AppDbContext` — `EndpointGroup → Endpoint`-Beziehung von `SetNull` auf `Cascade` umstellen (`Schnittstellenzentrale.Infrastructure`)
7. EF-Migration `AddBodyModeToEndpoint` anlegen und anwenden (SQLite und SQL Server)
8. EF-Migration `CascadeDeleteEndpointGroup` anlegen und anwenden (SQLite und SQL Server)
9. `EndpointExecutionService` — `SendAndBuildResultAsync` und `BuildResult` um Stopwatch, `ResponseHeaders` und `ResponseSizeBytes` erweitern (`Schnittstellenzentrale.Infrastructure`)
10. Neue Blazor-Unterkomponenten anlegen: `RequestAuthPanel`, `RequestHeadersPanel`, `RequestQueryParamsPanel`, `RequestBodyPanel`, `ResponseBodyPanel`, `ResponseHeadersPanel` (`Schnittstellenzentrale`)
11. `RenameEndpointGroupDialog` und `ConfirmDeleteEndpointGroupDialog` anlegen (`Schnittstellenzentrale`)
12. `EndpointContextMenu` und `EndpointGroupContextMenu` anlegen (`Schnittstellenzentrale`)
13. `ApplicationContextMenu` um „Ordner anlegen" und „Endpunkt anlegen" erweitern (`Schnittstellenzentrale`)
14. `EndpointPage` anlegen — verwendet Unterkomponenten aus Schritt 10, `IEndpointRepository`, `IEndpointExecutionService`, `IJSRuntime` (Strg+S-Handler, `window.onbeforeunload`), `NavigationManager` (Navigation Guard), `ConcurrencyWarningDialog` (`Schnittstellenzentrale`)
15. `ApplicationGroupTree` erweitern — eager Datenladen, neue EventCallbacks, Knoten-Rendering für `EndpointGroup` und `Endpoint`, Icons, aufklappbasierte SignalR-Abonnementverwaltung, Event-Handler für `EndpointChanged`/`EndpointGroupChanged`, Resize-Handle (`Schnittstellenzentrale`)
16. `Home` erweitern — neue Event-Handler, `EndpointPage`-Rendering, Dialog-Flows, Sidebar-Breite aus `localStorage` initialisieren (`Schnittstellenzentrale`)
17. `ApplicationCard` — `<EndpointList>`-Aufruf entfernen (`Schnittstellenzentrale`)
18. `EndpointList` entfernen (nach Verifikation aller Aufrufstellen) (`Schnittstellenzentrale`)
19. `EndpointExecutionPanel` entfernen (nach Verifikation aller Aufrufstellen) (`Schnittstellenzentrale`)
20. `EndpointEditor` entfernen (nach Verifikation aller Aufrufstellen) (`Schnittstellenzentrale`)
21. Tests anpassen und neue Tests anlegen (`Schnittstellenzentrale.Tests`)

---

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `DeleteEndpointGroup_WithEndpoints_CascadesDelete` | `EndpointRepositoryIntegrationTests` | Löschen einer Gruppe löscht kaskadierend alle enthaltenen Endpunkte |
| `DeleteEndpointGroup_WithoutEndpoints_DeletesGroup` | `EndpointRepositoryIntegrationTests` | Löschen einer leeren Gruppe löscht nur die Gruppe |
| `Execute_SetsResponseHeaders` | `EndpointExecutionServiceTests` | `ResponseHeaders` ist nach Ausführung befüllt |
| `Execute_SetsDurationMs` | `EndpointExecutionServiceTests` | `DurationMs` ist nach Ausführung größer als 0 |
| `Execute_SetsResponseSizeBytes` | `EndpointExecutionServiceTests` | `ResponseSizeBytes` entspricht der Byte-Länge des `ResponseBody` |
| `LöschenEintrag_LöstCallbackAus` | `EndpointContextMenuTests` | Klick auf „Endpunkt löschen" löst `OnDeleteRequested` aus und schließt das Menü |
| `EndpunktAnlegen_LöstCallbackAus` | `EndpointGroupContextMenuTests` | Klick auf „Endpunkt anlegen" löst `OnCreateEndpointRequested` aus |
| `OrdnerUmbenennen_LöstCallbackAus` | `EndpointGroupContextMenuTests` | Klick auf „Ordner umbenennen" löst `OnRenameEndpointGroupRequested` aus |
| `OrdnerLöschen_LöstCallbackAus` | `EndpointGroupContextMenuTests` | Klick auf „Ordner löschen" löst `OnDeleteEndpointGroupRequested` aus |
| `ExecuteWithTwoEndpointContextsAsync` (Hilfsmethode) | `TestHelpers` | Wie `ExecuteWithTwoContextsAsync`, aber für `EndpointRepository`; wird für Concurrency-Tests benötigt |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| `EndpointRepositoryIntegrationTests` | `DeleteEndpointGroupAsync`-Verhalten ändert sich: statt `SetNull` gilt jetzt `Cascade`; bestehende Tests müssen prüfen, ob sie vom neuen Verhalten betroffen sind |
| `EndpointExecutionServiceTests` | `BuildResult` gibt jetzt `EndpointExecutionResult` mit neuen Feldern zurück; Assertions auf Gleichheit oder Mock-Rückgabewerte müssen ggf. angepasst werden |

---

## Offene Punkte

Keine. Alle offenen Fragen aus der Anforderung wurden beantwortet und im Plan berücksichtigt.
