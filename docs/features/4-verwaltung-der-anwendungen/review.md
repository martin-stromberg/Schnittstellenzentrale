# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

- [x] `ApplicationGroupEditor` (Blazor-Komponente) — angelegt
- [x] Parameter `OnSaved` in `ApplicationGroupEditor` — vorhanden
- [x] Parameter `OnCancel` in `ApplicationGroupEditor` — vorhanden
- [x] Pflichtfeld `Name` in `ApplicationGroupEditor` (`EditForm` mit `DataAnnotationsValidator`) — vorhanden
- [x] Aufruf `IApplicationRepository.AddGroupAsync` in `ApplicationGroupEditor` — vorhanden
- [x] Aufruf `ISignalRNotificationService.NotifyGroupChangedAsync` in `ApplicationGroupEditor` — vorhanden
- [x] Aufruf `OnSaved.InvokeAsync()` nach Speichern in `ApplicationGroupEditor` — vorhanden

- [x] `ApplicationEditor` (Blazor-Komponente) — angelegt
- [x] Parameter `OnSaved` in `ApplicationEditor` — vorhanden
- [x] Parameter `OnCancel` in `ApplicationEditor` — vorhanden
- [x] Pflichtfeld `Name` in `ApplicationEditor` — vorhanden
- [x] Pflichtfeld `BaseUrl` in `ApplicationEditor` — vorhanden
- [x] Optionales Feld `Description` in `ApplicationEditor` — vorhanden
- [x] Optionales Feld `SwaggerUrl` in `ApplicationEditor` — vorhanden
- [x] Optionales Feld `MetadataUrl` in `ApplicationEditor` — vorhanden
- [x] Optionales Feld `ApplicationGroupId` (Select-Element) in `ApplicationEditor` — vorhanden
- [x] Laden aller Gruppen in `OnInitializedAsync` in `ApplicationEditor` — vorhanden
- [x] Aufruf `IApplicationRepository.AddApplicationAsync` in `ApplicationEditor` — vorhanden
- [x] Aufruf `ISignalRNotificationService.NotifyApplicationChangedAsync` in `ApplicationEditor` — vorhanden
- [x] Aufruf `OnSaved.InvokeAsync()` nach Speichern in `ApplicationEditor` — vorhanden

- [x] Feld `_showGroupEditor` (`bool`) in `ApplicationGroupTree` — vorhanden
- [x] Feld `_showApplicationEditor` (`bool`) in `ApplicationGroupTree` — vorhanden
- [x] Methode `ShowGroupEditor` in `ApplicationGroupTree` — vorhanden (setzt `_showGroupEditor = true`, `_showApplicationEditor = false`)
- [x] Methode `ShowApplicationEditor` in `ApplicationGroupTree` — vorhanden (setzt `_showApplicationEditor = true`, `_showGroupEditor = false`)
- [x] Methode `OnGroupSaved` in `ApplicationGroupTree` — vorhanden (setzt `_showGroupEditor = false`, ruft `LoadDataAsync` und `StateHasChanged` auf)
- [x] Methode `OnApplicationSaved` in `ApplicationGroupTree` — vorhanden (setzt `_showApplicationEditor = false`, ruft `LoadDataAsync` und `StateHasChanged` auf)
- [x] Methode `OnEditorCancelled` in `ApplicationGroupTree` — vorhanden (setzt beide Flags auf `false`)
- [x] Schaltfläche „Neue Gruppe" in `ApplicationGroupTree` — vorhanden (ruft `ShowGroupEditor` auf)
- [x] Schaltfläche „Neue Anwendung" in `ApplicationGroupTree` — vorhanden (ruft `ShowApplicationEditor` auf)
- [x] Bedingte Einbettung `ApplicationGroupEditor` in `ApplicationGroupTree` — vorhanden (`@if (_showGroupEditor)`)
- [x] Bedingte Einbettung `ApplicationEditor` in `ApplicationGroupTree` — vorhanden (`@if (_showApplicationEditor)`)

- [x] Testmethode `AddGroup_PersistsNewGroup` in `ApplicationRepositoryIntegrationTests` — vorhanden
- [x] Testmethode `AddApplication_WithGroup_PersistsApplication` in `ApplicationRepositoryIntegrationTests` — vorhanden
- [x] Testmethode `AddApplication_WithoutGroup_PersistsUngroupedApplication` in `ApplicationRepositoryIntegrationTests` — vorhanden
- [x] Testmethode `AddApplication_WithStorageModeUser_SetsOwner` in `ApplicationRepositoryIntegrationTests` — vorhanden

## Offene Aufgaben

Keine.

## Hinweise

- `ApplicationGroupEditor` injiziert zusätzlich `IStorageModeService` (nicht im Plan erwähnt, aber für die bedingte SignalR-Benachrichtigung erforderlich — analog zu `ApplicationEditor`). Dies stellt keine Planabweichung dar.
- Der `ApplicationEditor` enthält eine „Ohne Gruppe"-Option (`value=""`) im Select-Element, wie in den offenen Punkten des Plans als Anforderung beschrieben.
- Die Schaltflächen sind oberhalb der Gruppenliste positioniert, wie im Plan vorgesehen.
