# Umsetzungsplan – Workspace-Oberfläche auf API-Pfad umstellen (Issue #89)

## Ziel

`WorkspacesLayout.razor` nutzt für alle Endpunkt-/Endpunktgruppen-Operationen und das Gruppen-Löschen mit Inhalt ausschließlich `IApplicationApiClient`. Direkte `IEndpointRepository`-Zugriffe und komponenteneigene Notify-Entscheidungen (`IStorageModeService`, `ISignalRNotificationService`) entfallen. Aktivitätsprotokoll, Fehleranzeige und Selektions-/Refresh-Logik bleiben erhalten.

## Scope-Klärung

### Scope-Ausschlüsse

| Ausschluss | Begründung | Status |
|------------|------------|--------|
| `CollectionContentView.razor` (reiner Repository-Lesezugriff) | Von der Anforderung explizit ausgenommen | bestätigt (Anforderungstext) |
| Weitere UI-Komponenten außerhalb `WorkspacesLayout` | Nicht Gegenstand der Anforderung | bestätigt (Anforderungstext) |
| Neue API-Endpunkte oder Client-Erweiterungen | Bereits vorhanden (Schritt 3 + bestehender Client) | bestätigt (Anforderungstext) |

## Arbeitspakete

### WP1: Injektionen und Gruppen-Löschpfad

- `@inject IEndpointRepository EndpointRepository`, `@inject IStorageModeService StorageModeService`, `@inject ISignalRNotificationService SignalRNotificationService` entfernen.
- `OnDeleteGroupConfirmedAll` → ein Aufruf `ApplicationApiClient.DeleteGroupAsync(group.Id, ApplicationGroupDeleteMode.Delete)`; `GetGroupByIdAsync`-Vorabladen und Einzel-`DeleteApplicationAsync`-Schleife entfallen.
- `OnDeleteGroupConfirmedGroupOnly` → `ApplicationApiClient.DeleteGroupAsync(group.Id, ApplicationGroupDeleteMode.Detach)`; `UpdateApplicationAsync`-Schleife entfällt.
- Selektions-Rücksetzung (`SetWorkspaceSelectionAsync(null)`) und `RefreshTreeAsync` bleiben; `_errorMessage`-Mapping unverändert.

### WP2: Endpunktgruppen-Operationen

- `OnCreateEndpointGroupConfirmed`: `EndpointRepository.AddEndpointGroupAsync` → `ApplicationApiClient.AddEndpointGroupAsync`; Team-Notify-Block entfällt; `ActivityLogService.Log` bleibt.
- `OnEndpointGroupRenamed`: → `ApplicationApiClient.UpdateEndpointGroupAsync`; Notify entfällt; Log bleibt.
- `HandleDeleteEndpointGroupRequested` (Zählabfrage): `EndpointRepository.GetEndpointsAsync`/`GetEndpointGroupsAsync` → gleichnamige Client-Methoden.
- `OnEndpointGroupDeleteConfirmed`: `EndpointRepository.DeleteEndpointGroupAsync` → Client; Notify entfällt; Selektions-Rücksetzung für betroffenen selektierten Endpunkt bleibt.

### WP3: Endpunkt-Operationen

- `HandleCreateEndpointRequested`: `AddEndpointAsync` → Client; Notify entfällt; `GetEndpointByIdAsync` → Client; Pfadrekonstruktion ohne Nav-Objekte:
  - `Application` via `GetApplicationByIdAsync(loaded.ApplicationId)`
  - `ApplicationGroup` via `GetGroupByIdAsync(app.ApplicationGroupId.Value)` (nur wenn gesetzt)
  - `EndpointGroup` via `GetEndpointGroupByIdAsync(loaded.EndpointGroupId.Value)` (nur wenn gesetzt)
  - Pfad `[ApplicationGroup?, Application, EndpointGroup?, Endpoint]` wie bisher (nur direkte Parent-Gruppe).
- `HandleDeleteEndpointRequested`: `EndpointRepository.DeleteEndpointAsync` → `ApplicationApiClient.DeleteEndpointAsync`; Notify entfällt; JS-`confirm` und Selektions-Rücksetzung bleiben.

### WP4: Verifikation

- `dotnet build` — 0 Fehler.
- `dotnet test --filter "FullyQualifiedName!~Playwright"` — grün.
- `dotnet test --filter "FullyQualifiedName~Playwright"` (Gruppen-CRUD, Endpunktgruppen-CRUD, SignalR, Layout) — grün.
- Manuelle Sichtprüfung: kein `IEndpointRepository`/`StorageModeService`/`SignalRNotificationService` mehr in `WorkspacesLayout.razor`; `CollectionContentView.razor` unverändert.

## Akzeptanzszenarien

1. **Endpunktgruppe anlegen (Happy Path):** Nutzer wählt im Baum „Ordner anlegen" → `CreateEndpointGroupDialog` → bestätigt mit Namen → Ordner erscheint im Baum, Aktivitätsprotokoll enthält Eintrag. Ausführungspfad: `POST /api/endpoint-groups` (Kontext-Header, Token-Rotation, Team-Notify im Controller).
2. **Endpunktgruppe umbenennen:** Rename-Dialog → Speichern → neuer Name im Baum; im Team-Modus genau eine SignalR-Benachrichtigung (aus dem Controller).
3. **Endpunktgruppe mit Inhalt löschen:** Lösch-Dialog zeigt korrekte Endpunktzahl (Zählabfrage via API) → bestätigen → Gruppe samt Inhalt weg; war einer der Endpunkte selektiert, wird die Selektion zurückgesetzt.
4. **Endpunkt anlegen:** „Endpunkt anlegen" → neuer Endpunkt erscheint und ist selektiert (Pfad `[Gruppe?, Anwendung, Ordner?, Endpunkt]` korrekt aufgebaut, inkl. ungruppierter Anwendung und wurzelhängendem Endpunkt ohne Ordner).
5. **Endpunkt löschen:** JS-Confirm → bestätigen → Endpunkt weg, Selektion zurückgesetzt falls er selektiert war.
6. **Anwendungsgruppe „Alles löschen":** ConfirmDeleteGroupDialog → „Alles löschen" → `DELETE /api/application-groups/{id}?content=delete` → Gruppe und Anwendungen weg, Baum aktualisiert.
7. **Anwendungsgruppe „Nur Gruppe löschen":** → `?content=detach` → Gruppe weg, Anwendungen in ungruppierter Menge sichtbar.
8. **Fehlerfall:** API liefert 404/400 → `_errorMessage` zeigt lokalisierte Fehlermeldung (bestehende resx-Keys).

## Offene Punkte

Keine.

## Risiken

- Pfadrekonstruktion ohne Nav-Objekte: zusätzliche GET-Aufrufe nötig; Funktionalität durch Playwright-Tests (Selektion nach Anlage in `NavigationTests`/`EndpointGroupCrudTests`) abgedeckt.
- `HttpRequestException`-Messages statt Repository-Exceptions: Fehlertexte ändern sich geringfügig, Anzeigepfad bleibt.
