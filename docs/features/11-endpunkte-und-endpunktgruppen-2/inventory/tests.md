# Tests

## Testklassen

### `SwaggerImportServiceTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/SwaggerImportServiceTests.cs`

Tests für `SwaggerImportService.ImportAsync` — die Methode, die der `SystemEndpointSyncService` aufrufen wird:

- `Import_NewSwaggerDefinition_ReturnsCorrectDiff` — Leere Datenbank: Swagger mit 2 Operationen ergibt 2 `NewEndpoints`, keine `ChangedEndpoints`, keine `RemovedEndpoints`.
- `Import_ChangedSwaggerOperation_ReturnsChangedInDiff` — Bestehender Endpunkt mit geändertem Namen erscheint in `ChangedEndpoints`.
- `Import_RemovedSwaggerOperation_ReturnsRemovedInDiff` — Bestehender Endpunkt, der nicht mehr in der Swagger-Definition ist, erscheint in `RemovedEndpoints`.

Kein Test für den Fehlerfall (HTTP-Fehler beim Abruf) oder für `ApplyDiffAsync` vorhanden.

### `SystemEntryInitializerTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/SystemEntryInitializerTests.cs`

Tests für `SystemEntryInitializer.InitializeAsync` — die Klasse, die die Vorbedingung für den `SystemEndpointSyncService` herstellt (Systemanwendung in DB mit korrekter `InterfaceUrl`):

- `InitializeAsync_WhenGroupAndApplicationMissing_CreatesBoth` — Legt Gruppe und Anwendung an; prüft `InterfaceUrl`.
- `InitializeAsync_WhenGroupExistsButApplicationMissing_CreatesApplication` — Legt nur die Anwendung an.
- `InitializeAsync_WhenUrlDiffers_UpdatesBaseUrlAndInterfaceUrl` — Aktualisiert URLs bei Konfigurationsänderung.
- `InitializeAsync_WhenUrlMatches_MakesNoChanges` — Kein doppeltes Anlegen bei gleicher URL.
- `InitializeAsync_IsIdempotent_OnRepeatedCall` — Wiederholter Aufruf ändert nichts.
- `InitializeAsync_WhenDbThrows_DoesNotPropagateException` — Datenbankfehler propagiert nicht.
- `InitializeAsync_WhenBaseUrlMissing_SkipsAndLogs` — Fehlende Konfiguration: kein Systemeintrag, keine Exception.

### `EndpointRepositoryIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/EndpointRepositoryIntegrationTests.cs`

Tests für `EndpointRepository` — betreffen `AddEndpointAsync`, `UpdateEndpointAsync` und `DeleteEndpointGroupAsync`, die der `SystemEndpointSyncService` indirekt nutzt:

- `AddThenUpdate_WithDifferentInstance_DoesNotThrowTrackingConflict` — EF-Tracking-Konflikt nach Add+Update wird vermieden.
- `AddThenUpdate_EndpointGroup_WithDifferentInstance_DoesNotThrowTrackingConflict` — Analoger Test für `EndpointGroup`.
- `SaveEndpoint_ConcurrentWrite_DetectsConflict` — `DbUpdateConcurrencyException` bei parallelem Update.
- `DeleteEndpointGroup_WithEndpoints_CascadesDelete` — Cascade-Delete von Endpunkten beim Löschen der Gruppe.
- `DeleteEndpointGroup_WithoutEndpoints_DeletesGroup` — Löschen einer leeren Gruppe.

## Hilfsmethoden

### `TestHelpers`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs`

- `CreateInMemoryDbContext()` — Erstellt einen `AppDbContext` mit SQLite In-Memory-Datenbank; wird in `SystemEntryInitializerTests` und `EndpointRepositoryIntegrationTests` verwendet.
- `ExecuteWithTwoContextsAsync(Func<ApplicationRepository, ApplicationRepository, Task>)` — Zwei unabhängige `ApplicationRepository`-Instanzen über dieselbe In-Memory-Connection für Concurrency-Tests.
- `ExecuteWithTwoEndpointContextsAsync(Func<(AppDbContext, EndpointRepository), (AppDbContext, EndpointRepository), Task>)` — Analoges Setup für `EndpointRepository`-Concurrency-Tests.

### `ControllerTestFactory`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/ControllerTestFactory.cs`

- Erbt von `WebApplicationFactory<Program>`; ersetzt Authentifizierung, `AppDbContext` und `ISignalRNotificationService` durch Test-Implementierungen. Wird für Controller-Integrationstests verwendet. Kein direkter Bezug zum `SystemEndpointSyncService`, aber relevant, falls zukünftige Tests den vollständigen Host starten.
