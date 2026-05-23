# Umsetzungsplan: Automatische Endpunktregistrierung der eigenen API beim Start

## Übersicht

Nach dem App-Start führt ein neuer `SystemEndpointSyncService` (implementiert `BackgroundService`) einen einmaligen Abgleich zwischen den in der Datenbank gespeicherten Endpunkten der Systemanwendung und der eigenen Swagger-Definition durch. Neue Endpunkte werden angelegt, entfernte gelöscht; ändert sich der Name eines Endpunkts (durch geänderte `operationId`), wird nur der `Name` in der Datenbank aktualisiert — alle anderen manuell konfigurierten Felder (`AuthenticationType`, `Body`, `Headers`, `QueryParameters`) bleiben erhalten. Betroffen sind die Projekte `Schnittstellenzentrale` (neuer Service, DI-Registrierung) und `Schnittstellenzentrale.Tests` (neue Testklasse, Anpassung `ControllerTestFactory`).

---

## Programmabläufe

### Endpunktabgleich beim App-Start

1. Der Host startet nach `app.Run()` alle registrierten `IHostedService`-Instanzen, darunter `SystemEndpointSyncService`.
2. `SystemEndpointSyncService.ExecuteAsync` beginnt in einem `try/catch`-Block.
3. Es wird ein neuer DI-Scope über `IServiceScopeFactory.CreateScope()` erzeugt.
4. Aus dem Scope wird `IApplicationRepository` aufgelöst; `GetSystemGroupAsync()` wird aufgerufen.
5. Gibt `GetSystemGroupAsync()` `null` zurück, wird eine Warnung per `ILogger<SystemEndpointSyncService>` geloggt und `ExecuteAsync` beendet (kein Fehler).
6. Aus der Systemgruppe wird die Systemanwendung (`IsSystem == true`) ermittelt. Ist keine vorhanden, analog: Warnung loggen und beenden.
7. Aus dem Scope wird `ISwaggerImportService` aufgelöst; `ImportAsync(systemApplication)` wird aufgerufen und liefert einen `ImportDiff`.
8. Ist `diff.ErrorMessage` nicht `null`, wird ein Fehler geloggt und `ExecuteAsync` beendet.
9. Für jeden Eintrag in `diff.NewEndpoints` wird `IEndpointRepository.AddEndpointAsync(endpoint)` aufgerufen.
10. Für jeden Eintrag in `diff.RemovedEndpoints` wird `IEndpointRepository.DeleteEndpointAsync(endpoint.Id)` aufgerufen.
11. Für jeden Eintrag in `diff.ChangedEndpoints` wird `IEndpointRepository.UpdateEndpointNameAsync(existingId, newName)` aufgerufen — nur der `Name` wird überschrieben; alle anderen Felder bleiben unverändert.
12. Der Scope wird disposed; `ExecuteAsync` endet ohne erneuten Aufruf.
13. Tritt in Schritt 3–11 eine unerwartete Exception auf, wird sie im `catch`-Block per `ILogger<SystemEndpointSyncService>` auf `Error`-Level geloggt; die Exception wird nicht propagiert.

Beteiligte Klassen/Komponenten: `SystemEndpointSyncService`, `IServiceScopeFactory`, `IApplicationRepository`, `ISwaggerImportService`, `IEndpointRepository`, `ImportDiff`, `Application`, `Endpoint`

---

## Neue Klassen

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `SystemEndpointSyncService` | Klasse, erbt von `BackgroundService` | Führt nach App-Start einmalig den selektiven Endpunktabgleich durch; löst Scoped-Dienste über `IServiceScopeFactory` auf |

---

## Änderungen an bestehenden Klassen

### `Program.cs` (Startup)

- **Neue Registrierung:** `builder.Services.AddHostedService<SystemEndpointSyncService>()` — registriert den neuen Service als `IHostedService`; wird nach dem bestehenden `AddScoped`-Block für Repositories und Services eingefügt.

### `IEndpointRepository` (Interface)

- **Neue Methode:** `UpdateEndpointNameAsync(int id, string name)` — aktualisiert ausschließlich den `Name`-Wert eines bestehenden Endpunkts anhand seiner ID, ohne andere Felder zu berühren.

### `EndpointRepository` (Klasse)

- **Neue Methode:** `UpdateEndpointNameAsync(int id, string name)` — implementiert das Interface; führt ein gezieltes Update nur der `Name`-Spalte durch (z. B. per `ExecuteUpdateAsync` oder durch selektives Laden und Aktualisieren des Felds).

### `ControllerTestFactory` (Testklasse)

- **Neue Konfiguration:** Der `SystemEndpointSyncService` wird in der `WebApplicationFactory`-Konfiguration unterdrückt, indem er via `RemoveHostedService<SystemEndpointSyncService>()` (oder äquivalent über `services.Remove(...)` auf dem `ServiceDescriptor`) aus der DI-Registrierung entfernt wird. Dadurch läuft er bei Integrationstests nicht im Hintergrund.

---

## Datenbankmigrationen

Keine.

---

## Validierungsregeln

Keine.

---

## Konfigurationsänderungen

Keine.

---

## Konventionen

### `IServiceScopeFactory` als Standard für Hosted Services

`IServiceScopeFactory` wird als Constructor-Dependency in `IHostedService`- bzw. `BackgroundService`-Implementierungen injiziert, um Scoped-Dienste innerhalb des Singleton-Lebenszyklus aufzulösen. `SystemEndpointSyncService` ist das erste und maßgebliche Beispiel dieser Konvention im Projekt. Alle zukünftigen Hosted Services sollen dieses Muster verwenden.

---

## Seiteneffekte und Risiken

- **`ApplyDiffAsync` wird nicht verwendet:** Der `SystemEndpointSyncService` ruft `ApplyDiffAsync` bewusst nicht auf, da diese Methode auch `ChangedEndpoints` vollständig überschreiben würde. Zukünftige Erweiterungen von `ApplyDiffAsync` wirken sich daher nicht automatisch auf den Sync-Service aus — dies ist gewollt, muss aber bei späteren Änderungen an `SwaggerImportService` bedacht werden.
- **Reihenfolge gegenüber `SystemEntryInitializer`:** `SystemEndpointSyncService` setzt voraus, dass `SystemEntryInitializer.InitializeAsync` zuvor erfolgreich die Systemanwendung mit einer gültigen `InterfaceUrl` angelegt hat. Schlägt `SystemEntryInitializer` lautlos fehl, erkennt `SystemEndpointSyncService` dies über `null` von `GetSystemGroupAsync()` und bricht mit Warnung ab.
- **`ControllerTestFactory` muss angepasst werden:** Ohne explizite Unterdrückung würde `SystemEndpointSyncService` bei allen Controller-Integrationstests im Hintergrund starten. Mit der geplanten Anpassung wird dieser Seiteneffekt vollständig eliminiert.
- **`UpdateEndpointNameAsync` ist eine neue Repository-Methode:** Das Interface `IEndpointRepository` und die Implementierung `EndpointRepository` werden erweitert. Bestehende Mocks in Tests, die `IEndpointRepository` vollständig implementieren müssen, sind ggf. anzupassen.

---

## Umsetzungsreihenfolge

1. `IEndpointRepository` um `UpdateEndpointNameAsync(int id, string name)` erweitern.
2. `EndpointRepository` mit der Implementierung von `UpdateEndpointNameAsync` ergänzen.
3. `SystemEndpointSyncService` im Projekt `Schnittstellenzentrale` anlegen (erbt von `BackgroundService`; Constructor erhält `IServiceScopeFactory` und `ILogger<SystemEndpointSyncService>`).
4. `ExecuteAsync` implementieren: Scope erzeugen, Systemanwendung ermitteln, `ImportAsync` aufrufen, `NewEndpoints` anlegen, `RemovedEndpoints` löschen, `ChangedEndpoints` per `UpdateEndpointNameAsync` mit dem neuen Namen aktualisieren, Fehlerbehandlung per `try/catch`.
5. `Program.cs` um `builder.Services.AddHostedService<SystemEndpointSyncService>()` ergänzen.
6. `ControllerTestFactory` anpassen: `SystemEndpointSyncService` via `RemoveHostedService` aus der DI-Registrierung entfernen.
7. Testklasse `SystemEndpointSyncServiceTests` anlegen und alle Testfälle implementieren.

---

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `ExecuteAsync_NewEndpoints_AreAdded` | `SystemEndpointSyncServiceTests` | `AddEndpointAsync` wird für jeden Eintrag in `diff.NewEndpoints` aufgerufen |
| `ExecuteAsync_RemovedEndpoints_AreDeleted` | `SystemEndpointSyncServiceTests` | `DeleteEndpointAsync` wird für jeden Eintrag in `diff.RemovedEndpoints` aufgerufen |
| `ExecuteAsync_ChangedEndpoints_NameIsUpdated` | `SystemEndpointSyncServiceTests` | `UpdateEndpointNameAsync` wird für jeden Eintrag in `diff.ChangedEndpoints` mit dem neuen Namen aufgerufen; `UpdateEndpointAsync` wird nicht aufgerufen |
| `ExecuteAsync_WhenImportReturnsError_LogsErrorAndStarts` | `SystemEndpointSyncServiceTests` | Bei gesetztem `diff.ErrorMessage` wird ein Fehler geloggt; keine Repository-Aufrufe; keine Exception-Propagierung |
| `ExecuteAsync_WhenDbThrows_LogsErrorAndStarts` | `SystemEndpointSyncServiceTests` | Exception aus `AddEndpointAsync` oder `DeleteEndpointAsync` wird abgefangen und geloggt; Anwendung startet normal |
| `ExecuteAsync_IsIdempotent_OnRepeatedCall` | `SystemEndpointSyncServiceTests` | Wiederholter Aufruf mit identischem Diff (alle Endpunkte bereits vorhanden, keine entfernt) erzeugt keine doppelten Einträge |
| `ExecuteAsync_WhenSystemGroupMissing_LogsWarningAndSkips` | `SystemEndpointSyncServiceTests` | `GetSystemGroupAsync()` gibt `null` zurück: Warnung wird geloggt, keine weiteren Aufrufe |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| `ControllerTestFactory` | `SystemEndpointSyncService` muss via `RemoveHostedService` unterdrückt werden, damit er bei Integrationstests nicht im Hintergrund läuft |

---

## Offene Punkte

Keine.
