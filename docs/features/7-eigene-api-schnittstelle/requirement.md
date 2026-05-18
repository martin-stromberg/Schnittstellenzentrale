# Anforderung: Eigene REST-API-Schnittstelle

## Fachliche Zusammenfassung

Die Schnittstellenzentrale wird um eine eigene REST-API erweitert, die zunächst die Anlage von `ApplicationGroup`- und `Application`-Datensätzen über HTTP-Endpunkte ermöglicht. Parallel dazu wird die interne Datenzugriffsschicht der Blazor-Webseite umgestellt: Statt `IApplicationRepository` direkt zu injizieren, rufen die betroffenen UI-Komponenten (`ApplicationGroupEditor`, `ApplicationEditor`) ihre Schreiboperationen künftig über eine neue REST-Client-Klasse auf, die die eigene API anspricht. Damit entsteht eine saubere Trennung zwischen der HTTP-API-Schicht und der Datenbankschicht, und die API kann künftig auch von externen Clients genutzt werden.

---

## Betroffene Klassen und Komponenten

### API-Schicht (neu zu erstellen — `Schnittstellenzentrale`)

| Artefakt | Beschreibung |
|---|---|
| `ApplicationGroupsController` | ASP.NET Core Minimal-API-Handler oder `ControllerBase`-Klasse; exponiert `POST /api/application-groups` zum Anlegen einer `ApplicationGroup`. |
| `ApplicationsController` | Exponiert `POST /api/applications` zum Anlegen einer `Application`. |

*Annahme: Die API wird als ASP.NET Core Web-API innerhalb der bestehenden Blazor-Server-App (`Schnittstellenzentrale`) implementiert, da die Anforderung kein separates API-Projekt erwähnt. Alternativ wäre ein eigenes Projekt `Schnittstellenzentrale.Api` denkbar.*

### DTO-Klassen (neu zu erstellen — `Schnittstellenzentrale.Core` oder eigenes `Contracts`-Projekt)

| Klasse | Beschreibung |
|---|---|
| `CreateApplicationGroupRequest` | Request-Body für `POST /api/application-groups` (Pflichtfeld: `Name`). |
| `ApplicationGroupResponse` | Response-Body für die angelegte `ApplicationGroup` (mindestens `Id`, `Name`). |
| `CreateApplicationRequest` | Request-Body für `POST /api/applications` (Pflichtfelder: `Name`, `BaseUrl`; optional: `Description`, `SwaggerUrl`, `MetadataUrl`, `ApplicationGroupId`). |
| `ApplicationResponse` | Response-Body für die angelegte `Application` (mindestens `Id`, `Name`, `BaseUrl`, `ApplicationGroupId`). |

### REST-Client-Klasse (neu zu erstellen — `Schnittstellenzentrale` oder `Schnittstellenzentrale.Infrastructure`)

| Artefakt | Beschreibung |
|---|---|
| `IApplicationApiClient` | Interface mit Methoden analog zu den bisherigen Repository-Schreiboperationen: `AddGroupAsync(CreateApplicationGroupRequest)` → `Task<ApplicationGroupResponse>` und `AddApplicationAsync(CreateApplicationRequest)` → `Task<ApplicationResponse>`. |
| `ApplicationApiClient` | Implementierung von `IApplicationApiClient` via `HttpClient`; sendet JSON-Requests an die eigene API. |

### UI-Komponenten (Blazor) — zu ändern

| Komponente | Änderung |
|---|---|
| `ApplicationGroupEditor` | Injektion von `IApplicationRepository` durch `IApplicationApiClient` ersetzen; `AddGroupAsync`-Aufruf auf den REST-Client umstellen. |
| `ApplicationEditor` | Analog: Schreiboperation `AddApplicationAsync` über `IApplicationApiClient`; Leseoperation `GetGroupsAsync` (zum Befüllen der Gruppenauswahl) bleibt vorerst auf `IApplicationRepository` oder wird ebenfalls über den Client geführt (siehe Offene Fragen). |

### DI-Registrierung

- `ApplicationApiClient` wird in `Program.cs` als `IApplicationApiClient` registriert (`AddHttpClient<IApplicationApiClient, ApplicationApiClient>`).
- Die API-Controller werden über `builder.Services.AddControllers()` und `app.MapControllers()` eingebunden.

### Tests

- Integrationstests für `ApplicationGroupsController` und `ApplicationsController` (WebApplicationFactory, In-Memory-Datenbank).
- Unit-Tests für `ApplicationApiClient` mit gemocktem `HttpMessageHandler`.

---

## Implementierungsansatz

- Die neuen Controller delegieren Schreiboperationen an das bestehende `IApplicationRepository` (bzw. `ApplicationRepository`); die Datenbankschicht bleibt unverändert.
- `ApplicationApiClient` nutzt den bereits in `Program.cs` registrierten `IHttpClientFactory`-Mechanismus. Die Basis-URL der eigenen API wird aus der Konfiguration bezogen (z. B. `Api:BaseUrl`) oder zur Laufzeit aus dem aktuellen Request-Kontext abgeleitet (Loopback-Aufruf).
- Für den Loopback-Aufruf innerhalb derselben Anwendung ist Windows-Authentifizierung zu berücksichtigen: Der `HttpClient` muss entweder `UseDefaultCredentials = true` setzen oder ein API-Key-/Bearer-Schema verwenden, das für interne Aufrufe ohne Benutzerkontext auskommt (Annahme: Loopback mit `UseDefaultCredentials` analog zum bestehenden `"negotiate"`-Client).
- Die API-Endpunkte sollen durch die bestehende Windows-Authentifizierungs-Middleware (`UseAuthentication` / `UseAuthorization`) gesichert sein. Ein `[Authorize]`-Attribut auf den Controllern genügt.
- Request-Validierung erfolgt über Data-Annotations auf den DTO-Klassen (z. B. `[Required]`, `[MaxLength(200)]`) und ASP.NET Core Model-Validation.
- Nach erfolgreichem Anlegen gibt die API `201 Created` mit dem `Location`-Header zurück (REST-Konvention).
- `ISignalRNotificationService.NotifyGroupChangedAsync` bzw. `NotifyApplicationChangedAsync` werden weiterhin im `ApplicationRepository` (oder explizit im Controller) aufgerufen, um Team-Modus-Benachrichtigungen sicherzustellen.

---

## Konfiguration

| Schlüssel | Ebene | Beschreibung |
|---|---|---|
| `Api:BaseUrl` | `appsettings.json` | Basis-URL der eigenen REST-API für den `ApplicationApiClient` (z. B. `https://localhost:5001`). Ermöglicht Loopback-Konfiguration ohne Hardcodierung. *(Annahme)* |

*Falls die Basis-URL dynamisch aus dem laufenden Request abgeleitet wird (z. B. über `IHttpContextAccessor`), entfällt dieser Konfigurationsschlüssel.*

---

## Offene Fragen

1. **API-Hosting-Strategie:** Soll die REST-API innerhalb der bestehenden Blazor-Server-App gehostet werden (`app.MapControllers()` in `Program.cs`), oder wird ein separates ASP.NET Core Web-API-Projekt (`Schnittstellenzentrale.Api`) bevorzugt? Ein separates Projekt ermöglicht eine unabhängige Versionierung und Skalierung, erhöht aber den Verwaltungsaufwand.

2. **Authentifizierung für interne API-Aufrufe:** Wie soll sich der `ApplicationApiClient` beim Loopback-Aufruf authentifizieren? Optionen: (a) `UseDefaultCredentials = true` (analog zum bestehenden `"negotiate"`-Client), (b) API-Key im Request-Header, (c) Bypass der Authentifizierung für Loopback-Requests.

3. **Umfang der REST-API:** Die Anforderung nennt explizit nur die Anlage (POST) von Gruppen und Anwendungen. Sollen Lese- (GET), Aktualisierungs- (PUT/PATCH) und Löschoperationen (DELETE) ebenfalls in diesem Feature implementiert werden, oder sind diese Gegenstand einer späteren Erweiterung?

4. **Lesezugriff im `ApplicationEditor`:** Soll die Leseoperation `GetGroupsAsync` (zum Befüllen der Gruppenauswahliste im `ApplicationEditor`) ebenfalls über die neue API geführt werden, oder verbleibt sie auf `IApplicationRepository`? Konsistenz spricht für die API, Performance für den direkten Repository-Zugriff.

5. **DTO-Verortung:** Sollen die Request/Response-DTO-Klassen im Projekt `Schnittstellenzentrale.Core` (als Contracts), in einem eigenen Projekt `Schnittstellenzentrale.Contracts` oder direkt im Webprojekt abgelegt werden? Die Wahl beeinflusst, ob der Client die DTOs referenzieren kann, ohne das Infrastrukturprojekt einzubinden.

6. **StorageMode-Kontext in der API:** Die bisherigen Repository-Methoden erhalten `StorageMode` und `owner` als Parameter. Wie überträgt der `ApplicationApiClient` diesen Kontext an die API — als Query-Parameter, als Header oder leitet die API ihn selbst aus dem authentifizierten Benutzer ab?

7. **API-Versionierung:** Soll von Anfang an ein Versionierungsschema (z. B. URL-Präfix `/api/v1/`) eingeführt werden, um künftige Breaking Changes zu ermöglichen?
