# Datenmodell

## `SystemEnvironment`
Datei: `src/Schnittstellenzentrale.Core/Models/SystemEnvironment.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename der Umgebung |
| `Mode` | `StorageMode` | Speichermodus (User oder Team) |
| `Owner` | `string?` | Besitzer bei User-Modus |
| `Variables` | `ICollection<EnvironmentVariable>` | Enthaltene Variablen |

Wird von `IActiveEnvironmentService` als `ActiveEnvironment` gehalten und bei `sz.environment.set` im `EndpointScriptRunner` neu aufgebaut.

---

## `EnvironmentVariable`
Datei: `src/Schnittstellenzentrale.Core/Models/EnvironmentVariable.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Variablenname |
| `Value` | `string` | Variablenwert |
| `IsValueMasked` | `bool` | Gibt an, ob der Wert maskiert angezeigt wird |
| `SystemEnvironmentId` | `int` | Fremdschlüssel zur `SystemEnvironment` |
| `SystemEnvironment` | `SystemEnvironment?` | Navigationseigenschaft |

Beim Neuaufbau der Variablenliste in `sz.environment.set` werden neue `EnvironmentVariable`-Objekte ohne `Id` und ohne `IsValueMasked`-Übernahme erstellt.

---

## `ScriptContext`
Datei: `src/Schnittstellenzentrale.Core/Models/ScriptContext.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `EnvironmentService` | `IActiveEnvironmentService` | Zugang zum aktiven Umgebungsservice |
| `Request` | `ScriptRequestData` | Snapshot der Request-Daten |
| `Response` | `ScriptResponseData?` | Snapshot der HTTP-Antwort (nur Post-Request-Skript) |
| `ExecuteEndpoint` | `Func<string, Task<EndpointExecutionResult>>` | Callback für `sz.execute(name)` |
| `CallDepth` | `Dictionary<int, int>` | Rekursionsschutz-Zähler pro Endpunkt-ID |

`ScriptContext` enthält derzeit **keine** Eigenschaft für `ISystemEnvironmentRepository`. Diese fehlt gemäß Anforderung noch.
