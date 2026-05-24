# Detailanalyse: Vorhandene Testinfrastruktur

## `ControllerTestFactory`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/ControllerTestFactory.cs`

Leitet von `WebApplicationFactory<Program>` ab. Konfiguriert in `ConfigureTestServices`:
- Entfernt alle produktiven `IAuthenticationSchemeProvider`- und `IAuthenticationHandlerProvider`-Registrierungen
- Hängt `TestAuthHandler` als Schema `"Test"` ein
- Ersetzt `IDbContextFactory<AppDbContext>` durch SQLite In-Memory über eine offengehaltene `SqliteConnection`
- Entfernt und re-registriert `IApplicationRepository`
- Entfernt alle `IHostedService`-Registrierungen
- Mockt `ISignalRNotificationService` per `Moq`
- Ermöglicht optionale Überschreibung der Token-Lebenszeit via `TokenLifetime`-Property

In `CreateHost` wird nach dem Host-Start `db.Database.EnsureCreated()` aufgerufen.

Hilfsmethode `ObtainTokenAsync(HttpClient)` ruft `POST /authenticate` auf und gibt den zurückgelieferten Bearer-Token zurück.

| Eigenschaft/Methode | Beschreibung |
|---|---|
| `TokenLifetime` (`TimeSpan?`) | Überschreibt die Standard-Token-Lebenszeit im `ITokenStore` |
| `ObtainTokenAsync(HttpClient)` | Holt Bearer-Token vom Test-Server |
| `ConfigureWebHost(IWebHostBuilder)` | Überschreibt Auth, DB, IHostedService, SignalR |
| `CreateHost(IHostBuilder)` | Erzeugt DB-Schema nach Host-Start |

---

## `TestAuthHandler`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/TestAuthHandler.cs`

Implementiert `AuthenticationHandler<AuthenticationSchemeOptions>`. Gibt bei jedem Aufruf von `HandleAuthenticateAsync` einen fest codierten Benutzer `TEST\testuser` (als `ClaimTypes.Name`) zurück, ohne weitere Prüfung. Kann direkt in `PlaywrightTestFactory` wiederverwendet werden.

---

## `TestHelpers`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs`

Statische Hilfsklasse mit zwei Methoden:

| Methode | Beschreibung |
|---|---|
| `CreateInMemoryDbContext()` | Erzeugt `IDbContextFactory<AppDbContext>` mit SQLite In-Memory; gibt Factory und offene `SqliteConnection` zurück |
| `ExecuteWithTwoContextsAsync(...)` | Führt Test mit zwei unabhängigen `ApplicationRepository`-Instanzen über dieselbe In-Memory-Connection aus |

Enthält private innere Klasse `FixedOptionsDbContextFactory` als `IDbContextFactory<AppDbContext>`-Implementierung.

---

## `AppDbContext` und Datenbankinfrastruktur
Datei: `src/Schnittstellenzentrale.Infrastructure/Data/AppDbContext.cs`

Kennt folgende `DbSet`-Eigenschaften: `ApplicationGroups`, `Applications`, `EndpointGroups`, `Endpoints`, `EndpointHeaders`, `EndpointQueryParameters`. Unterstützt `EnsureCreated()` und `MigrateAsync()`. Wird als `IDbContextFactory<AppDbContext>` registriert.

`DatabaseProviderFactory` (Datei: `src/Schnittstellenzentrale.Infrastructure/Data/DatabaseProviderFactory.cs`) liest `DatabaseProvider`-Konfigurationsschlüssel und registriert SQLite oder SQL Server. Standard-Connection-String: `Data Source=schnittstellenzentrale.db`.

`AppDbContextFactory` (Datei: `src/Schnittstellenzentrale.Infrastructure/Data/AppDbContextFactory.cs`) ist die Design-Time-Factory; nicht relevant für Tests, zeigt aber, dass SQLite mit Dateiname `schnittstellenzentrale.db` der Standard ist.

---

## `SystemEntryInitializer`
Datei: `src/Schnittstellenzentrale/SystemEntryInitializer.cs`

Statische Klasse. Methode `InitializeAsync(IServiceProvider, IConfiguration)` legt Systemgruppe und Systemanwendung in der Datenbank an, sofern `Api:BaseUrl` konfiguriert ist. Wird in `Program.cs` nach der Datenbankinitialisierung aufgerufen. Für Playwright-Tests muss `Api:BaseUrl` auf `Server.BaseAddress` gesetzt werden, damit der korrekte Systemeintrag entsteht.

---

## SignalR-Infrastruktur

### `EndpointHub`
Datei: `src/Schnittstellenzentrale/Hubs/EndpointHub.cs`

Leitet von `Hub` ab. Stellt Methoden zum Abonnieren/Abbestellen von SignalR-Gruppen bereit:

| Methode | SignalR-Gruppe |
|---|---|
| `SubscribeToApplication(int applicationId)` | `application:{applicationId}` |
| `UnsubscribeFromApplication(int applicationId)` | `application:{applicationId}` |
| `SubscribeToGroup(int groupId)` | `group:{groupId}` |
| `UnsubscribeFromGroup(int groupId)` | `group:{groupId}` |

Registriert in `Program.cs` unter `/hubs/endpoint`.

### `SignalRNotificationService<THub>`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/SignalRNotificationService.cs`

Implementiert `ISignalRNotificationService`. Sendet folgende SignalR-Events:

| Event | Zielgruppe |
|---|---|
| `ApplicationChanged` | `application:{applicationId}` |
| `GroupChanged` | `group:{groupId}` |
| `EndpointChanged` | `application:{applicationId}` |
| `EndpointGroupChanged` | `application:{applicationId}` |

In `Program.cs` registriert als `SignalRNotificationService<EndpointHub>`.

---

## `StorageModeService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/StorageModeService.cs`

Implementiert `IStorageModeService`. Hält `CurrentMode` (Standardwert: `StorageMode.Team`). Löst `OnModeChanged`-Event aus, wenn `SetMode()` den Modus wechselt.

---

## `SystemEndpointSyncService`
Datei: `src/Schnittstellenzentrale/SystemEndpointSyncService.cs`

`BackgroundService`, der nach App-Start einmalig die eigenen API-Endpunkte in der Datenbank abgleicht. Wird in `ControllerTestFactory` (und soll auch in `PlaywrightTestFactory`) per `services.RemoveAll<IHostedService>()` deaktiviert.

---

## Authentifizierungsmechanismus (Produktion)

In `Program.cs` ist Windows-Authentifizierung (Negotiate/NTLM/Kerberos) als primäres Schema eingetragen. Die API-Endpunkte sind mit `[Authorize]` geschützt und verwenden Bearer-Tokens, die über `POST /authenticate` ausgestellt werden. `TestAuthHandler` ersetzt in Tests beide Schritte: er liefert direkt `TEST\testuser` als authentifizierten Benutzer, sodass `/authenticate` ohne Windows-Credentials funktioniert.
