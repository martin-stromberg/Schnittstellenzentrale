# Umsetzungsplan: Persistierung von Umgebungsvariablen durch sz.environment.set

## Übersicht

Wenn `sz.environment.set` in einem Post-Request-Skript aufgerufen wird und eine `SystemEnvironment` aktiv ist, soll die geänderte Variable zusätzlich zur In-Memory-Aktualisierung über eine neue Repository-Methode `UpdateVariableAsync` gezielt in der Datenbank persistiert werden. Anschließend wird `ISignalRNotificationService.NotifyEnvironmentChangedAsync()` aufgerufen, damit verbundene Clients die Änderung sofort erhalten. Dazu werden `ScriptContext` um Repository- und SignalR-Service-Eigenschaften erweitert, `EndpointExecutionService.BuildScriptContext` angepasst und das Lambda in `EndpointScriptRunner.BuildEnvironmentObject` um den blockierenden Persistierungs- und Benachrichtigungsaufruf ergänzt.

## Programmabläufe

### sz.environment.set mit aktiver Systemumgebung (Persistierungsfall)

1. Das Jint-Lambda `sz.environment.set(name, value)` wird aus dem Post-Request-Skript aufgerufen.
2. `EndpointScriptRunner.BuildEnvironmentObject` liest `context.EnvironmentService.ActiveEnvironment` — Ergebnis ist nicht `null`.
3. Das Lambda liest `context.EnvironmentService.ActiveVariables` und baut die neue `updatedVariables`-Dictionary auf.
4. Beim Neuaufbau der `EnvironmentVariable`-Objekte werden die `Id`- und `IsValueMasked`-Werte der bestehenden Variablen aus `context.EnvironmentService.ActiveEnvironment.Variables` übernommen, sofern eine Variable mit demselben Namen bereits existiert.
5. `context.EnvironmentService.SetActiveEnvironment(updatedEnv)` wird aufgerufen, um den In-Memory-Zustand zu aktualisieren.
6. Da `activeEnv != null`, wird anschließend `Task.Run(() => context.EnvironmentRepository!.UpdateVariableAsync(activeEnv.Id, name, value)).GetAwaiter().GetResult()` aufgerufen, um nur die eine geänderte Variable gezielt zu persistieren.
7. Nach erfolgreichem `UpdateVariableAsync`-Aufruf wird `Task.Run(() => context.SignalRNotificationService!.NotifyEnvironmentChangedAsync()).GetAwaiter().GetResult()` aufgerufen, damit verbundene Clients die Änderung sofort erhalten.
8. Wirft einer der Aufrufe in Schritt 6 oder 7 eine Exception, wird diese propagiert; `EndpointScriptRunner.ExecuteAsync` gibt ein `ScriptExecutionResult` mit `Success = false` zurück, das die Fehlermeldung enthält.

Beteiligte Klassen/Komponenten: `EndpointScriptRunner`, `ScriptContext`, `IActiveEnvironmentService`, `ISystemEnvironmentRepository`, `ISignalRNotificationService`, `SystemEnvironment`, `EnvironmentVariable`

### sz.environment.set ohne aktive Systemumgebung (unverändertes Verhalten)

1. Das Lambda `sz.environment.set(name, value)` wird aufgerufen.
2. `context.EnvironmentService.ActiveEnvironment` ist `null`.
3. Der In-Memory-Zustand wird wie bisher via `context.EnvironmentService.SetActiveEnvironment` aktualisiert.
4. Da `activeEnv == null`, werden weder `UpdateVariableAsync` noch `NotifyEnvironmentChangedAsync` aufgerufen.

Beteiligte Klassen/Komponenten: `EndpointScriptRunner`, `ScriptContext`, `IActiveEnvironmentService`

### BuildScriptContext befüllt EnvironmentRepository und SignalRNotificationService

1. `EndpointExecutionService.BuildScriptContext` wird aufgerufen.
2. Das injizierte `ISystemEnvironmentRepository` wird als `EnvironmentRepository`-Eigenschaft in den neuen `ScriptContext` eingetragen.
3. Das injizierte `ISignalRNotificationService` wird als `SignalRNotificationService`-Eigenschaft in den neuen `ScriptContext` eingetragen.
4. Der befüllte `ScriptContext` wird zurückgegeben.

Beteiligte Klassen/Komponenten: `EndpointExecutionService`, `ScriptContext`, `ISystemEnvironmentRepository`, `ISignalRNotificationService`

## Neue Klassen

Keine.

## Änderungen an bestehenden Klassen

### `ScriptContext` (Datenmodellklasse)

- **Neue Eigenschaften:** `EnvironmentRepository` (`ISystemEnvironmentRepository?`) — Ermöglicht `EndpointScriptRunner` den Zugriff auf das Repository für die gezielte Persistierung einer einzelnen Variable. Nullable, weil `ScriptContext` auch außerhalb von `EndpointExecutionService` erzeugt werden kann (z. B. in Tests).
- **Neue Eigenschaften:** `SignalRNotificationService` (`ISignalRNotificationService?`) — Ermöglicht `EndpointScriptRunner` den Aufruf von `NotifyEnvironmentChangedAsync()` nach der Persistierung. Nullable aus demselben Grund wie `EnvironmentRepository`.

### `ISystemEnvironmentRepository` (Interface)

- **Neue Methoden:** `UpdateVariableAsync(int environmentId, string name, string value)` — Persistiert gezielt die eine geänderte Variable der angegebenen Umgebung. Gibt `Task` zurück. Wirft eine Exception, wenn die Umgebung oder Variable nicht gefunden wird.

### `SystemEnvironmentRepository` (Logikklasse)

- **Neue Methoden:** `UpdateVariableAsync(int environmentId, string name, string value)` — Implementierung von `ISystemEnvironmentRepository.UpdateVariableAsync`. Lädt die Umgebung per `GetByIdAsync`, sucht die Variable nach Name, setzt deren `Value` und schreibt via `SaveChangesAsync` zurück. Existiert keine Variable mit dem angegebenen Namen, wird eine neue angelegt. `IsValueMasked` wird für bestehende Variablen nicht verändert.

### `EndpointExecutionService` (Logikklasse)

- **Geänderte Methoden:** `BuildScriptContext` — Befüllt die neuen Eigenschaften `ScriptContext.EnvironmentRepository` und `ScriptContext.SignalRNotificationService` mit den injizierten Abhängigkeiten. `ISystemEnvironmentRepository` und `ISignalRNotificationService` werden als Konstruktorparameter injiziert, sofern noch nicht vorhanden.

### `EndpointScriptRunner` (Logikklasse)

- **Geänderte Methoden:** `BuildEnvironmentObject` — Das `sz.environment.set`-Lambda wird um folgende Logik erweitert:
  - Beim Neuaufbau der Variablenliste werden `Id` und `IsValueMasked` aus `ActiveEnvironment.Variables` für Variablen mit übereinstimmendem Namen übernommen.
  - Nach dem In-Memory-Update via `SetActiveEnvironment` wird geprüft, ob `context.EnvironmentRepository != null` und `activeEnv != null`. Falls ja, wird `UpdateVariableAsync(activeEnv.Id, name, value)` blockierend aufgerufen (Muster: `Task.Run(...).GetAwaiter().GetResult()`).
  - Nach erfolgreichem `UpdateVariableAsync` wird `context.SignalRNotificationService?.NotifyEnvironmentChangedAsync()` blockierend aufgerufen.
  - Wirft einer der Aufrufe eine Exception, wird diese nicht still protokolliert, sondern propagiert, sodass `ExecuteAsync` ein `ScriptExecutionResult` mit `Success = false` zurückgibt.

## Datenbankmigrationen

Keine.

## Validierungsregeln

Keine.

## Konfigurationsänderungen

Keine.

## Seiteneffekte und Risiken

- **IsValueMasked-Korrektur als Seiteneffekt:** Die Übernahme von `Id` und `IsValueMasked` aus bestehenden Variablen korrigiert einen bestehenden Fehler im Lambda. Bestehende Tests, die `SetActiveEnvironment` nach `sz.environment.set` prüfen, könnten nun andere Objekte als bisher erwarten (konkret: Objekte mit `Id != 0` und `IsValueMasked`-Wert), falls die Test-Umgebung eine Systemumgebung mit gesetzten Variablen verwendet.
- **Fehlerfall blockiert Request-Ergebnis:** Da der `UpdateVariableAsync`- oder `NotifyEnvironmentChangedAsync`-Fehler propagiert wird, schlägt bei einem Datenbank- oder SignalR-Problem das Post-Request-Skript fehl und das `EndpointExecutionResult` enthält eine Fehlermeldung. Das entspricht dem bestehenden Fehlerverhalten bei `sz.execute`-Fehlern und verhindert inkonsistente Zustände.
- **`EndpointExecutionService`-Konstruktor:** Da `ISystemEnvironmentRepository` und `ISignalRNotificationService` als neue Konstruktorabhängigkeiten eingetragen werden, müssen alle Stellen, die `EndpointExecutionService` direkt instanziieren (insbesondere Testklassen), angepasst werden.
- **SignalR-Benachrichtigung auch bei User-Modus:** `NotifyEnvironmentChangedAsync` wird unabhängig vom `StorageMode` der aktiven Umgebung aufgerufen — analog zum bestehenden Verhalten in `EnvironmentManagementOverlay`, wo die Benachrichtigung nur im Team-Modus gesendet wird. Falls diese Einschränkung gewünscht ist, muss das Lambda `activeEnv.Mode == StorageMode.Team` prüfen. Da die Anforderung dazu keine Aussage macht, wird die Benachrichtigung bedingungslos gesendet.

## Umsetzungsreihenfolge

1. `ISystemEnvironmentRepository` um `UpdateVariableAsync` erweitern.
2. `SystemEnvironmentRepository` implementiert `UpdateVariableAsync`.
3. `ScriptContext` um `ISystemEnvironmentRepository? EnvironmentRepository` und `ISignalRNotificationService? SignalRNotificationService` erweitern.
4. `EndpointExecutionService` um `ISystemEnvironmentRepository`- und `ISignalRNotificationService`-Konstruktorabhängigkeiten erweitern und `BuildScriptContext` anpassen.
5. `EndpointScriptRunner.BuildEnvironmentObject`: `Id`- und `IsValueMasked`-Übernahme beim Neuaufbau der Variablenliste implementieren.
6. `EndpointScriptRunner.BuildEnvironmentObject`: Blockierenden `UpdateVariableAsync`-Aufruf nach `SetActiveEnvironment` einfügen (nur wenn `activeEnv != null` und `EnvironmentRepository != null`).
7. `EndpointScriptRunner.BuildEnvironmentObject`: Blockierenden `NotifyEnvironmentChangedAsync`-Aufruf nach `UpdateVariableAsync` einfügen (nur wenn `SignalRNotificationService != null`).
8. Bestehende Tests in `EndpointScriptRunnerTests` und `EndpointExecutionServiceTests` anpassen (geänderte `ScriptContext`-Konstruktion, geänderte Mock-Setups).
9. Neue Testmethoden in `EndpointScriptRunnerTests` und `SystemEnvironmentRepositoryIntegrationTests` implementieren.

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `SzEnvironmentSet_MitAktiverSystemumgebung_PersistiertVariable` | `EndpointScriptRunnerTests` | `sz.environment.set` ruft `UpdateVariableAsync` genau einmal mit korrekten Parametern auf, wenn `ActiveEnvironment != null` |
| `SzEnvironmentSet_OhneAktiveSystemumgebung_PersistiertNicht` | `EndpointScriptRunnerTests` | `sz.environment.set` ruft `UpdateVariableAsync` nicht auf, wenn `ActiveEnvironment == null` |
| `SzEnvironmentSet_MitAktiverSystemumgebung_BenachrichtigtSignalR` | `EndpointScriptRunnerTests` | `sz.environment.set` ruft `NotifyEnvironmentChangedAsync` genau einmal auf nach erfolgreicher Persistierung |
| `SzEnvironmentSet_UebernehmtIsValueMasked_AusBestehendenVariablen` | `EndpointScriptRunnerTests` | Beim Setzen einer vorhandenen Variable bleibt `IsValueMasked` im neu aufgebauten `updatedEnv` erhalten |
| `SzEnvironmentSet_UebernehmtId_AusBestehendenVariablen` | `EndpointScriptRunnerTests` | Beim Setzen einer vorhandenen Variable wird deren `Id` im neu aufgebauten `updatedEnv` übernommen |
| `SzEnvironmentSet_DatenbankFehler_GibtScriptExecutionResultMitFehler` | `EndpointScriptRunnerTests` | Wenn `UpdateVariableAsync` wirft, liefert `ExecuteAsync` `Success = false` mit Fehlermeldung |
| `SzEnvironmentSet_SignalRFehler_GibtScriptExecutionResultMitFehler` | `EndpointScriptRunnerTests` | Wenn `NotifyEnvironmentChangedAsync` wirft, liefert `ExecuteAsync` `Success = false` mit Fehlermeldung |
| `UpdateVariableAsync_ExistingVariable_UpdatesValue` | `SystemEnvironmentRepositoryIntegrationTests` | Vorhandene Variable wird korrekt aktualisiert; `IsValueMasked` bleibt erhalten |
| `UpdateVariableAsync_NewVariable_InsertsVariable` | `SystemEnvironmentRepositoryIntegrationTests` | Nicht vorhandene Variable wird als neue Variable eingefügt |
| `CreateContextWithRepository(IActiveEnvironmentService?, ISystemEnvironmentRepository?, ISignalRNotificationService?, ScriptResponseData?)` | `EndpointScriptRunnerTests` | Hilfsmethode: erstellt `ScriptContext` mit optionalem Repository und optionalem SignalR-Service |
| `CreateEnvironmentRepositoryMock()` | `EndpointScriptRunnerTests` | Hilfsmethode: erstellt gemocktes `ISystemEnvironmentRepository`, das `UpdateVariableAsync` aufzeichnet |
| `CreateSignalRNotificationServiceMock()` | `EndpointScriptRunnerTests` | Hilfsmethode: erstellt gemocktes `ISignalRNotificationService`, das `NotifyEnvironmentChangedAsync` aufzeichnet |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| `CreateContext` (Hilfsmethode, `EndpointScriptRunnerTests`) | Muss `ScriptContext` mit den neuen Eigenschaften `EnvironmentRepository` und `SignalRNotificationService` (defaultmäßig `null`) erzeugen; wird durch `CreateContextWithRepository` ersetzt oder ergänzt |
| `SzEnvironmentSet_AktualisiertActiveVariables` (`EndpointScriptRunnerTests`) | Prüft bisher nur In-Memory ohne aktive Systemumgebung — bleibt inhaltlich korrekt, aber `ScriptContext`-Erzeugung ändert sich durch neue Hilfsmethode |
| `CreateService` (Hilfsmethode, `EndpointExecutionServiceTests`) | Muss `ISystemEnvironmentRepository` und `ISignalRNotificationService` als weitere Mock-Parameter übergeben, da der `EndpointExecutionService`-Konstruktor erweitert wird |
| `CreateServiceCapturingUri` (Hilfsmethode, `EndpointExecutionServiceTests`) | Gleicher Grund wie `CreateService` |
| Alle Tests in `EndpointExecutionServiceTests`, die `CreateService` oder `CreateServiceCapturingUri` verwenden | Indirekte Betroffenheit durch Signaturänderung der Hilfsmethoden; kein inhaltlicher Anpassungsbedarf, wenn Default-Mocks ergänzt werden |

## Offene Punkte

Keine.
