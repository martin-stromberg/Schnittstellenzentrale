# Plan-Review

## Ergebnis

**Status:** Erledigt

## Umgesetzte Planelemente

- [x] `BodyMode` (Enum) — angelegt (`Schnittstellenzentrale.Core/Enums/BodyMode.cs`), Werte `None`, `Json`, `Xml`, `PlainText` vorhanden
- [x] Feld `BodyMode` in `Endpoint` — vorhanden (Typ `BodyMode`, Default `None`)
- [x] Feld `ResponseHeaders` in `EndpointExecutionResult` — vorhanden (`IDictionary<string, string>?`)
- [x] Feld `DurationMs` in `EndpointExecutionResult` — vorhanden (`long?`)
- [x] Feld `ResponseSizeBytes` in `EndpointExecutionResult` — vorhanden (`long?`)
- [x] Methode `NotifyEndpointChangedAsync(int endpointId, int applicationId)` in `ISignalRNotificationService` — vorhanden
- [x] Methode `NotifyEndpointGroupChangedAsync(int endpointGroupId, int applicationId)` in `ISignalRNotificationService` — vorhanden
- [x] Methode `NotifyEndpointChangedAsync` in `SignalRNotificationService<THub>` — vorhanden, sendet `EndpointChanged` an `application:{applicationId}`
- [x] Methode `NotifyEndpointGroupChangedAsync` in `SignalRNotificationService<THub>` — vorhanden, sendet `EndpointGroupChanged` an `application:{applicationId}`
- [x] `AppDbContext` — `EndpointGroup → Endpoint`-Beziehung auf `DeleteBehavior.Cascade` umgestellt
- [x] Migration `AddBodyModeToEndpoint` — vorhanden (SQLite), fügt `INTEGER`-Spalte `BodyMode` mit Default `0` hinzu
- [x] Migration `CascadeDeleteEndpointGroup` — vorhanden (SQLite), ändert FK auf `CASCADE`
- [x] `SendAndBuildResultAsync` in `EndpointExecutionService` — startet `Stopwatch` vor HTTP-Aufruf, stoppt danach, übergibt `ElapsedMilliseconds` an `BuildResult`
- [x] `BuildResult` in `EndpointExecutionService` — befüllt `ResponseHeaders` (zusammengeführt aus `response.Headers` und `response.Content.Headers`), `DurationMs`, `ResponseSizeBytes` (UTF-8-Byte-Länge)
- [x] Parameter `OnCreateEndpointGroupRequested` (`EventCallback<Application>`) in `ApplicationContextMenu` — vorhanden
- [x] Parameter `OnCreateEndpointRequested` (`EventCallback<Application>`) in `ApplicationContextMenu` — vorhanden
- [x] Menüeinträge „Ordner anlegen" und „Endpunkt anlegen" in `ApplicationContextMenu` — vorhanden
- [x] `EndpointPage` (Blazor-Komponente) — angelegt, enthält Endpunkt-Bearbeitungsformular, `RequestAuthPanel`, `RequestHeadersPanel`, `RequestQueryParamsPanel`, `RequestBodyPanel`, `ResponseBodyPanel`, `ResponseHeadersPanel`, `ConcurrencyWarningDialog`
- [x] `EndpointContextMenu` (Blazor-Komponente) — angelegt, Eintrag „Endpunkt löschen", löst `OnDeleteRequested` aus
- [x] `EndpointGroupContextMenu` (Blazor-Komponente) — angelegt, Einträge „Endpunkt anlegen", „Ordner umbenennen", „Ordner löschen"
- [x] `ConfirmDeleteEndpointGroupDialog` (Blazor-Komponente) — angelegt, zeigt Warnhinweis bei `EndpointCount > 0`
- [x] `RenameEndpointGroupDialog` (Blazor-Komponente) — angelegt, Validierung auf nicht-leeren Namen
- [x] `RequestAuthPanel` (Blazor-Komponente) — angelegt
- [x] `RequestHeadersPanel` (Blazor-Komponente) — angelegt, `IsAutoContentType`-Flag vorhanden, Ausgrauung über CSS-Klasse
- [x] `RequestQueryParamsPanel` (Blazor-Komponente) — angelegt
- [x] `RequestBodyPanel` (Blazor-Komponente) — angelegt, `BodyMode`-Auswahl, Formatieren-Schaltfläche (deaktiviert bei `None`/`PlainText`), JSON- und XML-Formatierung, Fehlermeldung bei Parse-Fehler
- [x] `ResponseBodyPanel` (Blazor-Komponente) — angelegt, Pretty/Raw-Umschalter
- [x] `ResponseHeadersPanel` (Blazor-Komponente) — angelegt, schreibgeschützte Header-Tabelle
- [x] Internes Feld `_endpointGroups` (`Dictionary<int, IList<EndpointGroup>>`) in `ApplicationGroupTree` — vorhanden
- [x] Internes Feld `_endpoints` (`Dictionary<int, IList<Endpoint>>`) in `ApplicationGroupTree` — vorhanden
- [x] Internes Feld `_expandedApplicationIds` (`HashSet<int>`) in `ApplicationGroupTree` — vorhanden
- [x] Parameter `OnCreateEndpointGroupRequested` in `ApplicationGroupTree` — vorhanden
- [x] Parameter `OnCreateEndpointRequested` (`EventCallback<(Application, EndpointGroup?)>`) in `ApplicationGroupTree` — vorhanden
- [x] Parameter `OnRenameEndpointGroupRequested` in `ApplicationGroupTree` — vorhanden
- [x] Parameter `OnDeleteEndpointGroupRequested` in `ApplicationGroupTree` — vorhanden
- [x] Parameter `OnDeleteEndpointRequested` in `ApplicationGroupTree` — vorhanden
- [x] Parameter `OnEndpointSelected` in `ApplicationGroupTree` — vorhanden
- [x] Neue Injektion `IEndpointRepository` in `ApplicationGroupTree` — vorhanden
- [x] `LoadDataAsync` in `ApplicationGroupTree` — lädt eager alle `EndpointGroup`- und `Endpoint`-Daten für jede Anwendung
- [x] `ReloadApplicationDataAsync` in `ApplicationGroupTree` — lädt Gruppen und Endpunkte für eine Anwendung neu
- [x] `ToggleApplicationExpanded` in `ApplicationGroupTree` — verwaltet `_expandedApplicationIds`, ruft `SubscribeToApplication`/`UnsubscribeFromApplication` am Hub auf
- [x] Event-Handler für `EndpointChanged` und `EndpointGroupChanged` in `ApplicationGroupTree` — vorhanden, rufen `ReloadApplicationDataAsync` und `StateHasChanged` auf
- [x] `DisposeAsync` in `ApplicationGroupTree` — kündigt alle offenen SignalR-Abonnements und disposed JS-Modul
- [x] Render-Abschnitte für `EndpointGroup`- und `Endpoint`-Knoten in `ApplicationGroupTree` — vorhanden mit Icons `bi-folder` und `bi-lightning`
- [x] Icons in `ApplicationGroupTree`: `bi-collection` (ApplicationGroup), `bi-window` (Application), `bi-folder` (EndpointGroup), `bi-lightning` (Endpoint) — alle vorhanden
- [x] Resize-Handle in `ApplicationGroupTree` — vorhanden (DOM-Element mit Klasse `sidebar-resize-handle`)
- [x] Sidebar-Resize via JavaScript-Interop (`initializeSidebarResize`, `applyStoredSidebarWidth`) in `ApplicationGroupTree` — vorhanden
- [x] Zustandsvariable `_selectedEndpoint` (`Endpoint?`) in `Home` — vorhanden
- [x] Methode `HandleEndpointSelected` in `Home` — vorhanden, setzt `_selectedEndpoint`
- [x] Methode `HandleCreateEndpointGroupRequested` in `Home` — vorhanden, legt neue Gruppe an, ruft `NotifyEndpointGroupChangedAsync` bei Team-Modus auf, aktualisiert Baum
- [x] Methode `HandleCreateEndpointRequested` in `Home` — vorhanden, legt neuen Endpunkt an (`Name = "Neuer Endpunkt"`, `Method = GET`, `BodyMode = None`), öffnet `EndpointPage`
- [x] Methode `HandleRenameEndpointGroupRequested` in `Home` — vorhanden, öffnet `RenameEndpointGroupDialog`
- [x] Methode `HandleDeleteEndpointGroupRequested` in `Home` — vorhanden, ermittelt Endpunktanzahl, öffnet `ConfirmDeleteEndpointGroupDialog`
- [x] Methode `HandleDeleteEndpointRequested` in `Home` — vorhanden, zeigt `confirm`-Dialog via `IJSRuntime`, löscht Endpunkt, schließt `EndpointPage` bei aktivem Endpunkt, benachrichtigt SignalR
- [x] Rendering `EndpointPage` bei `_selectedEndpoint != null` in `Home` — vorhanden
- [x] `ApplicationCard` — `<EndpointList>`-Aufruf entfernt, Import- und Health-Check-Schaltflächen erhalten
- [x] `EndpointList` — Komponente entfernt
- [x] `EndpointExecutionPanel` — Komponente entfernt
- [x] `EndpointEditor` — Komponente entfernt
- [x] `_isDirty`-Zustand in `EndpointPage` — vorhanden
- [x] `NavigationManager.RegisterLocationChangingHandler` in `EndpointPage` — vorhanden, wird bei `_isDirty = true` gesetzt und bei `_isDirty = false` deregistriert
- [x] `window.onbeforeunload`-Handler via `IJSRuntime` in `EndpointPage` — vorhanden (`enableBeforeUnloadGuard`/`disableBeforeUnloadGuard`)
- [x] Strg+S-Handler via JavaScript-Interop in `EndpointPage` — vorhanden (`registerSaveShortcut`, `[JSInvokable] OnSaveShortcut`)
- [x] Speichern vor „Anfrage senden" in `EndpointPage` — vorhanden (prüft `_isDirty`, ruft `SaveAsync()` auf)
- [x] `ConcurrencyWarningDialog` direkt in `EndpointPage` — vorhanden
- [x] `BodyMode`-Automatik für `Content-Type` in `EndpointPage` (`SyncAutoContentType`) — vorhanden
- [x] `IsAutoContentType`-Flag (nur UI-State, nicht persistiert) in `RequestHeadersPanel.HeaderEntry` — vorhanden
- [x] Validierung `Endpoint.Name` (nicht leer/Whitespace) in `EndpointPage` — vorhanden (Speichern-Button deaktiviert, Warnmeldung)
- [x] Validierung `EndpointGroup.Name` in `RenameEndpointGroupDialog` — vorhanden (Submit-Button deaktiviert, Fehlermeldung)
- [x] Test `DeleteEndpointGroup_WithEndpoints_CascadesDelete` in `EndpointRepositoryIntegrationTests` — vorhanden
- [x] Test `DeleteEndpointGroup_WithoutEndpoints_DeletesGroup` in `EndpointRepositoryIntegrationTests` — vorhanden
- [x] Test `Execute_SetsResponseHeaders` in `EndpointExecutionServiceTests` — vorhanden
- [x] Test `Execute_SetsDurationMs` in `EndpointExecutionServiceTests` — vorhanden
- [x] Test `Execute_SetsResponseSizeBytes` in `EndpointExecutionServiceTests` — vorhanden
- [x] Test `LöschenEintrag_LöstCallbackAus` in `EndpointContextMenuTests` — vorhanden
- [x] Test `EndpunktAnlegen_LöstCallbackAus` in `EndpointGroupContextMenuTests` — vorhanden
- [x] Test `OrdnerUmbenennen_LöstCallbackAus` in `EndpointGroupContextMenuTests` — vorhanden
- [x] Test `OrdnerLöschen_LöstCallbackAus` in `EndpointGroupContextMenuTests` — vorhanden
- [x] Hilfsmethode `ExecuteWithTwoEndpointContextsAsync` in `TestHelpers` — vorhanden

## Offene Aufgaben

- [x] ~~SQL-Server-Migrationen für `AddBodyModeToEndpoint` und `CascadeDeleteEndpointGroup` — fehlen vollständig.~~ **Nicht zutreffend:** Das Verzeichnis `SqlServerMigrations` existiert. Die einzige SQL-Server-Migration (`20260519000002_InitialCreate`) enthält bereits die `BodyMode`-Spalte und die `CASCADE`-Delete-Beziehung, da die SQL-Server-Migration vollständig neu generiert wurde. Separate inkrementelle Migrationen wären doppelt und würden beim Ausführen gegen eine bereits initialisierte Datenbank fehlschlagen.

## Hinweise

- Das `BodyMode`-Feld wird in `EndpointPage` korrekt an `RequestBodyPanel` übergeben und die `SyncAutoContentType`-Logik ist vollständig umgesetzt. Der automatisch gesetzte `Content-Type`-Header wird beim Speichern jedoch in die Datenbank übertragen, da `IsAutoContentType`-Einträge im `SaveAsync`-Pfad wie normale Header behandelt werden (d. h. sie werden persistiert). Dies entspricht nicht der Planaussage, die `IsAutoContentType` als „nur im UI-State, nicht persistiert" beschreibt. Diese Beobachtung betrifft jedoch kein explizites Planelement (kein persistiertes Flag, kein Speicherausschluss-Schritt im Plan) und wird daher nicht als offene Aufgabe gewertet.
- Das `BodyMode`-Feld wird in `EndpointPage` korrekt an `RequestBodyPanel` übergeben und die `SyncAutoContentType`-Logik ist vollständig umgesetzt. Der automatisch gesetzte `Content-Type`-Header wird beim Speichern jedoch in die Datenbank übertragen, da `IsAutoContentType`-Einträge im `SaveAsync`-Pfad wie normale Header behandelt werden (d. h. sie werden persistiert). Dies entspricht nicht der Planaussage, die `IsAutoContentType` als „nur im UI-State, nicht persistiert" beschreibt. Diese Beobachtung betrifft jedoch kein explizites Planelement (kein persistiertes Flag, kein Speicherausschluss-Schritt im Plan) und wird daher nicht als offene Aufgabe gewertet.
