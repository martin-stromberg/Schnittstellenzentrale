# Tests

## Testklassen

### `EndpointPageTests`
Datei: `src/Schnittstellenzentrale.Tests/Components/EndpointPageTests.cs`

Framework: bUnit

- `OhneAnfrageergebnis_AntwortBereichNichtSichtbar` — Prüft, dass die `.response-section` ohne Ausführung nicht sichtbar ist.
- `AnfrageErgebnis_ResponseBodyWirdKorrektAngezeigt` — Prüft, dass der Response-Body nach Ausführung in `<pre>` angezeigt wird (JSON-Teilstring).
- `AnfrageErgebnis_StatusCodeWirdAngezeigt` — Prüft, dass der HTTP-Statuscode (z. B. 404) in `.response-section` sichtbar ist.
- `EndpunktMitBody_TextareaZeigtGespeichertenBody` — Prüft, dass der Body-Tab die gespeicherte Body-Zeichenkette im `<textarea>`-Feld anzeigt.

Kein Test zu Pfad-Platzhaltern, Query-String-Extraktion, `IsPathParameter`, `SyncPathParameters`, `ExtractAndStripQueryString`, `ResolveDisplayUrl` oder `OnPathBlur` vorhanden.

---

### `EndpointExecutionServiceTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/EndpointExecutionServiceTests.cs`

Framework: xUnit + Moq

- `Execute_WithAuthTypeNone_SendsRequestWithoutCredentials` — Prüft, dass kein `Authorization`-Header gesetzt wird.
- `Execute_WithAuthTypeBasic_SendsBasicAuthHeader` — Prüft, dass der `Basic`-Auth-Header korrekt gesetzt wird.
- `Execute_WithNegotiateAuthType_UsesNegotiateHandler` — Prüft (Theory), dass `"negotiate"`-Client für `Negotiate` und `NegotiateWithImpersonation` verwendet wird.
- `Execute_WithAuthTypeBearerToken_SendsBearerHeader` — Prüft, dass der `Bearer`-Auth-Header korrekt gesetzt wird.
- `Execute_SetsResponseHeaders` — Prüft, dass Response-Header korrekt in das Ergebnis übernommen werden.
- `Execute_SetsDurationMs` — Prüft, dass die Laufzeitmessung einen positiven Wert liefert.
- `Execute_SetsResponseSizeBytes` — Prüft, dass die Response-Größe korrekt in Bytes berechnet wird.
- `Execute_OnConnectionError_DoesNotCallHealthCheck` — Prüft, dass bei Verbindungsfehler kein Health-Check ausgelöst wird und `Success = false` zurückgegeben wird.

Kein Test zu Pfad-Platzhalter-Ersetzung in `BuildRequest` vorhanden.

---

### `EndpointExecutionIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/EndpointExecutionIntegrationTests.cs`

Framework: xUnit + WebApplicationFactory

- `ExecuteEndpoint_OwnApiWithBearerToken_ReturnsSuccess` — Echter End-to-End-Test gegen den Test-Server: legt Endpunkt auf `/api/application-groups` an, holt Bearer-Token und prüft HTTP 200.

---

### `EndpointExecutionTests` (Playwright)
Datei: `src/Schnittstellenzentrale.Tests/Playwright/EndpointExecutionTests.cs`

Framework: Playwright + xUnit

- `ExecuteEndpoint_ReturnsSuccessResponse` — E2E-Test im Browser: navigiert zur App, legt Endpunkt auf `/api/application-groups` an, klickt "Anfrage senden" und prüft, dass ein 2xx-Statuscode in `.response-section` erscheint.

Kein E2E-Test zu Pfad-Platzhaltern, Query-String-Extraktion oder URL-Auflösung vorhanden.

---

## Hilfsmethoden

### `TestHelpers`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs`

- `CreateInMemoryDbContext()` — Erstellt eine `IDbContextFactory<AppDbContext>` mit SQLite In-Memory-Provider. Gibt Factory und offene `SqliteConnection` zurück.
- `ExecuteWithTwoContextsAsync(Func<ApplicationRepository, ApplicationRepository, Task>)` — Führt einen Test mit zwei unabhängigen Repository-Instanzen auf derselben In-Memory-Datenbankverbindung aus (für Concurrency-Szenarien).

### `EndpointPageTests` (Hilfsmethoden)

- `CreateEndpoint(string? body)` — Erstellt einen minimal konfigurierten `Endpoint` mit `Id = 1`, `Name = "Test"`, `RelativePath = "/test"`, `Method = GET`, leeren `Headers` und `QueryParameters`. Wird von allen Tests in `EndpointPageTests` als Ausgangsobjekt verwendet.

### `EndpointExecutionServiceTests` (Hilfsmethoden)

- `CreateApp()` — Erstellt eine `Application` mit `BaseUrl = "http://localhost:5000"`.
- `CreateEndpoint(AuthenticationType)` — Erstellt einen `Endpoint` mit leerem `QueryParameters`-Array und konfigurierbarer Authentifizierungsart.
- `CreateService(Mock<IHealthCheckService>, Mock<ICredentialService>, HttpStatusCode, string)` — Erstellt einen `EndpointExecutionService` mit gemocktem `HttpMessageHandler` und konfigurierbarer Response.
