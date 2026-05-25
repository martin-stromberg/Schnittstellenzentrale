# Tests

## Testklassen

### `EndpointExecutionServiceTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/EndpointExecutionServiceTests.cs`

Unit-Tests für `EndpointExecutionService`. Alle Tests instanziieren den Service direkt ohne DI-Container.

| Testmethode | Was wird getestet? |
|---|---|
| `Execute_WithAuthTypeNone_SendsRequestWithoutCredentials` | Kein Authorization-Header bei `AuthenticationType.None` |
| `Execute_WithAuthTypeBasic_SendsBasicAuthHeader` | Basic-Auth-Header wird korrekt gesetzt |
| `Execute_WithNegotiateAuthType_UsesNegotiateHandler` | Negotiate- und NegotiateWithImpersonation-Authtypen verwenden Named-Client `"negotiate"` |
| `Execute_WithAuthTypeBearerToken_SendsBearerHeader` | Bearer-Token wird als Authorization-Header gesetzt |
| `Execute_SetsResponseHeaders` | Response-Header werden korrekt übernommen |
| `Execute_SetsDurationMs` | `DurationMs` wird gemessen und > 0 |
| `Execute_SetsResponseSizeBytes` | `ResponseSizeBytes` entspricht UTF-8-Byte-Länge des Body |
| `Execute_OnConnectionError_DoesNotCallHealthCheck` | Bei Verbindungsfehler kein Health-Check, `Success = false`, kein StatusCode |
| `BuildRequest_ErsetztPfadPlatzhalterDurchGespeicherteWerte` | `{id}` im Pfad wird durch QueryParameter-Wert ersetzt |
| `BuildRequest_HaengtNurNichtPlatzhalterParameterAlsQueryStringAn` | Pfad-Platzhalter landen nicht doppelt im Query-String |
| `BuildRequest_ResolvesDoubleBracePlaceholders` | `{{env}}` wird durch Umgebungsvariable ersetzt |
| `BuildRequest_ResolvesSingleBracePlaceholdersFromQueryParameters` | `{id}` wird durch QueryParameter ersetzt |
| `BuildRequest_MissingVariable_ReplacesWithEmptyString` | Fehlende Umgebungsvariable ergibt leeren String |
| `BuildRequest_NoActiveEnvironment_ReplacesAllDoubleBracePlaceholdersWithEmptyString` | Ohne aktive Umgebung werden alle `{{...}}` durch leer ersetzt |
| `BuildRequest_ResolvesPlaceholdersInBaseUrl` | Platzhalter in `BaseUrl` werden aufgelöst |
| `BuildRequest_ResolvesPlaceholdersInRelativePath` | Platzhalter im `RelativePath` werden aufgelöst |
| `BuildRequest_ResolvesPlaceholdersInHeaderNamesAndValues` | Platzhalter in Header-Keys und -Values werden aufgelöst |
| `BuildRequest_ResolvesPlaceholdersInQueryParameterNamesAndValues` | Platzhalter in Query-Parameter-Keys und -Values werden aufgelöst |
| `BuildRequest_ResolvesPlaceholdersInBearerToken` | Platzhalter im Bearer-Token werden aufgelöst |
| `BuildRequest_ResolvesDoubleBracePlaceholdersBeforeSingleBrace` | `{{env}}` und `{id}` werden korrekt kombiniert aufgelöst |
| `BuildRequest_ResolvesPlaceholdersInBody` | Platzhalter im Request-Body werden aufgelöst |

Kein Test für Pre-/Post-Skript-Logik, `sz.execute()`, Rekursionsschutz oder Skript-Fehlerbehandlung vorhanden.

---

### `EndpointPageTests`
Datei: `src/Schnittstellenzentrale.Tests/Components/EndpointPageTests.cs`

bUnit-Tests für die `EndpointPage`-Komponente.

| Testmethode | Was wird getestet? |
|---|---|
| `OhneAnfrageergebnis_AntwortBereichNichtSichtbar` | Ohne Ergebnis ist `.response-section` nicht gerendert |
| `AnfrageErgebnis_ResponseBodyWirdKorrektAngezeigt` | Response-Body wird nach Senden angezeigt |
| `AnfrageErgebnis_StatusCodeWirdAngezeigt` | HTTP-Statuscode ist sichtbar im Response-Bereich |
| `EndpunktMitBody_TextareaZeigtGespeichertenBody` | Body-Textarea zeigt gespeicherten Body |
| `PfadMitPlatzhalter_WirdBeimLadenAlsNichtLoeschbarerEintragAngezeigt` | Pfad-Platzhalter erscheinen als nicht-löschbarer QueryParam-Eintrag |
| `PfadMitPlatzhalter_VorhandenerWertBleibtErhalten_WennPlatzhalterUnveraendert` | Eingegebener Platzhalter-Wert bleibt nach erneutem `SyncPathParameters` erhalten |
| `GespeicherterPlatzhalterWert_WirdNachLadenNichtDupliziert` | DB-gespeicherter Wert wird nicht als Duplikat angelegt |
| `GeaenderterPfad_EntferntWeggefalleneUndFuegtNeueHinzu` | Pfadänderung aktualisiert Platzhalter-Liste korrekt |
| `PfadMitQueryString_WirdExtrahiertUndPfadBereinigt` | Query-String wird aus Pfad extrahiert |
| `AufgeloesteUrl_WirdImPfadfeldAngezeigt` | Pfadfeld zeigt die aufgelöste URL mit Platzhalter-Werten und Query-Parametern |

Kein Test für die Registerkarten „Pre-Request-Skript" und „Post-Request-Skript" vorhanden.

---

### `EndpointExecutionIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/EndpointExecutionIntegrationTests.cs`

| Testmethode | Was wird getestet? |
|---|---|
| `ExecuteEndpoint_OwnApiWithBearerToken_ReturnsSuccess` | Echter Request gegen den Test-Server mit Bearer-Token liefert HTTP 200 |

---

## Hilfsmethoden

### `EndpointExecutionServiceTests` (interne Hilfsmethoden)

| Hilfsmethode | Zweck |
|---|---|
| `CreateApp()` | Erstellt eine Test-`Application` mit `BaseUrl = "http://localhost:5000"` |
| `CreateEndpoint(AuthenticationType)` | Erstellt einen Standard-`Endpoint` mit `GET /test` |
| `CreateEndpoint(AuthenticationType, string, EndpointQueryParameter[]?)` | Erstellt einen Endpunkt mit angepasstem Pfad und QueryParameters |
| `CreateEmptyActiveEnvironmentMock()` | Mock von `IActiveEnvironmentService` ohne aktive Umgebung und leere Variablen |
| `CreateService(...)` | Erstellt einen `EndpointExecutionService` mit gemocktem `HttpMessageHandler` |
| `CreateServiceCapturingUri(...)` | Wie `CreateService`, aber mit URI-Capture-Callback für Assertions auf die gesendete URL |
| `CreateActiveEnvironmentMock(Dictionary<string, string>)` | Mock mit definierten Variablen |
| `CreateEndpointWithHeaders(...)` | Erstellt Endpunkt mit vordefinierten Headern |
| `CreateEndpointWithBody(...)` | Erstellt POST-Endpunkt mit Body |

### `EndpointPageTests` (interne Hilfsmethoden)

| Hilfsmethode | Zweck |
|---|---|
| `CreateEndpoint(string?, string, QueryParamEntry[]?)` | Erstellt einen Test-Endpunkt für bUnit-Rendering |
