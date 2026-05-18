# Bestandsaufnahme: Eigene REST-API-Schnittstelle

Analysiert wurde der bestehende Code in `src/` bezogen auf die Anforderung, eine eigene REST-API für die Anlage von `ApplicationGroup`- und `Application`-Datensätzen einzuführen und die Blazor-UI-Komponenten auf einen neuen REST-Client umzustellen.

## Zusammenfassung

- **Datenmodell vorhanden:** `ApplicationGroup` und `Application` sind vollständig modelliert, inkl. Datenbankconstraints (`MaxLength`, `IsRequired`, Concurrency-Token `RowVersion`) und Navigation-Properties.
- **Repository vollständig vorhanden:** `IApplicationRepository` definiert alle CRUD-Methoden für Gruppen und Applications; `ApplicationRepository` implementiert sie vollständig inkl. Owner-Filter-Logik für `StorageMode.User`.
- **`StorageMode`-Kontext:** Repository-Methoden nehmen `StorageMode` und `owner` als explizite Parameter — diese Übergabe muss beim API-Entwurf berücksichtigt werden.
- **UI-Komponenten injizieren direkt das Repository:** `ApplicationGroupEditor` und `ApplicationEditor` rufen `IApplicationRepository` direkt auf. Der Wechsel auf `IApplicationApiClient` ist noch nicht vorbereitet.
- **SignalR-Benachrichtigungen in der UI:** Beide Komponenten rufen `ISignalRNotificationService` nach dem Speichern selbst auf. Bei einem Controller-basierten Ansatz muss geklärt werden, wo diese Benachrichtigungen künftig ausgelöst werden.
- **Windows-Authentifizierung aktiv:** `UseAuthentication` / `UseAuthorization` sind in `Program.cs` bereits eingebunden. Ein benannter HTTP-Client `"negotiate"` mit `UseDefaultCredentials = true` ist registriert.
- **Keine Controller-Infrastruktur vorhanden:** `AddControllers()` und `MapControllers()` fehlen in `Program.cs`. Es gibt weder Controller-Klassen noch Controller-Verzeichnisse.
- **Keine DTOs vorhanden:** `CreateApplicationGroupRequest`, `ApplicationGroupResponse`, `CreateApplicationRequest`, `ApplicationResponse` existieren nicht.
- **Kein `IApplicationApiClient` vorhanden:** Weder Interface noch Implementierung existieren.
- **Kein `Api:BaseUrl`-Konfigurationsschlüssel vorhanden:** `appsettings.json` enthält keine API-Basis-URL.
- **Tests für Repository umfangreich vorhanden:** 21 Integrationstests in `ApplicationRepositoryIntegrationTests` decken alle CRUD-Operationen und Randfälle ab. `TestHelpers` stellt In-Memory-Datenbankfabrik und Zwei-Kontext-Hilfsmethode bereit. Keine Tests für Controller oder HTTP-Client vorhanden.

## Details

- [Datenmodell](inventory/models.md)
- [Logik](inventory/logic.md)
- [Enums](inventory/enums.md)
- [Interfaces](inventory/interfaces.md)
- [Tests](inventory/tests.md)
