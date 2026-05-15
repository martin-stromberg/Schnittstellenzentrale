# Bestandsaufnahme: Verwaltung der Anwendungen

Analysiert wurden die bestehenden Datenmodell-, Repository-, Interface- und UI-Komponenten in Bezug auf die Anforderung, Erfassungsmasken für `ApplicationGroup`- und `Application`-Datensätze bereitzustellen.

## Zusammenfassung

- `Application` und `ApplicationGroup` sind vollständig modelliert inkl. `RowVersion` für optimistische Nebenläufigkeitskontrolle.
- `IApplicationRepository` definiert alle benötigten Lese- und Schreiboperationen (`AddGroupAsync`, `AddApplicationAsync`, `UpdateGroupAsync`, `UpdateApplicationAsync`, `DeleteGroupAsync`, `DeleteApplicationAsync`).
- `ApplicationRepository` implementiert `IApplicationRepository` vollständig; Owner-Filterung per `StorageMode` ist umgesetzt.
- `ApplicationGroupTree` lädt und zeigt Gruppen und ungroupierte Anwendungen; reagiert auf `StorageModeService.OnModeChanged`; `LoadDataAsync` und `StateHasChanged` sind vorhanden und können nach Speichern aufgerufen werden.
- `ISignalRNotificationService` mit `NotifyApplicationChangedAsync` und `NotifyGroupChangedAsync` ist vollständig definiert und implementiert.
- `ICurrentUserService` und `IStorageModeService` sind vorhanden und stellen Owner-Name und aktuellen Modus bereit.
- `EndpointEditor` existiert als Referenzimplementierung eines `EditForm`-basierten Editors mit `OnSaved`- und `OnCancel`-Callbacks.
- `CollapsibleSection` steht als wiederverwendbare Baumstruktur-Komponente bereit.
- **Fehlend:** `ApplicationGroupEditor`- und `ApplicationEditor`-Blazor-Komponenten existieren noch nicht.
- **Fehlend:** Schaltflächen „Neue Gruppe" / „Neue Anwendung" in `ApplicationGroupTree` fehlen.
- **Fehlend:** Integrationstests für `AddGroupAsync` und `AddApplicationAsync` existieren noch nicht; bestehende Tests decken nur `GetApplicationsAsync` ab.

## Details

- [Datenmodell](inventory/models.md)
- [Logik](inventory/logic.md)
- [Enums](inventory/enums.md)
- [Interfaces](inventory/interfaces.md)
- [Tests](inventory/tests.md)
