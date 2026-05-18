# Logik

## `AppDbContext`
Datei: `src/Schnittstellenzentrale.Infrastructure/Data/AppDbContext.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `SaveChangesAsync` | `public override` | Ruft `UpdateRowVersions()` auf, dann `base.SaveChangesAsync` |
| `SaveChanges` | `public override` | Ruft `UpdateRowVersions()` auf, dann `base.SaveChanges` |
| `UpdateRowVersions` | `private` | Setzt `RowVersion` auf neuen `Guid`-Wert für alle geänderten oder neuen Entitäten |
| `OnModelCreating` | `protected override` | Konfiguriert alle Entitäten via Fluent API |

`OnModelCreating` konfiguriert `ApplicationGroup` und `Application` aktuell **ohne** `IsSystem`. Die Spaltenkonfiguration für `IsSystem` (kein `IsRequired`, DB-Default `false`) fehlt noch.

---

## `ApplicationRepository`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/ApplicationRepository.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `GetGroupsAsync` | `public` | Gibt alle Gruppen inkl. Anwendungen zurück; bei `StorageMode.User` nur Gruppen, die eigene Anwendungen enthalten |
| `GetGroupByIdAsync` | `public` | Gibt eine Gruppe inkl. Anwendungen per ID zurück |
| `AddGroupAsync` | `public` | Legt eine neue Gruppe an |
| `UpdateGroupAsync` | `public` | Aktualisiert eine bestehende Gruppe |
| `DeleteGroupAsync` | `public` | Löscht eine Gruppe per ID |
| `GetApplicationsAsync` | `public` | Gibt Anwendungen gefiltert nach `StorageMode` und Owner zurück |
| `GetUngroupedApplicationsAsync` | `public` | Gibt Anwendungen ohne Gruppenzuordnung zurück |
| `GetApplicationByIdAsync` | `public` | Gibt eine Anwendung inkl. aller Navigationseigenschaften per ID zurück |
| `AddApplicationAsync` | `public` | Legt eine neue Anwendung an |
| `UpdateApplicationAsync` | `public` | Aktualisiert eine bestehende Anwendung; löst Tracking-Konflikte für `ApplicationGroup` auf |
| `DeleteApplicationAsync` | `public` | Löscht eine Anwendung per ID |
| `ApplyOwnerFilter` | `private static` | Filtert Anwendungen nach Owner, wenn `StorageMode.User` |

Der `DeleteGroupAsync`- und `DeleteApplicationAsync`-Methoden prüfen aktuell **nicht** auf `IsSystem`. Diese Guard-Prüfung fehlt noch.

---

## `ApplicationGroupsController`
Datei: `src/Schnittstellenzentrale/Controllers/ApplicationGroupsController.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `GetAllAsync` | `public` | GET /api/application-groups — gibt alle Gruppen zurück |
| `GetByIdAsync` | `public` | GET /api/application-groups/{id} — gibt eine Gruppe per ID zurück |
| `CreateAsync` | `public` | POST /api/application-groups — legt neue Gruppe an |
| `UpdateAsync` | `public` | PUT /api/application-groups/{id} — aktualisiert Gruppe |
| `DeleteAsync` | `public` | DELETE /api/application-groups/{id} — löscht Gruppe |
| `MapToResponse` | `private static` | Mappt `ApplicationGroup` auf `ApplicationGroupResponse` |

`DeleteAsync` enthält aktuell **keine** Prüfung auf `IsSystem`. Die `MapToResponse`-Methode überträgt `IsSystem` noch **nicht** auf das DTO.

---

## `ApplicationsController`
Datei: `src/Schnittstellenzentrale/Controllers/ApplicationsController.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `GetAllAsync` | `public` | GET /api/applications — gibt alle Anwendungen zurück |
| `GetUngroupedAsync` | `public` | GET /api/applications/ungrouped — gibt ungegruppierte Anwendungen zurück |
| `GetByIdAsync` | `public` | GET /api/applications/{id} — gibt eine Anwendung per ID zurück |
| `CreateAsync` | `public` | POST /api/applications — legt neue Anwendung an |
| `UpdateAsync` | `public` | PUT /api/applications/{id} — aktualisiert Anwendung |
| `DeleteAsync` | `public` | DELETE /api/applications/{id} — löscht Anwendung |

`DeleteAsync` enthält aktuell **keine** Prüfung auf `IsSystem`. Es existiert kein `MapToResponse` — stattdessen wird `ApplicationResponse` inline im Controller befüllt, jedoch ohne `IsSystem`.

---

## `Program.cs`
Datei: `src/Schnittstellenzentrale/Program.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `EnsureDatabaseInitializedAsync` | `static` | Erstellt einen DI-Scope, holt `AppDbContext` und ruft `MigrateDatabaseAsync` auf |
| `MigrateDatabaseAsync` | `static` | Ruft `dbContext.Database.MigrateAsync()` auf |

Ein Aufruf von `SystemEntryInitializer.InitializeAsync` nach `EnsureDatabaseInitializedAsync` fehlt noch. `IConfiguration` wird in `Program.cs` über `builder.Configuration` bereitgestellt; `Api:BaseUrl` ist in `appsettings.json` bereits gesetzt.

---

## `ApplicationGroupTree` (Blazor-Komponente)
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `OnInitializedAsync` | `protected override` | Abonniert `StorageModeService.OnModeChanged`, lädt Daten |
| `RefreshAsync` | `public` | Lädt Daten neu und löst `StateHasChanged` aus |
| `LoadDataAsync` | `private` | Ruft `ApplicationApiClient.GetGroupsAsync` und `GetUngroupedApplicationsAsync` auf |
| `SelectApplication` | `private` | Löst `OnApplicationSelected` aus |
| `OnModeChanged` | `private` | Reagiert auf Mode-Wechsel; setzt Drag-Zustand zurück, lädt Daten neu |
| `Dispose` | `public` | Meldet `OnModeChanged` vom Event ab |
| `OnDragStart` | `private` | Setzt `_draggedApplication` auf die gezogene Anwendung |
| `OnDragEnter` | `private` | Setzt `_dropTargetGroupId` |
| `OnDragLeave` | `private` | Setzt `_dropTargetGroupId` zurück |
| `OnDragEnterUngrouped` | `private` | Setzt `_dropTargetIsUngrouped = true` |
| `OnDragLeaveUngrouped` | `private` | Setzt `_dropTargetIsUngrouped = false` |
| `OnDrop` | `private` | Verschiebt `_draggedApplication` in Zielgruppe per `ApplicationApiClient.UpdateApplicationAsync` |
| `OnRemoveFromGroupRequested` | `private` | Setzt `ApplicationGroupId = null` und aktualisiert per API |

Abonnierte Events: `StorageModeService.OnModeChanged`

`OnDragStart` prüft aktuell **nicht** auf `IsSystem`. Dadurch können Systemanwendungen per Drag & Drop verschoben werden. `OnDrop` prüft `IsSystem` ebenfalls **nicht**.

---

## `ApplicationGroupContextMenu` (Blazor-Komponente)
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupContextMenu.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `ToggleMenu` | `private` | Öffnet/schließt das Kontextmenü |
| `RenameRequested` | `private` | Schließt Menü, löst `OnRenameRequested` aus |
| `DeleteRequested` | `private` | Schließt Menü, löst `OnDeleteRequested` aus |

Parameter: `Group` (`ApplicationGroup`), `OnRenameRequested`, `OnDeleteRequested`

Die Schaltflächen „Umbenennen" und „Löschen" sind für alle Gruppen gleich aktiv — es gibt noch **keine** Deaktivierung für `IsSystem`-Gruppen.

---

## `ApplicationContextMenu` (Blazor-Komponente)
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationContextMenu.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `ToggleMenu` | `private` | Öffnet/schließt das Kontextmenü |
| `EditRequested` | `private` | Schließt Menü, löst `OnEditRequested` aus |
| `RemoveFromGroupRequested` | `private` | Schließt Menü, löst `OnRemoveFromGroupRequested` aus |
| `DeleteRequested` | `private` | Schließt Menü, löst `OnDeleteRequested` aus |

Parameter: `Application` (`Application`), `OnEditRequested`, `OnRemoveFromGroupRequested`, `OnDeleteRequested`

„Aus Gruppe entfernen" wird nur angezeigt, wenn `Application.ApplicationGroupId.HasValue`. Schaltflächen „Bearbeiten" und „Löschen" sind für alle Anwendungen gleich aktiv — es gibt noch **keine** Deaktivierung für `IsSystem`-Anwendungen.
