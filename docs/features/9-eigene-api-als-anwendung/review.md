# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

### Neue Klassen

- [x] `SystemEntryInitializer` (Statische Klasse) — angelegt

### Datenmodell

- [x] Feld `IsSystem` (`bool`) in `ApplicationGroup` — vorhanden
- [x] Feld `IsSystem` (`bool`) in `Application` — vorhanden

### Response-DTOs

- [x] Feld `IsSystem` (`bool`) in `ApplicationGroupResponse` — vorhanden
- [x] Feld `IsSystem` (`bool`) in `ApplicationResponse` — vorhanden

### Interface

- [x] Methode `GetSystemGroupAsync` in `IApplicationRepository` — vorhanden

### Repository

- [x] Methode `GetSystemGroupAsync` in `ApplicationRepository` — vorhanden; filtert per `FirstOrDefaultAsync(g => g.IsSystem)`, lädt `Applications`

### Datenbankschicht

- [x] `AppDbContext.OnModelCreating` — `IsSystem` für `ApplicationGroup` und `Application` mit `HasDefaultValue(false)` konfiguriert

### Migration

- [x] Migration `AddIsSystemToApplicationGroupAndApplication` — vorhanden; fügt `IsSystem` (bool, nullable: false, default: false) zu `ApplicationGroups` und `Applications` hinzu

### Controller

- [x] `ApplicationGroupsController.DeleteAsync` — Guard auf `IsSystem` vorhanden; gibt 403 zurück
- [x] `ApplicationGroupsController.UpdateAsync` — Guard auf `IsSystem` vorhanden; gibt 403 zurück
- [x] `ApplicationGroupsController.MapToResponse` — überträgt `IsSystem` in `ApplicationGroupResponse`
- [x] `ApplicationsController.DeleteAsync` — Guard auf `IsSystem` vorhanden; gibt 403 zurück
- [x] `ApplicationsController.UpdateAsync` — Guard auf `IsSystem` vorhanden; gibt 403 zurück
- [x] Inline-Mapping in `ApplicationsController` (`GetByIdAsync`, `GetAllAsync`, `GetUngroupedAsync`) — `IsSystem` wird über `MapToResponse` in `ApiControllerBase` übertragen

### Startup

- [x] `Program.cs` — Aufruf von `SystemEntryInitializer.InitializeAsync(app.Services, builder.Configuration)` nach `EnsureDatabaseInitializedAsync` vorhanden

### `SystemEntryInitializer.InitializeAsync`

- [x] Erstellt `IServiceScope`, löst `IApplicationRepository` auf
- [x] Liest `Api:BaseUrl`; loggt Warnung und kehrt zurück, wenn leer
- [x] Legt Systemgruppe an, wenn keine vorhanden (`AddGroupAsync`, `IsSystem = true`, Name „Schnittstellenzentrale")
- [x] Legt Systemanwendung an, wenn keine vorhanden (`AddApplicationAsync`, `IsSystem = true`, `BaseUrl`, `InterfaceUrl`, `ApplicationGroupId`)
- [x] Aktualisiert `BaseUrl` und `InterfaceUrl`, wenn vorhanden aber abweichend (`UpdateApplicationAsync`)
- [x] Fängt alle Ausnahmen ab, loggt per Serilog als Fehler, ohne den Programmstart zu blockieren

### Blazor-Komponenten

- [x] `ApplicationGroupContextMenu` — `disabled`-Attribut auf „Umbenennen" und „Löschen" gesetzt, wenn `Group.IsSystem`
- [x] `ApplicationContextMenu` — `disabled`-Attribut auf „Bearbeiten" und „Löschen" gesetzt, wenn `Application.IsSystem`
- [x] `ApplicationGroupTree.OnDragStart` — Guard vorhanden: wenn `application.IsSystem`, wird `_draggedApplication` nicht gesetzt
- [x] `ApplicationGroupTree` Razor-Template — `draggable`-Attribut per Ausdruck auf `"false"` gesetzt, wenn `application.IsSystem`

### Tests

- [x] `InitializeAsync_WhenGroupAndApplicationMissing_CreatesBoth` in `SystemEntryInitializerTests` — vorhanden
- [x] `InitializeAsync_WhenGroupExistsButApplicationMissing_CreatesApplication` in `SystemEntryInitializerTests` — vorhanden
- [x] `InitializeAsync_WhenUrlDiffers_UpdatesBaseUrlAndInterfaceUrl` in `SystemEntryInitializerTests` — vorhanden
- [x] `InitializeAsync_WhenUrlMatches_MakesNoChanges` in `SystemEntryInitializerTests` — vorhanden
- [x] `InitializeAsync_IsIdempotent_OnRepeatedCall` in `SystemEntryInitializerTests` — vorhanden
- [x] `InitializeAsync_WhenDbThrows_DoesNotPropagateException` in `SystemEntryInitializerTests` — vorhanden
- [x] `InitializeAsync_WhenBaseUrlMissing_SkipsAndLogs` in `SystemEntryInitializerTests` — vorhanden
- [x] `DeleteApplicationGroup_WithSystemGroup_Returns403` in `ApplicationGroupsControllerIntegrationTests` — vorhanden
- [x] `PutApplicationGroup_WithSystemGroup_Returns403` in `ApplicationGroupsControllerIntegrationTests` — vorhanden
- [x] `DeleteApplication_WithSystemApplication_Returns403` in `ApplicationsControllerIntegrationTests` — vorhanden
- [x] `PutApplication_WithSystemApplication_Returns403` in `ApplicationsControllerIntegrationTests` — vorhanden
- [x] `Bearbeiten_Deaktiviert_WennIsSystem` in `ApplicationContextMenuTests` — vorhanden
- [x] `Löschen_Deaktiviert_WennIsSystem` in `ApplicationContextMenuTests` — vorhanden

## Offene Aufgaben

Keine.

## Hinweise

- `MapToResponse` für `Application` wurde nicht direkt in `ApplicationsController` als `private static`-Methode implementiert, sondern als `protected static`-Methode in `ApiControllerBase`. Dies deckt alle im Plan genannten Aufrufstellen (`GetByIdAsync`, `GetAllAsync`, `GetUngroupedAsync`, `CreateAsync`, `UpdateAsync`) vollständig ab und entspricht dem Planinhalt.
- `ApplicationGroupsController.MapToResponse` delegiert das Mapping der enthaltenen Anwendungen an dieselbe Methode in `ApiControllerBase`, wodurch `IsSystem` konsistent übertragen wird.
- Der bestehende Test `GetApplicationById_WithValidId_Returns200WithAllFields` prüft `IsSystem` nicht explizit; da `ApplicationResponse` das Feld nun trägt und der Deserialisierer es befüllt, ist der Test rückwärtskompatibel. Der Plan nennt dies unter „Betroffene bestehende Tests" als potenziellen Anpassungsbedarf — eine explizite Assertion fehlt noch, liegt aber außerhalb der neu geplanten Testmethoden und ist daher kein offener Planpunkt.
