# Bestandsaufnahme: Systemeintrag für eigene API beim Programmstart

Analysiert wurden die Datenmodellklassen, die Datenbankschicht, die Controller, die Blazor-UI-Komponenten sowie die vorhandene Testinfrastruktur, die von der Anforderung „Systemeintrag für eigene API beim Programmstart" betroffen sind.

## Zusammenfassung

- `ApplicationGroup` und `Application` besitzen noch kein `IsSystem`-Feld; die EF-Core-Konfiguration in `AppDbContext.OnModelCreating` sowie die letzte Migration (`AddInterfaceUrl`) enthalten es nicht.
- Die Request-DTOs (`CreateApplicationGroupRequest`, `UpdateApplicationGroupRequest`, `CreateApplicationRequest`, `UpdateApplicationRequest`) enthalten `IsSystem` erwartungsgemäß **nicht** — dieser Zustand ist korrekt und soll beibehalten werden.
- Die Response-DTOs (`ApplicationGroupResponse`, `ApplicationResponse`) enthalten `IsSystem` noch **nicht**; die Erweiterung ist optional, aber für die UI-seitige Schutzlogik vorgesehen.
- `ApplicationGroupsController.DeleteAsync` und `ApplicationsController.DeleteAsync` enthalten keine Guard-Prüfung auf `IsSystem`; Systemeinträge können derzeit ungehindert gelöscht werden.
- `ApplicationGroupsController.UpdateAsync` und `ApplicationsController.UpdateAsync` enthalten ebenfalls keine Prüfung auf `IsSystem`.
- Ein `SystemEntryInitializer` existiert noch nicht; `Program.cs` enthält ausschließlich `EnsureDatabaseInitializedAsync` und `MigrateDatabaseAsync`.
- Der Konfigurationsschlüssel `Api:BaseUrl` ist in `appsettings.json` bereits vorhanden (`"https://localhost:5001"`).
- `ApplicationGroupTree.OnDragStart` und `OnDrop` enthalten noch keine Guards gegen `IsSystem`-Anwendungen.
- `ApplicationGroupContextMenu` und `ApplicationContextMenu` deaktivieren Schaltflächen noch nicht in Abhängigkeit von `IsSystem`.
- Die Testklassen `ApplicationGroupsControllerIntegrationTests` und `ApplicationsControllerIntegrationTests` decken CRUD vollständig ab, enthalten aber noch keine Tests für das 403-Verhalten bei Systemeinträgen.
- `ApplicationContextMenuTests` testet bereits die Sichtbarkeit von „Aus Gruppe entfernen", aber noch nicht das Deaktivieren von Aktionen für Systemeinträge.
- `TestHelpers` und `ControllerTestFactory` sind als Testinfrastruktur vorhanden und wiederverwendbar.
- Ein `SystemEntryInitializerTests` existiert noch nicht.

## Details

- [Datenmodell](inventory/models.md)
- [Logik](inventory/logic.md)
- [Interfaces](inventory/interfaces.md)
- [Tests](inventory/tests.md)
