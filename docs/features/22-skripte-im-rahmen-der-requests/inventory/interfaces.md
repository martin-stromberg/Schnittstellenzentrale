# Interfaces

## `IActiveEnvironmentService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IActiveEnvironmentService.cs`

| Member | Typ / Parameter | Rückgabewert | Zweck |
|--------|----------------|--------------|-------|
| `ActiveEnvironment` | — | `SystemEnvironment?` | Aktuell aktive Umgebung |
| `ActiveVariables` | — | `IReadOnlyDictionary<string, string>` | Materialisierte Variablen der aktiven Umgebung |
| `OnActiveEnvironmentChanged` | — | `event Action?` | Wird nach jeder Umgebungsänderung ausgelöst |
| `SetActiveEnvironment` | `SystemEnvironment? environment` | `void` | Setzt die aktive Umgebung und aktualisiert `ActiveVariables` |

Das Interface enthält ausschließlich eine synchrone Methode `SetActiveEnvironment`. Eine asynchrone Variante (z. B. `SetActiveEnvironmentAsync`) existiert noch nicht.

---

## `ISystemEnvironmentRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ISystemEnvironmentRepository.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `GetEnvironmentsAsync` | `StorageMode mode, string? owner` | `Task<IList<SystemEnvironment>>` | Gibt alle passenden Umgebungen zurück |
| `GetByIdAsync` | `int id` | `Task<SystemEnvironment?>` | Gibt eine Umgebung per ID zurück |
| `AddAsync` | `SystemEnvironment systemEnvironment` | `Task<SystemEnvironment>` | Persistiert eine neue Umgebung |
| `UpdateAsync` | `SystemEnvironment systemEnvironment` | `Task<SystemEnvironment>` | Aktualisiert eine bestehende Umgebung |
| `DeleteAsync` | `int id` | `Task` | Löscht eine Umgebung |

`UpdateAsync` ist die Methode, die gemäß Anforderung in `EndpointScriptRunner` aufgerufen werden soll, wenn eine Systemumgebung aktiv ist.

---

## `IEndpointScriptRunner`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IEndpointScriptRunner.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `ExecuteAsync` | `string script, ScriptContext context` | `Task<ScriptExecutionResult>` | Führt ein JavaScript-Skript im gegebenen Kontext aus |
