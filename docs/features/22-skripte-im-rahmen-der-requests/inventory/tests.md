# Tests

## Testklassen

### `SwaggerImportServiceTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/SwaggerImportServiceTests.cs`

- `Import_NewSwaggerDefinition_ReturnsCorrectDiff` — Prüft, dass ein leerer Bestand und zwei Operationen in der Swagger-Definition zu zwei `NewEndpoints` im Diff führt.
- `Import_ChangedSwaggerOperation_ReturnsChangedInDiff` — Prüft, dass ein Endpunkt mit geändertem Namen im `ChangedEndpoints`-Array erscheint.
- `Import_RemovedSwaggerOperation_ReturnsRemovedInDiff` — Prüft, dass ein Endpunkt, der nicht mehr in der Swagger-Definition vorkommt, in `RemovedEndpoints` landet.

Hinweis: Es gibt **keine** Testfälle für OpenAPI-Erweiterungsfelder (`x-sz-post-request-script`, `x-sz-bearer-token` o. ä.). Diese fehlen vollständig.

---

### `EndpointScriptRunnerTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/EndpointScriptRunnerTests.cs`

- `Syntaxfehler_GibtScriptExecutionResultMitErrorMessage` — Prüft, dass ungültiges JavaScript zu `Success = false` führt.
- `RuntimeException_GibtScriptExecutionResultMitErrorMessage` — Prüft, dass ein `throw` im Skript die Fehlermeldung in `ErrorMessage` überträgt.
- `SzEnvironmentGet_LiestVariableAusActiveVariables` — Prüft, dass `sz.environment.get(name)` den Wert aus den aktiven Variablen zurückgibt.
- `SzEnvironmentSet_AktualisiertActiveVariables` — Prüft, dass `sz.environment.set(name, value)` `SetActiveEnvironment` aufruft und die Variable enthält.
- `SzRequestUrl_GibtKorrekteUrlZurueck` — Prüft, dass `sz.request.url` die korrekte URL enthält.
- `SzRequestBodyAsJson_ParstJsonKorrekt` — Prüft, dass `sz.request.body.asJson()` ein JSON-Objekt korrekt deserialisiert.
- `SzRequestBodyAsXml_ParstXmlKorrekt` — Prüft, dass `sz.request.body.asXml()` ein XML-Dokument korrekt deserialisiert.
- `SzResponseBodyAsJson_ParstJsonKorrekt` — Prüft, dass `sz.response.body.asJson()` den Response-Body korrekt deserialisiert.
- `SzResponseBodyAsXml_ParstXmlKorrekt` — Prüft, dass `sz.response.body.asXml()` den Response-Body als XML korrekt deserialisiert.

---

## Hilfsmethoden

### `SwaggerImportServiceTests`
- `CreateService(string swaggerJson, Mock<IEndpointRepository> repoMock)` — Erzeugt eine `SwaggerImportService`-Instanz mit gemocktem HTTP-Client, der das angegebene JSON zurückgibt.

### `EndpointExecutionServiceTests`
- `CreateApp()` — Erzeugt eine Testanwendung mit `BaseUrl = "http://localhost:5000"`.
- `CreateEndpoint(...)` — Erzeugt einen Testendpunkt mit konfigurierbaren Parametern inkl. `PreRequestScript` und `PostRequestScript`.
- `CreateEmptyActiveEnvironmentMock()` — Mock für `IActiveEnvironmentService` ohne aktive Variablen.
- `CreateScriptRunnerMock(ScriptExecutionResult result)` — Mock für `IEndpointScriptRunner` mit konfiguriertem Rückgabewert.
- `CreateEmptyEndpointRepositoryMock()` — Mock für `IEndpointRepository` ohne Endpunkte.

### `EndpointScriptRunnerTests`
- `CreateContext(IActiveEnvironmentService? envService, ScriptResponseData? response)` — Erzeugt einen `ScriptContext` mit Standard-Request-Daten.
- `CreateEmptyEnvironmentService()` — Mock für `IActiveEnvironmentService` ohne Variablen.
- `CreateEnvironmentServiceWithVariables(Dictionary<string, string> variables)` — Mock für `IActiveEnvironmentService` mit vorgegebenen Variablen.
