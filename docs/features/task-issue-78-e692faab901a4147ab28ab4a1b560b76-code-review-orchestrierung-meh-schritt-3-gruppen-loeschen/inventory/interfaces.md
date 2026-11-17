# Bestandsaufnahme: Interfaces

## `IApplicationRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IApplicationRepository.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `GetGroupsAsync` | `StorageMode storageMode, string owner` | `Task<IList<ApplicationGroup>>` | alle Gruppen (Owner-gefiltert im User-Modus) |
| `GetGroupByIdAsync` | `int id` | `Task<ApplicationGroup?>` | einzelne Gruppe inkl. `Applications` |
| `GetSystemGroupAsync` | — | `Task<ApplicationGroup?>` | Systemgruppe |
| `AddGroupAsync` | `ApplicationGroup group` | `Task<ApplicationGroup>` | anlegen |
| `UpdateGroupAsync` | `ApplicationGroup group` | `Task<ApplicationGroup>` | umbenennen (RowVersion) |
| `DeleteGroupAsync` | `int id` | `Task` | **bisher ohne Modus/Transaktion** — wird erweitert |
| `GetApplicationsAsync` / `GetUngroupedApplicationsAsync` | `StorageMode, owner` | `Task<IList<Application>>` | Listen |
| `GetApplicationByIdAsync` / `AddApplicationAsync` / `UpdateApplicationAsync` / `DeleteApplicationAsync` | … | … | Anwendungs-CRUD |
| `GetApplicationCountByGroupAsync` / `GetEndpointCountByGroupAsync` | `int groupId` | `Task<int>` | Zähler |

Aufrufer von `DeleteGroupAsync`: `ApplicationGroupsController.DeleteAsync`, `ODataApplicationGroupsController.DeleteAsync`, `SystemEntryInitializerTests` (Fake-Implementierung, Z. 320), `ApplicationRepositoryIntegrationTests`.

## `IApplicationApiClient`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IApplicationApiClient.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `GetGroupsAsync` | `StorageMode, owner` | `Task<IList<ApplicationGroup>>` | Gruppenliste |
| `GetGroupByIdAsync` | `int id` | `Task<ApplicationGroup?>` | einzelne Gruppe |
| `AddGroupAsync` / `UpdateGroupAsync` | `ApplicationGroup` | `Task<ApplicationGroup>` | anlegen/ändern |
| `DeleteGroupAsync` | `int id` | `Task` | **bisher ohne Parameter** — wird um parametrisierte Variante erweitert |
| übrige | … | … | Anwendungen, Endpunktgruppen, Endpunkte, Header, Query-Parameter, Umgebungen, Import/Apply |

Aufrufer von `DeleteGroupAsync`: `WorkspacesLayout.OnDeleteGroupConfirmedAll`/`OnDeleteGroupConfirmedGroupOnly` (beide Z. 226/243 — Umstellung erst Schritt 4).

## `ISignalRNotificationService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ISignalRNotificationService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `NotifyTreeChangedAsync` | — | `Task` | Benachrichtigung über Baum-Änderung (bleibt unverändert) |
