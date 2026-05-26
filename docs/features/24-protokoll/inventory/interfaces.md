# Interface-Bestandsaufnahme

## `IActiveEnvironmentService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IActiveEnvironmentService.cs`

| Methode / Eigenschaft | Parameter | Rückgabewert | Zweck |
|----------------------|-----------|--------------|-------|
| `ActiveEnvironment` | — | `SystemEnvironment?` | Aktuell aktive Umgebung |
| `ActiveVariables` | — | `IReadOnlyDictionary<string, string>` | Materialisierte Variablen-Map |
| `OnActiveEnvironmentChanged` | — | `event Action?` | Event bei Umgebungswechsel |
| `SetActiveEnvironment(SystemEnvironment?)` | `environment` | `void` | Setzt aktive Umgebung und feuert Event |

Relevanz: `ActiveEnvironment?.Variables` wird für Maskierungslogik in `EndpointExecutionService` benötigt (offene Frage 2 der Anforderung). Wird vom `ActivityLogPanel` nicht direkt verwendet, aber `IActivityLogService` kann darauf zugreifen.

---

## `IStorageModeService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IStorageModeService.cs`

| Methode / Eigenschaft | Parameter | Rückgabewert | Zweck |
|----------------------|-----------|--------------|-------|
| `CurrentMode` | — | `StorageMode` | Aktueller Speichermodus |
| `OnModeChanged` | — | `event Action?` | Event bei Modus-Wechsel |
| `SetMode(StorageMode)` | `mode` | `void` | Setzt Modus und feuert Event |

Relevanz: `SetMode` wird in `MainLayout.OnStorageModeChanged` aufgerufen — Auslöser für `ContextSwitched`-Protokolleintrag.

---

## `ICurrentUserService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ICurrentUserService.cs`

| Methode / Eigenschaft | Parameter | Rückgabewert | Zweck |
|----------------------|-----------|--------------|-------|
| `GetCurrentUserName()` | — | `string` | Gibt den aktuellen Windows-Benutzernamen zurück |

Relevanz: Kann im `ActivityLogService.Log`-Aufruf zur Diagnose mitgeführt werden.

---

## `IEndpointExecutionService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IEndpointExecutionService.cs`

| Methode / Eigenschaft | Parameter | Rückgabewert | Zweck |
|----------------------|-----------|--------------|-------|
| `ExecuteAsync(Endpoint)` | `endpoint` | `Task<EndpointExecutionResult>` | Führt einen Endpunkt aus |

---

## `IEndpointScriptRunner`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IEndpointScriptRunner.cs`

| Methode / Eigenschaft | Parameter | Rückgabewert | Zweck |
|----------------------|-----------|--------------|-------|
| `ExecuteAsync(string, ScriptContext)` | `script`, `context` | `Task<ScriptExecutionResult>` | Führt ein JavaScript-Skript aus |

---

Hinweis: `IActivityLogService` existiert noch nicht und ist vollständig neu zu erstellen mit `Entries`, `OnEntryAdded`, `Log(ActivityLogCategory, string, string?)` und `Clear()`.
