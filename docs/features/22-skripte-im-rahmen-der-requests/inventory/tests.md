# Tests

## Testklassen

### `EndpointScriptRunnerTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/EndpointScriptRunnerTests.cs`

- `Syntaxfehler_GibtScriptExecutionResultMitErrorMessage` — Ungültiges JavaScript liefert `Success = false` mit `ErrorMessage`
- `RuntimeException_GibtScriptExecutionResultMitErrorMessage` — Geworfener JavaScript-Fehler liefert `Success = false` mit der Fehlermeldung
- `SzEnvironmentGet_LiestVariableAusActiveVariables` — `sz.environment.get` liest korrekt aus `ActiveVariables`
- `SzEnvironmentSet_AktualisiertActiveVariables` — `sz.environment.set` ruft `SetActiveEnvironment` einmal auf und übergibt die neue Variable; getestet ohne aktive Systemumgebung (`ActiveEnvironment == null`)
- `SzRequestUrl_GibtKorrekteUrlZurueck` — `sz.request.url` enthält die konfigurierte URL
- `SzRequestBodyAsJson_ParstJsonKorrekt` — `sz.request.body.asJson()` parst JSON korrekt
- `SzRequestBodyAsXml_ParstXmlKorrekt` — `sz.request.body.asXml()` parst XML korrekt
- `SzResponseBodyAsJson_ParstJsonKorrekt` — `sz.response.body.asJson()` parst JSON korrekt
- `SzResponseBodyAsXml_ParstXmlKorrekt` — `sz.response.body.asXml()` parst XML korrekt

Kein Test für `sz.environment.set` mit gesetztem `ActiveEnvironment` (d. h. der Persistierungsfall ist noch **nicht** abgedeckt).

---

### `EndpointExecutionServiceTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/EndpointExecutionServiceTests.cs`

Relevante Tests für den Skriptbereich:

- `PreScript_SetsEnvironmentVariable_VariableAvailableInRequest` — Pre-Skript kann Umgebungsvariable setzen, die dann beim HTTP-Request aufgelöst wird
- `PreScript_Fehler_BlockiertHttpRequest_FehlerMeldungImErgebnis` — Fehlerhaftes Pre-Skript verhindert HTTP-Request
- `PostScript_LiestResponseBody_SetzUmgebungsvariable` — Post-Skript erhält `ScriptContext` mit gesetzter `Response`
- `PostScript_Fehler_HttpErgebnisVorhanden_FehlerMeldungAngehaengt` — Post-Skript-Fehler wird an HTTP-Ergebnis angehängt
- `SzExecute_LoesteAusfuehrungDesZweitenEndpunktsAus` — `sz.execute` löst Ausführung eines zweiten Endpunkts aus
- `SzExecute_RekursionsschutzGreiftBeimDrittenAufruf` — Rekursionsschutz greift nach zwei Ebenen
- `SzExecute_MehrdeutigerName_GibtFehlerZurueck` — Mehrdeutiger Endpunktname liefert Fehlermeldung
- `EndpunktOhneSkript_VerhaeltSichWieBisher` — Endpunkt ohne Skript ruft `ExecuteAsync` des Runners nie auf

---

### `EndpointExecutionIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/EndpointExecutionIntegrationTests.cs`

- `ExecuteEndpoint_OwnApiWithBearerToken_ReturnsSuccess` — Echter HTTP-Request gegen den Test-Server mit Bearer-Token; `IEndpointScriptRunner` ist gemockt

Kein Integrationstest, der nach Skriptausführung den Datenbankstand der Umgebungsvariablen prüft.

---

### `SystemEnvironmentRepositoryIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/SystemEnvironmentRepositoryIntegrationTests.cs`

- `AddEnvironment_PersistsEnvironment` — Neu angelegte Umgebung ist nach `GetByIdAsync` abrufbar
- `AddEnvironment_WithDuplicateName_ThrowsConstraintException` — Doppelter Name wirft `DbUpdateException`
- `DeleteEnvironment_CascadesVariables` — Löschen der Umgebung entfernt auch die Variablen
- `GetEnvironments_WithStorageModeUser_ReturnsOnlyOwnedEnvironments` — User-Umgebungen sind Besitzer-isoliert
- `GetEnvironments_WithStorageModeTeam_ReturnsAllTeamEnvironments` — Team-Umgebungen sind für alle sichtbar
- `UpdateEnvironment_PersistsChanges` — Geänderter Name und Variablenwert werden persistiert

---

## Hilfsmethoden

### `EndpointScriptRunnerTests` (Hilfsmethoden in derselben Klasse)
- `CreateContext(IActiveEnvironmentService?, ScriptResponseData?)` — Erstellt einen `ScriptContext` mit Standard-Request-Daten und optionalem Umgebungsservice
- `CreateEmptyEnvironmentService()` — Erstellt einen gemockten `IActiveEnvironmentService` ohne aktive Umgebung und ohne Variablen
- `CreateEnvironmentServiceWithVariables(Dictionary<string, string>)` — Erstellt einen gemockten `IActiveEnvironmentService` mit gesetzter `SystemEnvironment` und Variablen

### `EndpointExecutionServiceTests` (Hilfsmethoden in derselben Klasse)
- `CreateApp()` — Erstellt eine `Application`-Testinstanz
- `CreateEndpoint(...)` — Erstellt einen `Endpoint` mit konfigurierbaren Eigenschaften inklusive `preRequestScript` und `postRequestScript`
- `CreateEmptyActiveEnvironmentMock()` — Erstellt einen Mock ohne aktive Umgebung
- `CreateScriptRunnerMock(ScriptExecutionResult)` — Erstellt einen Mock, der immer das übergebene Ergebnis zurückliefert
- `CreateEmptyEndpointRepositoryMock()` — Erstellt einen Mock, der leere Listen zurückliefert
- `CreateService(...)` — Erstellt eine `EndpointExecutionService`-Instanz mit konfigurierbaren Mock-Abhängigkeiten
- `CreateServiceCapturingUri(...)` — Erstellt einen Service, der die gesendete URI mitschneidet
- `CreateActiveEnvironmentMock(Dictionary<string, string>)` — Erstellt einen Mock mit gesetzter Umgebung und Variablen

### `SystemEnvironmentRepositoryIntegrationTests` (Hilfsmethoden in derselben Klasse)
- `ExecuteWithContextAsync(Func<SystemEnvironmentRepository, Task>, string)` — Erstellt In-Memory-DB-Kontext und führt Test mit einem Benutzer aus
- `ExecuteWithTwoUsersAsync(Func<SystemEnvironmentRepository, SystemEnvironmentRepository, Task>)` — Führt Test mit zwei unterschiedlichen Benutzern auf derselben In-Memory-DB aus
