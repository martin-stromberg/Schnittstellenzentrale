# Übersetzte Anforderung – Workspace-Oberfläche vollständig auf den API-Pfad umstellen (Issue #89)

## Fachliche Zusammenfassung

Der Workspace-Bereich der Blazor-Oberfläche (`WorkspacesLayout.razor`) soll für sämtliche Endpunkt- und Endpunktgruppen-Operationen (Anlegen, Umbenennen, Löschen sowie die zugehörigen Bestätigungs- und Zählabfragen) ausschließlich über den bestehenden `IApplicationApiClient` gehen — also über den REST-Controller-Pfad mit Kontext-Headern (`X-Storage-Mode`, `X-Owner`, Bearer-Token), Team-Benachrichtigung und konsistenten Nebenbedingungen. Die direkte Nutzung von `IEndpointRepository` in dieser Komponente entfällt vollständig; ebenso entfallen die komponenteneigenen Entscheidungen, ob eine SignalR-Änderungsbenachrichtigung auszulösen ist — diese Politik liegt einheitlich im Controller-Pfad.

Zusätzlich wird das Löschen einer Anwendungsgruppe mit Inhalt über den in Schritt 3 geschaffenen Gruppen-Lösch-Endpunkt `DELETE /api/application-groups/{id}?content=delete|detach` abgewickelt: Beide Bestätigungsvarianten des Löschdialogs rufen denselben Endpunkt mit unterschiedlichem Parameterwert auf; die bisherigen mehrstufigen UI-Abläufe (Anwendungen einzeln löschen bzw. einzeln umhängen, dann Gruppe löschen) entfallen.

Das UI-seitige Aktivitätsprotokoll (`IActivityLogService`) für Ordner-/Endpunkt-Aktionen bleibt als UI-Eigenheit erhalten. Entfernt werden ausschließlich die direkten Repository-Zugriffe und die redundanten Benachrichtigungsentscheidungen. Reine Lesezugriffe anderer Komponenten auf Repositories (z. B. `CollectionContentView`) bleiben unberührt.

## Fachlicher Scope

### Verarbeitungseinheit

- Ein Endpunkt- bzw. Endpunktgruppen-CRUD-Vorgang aus der Workspace-Oberfläche (Create/Rename/Delete inkl. Vorbereitungs-Zählabfragen).
- Ein Gruppen-Löschvorgang mit Inhalt aus dem `ConfirmDeleteGroupDialog` (beide Varianten: „Alles löschen" = `content=delete`, „Nur Gruppe löschen" = `content=detach`).

### Auslösende Ereignisse

- Nutzeraktionen in `WorkspacesLayout`: Endpunktgruppe anlegen/umbenennen/löschen, Endpunkt anlegen/löschen, Anwendungsgruppe löschen (beide Dialogvarianten), Lösch-Zählabfrage für den `ConfirmDeleteEndpointGroupDialog`.

### Erwartete Wirkung

- Es existiert nur noch ein produktiver Ausführungspfad für Endpunkt-/Endpunktgruppen-CRUD aus der UI — mit einheitlichen Nebenbedingungen (Kontext-Header, Team-Notify) und ohne Doppelbenachrichtigungen.
- `WorkspacesLayout` injiziert weder `IEndpointRepository` noch `IStorageModeService`/`ISignalRNotificationService` für Notify-Entscheidungen.
- Gruppen-Löschung mit Inhalt läuft als ein API-Aufruf (atomar serverseitig) statt als mehrstufige UI-Komposition.

### Umgang mit fehlenden Objekten

- Gelöschte/nicht gefundene Endpunktgruppen oder Endpunkte: der Controller-Pfad liefert 404 → der Client wirft eine Exception, die wie bisher über `_errorMessage` im UI angezeigt wird.
- Nach dem Anlegen eines Endpunkts wird der Baum-Pfad zur Auswahl über API-Abfragen rekonstruiert (`GetEndpointByIdAsync`, `GetApplicationByIdAsync`, `GetGroupByIdAsync`, `GetEndpointGroupByIdAsync`), da die API-Antwort keine Navigationsobjekte enthält.

### Scope-Ausschlüsse

| Ausschluss | Begründung | Status |
|------------|------------|--------|
| `CollectionContentView.razor` (reiner Lesezugriff via `EndpointRepository.GetEndpointsByApplicationIdsAsync`) | Explizit von der Anforderung ausgenommen („Reine Lesezugriffe anderer Komponenten … bleiben unberührt") | bestätigt (Anforderungstext) |
| EndpointPage / andere Komponenten außerhalb `WorkspacesLayout` | Nicht Gegenstand der Anforderung | bestätigt (Anforderungstext) |
| Keine neuen API-Endpunkte / keine Client-Erweiterungen | Der `content`-Parameter-Endpunkt und `DeleteGroupAsync(id, mode)` existieren bereits (Schritt 3) | bestätigt (Anforderungstext) |

## Akzeptanzkriterien

1. `WorkspacesLayout.razor` enthält kein `@inject IEndpointRepository`, kein `@inject ISignalRNotificationService`, kein `@inject IStorageModeService` mehr; sämtliche Endpunkt-/Endpunktgruppen-Operationen laufen über `ApplicationApiClient`.
2. `OnDeleteGroupConfirmedAll` ruft `DeleteGroupAsync(id, ApplicationGroupDeleteMode.Delete)`; `OnDeleteGroupConfirmedGroupOnly` ruft `DeleteGroupAsync(id, ApplicationGroupDeleteMode.Detach)` — ohne vorherige Einzellöschung/-umhängung der Anwendungen.
3. Die Zählabfrage für den Löschdialog (`HandleDeleteEndpointGroupRequested`) nutzt `GetEndpointsAsync`/`GetEndpointGroupsAsync` des API-Clients.
4. Aktivitätsprotokoll-Einträge (`ActivityLogService.Log` für Ordner/Endpunkt-Anlage und -Umbenennung) bleiben erhalten.
5. Fehlerbehandlung (`_errorMessage` über resx-Keys) und Baum-Refresh/Selektionslogik bleiben funktional gleichwertig.
6. Build grün; bestehende Unit-, Komponenten-, Integrations- und Playwright-Tests (Gruppen-CRUD, Endpunktgruppen-CRUD, SignalR-Synchronisation, Layout-Prüfung) bleiben ohne neue Tests grün; bestehende Tests werden bei Bedarf an die geänderten Abhängigkeiten angepasst.
