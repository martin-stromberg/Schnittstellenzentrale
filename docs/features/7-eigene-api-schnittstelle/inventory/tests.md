# Tests

## Testklassen

### `ApplicationRepositoryIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ApplicationRepositoryIntegrationTests.cs`

Integrationstests für `ApplicationRepository` mit SQLite In-Memory-Datenbank (via `TestHelpers.CreateInMemoryDbContext`).

- `GetApplications_WithStorageModeUser_ReturnsOnlyUserData` — User-Modus filtert nach Owner
- `GetApplications_WithStorageModeTeam_ReturnsTeamData` — Team-Modus gibt alle Applications zurück
- `AddGroup_PersistsNewGroup` — Anlage einer Gruppe wird persistent gespeichert
- `AddApplication_WithGroup_PersistsApplication` — Application mit Gruppe wird korrekt gespeichert
- `AddApplication_WithoutGroup_PersistsUngroupedApplication` — Application ohne Gruppe landet in `GetUngroupedApplicationsAsync`
- `AddApplication_WithStorageModeUser_FiltersToCurrentOwner` — Owner-Filter greift bei User-Modus
- `UpdateGroup_RenamesGroup` — Umbenennung einer Gruppe wird gespeichert
- `UpdateApplication_ChangesGroup` — Gruppenwechsel wird korrekt persistiert
- `UpdateApplication_SetsGroupToNull` — Entfernen aus Gruppe (FK auf null) wird gespeichert
- `DeleteGroup_SetsMemberApplicationsGroupless` — Nach Gruppen-Löschung haben Applications `ApplicationGroupId == null`
- `DeleteApplication_RemovesApplication` — Application wird nach Delete nicht mehr zurückgegeben
- `DeleteGroup_WithApplicationsDeletedFirst_RemovesGroupAndApplications` — Vollständige Löschung von Gruppe und zugehörigen Applications
- `GetGroups_WithStorageModeUser_ReturnsOnlyGroupsWithOwnedApplications` — User-Modus filtert Gruppen nach eigenen Applications
- `GetUngroupedApplications_WithStorageModeUser_ReturnsOnlyOwnUngroupedApplications` — Kombination aus Ungruppiert- und User-Filter
- `UpdateApplication_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException` — Veraltetes RowVersion löst Concurrency-Exception aus (via zwei Contexte)
- `UpdateGroup_WithNewInstanceAfterAdd_ShouldRenameGroup` — Neue Instanz mit Id + RowVersion kann Update auslösen
- `UpdateGroup_WithNewInstanceAfterGetGroups_ShouldRenameGroup` — Gleicher Test nach vorherigem `GetGroupsAsync`-Aufruf
- `UpdateApplication_WithNewInstanceAfterAdd_ShouldUpdateApplication` — Neue Instanz kann Application aktualisieren
- `UpdateApplication_WithNewInstanceAfterGetApplications_ShouldUpdateApplication` — Gleicher Test nach vorherigem `GetApplicationsAsync`-Aufruf
- `AusGruppeEntfernen_NachGetGroupsAsync_PersistiertInDb` — Simulation des `ApplicationGroupTree`-Flows: FK auf null setzen und updaten
- `UpdateGroup_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException` — Veraltetes RowVersion bei Gruppe (via zwei Contexte)

### `ApplicationContextMenuTests`
Datei: `src/Schnittstellenzentrale.Tests/Components/ApplicationContextMenuTests.cs`

bUnit-Tests für die `ApplicationContextMenu`-Komponente.

- `AusGruppeEntfernen_NurSichtbar_WennAnwendungInGruppe` — Button nur sichtbar, wenn Application in einer Gruppe
- `AusGruppeEntfernen_NichtSichtbar_WennAnwendungOhneGruppe` — Button nicht sichtbar, wenn Application keine Gruppe hat
- `AusGruppeEntfernen_LöstCallbackAus_UndSchliestMenu` — Callback `OnRemoveFromGroupRequested` wird ausgelöst, Menü schließt sich

---

## Hilfsmethoden

### `TestHelpers`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs`

- `CreateInMemoryDbContext()` — erstellt `AppDbContext` mit SQLite In-Memory-Provider; gibt `(AppDbContext, SqliteConnection)` zurück; Aufrufer muss beide disposen (Context zuerst)
- `ExecuteWithTwoContextsAsync(Func<ApplicationRepository, ApplicationRepository, Task>)` — erstellt zwei unabhängige `ApplicationRepository`-Instanzen über dieselbe SQLite-Connection; ermöglicht Concurrency-Tests
