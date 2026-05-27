# Logik – Bestandsaufnahme

## `CollapsibleSection`
Datei: `src/Schnittstellenzentrale/Components/Shared/CollapsibleSection.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OnInitialized()` | `protected override` | Setzt `_expanded` auf den Wert des Parameters `InitiallyExpanded` |
| `Toggle()` | `private` | Invertiert `_expanded` (Auf-/Zuklappen) |
| `HandleDragOver()` | `private` | Leere Methode; verhindert Browser-Standard bei Drag-over |
| `HandleDrop()` | `private async Task` | Löst `OnDrop`-EventCallback aus, wenn ein Delegate vorhanden ist |

Interner Zustand:
- `_expanded` (`bool`) — aktueller Kollaps-Zustand

Der Chevron-Button (`<button class="sz-tree-chevron-btn">`) trägt `@onclick="Toggle"`.
Der `<span class="sz-tree-node-text">` hat **keinen** Click-Handler.

---

## `ApplicationGroupTree`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OnInitializedAsync()` | `protected override async Task` | Abonniert `StorageModeService.OnModeChanged`, setzt `_owner`, lädt Daten, baut Hub-Verbindung auf |
| `OnAfterRenderAsync(bool)` | `protected override async Task` | Beim ersten Render: JS-Modul laden, Sidebar-Breite wiederherstellen, Resize-Handle initialisieren |
| `ConnectHubAsync()` | `private async Task` | Baut SignalR-Verbindung auf; abonniert `EndpointChanged` und `EndpointGroupChanged` |
| `RefreshAsync()` | `public async Task` | Lädt alle Daten neu und ruft `StateHasChanged` auf |
| `ExpandApplicationAsync(int)` | `public async Task` | Fügt eine Anwendungs-ID zu `_expandedApplicationIds` hinzu und abonniert Hub-Kanal |
| `LoadDataAsync()` | `private async Task` | Lädt Gruppen, ungroupte Anwendungen sowie Endpunkte/-gruppen für alle Anwendungen |
| `ReloadApplicationDataAsync(int)` | `private async Task` | Lädt `EndpointGroups` und `Endpoints` für eine einzelne Anwendung neu |
| `EnumerateApplications()` | `private IEnumerable<Application>` | Iteriert alle Anwendungen aus Gruppen und der „Ohne Gruppe"-Liste |
| `SelectApplication(int)` | `private async Task` | Löst `OnApplicationSelected`-EventCallback aus |
| `OnModeChanged()` | `private void` | Reagiert auf Speichermodus-Wechsel: setzt Drag-State zurück, lädt Daten neu, löscht Selektion |
| `ToggleApplicationExpanded(int)` | `private async Task` | Fügt Anwendungs-ID zu `_expandedApplicationIds` hinzu oder entfernt sie; passt Hub-Abonnement an |
| `DisposeAsync()` | `public async ValueTask` | Deabonniert Events, trennt Hub-Verbindung, disposed JS-Modul |
| `RequestCreateGroup()` | `private async Task` | Delegiert an `OnCreateGroupRequested` |
| `RequestCreateApplication()` | `private async Task` | Delegiert an `OnCreateApplicationRequested` |
| `RequestEditApplication(Application)` | `private async Task` | Delegiert an `OnEditApplicationRequested` |
| `RequestRenameGroup(ApplicationGroup)` | `private async Task` | Delegiert an `OnRenameGroupRequested` |
| `RequestDeleteGroup(ApplicationGroup)` | `private async Task` | Delegiert an `OnDeleteGroupRequested` |
| `RequestDeleteApplication(Application)` | `private async Task` | Delegiert an `OnDeleteApplicationRequested` |
| `RequestCreateEndpointGroup(Application)` | `private async Task` | Delegiert an `OnCreateEndpointGroupRequested` |
| `RequestCreateEndpointForApplication(Application)` | `private async Task` | Delegiert an `OnCreateEndpointRequested` mit `Group = null` |
| `RequestCreateEndpointForGroup(Application, EndpointGroup)` | `private async Task` | Delegiert an `OnCreateEndpointRequested` mit konkreter Gruppe |
| `RequestRenameEndpointGroup(EndpointGroup)` | `private async Task` | Delegiert an `OnRenameEndpointGroupRequested` |
| `RequestDeleteEndpointGroup(EndpointGroup)` | `private async Task` | Delegiert an `OnDeleteEndpointGroupRequested` |
| `RequestDeleteEndpoint(Endpoint)` | `private async Task` | Delegiert an `OnDeleteEndpointRequested` |
| `RequestSelectEndpoint(Endpoint)` | `private async Task` | Delegiert an `OnEndpointSelected` |
| `OnRemoveFromGroupRequested(Application)` | `private async Task` | Setzt `ApplicationGroupId = null`, aktualisiert via API, lädt Daten neu |
| `OnDragStart(Application)` | `private void` | Setzt `_draggedApplication`; bricht ab bei `IsSystem` |
| `OnDragEnter(int)` | `private void` | Setzt `_dropTargetGroupId` und zählt Enter-Events |
| `OnDragLeave()` | `private void` | Dekrementiert Enter-Zähler; löscht `_dropTargetGroupId` bei 0 |
| `OnDragEnterUngrouped()` | `private void` | Inkrementiert `_dragEnterCountUngrouped`, setzt `_dropTargetIsUngrouped` |
| `OnDragLeaveUngrouped()` | `private void` | Dekrementiert `_dragEnterCountUngrouped`; löscht Flag bei 0 |
| `OnDrop(int?)` | `private async Task` | Verschiebt `_draggedApplication` in Zielgruppe via API; loggt Aktivität |
| `ResetDragState()` | `private void` | Setzt alle Drag-&-Drop-Felder zurück |
| `RenderEndpointGroup(EndpointGroup, Dictionary<int,IEnumerable<EndpointGroup>>, IEnumerable<Endpoint>, Application)` | `private RenderFragment` | Rendert eine Endpunktordner-Zeile mit untergeordneten Gruppen und Endpunkten rekursiv |
| `RenderApplication(Application)` | `private RenderFragment` | Rendert eine Anwendungszeile mit Chevron, Namens-Button und aufgeklappten Endpunkten |

Interner Zustand:
- `_expandedApplicationIds` (`HashSet<int>`) — IDs aufgeklappter Anwendungen
- `_groups` (`IList<ApplicationGroup>?`) — geladene Gruppen
- `_ungroupedApplications` (`IList<Application>?`) — ungroupte Anwendungen
- `_endpointGroups` (`Dictionary<int, IList<EndpointGroup>>`) — Endpunktordner je Anwendung
- `_endpoints` (`Dictionary<int, IList<Endpoint>>`) — Endpunkte je Anwendung
- kein `_expandedEndpointGroupIds`-Zustand vorhanden

Abonnierte Events:
- `StorageModeService.OnModeChanged` → `OnModeChanged`
- SignalR `EndpointChanged` → `ReloadApplicationDataAsync` + `StateHasChanged`
- SignalR `EndpointGroupChanged` → `ReloadApplicationDataAsync` + `StateHasChanged`

**Render-Verhalten `RenderEndpointGroup`:**
Der `<span class="sz-tree-item-label">` trägt **keinen** Click-Handler. Der `<div class="sz-tree-children">` wird bedingungslos gerendert (kein Kollaps-Zustand für Endpunktordner).

**Render-Verhalten `RenderApplication`:**
Der Chevron-Button ruft `ToggleApplicationExpanded(app.Id)` auf. Der `<button class="sz-tree-item-btn">` ruft ausschliesslich `SelectApplication(app.Id)` auf — kein kombinierter Handler.
