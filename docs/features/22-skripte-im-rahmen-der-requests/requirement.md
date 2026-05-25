## Fachliche Zusammenfassung

Wenn ein Post-Request-Skript über `sz.environment.set(name, value)` eine Variable der aktiven `SystemEnvironment` verändert, wird die Änderung bisher ausschließlich im In-Memory-Zustand des `IActiveEnvironmentService` (d. h. in `ActiveVariables`) gehalten und nicht in die Datenbank zurückgeschrieben. Das Verhalten soll so erweitert werden, dass eine Variablenänderung durch ein Skript — sofern eine Systemumgebung aktiv ist — zusätzlich über `ISystemEnvironmentRepository.UpdateAsync` persistiert wird. Ist keine Systemumgebung gewählt (`ActiveEnvironment == null`), verbleibt das bisherige Verhalten: die Variablen existieren nur für die Laufzeit des Requests im Arbeitsspeicher.

---

## Betroffene Klassen und Komponenten

### Logikklassen / Services

- `EndpointScriptRunner` — enthält `BuildEnvironmentObject` mit der Lambda-Implementierung von `sz.environment.set`; hier ist die Persistierungslogik einzuhängen
- `IActiveEnvironmentService` / `ActiveEnvironmentService` — kein Änderungsbedarf am Interface erwartet; `SetActiveEnvironment` wird weiterhin zur In-Memory-Aktualisierung genutzt
- `ISystemEnvironmentRepository` / `SystemEnvironmentRepository` — `UpdateAsync` wird bei gesetzter Umgebung nach dem In-Memory-Update aufgerufen
- `ScriptContext` — muss Zugang zum `ISystemEnvironmentRepository` erhalten, damit `EndpointScriptRunner` bei Bedarf persistieren kann

### Interfaces

- `IActiveEnvironmentService` — ggf. Ergänzung einer asynchronen Variante von `SetActiveEnvironment` (z. B. `SetActiveEnvironmentAsync`), falls die Persistierung direkt im Service gekapselt werden soll (Designentscheidung, s. u.)

### Datenmodellklassen

- Keine neuen Klassen oder Eigenschaften erforderlich; `SystemEnvironment` und `EnvironmentVariable` sind ausreichend.

### Tests

- `EndpointScriptRunnerTests` — neue Testfälle für `sz.environment.set` mit aktiver Umgebung (Persistierung wird aufgerufen) und ohne aktive Umgebung (nur In-Memory)
- `EndpointExecutionIntegrationTests` — ggf. Integrationstest, der nach Skriptausführung den Datenbankstand prüft

---

## Implementierungsansatz

Der kritische Pfad liegt in `EndpointScriptRunner.BuildEnvironmentObject`, Methode `sz.environment.set`:

1. **Aktueller Stand:** Nach der In-Memory-Aktualisierung via `context.EnvironmentService.SetActiveEnvironment(updatedEnv)` endet die Methode.
2. **Erweiterung:** Ist `activeEnv != null` (d. h. eine echte Systemumgebung ist aktiv), soll zusätzlich `ISystemEnvironmentRepository.UpdateAsync(updatedEnv)` aufgerufen werden. Da `EndpointScriptRunner` ein synchrones Lambda in Jint registriert, muss der asynchrone Repository-Aufruf über `Task.Run(...).GetAwaiter().GetResult()` blockierend ausgeführt werden — analog zur bestehenden `sz.execute`-Implementierung in derselben Klasse.
3. **`ScriptContext`-Erweiterung:** `ScriptContext` erhält eine neue Eigenschaft `ISystemEnvironmentRepository? EnvironmentRepository`, die `EndpointExecutionService.BuildScriptContext` befüllt.
4. **Keine Persistierung ohne aktive Umgebung:** Ist `activeEnv == null`, wird ausschließlich der In-Memory-Zustand aktualisiert — keine Datenbankoperation.

*Annahme:* Die Persistierung wird direkt im `EndpointScriptRunner` durch Zugriff auf `ScriptContext.EnvironmentRepository` ausgelöst, nicht durch eine neue Methode im `IActiveEnvironmentService`. Das vermeidet eine Abhängigkeit des Interfaces auf das Repository.

---

## Konfiguration

Das Feature erfordert keine zusätzliche Konfigurationsebene. Das Verhalten ergibt sich automatisch aus dem Zustand der aktiven Umgebung (`ActiveEnvironment != null` → persistieren; `null` → nur In-Memory).

---

## Offene Fragen

1. **Fehlerbehandlung bei Persistierungsfehler:** Soll ein Datenbankfehler beim `UpdateAsync`-Aufruf innerhalb von `sz.environment.set` das Post-Request-Skript und damit das Gesamtergebnis des Requests fehlschlagen lassen, oder wird der Fehler still protokolliert und nur die In-Memory-Änderung behalten?
2. **SignalR-Benachrichtigung:** Soll nach der Persistierung `ISignalRNotificationService.NotifyEnvironmentChangedAsync()` aufgerufen werden, damit andere verbundene Clients im Team-Modus die aktualisierte Variable sofort erhalten? (Derzeit geschieht dies nur bei manuellen Änderungen über den Editor.)
3. **`IsValueMasked`-Erhalt:** Beim Neuaufbau der Variablenliste in `sz.environment.set` werden alle `EnvironmentVariable`-Einträge ohne `Id` und ohne `IsValueMasked` neu konstruiert. Soll der vorhandene Wert aus der aktiven Umgebung übernommen werden, um die Maskierungsflagge nicht zu verlieren?
4. **Scope des Updates:** Wird die gesamte Umgebung (alle Variablen) via `UpdateAsync` geschrieben, oder soll ein gezielter Einzelvariablen-Update-Pfad eingeführt werden? Ein gezielter Pfad wäre effizienter, erfordert aber eine neue Repository-Methode (z. B. `SetVariableAsync(int environmentId, string name, string value)`).
