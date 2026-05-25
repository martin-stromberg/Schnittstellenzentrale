# Logikklassen

## `SwaggerImportService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/SwaggerImportService.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `ImportAsync(Application application)` | `public` | Ruft die Swagger-Definition von `application.InterfaceUrl` ab, parst sie mit `OpenApiJsonReader` und erzeugt `Endpoint`-Instanzen (nur `Name`, `Method`, `RelativePath`, `ApplicationId`). Ruft `ImportDiffCalculator.Calculate` auf und gibt ein `ImportDiff` zurück. |
| `ApplyDiffAsync(ImportDiff diff)` | `public` | Persistiert den Diff über `IEndpointRepository`: neue Endpunkte werden angelegt, geänderte aktualisiert, entfernte gelöscht. |
| `MapHttpMethod(string method)` | `private` | Wandelt den HTTP-Methoden-String aus der OpenAPI-Definition in den `HttpMethod`-Enum um. |

Injizierte Abhängigkeiten: `IHttpClientFactory`, `IEndpointRepository`, `ILogger<SwaggerImportService>`

Nicht injiziert: `ICredentialService` — ist derzeit **nicht** als Abhängigkeit vorhanden.

Hinweis: `ImportAsync` liest aktuell **keine** OpenAPI-Erweiterungsfelder (`operation.Value.Extensions`) aus. `PreRequestScript`, `PostRequestScript` und `AuthenticationType` werden beim Import nicht belegt.

---

## `ImportDiffCalculator`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ImportDiffCalculator.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `Calculate(IList<Endpoint> existing, IList<Endpoint> imported)` | `internal static` | Vergleicht bestehende und importierte Endpunkte anhand des Schlüssels `{Method}:{RelativePath}` und befüllt `ImportDiff`. |
| `TryBuildDictionary(...)` | `private static` | Baut ein Dictionary nach Schlüssel; gibt `false` bei Duplikat zurück. |
| `BuildKey(Endpoint endpoint)` | `private static` | Erzeugt den Vergleichsschlüssel: `"{Method}:{RelativePath}"`. |
| `HasChanged(Endpoint existing, Endpoint imported)` | `private static` | Prüft auf Änderungen bei `Name`, `Body` und `AuthenticationType`. `PreRequestScript` und `PostRequestScript` werden **nicht** verglichen. |
| `MergeExistingIdentity(Endpoint existing, Endpoint imported)` | `private static` | Erstellt einen neuen `Endpoint` mit der Identität (Id, RowVersion, EndpointGroupId, Headers, QueryParameters) des bestehenden und den Felddaten des importierten. `PreRequestScript` und `PostRequestScript` werden im Merge **nicht** übertragen. |

---

## `EndpointExecutionService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `ExecuteAsync(Endpoint endpoint)` | `public` | Öffentlicher Einstiegspunkt; delegiert an die private Überladung mit leerem `callDepth`. |
| `ExecuteAsync(Endpoint endpoint, Dictionary<int,int> callDepth)` | `private` | Führt Pre-Request-Skript, HTTP-Request und Post-Request-Skript aus; enthält Rekursionsschutz. |
| `BuildScriptContext(...)` | `private` | Erzeugt den `ScriptContext` mit Request-Daten, Response-Daten und `ExecuteEndpoint`-Callback. |
| `ExecuteWithAuthAsync(Endpoint endpoint)` | `private` | Führt den HTTP-Request mit dem konfigurierten Authentifizierungstyp aus. |
| `ExecuteImpersonatedAsync(Endpoint endpoint)` | `private` | Führt den Request unter Windows-Impersonation aus. |
| `SendAndBuildResultAsync(...)` | `private static` | Sendet die HTTP-Anfrage und misst die Laufzeit. |
| `BuildResult(...)` | `private static` | Erzeugt das `EndpointExecutionResult` aus dem HTTP-Response. |
| `BuildRequest(Endpoint endpoint)` | `private` | Baut das `HttpRequestMessage`-Objekt; löst Platzhalter in URL, Headern und Body auf. |
| `ResolvePlaceholders(string input, ...)` | `private static` | Ersetzt `{{name}}`-Platzhalter durch Werte aus `ActiveVariables`. |
| `ExecuteEndpointByNameAsync(int applicationId, string name, ...)` | `private` | Sucht einen Endpunkt per Name und führt ihn aus (für `sz.execute`). |
| `ApplyAuthentication(HttpRequestMessage request, Endpoint endpoint)` | `private` | Liest den Bearer-Token oder Basic-Credentials aus dem `ICredentialService` und setzt den `Authorization`-Header. Bei `BearerToken` wird der Token-Wert über `ResolvePlaceholders` aufgelöst, bevor er gesetzt wird. |

Injizierte Abhängigkeiten: `IHttpClientFactory`, `IHealthCheckService`, `ICredentialService`, `IActiveEnvironmentService`, `IEndpointScriptRunner`, `IEndpointRepository`

---

## `EndpointScriptRunner`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/EndpointScriptRunner.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `ExecuteAsync(string script, ScriptContext context)` | `public` | Führt das JavaScript-Skript in einer Jint-Engine aus; Timeout 5000 ms, Speicherlimit 4 MB. Gibt `ScriptExecutionResult` zurück. |
| `RegisterSzObject(Engine engine, ScriptContext context)` | `private static` | Registriert das `sz`-API-Objekt (mit `environment`, `request`, `response`, `execute`) in der Engine. |
| `BuildEnvironmentObject(...)` | `private static` | Erzeugt `sz.environment` mit `get`/`set`-Funktionen. |
| `BuildRequestObject(...)` | `private static` | Erzeugt `sz.request` mit `url`, `method`, `body`, `headers`. |
| `BuildResponseObject(...)` | `private static` | Erzeugt `sz.response` mit `body`, `headers`. |
| `BuildHeadersObject(...)` | `private static` | Erzeugt ein JS-Objekt aus einem Header-Dictionary. |
| `BuildBodyObject(Engine, ScriptRequestData)` | `private static` | Erzeugt das Body-Objekt für den Request. |
| `BuildBodyObject(Engine, ScriptResponseData)` | `private static` | Erzeugt das Body-Objekt für die Response. |
| `BuildBodyObjectCore(...)` | `private static` | Kern-Implementierung mit `asJson()`, `asXml()` und `raw`. |
