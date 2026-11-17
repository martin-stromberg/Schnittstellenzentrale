# Bestandsaufnahme: Logik / Abläufe

## `ApplicationGroupsController`
Datei: `src/Schnittstellenzentrale/Controllers/ApplicationGroupsController.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `GetAllAsync` | public | `GET /api/application-groups` — benötigt `X-Storage-Mode`+`X-Owner` (`RequiresContextHeaders(includeOwner: true)`) |
| `GetByIdAsync` | public | `GET /api/application-groups/{id}` — 404 bei fehlender Gruppe |
| `CreateAsync` | public | `POST` — 201; `NotifyTreeChangedAsync` bei `StorageMode.Team` |
| `UpdateAsync` | public | `PUT {id}` — 404/403 (IsSystem)/200; `NotifyTreeChangedAsync` bei Team |
| `DeleteAsync(int id)` | public | `DELETE {id}` — Tokenprüfung → `ParseStorageMode` → `GetGroupByIdAsync` → 404 → `IsSystem` → 403 → `_applicationRepository.DeleteGroupAsync(id)` → `NotifyTreeChangedAsync` bei Team → 204 |

Publizierte Events: `ISignalRNotificationService.NotifyTreeChangedAsync()` (nur Team-Modus) bei Create/Update/Delete.

## `ApplicationRepository`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/ApplicationRepository.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `GetGroupsAsync(storageMode, owner)` | public | lädt Gruppen inkl. `Applications`; im User-Modus nur Gruppen mit eigenen Anwendungen |
| `GetGroupByIdAsync(id)` | public | `Include(g => g.Applications)` |
| `GetSystemGroupAsync` | public | erste Gruppe mit `IsSystem` |
| `AddGroupAsync` / `UpdateGroupAsync` | public | Insert / Update mit RowVersion-Concurrency |
| `DeleteGroupAsync(int id)` | public | `FindAsync` → `Remove` → `SaveChanges`; Mitglieder werden per DB-`SetNull`-Regel ungruppiert — **keine explizite Transaktion, keine Moduswahl** |
| `DeleteApplicationAsync(int id)` | public | `FindAsync` → `Remove` → `SaveChanges`; Endpunkte/Endpunktgruppen kaskadieren per DB |
| `GetApplicationCountByGroupAsync` / `GetEndpointCountByGroupAsync` | public | Zähler für Dialog-Anzeige |
| `ApplyOwnerFilter` | private static | Owner-Filter für `StorageMode.User` |

Contexts werden pro Aufruf über `IDbContextFactory<AppDbContext>` erzeugt (`await using`).

## `ApplicationApiClient`
Datei: `src/Schnittstellenzentrale/Services/ApplicationApiClient.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `DeleteGroupAsync(int id)` | public | DELETE `{base}/api/application-groups/{id}` via `BuildDeleteRequest` + `SendWithTokenNoContentAsync` (Token-Rotation über `ExecuteWithTokenAsync`) |
| `GetBaseUrl` | private | `Api:BaseUrl` aus Konfiguration, sonst HttpContext, sonst `https://localhost:5001` |
| `BuildDeleteRequest(relativeUrl, storageMode, token)` | private static | baut DELETE-Request mit Bearer-Token und `X-Storage-Mode` |
| `BuildGetRequest(...)` | private static | unterstützt `queryParams`-Dictionary (Vorbild für Query-Parameter-Bau) |

## `WorkspacesLayout` (UI-Kontext — Umstellung erst in Schritt 4!)
Datei: `src/Schnittstellenzentrale/Components/Layout/WorkspacesLayout.razor`

| Methode | Beschreibung |
|---------|-------------|
| `OnDeleteGroupRequested` (Z. 164) | öffnet `ConfirmDeleteGroupDialog` |
| `OnDeleteGroupConfirmedAll` (Z. 217–231) | **Mehrschritt:** `GetGroupByIdAsync` → je Anwendung `DeleteApplicationAsync` → `DeleteGroupAsync` — nicht atomar |
| `OnDeleteGroupConfirmedGroupOnly` (Z. 233–248) | **Mehrschritt:** je Anwendung `ApplicationGroupId=null` + `UpdateApplicationAsync` → `DeleteGroupAsync` — nicht atomar |

## `ODataApplicationGroupsController`
Datei: `src/Schnittstellenzentrale/OData/ODataApplicationGroupsController.cs` (Z. 163)
`DeleteAsync` ruft ebenfalls `_applicationRepository.DeleteGroupAsync(key)` — Signaturänderung des Repositorys muss kompatibel bleiben oder die Aufrufstelle angepasst werden.

## Weitere beteiligte Artefakte

- `ISignalRNotificationService.NotifyTreeChangedAsync()` (`Core/Interfaces`) — einzige Benachrichtigung bei Strukturänderungen.
- `TestHelpers.CreateInMemoryDbContext()` (`Tests/Helpers`) — SQLite-In-Memory-Factory für Repository-Tests (`EnsureCreated` → DB-seitige FK-Aktionen aktiv).
- `ControllerTestFactory` — `WebApplicationFactory<Program>` mit `TestAuthHandler`, SQLite-In-Memory, gemocktem `ISignalRNotificationService`, `ObtainTokenAsync`-Helfer.
