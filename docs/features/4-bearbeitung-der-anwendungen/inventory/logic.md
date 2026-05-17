# Logik

## `ApplicationContextMenu` (Blazor-Komponente)
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationContextMenu.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `ToggleMenu` | `private` | Wechselt `_isOpen` zwischen `true` und `false` |
| `EditRequested` | `private async` | Setzt `_isOpen = false`, löst `OnEditRequested` mit `Application` aus |
| `RemoveFromGroupRequested` | `private async` | Setzt `_isOpen = false`, löst `OnRemoveFromGroupRequested` mit `Application` aus |
| `DeleteRequested` | `private async` | Setzt `_isOpen = false`, löst `OnDeleteRequested` mit `Application` aus |

Parameter:

| Parameter | Typ | Zweck |
|---|---|---|
| `Application` | `Application` | Die betroffene Anwendung |
| `OnEditRequested` | `EventCallback<Application>` | Callback für „Bearbeiten" |
| `OnRemoveFromGroupRequested` | `EventCallback<Application>` | Callback für „Aus Gruppe entfernen" |
| `OnDeleteRequested` | `EventCallback<Application>` | Callback für „Löschen" |

Felder: `_isOpen` (`bool`) — steuert Sichtbarkeit des Dropdowns.

Markup-Ist-Zustand:
- Zahnrad-Icon: `⚙` (U+2699, einfaches Unicode-Zeichen, kein Emoji-Suffix)
- Toggle-Button: `<button class="btn btn-link btn-sm context-menu-toggle">`
- Overlay zum Schließen: `<div class="context-menu-overlay" @onclick="() => _isOpen = false">` — `position: fixed; inset: 0; z-index: 999` (aus `app.css`)
- Dropdown-Panel: `<div class="context-menu-dropdown">` — **keine** CSS-Regel für `.context-menu-dropdown` in `app.css` vorhanden
- Aktions-Buttons: `<button class="dropdown-item">` mit `@onclick`-Handlern
- `RemoveFromGroupRequested`-Button erscheint nur, wenn `Application.ApplicationGroupId.HasValue`

Defekte (Ist-Zustand):
- Overlay liegt mit `z-index: 999` über dem Dropdown-Panel — Klicks auf Menü-Einträge landen auf dem Overlay, bevor der Button-`@onclick` ausgeführt wird
- `.context-menu-dropdown` hat keine `position: absolute`-Regel — Dropdown verdrängt nachfolgenden Inhalt aus dem Fluss
- `.context-menu-toggle` hat keine `opacity: 0`-Regel — Icon immer sichtbar
- `context-menu-container` hat kein `position: relative` in CSS, was für `position: absolute` des Dropdowns benötigt wird

---

## `ApplicationGroupContextMenu` (Blazor-Komponente)
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupContextMenu.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `ToggleMenu` | `private` | Wechselt `_isOpen` zwischen `true` und `false` |
| `RenameRequested` | `private async` | Setzt `_isOpen = false`, löst `OnRenameRequested` mit `Group` aus |
| `DeleteRequested` | `private async` | Setzt `_isOpen = false`, löst `OnDeleteRequested` mit `Group` aus |

Parameter:

| Parameter | Typ | Zweck |
|---|---|---|
| `Group` | `ApplicationGroup` | Die betroffene Gruppe |
| `OnRenameRequested` | `EventCallback<ApplicationGroup>` | Callback für „Umbenennen" |
| `OnDeleteRequested` | `EventCallback<ApplicationGroup>` | Callback für „Löschen" |

Felder: `_isOpen` (`bool`) — steuert Sichtbarkeit des Dropdowns.

Markup-Ist-Zustand: Struktur identisch mit `ApplicationContextMenu` (gleiche CSS-Klassen, gleiches Icon, gleiche Overlay-Mechanik). Defekte analog.

---

## `ApplicationGroupTree` (Blazor-Komponente)
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `OnInitializedAsync` | `protected override async` | Abonniert `StorageModeService.OnModeChanged`; lädt initiale Daten über `LoadDataAsync` |
| `LoadDataAsync` | `private async` | Lädt `_groups` und `_ungroupedApplications` via `IApplicationRepository` |
| `SelectApplication` | `private async` | Löst `OnApplicationSelected`-Callback aus |
| `OnModeChanged` | `private` | Setzt alle Editor/Dialog-States zurück, lädt Daten neu, ruft `StateHasChanged` auf; löst `OnSelectionCleared` aus |
| `Dispose` | `public` | Deabonniert `StorageModeService.OnModeChanged` |
| `ShowGroupEditor` | `private` | Blendet `ApplicationGroupEditor` ein |
| `ShowApplicationEditor` | `private` | Blendet `ApplicationEditor` ein |
| `OnGroupSaved` | `private async` | Schließt Gruppeneditor, lädt Daten neu |
| `OnApplicationSaved` | `private async` | Schließt Anwendungseditor, lädt Daten neu |
| `OnEditorCancelled` | `private` | Schließt alle Editoren |
| `OnDialogCancelled` | `private` | Setzt Rename/Delete-Dialog-Targets auf `null` |
| `OnRenameGroupRequested` | `private` | Setzt `_renameTargetGroup` |
| `OnGroupRenamed` | `private async` | Ruft `UpdateGroupAsync` auf, bei Team-Mode SignalR-Benachrichtigung, lädt Daten neu |
| `OnDeleteGroupRequested` | `private` | Setzt `_deleteTargetGroup` |
| `OnDeleteGroupConfirmedAll` | `private async` | Löscht alle Anwendungen der Gruppe einzeln, dann die Gruppe selbst |
| `OnDeleteGroupConfirmedGroupOnly` | `private async` | Setzt `ApplicationGroupId` aller Anwendungen auf `null`, löscht dann die Gruppe |
| `ProcessGroupApplicationsAsync` | `private async` | Iteriert Anwendungen einer Gruppe und wendet einen Delegate auf jede an |
| `OnEditApplicationRequested` | `private` | Setzt `_editTargetApplication` |
| `OnApplicationEdited` | `private async` | Setzt `_editTargetApplication = null`, lädt Daten neu |
| `OnRemoveFromGroupRequested` | `private async` | Setzt `ApplicationGroupId = null`, ruft `UpdateApplicationAsync` auf |
| `OnDeleteApplicationRequested` | `private` | Setzt `_deleteTargetApplication` |
| `OnDeleteApplicationConfirmed` | `private async` | Ruft `DeleteApplicationAsync` auf, löst `OnSelectionCleared` aus, lädt Daten neu |
| `OnDragStart` | `private` | Setzt `_draggedApplication` |
| `OnDrop` | `private async` | Setzt `ApplicationGroupId` auf `targetGroupId`, ruft `UpdateApplicationAsync` auf, setzt `_draggedApplication = null`, lädt Daten neu |

Parameter:

| Parameter | Typ | Zweck |
|---|---|---|
| `OnApplicationSelected` | `EventCallback<int>` | Wird aufgerufen, wenn der Benutzer eine Anwendung wählt |
| `OnSelectionCleared` | `EventCallback` | Wird bei Löschung einer Anwendung oder Moduswechsel ausgelöst |

Felder: `_groups`, `_ungroupedApplications`, `_owner`, `_showGroupEditor`, `_showApplicationEditor`, `_renameTargetGroup`, `_deleteTargetGroup`, `_deleteTargetApplication`, `_editTargetApplication`, `_draggedApplication`, `_errorMessage`

Abonnierte Events: `IStorageModeService.OnModeChanged`

Drag-&-Drop-Ist-Zustand:
- `ondragstart` auf `.tree-leaf`-Divs ruft `() => OnDragStart(app)` auf — setzt nur `_draggedApplication`, ruft **kein** `e.DataTransfer.SetData()` auf (kein `DragEventArgs`-Parameter)
- `ondragover:preventDefault="true"` auf `.tree-leaf`-Divs vorhanden
- Drop-Target: `<CollapsibleSection OnDrop="() => OnDrop(group.Id)">` — Weiterleitung über `CollapsibleSection.HandleDrop`
- Kein `ondragenter`/`ondragleave`-Handler vorhanden — kein visuelles Drag-over-Feedback
- Kein `_dropTargetGroupId`-Feld vorhanden

---

## `CollapsibleSection` (Blazor-Komponente)
Datei: `src/Schnittstellenzentrale/Components/Shared/CollapsibleSection.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `OnInitialized` | `protected override` | Setzt `_expanded` auf `InitiallyExpanded` |
| `Toggle` | `private` | Wechselt `_expanded` |
| `HandleDrop` | `private async` | Ruft `OnDrop.InvokeAsync()` auf, wenn `OnDrop.HasDelegate` |

Parameter:

| Parameter | Typ | Zweck |
|---|---|---|
| `Title` | `string` | Überschrift des Abschnitts |
| `ChildContent` | `RenderFragment?` | Untergeordneter Inhalt |
| `TitleActions` | `RenderFragment?` | Aktions-Elemente neben dem Titel (z. B. Kontextmenü) |
| `InitiallyExpanded` | `bool` | Gibt an, ob der Abschnitt initial geöffnet ist (Default: `false`) |
| `OnDrop` | `EventCallback` | Drop-Callback; wird von `HandleDrop` ausgelöst |

Felder: `_expanded` (`bool`)

Drag-&-Drop-Ist-Zustand:
- `@ondrop="HandleDrop"` auf dem Root-`<div class="collapsible-section">` — leitet Drop weiter
- `@ondrop:preventDefault="OnDrop.HasDelegate"` — nur aktiv, wenn Delegate vorhanden
- `@ondragover:preventDefault="OnDrop.HasDelegate"` — verhindert browser-Default für Dragover, damit Drop erlaubt wird
- Kein `ondragenter`/`ondragleave` auf diesem Element

---

## `ApplicationRepository`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/ApplicationRepository.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `GetGroupsAsync` | `public async` | Gruppen mit `Include(Applications)` laden; im `User`-Mode auf Gruppen mit eigenen Anwendungen gefiltert |
| `GetGroupByIdAsync` | `public async` | Gruppe mit ID inkl. Anwendungen laden |
| `AddGroupAsync` | `public async` | Gruppe in Context einfügen und speichern |
| `UpdateGroupAsync` | `public async` | Gruppe im Context aktualisieren und speichern |
| `DeleteGroupAsync` | `public async` | Gruppe per `FindAsync` laden, entfernen, speichern |
| `GetApplicationsAsync` | `public async` | Anwendungen mit `Include(ApplicationGroup)` laden; Owner-Filter über `ApplyOwnerFilter` |
| `GetUngroupedApplicationsAsync` | `public async` | Nur Anwendungen mit `ApplicationGroupId == null`; Owner-Filter |
| `GetApplicationByIdAsync` | `public async` | Anwendung inkl. `ApplicationGroup`, `Endpoints`, `EndpointGroups` laden |
| `AddApplicationAsync` | `public async` | Anwendung in Context einfügen und speichern |
| `UpdateApplicationAsync` | `public async` | Anwendung im Context aktualisieren und speichern |
| `DeleteApplicationAsync` | `public async` | Anwendung per `FindAsync` laden, entfernen, speichern |
| `ApplyOwnerFilter` | `private static` | Im `StorageMode.User`: Query auf `Owner == owner` einschränken |

---

## `StorageModeService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/StorageModeService.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `SetMode` | `public` | Setzt `CurrentMode` und feuert `OnModeChanged` — nur bei tatsächlicher Änderung |

Publizierte Events: `OnModeChanged` (`event Action?`)

---

## `SignalRNotificationService<THub>`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/SignalRNotificationService.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `NotifyApplicationChangedAsync` | `public async` | Sendet `"ApplicationChanged"` an SignalR-Gruppe `application:{applicationId}` |
| `NotifyGroupChangedAsync` | `public async` | Sendet `"GroupChanged"` an SignalR-Gruppe `group:{groupId}` |
