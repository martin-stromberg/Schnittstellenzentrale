# Tests

## Testklassen

### `EndpointExecutionServiceTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/EndpointExecutionServiceTests.cs`

- `Execute_WithAuthTypeNone_SendsRequestWithoutCredentials` — Prüft, dass ohne Authentifizierung kein `Authorization`-Header gesendet wird
- `Execute_WithAuthTypeBasic_SendsBasicAuthHeader` — Prüft Basic-Auth-Header-Setzung
- `Execute_WithNegotiateAuthType_UsesNegotiateHandler` — Prüft Verwendung des Negotiate-Clients (Theory für `Negotiate` und `NegotiateWithImpersonation`)
- `Execute_WithAuthTypeBearerToken_SendsBearerHeader` — Prüft Bearer-Token-Header-Setzung mit korrektem Parameter
- `Execute_SetsResponseHeaders` — Prüft, dass Antwort-Header ins Ergebnis übernommen werden
- `Execute_SetsDurationMs` — Prüft, dass `DurationMs` gesetzt und positiv ist
- `Execute_SetsResponseSizeBytes` — Prüft, dass `ResponseSizeBytes` der UTF-8-Byte-Anzahl des Body entspricht
- `Execute_OnConnectionError_DoesNotCallHealthCheck` — Prüft, dass bei Verbindungsfehlern kein Health-Check ausgelöst wird
- `BuildRequest_ErsetztPfadPlatzhalterDurchGespeicherteWerte` — Prüft `{id}`-Auflösung im Pfad
- `BuildRequest_HaengtNurNichtPlatzhalterParameterAlsQueryStringAn` — Prüft, dass aufgelöste Pfad-Parameter nicht erneut im Query-String erscheinen

Fehlende Testszenarien (laut Anforderung):
- `{{var}}`-Platzhalter werden vor `{pfad}`-Platzhaltern aufgelöst
- Fehlende Variable ergibt leeren String
- Keine aktive Umgebung → alle `{{...}}` durch leere Strings ersetzt
- `{{...}}`-Auflösung in Basis-URL, relativer URL, Header-Name/-Wert, Query-Parametern, Bearer-Token und Body

---

### `ApplicationRepositoryIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/ApplicationRepositoryIntegrationTests.cs`

- `GetApplications_WithStorageModeUser_ReturnsOnlyUserData` — Owner-Filterung im User-Modus
- `GetApplications_WithStorageModeTeam_ReturnsTeamData` — Keine Filterung im Team-Modus
- `AddGroup_PersistsNewGroup` — Persistierung einer neuen Gruppe
- `AddApplication_WithGroup_PersistsApplication` — Verknüpfung mit Gruppe
- `AddApplication_WithoutGroup_PersistsUngroupedApplication` — Ungegruppierte Anwendung
- `AddApplication_WithStorageModeUser_FiltersToCurrentOwner` — Owner-Filterung nach Add
- `UpdateGroup_RenamesGroup` — Umbenennen einer Gruppe
- `UpdateApplication_ChangesGroup` — Gruppenverschiebung
- `UpdateApplication_SetsGroupToNull` — Aus Gruppe entfernen
- `DeleteGroup_SetsMemberApplicationsGroupless` — DeleteBehavior.SetNull bei Gruppe
- `DeleteApplication_RemovesApplication` — Löschen einer Anwendung
- `DeleteGroup_WithApplicationsDeletedFirst_RemovesGroupAndApplications` — Kaskadierendes Löschen (manuell)
- `GetGroups_WithStorageModeUser_ReturnsOnlyGroupsWithOwnedApplications` — Gruppenfilterung via owned Applications
- `GetUngroupedApplications_WithStorageModeUser_ReturnsOnlyOwnUngroupedApplications` — User-Modus-Filterung ungegruppierter Anwendungen
- `UpdateApplication_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException` — Optimistisches Sperren bei Anwendung
- `UpdateGroup_WithStaleRowVersion_ThrowsDbUpdateConcurrencyException` — Optimistisches Sperren bei Gruppe
- `UpdateGroup_WithNewInstanceAfterAdd_ShouldRenameGroup` — Neue Instanz nach Add
- `UpdateGroup_WithNewInstanceAfterGetGroups_ShouldRenameGroup` — Neue Instanz nach GetGroups
- `UpdateApplication_WithNewInstanceAfterAdd_ShouldUpdateApplication` — Neue Instanz nach Add
- `UpdateApplication_WithNewInstanceAfterGetApplications_ShouldUpdateApplication` — Neue Instanz nach GetApplications
- `AusGruppeEntfernen_NachGetGroupsAsync_PersistiertInDb` — FK-Update nach Navigation-Fixup

Dient als Strukturvorlage für `SystemEnvironmentRepositoryIntegrationTests`. Noch fehlende Testklasse laut Anforderung:
- `SystemEnvironmentRepositoryIntegrationTests` — CRUD, Namenseindeutigkeit (Unique-Constraint), Cascade Delete

---

### `EndpointExecutionTests` (Playwright)
Datei: `src/Schnittstellenzentrale.Tests/Playwright/EndpointExecutionTests.cs`

- `ExecuteEndpoint_ReturnsSuccessResponse` — E2E: Endpunkt anlegen und ausführen, HTTP-2xx-Status in Response-Bereich prüfen
- `EndpunktMitPlatzhalterUndQueryString_ZeigtKorrekteEintraegeUndSendetAufgeloestUrl` — E2E: Pfad mit `{id}`-Platzhalter und Query-String; Eintragsanzeige und aufgelöste URL prüfen

Fehlende Playwright-Tests laut Anforderung:
- E2E-Szenario: Umgebung mit Variable anlegen, aktivieren, Endpunkt mit `{{baseUrl}}/api/test` senden, gesendete URL in Antwortanzeige prüfen

## Hilfsmethoden

### `TestHelpers`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs`

- `CreateInMemoryDbContext()` — Erstellt `IDbContextFactory<AppDbContext>` mit SQLite In-Memory-Datenbank; gibt `(Factory, SqliteConnection)` zurück; Aufrufer muss Connection disposen
- `ExecuteWithTwoContextsAsync(Func<ApplicationRepository, ApplicationRepository, Task>)` — Führt Test mit zwei unabhängigen `ApplicationRepository`-Instanzen über dieselbe SQLite-Connection aus; ermöglicht Concurrency-Tests

`TestHelpers` muss für `SystemEnvironmentRepositoryIntegrationTests` um eine Methode für `SystemEnvironmentRepository` erweitert oder direkt wiederverwendet werden.
