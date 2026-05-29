# Interfaces

## `IThemeService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IThemeService.cs`

| Methode/Eigenschaft | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `CurrentScheme` (Eigenschaft) | — | `ColorScheme` | Aktuell aktives Theme |
| `OnThemeChanged` (Event) | — | `Action?` | Wird bei Theme-Wechsel gefeuert |
| `InitializeAsync()` | — | `Task` | Liest gespeichertes Theme aus `localStorage` |
| `SetTheme(ColorScheme)` | `scheme` | `Task` | Setzt und persistiert das Theme |

Implementiert von: `ThemeService`

---

## `IStorageModeService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IStorageModeService.cs`

| Methode/Eigenschaft | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `CurrentMode` (Eigenschaft) | — | `StorageMode` | Aktuell aktiver Speichermodus |
| `OnModeChanged` (Event) | — | `Action?` | Wird bei Moduswechsel gefeuert |
| `SetMode(StorageMode)` | `mode` | `void` | Setzt den Speichermodus |

Implementiert von: `StorageModeService`

---

## `IApplicationRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IApplicationRepository.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `GetGroupsAsync` | `StorageMode, string` | `Task<IList<ApplicationGroup>>` | Lädt alle Gruppen gefiltert nach Modus/Eigentümer |
| `GetGroupByIdAsync` | `int` | `Task<ApplicationGroup?>` | Lädt eine Gruppe per ID |
| `GetSystemGroupAsync` | — | `Task<ApplicationGroup?>` | Lädt die Systemgruppe |
| `AddGroupAsync` | `ApplicationGroup` | `Task<ApplicationGroup>` | Legt neue Gruppe an |
| `UpdateGroupAsync` | `ApplicationGroup` | `Task<ApplicationGroup>` | Aktualisiert eine Gruppe |
| `DeleteGroupAsync` | `int` | `Task` | Löscht eine Gruppe |
| `GetApplicationsAsync` | `StorageMode, string` | `Task<IList<Application>>` | Lädt alle Anwendungen |
| `GetUngroupedApplicationsAsync` | `StorageMode, string` | `Task<IList<Application>>` | Lädt Anwendungen ohne Gruppe |
| `GetApplicationByIdAsync` | `int` | `Task<Application?>` | Lädt eine Anwendung mit Navigation |
| `AddApplicationAsync` | `Application` | `Task<Application>` | Legt neue Anwendung an |
| `UpdateApplicationAsync` | `Application` | `Task<Application>` | Aktualisiert eine Anwendung |
| `DeleteApplicationAsync` | `int` | `Task` | Löscht eine Anwendung |

Implementiert von: `ApplicationRepository`

Fehlend (laut Anforderung): Methoden für `ApplicationLink`-CRUD, Zählung von Anwendungen/Endpunkten je Gruppe.

---

## `IActivityLogService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IActivityLogService.cs`

| Methode/Eigenschaft | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `Entries` (Eigenschaft) | — | `IReadOnlyList<ActivityLogEntry>` | Alle bisher geloggten Einträge |
| `OnEntryAdded` (Event) | — | `Action?` | Wird nach jedem neuen Eintrag gefeuert |
| `Log` | `ActivityLogCategory, string, string?` | `void` | Fügt einen Eintrag hinzu |
| `Clear` | — | `void` | Leert die Eintrags-Liste |

Implementiert von: `ActivityLogService` (In-Memory, keine Persistenz)

---

## `ISystemEnvironmentRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ISystemEnvironmentRepository.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `GetEnvironmentsAsync` | `StorageMode, string?` | `Task<IList<SystemEnvironment>>` | Lädt Umgebungen gefiltert nach Modus/Eigentümer |
| `GetByIdAsync` | `int` | `Task<SystemEnvironment?>` | Lädt eine Umgebung per ID |
| `AddAsync` | `SystemEnvironment` | `Task<SystemEnvironment>` | Legt neue Umgebung an |
| `UpdateAsync` | `SystemEnvironment` | `Task<SystemEnvironment>` | Aktualisiert eine Umgebung |
| `DeleteAsync` | `int` | `Task` | Löscht eine Umgebung |
| `UpdateVariableAsync` | `int, string, string` | `Task` | Aktualisiert den Wert einer einzelnen Variable |

---

## `IEndpointExecutionService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IEndpointExecutionService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---|---|---|---|
| `ExecuteAsync` | `Endpoint` | `Task<EndpointExecutionResult>` | Führt einen Endpunkt aus |

Implementiert von: `EndpointExecutionService`

Hinweis: Schreibt Ergebnisse nur in `IActivityLogService`; keine persistente History-Speicherung.
