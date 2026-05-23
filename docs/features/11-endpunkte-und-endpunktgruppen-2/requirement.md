# Anforderung: Automatische Endpunktregistrierung der eigenen API beim Start

## Fachliche Zusammenfassung

Der bestehende `SystemEntryInitializer` wird um einen nachgelagerten Endpunktabgleich ergänzt: Nach dem App-Start führt ein `IHostedService` für die Systemanwendung einen Abgleich zwischen den in der Datenbank gespeicherten `Endpoint`-Einträgen und den Pfaden der eigenen Swagger-Definition (`/swagger/v1/swagger.json`) durch. Neue Endpunkte werden über `IEndpointRepository.AddEndpointAsync` angelegt, entfernte Endpunkte werden über `DeleteEndpointAsync` gelöscht; bestehende Endpunkte werden nicht überschrieben — ihre manuell konfigurierten Felder (`AuthenticationType`, `Body`, `Headers`, `QueryParameters`) bleiben erhalten. Der Abgleich nutzt die vorhandene Infrastruktur aus `ISwaggerImportService` und `ImportDiffCalculator`, schränkt jedoch `ApplyDiffAsync` so ein, dass geänderte Endpunkte (`ChangedEndpoints`) nicht überschrieben werden. Schlägt der Abgleich fehl, wird der Fehler per Serilog protokolliert; die Anwendung läuft normal weiter.

---

## Betroffene Klassen und Komponenten

### Startup-Routine / Hosted Service — neu zu erstellen (`Schnittstellenzentrale`)

| Artefakt | Beschreibung |
|---|---|
| `SystemEndpointSyncService` (neue Klasse, implementiert `IHostedService` oder erbt von `BackgroundService`) | Wird nach `app.Run()` durch den Host gestartet. Ermittelt die Systemanwendung über `IApplicationRepository.GetSystemGroupAsync()`, ruft `ISwaggerImportService.ImportAsync` auf und wendet den Diff selektiv an: nur `NewEndpoints` und `RemovedEndpoints` werden verarbeitet; `ChangedEndpoints` werden ignoriert. Fehler werden abgefangen und per `ILogger<SystemEndpointSyncService>` geloggt. |

### DI / Startup (`Schnittstellenzentrale`)

| Artefakt | Änderung |
|---|---|
| `Program.cs` | Registrierung von `SystemEndpointSyncService` via `builder.Services.AddHostedService<SystemEndpointSyncService>()`. Kein Aufruf vor `app.Run()` — der `IHostedService`-Mechanismus stellt den zeitlich nachgelagerten Start sicher. |

### Logikklassen / Services — zu prüfen / ggf. zu erweitern (`Schnittstellenzentrale.Infrastructure`)

| Artefakt | Änderung |
|---|---|
| `ISwaggerImportService` / `SwaggerImportService` | Wird unverändert wiederverwendet. `ImportAsync` liefert den `ImportDiff`; `ApplyDiffAsync` wird vom `SystemEndpointSyncService` **nicht** direkt aufgerufen, da es auch `ChangedEndpoints` anwenden würde. *Annahme: `ApplyDiffAsync` wird nicht verändert; der neue Service ruft stattdessen selektiv `IEndpointRepository.AddEndpointAsync` und `DeleteEndpointAsync` auf.* |
| `ImportDiffCalculator` | Wird unverändert wiederverwendet. Der Schlüssel `Method:RelativePath` (bereits implementiert als `BuildKey`) dient als Identifikationsmerkmal für bestehende Endpunkte — bestätigt das offene Punkt aus der Anforderung. |

### Tests — neu zu erstellen (`Schnittstellenzentrale.Tests`)

| Artefakt | Beschreibung |
|---|---|
| `SystemEndpointSyncServiceTests` | Unit-Tests mit gemocktem `ISwaggerImportService` und `IApplicationRepository`: (1) Neue Endpunkte werden angelegt. (2) Entfernte Endpunkte werden gelöscht. (3) Bestehende Endpunkte (`ChangedEndpoints`) werden nicht überschrieben. (4) Fehler beim HTTP-Abruf der Swagger-Definition lassen die Anwendung starten; Fehler ist im Log sichtbar. (5) Fehler beim Datenbankzugriff lassen die Anwendung starten. (6) Wiederholter Start ist idempotent. |

---

## Implementierungsansatz

### Timing-Lösung via `IHostedService`

Da `SystemEntryInitializer` vor `app.Run()` ausgeführt wird und der eigene Swagger-Endpunkt zu diesem Zeitpunkt noch nicht per HTTP erreichbar ist, muss der Endpunktabgleich nach `app.Run()` stattfinden. Ein als `IHostedService` registrierter `SystemEndpointSyncService` wird vom Host nach dem Start des HTTP-Servers gestartet und kann dann den eigenen Swagger-Endpunkt per HTTP abrufen. `BackgroundService.StartAsync` (bzw. `ExecuteAsync` mit sofortigem Return nach einmaligem Durchlauf) ist der geeignete Einstiegspunkt.

### Selektive Diff-Anwendung

`ISwaggerImportService.ImportAsync` liefert einen `ImportDiff` mit `NewEndpoints`, `ChangedEndpoints` und `RemovedEndpoints`. Der `SystemEndpointSyncService` verarbeitet ausschließlich:
- `diff.NewEndpoints` → `IEndpointRepository.AddEndpointAsync` pro Eintrag
- `diff.RemovedEndpoints` → `IEndpointRepository.DeleteEndpointAsync` pro Eintrag

`diff.ChangedEndpoints` wird ignoriert, um manuell konfigurierte Felder (`AuthenticationType`, `Body`, `Headers`, `QueryParameters`) zu erhalten.

### Fehlerbehandlung

Alle Aufrufe in `ExecuteAsync` sind in einem `try/catch` eingeschlossen. Ausnahmen werden via `ILogger<SystemEndpointSyncService>` auf `Error`-Level geloggt (analog zur Konvention in `SystemEntryInitializer` mit `Log.Error`). Die Anwendung startet in jedem Fall normal; der `IHostedService`-Fehler wird nicht nach oben propagiert.

### Endpunktidentifikation

Bestehende Endpunkte werden anhand der Kombination `Method:RelativePath` identifiziert (Schlüssel `BuildKey` im `ImportDiffCalculator`). Diese Konvention ist bereits implementiert und wird nicht geändert.

### Abhängigkeiten

- `IApplicationRepository` (bereits registriert als `Scoped`) — wird per `IServiceScopeFactory` in `ExecuteAsync` aufgelöst, da `IHostedService` als `Singleton` registriert wird.
- `ISwaggerImportService` (bereits registriert als `Scoped`) — analog per `IServiceScopeFactory`.
- `IEndpointRepository` (bereits registriert als `Scoped`) — analog per `IServiceScopeFactory`.

---

## Konfiguration

Kein zusätzlicher Konfigurationsbedarf. Der Service liest die Systemanwendung aus der Datenbank (über `IApplicationRepository.GetSystemGroupAsync`); die Swagger-URL ergibt sich aus `Application.InterfaceUrl`, das bereits durch `SystemEntryInitializer` auf `{Api:BaseUrl}/swagger/v1/swagger.json` gesetzt wird.

---

## Offene Fragen

1. **Identifikationsmerkmal bestehender Endpunkte:** Die Anforderung nennt die Kombination aus HTTP-Methode und relativem Pfad als vermutlichen Schlüssel. Der `ImportDiffCalculator` implementiert genau dies (`Method:RelativePath`). *Annahme: Diese Konvention wird übernommen und muss nicht geändert werden — sollte vor der Implementierung bestätigt werden.*

2. **`ChangedEndpoints`-Semantik:** Aktuell vergleicht `ImportDiffCalculator.HasChanged` Name, Body und `AuthenticationType`. Da der Endpunktabgleich bestehende Konfigurationen nicht überschreiben soll, werden `ChangedEndpoints` ignoriert. Soll der Name eines bestehenden Endpunkts aktualisiert werden, wenn die Swagger-Definition ihn ändert (`operationId` ändert sich)? *Annahme: Nein — vollständige Erhaltung aller manuell konfigurierten Felder.*

3. **Verhalten bei fehlendem Systemeintrag:** Was soll `SystemEndpointSyncService` tun, wenn `GetSystemGroupAsync()` `null` zurückgibt (z. B. weil `SystemEntryInitializer` zuvor fehlgeschlagen ist)? Optionen: (a) Abgleich überspringen und warnen, (b) Fehler loggen und abbrechen. *Annahme: Überspringen und Warnung loggen.*

4. **Scoped-Dienste in einem Singleton-`IHostedService`:** Da `SystemEndpointSyncService` als `Singleton` gestartet wird, `ISwaggerImportService` und `IEndpointRepository` aber als `Scoped` registriert sind, muss `IServiceScopeFactory` verwendet werden. Ist diese Konvention im Projekt bereits etabliert oder neu?

5. **Einmaliger vs. wiederholter Lauf:** Die Anforderung schreibt vor, dass der Abgleich ausschließlich beim Start erfolgt. `BackgroundService.ExecuteAsync` soll daher nur einmal durchlaufen und danach beendet werden (`return` nach Abschluss). Soll trotzdem eine Retry-Logik bei transienten Fehlern (z. B. Swagger-Endpunkt noch nicht bereit) eingebaut werden?
