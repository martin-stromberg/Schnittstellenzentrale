# Datenmodell-Bestandsaufnahme

## `ScriptContext`
Datei: `src/Schnittstellenzentrale.Core/Models/ScriptContext.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `EnvironmentService` | `IActiveEnvironmentService` | Aktiver Umgebungsservice für `sz.environment`-Zugriff im Skript |
| `Request` | `ScriptRequestData` | Snapshot der Request-Daten |
| `Response` | `ScriptResponseData?` | Snapshot der HTTP-Antwort; nur im Post-Request-Skript gesetzt |
| `ExecuteEndpoint` | `Func<string, Task<EndpointExecutionResult>>` | Callback für `sz.execute(name)` |
| `CallDepth` | `Dictionary<int, int>` | Rekursionsschutz: Aufrufzähler pro Endpunkt-ID |

Hinweis: `ScriptContext` enthält kein `EndpointName`-Feld (offene Frage 3 der Anforderung).

---

## `ScriptExecutionResult`
Datei: `src/Schnittstellenzentrale.Core/Models/ScriptExecutionResult.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Success` | `bool` | Gibt an, ob das Skript erfolgreich ausgeführt wurde |
| `ErrorMessage` | `string?` | Fehlermeldung bei Syntaxfehler oder Runtime-Exception |

---

## `EndpointExecutionResult`
Datei: `src/Schnittstellenzentrale.Core/Models/EndpointExecutionResult.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Success` | `bool` | Ob der Request erfolgreich war |
| `StatusCode` | `int?` | HTTP-Statuscode der Antwort |
| `RequestDetails` | `string?` | Zusammenfassung: Methode + URL |
| `ResponseBody` | `string?` | Antwort-Body als String |
| `ErrorMessage` | `string?` | Fehlermeldung bei Ausnahmen |
| `ResponseHeaders` | `IDictionary<string, string>?` | Response-Header |
| `DurationMs` | `long?` | Laufzeit in Millisekunden |
| `ResponseSizeBytes` | `long?` | Größe des Response-Body in Bytes |

---

## `EnvironmentVariable`
Datei: `src/Schnittstellenzentrale.Core/Models/EnvironmentVariable.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Variablenname |
| `Value` | `string` | Variablenwert |
| `IsValueMasked` | `bool` | Gibt an, ob der Wert im UI maskiert werden soll |
| `SystemEnvironmentId` | `int` | Fremdschlüssel auf `SystemEnvironment` |
| `SystemEnvironment` | `SystemEnvironment?` | Navigationseigenschaft |

Hinweis: `IsValueMasked` ist bereits vorhanden — relevant für die Maskierungslogik in `EndpointExecutionService`.

---

## `SystemEnvironment`
Datei: `src/Schnittstellenzentrale.Core/Models/SystemEnvironment.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Name der Umgebung |
| `Mode` | `StorageMode` | Team- oder Benutzermodus |
| `Owner` | `string?` | Besitzer (Windows-Benutzername) |
| `Variables` | `ICollection<EnvironmentVariable>` | Enthaltene Variablen |
