# Bestandsaufnahme: Verwaltung von Systemumgebungen mit Variablen

Analysiert wurde der bestehende Code bezogen auf die Anforderung zur Einführung von `SystemEnvironment` und `EnvironmentVariable` — einschließlich der Platzhalter-Auflösung im `EndpointExecutionService` und der Verwaltungs-UI im `MainLayout`.

## Zusammenfassung

- **`StorageMode`-Enum** ist vollständig vorhanden (`Team` / `User`) und wird bereits in `ApplicationRepository` für Owner-Filterung verwendet — das Muster ist auf `SystemEnvironmentRepository` übertragbar.
- **`AppDbContext`** enthält keine `DbSet<SystemEnvironment>` oder `DbSet<EnvironmentVariable>`; beide Tabellen fehlen vollständig (kein Modell, keine Migration, keine Konfiguration).
- **`ISystemEnvironmentRepository`** und **`SystemEnvironmentRepository`** existieren nicht.
- **`IActiveEnvironmentService`** und **`ActiveEnvironmentService`** existieren nicht.
- **`EndpointExecutionService`** löst `{Pfadparameter}`-Platzhalter bereits via `EndpointUrlBuilder.Resolve` auf; `{{Variablenname}}`-Auflösung und Abhängigkeit von `IActiveEnvironmentService` fehlen.
- **`BuildRequest`** verarbeitet noch keine Basis-URL, Header oder Bearer-Token-Felder für Variablenauflösung — nur Pfad und Query-Parameter (via `EndpointUrlBuilder`).
- **`MainLayout`** enthält den `StorageMode`-Schalter, aber weder `EnvironmentSelector`, Zahnrad-Icon noch `localStorage`-Interaktion.
- **Blazor-Komponenten** `EnvironmentSelector`, `EnvironmentManagementOverlay` und `EnvironmentEditor` existieren nicht.
- **`ISignalRNotificationService`** hat keine Methode `NotifyEnvironmentChangedAsync`.
- **Testinfrastruktur** (`TestHelpers`, `EndpointExecutionServiceTests`, `ApplicationRepositoryIntegrationTests`) ist als Vorlage vorhanden; `SystemEnvironmentRepositoryIntegrationTests` und die neuen `EndpointExecutionServiceTests`-Szenarien fehlen.
- Die Hilfsmethoden `TestHelpers.CreateInMemoryDbContext` und `TestHelpers.ExecuteWithTwoContextsAsync` können für neue Integrationstests direkt oder mit kleinen Anpassungen wiederverwendet werden.

## Details

- [Datenmodell](inventory/models.md)
- [Logik](inventory/logic.md)
- [Enums](inventory/enums.md)
- [Interfaces](inventory/interfaces.md)
- [Tests](inventory/tests.md)
