# Logik / UI-Komponenten

## `ApplicationGroupTree` (`src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor`)

Zentraler Baum in der Workspaces-Sidebar. Expand-Zustände sind **komponenteninterne `HashSet<int>`-Felder ohne Persistenz** (kein localStorage, kein Server-State) — nach `Page.ReloadAsync()` ist alles zugeklappt.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OnInitializedAsync` | protected override | Lädt Gruppen/Anwendungen via `LoadDataAsync`, startet SignalR-Hub (`ConnectHubAsync`, fire-and-forget `_ =`) |
| `OnAfterRenderAsync` | protected override | Lädt JS-Modul `endpoint-page.js`, wendet gespeicherte Sidebar-Breite an (nur diese wird in localStorage persistiert) |
| `ConnectHubAsync` | private | `HubConnection` mit `WithAutomaticReconnect`; registriert `TreeChanged`/`EndpointChanged`/`EndpointGroupChanged`-Handler; `SubscribeToWorkspace` nach `StartAsync`. Fehler → nur LogDebug, `_hubConnection = null` |
| `LoadDataAsync` | private | `GetGroupsAsync` + `GetUngroupedApplicationsAsync`; leert `_endpointGroups`/`_endpoints` und lädt Daten aller **bereits expandierten** Anwendungen nach |
| `ReloadApplicationDataAsync` | private | `GetEndpointGroupsAsync` + `GetEndpointsAsync` einer Anwendung; Fehler werden geschluckt (Fehlermeldung in `_errorMessage`) |
| `ExpandGroupAsync(int)` | public | Fügt `groupId` zu `_expandedGroupIds` hinzu; aufgerufen von `WorkspacesSidebar.ExpandGroup` |
| `ExpandApplicationAsync(int)` | public | Expand + `ReloadApplicationDataAsync` + `SubscribeToApplication` (nur wenn Hub `Connected`) |
| `ToggleApplicationExpanded(int)` | private | Umschalten von `_expandedApplicationIds`; beim Expand REST-Nachladen, beim Zu-/Aufklappen Hub-Un-/Subscribe (nur `Connected`-State) |
| `SelectAndExpandApplication(int)` | private | `ToggleApplicationExpanded` + `SelectApplication` — **der vom Test angeklickte Handler** (`sz-tree-item-btn` der Anwendungszeile, Zeile 504) |
| `ToggleEndpointGroupExpanded(int)` | private | Reines Umschalten von `_expandedEndpointGroupIds` (Chevron-Klick, Zeile 462) |
| `RequestSelectEndpointGroup` | private | `ToggleEndpointGroupExpanded` + `OnEndpointGroupSelected` (Label-Klick, Zeile 466) |
| `RenderEndpointGroup` | private RenderFragment | Ordnerzeile: Chevron-Button **immer gerendert** (Zeilen 462–464, `chevron-down` bei expandiert, sonst `chevron-right`), Kinder nur bei expandierter ID (Zeile 472). Leere Ordner zeigen also ebenfalls `sz-icon-chevron-right` |
| `RenderApplication` | private RenderFragment | Anwendungszeile: eigener Chevron (`ToggleApplicationExpanded`), `sz-tree-item-btn` (`SelectAndExpandApplication`), `ApplicationContextMenu` mit `data-testid="context-menu-toggle"`; Kinder-Rendering nur bei `_expandedApplicationIds.Contains(app.Id)` (Zeile 512) |
| `OnModeChanged` | private | Leert `_expandedApplicationIds`/`_expandedEndpointGroupIds` bei Storage-Modus-Wechsel |

Abonnierte Events: `StorageModeService.OnModeChanged`; SignalR-Hub-Events `TreeChanged`, `EndpointChanged`, `EndpointGroupChanged`.
Publizierte Events: `EventCallback`s `OnApplicationSelected`, `OnEndpointSelected`, `OnEndpointGroupSelected`, `OnCreateEndpointGroupRequested`, `OnCreateEndpointRequested`, u. a. (Zeilen 64–79) — werden von `WorkspacesSidebar` an `WorkspacesLayout` weitergereicht.

## `CollapsibleSection` (`src/Schnittstellenzentrale/Components/Shared/CollapsibleSection.razor`)

Wrapper für die Gruppenzeile (Anwendungsgruppe/Systemgruppe).

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OnInitialized` | protected override | `_expanded = InitiallyExpanded` — einmalig, spätere Parameteränderungen werden **nicht** übernommen |
| `Toggle` | private | `_expanded` umschalten (Chevron-Klick auf `.sz-tree-chevron-btn` innerhalb `.collapsible-section`) |
| `OnTitleClick` | private | `_expanded` umschalten + `OnTitleClicked` (Klick auf `.sz-tree-node-text`) |

Komponentenlokaler Zustand `_expanded` — kein Persistenzmechanismus; nach Reload zugeklappt. Für die Systemgruppe rendert `ApplicationGroupTree` `InitiallyExpanded="@_expandedGroupIds.Contains(group.Id)"` (Zeile 28) — nach Reload ist `_expandedGroupIds` leer → zugeklappt.

## `EndpointPage` (`src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor`)

Endpunkt-Editor; im Testpfad nach „Endpunkt anlegen" sichtbar (`HandleCreateEndpointRequested` selektiert den neuen Endpunkt).

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `MarkDirty` | private | `_isDirty = true` + `EnableNavigationGuardsAsync` (LocationChanging-Handler + `enableBeforeUnloadGuard` via JS) |
| `SaveAsync` | private | `PersistAsync` + `OnSaveSuccessAsync`; `DbUpdateConcurrencyException` → `_showConcurrencyWarning` |
| `PersistAsync` | private | `Id == 0` → `CreateEndpointWithDetailsAsync` (`AddEndpointAsync` + Header/QueryParameter einzeln), sonst `UpdateEndpointAsync` |
| `OnSaveSuccessAsync` | private | `_isDirty = false`, LocationChanging-Registration abmelden, `disableBeforeUnloadGuard` via JS, `OnEndpointSaved` feuern (→ `WorkspacesLayout.OnEndpointSaved` → `RefreshTreeAsync`) |
| `HandleLocationChanging` | private | Bei `_isDirty` JS-`confirm`; `PreventNavigation` bei Abbruch (nur Blazor-Navigationen, kein Browser-Reload) |
| `DisposeAsync` | public | Abmeldung inkl. `disableBeforeUnloadGuard` (JSDisconnectedException-tolerant) |

Sichtbare Signale: `.sz-endpoint-dirty-badge` (Zeilen 17–19) solange `_isDirty`; `.sz-error` bei Fehlern. Test-HTML-Klassen: `.sz-alert-danger` wird im Baum/Layout bei Fehlern gerendert (nicht auf der EndpointPage — dort `.sz-error`).

## `endpoint-page.js` (`src/Schnittstellenzentrale/wwwroot/endpoint-page.js`)

| Export | Zweck |
|--------|-------|
| `enableBeforeUnloadGuard` | Hängt `beforeunload`-Handler (`preventDefault` + `returnValue`), der Browser-Reloads blockt/dialogpflichtig macht |
| `disableBeforeUnloadGuard` | Entfernt den Handler wieder |
| `registerSaveShortcut` / `unregisterSaveShortcut` | Ctrl+S → `OnSaveShortcut` |
| `applyStoredSidebarWidth` / `initializeSidebarResize` / `getStoredSidebarWidth` / `setStoredSidebarWidth` | Einziger localStorage-Zustand des Baums (`sidebarWidth`) — keine Expand-Persistenz |

## `WorkspacesLayout` (`src/Schnittstellenzentrale/Components/Layout/WorkspacesLayout.razor`)

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `HandleCreateEndpointGroupRequested` | private | Panel `CreateEndpointGroupDialog` öffnen (`_createEndpointGroupApplication = app`) |
| `OnCreateEndpointGroupConfirmed` | private | `AddEndpointGroupAsync` + `RefreshTreeAsync` — Root-Ordner (`ParentGroupId` nicht gesetzt) |
| `HandleCreateEndpointRequested` | private | `AddEndpointAsync` mit `EndpointGroupId = args.Group?.Id` — über das **Anwendungs**-Kontextmenü ist `Group = null` (Zeile 278); danach `GetEndpointByIdAsync`, Pfad-Aufbau und `SetWorkspaceSelectionAsync` → `EndpointPage` wird gezeigt |
| `OnEndpointSaved` | private | `RefreshTreeAsync` (lädt Baumdaten neu, Expand-Sets bleiben innerhalb der Session erhalten) |
| `RefreshTreeAsync` | private | Delegiert an `WorkspacesSidebar.RefreshTreeAsync` → `ApplicationGroupTree.RefreshAsync` |

## `WorkspacesSidebar` (`src/Schnittstellenzentrale/Components/Shared/WorkspacesSidebar.razor`)

Dünne Weiterreiche-Schicht: `RefreshTreeAsync` → `_tree.RefreshAsync()`; `ExpandGroup` → `_tree.ExpandGroupAsync(groupId)`; `OnEndpointGroupSelected` setzt `WorkspaceSelection` mit Breadcrumb-Pfad.

## `ApplicationContextMenu` (`src/Schnittstellenzentrale/Components/Shared/ApplicationContextMenu.razor`)

⚙-Button `data-testid="context-menu-toggle"`; Menüeinträge u. a. `ApplicationContextMenu_CreateFolderButton` („Ordner anlegen" → `OnCreateEndpointGroupRequested`) und `ApplicationContextMenu_CreateEndpointButton` („Endpunkt anlegen" → `OnCreateEndpointRequested` mit `Group = null`-Semantik). `EndpointGroupContextMenu` existiert separat für Ordner-Kontextmenüs (`RequestCreateEndpointForGroup` → `Group != null`).

## `SystemEntryInitializer` (`src/Schnittstellenzentrale/SystemEntryInitializer.cs`)

`InitializeAsync(IServiceProvider, IConfiguration)` legt beim Start die Systemgruppe „Schnittstellenzentrale" (`IsSystem = true`) und die Systemanwendung „Schnittstellenzentrale" an, sofern `Api:BaseUrl` konfiguriert ist. Wird in den Playwright-Tests über `TestDatabaseSeeder.ResetAsync` aufgerufen — der Test-Lokator `SystemAppRow` findet die Systemanwendung über `HasText = "Schnittstellenzentrale"` (`IsSystem`-Eintrag, nicht draggable).
