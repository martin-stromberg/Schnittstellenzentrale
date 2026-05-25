# Logik

## `EndpointScriptRunner`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/EndpointScriptRunner.cs`

Implementiert `IEndpointScriptRunner`. Führt JavaScript-Skripte über Jint aus und stellt das `sz`-API-Objekt bereit.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `ExecuteAsync(string script, ScriptContext context)` | `public` | Führt ein Skript aus; fängt Timeout-, JavaScript- und allgemeine Exceptions ab |
| `RegisterSzObject(Engine engine, ScriptContext context)` | `private static` | Registriert das `sz`-Objekt mit `environment`, `request`, `response` und `execute` |
| `BuildEnvironmentObject(Engine engine, ScriptContext context)` | `private static` | Erstellt das `sz.environment`-Objekt mit den Lambdas `get` und `set` |
| `BuildRequestObject(Engine engine, ScriptRequestData request)` | `private static` | Erstellt das `sz.request`-Objekt |
| `BuildResponseObject(Engine engine, ScriptResponseData response)` | `private static` | Erstellt das `sz.response`-Objekt |
| `BuildHeadersObject(Engine engine, IEnumerable<KeyValuePair<string, string>> headers)` | `private static` | Erstellt ein Headers-Objekt für `sz.request.headers` / `sz.response.headers` |
| `BuildBodyObject(Engine engine, ScriptRequestData request)` | `private static` | Erstellt das `sz.request.body`-Objekt |
| `BuildBodyObject(Engine engine, ScriptResponseData response)` | `private static` | Erstellt das `sz.response.body`-Objekt |
| `BuildBodyObjectCore(Engine engine, Func<object?> getJson, Func<object?> getXml, string? rawValue)` | `private static` | Gemeinsame Implementierung für request/response body |

### Kernstelle: `sz.environment.set`-Lambda (in `BuildEnvironmentObject`)

Das Lambda liest `context.EnvironmentService.ActiveEnvironment` und `context.EnvironmentService.ActiveVariables`, baut eine neue `updatedVariables`-Dictionary auf, konstruiert daraus neue `EnvironmentVariable`-Objekte **ohne `Id` und ohne `IsValueMasked`** und ruft `context.EnvironmentService.SetActiveEnvironment(updatedEnv)` auf.

Nach diesem `SetActiveEnvironment`-Aufruf endet das Lambda — es findet **keine Datenbankpersistierung** statt.

### Analogie: `sz.execute`-Lambda (blockierender Async-Aufruf)

Das `sz.execute`-Lambda ruft `Task.Run(() => context.ExecuteEndpoint(name)).GetAwaiter().GetResult()` — dies ist das bestehende Muster für blockierenden async-Aufruf in einem synchronen Jint-Lambda.

---

## `ActiveEnvironmentService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ActiveEnvironmentService.cs`

Implementiert `IActiveEnvironmentService`. Scoped-Service.

| Methode / Eigenschaft | Sichtbarkeit | Kurzbeschreibung |
|----------------------|-------------|------------------|
| `ActiveEnvironment` | `public` (get, private set) | Gibt die aktuell aktive `SystemEnvironment` zurück |
| `ActiveVariables` | `public` (get, private set) | Materialisiertes Dictionary der aktiven Variablen |
| `OnActiveEnvironmentChanged` | `public event Action?` | Wird nach jeder Änderung ausgelöst |
| `SetActiveEnvironment(SystemEnvironment? environment)` | `public` | Setzt `ActiveEnvironment`, baut `ActiveVariables` neu auf und feuert `OnActiveEnvironmentChanged` |

Publizierte Events: `OnActiveEnvironmentChanged`

---

## `EndpointExecutionService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs`

Implementiert `IEndpointExecutionService`. Führt Endpunkte aus und ruft Pre-/Post-Request-Skripte auf.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `ExecuteAsync(Endpoint endpoint)` | `public` | Öffentlicher Einstiegspunkt; delegiert an private Überladung mit leerem `callDepth` |
| `ExecuteAsync(Endpoint endpoint, Dictionary<int, int> callDepth)` | `private` | Führt Pre-Skript, HTTP-Request und Post-Skript aus |
| `BuildScriptContext(Endpoint endpoint, Dictionary<int, int> callDepth, ScriptResponseData? response)` | `private` | Erzeugt den `ScriptContext`; befüllt `EnvironmentService`, `Request`, `Response`, `CallDepth` und `ExecuteEndpoint` |
| `ExecuteWithAuthAsync(Endpoint endpoint)` | `private` | HTTP-Ausführung ohne Impersonation |
| `ExecuteImpersonatedAsync(Endpoint endpoint)` | `private` | HTTP-Ausführung mit Windows-Impersonation |
| `SendAndBuildResultAsync(HttpClient, Endpoint, HttpRequestMessage)` | `private static` | Sendet Request, misst Zeit, ruft `BuildResult` auf |
| `BuildResult(Endpoint, HttpResponseMessage, long)` | `private static` | Baut `EndpointExecutionResult` aus der HTTP-Antwort auf |
| `BuildRequest(Endpoint endpoint)` | `private` | Erstellt `HttpRequestMessage` mit Platzhalterauflösung |
| `ResolvePlaceholders(string input, IReadOnlyDictionary<string, string> variables)` | `private static` | Ersetzt `{{name}}`-Platzhalter durch Umgebungsvariablenwerte |
| `ExecuteEndpointByNameAsync(int applicationId, string name, Dictionary<int, int> callDepth)` | `private` | Sucht Endpunkt nach Name und führt ihn aus (Callback für `sz.execute`) |
| `ApplyAuthentication(HttpRequestMessage, Endpoint)` | `private` | Wendet Basic/Bearer-Auth an |

`BuildScriptContext` injiziert derzeit **kein** `ISystemEnvironmentRepository` in den `ScriptContext` — das ist gemäß Anforderung noch offen.

---

## `SystemEnvironmentRepository`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/SystemEnvironmentRepository.cs`

Implementiert `ISystemEnvironmentRepository`. EF-Core-Implementierung mit `IDbContextFactory<AppDbContext>`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `GetEnvironmentsAsync(StorageMode mode, string? owner)` | `public` | Gibt alle Umgebungen gefiltert nach Modus und Besitzer zurück |
| `GetByIdAsync(int id)` | `public` | Gibt eine einzelne Umgebung mit Variablen zurück |
| `AddAsync(SystemEnvironment)` | `public` | Fügt eine neue Umgebung hinzu; setzt bei User-Modus den Owner |
| `UpdateAsync(SystemEnvironment)` | `public` | Aktualisiert Umgebung und Variablen; berücksichtigt `Id`, `IsValueMasked` und Delete/Insert/Update |
| `DeleteAsync(int id)` | `public` | Löscht eine Umgebung (Cascade auf Variablen) |
| `ApplyOwnerFilter(IQueryable<SystemEnvironment>, StorageMode, string?)` | `private static` | Filtert nach Modus und Besitzer |

`UpdateAsync` ist die zentrale Methode für die geplante Persistierung. Sie liest das bestehende Objekt aus der DB, gleicht Variablen ab und schreibt alles per `SaveChangesAsync` zurück. Variablen ohne `Id == 0` werden als neu eingefügt, vorhandene werden nach `Id` abgeglichen.
