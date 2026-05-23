# Logik

## `SystemEntryInitializer`
Datei: `src/Schnittstellenzentrale/SystemEntryInitializer.cs`

Statische Hilfsklasse, die vor `app.Run()` in `Program.cs` aufgerufen wird. Sie ist kein `IHostedService`, sondern wird explizit mit `await SystemEntryInitializer.InitializeAsync(app.Services, builder.Configuration)` aufgerufen. Löst Scoped-Dienste selbst über `services.CreateScope()` auf.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `InitializeAsync(IServiceProvider, IConfiguration)` | `public static async` | Legt Systemgruppe und Systemanwendung an, falls nicht vorhanden, oder aktualisiert deren URLs. Fehler werden per `Log.Error` (Serilog) abgefangen und geloggt; kein Propagieren. |

Besonderheit: Setzt `InterfaceUrl` der Systemanwendung auf `{Api:BaseUrl}/swagger/v1/swagger.json` — genau die URL, die der `SystemEndpointSyncService` später per HTTP abrufen soll.

## `SwaggerImportService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/SwaggerImportService.cs`

Registriert als `Scoped` in `Program.cs` (`builder.Services.AddScoped<ISwaggerImportService, SwaggerImportService>()`).

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `ImportAsync(Application)` | `public async` | Ruft die Swagger-Definition per HTTP ab, parst sie mit `OpenApiJsonReader`, baut eine Liste importierter `Endpoint`-Objekte auf und delegiert den Vergleich an `ImportDiffCalculator.Calculate`. Bei HTTP-Fehler: Rückgabe von `ImportDiff` mit `ErrorMessage`. Bei Parse-Fehler: Rückgabe von `ImportDiff` mit `ErrorMessage`. |
| `ApplyDiffAsync(ImportDiff)` | `public async` | Wendet alle drei Diff-Kategorien an: `NewEndpoints` → `AddEndpointAsync`, `ChangedEndpoints` → `UpdateEndpointAsync`, `RemovedEndpoints` → `DeleteEndpointAsync`. **Hinweis:** Diese Methode verarbeitet auch `ChangedEndpoints` und ist daher für den `SystemEndpointSyncService` nicht direkt verwendbar — der neue Service muss selektiv nur `NewEndpoints` und `RemovedEndpoints` verarbeiten. |
| `MapHttpMethod(string)` | `private` | Konvertiert HTTP-Methodenstrings aus der Swagger-Definition in den `HttpMethod`-Enum. |

Abonnierte Events: keine
Publizierte Events: keine

## `ImportDiffCalculator`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ImportDiffCalculator.cs`

Interne statische Klasse, ausschließlich von `SwaggerImportService.ImportAsync` aufgerufen.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `Calculate(IList<Endpoint>, IList<Endpoint>)` | `internal static` | Vergleicht bestehende und importierte Endpunkte anhand des Schlüssels `Method:RelativePath`. Befüllt `NewEndpoints`, `ChangedEndpoints` und `RemovedEndpoints`. Bei duplizierten Schlüsseln wird `ImportDiff` mit `ErrorMessage` zurückgegeben. |
| `TryBuildDictionary(IEnumerable<Endpoint>, out Dictionary<string, Endpoint>, out string?)` | `private static` | Baut ein Dictionary `{BuildKey → Endpoint}` auf; gibt `false` und den duplizierten Schlüssel zurück, falls ein Duplikat gefunden wird. |
| `BuildKey(Endpoint)` | `private static` | Bildet den Identifikationsschlüssel als `"{Method}:{RelativePath}"`. |
| `HasChanged(Endpoint, Endpoint)` | `private static` | Vergleicht `Name`, `Body` und `AuthenticationType` zwischen bestehendem und importiertem Endpunkt. |
| `MergeExistingIdentity(Endpoint, Endpoint)` | `private static` | Erzeugt einen neuen `Endpoint` mit den Feldern des importierten Endpunkts, übernimmt aber `Id`, `EndpointGroupId`, `RowVersion`, `Headers` und `QueryParameters` vom bestehenden Endpunkt. |

## `EndpointRepository`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/EndpointRepository.cs`

Registriert als `Scoped` in `Program.cs` (`builder.Services.AddScoped<IEndpointRepository, EndpointRepository>()`).

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `GetEndpointsAsync(int)` | `public async` | Lädt alle Endpunkte einer Anwendung inkl. `Headers`, `QueryParameters` und `EndpointGroup` per `AsNoTracking`. |
| `AddEndpointAsync(Endpoint)` | `public async` | Speichert einen neuen Endpunkt und detacht ihn anschließend. |
| `DeleteEndpointAsync(int)` | `public async` | Löscht Endpunkt per ID über `DeleteByIdAsync`. |
| `UpdateEndpointAsync(Endpoint)` | `public async` | Aktualisiert einen bestehenden Endpunkt und detacht ihn anschließend. |

## `ApplicationRepository`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/ApplicationRepository.cs`

Registriert als `Scoped` in `Program.cs` (`builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>()`).

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `GetSystemGroupAsync()` | `public async` | Liefert die erste `ApplicationGroup` mit `IsSystem == true`, inkl. ihrer `Applications`. Gibt `null` zurück, wenn keine Systemgruppe existiert. |

## `Program.cs`
Datei: `src/Schnittstellenzentrale/Program.cs`

Relevante DI-Registrierungen für den geplanten `SystemEndpointSyncService`:

| Dienst | Lifetime | Registriert als |
|---|---|---|
| `IApplicationRepository` | `Scoped` | `ApplicationRepository` |
| `IEndpointRepository` | `Scoped` | `EndpointRepository` |
| `ISwaggerImportService` | `Scoped` | `SwaggerImportService` |

Kein `AddHostedService`-Aufruf vorhanden. `SystemEntryInitializer.InitializeAsync` wird direkt vor `app.Run()` aufgerufen — nicht als `IHostedService`.
