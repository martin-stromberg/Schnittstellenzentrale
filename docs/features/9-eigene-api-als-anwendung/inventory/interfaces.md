# Interfaces

## `IApplicationRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IApplicationRepository.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `GetGroupsAsync` | `StorageMode storageMode, string owner` | `Task<IList<ApplicationGroup>>` | Alle Gruppen laden |
| `GetGroupByIdAsync` | `int id` | `Task<ApplicationGroup?>` | Einzelne Gruppe per ID laden |
| `AddGroupAsync` | `ApplicationGroup group` | `Task<ApplicationGroup>` | Neue Gruppe anlegen |
| `UpdateGroupAsync` | `ApplicationGroup group` | `Task<ApplicationGroup>` | Gruppe aktualisieren |
| `DeleteGroupAsync` | `int id` | `Task` | Gruppe löschen |
| `GetApplicationsAsync` | `StorageMode storageMode, string owner` | `Task<IList<Application>>` | Alle Anwendungen laden |
| `GetUngroupedApplicationsAsync` | `StorageMode storageMode, string owner` | `Task<IList<Application>>` | Ungegruppierte Anwendungen laden |
| `GetApplicationByIdAsync` | `int id` | `Task<Application?>` | Einzelne Anwendung per ID laden |
| `AddApplicationAsync` | `Application application` | `Task<Application>` | Neue Anwendung anlegen |
| `UpdateApplicationAsync` | `Application application` | `Task<Application>` | Anwendung aktualisieren |
| `DeleteApplicationAsync` | `int id` | `Task` | Anwendung löschen |

Das Interface enthält aktuell keine Methode zum gezielten Suchen nach `IsSystem`-Einträgen (z. B. `GetSystemGroupAsync` oder `GetSystemApplicationAsync`). Solche Methoden wären für den `SystemEntryInitializer` nötig oder alternativ ein direkter Zugriff auf `AppDbContext` via `IServiceScope`.

---

## `IApplicationApiClient`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IApplicationApiClient.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `GetGroupsAsync` | `StorageMode storageMode, string owner` | `Task<IList<ApplicationGroup>>` | Gruppen über HTTP-API laden |
| `GetGroupByIdAsync` | `int id` | `Task<ApplicationGroup?>` | Einzelne Gruppe per ID laden |
| `AddGroupAsync` | `ApplicationGroup group` | `Task<ApplicationGroup>` | Neue Gruppe anlegen |
| `UpdateGroupAsync` | `ApplicationGroup group` | `Task<ApplicationGroup>` | Gruppe aktualisieren |
| `DeleteGroupAsync` | `int id` | `Task` | Gruppe löschen |
| `GetUngroupedApplicationsAsync` | `StorageMode storageMode, string owner` | `Task<IList<Application>>` | Ungegruppierte Anwendungen laden |
| `GetApplicationByIdAsync` | `int id` | `Task<Application?>` | Einzelne Anwendung per ID laden |
| `AddApplicationAsync` | `Application application` | `Task<Application>` | Neue Anwendung anlegen |
| `UpdateApplicationAsync` | `Application application` | `Task<Application>` | Anwendung aktualisieren |
| `DeleteApplicationAsync` | `int id` | `Task` | Anwendung löschen |

Wird von `ApplicationGroupTree` verwendet. Der `SystemEntryInitializer` soll laut Anforderung **nicht** diesen Client verwenden, sondern direkt auf `IApplicationRepository` zugreifen.
