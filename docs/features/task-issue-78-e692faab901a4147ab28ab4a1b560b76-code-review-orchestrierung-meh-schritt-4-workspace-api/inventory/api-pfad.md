# Detail: API-Pfad – Fakten zu Controllern und Client

## Endpunktgruppen (`api/endpoint-groups`, `EndpointGroupsController.cs`)

| Methode | Route | Nebenbedingungen |
|---------|-------|-------------------|
| GET Liste | `?applicationId=` | `[RequiresContextHeaders]`, 401 ohne Token |
| GET einzeln | `/{id}` | 404 wenn nicht gefunden |
| POST | Body `CreateEndpointGroupRequest` (Name, ApplicationId, ParentGroupId) | 201; Notify `NotifyEndpointGroupChangedAsync` bei `StorageMode.Team` |
| PUT `/{id}` | Body `UpdateEndpointGroupRequest` (Name, RowVersion) | ApplicationId/ParentGroupId aus Bestand; 404; Notify bei Team |
| DELETE `/{id}` | – | 404 bei fehlender Gruppe; EF-Kaskade für Kinder; Notify bei Team |

## Endpunkte (`api/endpoints`, `EndpointsController.cs`)

| Methode | Route | Nebenbedingungen |
|---------|-------|-------------------|
| GET Liste | `?applicationId=&groupId=` | groupId-Prüfung gegen applicationId → 404 |
| GET einzeln | `/{id}` | 404 |
| POST | Body `CreateEndpointRequest` | 201; Notify `NotifyEndpointChangedAsync` bei Team |
| PUT `/{id}` | Body `UpdateEndpointRequest` (inkl. Headers/QueryParameters) | 404; Aggregat-Ersetzung; Notify bei Team |
| DELETE `/{id}` | – | 404; Notify bei Team |

## Anwendungsgruppen-Löschen (`api/application-groups/{id}?content=…`, Schritt 3)

- `content=delete`: enthaltene Anwendungen samt Endpunkten/Endpunktgruppen löschen.
- `content=detach` oder fehlend: Anwendungen in die ungruppierte Menge umhängen.
- Ungültiger Wert → 400; fehlende Gruppe → 404; Systemgruppe → 403.
- Serverseitig atomar (`ApplicationRepository.DeleteGroupAsync(id, mode)`, transaktional inkl. Rollback); Notify `NotifyTreeChangedAsync` bei Team-Modus.
- Client: `IApplicationApiClient.DeleteGroupAsync(int id, ApplicationGroupDeleteMode mode)` serialisiert `mode.ToString().ToLowerInvariant()` als `content`.

## Kontext- und Token-Mechanik

- `[RequiresContextHeaders]`: verlangt `X-Storage-Mode` (und für Listen-endpunkte teils `X-Owner`); `ParseRequestContextAsync` rotiert das Bearer-Token (`X-New-Token`-Response-Header).
- `ApplicationApiClient` setzt Header automatisch aus `IStorageModeService`/`ICurrentUserService`/`ITokenStore` — der Blazor-Aufrufer muss nichts übergeben.

## Semantik-Abgleich Ist ↔ Soll

- Notify: identische Bedingung (`StorageMode == Team`), nur einmal pro Mutation — früher UI-seitig nach Repo-Aufruf, jetzt im Controller nach Repo-Aufruf.
- Fehler: Repo → direkte Exception; API → `HttpRequestException` nach `EnsureSuccessStatusCode`; beide landen in `catch (Exception)` → `_errorMessage`.
- `GetEndpointsAsync(applicationId)`/`GetEndpointGroupsAsync(applicationId)`: Controller liefern flache Listen ohne Nav-Objekte — für `CountEndpointsRecursive`/`CollectGroupIdsRecursive` ausreichend (nur `Id`, `EndpointGroupId`, `ParentGroupId`, `ApplicationId` nötig; alle im Response-DTO).
