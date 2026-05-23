# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

- [x] `SystemEndpointSyncService` (Klasse, erbt von `BackgroundService`) — angelegt in `src/Schnittstellenzentrale/SystemEndpointSyncService.cs`
- [x] Constructor-Parameter `IServiceScopeFactory` in `SystemEndpointSyncService` — vorhanden
- [x] Constructor-Parameter `ILogger<SystemEndpointSyncService>` in `SystemEndpointSyncService` — vorhanden
- [x] Methode `ExecuteAsync` in `SystemEndpointSyncService` — vorhanden
- [x] Scope-Erzeugung via `IServiceScopeFactory.CreateScope()` in `ExecuteAsync` — vorhanden
- [x] Auflösung von `IApplicationRepository` und Aufruf von `GetSystemGroupAsync()` — vorhanden
- [x] Warnung loggen und beenden wenn `GetSystemGroupAsync()` null zurückgibt — vorhanden
- [x] Ermittlung der Systemanwendung (`IsSystem == true`) — vorhanden
- [x] Warnung loggen und beenden wenn keine Systemanwendung gefunden — vorhanden
- [x] Auflösung von `ISwaggerImportService` und Aufruf von `ImportAsync(systemApp)` — vorhanden
- [x] Fehler loggen und beenden wenn `diff.ErrorMessage != null` — vorhanden
- [x] `AddEndpointAsync` für jeden Eintrag in `diff.NewEndpoints` aufrufen — vorhanden
- [x] `DeleteEndpointAsync` für jeden Eintrag in `diff.RemovedEndpoints` aufrufen — vorhanden
- [x] `UpdateEndpointNameAsync` für jeden Eintrag in `diff.ChangedEndpoints` aufrufen — vorhanden
- [x] `try/catch`-Block mit `LogError` bei unerwarteter Exception — vorhanden
- [x] Methode `UpdateEndpointNameAsync(int id, string name)` in `IEndpointRepository` — vorhanden
- [x] Methode `UpdateEndpointNameAsync(int id, string name)` in `EndpointRepository` — vorhanden (implementiert via `ExecuteUpdateAsync`, nur `Name`-Spalte)
- [x] `builder.Services.AddHostedService<SystemEndpointSyncService>()` in `Program.cs` — vorhanden (Zeile 75)
- [x] Unterdrückung von `SystemEndpointSyncService` in `ControllerTestFactory` — vorhanden via `services.RemoveAll<IHostedService>()`
- [x] Testklasse `SystemEndpointSyncServiceTests` — angelegt in `src/Schnittstellenzentrale.Tests/Services/SystemEndpointSyncServiceTests.cs`
- [x] Test `ExecuteAsync_NewEndpoints_AreAdded` — vorhanden
- [x] Test `ExecuteAsync_RemovedEndpoints_AreDeleted` — vorhanden
- [x] Test `ExecuteAsync_ChangedEndpoints_NameIsUpdated` — vorhanden (prüft auch, dass `UpdateEndpointAsync` nicht aufgerufen wird)
- [x] Test `ExecuteAsync_WhenImportReturnsError_LogsErrorAndStarts` — vorhanden
- [x] Test `ExecuteAsync_WhenDbThrows_LogsErrorAndStarts` — vorhanden
- [x] Test `ExecuteAsync_IsIdempotent_OnRepeatedCall` — vorhanden
- [x] Test `ExecuteAsync_WhenSystemGroupMissing_LogsWarningAndSkips` — vorhanden

## Offene Aufgaben

Keine.

## Hinweise

- `ControllerTestFactory` unterdrückt alle `IHostedService`-Registrierungen via `services.RemoveAll<IHostedService>()` (statt `RemoveHostedService<SystemEndpointSyncService>()`). Dies entspricht einer breiteren, aber gleichwertig wirksamen Lösung gegenüber dem im Plan genannten selektiven Entfernen.
- Der Plan nennt als Alternative zu `RemoveHostedService<SystemEndpointSyncService>()` auch `services.Remove(...)` auf dem `ServiceDescriptor` — die gewählte Implementierung mit `RemoveAll<IHostedService>()` erfüllt den Zweck vollständig.
