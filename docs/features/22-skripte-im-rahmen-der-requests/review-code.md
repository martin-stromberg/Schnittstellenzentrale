# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### ScriptContext.cs (ScriptContext)

- **Fehlende Kapselung / Kopplung** — `ScriptContext` ist ein reines Datentransfer-Objekt mit sieben öffentlichen Properties, darunter zwei Services (`ISystemEnvironmentRepository`, `ISignalRNotificationService`), die nur von `EndpointScriptRunner` benötigt werden. Dadurch trägt `ScriptContext` zwei verschiedene Verantwortlichkeiten: Eingabedaten bündeln und Infrastruktur-Abhängigkeiten für den Runner durchreichen. Der Kommentar „Nullable, weil `ScriptContext` auch außerhalb von `EndpointScriptRunner` erzeugt werden kann" bestätigt das Problem explizit.

  Empfehlung: Repository und SignalRNotificationService aus `ScriptContext` entfernen und stattdessen per Konstruktor-Injektion in `EndpointScriptRunner` einbringen. `ScriptContext` bleibt dann ein reines Eingabe-DTO ohne Infrastruktur-Referenzen.

### EndpointScriptRunner.cs (BuildEnvironmentObject)

- **God-Methode / Fehlende Kapselung** — Die statische Methode `BuildEnvironmentObject` (Zeilen 102–156) enthält innerhalb des `set`-Lambdas vollständige Domänenlogik: Dictionary-Merge, Neuaufbau der `EnvironmentVariable`-Liste, Konstruktion eines neuen `SystemEnvironment`-Objekts sowie den Aufruf von `PersistVariable`. Diese Logik ist konzeptuell klar von der Registrierung des JS-Objekts getrennt und umfasst ca. 40 Zeilen innerhalb des Lambdas.

  Empfehlung: Den Inhalt des `set`-Lambdas in eine dedizierte private Methode `ApplyEnvironmentSet(string name, string value, ScriptContext context)` auslagern, die dann im Lambda nur noch aufgerufen wird.

### ScriptRequestData.cs / ScriptResponseData.cs (Doppelter Code)

- **Doppelter Code** — `ScriptResponseData.AsJson()` und `ScriptResponseData.AsXml()` sind inhaltlich identisch mit den Methoden in `ScriptRequestData`, rufen aber `ScriptRequestData.ConvertJsonElement` und `ScriptRequestData.ConvertXmlToObject` als statische `internal`-Hilfsmethoden auf der falschen Klasse auf. Die Konvertierungslogik ist sachlich an `ScriptRequestData` gebunden, obwohl sie von beiden Klassen genutzt wird.

  Empfehlung: `ConvertJsonElement` und `ConvertXmlToObject` in eine neue interne Hilfsklasse (z. B. `ScriptBodyParser`) im selben Namespace verschieben. Beide Klassen rufen diese dann auf, ohne dass eine von der anderen abhängt.

### EndpointExecutionService.cs (BuildScriptContext)

- **Fehlende Kapselung / Inkonsistenz** — In `BuildScriptContext` (Zeile 145) wird `ScriptRequestData.Url` als `baseUrl.TrimEnd('/') + "/" + endpoint.RelativePath.TrimStart('/')` gebaut — ohne Platzhalter-Auflösung (`{{...}}`). In `BuildRequest` (ab Zeile 220) werden Platzhalter dagegen vollständig aufgelöst. Das Skript erhält somit eine andere URL als der tatsächlich gesendete HTTP-Request.

  Empfehlung: In `BuildScriptContext` `ResolvePlaceholders` auf `baseUrl` und `relativePath` anwenden, bevor die URL in `ScriptRequestData.Url` eingetragen wird, analog zur Logik in `BuildRequest`.

### EndpointExecutionServiceTests.cs (CreateEndpoint)

- **Testqualität — implizite Parameterüberschreibung** — Die Factory-Methode `CreateEndpoint` (Zeilen 23–46) enthält die Logik `Method = body != null ? Core.Enums.HttpMethod.POST : method`. Ein Test, der `body` setzt, überschreibt damit implizit den `method`-Parameter, ohne dass ein Testautor das ohne Lesen der Factory-Methode erkennt.

  Empfehlung: Diese implizite Überschreibung entfernen. Der `method`-Parameter soll ausschließlich `method` bestimmen. Tests, die POST mit Body benötigen, geben `method: Core.Enums.HttpMethod.POST` explizit an.

### EndpointScriptRunnerTests.cs (Hilfsmethoden)

- **Doppelter Code** — `CreateEnvironmentRepositoryMock()` (Zeilen 41–46) und `CreateEnvironmentRepositoryMockCapturing()` (Zeilen 49–55) richten dasselbe Setup ein und unterscheiden sich nur im Rückgabetyp (`ISystemEnvironmentRepository` vs. `Mock<ISystemEnvironmentRepository>`). Gleiches gilt für `CreateSignalRNotificationServiceMock()` und `CreateSignalRNotificationServiceMockCapturing()` (Zeilen 57–69).

  Empfehlung: Die vier Methoden auf zwei zusammenfassen, die jeweils `Mock<T>` zurückgeben. Aufrufstellen, die nur `.Object` benötigen, rufen `.Object` auf der zurückgegebenen Mock-Instanz auf.

## Geprüfte Dateien

- `src/Schnittstellenzentrale.Core/Interfaces/IEndpointRepository.cs`
- `src/Schnittstellenzentrale.Core/Interfaces/IEndpointScriptRunner.cs`
- `src/Schnittstellenzentrale.Core/Interfaces/ISignalRNotificationService.cs`
- `src/Schnittstellenzentrale.Core/Interfaces/ISystemEnvironmentRepository.cs`
- `src/Schnittstellenzentrale.Core/Models/Endpoint.cs`
- `src/Schnittstellenzentrale.Core/Models/EnvironmentVariable.cs`
- `src/Schnittstellenzentrale.Core/Models/ImportDiff.cs`
- `src/Schnittstellenzentrale.Core/Models/ScriptContext.cs`
- `src/Schnittstellenzentrale.Core/Models/ScriptExecutionResult.cs`
- `src/Schnittstellenzentrale.Core/Models/ScriptRequestData.cs`
- `src/Schnittstellenzentrale.Core/Models/ScriptResponseData.cs`
- `src/Schnittstellenzentrale.Core/Models/SystemEnvironment.cs`
- `src/Schnittstellenzentrale.Infrastructure/Repositories/EndpointRepository.cs`
- `src/Schnittstellenzentrale.Infrastructure/Repositories/ModelUpdateExtensions.cs`
- `src/Schnittstellenzentrale.Infrastructure/Repositories/SystemEnvironmentRepository.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/ActiveEnvironmentService.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/EndpointScriptRunner.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/ImportDiffCalculator.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/SignalRNotificationService.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/SwaggerImportService.cs`
- `src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor`
- `src/Schnittstellenzentrale/Filters/SzExtensionsOperationFilter.cs`
- `src/Schnittstellenzentrale/SystemEndpointSyncService.cs`
- `src/Schnittstellenzentrale.Tests/Integration/EndpointExecutionIntegrationTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/EndpointExecutionServiceTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/EndpointScriptRunnerTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/SwaggerImportServiceTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/SystemEndpointSyncServiceTests.cs`
