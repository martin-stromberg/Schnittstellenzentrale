# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### SystemEndpointSyncServiceTests.cs (SystemEndpointSyncServiceTests)

- **Irreführende Testnamen** — `ExecuteAsync_WhenImportReturnsError_LogsErrorAndStarts` (Zeile 111) und `ExecuteAsync_WhenDbThrows_LogsErrorAndStarts` (Zeile 128): Das Suffix `AndStarts` ist irreführend. `ExecuteAsync` eines `BackgroundService` läuft einmalig durch; dass der Service „startet" ist kein prüfbares Verhalten in diesen Tests. Beide Tests prüfen lediglich, dass keine Exception nach außen dringt und keine unerwünschten Repository-Aufrufe erfolgen.

  Empfehlung: `ExecuteAsync_WhenImportReturnsError_LogsErrorAndStarts` umbenennen in `ExecuteAsync_WhenImportReturnsError_LogsErrorAndSkipsRepositoryCalls`; `ExecuteAsync_WhenDbThrows_LogsErrorAndStarts` umbenennen in `ExecuteAsync_WhenDbThrows_DoesNotThrow`.

- **Testname behauptet Verhalten, das nicht verifiziert wird** — `ExecuteAsync_WhenImportReturnsError_LogsErrorAndStarts` (Zeile 111): Der Testname enthält `LogsError`, der Test überprüft jedoch nicht, dass tatsächlich eine Fehlermeldung geloggt wurde. Der `loggerMock` wird zwar via Destrukturierung ignoriert (`_`), aber kein `Verify`-Aufruf auf dem Logger findet statt.

  Empfehlung: Entweder den `loggerMock` einbinden und `loggerMock.Verify(l => l.Log(LogLevel.Error, ...), Times.Once)` hinzufügen, oder den irreführenden `LogsError`-Teil aus dem Testnamen entfernen.

### EndpointRepository.cs (EndpointRepository)

- **Inkonsistenz beim Detach vor Update** — `UpdateEndpointAsync` (Zeile 53–59) und `UpdateEndpointGroupAsync` (Zeile 103–109) rufen `_context.Endpoints.Update(endpoint)` bzw. `_context.EndpointGroups.Update(group)` direkt auf, ohne zuvor zu prüfen, ob eine bereits getrackte Instanz mit derselben Id im `ChangeTracker` existiert. `ApplicationRepository.UpdateGroupAsync` (Zeile 66–73) und `UpdateApplicationAsync` (Zeile 138–156) folgen dem expliziten Muster: zuerst vorhandene Tracking-Einträge auf `Detached` setzen, dann `Update()` aufrufen. Die Abweichung ist eine potenzielle Quelle von Tracking-Konflikten, wenn ein Aufrufer im selben Scope zuvor `GetEndpointsAsync` (ohne `AsNoTracking`) oder eine andere Methode genutzt hat, die eine getrackte Instanz hinterlässt.

  Empfehlung: In `UpdateEndpointAsync` und `UpdateEndpointGroupAsync` jeweils denselben defensiven Detach-Block voranstellen, der bereits in `UpdateGroupAsync` und `UpdateApplicationAsync` verwendet wird.

## Geprüfte Dateien

- `src/Schnittstellenzentrale.Core/Interfaces/IEndpointRepository.cs`
- `src/Schnittstellenzentrale.Infrastructure/Repositories/EndpointRepository.cs`
- `src/Schnittstellenzentrale.Tests/Helpers/ControllerTestFactory.cs`
- `src/Schnittstellenzentrale/Program.cs`
- `src/Schnittstellenzentrale/SystemEndpointSyncService.cs`
- `src/Schnittstellenzentrale.Tests/Services/SystemEndpointSyncServiceTests.cs`
