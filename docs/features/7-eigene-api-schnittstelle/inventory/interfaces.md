# Interfaces

## `IApplicationRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IApplicationRepository.cs`

Implementiert durch `ApplicationRepository`.

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `GetGroupsAsync` | `StorageMode storageMode, string owner` | `Task<IList<ApplicationGroup>>` | Alle Gruppen laden (mit Owner-Filter im User-Modus) |
| `GetGroupByIdAsync` | `int id` | `Task<ApplicationGroup?>` | Einzelne Gruppe per Id laden |
| `AddGroupAsync` | `ApplicationGroup group` | `Task<ApplicationGroup>` | Neue Gruppe anlegen |
| `UpdateGroupAsync` | `ApplicationGroup group` | `Task<ApplicationGroup>` | Gruppe aktualisieren |
| `DeleteGroupAsync` | `int id` | `Task` | Gruppe löschen |
| `GetApplicationsAsync` | `StorageMode storageMode, string owner` | `Task<IList<Application>>` | Alle Applications laden (mit Owner-Filter) |
| `GetUngroupedApplicationsAsync` | `StorageMode storageMode, string owner` | `Task<IList<Application>>` | Applications ohne Gruppe laden |
| `GetApplicationByIdAsync` | `int id` | `Task<Application?>` | Einzelne Application per Id laden |
| `AddApplicationAsync` | `Application application` | `Task<Application>` | Neue Application anlegen |
| `UpdateApplicationAsync` | `Application application` | `Task<Application>` | Application aktualisieren |
| `DeleteApplicationAsync` | `int id` | `Task` | Application löschen |

---

## `ISignalRNotificationService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ISignalRNotificationService.cs`

Implementiert durch `SignalRNotificationService<THub>`.

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `NotifyApplicationChangedAsync` | `int applicationId` | `Task` | Team-Modus: Benachrichtigung bei Application-Änderung |
| `NotifyGroupChangedAsync` | `int groupId` | `Task` | Team-Modus: Benachrichtigung bei Gruppen-Änderung |

Wird in `ApplicationGroupEditor.SaveAsync` (für Gruppen) und in `ApplicationEditor.SaveAsync` (für Applications) aufgerufen.

---

## `IStorageModeService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IStorageModeService.cs`

Implementiert durch `StorageModeService`.

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `SetMode` | `StorageMode mode` | `void` | Aktuellen Storage-Modus setzen |

Eigenschaft: `CurrentMode` (`StorageMode`) — liefert den aktuellen Modus.  
Event: `OnModeChanged` (`Action?`) — wird bei Moduswechsel ausgelöst.

---

## `ICurrentUserService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ICurrentUserService.cs`

Implementiert durch `WindowsCurrentUserService`.

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `GetCurrentUserName` | — | `string` | Liefert den Windows-Benutzernamen (z. B. `DOMAIN\user`) |

Wird in `ApplicationEditor.OnInitializedAsync` (für `GetGroupsAsync`-Aufruf) und `ApplicationEditor.SaveAsync` (für Owner-Zuweisung) verwendet.
