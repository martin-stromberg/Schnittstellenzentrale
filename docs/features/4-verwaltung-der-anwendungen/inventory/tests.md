# Tests

## Testklassen

### `ApplicationRepositoryIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ApplicationRepositoryIntegrationTests.cs`

- `GetApplications_WithStorageModeUser_ReturnsOnlyUserData` — Prüft, dass `GetApplicationsAsync` bei `StorageMode.User` nur Anwendungen des angegebenen Owners zurückgibt.
- `GetApplications_WithStorageModeTeam_ReturnsTeamData` — Prüft, dass `GetApplicationsAsync` bei `StorageMode.Team` alle Anwendungen unabhängig vom Owner zurückgibt.

Nicht vorhanden: Tests für `AddGroupAsync`, `AddApplicationAsync`, `GetGroupsAsync`, `GetUngroupedApplicationsAsync`, `UpdateGroupAsync`, `UpdateApplicationAsync`, `DeleteGroupAsync`, `DeleteApplicationAsync`.

## Hilfsmethoden

### `TestHelpers`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs`

- `CreateInMemoryDbContext()` — Erstellt einen `AppDbContext` mit SQLite In-Memory-Datenbank (offene `SqliteConnection`). Rückgabe: `(AppDbContext, SqliteConnection)`. Beide Objekte müssen vom Aufrufer in der richtigen Reihenfolge disposed werden (erst Context, dann Connection).
