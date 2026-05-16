# Tests

## Testklassen

### `ApplicationRepositoryIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ApplicationRepositoryIntegrationTests.cs`

- `GetApplications_WithStorageModeUser_ReturnsOnlyUserData` — Prüft, dass im `User`-Mode nur Anwendungen des angegebenen Owners zurückgegeben werden.
- `GetApplications_WithStorageModeTeam_ReturnsTeamData` — Prüft, dass im `Team`-Mode alle Anwendungen unabhängig vom Owner zurückgegeben werden.
- `AddGroup_PersistsNewGroup` — Prüft, dass eine neu angelegte Gruppe korrekt persistiert und per `GetGroupsAsync` abrufbar ist.
- `AddApplication_WithGroup_PersistsApplication` — Prüft, dass eine Anwendung mit gesetzter `ApplicationGroupId` korrekt gespeichert wird.
- `AddApplication_WithoutGroup_PersistsUngroupedApplication` — Prüft, dass eine gruppenlose Anwendung über `GetUngroupedApplicationsAsync` abrufbar ist.
- `AddApplication_WithStorageModeUser_FiltersToCurrentOwner` — Prüft, dass eine Anwendung mit Owner im `User`-Mode korrekt gefiltert wird.
- `UpdateGroup_RenamesGroup` — Prüft, dass `UpdateGroupAsync` den Namen einer Gruppe korrekt ändert.
- `UpdateApplication_ChangesGroup` — Prüft, dass `UpdateApplicationAsync` die Gruppe einer Anwendung wechselt.
- `UpdateApplication_SetsGroupToNull` — Prüft, dass `UpdateApplicationAsync` eine Anwendung aus einer Gruppe entfernt (`ApplicationGroupId = null`).
- `DeleteGroup_SetsMemberApplicationsGroupless` — Prüft, dass nach `DeleteGroupAsync` die zugehörigen Anwendungen gruppenlos sind (DB-Kaskade mit `SetNull`).
- `DeleteApplication_RemovesApplication` — Prüft, dass `DeleteApplicationAsync` die Anwendung vollständig entfernt.
- `DeleteGroup_WithApplicationsDeletedFirst_RemovesGroupAndApplications` — Prüft das manuelle Löschen aller Anwendungen vor dem Löschen der Gruppe.
- `GetGroups_WithStorageModeUser_ReturnsOnlyGroupsWithOwnedApplications` — Prüft, dass im `User`-Mode nur Gruppen zurückgegeben werden, die mindestens eine eigene Anwendung enthalten.
- `GetUngroupedApplications_WithStorageModeUser_ReturnsOnlyOwnUngroupedApplications` — Prüft, dass nur ungegruppierte Anwendungen des eigenen Owners zurückgegeben werden.
- `UpdateApplication_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException` — Prüft, dass eine veraltete `RowVersion` beim Update eine `DbUpdateConcurrencyException` auslöst.
- `UpdateGroup_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException` — Prüft, dass eine veraltete `RowVersion` beim Gruppen-Update eine `DbUpdateConcurrencyException` auslöst.

Es existieren keine UI-Tests (Blazor-Komponenten-Tests) für `ApplicationContextMenu`, `ApplicationGroupContextMenu`, `ApplicationGroupTree` oder `CollapsibleSection`.

## Hilfsmethoden

### `TestHelpers`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs`

- `CreateInMemoryDbContext` — Erstellt einen `AppDbContext` mit SQLite In-Memory-Provider (offene Verbindung, Schema per `EnsureCreated`). Gibt `(AppDbContext, SqliteConnection)` zurück; Aufrufer ist für das Dispose beider Objekte verantwortlich (Context zuerst, dann Connection).
- `ExecuteWithTwoContextsAsync` — Erstellt zwei unabhängige `ApplicationRepository`-Instanzen über dieselbe SQLite-In-Memory-Connection. Ermöglicht Concurrency-Tests. Wird von `UpdateApplication_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException` und `UpdateGroup_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException` genutzt.

### `ApplicationRepositoryIntegrationTests` (privat)
- `ExecuteWithContextAsync` — Erstellt per `TestHelpers.CreateInMemoryDbContext()` eine isolierte Datenbankinstanz, führt den übergebenen Test-Delegate aus und disposed anschließend Context und Connection. Alle einfachen Tests nutzen diese Hilfsmethode.
