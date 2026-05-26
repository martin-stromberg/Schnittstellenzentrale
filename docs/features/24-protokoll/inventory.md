# Bestandsaufnahme: Aktivitätsprotokoll

Analysiert wurden die Klassen und Komponenten, die laut Anforderung erweitert oder als Referenz für Implementierungsmuster verwendet werden sollen: `EndpointExecutionService`, `EndpointScriptRunner`, `MainLayout`, `ApplicationGroupTree`, `Home`, `IActiveEnvironmentService`, `IStorageModeService`, `LocalStorageKeys` sowie das JS-Modul `endpoint-page.js`.

## Zusammenfassung

- `ActivityLogEntry`, `ActivityLogCategory`, `IActivityLogService` und `ActivityLogService` existieren noch nicht und sind vollständig neu zu erstellen.
- `ActivityLogPanel` und `activity-log-panel.js` existieren noch nicht.
- `LocalStorageKeys` enthält nur einen Schlüssel (`SelectedEnvironmentId`); `ActivityLogDisplayMode` und `ActivityLogPanelHeight` fehlen.
- `EndpointExecutionService` hat noch kein `IActivityLogService` injiziert; kein Logging von erfolgreichen Requests, HTTP-Fehlern oder internen Exceptions.
- `EndpointScriptRunner` hat noch kein `IActivityLogService` injiziert; `sz.console.write` ist nicht registriert; keine Protokollierung von Skriptausführung oder Fehlern.
- `MainLayout` hat noch kein `IActivityLogService` injiziert; kein Protokoll-Symbol in der `.top-row`; kein `ContextSwitched`-Logging für Modus- oder Umgebungswechsel.
- `ApplicationGroupTree` und `Home` haben noch kein `IActivityLogService` injiziert; keine Protokollaufrufe nach Persistierungsoperationen.
- Das Scoped-Service-Muster (analog zu `ActiveEnvironmentService`, `StorageModeService`) und das Event-Abonnement-/Dispose-Muster (analog zu `StorageModeService.OnModeChanged` in `ApplicationGroupTree`) sind im Projekt etabliert und können direkt übernommen werden.
- `EnvironmentVariable.IsValueMasked` existiert bereits — die erforderliche Maskierungsinfrastruktur ist im Datenmodell vorhanden.
- `EndpointExecutionResult.RequestDetails` enthält bereits „Methode + URL", ist aber nicht vollständig (kein Statuscode).
- `endpoint-page.js` stellt mit `initializeSidebarResize` und `localStorage`-Zugriff ein vollständiges Referenzmuster für das geplante `activity-log-panel.js` bereit.
- Tests für `EndpointExecutionService` und `EndpointScriptRunner` sind umfangreich vorhanden; Muster für Mocks (`CreateEmptyActiveEnvironmentMock`, `CreateRunner`) können für neue Tests übernommen werden. `MainLayoutTests` sind vorhanden, aber knapp.

## Details

- [Datenmodell](inventory/models.md)
- [Logik](inventory/logic.md)
- [Enums](inventory/enums.md)
- [Interfaces](inventory/interfaces.md)
- [JavaScript und LocalStorageKeys](inventory/javascript.md)
- [Tests](inventory/tests.md)
