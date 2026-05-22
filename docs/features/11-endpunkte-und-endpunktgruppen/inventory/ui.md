# UI-Komponenten

## `ApplicationGroupTree`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor`

Zeigt `ApplicationGroup`- und `Application`-Knoten als aufklappbaren Baum. Verwendet `IApplicationApiClient` (Loopback-HTTP) für das Laden der Daten. Unterstützt Drag-and-Drop zum Verschieben von Anwendungen zwischen Gruppen.

**Parameter (EventCallbacks):**

| Parameter | Typ | Zweck |
|---|---|---|
| `OnApplicationSelected` | `EventCallback<int>` | Anwendung wurde im Baum ausgewählt |
| `OnSelectionCleared` | `EventCallback` | Auswahl wurde aufgehoben (z. B. bei StorageMode-Wechsel) |
| `OnCreateGroupRequested` | `EventCallback` | Benutzer klickt „Neue Gruppe" |
| `OnCreateApplicationRequested` | `EventCallback` | Benutzer klickt „Neue Anwendung" |
| `OnEditApplicationRequested` | `EventCallback<Application>` | Bearbeiten einer Anwendung angefordert |
| `OnRenameGroupRequested` | `EventCallback<ApplicationGroup>` | Umbenennen einer Gruppe angefordert |
| `OnDeleteGroupRequested` | `EventCallback<ApplicationGroup>` | Löschen einer Gruppe angefordert |
| `OnDeleteApplicationRequested` | `EventCallback<Application>` | Löschen einer Anwendung angefordert |

Abonniert `IStorageModeService.OnModeChanged` und lädt bei Änderung neu.

**Noch nicht vorhanden laut Anforderung:**
- `EndpointGroup`- und `Endpoint`-Knoten im Baum
- Icons je Knotentyp (Bootstrap Icons)
- `EventCallback`-Parameter für Endpunkt-/Ordner-CRUD (`OnCreateEndpointGroupRequested`, `OnCreateEndpointRequested`, `OnRenameEndpointGroupRequested`, `OnDeleteEndpointGroupRequested`, `OnDeleteEndpointRequested`, `OnEndpointSelected`)
- Resize-Handle an der Sidebar

---

## `ApplicationContextMenu`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationContextMenu.razor`

Kontextmenü für `Application`-Knoten mit Einträgen: „Bearbeiten", „Aus Gruppe entfernen" (nur wenn in einer Gruppe) und „Löschen". Systemeinträge (`IsSystem == true`) deaktivieren „Bearbeiten" und „Löschen".

**Parameter:**

| Parameter | Typ | Zweck |
|---|---|---|
| `Application` | `Application` | Die betroffene Anwendung |
| `OnEditRequested` | `EventCallback<Application>` | Bearbeiten-Aktion |
| `OnRemoveFromGroupRequested` | `EventCallback<Application>` | Aus Gruppe entfernen |
| `OnDeleteRequested` | `EventCallback<Application>` | Löschen-Aktion |

**Noch nicht vorhanden laut Anforderung:** Einträge „Ordner anlegen" (`OnCreateEndpointGroupRequested`) und „Endpunkt anlegen" (`OnCreateEndpointRequested`).

---

## `ApplicationGroupContextMenu`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupContextMenu.razor`

Kontextmenü für `ApplicationGroup`-Knoten mit Einträgen: „Umbenennen" und „Löschen". Systemeinträge deaktivieren beide Aktionen.

**Parameter:**

| Parameter | Typ | Zweck |
|---|---|---|
| `Group` | `ApplicationGroup` | Die betroffene Gruppe |
| `OnRenameRequested` | `EventCallback<ApplicationGroup>` | Umbenennen-Aktion |
| `OnDeleteRequested` | `EventCallback<ApplicationGroup>` | Löschen-Aktion |

Hinweis: Diese Komponente ist für `ApplicationGroup`, nicht für `EndpointGroup`. Laut Anforderung ist ein separates `EndpointGroupContextMenu` für `EndpointGroup`-Knoten zu erstellen.

---

## `EndpointList`
Datei: `src/Schnittstellenzentrale/Components/Shared/EndpointList.razor`

Zeigt alle Endpunkte einer Anwendung gegliedert nach `EndpointGroup`. Für jeden Endpunkt wird eine `EndpointExecutionPanel`-Instanz gerendert. Lädt Daten direkt über `IEndpointRepository`.

**Parameter:**

| Parameter | Typ | Zweck |
|---|---|---|
| `ApplicationId` | `int` | Id der Anwendung |

Laut Anforderung soll diese Komponente entfernt oder auf ein Minimum reduziert werden, da Endpunkte künftig im Navigationsbaum erscheinen.

---

## `EndpointExecutionPanel`
Datei: `src/Schnittstellenzentrale/Components/Shared/EndpointExecutionPanel.razor`

Zeigt einen einzelnen Endpunkt mit Ausführen-Schaltfläche und inline-Ergebnisanzeige. Enthält eine eingebettete `EndpointEditor`-Komponente für die Bearbeitung. Zeigt `HealthCheckDialog` bei Verbindungsfehlern.

Verwendete Services: `IEndpointExecutionService`, `IHealthCheckService`, `IEndpointRepository`.

**Parameter:**

| Parameter | Typ | Zweck |
|---|---|---|
| `Endpoint` | `Endpoint` | Der auszuführende Endpunkt |
| `OnEndpointUpdated` | `EventCallback<Endpoint>` | Wird nach Speichern ausgelöst |

Laut Anforderung wird diese Komponente durch `EndpointPage` ersetzt.

---

## `EndpointEditor`
Datei: `src/Schnittstellenzentrale/Components/Shared/EndpointEditor.razor`

Formular zur Bearbeitung eines `Endpoint`-Datensatzes. Unterstützt Anlegen (Id = 0) und Bearbeiten. Enthält Felder für Name, Methode, Pfad, Authentifizierung, Body, Header und Query-Parameter. Behandelt optimistische Nebenläufigkeit über `ConcurrencyWarningDialog`.

**Parameter:**

| Parameter | Typ | Zweck |
|---|---|---|
| `Endpoint` | `Endpoint?` | Vorhandener Endpunkt (null = Neuanlage) |
| `ApplicationId` | `int` | Anwendungs-Id (für Neuanlage) |
| `OnSaved` | `EventCallback` | Wird nach erfolgreichem Speichern ausgelöst |
| `OnCancel` | `EventCallback` | Wird beim Abbrechen ausgelöst |

Wird aktuell innerhalb von `EndpointExecutionPanel` gerendert.

---

## `ApplicationCard`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationCard.razor`

Zeigt Details einer Anwendung (Name, Description, BaseUrl, InterfaceUrl) und bindet `EndpointList` ein. Bietet Schaltflächen für Swagger-Import, OData-Import und Health-Check.

Laut Anforderung soll der `<EndpointList>`-Aufruf entfernt werden; Import- und Health-Check-Funktionalität bleibt erhalten.

---

## `Home` (Seite)
Datei: `src/Schnittstellenzentrale/Components/Pages/Home.razor`

Koordiniert die gesamte rechte Spalte: zeigt je nach Zustand `ApplicationGroupEditor`, `ApplicationEditor`, `RenameGroupDialog`, `ConfirmDeleteGroupDialog`, `ConfirmDeleteApplicationDialog` oder `ApplicationCard`. Empfängt Events vom `ApplicationGroupTree` und verwaltet alle Dialog-Zustände.

**Noch nicht vorhanden laut Anforderung:**
- Zustandsvariablen und Handler für `EndpointGroup`- und `Endpoint`-CRUD-Dialoge
- `EndpointPage`-Rendering bei Endpunktauswahl

---

## Noch nicht existierende Komponenten laut Anforderung

| Komponente | Status |
|---|---|
| `EndpointPage` | Nicht vorhanden |
| `EndpointContextMenu` | Nicht vorhanden |
| `EndpointGroupContextMenu` | Nicht vorhanden |
| `ConfirmDeleteEndpointGroupDialog` | Nicht vorhanden |
| `RenameEndpointGroupDialog` | Nicht vorhanden |
| `RequestAuthPanel` | Nicht vorhanden |
| `RequestHeadersPanel` | Nicht vorhanden |
| `RequestQueryParamsPanel` | Nicht vorhanden |
| `RequestBodyPanel` | Nicht vorhanden |
| `ResponseBodyPanel` | Nicht vorhanden |
| `ResponseHeadersPanel` | Nicht vorhanden |
