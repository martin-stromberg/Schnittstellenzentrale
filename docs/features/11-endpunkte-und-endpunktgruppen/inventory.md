# Bestandsaufnahme: Endpunkte und Ordner für Anwendungen

Analysiert wurde der gesamte für Endpunkte relevante Codebestand (`Endpoint`, `EndpointGroup`, `EndpointExecutionResult`, `IEndpointRepository`, `ISignalRNotificationService`, `EndpointExecutionService`, `SignalRNotificationService`, `EndpointHub`, `AppDbContext` sowie die Blazor-Komponenten `ApplicationGroupTree`, `ApplicationContextMenu`, `EndpointList`, `EndpointExecutionPanel`, `EndpointEditor`, `ApplicationCard`, `Home`), bezogen auf die Anforderung zur Erweiterung des Navigationsbaums und der Endpunkt-Bearbeitungsansicht.

## Zusammenfassung

- `Endpoint` ist vollständig modelliert (Name, Method, RelativePath, Body, AuthenticationType, Headers, QueryParameters, EndpointGroup-Zuordnung, RowVersion) — das neue Feld `BodyMode` fehlt noch.
- `EndpointGroup` ist vollständig modelliert (Name, ApplicationId, Endpoints, RowVersion).
- `EndpointExecutionResult` enthält `Success`, `StatusCode`, `RequestDetails`, `ResponseBody` und `ErrorMessage` — die neuen Felder `ResponseHeaders`, `DurationMs` und `ResponseSizeBytes` fehlen noch.
- Der Enum `BodyMode` (`None`, `Json`, `Xml`, `PlainText`) existiert noch nicht.
- `IEndpointRepository` ist vollständig definiert und implementiert (CRUD für `Endpoint`, `EndpointGroup`, `EndpointHeader`, `EndpointQueryParameter`).
- `ISignalRNotificationService` besitzt nur `NotifyApplicationChangedAsync` und `NotifyGroupChangedAsync` — die neuen Methoden `NotifyEndpointChangedAsync` und `NotifyEndpointGroupChangedAsync` fehlen.
- `EndpointHub` kennt nur `application:{id}`- und `group:{id}`-Gruppen; keine Gruppen für Endpunkte oder Endpunktgruppen.
- Die EF-Beziehung `EndpointGroup → Endpoint` ist mit `OnDelete(DeleteBehavior.SetNull)` konfiguriert — kein Kaskadenlöschen; beim Löschen einer Gruppe werden Endpunkte nicht mitgelöscht, sondern ihre Gruppenzuordnung wird auf `null` gesetzt.
- `ApplicationGroupTree` zeigt ausschließlich `ApplicationGroup`- und `Application`-Knoten; keine Endpunkt- oder Ordnerknoten, keine Icons, kein Resize-Handle.
- `ApplicationContextMenu` hat die drei bestehenden Einträge (Bearbeiten, Aus Gruppe entfernen, Löschen); neue Einträge für „Ordner anlegen" und „Endpunkt anlegen" fehlen.
- `EndpointList` und `EndpointExecutionPanel` sind vorhanden und funktionsfähig; sie werden laut Anforderung durch den Navigationsbaum bzw. `EndpointPage` ersetzt.
- `EndpointEditor` existiert als eigenständige Formularkomponente innerhalb von `EndpointExecutionPanel`.
- `ApplicationCard` bindet `EndpointList` noch ein.
- Alle neuen UI-Komponenten (`EndpointPage`, `EndpointContextMenu`, `EndpointGroupContextMenu`, `ConfirmDeleteEndpointGroupDialog`, `RenameEndpointGroupDialog`, `RequestAuthPanel`, `RequestHeadersPanel`, `RequestQueryParamsPanel`, `RequestBodyPanel`, `ResponseBodyPanel`, `ResponseHeadersPanel`) fehlen vollständig.
- `EndpointRepositoryIntegrationTests` enthält nur einen Concurrency-Test; Kaskadenlösch-Szenarien für `DeleteEndpointGroupAsync` fehlen.
- `EndpointExecutionServiceTests` prüft die fünf Authentifizierungstypen und Verbindungsfehler; Tests für `ResponseHeaders`, `DurationMs` und `ResponseSizeBytes` fehlen.
- `ApplicationContextMenuTests` (bUnit) existiert als Vorlage für die zu erstellenden Komponententests für `EndpointContextMenu` und `EndpointGroupContextMenu`.

## Details

- [Datenmodell](inventory/models.md)
- [Enums](inventory/enums.md)
- [Interfaces](inventory/interfaces.md)
- [Logik](inventory/logic.md)
- [UI-Komponenten](inventory/ui.md)
- [Tests](inventory/tests.md)
