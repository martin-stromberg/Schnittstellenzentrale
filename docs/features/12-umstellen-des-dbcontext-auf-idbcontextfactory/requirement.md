# Anforderung: DbContext-Workarounds durch IDbContextFactory ersetzen

## Zusammenfassung

Die direkte `AppDbContext`-Injektion in den Repositories wird durch `IDbContextFactory<AppDbContext>` ersetzt. Jede Datenbankoperation erhält einen eigenen kurzlebigen Context, der nach der Operation sofort disposed wird. Alle bestehenden Workarounds (`AsNoTracking()`, `ChangeTracker.Clear()`, `EntityState.Detached`) werden entfernt, da sie durch das neue Pattern obsolet werden.

## Auslöser und Akteure

- **Auslöser:** Technisches Refactoring — bestehende Workarounds gegen Blazor-Server-Circuit-Lifetime-Konflikte sollen durch die von Microsoft empfohlene Lösung ersetzt werden
- **Akteure:** Entwickler (kein Endnutzer-Impact)

## Beschreibung

- `DatabaseProviderFactory.RegisterDbContext()` wird auf `AddDbContextFactory<AppDbContext>()` umgestellt (beide Provider: SQLite und SQL Server)
- `ApplicationRepository` und `EndpointRepository` injizieren statt `AppDbContext` nun `IDbContextFactory<AppDbContext>`
- Jede Repository-Methode erstellt einen eigenen Context via `await using var context = await _factory.CreateDbContextAsync()` und disposed ihn am Ende der Methode
- Alle Workarounds werden entfernt: `AsNoTracking()` (13+ Stellen), `ChangeTracker.Clear()` (5 Stellen), `EntityState.Detached` (10+ Stellen)
- Die Integrationstests in `Schnittstellenzentrale.Tests` werden entsprechend angepasst

## Eingaben und Ausgaben

- **Eingaben:** Keine neuen Eingaben — bestehende Repository-Interfaces (`IApplicationRepository`, `IEndpointRepository`) bleiben unverändert
- **Ausgaben/Ergebnisse:** Funktional identisches Verhalten, jedoch ohne Tracking-Konflikte durch langlebigen Context

## Fehlerbehandlung

Keine Änderung zur bisherigen Fehlerbehandlung — Exceptions aus EF Core propagieren wie bisher.

## Abgrenzung

- `AppDbContextFactory.cs` (Design-Time-Factory für EF Core Migrations) wird nicht angefasst
- Die Repository-Interfaces (`IApplicationRepository`, `IEndpointRepository`) in `Schnittstellenzentrale.Core` bleiben unverändert
- Keine fachlichen Änderungen an Queries oder Datenbankschema
- Kein Einführen von Unit-of-Work oder geteilten Transactions über Repository-Grenzen hinweg

## Akzeptanzkriterien

- [ ] `DatabaseProviderFactory` registriert `IDbContextFactory<AppDbContext>` statt `AppDbContext` direkt
- [ ] `ApplicationRepository` verwendet ausschließlich factory-erstellte, kurzlebige Contexts
- [ ] `EndpointRepository` verwendet ausschließlich factory-erstellte, kurzlebige Contexts
- [ ] Kein `AsNoTracking()`, `ChangeTracker.Clear()` oder `EntityState.Detached` mehr im Produktivcode
- [ ] Alle bestehenden Integrationstests laufen grün
- [ ] Integrationstests verwenden ebenfalls `IDbContextFactory` (kein direktes `AppDbContext`)
