# Interfaces

## `IApplicationRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IApplicationRepository.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `GetGroupsAsync` | `StorageMode storageMode, string owner` | `Task<IList<ApplicationGroup>>` | Alle Gruppen laden (im User-Mode gefiltert nach Gruppen mit mindestens einer eigenen Anwendung) |
| `GetGroupByIdAsync` | `int id` | `Task<ApplicationGroup?>` | Einzelne Gruppe inkl. Anwendungen laden |
| `AddGroupAsync` | `ApplicationGroup group` | `Task<ApplicationGroup>` | Neue Gruppe anlegen |
| `UpdateGroupAsync` | `ApplicationGroup group` | `Task<ApplicationGroup>` | Bestehende Gruppe aktualisieren (z. B. umbenennen) |
| `DeleteGroupAsync` | `int id` | `Task` | Gruppe löschen |
| `GetApplicationsAsync` | `StorageMode storageMode, string owner` | `Task<IList<Application>>` | Alle Anwendungen laden (im User-Mode nach Owner gefiltert) |
| `GetUngroupedApplicationsAsync` | `StorageMode storageMode, string owner` | `Task<IList<Application>>` | Nur gruppenlose Anwendungen laden |
| `GetApplicationByIdAsync` | `int id` | `Task<Application?>` | Einzelne Anwendung inkl. Endpoints, EndpointGroups, Headers, QueryParameters laden |
| `AddApplicationAsync` | `Application application` | `Task<Application>` | Neue Anwendung anlegen |
| `UpdateApplicationAsync` | `Application application` | `Task<Application>` | Bestehende Anwendung aktualisieren (inkl. Gruppenänderung) |
| `DeleteApplicationAsync` | `int id` | `Task` | Anwendung löschen |

---

## `IStorageModeService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IStorageModeService.cs`

| Methode / Eigenschaft | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `CurrentMode` | — | `StorageMode` | Aktuell aktiver Speichermodus |
| `OnModeChanged` | — | `event Action?` | Event, das beim Moduswechsel ausgelöst wird |
| `SetMode` | `StorageMode mode` | `void` | Modus wechseln und `OnModeChanged` auslösen |

---

## `ISignalRNotificationService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ISignalRNotificationService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `NotifyApplicationChangedAsync` | `int applicationId` | `Task` | SignalR-Benachrichtigung an Gruppe `application:{id}` senden |
| `NotifyGroupChangedAsync` | `int groupId` | `Task` | SignalR-Benachrichtigung an Gruppe `group:{id}` senden |
