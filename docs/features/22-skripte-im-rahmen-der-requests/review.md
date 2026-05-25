# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

### `ScriptContext` — erweitert

- [x] Eigenschaft `EnvironmentRepository` (`ISystemEnvironmentRepository?`) — vorhanden (`ScriptContext.cs`, Zeile 24)
- [x] Eigenschaft `SignalRNotificationService` (`ISignalRNotificationService?`) — vorhanden (`ScriptContext.cs`, Zeile 27)

### `ISystemEnvironmentRepository` — erweitert

- [x] Methode `UpdateVariableAsync(int environmentId, string name, string value)` — vorhanden (`ISystemEnvironmentRepository.cs`, Zeile 13)

### `SystemEnvironmentRepository` — erweitert

- [x] Methode `UpdateVariableAsync(int environmentId, string name, string value)` — implementiert; lädt Umgebung per `FirstOrDefaultAsync`, sucht Variable nach Name, aktualisiert `Value` oder fügt neue Variable ein, schreibt per `SaveChangesAsync`; `IsValueMasked` bestehender Variablen wird nicht verändert

### `EndpointExecutionService` — erweitert

- [x] Konstruktorparameter `ISystemEnvironmentRepository environmentRepository` — vorhanden
- [x] Konstruktorparameter `ISignalRNotificationService signalRNotificationService` — vorhanden
- [x] `BuildScriptContext` befüllt `EnvironmentRepository` aus `_environmentRepository` — vorhanden
- [x] `BuildScriptContext` befüllt `SignalRNotificationService` aus `_signalRNotificationService` — vorhanden

### `EndpointScriptRunner.BuildEnvironmentObject` — erweitert

- [x] Beim Neuaufbau der Variablenliste werden `Id` und `IsValueMasked` aus `ActiveEnvironment.Variables` für gleichnamige Variablen übernommen — umgesetzt (Lambda Zeilen 120–128)
- [x] Nach `SetActiveEnvironment` wird `UpdateVariableAsync(activeEnv.Id, name, value)` blockierend aufgerufen, wenn `activeEnv != null` und `context.EnvironmentRepository != null` — umgesetzt (Zeilen 147–149)
- [x] Nach `UpdateVariableAsync` wird `NotifyEnvironmentChangedAsync()` blockierend aufgerufen — umgesetzt (Zeile 150)
- [x] Exceptions aus beiden Aufrufen werden propagiert und führen zu `ScriptExecutionResult { Success = false }` — umgesetzt (durch allgemeine `catch (Exception)` in `ExecuteAsync`)

### Tests in `EndpointScriptRunnerTests`

- [x] `CreateContextWithRepository(IActiveEnvironmentService?, ISystemEnvironmentRepository?, ISignalRNotificationService?, ScriptResponseData?)` — Hilfsmethode vorhanden; `CreateContext` delegiert dorthin
- [x] `CreateEnvironmentRepositoryMock()` — vorhanden (gibt `mock.Object` zurück); zusätzlich `CreateEnvironmentRepositoryMockCapturing()` für Verify-Zugriff
- [x] `CreateSignalRNotificationServiceMock()` — vorhanden; zusätzlich `CreateSignalRNotificationServiceMockCapturing()`
- [x] `SzEnvironmentSet_MitAktiverSystemumgebung_PersistiertVariable` — vorhanden; prüft `UpdateVariableAsync` genau einmal mit korrekten Parametern
- [x] `SzEnvironmentSet_OhneAktiveSystemumgebung_PersistiertNicht` — vorhanden; prüft, dass `UpdateVariableAsync` nie aufgerufen wird
- [x] `SzEnvironmentSet_MitAktiverSystemumgebung_BenachrichtigtSignalR` — vorhanden; prüft `NotifyEnvironmentChangedAsync` genau einmal
- [x] `SzEnvironmentSet_UebernehmtIsValueMasked_AusBestehendenVariablen` — vorhanden
- [x] `SzEnvironmentSet_UebernehmtId_AusBestehendenVariablen` — vorhanden
- [x] `SzEnvironmentSet_DatenbankFehler_GibtScriptExecutionResultMitFehler` — vorhanden
- [x] `SzEnvironmentSet_SignalRFehler_GibtScriptExecutionResultMitFehler` — vorhanden

### Betroffene bestehende Tests in `EndpointScriptRunnerTests`

- [x] `CreateContext` — delegiert jetzt an `CreateContextWithRepository` mit `null`-Defaults für Repository und SignalR-Service; bestehende Tests bleiben inhaltlich korrekt

### Betroffene bestehende Tests in `EndpointExecutionServiceTests`

- [x] `CreateService` — nimmt `environmentRepositoryMock` und `signalRNotificationServiceMock` als optionale Parameter; Defaults werden intern erzeugt (`CreateEmptyEnvironmentRepositoryMock`, `CreateEmptySignalRNotificationServiceMock`)
- [x] `CreateServiceCapturingUri` — ruft `CreateService` auf; neue Parameter werden mit Defaults abgedeckt
- [x] Alle bestehenden Tests in `EndpointExecutionServiceTests` — laufen ohne inhaltliche Anpassung weiter, da `CreateService`/`CreateServiceCapturingUri` die neuen Mocks intern erzeugen; direkte Instanziierungen in einzelnen Tests (`Execute_WithNegotiateAuthType_UsesNegotiateHandler`, `Execute_OnConnectionError_DoesNotCallHealthCheck`, etc.) wurden ebenfalls auf die neue 8-Parameter-Signatur umgestellt

### Tests in `SystemEnvironmentRepositoryIntegrationTests`

- [x] `UpdateVariableAsync_ExistingVariable_UpdatesValue` — vorhanden; prüft aktualisierten Wert und erhaltenes `IsValueMasked`
- [x] `UpdateVariableAsync_NewVariable_InsertsVariable` — vorhanden; prüft eingefügte neue Variable

## Offene Aufgaben

Keine.

## Hinweise

- Die Implementierung verwendet `context.SignalRNotificationService!.NotifyEnvironmentChangedAsync()` (Null-Assertion-Operator `!`) statt des im Plan notierten Null-Conditional-Operators `?.`. Das führt zu keinem Problem in der Praxis, weil `EndpointExecutionService.BuildScriptContext` den Service immer setzt. In Test-Szenarien, die `CreateContextWithRepository` mit `signalRNotificationService: null` und gleichzeitig `environmentRepository != null` sowie `activeEnv != null` aufrufen, würde jedoch eine `NullReferenceException` entstehen. Alle vorhandenen Tests vermeiden diese Kombination korrekt.
- Der Plan beschreibt unter „Seiteneffekte und Risiken" eine mögliche Einschränkung der SignalR-Benachrichtigung auf `StorageMode.Team`; diese optionale Prüfung wurde bewusst nicht implementiert, was dem Plan entspricht.
