# Interfaces

## `IApplicationRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IApplicationRepository.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `GetGroupsAsync` | `StorageMode, string owner` | `Task<IList<ApplicationGroup>>` | Alle Gruppen abrufen, optional Owner-gefiltert |
| `GetGroupByIdAsync` | `int id` | `Task<ApplicationGroup?>` | Einzelne Gruppe per ID |
| `AddGroupAsync` | `ApplicationGroup` | `Task<ApplicationGroup>` | Neue Gruppe persistieren |
| `UpdateGroupAsync` | `ApplicationGroup` | `Task<ApplicationGroup>` | Bestehende Gruppe aktualisieren |
| `DeleteGroupAsync` | `int id` | `Task` | Gruppe löschen |
| `GetApplicationsAsync` | `StorageMode, string owner` | `Task<IList<Application>>` | Alle Anwendungen, optional Owner-gefiltert |
| `GetUngroupedApplicationsAsync` | `StorageMode, string owner` | `Task<IList<Application>>` | Anwendungen ohne Gruppe |
| `GetApplicationByIdAsync` | `int id` | `Task<Application?>` | Einzelne Anwendung per ID |
| `AddApplicationAsync` | `Application` | `Task<Application>` | Neue Anwendung persistieren |
| `UpdateApplicationAsync` | `Application` | `Task<Application>` | Bestehende Anwendung aktualisieren |
| `DeleteApplicationAsync` | `int id` | `Task` | Anwendung löschen |

Implementiert von: `ApplicationRepository` (`src/Schnittstellenzentrale.Infrastructure/Repositories/ApplicationRepository.cs`)

## `ISignalRNotificationService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ISignalRNotificationService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `NotifyApplicationChangedAsync` | `int applicationId` | `Task` | Benachrichtigt SignalR-Clients bei Anwendungsänderung |
| `NotifyGroupChangedAsync` | `int groupId` | `Task` | Benachrichtigt SignalR-Clients bei Gruppenänderung |

Implementiert von: `SignalRNotificationService<THub>` (`src/Schnittstellenzentrale.Infrastructure/Services/SignalRNotificationService.cs`)

## `ICurrentUserService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ICurrentUserService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `GetCurrentUserName` | — | `string` | Liefert den Windows-Benutzernamen für die Owner-Zuweisung |

## `IStorageModeService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IStorageModeService.cs`

| Methode / Eigenschaft | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `CurrentMode` | — | `StorageMode` | Aktueller Speichermodus |
| `OnModeChanged` | — | `event Action?` | Event bei Moduswechsel |
| `SetMode` | `StorageMode mode` | `void` | Modus setzen und Event auslösen |

Implementiert von: `StorageModeService` (`src/Schnittstellenzentrale.Infrastructure/Services/StorageModeService.cs`)
Wird abonniert von: `ApplicationGroupTree`
