# Logik

## `EndpointExecutionService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs`

Implementiert `IEndpointExecutionService`. Führt HTTP-Requests für Endpunkte aus, löst `{{...}}`-Platzhalter auf und verwaltet Authentifizierung.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `ExecuteAsync(Endpoint)` | `public` | Einstiegspunkt; delegiert an `ExecuteImpersonatedAsync` oder `ExecuteWithAuthAsync` |
| `ExecuteWithAuthAsync(Endpoint)` | `private` | Erstellt HTTP-Client, baut Request, sendet und gibt Ergebnis zurück |
| `ExecuteImpersonatedAsync(Endpoint)` | `private` | Führt Request unter Windows-Impersonierung aus |
| `SendAndBuildResultAsync(HttpClient, Endpoint, HttpRequestMessage)` | `private static` | Sendet Request, misst Dauer, delegiert an `BuildResult` |
| `BuildResult(Endpoint, HttpResponseMessage, long)` | `private static` | Baut `EndpointExecutionResult` aus der HTTP-Antwort |
| `BuildRequest(Endpoint)` | `private` | Konstruiert `HttpRequestMessage` mit aufgelösten Platzhaltern, Headern, Body |
| `ResolvePlaceholders(string, IReadOnlyDictionary<string, string>)` | `private static` | Ersetzt `{{name}}`-Platzhalter durch Umgebungsvariablen |
| `ApplyAuthentication(HttpRequestMessage, Endpoint)` | `private` | Setzt Authorization-Header (Basic, Bearer) |

Abonnierte Events: keine.
Publizierte Events: keine.

Abhängigkeiten: `IHttpClientFactory`, `IHealthCheckService`, `ICredentialService`, `IActiveEnvironmentService`.

Die Klasse besitzt noch **keine** Abhängigkeit zu `IEndpointScriptRunner` und hat **keine** Pre-/Post-Skript-Logik.

---

## `ActiveEnvironmentService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ActiveEnvironmentService.cs`

Implementiert `IActiveEnvironmentService`. Hält die aktiv gewählte Umgebung im Scoped-Zustand.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `SetActiveEnvironment(SystemEnvironment?)` | `public` | Setzt die aktive Umgebung, materialisiert das Variablen-Dictionary und feuert `OnActiveEnvironmentChanged` |

Publizierte Events: `OnActiveEnvironmentChanged` (Action).

`SetActiveEnvironment` ersetzt die gesamte `ActiveVariables`-Collection — kein Einzelzugriff per Name vorhanden. Für `sz.environment.get(name)` und `sz.environment.set(name, value)` fehlen dedizierte Methoden.

---

## `ModelUpdateExtensions` (Endpunkt-Relevanz)
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/ModelUpdateExtensions.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `ApplyUpdate(this Endpoint, Endpoint)` | `internal static` | Kopiert bearbeitbare Felder von `source` nach `existing` bei einem Update |

Die Methode übernimmt aktuell: `Name`, `Method`, `RelativePath`, `Body`, `BodyMode`, `AuthenticationType`, `EndpointGroupId`. Die Felder `PreRequestScript` und `PostRequestScript` sind **noch nicht** enthalten und müssen bei der Implementierung ergänzt werden.

---

## `EndpointRepository`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/EndpointRepository.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `GetEndpointsAsync(int)` | `public` | Lädt alle Endpunkte einer Anwendung inkl. Headers, QueryParameters, EndpointGroup |
| `GetEndpointByIdAsync(int)` | `public` | Lädt einen Endpunkt mit Application, Headers, QueryParameters, EndpointGroup |
| `AddEndpointAsync(Endpoint)` | `public` | Persistiert neuen Endpunkt |
| `UpdateEndpointAsync(Endpoint)` | `public` | Aktualisiert bestehenden Endpunkt; ruft `ApplyUpdate` auf |
| `DeleteEndpointAsync(int)` | `public` | Löscht Endpunkt |
| `GetEndpointGroupsAsync(int)` | `public` | Lädt Endpunktgruppen einer Anwendung |
| `GetEndpointGroupByIdAsync(int)` | `public` | Lädt eine Endpunktgruppe mit Endpunkten |
| `AddEndpointGroupAsync(EndpointGroup)` | `public` | Persistiert neue Endpunktgruppe |
| `UpdateEndpointGroupAsync(EndpointGroup)` | `public` | Aktualisiert Endpunktgruppe |
| `DeleteEndpointGroupAsync(int)` | `public` | Löscht Endpunktgruppe |
| `AddHeaderAsync(EndpointHeader)` | `public` | Persistiert neuen Header |
| `DeleteHeaderAsync(int)` | `public` | Löscht Header |
| `AddQueryParameterAsync(EndpointQueryParameter)` | `public` | Persistiert neuen Query-Parameter |
| `DeleteQueryParameterAsync(int)` | `public` | Löscht Query-Parameter |
