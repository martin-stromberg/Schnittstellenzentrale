# Umsetzungsplan: DbContext-Workarounds durch IDbContextFactory ersetzen

## Zusammenfassung

`ApplicationRepository` und `EndpointRepository` werden von der direkten `AppDbContext`-Injektion auf `IDbContextFactory<AppDbContext>` umgestellt. Jede Repository-Methode erhält einen eigenen kurzlebigen Context, der nach der Operation sofort disposed wird. Damit werden alle bestehenden Tracking-Workarounds (`AsNoTracking()`, `ChangeTracker.Clear()`, `EntityState.Detached`) obsolet und können vollständig entfernt werden.

---

## Offene Punkte

Keine.

---

## Betroffene Dateien

- `src/Schnittstellenzentrale.Infrastructure/Data/DatabaseProviderFactory.cs` — Geändert
- `src/Schnittstellenzentrale.Infrastructure/Repositories/ApplicationRepository.cs` — Geändert
- `src/Schnittstellenzentrale.Infrastructure/Repositories/EndpointRepository.cs` — Geändert
- `src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs` — Geändert
- `src/Schnittstellenzentrale.Tests/Services/DatabaseProviderFactoryTests.cs` — Geändert
- `src/Schnittstellenzentrale.Tests/Integration/ApplicationRepositoryIntegrationTests.cs` — Geändert
- `src/Schnittstellenzentrale.Tests/Integration/EndpointRepositoryIntegrationTests.cs` — Geändert

---

## Umsetzungsschritte

### Schritt 1: `DatabaseProviderFactory` auf `AddDbContextFactory` umstellen

Datei: `src/Schnittstellenzentrale.Infrastructure/Data/DatabaseProviderFactory.cs`

In der Methode `RegisterDbContext(IServiceCollection, IConfiguration)` werden beide Provider-Zweige (SQLite und SQL Server) von `AddDbContext<AppDbContext>(...)` auf `AddDbContextFactory<AppDbContext>(...)` umgestellt. Die Options-Konfiguration (Connection-String, Migrations-Assembly-Filter) bleibt identisch — nur der Registrierungsaufruf ändert sich.

```csharp
// Vorher
services.AddDbContext<AppDbContext>(options => options.UseSqlite(...));

// Nachher
services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(...));
```

Gleiches gilt für den SQL-Server-Zweig.

> Hinweis: `AddDbContextFactory` registriert implizit auch `IDbContextFactory<AppDbContext>` und `DbContextOptions<AppDbContext>`. `AppDbContext` selbst ist danach nicht mehr direkt als Scoped-Service im Container verfügbar — was für die Repositories gewünscht ist. Der bestehende Aufruf in `EnsureDatabaseInitializedAsync` in `Program.cs` (Zeile 109–114) löst `AppDbContext` über einen eigenen Scope auf und ist vom Refactoring nicht betroffen.

---

### Schritt 2: `ApplicationRepository` auf `IDbContextFactory` umstellen

Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/ApplicationRepository.cs`

#### 2a: Konstruktor anpassen

Das Feld `AppDbContext _context` wird durch `IDbContextFactory<AppDbContext> _factory` ersetzt. Der Konstruktorparameter wird entsprechend geändert.

```csharp
// Vorher
private readonly AppDbContext _context;
public ApplicationRepository(AppDbContext context) { _context = context; }

// Nachher
private readonly IDbContextFactory<AppDbContext> _factory;
public ApplicationRepository(IDbContextFactory<AppDbContext> factory) { _factory = factory; }
```

#### 2b: Alle Methoden auf lokalen Context umstellen

Jede der folgenden Methoden erhält am Anfang des Methodenrumpfs eine `await using`-Deklaration und verwendet anschließend die lokale Variable `context` anstelle von `_context`:

- `GetGroupsAsync` — ersetzt `_context` durch `context`, entfernt `AsNoTracking()`
- `GetGroupByIdAsync` — ersetzt `_context` durch `context`, entfernt `AsNoTracking()`
- `GetSystemGroupAsync` — ersetzt `_context` durch `context`, entfernt `AsNoTracking()`
- `AddGroupAsync` — ersetzt `_context` durch `context`
- `UpdateGroupAsync` — ersetzt `_context` durch `context`, entfernt `ChangeTracker.Entries<ApplicationGroup>()` + `EntityState.Detached`-Workaround
- `DeleteGroupAsync` — ersetzt `_context` durch `context`
- `GetApplicationsAsync` — ersetzt `_context` durch `context`, entfernt `AsNoTracking()`
- `GetUngroupedApplicationsAsync` — ersetzt `_context` durch `context`, entfernt `AsNoTracking()`
- `GetApplicationByIdAsync` — ersetzt `_context` durch `context`, entfernt `AsNoTracking()`
- `AddApplicationAsync` — ersetzt `_context` durch `context`
- `UpdateApplicationAsync` — ersetzt `_context` durch `context`, entfernt `ChangeTracker.Entries<Application>()` + `EntityState.Detached` (App), `ChangeTracker.Entries<ApplicationGroup>()` + `EntityState.Detached` (Group) und `application.ApplicationGroup = null`
- `DeleteApplicationAsync` — ersetzt `_context` durch `context`

Muster für jede Methode:

```csharp
public async Task<...> XxxAsync(...)
{
    await using var context = await _factory.CreateDbContextAsync();
    // bisherige Logik mit context statt _context, ohne Workarounds
}
```

Da `DeleteGroupAsync` und `DeleteApplicationAsync` bisher intern über `FindAsync` und `Remove` arbeiten (keine separate Hilfsmethode wie in `EndpointRepository`), wird der Context ebenfalls lokal erstellt — keine weiteren strukturellen Änderungen notwendig.

---

### Schritt 3: `EndpointRepository` auf `IDbContextFactory` umstellen

Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/EndpointRepository.cs`

#### 3a: Konstruktor anpassen

Analog zu Schritt 2a: `AppDbContext _context` → `IDbContextFactory<AppDbContext> _factory`.

#### 3b: Alle öffentlichen Methoden auf lokalen Context umstellen

Jede Methode erhält einen lokalen `await using`-Context. Workarounds werden entfernt:

- `GetEndpointsAsync` — entfernt `AsNoTracking()`
- `GetEndpointByIdAsync` — entfernt `AsNoTracking()`
- `AddEndpointAsync` — entfernt `ChangeTracker.Clear()` vor Insert und `EntityState.Detached` nach `SaveChanges`
- `UpdateEndpointAsync` — entfernt `ChangeTracker.Clear()` vor Update und `EntityState.Detached` nach `SaveChanges`
- `DeleteEndpointAsync` — ruft weiterhin `DeleteByIdAsync` auf (siehe 3c)
- `GetEndpointGroupsAsync` — entfernt `AsNoTracking()`
- `GetEndpointGroupByIdAsync` — entfernt `AsNoTracking()`
- `AddEndpointGroupAsync` — entfernt `ChangeTracker.Clear()` und `EntityState.Detached`
- `UpdateEndpointGroupAsync` — entfernt `ChangeTracker.Clear()` und `EntityState.Detached`
- `DeleteEndpointGroupAsync` — ruft weiterhin `DeleteByIdAsync` auf (siehe 3c)
- `AddHeaderAsync` — entfernt `EntityState.Detached`
- `DeleteHeaderAsync` — ruft weiterhin `DeleteByIdAsync` auf (siehe 3c)
- `AddQueryParameterAsync` — entfernt `EntityState.Detached`
- `DeleteQueryParameterAsync` — ruft weiterhin `DeleteByIdAsync` auf (siehe 3c)

#### 3c: Private Hilfsmethode `DeleteByIdAsync<T>` anpassen

`DeleteByIdAsync<T>(DbSet<T>, int)` erhält keinen `DbSet` mehr als Parameter, da der Context jetzt pro Aufruf lokal ist. Die Methode wird auf `DeleteByIdAsync<T>(int id)` vereinfacht und erstellt ihren Context selbst:

```csharp
private async Task DeleteByIdAsync<T>(int id) where T : class
{
    await using var context = await _factory.CreateDbContextAsync();
    var entity = await context.Set<T>().FindAsync(id);
    if (entity is not null)
    {
        context.Set<T>().Remove(entity);
        await context.SaveChangesAsync();
    }
}
```

Alle Aufrufer (`DeleteEndpointAsync`, `DeleteEndpointGroupAsync`, `DeleteHeaderAsync`, `DeleteQueryParameterAsync`) werden entsprechend angepasst.

---

## Testanpassungen

### `TestHelpers`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs`

#### `CreateInMemoryDbContext()`

Die Methode wird auf eine Factory-basierte Variante umgestellt. Sie erstellt weiterhin eine `SqliteConnection` und einen `DbContextOptions<AppDbContext>`, erzeugt daraus aber eine `IDbContextFactory<AppDbContext>`-Instanz (z. B. via `new PooledDbContextFactory<AppDbContext>(options)` oder einer einfachen `TestDbContextFactory`-Hilfsklasse) und ruft `EnsureCreated()` über einen initialen Context auf.

Rückgabewert: `(IDbContextFactory<AppDbContext>, SqliteConnection)` statt `(AppDbContext, SqliteConnection)`.

> Da `PooledDbContextFactory` intern eine Connection-Option via `DbContextOptions` verwendet, muss die `SqliteConnection` weiterhin von außen offen gehalten und nach dem Test disposed werden — dies bleibt unverändert verantwortlich beim Aufrufer.

#### `ExecuteWithTwoContextsAsync`

Signatur: `Func<ApplicationRepository, ApplicationRepository, Task>` bleibt gleich.

Intern werden zwei `IDbContextFactory<AppDbContext>`-Instanzen (beide auf derselben `SqliteConnection`) erstellt und an je einen `ApplicationRepository`-Konstruktor übergeben. Der direkte `AppDbContext`-Parameter entfällt vollständig.

#### `ExecuteWithTwoEndpointContextsAsync`

Die bisherige Signatur `Func<(AppDbContext, EndpointRepository), (AppDbContext, EndpointRepository), Task>` wird angepasst, da Tests keinen direkten Zugriff auf `AppDbContext` mehr benötigen (Workarounds werden entfernt). Die neue Signatur lautet `Func<EndpointRepository, EndpointRepository, Task>`. Beide Repositories werden mit je einer eigenen Factory auf derselben Connection instanziiert.

---

### `DatabaseProviderFactoryTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/DatabaseProviderFactoryTests.cs`

Beide Tests (`CreateSqliteContext_ReturnsSqliteDbContext`, `CreateSqlServerContext_ReturnsSqlServerDbContext`) lösen bisher `AppDbContext` direkt aus dem Container auf. Nach dem Refactoring wird stattdessen `IDbContextFactory<AppDbContext>` aufgelöst und ein Context via `factory.CreateDbContext()` erstellt. Die Prüfung des Provider-Typs (`context.Database.ProviderName`) bleibt identisch.

```csharp
// Vorher
var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

// Nachher
var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
await using var context = factory.CreateDbContext();
```

---

### `ApplicationRepositoryIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ApplicationRepositoryIntegrationTests.cs`

Die private Hilfsmethode `ExecuteWithContextAsync` erstellt intern einen `ApplicationRepository` mit `AppDbContext` direkt — nach der Umstellung erhält sie stattdessen eine `IDbContextFactory<AppDbContext>` aus `TestHelpers.CreateInMemoryDbContext()`.

`ExecuteWithTwoContextsAsync` liefert weiterhin zwei `ApplicationRepository`-Instanzen (Signatur unverändert, Implementierung in `TestHelpers` angepasst).

Alle Tests, die nach dem Repository-Aufruf direkt auf `AppDbContext`-Zustand zugreifen (z. B. Tracking-State prüfen), entfallen oder enthalten diesen Zugriff bereits nicht — laut Inventory enthält keiner der Tests expliziten Tracking-State-Zugriff, sondern prüft nur persistierte Daten.

---

### `EndpointRepositoryIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/EndpointRepositoryIntegrationTests.cs`

Tests, die bisher `EntityState.Detached` im Setup-Code setzen, entfernen diese Zeilen (da der neue kurzlebige Context keinen Tracking-Zustand über Methodenaufrufe hinweg trägt).

`ExecuteWithTwoEndpointContextsAsync` liefert nach der `TestHelpers`-Anpassung zwei `EndpointRepository`-Instanzen statt Tupeln. Alle Aufrufer werden entsprechend angepasst: Zugriffe auf das `AppDbContext`-Element des Tupels werden entfernt.

---

## Akzeptanzkriterien-Mapping

| Akzeptanzkriterium | Umsetzungsschritt(e) |
|--------------------|----------------------|
| `DatabaseProviderFactory` registriert `IDbContextFactory<AppDbContext>` statt `AppDbContext` direkt | Schritt 1 |
| `ApplicationRepository` verwendet ausschließlich factory-erstellte, kurzlebige Contexts | Schritte 2a, 2b |
| `EndpointRepository` verwendet ausschließlich factory-erstellte, kurzlebige Contexts | Schritte 3a, 3b, 3c |
| Kein `AsNoTracking()`, `ChangeTracker.Clear()` oder `EntityState.Detached` mehr im Produktivcode | Schritte 2b, 3b |
| Alle bestehenden Integrationstests laufen grün | Testanpassungen (alle Abschnitte) |
| Integrationstests verwenden ebenfalls `IDbContextFactory` (kein direktes `AppDbContext`) | Testanpassungen: `TestHelpers`, `ApplicationRepositoryIntegrationTests`, `EndpointRepositoryIntegrationTests` |
