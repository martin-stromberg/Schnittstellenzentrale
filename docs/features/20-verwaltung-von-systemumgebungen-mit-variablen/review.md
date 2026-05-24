# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

### Neue Klassen / Interfaces

- [x] `SystemEnvironment` (Datenmodellklasse) — angelegt
- [x] Feld `Id` in `SystemEnvironment` — vorhanden
- [x] Feld `Name` in `SystemEnvironment` — vorhanden
- [x] Feld `Mode` in `SystemEnvironment` — vorhanden
- [x] Feld `Owner` in `SystemEnvironment` — vorhanden
- [x] Feld `Variables` in `SystemEnvironment` — vorhanden (`ICollection<EnvironmentVariable>`)
- [x] `EnvironmentVariable` (Datenmodellklasse) — angelegt
- [x] Feld `Name` in `EnvironmentVariable` — vorhanden
- [x] Feld `Value` in `EnvironmentVariable` — vorhanden
- [x] Feld `IsValueMasked` in `EnvironmentVariable` — vorhanden
- [x] `ISystemEnvironmentRepository` (Interface) — angelegt
- [x] Methode `GetEnvironmentsAsync` in `ISystemEnvironmentRepository` — vorhanden
- [x] Methode `GetByIdAsync` in `ISystemEnvironmentRepository` — vorhanden
- [x] Methode `AddAsync` in `ISystemEnvironmentRepository` — vorhanden
- [x] Methode `UpdateAsync` in `ISystemEnvironmentRepository` — vorhanden
- [x] Methode `DeleteAsync` in `ISystemEnvironmentRepository` — vorhanden
- [x] `SystemEnvironmentRepository` (Klasse) — angelegt
- [x] `IActiveEnvironmentService` (Interface) — angelegt
- [x] Eigenschaft `ActiveEnvironment` in `IActiveEnvironmentService` — vorhanden
- [x] Eigenschaft `ActiveVariables` in `IActiveEnvironmentService` — vorhanden (`IReadOnlyDictionary<string, string>`)
- [x] Event `OnActiveEnvironmentChanged` in `IActiveEnvironmentService` — vorhanden
- [x] Methode `SetActiveEnvironment` in `IActiveEnvironmentService` — vorhanden
- [x] `ActiveEnvironmentService` (Klasse) — angelegt
- [x] `EnvironmentSelector` (Blazor-Komponente) — angelegt
- [x] `EnvironmentManagementOverlay` (Blazor-Komponente) — angelegt
- [x] `EnvironmentEditor` (Blazor-Komponente) — angelegt
- [x] `SystemEnvironmentRepositoryIntegrationTests` (Testklasse) — angelegt

### Änderungen an bestehenden Klassen

#### `AppDbContext`

- [x] Eigenschaft `SystemEnvironments` (`DbSet<SystemEnvironment>`) — vorhanden
- [x] Eigenschaft `EnvironmentVariables` (`DbSet<EnvironmentVariable>`) — vorhanden
- [x] `OnModelCreating` — Unique-Constraint auf `SystemEnvironment` (`Name` + `Mode` + `Owner`) — vorhanden
- [x] `OnModelCreating` — Unique-Constraint auf `EnvironmentVariable` (`Name` + `SystemEnvironmentId`) — vorhanden
- [x] `OnModelCreating` — Cascade Delete von `SystemEnvironment` auf `EnvironmentVariable` — vorhanden

#### `ISignalRNotificationService`

- [x] Methode `NotifyEnvironmentChangedAsync()` — vorhanden

#### `SignalRNotificationService<THub>`

- [x] Methode `NotifyEnvironmentChangedAsync()` — vorhanden (sendet `EnvironmentChanged` an Gruppe `environments`)

#### `EndpointExecutionService`

- [x] Konstruktorparameter `IActiveEnvironmentService activeEnvironmentService` — vorhanden
- [x] Methode `ResolvePlaceholders(string input, IReadOnlyDictionary<string, string> variables)` (privat, statisch) — vorhanden; Regex `\{\{([^}]+)\}\}` compiliert; fehlende Variable ergibt leeren String; `null`-sichere Rückgabe
- [x] `BuildRequest` — `ResolvePlaceholders` für Basis-URL — vorhanden
- [x] `BuildRequest` — `ResolvePlaceholders` für `RelativePath` — vorhanden
- [x] `BuildRequest` — `ResolvePlaceholders` für Header-Namen und -Werte — vorhanden
- [x] `BuildRequest` — `ResolvePlaceholders` für Query-Parameter-Namen und -Werte — vorhanden
- [x] `BuildRequest` — `ResolvePlaceholders` für Body — vorhanden
- [x] `BuildRequest` — `ResolvePlaceholders` für Bearer-Token — vorhanden (in `ApplyAuthentication`, aufgerufen nach `BuildRequest`)

#### `MainLayout`

- [x] Injizierte Abhängigkeit `IActiveEnvironmentService` — vorhanden
- [x] Injizierte Abhängigkeit `IJSRuntime` — vorhanden
- [x] Injizierte Abhängigkeit `ISystemEnvironmentRepository` — vorhanden
- [x] Methode `RestoreEnvironmentFromLocalStorageAsync(StorageMode mode)` — vorhanden
- [x] `OnAfterRenderAsync` — ruft `RestoreEnvironmentFromLocalStorageAsync` beim ersten Render auf — vorhanden
- [x] `OnStorageModeChanged` — ruft `RestoreEnvironmentFromLocalStorageAsync` für den neuen Modus auf — vorhanden
- [x] Methode `OnEnvironmentChanged` — vorhanden; lädt Umgebung via `GetByIdAsync` neu, setzt `IActiveEnvironmentService` auf `null` bei gelöschter Umgebung, löscht `localStorage`-Eintrag, aktualisiert `EnvironmentSelector`
- [x] SignalR-Abonnement `EnvironmentChanged` — vorhanden (via `ConnectHubAsync`, aufgerufen in `OnAfterRenderAsync`; `HubConnection.On("EnvironmentChanged", ...)` ruft `OnEnvironmentChanged` auf)
- [x] `Dispose` — meldet SignalR-Abonnement ab — vorhanden (`DisposeAsync` disposed `_hubConnection`)
- [x] `EnvironmentSelector`-Komponente im Header integriert — vorhanden
- [x] Zahnrad-Icon im Header als Trigger für `EnvironmentManagementOverlay.OpenAsync()` — vorhanden

#### `TestHelpers`

- [x] Methode `ExecuteWithTwoSystemEnvironmentRepositoriesAsync(Func<SystemEnvironmentRepository, SystemEnvironmentRepository, Task>)` — vorhanden

#### `EndpointExecutionServiceTests` (bestehende Tests angepasst)

- [x] Bestehende Tests verwenden `IActiveEnvironmentService`-Mock im Konstruktor — vorhanden (alle Tests nutzen `CreateEmptyActiveEnvironmentMock()`)

### Datenbankmigrationen

- [x] Migration `AddSystemEnvironments` — vorhanden
- [x] Tabelle `SystemEnvironments` angelegt — vorhanden
- [x] Tabelle `EnvironmentVariables` angelegt — vorhanden
- [x] Unique-Constraints und Cascade-Delete-Fremdschlüssel — vorhanden

### DI-Registrierungen

- [x] `ISystemEnvironmentRepository` / `SystemEnvironmentRepository` — registriert (`AddScoped`)
- [x] `IActiveEnvironmentService` / `ActiveEnvironmentService` — registriert (`AddScoped`)

### Neue Tests

- [x] `AddEnvironment_PersistsEnvironment` in `SystemEnvironmentRepositoryIntegrationTests` — vorhanden
- [x] `AddEnvironment_WithDuplicateName_ThrowsConstraintException` in `SystemEnvironmentRepositoryIntegrationTests` — vorhanden
- [x] `DeleteEnvironment_CascadesVariables` in `SystemEnvironmentRepositoryIntegrationTests` — vorhanden
- [x] `GetEnvironments_WithStorageModeUser_ReturnsOnlyOwnedEnvironments` in `SystemEnvironmentRepositoryIntegrationTests` — vorhanden
- [x] `GetEnvironments_WithStorageModeTeam_ReturnsAllTeamEnvironments` in `SystemEnvironmentRepositoryIntegrationTests` — vorhanden
- [x] `UpdateEnvironment_PersistsChanges` in `SystemEnvironmentRepositoryIntegrationTests` — vorhanden
- [x] `BuildRequest_ResolvesDoubleBracePlaceholdersBeforeSingleBrace` in `EndpointExecutionServiceTests` — vorhanden
- [x] `BuildRequest_MissingVariable_ReplacesWithEmptyString` in `EndpointExecutionServiceTests` — vorhanden
- [x] `BuildRequest_NoActiveEnvironment_ReplacesAllDoubleBracePlaceholdersWithEmptyString` in `EndpointExecutionServiceTests` — vorhanden
- [x] `BuildRequest_ResolvesPlaceholdersInBaseUrl` in `EndpointExecutionServiceTests` — vorhanden
- [x] `BuildRequest_ResolvesPlaceholdersInRelativePath` in `EndpointExecutionServiceTests` — vorhanden
- [x] `BuildRequest_ResolvesPlaceholdersInHeaderNamesAndValues` in `EndpointExecutionServiceTests` — vorhanden
- [x] `BuildRequest_ResolvesPlaceholdersInQueryParameterNamesAndValues` in `EndpointExecutionServiceTests` — vorhanden
- [x] `BuildRequest_ResolvesPlaceholdersInBearerToken` in `EndpointExecutionServiceTests` — vorhanden
- [x] `BuildRequest_ResolvesPlaceholdersInBody` in `EndpointExecutionServiceTests` — vorhanden
- [x] `UmgebungMitVariable_Aktivieren_EndpunktSendetAufgeloestUrl` (Playwright) — vorhanden (in `EndpointExecutionTests`)
- [x] `MaskierterWert_IstNichtImKlartextImDomSichtbar` (Playwright) — vorhanden (in `EnvironmentManagementTests`)
- [x] `ExecuteWithTwoSystemEnvironmentRepositoriesAsync` in `TestHelpers` — vorhanden

## Offene Aufgaben

Keine.

## Hinweise

- Der Bearer-Token wird plankonform durch `ResolvePlaceholders` aufgelöst, jedoch geschieht dies in `ApplyAuthentication` statt in `BuildRequest`. Der Plan ordnet die Auflösung explizit `BuildRequest` zu; die funktionale Wirkung ist identisch.
- Das SignalR-Abonnement für `EnvironmentChanged` wird im Plan als Teil von `OnInitialized` beschrieben, ist aber in `ConnectHubAsync` implementiert, das in `OnAfterRenderAsync` (beim ersten Render) aufgerufen wird. Funktional äquivalent, da die Verbindung ebenfalls beim Start der Komponente aufgebaut wird.
- Mocks von `ISignalRNotificationService` in bestehenden Tests (`ControllerTestFactory`, `PlaywrightTestFactory`, `MainLayoutTests`, `EndpointPageTests`) verwenden `new Mock<ISignalRNotificationService>()` ohne `MockBehavior.Strict`. Moq erzeugt damit automatisch eine leere Stub-Implementierung für `NotifyEnvironmentChangedAsync`, sodass kein Test-Bruch durch das neue Interface-Mitglied entsteht.
