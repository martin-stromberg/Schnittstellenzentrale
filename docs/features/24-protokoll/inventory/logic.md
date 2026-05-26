# Logik-Bestandsaufnahme

## `EndpointExecutionService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `ExecuteAsync(Endpoint)` | `public` | Öffentlicher Einstiegspunkt; delegiert an interne Überladung mit leerem `callDepth` |
| `ExecuteAsync(Endpoint, Dictionary<int,int>)` | `private` | Führt Pre-/Post-Skripte und HTTP-Request aus |
| `BuildScriptContext(Endpoint, Dictionary<int,int>, ScriptResponseData?)` | `private` | Erstellt einen `ScriptContext` mit aufgelösten URLs und Variablen |
| `ExecuteWithAuthAsync(Endpoint)` | `private` | HTTP-Ausführung für Standard-Authentifizierungstypen |
| `ExecuteImpersonatedAsync(Endpoint)` | `private` | HTTP-Ausführung unter Windows-Impersonation |
| `SendAndBuildResultAsync(HttpClient, Endpoint, HttpRequestMessage)` | `private static` | Sendet Request, stoppt Zeit, gibt Result zurück |
| `BuildResult(Endpoint, HttpResponseMessage, long)` | `private static` | Baut `EndpointExecutionResult` aus HTTP-Antwort |
| `BuildRequest(Endpoint)` | `private` | Erstellt `HttpRequestMessage` mit aufgelösten Platzhaltern |
| `ResolvePlaceholders(string, IReadOnlyDictionary<string,string>)` | `private static` | Ersetzt `{{variable}}`-Platzhalter im Text |
| `ExecuteEndpointByNameAsync(int, string, Dictionary<int,int>)` | `private` | Sucht Endpunkt per Name und führt ihn aus (`sz.execute`) |
| `ApplyAuthentication(HttpRequestMessage, Endpoint)` | `private` | Setzt Authorization-Header je nach `AuthenticationType` |

Konstruktor-Abhängigkeiten: `IHttpClientFactory`, `IHealthCheckService`, `ICredentialService`, `IActiveEnvironmentService`, `IEndpointScriptRunner`, `IEndpointRepository`, `ISystemEnvironmentRepository`, `ISignalRNotificationService`

Hinweis: `IActivityLogService` ist noch nicht injiziert. Kein Logging-Aufruf für erfolgreiche Requests, HTTP-Fehler oder interne Exceptions vorhanden.

---

## `EndpointScriptRunner`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/EndpointScriptRunner.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `ExecuteAsync(string, ScriptContext)` | `public` | Führt JavaScript via Jint aus; fängt `TimeoutException`, `JavaScriptException` und allgemeine `Exception` |
| `RegisterSzObject(Engine, ScriptContext)` | `private` | Registriert das `sz`-Objekt im Jint-Engine |
| `BuildEnvironmentObject(Engine, ScriptContext, ISystemEnvironmentRepository, ISignalRNotificationService)` | `private static` | Erstellt `sz.environment` mit `get` und `set` |
| `ApplyEnvironmentSet(ScriptContext, string, string, ISystemEnvironmentRepository, ISignalRNotificationService)` | `private static` | Aktualisiert aktive Umgebung und persistiert Variable |
| `PersistVariable(int, string, string, ISystemEnvironmentRepository, ISignalRNotificationService)` | `private static` | Persistiert Variable in DB und benachrichtigt SignalR (synchron via `Task.Run().GetAwaiter().GetResult()`) |
| `BuildRequestObject(Engine, ScriptRequestData)` | `private static` | Erstellt `sz.request` |
| `BuildResponseObject(Engine, ScriptResponseData)` | `private static` | Erstellt `sz.response` |
| `BuildHeadersObject(Engine, IEnumerable<KeyValuePair<string,string>>)` | `private static` | Hilfsmethode für Header-Objekte |
| `BuildBodyObject(Engine, ScriptRequestData)` | `private static` | Erstellt Body-Objekt für Request |
| `BuildBodyObject(Engine, ScriptResponseData)` | `private static` | Erstellt Body-Objekt für Response |
| `BuildBodyObjectCore(Engine, Func<object?>, Func<object?>, string?)` | `private static` | Kernlogik für Body-Objekt mit `asJson`, `asXml`, `raw` |

Konstruktor-Abhängigkeiten: `ISystemEnvironmentRepository`, `ISignalRNotificationService`

Registrierte `sz`-Lambdas: `sz.environment.get`, `sz.environment.set`, `sz.request`, `sz.response` (optional), `sz.execute`

Hinweis: `sz.console.write` ist noch nicht registriert. `IActivityLogService` ist noch nicht injiziert. Keine Protokollierung von Skriptausführung oder Fehlern vorhanden.

---

## `StorageModeService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/StorageModeService.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `SetMode(StorageMode)` | `public` | Setzt Modus und feuert `OnModeChanged`, wenn der Modus sich geändert hat |

Publizierte Events: `OnModeChanged`

---

## `ActiveEnvironmentService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ActiveEnvironmentService.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `SetActiveEnvironment(SystemEnvironment?)` | `public` | Setzt aktive Umgebung, materialisiert Variablen-Dictionary, feuert `OnActiveEnvironmentChanged` |

Publizierte Events: `OnActiveEnvironmentChanged`

---

## `MainLayout.razor`
Datei: `src/Schnittstellenzentrale/Components/Layout/MainLayout.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OnInitialized()` | `protected override` | Abonniert `OnModeChanged`, `OnThemeChanged`, `OnActiveEnvironmentChanged` |
| `OnAfterRenderAsync(bool)` | `protected override` | Beim ersten Render: Theme initialisieren, Umgebung aus localStorage wiederherstellen, SignalR verbinden |
| `ConnectHubAsync()` | `private` | Baut SignalR-Verbindung auf; abonniert `EnvironmentChanged`-Event |
| `OnStorageModeChanged(ChangeEventArgs)` | `private` | Ruft `StorageModeService.SetMode` auf und stellt Umgebung wieder her |
| `RestoreEnvironmentFromLocalStorageAsync(StorageMode)` | `private` | Liest gespeicherte Umgebungs-ID aus `localStorage` und setzt `ActiveEnvironmentService` |
| `OnEnvironmentChanged()` | `private` | Aktualisiert aktive Umgebung nach SignalR-Benachrichtigung |
| `OpenEnvironmentManagementAsync()` | `private` | Öffnet das Umgebungsverwaltungs-Overlay |
| `OnStateChanged()` | `private` | Trigger für `InvokeAsync(StateHasChanged)` |
| `DisposeAsync()` | `public` | Kündigt Events und SignalR-Verbindung |

Abonnierte Events: `StorageModeService.OnModeChanged`, `ThemeService.OnThemeChanged`, `ActiveEnvironmentService.OnActiveEnvironmentChanged`, SignalR `EnvironmentChanged`

Injizierte Services: `IStorageModeService`, `IThemeService`, `IActiveEnvironmentService`, `ISystemEnvironmentRepository`, `IJSRuntime`, `NavigationManager`, `ILogger<MainLayout>`

Hinweis: `IActivityLogService` ist noch nicht injiziert. Kein Protokoll-Symbol in der `.top-row`, keine `ActivityLogPanel`-Einbindung, kein `ContextSwitched`-Logging.

---

## `ApplicationGroupTree.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OnInitializedAsync()` | `protected override` | Abonniert `OnModeChanged`, lädt Daten, verbindet SignalR |
| `OnAfterRenderAsync(bool)` | `protected override` | Beim ersten Render: JS-Modul laden, Sidebar-Breite anwenden und Resize initialisieren |
| `RefreshAsync()` | `public` | Lädt Daten neu und ruft `StateHasChanged` |
| `ExpandApplicationAsync(int)` | `public` | Expandiert eine Anwendung und abonniert SignalR-Kanal |
| `LoadDataAsync()` | `private` | Lädt alle Gruppen und Anwendungen |
| `ReloadApplicationDataAsync(int)` | `private` | Lädt Endpunktgruppen und Endpunkte einer Anwendung |
| `OnModeChanged()` | `private` | Reagiert auf Modus-Wechsel: Zurücksetzen und Reload |
| `ToggleApplicationExpanded(int)` | `private` | Toggle für expandierte Anwendungen |
| `OnDrop(int?)` | `private` | Verarbeitet Drag & Drop von Anwendungen |
| `DisposeAsync()` | `public` | Kündigt Events, SignalR, JS-Modul |

Abonnierte Events: `StorageModeService.OnModeChanged`, SignalR `EndpointChanged`, SignalR `EndpointGroupChanged`

Injizierte Services: `IApplicationApiClient`, `IEndpointRepository`, `IStorageModeService`, `ICurrentUserService`, `NavigationManager`, `IJSRuntime`, `ILogger<ApplicationGroupTree>`

Hinweis: `IActivityLogService` ist noch nicht injiziert. Keine `EntityCreated`-, `EntityModified`- oder `EntityMoved`-Protokollaufrufe vorhanden.

---

## `Home.razor`
Datei: `src/Schnittstellenzentrale/Components/Pages/Home.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OnGroupSaved()` | `private` | Nach Anlage einer Gruppe: blendet Editor aus, refresht Tree |
| `OnApplicationSaved()` | `private` | Nach Anlage einer Anwendung: blendet Editor aus, refresht Tree |
| `OnGroupRenamed(ApplicationGroup)` | `private` | Ruft `UpdateGroupAsync` auf, refresht Tree |
| `HandleCreateEndpointRequested(...)` | `private` | Legt neuen Endpunkt an via `AddEndpointAsync`, ggf. mit SignalR-Notify |
| `OnCreateEndpointGroupConfirmed(string)` | `private` | Legt Ordner an via `AddEndpointGroupAsync`, ggf. mit SignalR-Notify |
| `OnEndpointGroupRenamed(EndpointGroup)` | `private` | Ruft `UpdateEndpointGroupAsync` auf, ggf. mit SignalR-Notify |
| `OnEndpointGroupDeleteConfirmed(EndpointGroup)` | `private` | Löscht Ordner via `DeleteEndpointGroupAsync` |
| `HandleDeleteEndpointRequested(Endpoint)` | `private` | Löscht Endpunkt nach `confirm`-Dialog |

Injizierte Services: `IApplicationApiClient`, `IEndpointRepository`, `IStorageModeService`, `ISignalRNotificationService`, `IJSRuntime`

Hinweis: `IActivityLogService` ist noch nicht injiziert. Keine Protokollaufrufe nach Persistierungsoperationen.
