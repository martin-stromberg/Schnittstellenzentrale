# Tests

## Testklassen

### `MainLayoutTests`
Datei: `src/Schnittstellenzentrale.Tests/Components/MainLayoutTests.cs`

- `Layout_RendertModusSelektor` — Prüft, ob der StorageMode-Selektor im Header gerendert wird
- `Layout_RendertZahnradIcon` — Prüft, ob der Umgebungsverwaltungs-Button vorhanden ist
- `DisposeAsync_OhneHubConnection_WirftKeinenFehler` — Dispose ohne Hub-Verbindung schlägt nicht fehl
- `Wiederherstellen_GespeicherteIdVorhanden_SetzAktiveUmgebung` — Umgebung aus localStorage wird korrekt wiederhergestellt
- `Wiederherstellen_UmgebungNichtMehrInDb_BereinigTLocalStorage` — Veraltete ID in localStorage wird bereinigt
- `Wiederherstellen_KeinEintragImLocalStorage_SetzNichts` — Kein localStorage-Eintrag, keine Aktion
- `Wiederherstellen_BeiModuswechsel_VerwendetNeuenSchlüssel` — Nach Moduswechsel wird der modusabhängige Schlüssel verwendet
- `Wiederherstellen_GespeicherteIdVorhanden_SelectorZeigtAuswahl` — EnvironmentSelector zeigt die wiederhergestellte Auswahl
- `OnEnvironmentChanged_AktiveUmgebungNochVorhanden_AktualisiertUmgebung` — Aktive Umgebung nach externem Change-Event aktualisiert
- `OnEnvironmentChanged_AktiveUmgebungGelöscht_BereinigLocalStorageUndSetztNull` — Gelöschte Umgebung wird aus localStorage und ActiveEnvironmentService entfernt

### `ActivityLogServiceTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/ActivityLogServiceTests.cs`

- `Log_ErstelltEintragMitKorrektenFeldern` — Log-Eintrag enthält korrekte Category, Message, Details, Timestamp
- `Log_FeuertOnEntryAdded` — OnEntryAdded-Event wird nach Log() gefeuert
- `Log_EventFehler_WirdIgnoriert` — Fehler im OnEntryAdded-Handler werden nicht weitergeworfen
- `Clear_LeertEintraege` — Clear() entleert die Eintrags-Liste

### `ApplicationRepositoryIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ApplicationRepositoryIntegrationTests.cs`

- `GetApplications_WithStorageModeUser_ReturnsOnlyUserData`
- `GetApplications_WithStorageModeTeam_ReturnsTeamData`
- `AddGroup_PersistsNewGroup`
- `AddApplication_WithGroup_PersistsApplication`
- `AddApplication_WithoutGroup_PersistsUngroupedApplication`
- `AddApplication_WithStorageModeUser_FiltersToCurrentOwner`
- `UpdateGroup_RenamesGroup`
- `UpdateApplication_ChangesGroup`
- `UpdateApplication_SetsGroupToNull`
- `DeleteGroup_SetsMemberApplicationsGroupless`
- `DeleteApplication_RemovesApplication`
- `DeleteGroup_WithApplicationsDeletedFirst_RemovesGroupAndApplications`
- `GetGroups_WithStorageModeUser_ReturnsOnlyGroupsWithOwnedApplications`
- `GetUngroupedApplications_WithStorageModeUser_ReturnsOnlyOwnUngroupedApplications`
- `UpdateApplication_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException`
- `UpdateGroup_WithNewInstanceAfterAdd_ShouldRenameGroup`
- `UpdateGroup_WithNewInstanceAfterGetGroups_ShouldRenameGroup`
- `UpdateApplication_WithNewInstanceAfterAdd_ShouldUpdateApplication`
- `UpdateApplication_WithNewInstanceAfterGetApplications_ShouldUpdateApplication`
- `AusGruppeEntfernen_NachGetGroupsAsync_PersistiertInDb`
- `UpdateGroup_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException`

### `HomePageTests`
Datei: `src/Schnittstellenzentrale.Tests/Playwright/HomePageTests.cs`

- `StartPage_ShowsSystemGroup` — Systemgruppe „Schnittstellenzentrale" ist im Baum sichtbar
- `StartPage_ShowsOwnApiEndpoints` — Endpunkte der Systemanwendung erscheinen nach Aufklappen

### `StorageModeTests`
Datei: `src/Schnittstellenzentrale.Tests/Playwright/StorageModeTests.cs`

- `SwitchToTeamMode_ShowsTeamData` — Wechsel auf Team-Modus zeigt Baum
- `SwitchBackToUserMode_ShowsUserData` — Rückwechsel auf User-Modus zeigt Baum

### `EnvironmentManagementTests`
Datei: `src/Schnittstellenzentrale.Tests/Playwright/EnvironmentManagementTests.cs`

- `MaskierterWert_IstNichtImKlartextImDomSichtbar` — Maskierter Variablenwert erscheint nicht im DOM; Auge-Icon macht ihn sichtbar

### `ApplicationCrudTests`
Datei: `src/Schnittstellenzentrale.Tests/Playwright/ApplicationCrudTests.cs`

- `CreateApplication_AppearsInTree` — Neu angelegte Anwendung erscheint im Baum
- `EditApplication_UpdatesNameInTree` — Geänderter Name erscheint im Baum (weitere Tests vorhanden)

---

## Hilfsmethoden

### `TestHelpers` / `TestMockFactory`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/`

- `CreateInMemoryDbContext()` — Erstellt InMemory-DbContext + ApplicationRepository für Integrationstests
- `ExecuteWithTwoContextsAsync(Func<ApplicationRepository, ApplicationRepository, Task>)` — Öffnet zwei separate Repository-Instanzen (für Concurrency-Tests)
- `CreateEnv(int, string)` — Erstellt ein `SystemEnvironment`-Testobjekt mit ID und Name
