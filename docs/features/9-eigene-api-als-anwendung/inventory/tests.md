# Tests

## Testklassen

### `ApplicationGroupsControllerIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ApplicationGroupsControllerIntegrationTests.cs`

- `PostApplicationGroup_WithValidTokenAndRequest_Returns201AndLocation` — POST mit gültigem Token liefert 201 mit Location-Header und korrektem Body
- `PostApplicationGroup_WithoutToken_Returns401` — POST ohne Token liefert 401
- `PostApplicationGroup_WithExpiredToken_Returns401` — POST mit abgelaufenem Token liefert 401
- `PostApplicationGroup_WithMissingName_Returns400` — POST mit leerem Namen liefert 400
- `PostApplicationGroup_Returns_NewTokenHeader` — POST rotiert das Token (X-New-Token vorhanden)
- `PostApplicationGroup_AfterRotation_OldTokenIsUnauthorized` — altes Token nach Rotation ungültig
- `PostApplicationGroup_AfterRotation_NewTokenIsValid` — neues Token nach Rotation verwendbar
- `GetApplicationGroups_WithValidToken_Returns200WithList` — GET mit gültigem Token liefert 200 mit Liste
- `GetApplicationGroups_WithoutToken_Returns401` — GET ohne Token liefert 401
- `GetApplicationGroupById_WithValidId_Returns200` — GET nach Id liefert 200 mit korrektem Body
- `GetApplicationGroupById_WithInvalidId_Returns404` — GET nach nicht vorhandener Id liefert 404
- `PutApplicationGroup_WithValidRequest_Returns200AndRotatesToken` — PUT aktualisiert Gruppe und rotiert Token
- `PutApplicationGroup_WithInvalidId_Returns404` — PUT auf nicht vorhandene Id liefert 404
- `PutApplicationGroup_WithMissingName_Returns400` — PUT mit leerem Namen liefert 400
- `DeleteApplicationGroup_WithValidId_Returns204AndRotatesToken` — DELETE löscht Gruppe und rotiert Token
- `DeleteApplicationGroup_WithInvalidId_Returns404` — DELETE auf nicht vorhandene Id liefert 404

Noch **nicht** vorhanden: Test „DELETE auf Systemeintrag liefert 403".

---

### `ApplicationsControllerIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ApplicationsControllerIntegrationTests.cs`

- `PostApplication_WithValidTokenAndRequest_Returns201AndLocation` — POST mit gültigem Token liefert 201
- `PostApplication_WithoutToken_Returns401` — POST ohne Token liefert 401
- `PostApplication_WithMissingName_Returns400` — POST mit leerem Namen liefert 400
- `PostApplication_WithMissingBaseUrl_Returns400` — POST ohne BaseUrl liefert 400
- `GetApplications_WithValidToken_Returns200WithList` — GET liefert 200 mit Liste
- `GetApplications_WithoutToken_Returns401` — GET ohne Token liefert 401
- `GetUngroupedApplications_WithValidToken_Returns200WithList` — GET /ungrouped liefert 200
- `GetApplicationById_WithValidId_Returns200WithAllFields` — GET nach Id liefert alle Felder
- `GetApplicationById_WithInvalidId_Returns404` — GET nach nicht vorhandener Id liefert 404
- `PutApplication_WithValidRequest_Returns200AndRotatesToken` — PUT aktualisiert Anwendung
- `PutApplication_WithInvalidId_Returns404` — PUT auf nicht vorhandene Id liefert 404
- `PutApplication_WithMissingBaseUrl_Returns400` — PUT ohne BaseUrl liefert 400
- `DeleteApplication_WithValidId_Returns204AndRotatesToken` — DELETE löscht Anwendung
- `DeleteApplication_WithInvalidId_Returns404` — DELETE auf nicht vorhandene Id liefert 404

Noch **nicht** vorhanden: Test „DELETE auf Systemeintrag liefert 403".

---

### `ApplicationContextMenuTests`
Datei: `src/Schnittstellenzentrale.Tests/Components/ApplicationContextMenuTests.cs`

- `AusGruppeEntfernen_NurSichtbar_WennAnwendungInGruppe` — „Aus Gruppe entfernen" nur sichtbar, wenn `ApplicationGroupId` gesetzt
- `AusGruppeEntfernen_NichtSichtbar_WennAnwendungOhneGruppe` — „Aus Gruppe entfernen" ausgeblendet, wenn keine Gruppe
- `AusGruppeEntfernen_LöstCallbackAus_UndSchliestMenu` — Klick auf „Aus Gruppe entfernen" löst `OnRemoveFromGroupRequested` aus und schließt das Menü

Noch **nicht** vorhanden: Tests für deaktivierte Schaltflächen bei `IsSystem == true`.

---

## Hilfsmethoden

### `TestHelpers`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs`

- `CreateInMemoryDbContext()` — erstellt einen `AppDbContext` mit SQLite In-Memory-Provider; gibt `(AppDbContext, SqliteConnection)` zurück
- `ExecuteWithTwoContextsAsync(Func<ApplicationRepository, ApplicationRepository, Task>)` — führt einen Test mit zwei unabhängigen Repository-Instanzen auf derselben Connection aus; nützlich für Concurrency-Tests

### `ControllerTestFactory`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/ControllerTestFactory.cs`

- Erbt von `WebApplicationFactory<Program>` (xUnit `IClassFixture`)
- Ersetzt Authentifizierung durch `TestAuthHandler`
- Ersetzt `AppDbContext` durch SQLite In-Memory über eine geteilte `SqliteConnection`
- Ersetzt `IApplicationRepository` durch die echte `ApplicationRepository`-Implementierung
- Mockt `ISignalRNotificationService`
- Optionales Property `TokenLifetime` für Tests mit abgelaufenen Tokens
