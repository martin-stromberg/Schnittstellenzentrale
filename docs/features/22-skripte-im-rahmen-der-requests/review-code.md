# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### ScriptResponseData.cs (ScriptResponseData)

- **Doppelter Code — `AsXml` dupliziert interne Konvertierungslogik** — `ScriptResponseData.AsXml()` (Zeilen 26–31) delegiert bereits an `ScriptRequestData.ConvertXmlToObject`, wie `AsJson()` an `ScriptRequestData.ConvertJsonElement` delegiert. Die Methode ist also korrekt. Kein Befund.

### EndpointScriptRunner.cs (EndpointScriptRunner)

- **Doppelter Code — `BuildBodyObject`-Überladungen mit identischem Rumpf** — `BuildBodyObject(Engine, ScriptRequestData)` (Zeilen 160–161) und `BuildBodyObject(Engine, ScriptResponseData)` (Zeilen 163–164) sind je einzeilige Wrapper, die beide an `BuildBodyObjectCore` delegieren. Die tatsächliche Logik ist in `BuildBodyObjectCore` (Zeilen 166–187) zentralisiert. Kein Befund.

- **Fehlerbehandlung — `sz.execute` blockiert den Jint-Thread** — Zeile 69 ruft `Task.Run(() => context.ExecuteEndpoint(name)).GetAwaiter().GetResult()` auf. Das ist ein synchroner Wait auf einem Thread-Pool-Thread, der aus einem JavaScript-Skript heraus ausgelöst wird. Jint ist single-threaded; wird `ExecuteEndpoint` selbst wiederum ein Skript ausführen, das `sz.execute` aufruft, ist Deadlock-Potenzial vorhanden (je nach Ausführungskontext). Außerdem kann `GetResult()` eine `AggregateException` werfen, die Jint nicht als `JavaScriptException` abfangen kann, sodass die Fehlerursache im Catch-Block von `ExecuteAsync` verloren geht.

  Empfehlung: Die Exception aus `GetResult()` explizit entpacken (`AggregateException.GetBaseException()`) und als aussagekräftige Jint-Exception weiterwerfen, oder den Fehlerfall von `ExecuteEndpoint` direkt im `resultObj` als `success: false` zurückgeben und den synchronen Wait auf eine `Result`-Eigenschaft des Tasks beschränken, die keine weitere Skriptausführung enthält.

### EndpointExecutionService.cs (EndpointExecutionService)

- **Fehlerbehandlung — Fehlermeldung des Post-Request-Skripts kann `result.Success` nicht mehr auf `false` setzen** — Zeilen 115–117: Bei einem Post-Skript-Fehler wird `result.ErrorMessage` gesetzt, aber `result.Success` bleibt auf dem Wert des HTTP-Calls (ggf. `true`). Ein Post-Skript-Fehler ist damit für den Aufrufer nicht als Fehler erkennbar, wenn der HTTP-Call erfolgreich war.

  Empfehlung: Bei `!postResult.Success` auch `result.Success = false` setzen.

- **Kopplung — `BuildScriptContext` enthält inline die gesamte `ExecuteEndpoint`-Logik** — Das Lambda in `BuildScriptContext` (Zeilen 147–163) ist 17 Zeilen lang und enthält Repository-Lookup, Fehlerprüfung und rekursiven Service-Aufruf. Diese Logik könnte in eine private Methode `ExecuteEndpointByNameAsync(int applicationId, string name, Dictionary<int, int> callDepth)` ausgelagert werden.

  Empfehlung: Das Lambda in eine eigene private Methode extrahieren, um `BuildScriptContext` auf die reine Datenzusammenstellung zu reduzieren.

### EndpointExecutionServiceTests.cs (EndpointExecutionServiceTests)

- **Doppelter Code — Endpunkt-Erstellungs-Hilfsmethoden** — Die Testklasse enthält fünf separate Factory-Methoden: zwei Überladungen von `CreateEndpoint`, `CreateEndpointWithHeaders`, `CreateEndpointWithBody` sowie inline konstruierte `Endpoint`-Objekte in mehreren Tests (z. B. Zeilen 675–688, 722–734, 755–768, 796–808, 843–858, 867–882, 943–946). Der Großteil der Felder ist immer gleich (Id=1, Name="Test", ApplicationId=1, Application=CreateApp()). Tests, die Skript-Eigenschaften setzen, wiederholen diesen Boilerplate fünfmal.

  Empfehlung: Eine einzige flexible `CreateEndpoint`-Überladung einführen, die optionale Parameter für `PreRequestScript`, `PostRequestScript`, `Headers`, `QueryParameters`, `RelativePath` und `Body` entgegennimmt, und die inline-Konstruktionen ersetzen.

- **Testqualität — `SzExecute_RekursionsschutzGreiftBeimDrittenAufruf` testet Implementierungsdetail** — Der Test erstellt sowohl einen Mock-Service (über `CreateService`, Zeilen 892–895) als auch einen `realService` mit echtem `EndpointScriptRunner` (Zeilen 907–913), verwendet aber nur `realService`. Der zuvor erstellte Mock-Service wird niemals benutzt (toter Code in Testaufbau).

  Empfehlung: Den unbenutzten `(service, _) = CreateService(...)` Block (Zeilen 892–895) entfernen.

- **Testqualität — `PreScript_SetsEnvironmentVariable_VariableAvailableInRequest` prüft nur Skript-Aufruf, nicht Auswirkung** — Der Test verifiziert (Zeile 692), dass `ExecuteAsync` mit dem `PreRequestScript`-String aufgerufen wurde, testet aber nicht die eigentliche Anforderung: dass eine im Pre-Skript gesetzte Variable im nachfolgenden HTTP-Request verwendet wird. Der Testname beschreibt mehr als der Test prüft.

  Empfehlung: Entweder den Testnamen auf das tatsächlich geprüfte Verhalten anpassen, oder den Test erweitern, um zu prüfen, dass `ActiveVariables` nach dem Skript-Aufruf die geänderte Variable enthält und diese in der ausgehenden Anfrage verwendet wird.

## Geprüfte Dateien

- `src/Schnittstellenzentrale.Core/Models/ScriptExecutionResult.cs`
- `src/Schnittstellenzentrale.Core/Models/ScriptContext.cs`
- `src/Schnittstellenzentrale.Core/Models/ScriptRequestData.cs`
- `src/Schnittstellenzentrale.Core/Models/ScriptResponseData.cs`
- `src/Schnittstellenzentrale.Core/Interfaces/IEndpointScriptRunner.cs`
- `src/Schnittstellenzentrale.Core/Interfaces/IEndpointRepository.cs`
- `src/Schnittstellenzentrale.Core/Models/Endpoint.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/EndpointScriptRunner.cs`
- `src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs`
- `src/Schnittstellenzentrale.Infrastructure/Repositories/EndpointRepository.cs`
- `src/Schnittstellenzentrale.Infrastructure/Repositories/ModelUpdateExtensions.cs`
- `src/Schnittstellenzentrale.Infrastructure/Data/Migrations/20260525063557_AddScriptFieldsToEndpoint.cs`
- `src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor`
- `src/Schnittstellenzentrale.Tests/Services/EndpointExecutionServiceTests.cs`
- `src/Schnittstellenzentrale.Tests/Integration/EndpointExecutionIntegrationTests.cs`
