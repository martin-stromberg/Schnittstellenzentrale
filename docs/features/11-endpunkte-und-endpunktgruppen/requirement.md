# Anforderung: Endpunkte und Ordner für Anwendungen

## Fachliche Zusammenfassung

Der `ApplicationGroupTree` wird von einer reinen Anwendungs-/Gruppen-Ansicht zu einem vollständigen Navigationsbaum erweitert, der auch `EndpointGroup`- und `Endpoint`-Knoten hierarchisch darstellt. Für jeden Knotentyp werden typspezifische Icons und kontextsensitive Kontextmenüs eingeführt; das bisherige Anwendungs-Kontextmenü (`ApplicationContextMenu`) wird durch ein neues Kontextmenü ersetzt, das CRUD-Aktionen für `EndpointGroup` und `Endpoint` anbietet. Die bestehende `EndpointList`- und `EndpointExecutionPanel`-Komponente wird durch eine vollständige, seitenbasierte Endpunkt-Bearbeitungsansicht abgelöst, die Name (inline editierbar), HTTP-Methode, relative URL, `AuthenticationType`, Header, Query-Parameter und Body in einem Postman-ähnlichen Layout pflegt. Die `EndpointExecutionResult`-Klasse wird um Antwort-Header, Anfragedauer und Antwortgröße erweitert. Im Team-Modus werden alle Schreiboperationen auf `Endpoint` und `EndpointGroup` über `ISignalRNotificationService` und den `EndpointHub` an verbundene Clients gemeldet. Die Sidebar-Breite wird per Resize-Handle vom Nutzer frei anpassbar gemacht.

---

## Betroffene Klassen und Komponenten

### Datenmodellklassen — zu erweitern (`Schnittstellenzentrale.Core`)

| Klasse | Änderung |
|---|---|
| `EndpointExecutionResult` | Neue Felder: `ResponseHeaders` (`IDictionary<string, string>`), `DurationMs` (`long?`), `ResponseSizeBytes` (`long?`). |
| `Endpoint` | Neues Feld `BodyMode` (`BodyMode`-Enum, z. B. `None`, `Json`, `Xml`, `PlainText`) — steuert automatisch den `Content-Type`-Header und die Formatierungsfunktion. |

### Enums — neu (`Schnittstellenzentrale.Core`)

| Enum | Werte |
|---|---|
| `BodyMode` | `None`, `Json`, `Xml`, `PlainText` |

### Datenbankschicht — zu erweitern (`Schnittstellenzentrale.Infrastructure`)

| Artefakt | Änderung |
|---|---|
| EF-Core-Migration | Neue Spalte `BodyMode` auf der `Endpoint`-Tabelle (Default `None`). |

### Interfaces — zu erweitern (`Schnittstellenzentrale.Core`)

| Interface | Änderung |
|---|---|
| `ISignalRNotificationService` | Neue Methoden: `NotifyEndpointChangedAsync(int endpointId)`, `NotifyEndpointGroupChangedAsync(int endpointGroupId)`. |

### UI-Komponenten (Blazor) — neu zu erstellen (`Schnittstellenzentrale`)

| Komponente | Beschreibung |
|---|---|
| `EndpointPage` | Vollständige Endpunkt-Bearbeitungsseite (ersetzt die bisherige inline-Darstellung in `EndpointExecutionPanel`). Enthält Kopfbereich (inline-editierbarer Name, Dirty-Indikator), Adressleiste (Methode-Dropdown, RelativePath-Feld, „Anfrage senden"-Schaltfläche), Anfrage-Panel mit Registern (Autorisierung, Headers, Query-Parameter, Body) und Ausgabe-Panel mit Registern (Body in Pretty/Raw, Headers). Strg+S löst Speichern aus; Schließen mit ungespeicherten Änderungen zeigt Bestätigungsabfrage. |
| `EndpointContextMenu` | Kontextmenü für Endpunkt-Knoten im Navigationsbaum: „Endpunkt löschen". |
| `EndpointGroupContextMenu` | Kontextmenü für `EndpointGroup`-Knoten: „Endpunkt anlegen", „Ordner umbenennen", „Ordner löschen". |
| `ConfirmDeleteEndpointGroupDialog` | Bestätigungsdialog für das Löschen eines `EndpointGroup`-Eintrags mit enthaltenen Endpunkten; weist explizit darauf hin, dass alle enthaltenen Endpunkte mitgelöscht werden. Bei Ablehnung wird nichts gelöscht. |
| `RenameEndpointGroupDialog` | Inline-Formular oder Modal zum Umbenennen eines `EndpointGroup`-Eintrags. |
| `RequestAuthPanel` | Unterkomponente des Anfrage-Panels: `AuthenticationType`-Dropdown mit kontextsensitiven Feldern (Benutzername/Passwort für `Basic`; Token-Feld für `BearerToken`; keine weiteren Felder für `None`, `Negotiate`, `NegotiateWithImpersonation`). |
| `RequestHeadersPanel` | Unterkomponente: editierbare Tabelle für Header (Name/Wert, beliebig viele Zeilen). Zeigt `Content-Type` als automatisch befüllten, ausgegrayten Sondereintrag an, der manuell überschreibbar ist. |
| `RequestQueryParamsPanel` | Unterkomponente: editierbare Tabelle für Query-Parameter (Name/Wert, beliebig viele Zeilen). |
| `RequestBodyPanel` | Unterkomponente: Freitextfeld für Body, `BodyMode`-Auswahl (JSON/XML/Plain Text/None), „Formatieren"-Schaltfläche (Indentierung für JSON/XML; Fehlermeldung bei ungültigem Input). |
| `ResponseBodyPanel` | Unterkomponente: Pretty/Raw-Umschalter mit JSON/XML-Auswahl für Pretty-Modus; zeigt formatierten oder rohen Antworttext. |
| `ResponseHeadersPanel` | Unterkomponente: schreibgeschützte Tabelle der Antwort-Header (Name/Wert). |

### UI-Komponenten (Blazor) — zu erweitern (`Schnittstellenzentrale`)

| Komponente | Änderung |
|---|---|
| `ApplicationGroupTree` | (1) Erweitert den Baum um `EndpointGroup`- und `Endpoint`-Knoten unterhalb jedes `Application`-Knotens. (2) Jeder Knotentyp erhält ein eigenes Icon (Bootstrap Icons: `bi-collection` für `ApplicationGroup`, `bi-window` für `Application`, `bi-folder` für `EndpointGroup`, `bi-lightning` o. ä. für `Endpoint`). (3) Das bisherige `ApplicationContextMenu` (Bearbeiten, Aus Gruppe entfernen, Löschen) bleibt für Application-Knoten erhalten, erhält aber zwei neue Einträge: „Ordner anlegen" und „Endpunkt anlegen". Der bisherige einzige Anwendungsmenü-Eintrag wird durch diese Erweiterung abgelöst; das Hamburger/Kebab-Menü auf Anwendungsebene entfällt damit vollständig und wird durch das erweiterte `ApplicationContextMenu` ersetzt. (4) `EndpointGroupContextMenu` für Ordner-Knoten. (5) `EndpointContextMenu` für Endpunkt-Knoten. (6) Resize-Handle am rechten Rand der Sidebar; Breite wird per CSS-Variable oder Inline-Style gespeichert (keine serverseitige Persistenz erforderlich, Annahme). |
| `ApplicationContextMenu` | Bestehende Einträge (Bearbeiten, Aus Gruppe entfernen, Löschen) bleiben; neue Einträge: „Ordner anlegen" (`OnCreateEndpointGroupRequested`) und „Endpunkt anlegen" (`OnCreateEndpointRequested`). |
| `Home` (Seite) | Koordiniert das Öffnen von `EndpointPage` (bei Auswahl eines Endpunkts im Baum) sowie die Dialog-Flows für CRUD-Operationen auf `EndpointGroup` und `Endpoint`. |
| `ISignalRNotificationService` / `SignalRNotificationService` | Implementierung der neuen Notify-Methoden (`NotifyEndpointChangedAsync`, `NotifyEndpointGroupChangedAsync`). |

### Komponenten — zu entfernen oder zu refaktorieren

| Komponente | Maßnahme |
|---|---|
| `EndpointList` | Wird nicht mehr als eigenständige Komponente in `ApplicationCard` gerendert; Endpunkte erscheinen stattdessen direkt im Navigationsbaum. Kann entfernt oder auf ein Minimum reduziert werden. |
| `EndpointExecutionPanel` | Wird durch `EndpointPage` ersetzt. Kann entfernt werden, sofern keine anderen Aufrufstellen verbleiben. |
| `ApplicationCard` | Entfernung des `<EndpointList>`-Aufrufs aus der Card; Import-Schaltflächen und Health-Check bleiben. *Annahme: `ApplicationCard` bleibt für Metadaten und Import-Aktionen erhalten, zeigt aber keine Endpunkte mehr an.* |

### Tests — neu zu erstellen (`Schnittstellenzentrale.Tests`)

| Artefakt | Beschreibung |
|---|---|
| `EndpointRepositoryIntegrationTests` (Erweiterung) | Bestehende Tests um Szenarien für `DeleteEndpointGroupAsync` mit enthaltenen Endpunkten (Kaskadenlöschung) erweitern. |
| `EndpointExecutionServiceTests` (Erweiterung) | Prüfen, dass `EndpointExecutionResult` korrekt mit `ResponseHeaders`, `DurationMs` und `ResponseSizeBytes` befüllt wird. |
| Blazor-Komponententests | `EndpointContextMenu`, `EndpointGroupContextMenu` — Analog zu bestehenden `ApplicationContextMenuTests`. |

---

## Implementierungsansatz

### Navigationsbaum-Erweiterung

`ApplicationGroupTree` lädt beim Initialisieren und bei `StorageMode`-Änderungen zusätzlich `EndpointGroup`- und `Endpoint`-Daten pro `Application` über `IEndpointRepository`. Die geladenen Daten werden in einem verschachtelten Modell (z. B. `Dictionary<int, IList<EndpointGroup>>` und `Dictionary<int, IList<Endpoint>>`) gehalten. Jede `Application` im Baum rendert bei Aufklappen ihre `EndpointGroup`-Knoten (mit darin enthaltenen `Endpoint`-Knoten) und ungrouped `Endpoint`-Knoten. Der Baum erhält neue `EventCallback`-Parameter für alle neuen CRUD-Aktionen: `OnCreateEndpointGroupRequested(Application)`, `OnCreateEndpointRequested(Application, EndpointGroup?)`, `OnRenameEndpointGroupRequested(EndpointGroup)`, `OnDeleteEndpointGroupRequested(EndpointGroup)`, `OnDeleteEndpointRequested(Endpoint)`, `OnEndpointSelected(Endpoint)`.

Da `ApplicationGroupTree` bisher `IApplicationApiClient` (HTTP-Client) verwendet und `IEndpointRepository` direkt in anderen Komponenten injiziert wird, ist zu entscheiden, ob Endpunktdaten ebenfalls über einen `IEndpointApiClient` (HTTP-Loopback) oder direkt über `IEndpointRepository` geladen werden. *Annahme: Endpunktdaten werden im Navigationsbaum direkt über `IEndpointRepository` geladen (wie in `EndpointList`), da kein API-Controller für Endpunkte existiert.*

### Endpunkt-Bearbeitungsseite (`EndpointPage`)

- Empfängt einen `Endpoint`-Parameter (vollständig geladen inkl. `Headers`, `QueryParameters`, `Application`).
- Hält einen lokalen `_isDirty`-Zustand, der bei jeder Feldänderung gesetzt wird.
- Strg+S wird über JavaScript-Interop (`IJSRuntime`) als `keydown`-Listener auf `document` registriert; beim Verlassen der Seite wird der Listener deregistriert.
- „Anfrage senden" ruft vor der Ausführung `SaveAsync()` auf; bei Speicherfehler wird die Ausführung abgebrochen.
- `Content-Type`-Automatik: Bei Änderung von `BodyMode` wird in `_headers` ein `Content-Type`-Eintrag gesetzt oder aktualisiert (`application/json`, `application/xml`, `text/plain`); ein Flag `IsAutoContentType` markiert diesen Eintrag für ausgegraugte Darstellung. Der Nutzer kann den Wert manuell überschreiben; nach manueller Änderung wird `IsAutoContentType` auf `false` gesetzt.
- Formatieren-Button: JSON via `System.Text.Json.JsonSerializer`, XML via `System.Xml.Linq.XDocument`; bei Parse-Fehler wird eine Fehlermeldung im Body-Register angezeigt.
- Bestätigungsabfrage bei ungespeicherten Änderungen: Wird über `NavigationManager.RegisterLocationChangingHandler` implementiert (Blazor Server Navigation Guard).

### `EndpointExecutionResult`-Erweiterung

`EndpointExecutionService.ExecuteAsync` misst die Anfragedauer via `Stopwatch`, liest Antwort-Header aus `HttpResponseMessage.Headers` und `HttpResponseMessage.Content.Headers` zusammen und berechnet `ResponseSizeBytes` aus der gelesenen Antwort-Body-Länge.

### SignalR-Benachrichtigungen

`SignalRNotificationService` erhält Implementierungen für `NotifyEndpointChangedAsync` und `NotifyEndpointGroupChangedAsync`. Alle Schreiboperationen (`AddEndpointAsync`, `UpdateEndpointAsync`, `DeleteEndpointAsync`, `AddEndpointGroupAsync`, `UpdateEndpointGroupAsync`, `DeleteEndpointGroupAsync`) im UI-Kontext rufen diese Methoden auf, wenn `StorageModeService.CurrentMode == StorageMode.Team`.

### Sidebar-Resize

Der Resize-Handle am rechten Rand der Sidebar wird über ein `mousedown`/`mousemove`/`mouseup`-JavaScript-Interop (oder `pointermove`) realisiert. Die Breite wird als CSS-Variable (`--sidebar-width`) gesetzt und im `localStorage` persistiert (client-seitig). *Annahme: Serverseitige Persistenz ist nicht erforderlich.*

### Ordner löschen mit enthaltenen Endpunkten

`DeleteEndpointGroupAsync` löscht die Gruppe kaskadenweise (EF Core `Cascade Delete` ist bereits konfiguriert oder muss konfiguriert werden). `ConfirmDeleteEndpointGroupDialog` prüft, ob die Gruppe Endpunkte enthält, und zeigt ggf. den Warnhinweis an. Werden keine Endpunkte enthalten, erscheint eine einfachere Bestätigung.

---

## Konfiguration

Kein zusätzlicher Konfigurationsbedarf. Das Feature nutzt ausschließlich bestehende Konfigurationsschlüssel (`DatabaseProvider`, `ConnectionStrings:Default`). Die Sidebar-Breite wird client-seitig im `localStorage` persistiert.

---

## Offene Fragen

1. **Datenladen im Navigationsbaum:** Sollen `EndpointGroup`- und `Endpoint`-Daten beim Initialisieren des Baums für alle Anwendungen auf einmal geladen werden (ein Repository-Aufruf pro Anwendung, potenziell N+1), oder nur lazy beim Aufklappen eines Anwendungsknotens? Bei vielen Anwendungen mit vielen Endpunkten kann das initiale Laden teuer werden.

2. **`ApplicationCard`-Verbleib:** Soll `ApplicationCard` nach dem Verschieben der Endpunkte in den Navigationsbaum weiterhin angezeigt werden (für Metadaten, Import, Health-Check), oder wird auch die Anwendungsdetailansicht überarbeitet? Falls ja: Welche Informationen soll die rechte Spalte anzeigen, wenn ein Endpunkt im Baum ausgewählt ist?

3. **`EndpointPage`-Routing:** Wird `EndpointPage` als Blazor-Seite mit Route (z. B. `/endpoints/{id}`) oder als dynamisch gerenderte Komponente im `col-9`-Bereich der `Home`-Seite (wie bisher `ApplicationCard`) implementiert? Eine eigene Route vereinfacht die Browser-Navigation (Vor/Zurück), erfordert aber Anpassungen an der Sidebar-Zustandsverwaltung.

4. **`Content-Type`-Flag im Datenmodell:** Soll `IsAutoContentType` als flüchtiger (nur im UI-State vorhandener) Marker implementiert werden, oder als persistiertes Feld auf `EndpointHeader`? Ohne Persistierung wird nach dem Laden eines gespeicherten Endpunkts `Content-Type` immer als manuell gesetzter Header behandelt.

5. **Kaskadenlöschen in der Datenbank:** Ist `Cascade Delete` für `EndpointGroup → Endpoint` bereits in `AppDbContext.OnModelCreating` konfiguriert, oder muss es ergänzt und migriert werden?

6. **SignalR-Granularität für Endpunkte:** Auf welcher Ebene werden Clients benachrichtigt — pro `Application` (alle Clients, die diese Anwendung abonniert haben) oder global? Dies bestimmt die Hub-Gruppenstruktur in `EndpointHub`.

7. **`BodyMode`-Migration:** Der bestehende `Body`-Wert in der Datenbank hat keinen gespeicherten `BodyMode`. Soll bei der Migration für alle vorhandenen Endpunkte ein Default (`None` oder `PlainText`) gesetzt werden?

8. **Resize-Handle — serverseitige Persistenz:** Reicht client-seitige Persistierung der Sidebar-Breite im `localStorage`, oder soll die Breite benutzerspezifisch in der Datenbank gespeichert werden?

9. **Navigation Guard bei Blazor Server:** `NavigationManager.RegisterLocationChangingHandler` ist in Blazor Server verfügbar. Soll bei ungespeicherten Änderungen zusätzlich `window.onbeforeunload` (für Browser-Refresh/Tab-Close) per JavaScript-Interop gesetzt werden?

10. **Endpunkt anlegen — Standardwerte:** Welchen initialen Namen, welche Methode und welchen Pfad soll ein neu angelegter Endpunkt erhalten (z. B. `Neuer Endpunkt`, `GET`, leerer Pfad)?
