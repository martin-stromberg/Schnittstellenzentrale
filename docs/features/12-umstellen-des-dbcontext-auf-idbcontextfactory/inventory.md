# Bestandsaufnahme: DbContext-Workarounds durch IDbContextFactory ersetzen

Analysiert wurden die Datenbankregistrierung, die beiden Repository-Implementierungen sowie deren Integrationstests, bezogen auf das Refactoring von `AppDbContext`-Direktinjektion zu `IDbContextFactory<AppDbContext>`.

## Zusammenfassung

- `DatabaseProviderFactory.RegisterDbContext()` verwendet `AddDbContext<AppDbContext>()` für beide Provider — muss auf `AddDbContextFactory<AppDbContext>()` umgestellt werden
- `ApplicationRepository` injiziert `AppDbContext` direkt und enthält 8× `AsNoTracking()` sowie Tracking-Workarounds in `UpdateGroupAsync` und `UpdateApplicationAsync` (je 1–2× `EntityState.Detached` via `ChangeTracker.Entries`)
- `EndpointRepository` injiziert `AppDbContext` direkt und enthält 4× `AsNoTracking()`, 4× `ChangeTracker.Clear()` und 6× `EntityState.Detached`
- `Program.cs` registriert beide Repositories als `AddScoped` und verwendet `AppDbContext` direkt in `EnsureDatabaseInitializedAsync` (Migration-Infrastruktur — außerhalb des Refactoring-Scopes)
- `TestHelpers` erstellt `AppDbContext`-Instanzen direkt und übergibt sie an die Repository-Konstruktoren — muss auf `IDbContextFactory<AppDbContext>` umgestellt werden
- `DatabaseProviderFactoryTests` prüft Auflösung von `AppDbContext` aus dem DI-Container — muss auf `IDbContextFactory<AppDbContext>` angepasst werden
- `AppDbContextFactory.cs` (Design-Time) bleibt unverändert; die Repository-Interfaces bleiben unverändert

## Details

- [Logik](inventory/logic.md)
- [Interfaces](inventory/interfaces.md)
- [Tests](inventory/tests.md)
