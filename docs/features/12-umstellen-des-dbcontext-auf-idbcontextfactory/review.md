# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

### Schritt 1 – `DatabaseProviderFactory` auf `AddDbContextFactory` umstellen

- [x] SQLite-Zweig: `AddDbContext<AppDbContext>` → `AddDbContextFactory<AppDbContext>` — umgestellt
- [x] SQL-Server-Zweig: `AddDbContext<AppDbContext>` → `AddDbContextFactory<AppDbContext>` — umgestellt

### Schritt 2 – `ApplicationRepository` auf `IDbContextFactory` umstellen

- [x] Feld `AppDbContext _context` → `IDbContextFactory<AppDbContext> _factory` — ersetzt
- [x] Konstruktor: Parameter `AppDbContext context` → `IDbContextFactory<AppDbContext> factory` — ersetzt
- [x] `GetGroupsAsync` — `await using var context = await _factory.CreateDbContextAsync()` vorhanden, kein `AsNoTracking()`
- [x] `GetGroupByIdAsync` — lokaler Context, kein `AsNoTracking()`
- [x] `GetSystemGroupAsync` — lokaler Context, kein `AsNoTracking()`
- [x] `AddGroupAsync` — lokaler Context
- [x] `UpdateGroupAsync` — lokaler Context, kein `ChangeTracker.Entries` / `EntityState.Detached`-Workaround
- [x] `DeleteGroupAsync` — lokaler Context
- [x] `GetApplicationsAsync` — lokaler Context, kein `AsNoTracking()`
- [x] `GetUngroupedApplicationsAsync` — lokaler Context, kein `AsNoTracking()`
- [x] `GetApplicationByIdAsync` — lokaler Context, kein `AsNoTracking()`
- [x] `AddApplicationAsync` — lokaler Context
- [x] `UpdateApplicationAsync` — lokaler Context, kein `ChangeTracker.Entries` / `EntityState.Detached`, kein `application.ApplicationGroup = null`
- [x] `DeleteApplicationAsync` — lokaler Context

### Schritt 3 – `EndpointRepository` auf `IDbContextFactory` umstellen

- [x] Feld `AppDbContext _context` → `IDbContextFactory<AppDbContext> _factory` — ersetzt
- [x] Konstruktor: Parameter `AppDbContext context` → `IDbContextFactory<AppDbContext> factory` — ersetzt
- [x] `GetEndpointsAsync` — lokaler Context, kein `AsNoTracking()`
- [x] `GetEndpointByIdAsync` — lokaler Context, kein `AsNoTracking()`
- [x] `AddEndpointAsync` — lokaler Context, kein `ChangeTracker.Clear()`, kein `EntityState.Detached`
- [x] `UpdateEndpointAsync` — lokaler Context, kein `ChangeTracker.Clear()`, kein `EntityState.Detached`
- [x] `DeleteEndpointAsync` — delegiert an `DeleteByIdAsync<T>(int id)`
- [x] `GetEndpointGroupsAsync` — lokaler Context, kein `AsNoTracking()`
- [x] `GetEndpointGroupByIdAsync` — lokaler Context, kein `AsNoTracking()`
- [x] `AddEndpointGroupAsync` — lokaler Context, kein `ChangeTracker.Clear()`, kein `EntityState.Detached`
- [x] `UpdateEndpointGroupAsync` — lokaler Context, kein `ChangeTracker.Clear()`, kein `EntityState.Detached`
- [x] `DeleteEndpointGroupAsync` — delegiert an `DeleteByIdAsync<T>(int id)`
- [x] `AddHeaderAsync` — lokaler Context, kein `EntityState.Detached`
- [x] `DeleteHeaderAsync` — delegiert an `DeleteByIdAsync<T>(int id)`
- [x] `AddQueryParameterAsync` — lokaler Context, kein `EntityState.Detached`
- [x] `DeleteQueryParameterAsync` — delegiert an `DeleteByIdAsync<T>(int id)`
- [x] `DeleteByIdAsync<T>(int id)` — vereinfachte Signatur (kein `DbSet<T>`-Parameter), erstellt Context selbst via `_factory.CreateDbContextAsync()`

### Testanpassungen – `TestHelpers`

- [x] `CreateInMemoryDbContext()` — Rückgabewert ist `(IDbContextFactory<AppDbContext>, SqliteConnection)` via `FixedOptionsDbContextFactory`
- [x] `ExecuteWithTwoContextsAsync` — Signatur `Func<ApplicationRepository, ApplicationRepository, Task>` unverändert, zwei `FixedOptionsDbContextFactory`-Instanzen intern
- [x] `ExecuteWithTwoEndpointContextsAsync` — neue Signatur `Func<EndpointRepository, EndpointRepository, Task>` (kein Tupel mit `AppDbContext` mehr), zwei `FixedOptionsDbContextFactory`-Instanzen intern

### Testanpassungen – `DatabaseProviderFactoryTests`

- [x] `CreateSqliteContext_ReturnsSqliteDbContext` — löst `IDbContextFactory<AppDbContext>` auf, erstellt Context via `factory.CreateDbContext()`
- [x] `CreateSqlServerContext_ReturnsSqlServerDbContext` — analog

### Testanpassungen – `ApplicationRepositoryIntegrationTests`

- [x] `ExecuteWithContextAsync` — erstellt `ApplicationRepository` mit Factory aus `TestHelpers.CreateInMemoryDbContext()`, kein direkter `AppDbContext`-Parameter
- [x] Alle 21 Tests: kein direkter `AppDbContext`-Zugriff, keine Tracking-State-Prüfungen

### Testanpassungen – `EndpointRepositoryIntegrationTests`

- [x] Alle 6 Tests: verwenden `factory` aus `TestHelpers.CreateInMemoryDbContext()`, kein `EntityState.Detached` im Test-Setup-Code
- [x] `SaveEndpoint_ConcurrentWrite_DetectsConflict` — instanziiert zwei `EndpointRepository` direkt mit derselben Factory (kein `ExecuteWithTwoEndpointContextsAsync` nötig, da Factories geteilt werden)
- [x] Aufrufe von `ExecuteWithTwoEndpointContextsAsync` — entfallen vollständig; Tests nutzen stattdessen direkte `factory`-Instanziierung

## Offene Aufgaben

(keine)
