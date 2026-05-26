# Test-Bestandsaufnahme

## Testklassen

### `EndpointExecutionServiceTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/EndpointExecutionServiceTests.cs`

- `Execute_WithAuthTypeNone_SendsRequestWithoutCredentials` — Kein Authorization-Header bei `AuthenticationType.None`
- `Execute_WithAuthTypeBasic_SendsBasicAuthHeader` — Basic-Auth-Header wird gesetzt
- `Execute_WithNegotiateAuthType_UsesNegotiateHandler` — Negotiate-Client wird für Negotiate/NegotiateWithImpersonation verwendet
- `Execute_WithAuthTypeBearerToken_SendsBearerHeader` — Bearer-Header wird korrekt gesetzt
- `Execute_SetsResponseHeaders` — Response-Header werden in `EndpointExecutionResult` übernommen
- `Execute_SetsDurationMs` — `DurationMs` wird gemessen
- `Execute_SetsResponseSizeBytes` — `ResponseSizeBytes` wird berechnet
- `Execute_OnConnectionError_DoesNotCallHealthCheck` — `HttpRequestException` liefert `Success=false` ohne HealthCheck
- `BuildRequest_ErsetztPfadPlatzhalterDurchGespeicherteWerte` — `{id}` aus QueryParameter wird in Pfad ersetzt
- `BuildRequest_HaengtNurNichtPlatzhalterParameterAlsQueryStringAn` — Nur nicht-Platzhalter-Parameter landen im Query-String
- `BuildRequest_ResolvesDoubleBracePlaceholders` — `{{env}}`-Platzhalter werden aus aktiven Variablen ersetzt
- `BuildRequest_ResolvesSingleBracePlaceholdersFromQueryParameters` — `{id}` wird aus QueryParametern ersetzt
- `BuildRequest_MissingVariable_ReplacesWithEmptyString` — Fehlende Variablen ergeben leeren String
- `BuildRequest_NoActiveEnvironment_ReplacesAllDoubleBracePlaceholdersWithEmptyString` — Ohne Umgebung werden alle `{{...}}`-Platzhalter geleert
- `BuildRequest_ResolvesPlaceholdersInBaseUrl` — Platzhalter in `BaseUrl` werden aufgelöst
- `BuildRequest_ResolvesPlaceholdersInRelativePath` — Platzhalter in `RelativePath` werden aufgelöst
- `BuildRequest_ResolvesPlaceholdersInHeaderNamesAndValues` — Platzhalter in Header-Namen und -Werten werden aufgelöst
- `BuildRequest_ResolvesPlaceholdersInQueryParameterNamesAndValues` — Platzhalter in Query-Parameter-Namen und -Werten
- `BuildRequest_ResolvesPlaceholdersInBearerToken` — Platzhalter im Bearer-Token werden aufgelöst
- `BuildRequest_ResolvesDoubleBracePlaceholdersBeforeSingleBrace` — `{{...}}` wird vor `{...}` aufgelöst
- `BuildRequest_ResolvesPlaceholdersInBody` — Platzhalter im Request-Body werden aufgelöst
- `PreScript_SetsEnvironmentVariable_VariableAvailableInRequest` — Vorskript-Umgebungsvariable wirkt im Request
- `PreScript_Fehler_BlockiertHttpRequest_FehlerMeldungImErgebnis` — Fehlerhaftes Vorskript verhindert HTTP-Request
- `PostScript_LiestResponseBody_SetzUmgebungsvariable` — Post-Skript erhält Response-Body im Kontext
- `PostScript_Fehler_HttpErgebnisVorhanden_FehlerMeldungAngehaengt` — Fehlerhaftes Post-Skript hängt Fehlermeldung an
- `SzExecute_LoesteAusfuehrungDesZweitenEndpunktsAus` — `sz.execute` ruft zweiten Endpunkt auf
- `SzExecute_RekursionsschutzGreiftBeimDrittenAufruf` — Rekursionsschutz gibt Fehler zurück
- `EndpunktOhneSkript_VerhaeltSichWieBisher` — Endpunkt ohne Skript läuft ohne Skript-Aufruf
- `SzExecute_MehrdeutigerName_GibtFehlerZurueck` — Mehrdeutiger Endpunktname liefert Fehler

### `EndpointScriptRunnerTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/EndpointScriptRunnerTests.cs`

- `Syntaxfehler_GibtScriptExecutionResultMitErrorMessage` — Syntaxfehler im Skript liefert `Success=false` mit Meldung
- `RuntimeException_GibtScriptExecutionResultMitErrorMessage` — Runtime-Exception liefert `Success=false` mit Meldung
- `SzEnvironmentGet_LiestVariableAusActiveVariables` — `sz.environment.get` liest aus aktiven Variablen
- `SzEnvironmentSet_AktualisiertActiveVariables` — `sz.environment.set` ruft `SetActiveEnvironment` auf
- `SzRequestUrl_GibtKorrekteUrlZurueck` — `sz.request.url` gibt korrekte URL zurück
- `SzRequestBodyAsJson_ParstJsonKorrekt` — `sz.request.body.asJson()` parst JSON
- `SzRequestBodyAsXml_ParstXmlKorrekt` — `sz.request.body.asXml()` parst XML
- `SzResponseBodyAsJson_ParstJsonKorrekt` — `sz.response.body.asJson()` parst JSON
- `SzResponseBodyAsXml_ParstXmlKorrekt` — `sz.response.body.asXml()` parst XML
- `SzEnvironmentSet_MitAktiverSystemumgebung_PersistiertVariable` — `sz.environment.set` persistiert in DB
- `SzEnvironmentSet_OhneAktiveSystemumgebung_PersistiertNicht` — Ohne aktive Umgebung kein DB-Schreiben
- `SzEnvironmentSet_MitAktiverSystemumgebung_BenachrichtigtSignalR` — `sz.environment.set` feuert SignalR-Notify
- `SzEnvironmentSet_UebernehmtIsValueMasked_AusBestehendenVariablen` — `IsValueMasked` wird beibehalten
- `SzEnvironmentSet_UebernehmtId_AusBestehendenVariablen` — Variablen-ID wird beibehalten
- `SzEnvironmentSet_DatenbankFehler_GibtScriptExecutionResultMitFehler` — DB-Fehler liefert `Success=false`
- `SzEnvironmentSet_SignalRFehler_GibtScriptExecutionResultMitFehler` — SignalR-Fehler liefert `Success=false`

### `MainLayoutTests`
Datei: `src/Schnittstellenzentrale.Tests/Components/MainLayoutTests.cs`

- `Layout_RendertModusSelektor` — `select.form-select` ist vorhanden
- `Layout_RendertZahnradIcon` — Button mit `title='Umgebungen verwalten'` ist vorhanden
- `DisposeAsync_OhneHubConnection_WirftKeinenFehler` — Dispose ohne HubConnection wirft keine Exception

---

## Hilfsmethoden

### `EndpointExecutionServiceTests`
- `CreateApp()` — Erstellt eine Dummy-`Application`
- `CreateEndpoint(...)` — Erstellt einen konfigurierbaren Dummy-`Endpoint`
- `CreateEmptyActiveEnvironmentMock()` — Mock für `IActiveEnvironmentService` ohne Variablen
- `CreateScriptRunnerMock(ScriptExecutionResult)` — Mock für `IEndpointScriptRunner` mit definiertem Ergebnis
- `CreateEmptyEndpointRepositoryMock()` — Mock für `IEndpointRepository` mit leeren Listen
- `CreateEmptyEnvironmentRepositoryMock()` — Mock für `ISystemEnvironmentRepository`
- `CreateEmptySignalRNotificationServiceMock()` — Mock für `ISignalRNotificationService`
- `CreateService(...)` — Erstellt vollständige `EndpointExecutionService`-Instanz mit konfigurierbaren Mocks
- `CreateServiceCapturingUri(...)` — Erstellt Service mit Callback, der die gesendete URI erfasst
- `CreateActiveEnvironmentMock(Dictionary<string,string>)` — Mock mit spezifischen Variablen

### `EndpointScriptRunnerTests`
- `CreateRunner(...)` — Erstellt `EndpointScriptRunner` mit optionalen Repository-Mocks
- `CreateContext(...)` — Erstellt `ScriptContext` mit optionalem Umgebungsservice und Response
- `CreateEnvironmentRepositoryMock()` — Mock für `ISystemEnvironmentRepository`
- `CreateSignalRNotificationServiceMock()` — Mock für `ISignalRNotificationService`
- `CreateEmptyEnvironmentService()` — `IActiveEnvironmentService`-Mock ohne Umgebung
- `CreateEnvironmentServiceWithVariables(Dictionary<string,string>)` — `IActiveEnvironmentService`-Mock mit Variablen und zugehöriger `SystemEnvironment`
