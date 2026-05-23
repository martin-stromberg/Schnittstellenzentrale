# Tests

## Testklassen

### `DatabaseProviderFactoryTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/DatabaseProviderFactoryTests.cs`

Löst `AppDbContext` direkt aus dem DI-Container auf — muss nach dem Refactoring auf `IDbContextFactory<AppDbContext>` umgestellt werden.

- `CreateSqliteContext_ReturnsSqliteDbContext` — Prüft, dass `AppDbContext` mit SQLite-Provider registriert wird und aus dem Container auflösbar ist
- `CreateSqlServerContext_ReturnsSqlServerDbContext` — Prüft, dass `AppDbContext` mit SQL-Server-Provider registriert wird und aus dem Container auflösbar ist

### `ApplicationRepositoryIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ApplicationRepositoryIntegrationTests.cs`

Nutzt intern `ExecuteWithContextAsync` (privat) und `TestHelpers.ExecuteWithTwoContextsAsync` — beide übergeben `AppDbContext` direkt an den `ApplicationRepository`-Konstruktor.

- `GetApplications_WithStorageModeUser_ReturnsOnlyUserData` — Owner-Filter für `StorageMode.User`
- `GetApplications_WithStorageModeTeam_ReturnsTeamData` — Kein Filter bei `StorageMode.Team`
- `AddGroup_PersistsNewGroup` — Persistenz einer neuen Gruppe
- `AddApplication_WithGroup_PersistsApplication` — Persistenz mit Gruppenreferenz
- `AddApplication_WithoutGroup_PersistsUngroupedApplication` — Persistenz ohne Gruppe
- `AddApplication_WithStorageModeUser_FiltersToCurrentOwner` — Owner-Zuweisung bei User-Mode
- `UpdateGroup_RenamesGroup` — Umbenennung einer Gruppe
- `UpdateApplication_ChangesGroup` — Wechsel der Gruppe einer Anwendung
- `UpdateApplication_SetsGroupToNull` — Entfernen der Gruppenzuordnung
- `DeleteGroup_SetsMemberApplicationsGroupless` — Cascade-Verhalten beim Löschen einer Gruppe
- `DeleteApplication_RemovesApplication` — Löschen einer Anwendung
- `DeleteGroup_WithApplicationsDeletedFirst_RemovesGroupAndApplications` — Reihenfolge-Test für Delete
- `GetGroups_WithStorageModeUser_ReturnsOnlyGroupsWithOwnedApplications` — Gruppen-Filter bei User-Mode
- `GetUngroupedApplications_WithStorageModeUser_ReturnsOnlyOwnUngroupedApplications` — Ungrouped-Filter bei User-Mode
- `UpdateApplication_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException` — Concurrency-Konflikt via `ExecuteWithTwoContextsAsync`
- `UpdateGroup_WithNewInstanceAfterAdd_ShouldRenameGroup` — Tracking-Robustheit nach Add
- `UpdateGroup_WithNewInstanceAfterGetGroups_ShouldRenameGroup` — Tracking-Robustheit nach Get
- `UpdateApplication_WithNewInstanceAfterAdd_ShouldUpdateApplication` — Tracking-Robustheit nach Add
- `UpdateApplication_WithNewInstanceAfterGetApplications_ShouldUpdateApplication` — Tracking-Robustheit nach Get
- `AusGruppeEntfernen_NachGetGroupsAsync_PersistiertInDb` — Entfernen aus Gruppe via Navigation-Fixup-Simulation
- `UpdateGroup_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException` — Concurrency-Konflikt für Gruppen

### `EndpointRepositoryIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/EndpointRepositoryIntegrationTests.cs`

Nutzt `TestHelpers.CreateInMemoryDbContext()` direkt und übergibt den `AppDbContext` an `EndpointRepository`. Mehrere Tests setzen zusätzlich `EntityState.Detached` im Test-Setup-Code.

- `AddThenUpdate_WithDifferentInstance_DoesNotThrowTrackingConflict` — Tracking-Robustheit bei Endpoint
- `AddThenUpdate_EndpointGroup_WithDifferentInstance_DoesNotThrowTrackingConflict` — Tracking-Robustheit bei Endpunktgruppe
- `SaveEndpoint_ConcurrentWrite_DetectsConflict` — Concurrency-Erkennung via `ExecuteWithTwoEndpointContextsAsync`
- `UpdateEndpoint_WithApplicationIncluded_CalledTwiceWithDifferentInstances_DoesNotThrowTrackingConflict` — Reproduziert Blazor-Server-Tracking-Fehler (Navigation-Fixup-Konflikt nach zwei aufeinanderfolgenden Updates)
- `DeleteEndpointGroup_WithEndpoints_CascadesDelete` — Cascade-Delete Endpunkte bei Gruppenentfernung
- `DeleteEndpointGroup_WithoutEndpoints_DeletesGroup` — Löschen leerer Gruppe

---

## Hilfsmethoden

### `TestHelpers`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs`

Muss vollständig auf `IDbContextFactory<AppDbContext>` umgestellt werden.

| Hilfsmethode | Beschreibung |
|-------------|--------------|
| `CreateInMemoryDbContext()` | Erstellt eine SQLite-In-Memory-`SqliteConnection` und einen `AppDbContext` mit `EnsureCreated()`. Gibt `(AppDbContext, SqliteConnection)` zurück. Aufrufer muss beide in Reihenfolge disposen. |
| `ExecuteWithTwoContextsAsync(Func<ApplicationRepository, ApplicationRepository, Task>)` | Erstellt zwei `AppDbContext`-Instanzen auf derselben Connection für Concurrency-Tests mit `ApplicationRepository`. |
| `ExecuteWithTwoEndpointContextsAsync(Func<(AppDbContext, EndpointRepository), (AppDbContext, EndpointRepository), Task>)` | Wie `ExecuteWithTwoContextsAsync`, aber für `EndpointRepository`. Übergibt jeweils Tupel aus `AppDbContext` und `EndpointRepository`, damit Tests direkten Kontext-Zugriff für Setup haben. |
