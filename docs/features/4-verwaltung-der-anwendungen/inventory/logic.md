# Logik

## `ApplicationRepository`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/ApplicationRepository.cs`

Implementiert `IApplicationRepository`. Abhängigkeit: `AppDbContext`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `GetGroupsAsync(StorageMode, string)` | `public` | Liefert alle `ApplicationGroup`-Einträge inkl. `Applications`; bei `StorageMode.User` wird auf Gruppen mit Owner-Anwendungen gefiltert |
| `GetGroupByIdAsync(int)` | `public` | Lädt eine einzelne Gruppe per ID inkl. `Applications` |
| `AddGroupAsync(ApplicationGroup)` | `public` | Persistiert eine neue Gruppe |
| `UpdateGroupAsync(ApplicationGroup)` | `public` | Aktualisiert eine bestehende Gruppe |
| `DeleteGroupAsync(int)` | `public` | Löscht eine Gruppe per ID |
| `GetApplicationsAsync(StorageMode, string)` | `public` | Liefert alle Anwendungen; bei `StorageMode.User` gefiltert nach `Owner` |
| `GetUngroupedApplicationsAsync(StorageMode, string)` | `public` | Liefert Anwendungen ohne Gruppenzuordnung, Owner-gefiltert |
| `GetApplicationByIdAsync(int)` | `public` | Lädt eine Anwendung per ID inkl. aller Navigationseigenschaften |
| `AddApplicationAsync(Application)` | `public` | Persistiert eine neue Anwendung |
| `UpdateApplicationAsync(Application)` | `public` | Aktualisiert eine bestehende Anwendung |
| `DeleteApplicationAsync(int)` | `public` | Löscht eine Anwendung per ID |
| `ApplyOwnerFilter(IQueryable<Application>, StorageMode, string)` | `private static` | Hilfsmethode: filtert auf `Owner` bei `StorageMode.User` |

## `SignalRNotificationService<THub>`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/SignalRNotificationService.cs`

Implementiert `ISignalRNotificationService`. Abhängigkeit: `IHubContext<THub>`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `NotifyApplicationChangedAsync(int)` | `public` | Sendet `"ApplicationChanged"` an SignalR-Gruppe `application:{id}` |
| `NotifyGroupChangedAsync(int)` | `public` | Sendet `"GroupChanged"` an SignalR-Gruppe `group:{id}` |

## `StorageModeService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/StorageModeService.cs`

Implementiert `IStorageModeService`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `SetMode(StorageMode)` | `public` | Setzt den aktiven Modus und feuert `OnModeChanged`, wenn sich der Wert geändert hat |

Publizierte Events: `OnModeChanged` (wird ausgelöst bei Moduswechsel)

## `ApplicationGroupTree` (Blazor-Komponente)
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor`

Abonnierte Events: `IStorageModeService.OnModeChanged` → `OnModeChanged`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `OnInitializedAsync()` | `protected override` | Abonniert `OnModeChanged`, setzt `_owner`, ruft `LoadDataAsync` auf |
| `LoadDataAsync()` | `private` | Lädt `_groups` und `_ungroupedApplications` aus `IApplicationRepository` |
| `SelectApplication(int)` | `private` | Ruft `OnApplicationSelected.InvokeAsync` auf |
| `OnModeChanged()` | `private` | Ruft `LoadDataAsync` und `StateHasChanged` bei Moduswechsel auf |
| `Dispose()` | `public` | Meldet `OnModeChanged`-Handler ab |

Parameter: `EventCallback<int> OnApplicationSelected`

## `ApplicationCard` (Blazor-Komponente)
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationCard.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `OnParametersSetAsync()` | `protected override` | Lädt die Anwendung per ID aus `IApplicationRepository` |
| `OpenSwaggerImport()` | `private` | Startet Swagger-Import und öffnet Dialog |
| `OpenODataImport()` | `private` | Startet OData-Import und öffnet Dialog |
| `RunHealthCheck()` | `private` | Führt Health-Check aus und öffnet Dialog |
| `RemoveApplication()` | `private` | Löscht Anwendung über Repository und feuert `OnApplicationRemoved` |
| `CloseSwaggerImport()` | `private` | Schließt Swagger-Import-Dialog |
| `CloseODataImport()` | `private` | Schließt OData-Import-Dialog |
| `CloseHealthCheck()` | `private` | Schließt Health-Check-Dialog |

Parameter: `int ApplicationId`, `EventCallback OnApplicationRemoved`
