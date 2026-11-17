# Abnahmeprüfung – Entwicklungsschritt 6

## Ergebnis

**Status:** Anforderung vollständig erfüllt

## Hinweise

- **Zentrale Policy-Stelle vorhanden und einzige Entscheidungsinstanz:** `INotificationPolicy` (`src/Schnittstellenzentrale.Core/Interfaces/INotificationPolicy.cs`) und `NotificationPolicy` (`src/Schnittstellenzentrale.Infrastructure/Services/NotificationPolicy.cs`, Z. 23–42) kapseln die `storageMode == StorageMode.Team`-Prüfung an genau einer Stelle und delegieren an den unveränderten `ISignalRNotificationService`. Registrierung: `Program.cs` Z. 142 (`AddScoped<INotificationPolicy, NotificationPolicy>`). In `src/Schnittstellenzentrale` verbleibt `StorageMode.Team` nur noch in den `ParseStorageMode()`-Header-Auswertungen (`ApiControllerBase.cs` Z. 49, `ODataControllerBase.cs` Z. 32) — keine dezentrale Benachrichtigungs-Entscheidung mehr in Controllern oder den vier Entity-Set-Pfaden.
- **Alle vier OData-Controller benachrichtigen korrekt** auf jeder mutierenden Operation (Post/Put/Patch/Delete):
  - `ODataApplicationsController` und `ODataApplicationGroupsController` → `NotifyTreeMutationAsync` (Baum-Benachrichtigung, je 4 Aufrufstellen).
  - `ODataEndpointsController` → `NotifyEndpointMutationAsync` (Z. 95, 149, 203, 273; Delete nutzt vorgemerkte `applicationId`).
  - `ODataEndpointGroupsController` → `NotifyEndpointGroupMutationAsync` (Z. 93, 122, 165, 207).
  - Benachrichtigungsarten entsprechen damit exakt den REST-Pendants (`ApplicationsController`, `ApplicationGroupsController` → Tree; `EndpointsController`, `EndpointGroupsController` → entitätsspezifisch), die ebenfalls vollständig auf die Policy umgestellt wurden.
- **EndpointPage feuert keine eigene Nachricht mehr:** Die `@inject`-Zeilen für `IStorageModeService`/`ISignalRNotificationService` und die eigene `NotifyEndpointChangedAsync`-Prüfung wurden entfernt (`EndpointPage.razor` Z. 440–445); die Doppel-Benachrichtigung beim Speichern entfällt.
- **Transport unverändert:** `EndpointHub.cs`, `ISignalRNotificationService`, `SignalRNotificationService` haben keinen Diff zum Basisbranch (Hub, Gruppen, Ereignisnamen unverändert).
- **Einheitliche Anwendung in allen mutierenden API-Pfaden:** Zusätzlich nutzen `ApplicationImportController` (genau eine Tree-Benachrichtigung pro Apply, nur bei tatsächlichen Änderungen, Z. 150–158) und `SystemEndpointSyncService` (`NotifyBackgroundMutationAsync`, Z. 60–61) die Policy. REST-Subrouten für Endpunkt-Header/Query-Parameter (`EndpointsController` Z. 249, 281, 315, 347) benachrichtigen jetzt ebenfalls entitätsspezifisch — jeweils genau eine Nachricht pro Vorgang.
- **Testabdeckung:** `ODataNotificationIntegrationTests` (38 Tests: Team → genau eine passende Benachrichtigung, User → keine), `RestNotificationIntegrationTests`, `ImportApplyNotificationTests`, `NotificationPolicyTests` (8 Tests). Playwright-SignalR-Tests (`SignalRSyncTests.cs`, Infrastruktur) unverändert. Stichprobe: `dotnet test --filter "...~Notification"` → 57/57 grün.
- **Scope-Hinweis:** `EnvironmentManagementOverlay.razor` (Z. 133–134, 148–149) prüft weiterhin selbst `StorageMode.Team` vor `NotifyEnvironmentChangedAsync`. Umgebungen gehören nicht zu den vier genannten OData-Entity-Sets und es ist kein mutierender API-Pfad; als bewusst außerhalb des Schritt-Umfangs gewertet.
