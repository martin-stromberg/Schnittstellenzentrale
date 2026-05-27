# Interfaces

## `IActiveEnvironmentService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IActiveEnvironmentService.cs`

| Member | Parameter | Rückgabewert | Zweck |
|--------|-----------|--------------|-------|
| `ActiveEnvironment` | — | `SystemEnvironment?` | Aktuell aktive Umgebung |
| `ActiveVariables` | — | `IReadOnlyDictionary<string, string>` | Materialisierte Variablen der aktiven Umgebung |
| `OnActiveEnvironmentChanged` | — | `event Action?` | Event bei Änderung der aktiven Umgebung |
| `SetActiveEnvironment` | `SystemEnvironment? environment` | `void` | Setzt die aktive Umgebung; wird von `EnvironmentSelector.OnSelectionChanged` und `MainLayout.RestoreEnvironmentFromLocalStorageAsync` aufgerufen |

---

## `ISystemEnvironmentRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ISystemEnvironmentRepository.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `GetEnvironmentsAsync` | `StorageMode mode, string? owner` | `Task<IList<SystemEnvironment>>` | Lädt gefilterte Umgebungsliste |
| `GetByIdAsync` | `int id` | `Task<SystemEnvironment?>` | Lädt einzelne Umgebung; verwendet in `MainLayout.RestoreEnvironmentFromLocalStorageAsync` |
| `AddAsync` | `SystemEnvironment` | `Task<SystemEnvironment>` | Legt neue Umgebung an |
| `UpdateAsync` | `SystemEnvironment` | `Task<SystemEnvironment>` | Aktualisiert bestehende Umgebung |
| `DeleteAsync` | `int id` | `Task` | Löscht Umgebung |
| `UpdateVariableAsync` | `int environmentId, string name, string value` | `Task` | Aktualisiert einzelne Variable |

---

## `IStorageModeService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IStorageModeService.cs`

| Member | Parameter | Rückgabewert | Zweck |
|--------|-----------|--------------|-------|
| `CurrentMode` | — | `StorageMode` | Aktuell aktiver Modus; bestimmt den `localStorage`-Schlüssel |
| `OnModeChanged` | — | `event Action?` | Event bei Moduswechsel |
| `SetMode` | `StorageMode mode` | `void` | Setzt neuen Modus; löst `OnModeChanged` aus |
