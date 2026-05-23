# Logik

## `DatabaseProviderFactory`
Datei: `src/Schnittstellenzentrale.Infrastructure/Data/DatabaseProviderFactory.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `RegisterDbContext(IServiceCollection, IConfiguration)` | `public static` | Liest `DatabaseProvider` aus Konfiguration, registriert `AppDbContext` via `AddDbContext<AppDbContext>()` für SQLite oder SQL Server mit gefilterten Migrations-Assemblies |

**Zu änderndes Verhalten:** `AddDbContext<AppDbContext>()` → `AddDbContextFactory<AppDbContext>()` für beide Provider-Zweige.

---

## `ApplicationRepository`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/ApplicationRepository.cs`

Implementiert `IApplicationRepository`. Injiziert `AppDbContext _context` über den Konstruktor.

| Methode | Sichtbarkeit | Workarounds | Kurzbeschreibung |
|---------|-------------|-------------|------------------|
| `GetGroupsAsync(StorageMode, string)` | `public async` | `AsNoTracking()` | Lädt alle `ApplicationGroup`-Entitäten inkl. `Applications`; bei `StorageMode.User` werden nur Gruppen mit eigenen Anwendungen zurückgegeben |
| `GetGroupByIdAsync(int)` | `public async` | `AsNoTracking()` | Lädt eine Gruppe per Id inkl. `Applications` |
| `GetSystemGroupAsync()` | `public async` | `AsNoTracking()` | Lädt die Gruppe mit `IsSystem = true` inkl. `Applications` |
| `AddGroupAsync(ApplicationGroup)` | `public async` | — | Fügt eine neue Gruppe ein und speichert |
| `UpdateGroupAsync(ApplicationGroup)` | `public async` | `ChangeTracker.Entries<ApplicationGroup>()` + `EntityState.Detached` | Detacht ggf. bereits getrackte Instanz derselben Id, ruft dann `Update()` auf |
| `DeleteGroupAsync(int)` | `public async` | — | Lädt per `FindAsync`, entfernt und speichert |
| `GetApplicationsAsync(StorageMode, string)` | `public async` | `AsNoTracking()` | Lädt alle `Application`-Entitäten inkl. `ApplicationGroup`, optional gefiltert nach Owner |
| `GetUngroupedApplicationsAsync(StorageMode, string)` | `public async` | `AsNoTracking()` | Wie `GetApplicationsAsync`, aber nur ungrouped (`ApplicationGroupId == null`) |
| `GetApplicationByIdAsync(int)` | `public async` | `AsNoTracking()` | Lädt eine Anwendung per Id inkl. vollständigem Navigationsgraph (Group, Endpoints, Headers, QueryParameters, EndpointGroups) |
| `AddApplicationAsync(Application)` | `public async` | — | Fügt eine neue Anwendung ein und speichert |
| `UpdateApplicationAsync(Application)` | `public async` | `ChangeTracker.Entries<Application>()` + `EntityState.Detached` (App), `ChangeTracker.Entries<ApplicationGroup>()` + `EntityState.Detached` (Group), `application.ApplicationGroup = null` | Komplex: detacht App und ggf. zugehörige Gruppe, setzt Navigation auf `null` um Tracking-Konflikt zu vermeiden |
| `DeleteApplicationAsync(int)` | `public async` | — | Lädt per `FindAsync`, entfernt und speichert |
| `ApplyOwnerFilter(IQueryable<Application>, StorageMode, string)` | `private static` | — | Filtert Query nach Owner wenn `StorageMode.User` |

**Gesamte Workarounds in dieser Klasse:**
- 8× `AsNoTracking()`
- 2× `EntityState.Detached` (direkte Zuweisung auf `ChangeTracker.Entries`-Ergebnis)
- 2× `ChangeTracker.Entries<T>()` (lesender Zugriff auf den Tracker)
- 1× `application.ApplicationGroup = null` (Workaround gegen Navigation-Fixup-Konflikt)

---

## `EndpointRepository`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/EndpointRepository.cs`

Implementiert `IEndpointRepository`. Injiziert `AppDbContext _context` über den Konstruktor.

| Methode | Sichtbarkeit | Workarounds | Kurzbeschreibung |
|---------|-------------|-------------|------------------|
| `GetEndpointsAsync(int)` | `public async` | `AsNoTracking()` | Lädt alle Endpunkte einer Anwendung inkl. Headers, QueryParameters, EndpointGroup |
| `GetEndpointByIdAsync(int)` | `public async` | `AsNoTracking()` | Lädt einen Endpunkt per Id inkl. Application, Headers, QueryParameters, EndpointGroup |
| `AddEndpointAsync(Endpoint)` | `public async` | `ChangeTracker.Clear()`, `EntityState.Detached` | Cleared Tracker vor Insert, detacht nach SaveChanges |
| `UpdateEndpointAsync(Endpoint)` | `public async` | `ChangeTracker.Clear()`, `EntityState.Detached` | Cleared Tracker vor Update, detacht nach SaveChanges |
| `DeleteEndpointAsync(int)` | `public async` | — | Delegiert an `DeleteByIdAsync` |
| `GetEndpointGroupsAsync(int)` | `public async` | `AsNoTracking()` | Lädt alle Endpunktgruppen einer Anwendung |
| `GetEndpointGroupByIdAsync(int)` | `public async` | `AsNoTracking()` | Lädt eine Endpunktgruppe per Id inkl. Endpoints |
| `AddEndpointGroupAsync(EndpointGroup)` | `public async` | `ChangeTracker.Clear()`, `EntityState.Detached` | Cleared Tracker vor Insert, detacht nach SaveChanges |
| `UpdateEndpointGroupAsync(EndpointGroup)` | `public async` | `ChangeTracker.Clear()`, `EntityState.Detached` | Cleared Tracker vor Update, detacht nach SaveChanges |
| `DeleteEndpointGroupAsync(int)` | `public async` | — | Delegiert an `DeleteByIdAsync` |
| `AddHeaderAsync(EndpointHeader)` | `public async` | `EntityState.Detached` | Fügt Header ein, detacht nach SaveChanges |
| `DeleteHeaderAsync(int)` | `public async` | — | Delegiert an `DeleteByIdAsync` |
| `AddQueryParameterAsync(EndpointQueryParameter)` | `public async` | `EntityState.Detached` | Fügt QueryParameter ein, detacht nach SaveChanges |
| `DeleteQueryParameterAsync(int)` | `public async` | — | Delegiert an `DeleteByIdAsync` |
| `DeleteByIdAsync<T>(DbSet<T>, int)` | `private async` | — | Generische Hilfsmethode: lädt per `FindAsync`, entfernt und speichert |

**Gesamte Workarounds in dieser Klasse:**
- 4× `AsNoTracking()`
- 4× `ChangeTracker.Clear()`
- 6× `EntityState.Detached`

---

## `Program` (Startup)
Datei: `src/Schnittstellenzentrale/Program.cs`

Relevante Stellen:

| Stelle | Zeile | Beschreibung |
|--------|-------|--------------|
| `DatabaseProviderFactory.RegisterDbContext(...)` | 62 | Registriert den Db-Context (aktuell via `AddDbContext`) |
| `AddScoped<IApplicationRepository, ApplicationRepository>()` | 64 | Repository-Registrierung |
| `AddScoped<IEndpointRepository, EndpointRepository>()` | 65 | Repository-Registrierung |
| `EnsureDatabaseInitializedAsync` | 109–114 | Löst `AppDbContext` direkt aus Scope auf und ruft `MigrateAsync()` — außerhalb des Refactoring-Scopes |
