# Interfaces

## `IEndpointRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IEndpointRepository.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `GetEndpointsAsync` | `int applicationId` | `Task<IList<Endpoint>>` | Endpunkte einer Anwendung inkl. Kinder |
| `GetAllEndpointsAsync` | – | `Task<IList<Endpoint>>` | Alle Endpunkte |
| `GetEndpointsByApplicationIdsAsync` | `IEnumerable<int>` | `Task<IList<Endpoint>>` | Endpunkte mehrerer Anwendungen |
| `GetByGroupIdAsync` | `int endpointGroupId` | `Task<IList<Endpoint>>` | Endpunkte einer Gruppe |
| `GetEndpointByIdAsync` | `int id` | `Task<Endpoint?>` | Einzelner Endpunkt inkl. Kinder |
| `GetEndpointByNameAsync` | `int applicationId, string name` | `Task<IList<Endpoint>>` | Suche per Name |
| `AddEndpointAsync` | `Endpoint` | `Task<Endpoint>` | Anlegen |
| `UpdateEndpointAsync` | `Endpoint` | `Task<Endpoint>` | **Aggregat-Update** (Skalare + Vollmengen-Ersetzung der Kinder + Versionsprüfung) |
| `DeleteEndpointAsync` | `int id` | `Task` | Löschen |
| `GetEndpointGroupsAsync` / `GetAllEndpointGroupsAsync` / `GetEndpointGroupByIdAsync` | … | … | Gruppen-Lesen |
| `AddEndpointGroupAsync` / `UpdateEndpointGroupAsync` / `DeleteEndpointGroupAsync` | … | … | Gruppen-Schreiben |
| `GetHeaderByIdAsync` | `int id` | `Task<EndpointHeader?>` | Einzel-Header lesen |
| `AddHeaderAsync` | `EndpointHeader` | `Task<EndpointHeader>` | Einzel-Header anlegen |
| `DeleteHeaderAsync` | `int id` | `Task` | Einzel-Header löschen |
| `GetQueryParameterByIdAsync` | `int id` | `Task<EndpointQueryParameter?>` | Einzel-Parameter lesen |
| `AddQueryParameterAsync` | `EndpointQueryParameter` | `Task<EndpointQueryParameter>` | Einzel-Parameter anlegen |
| `DeleteQueryParameterAsync` | `int id` | `Task` | Einzel-Parameter löschen |

## `IApplicationApiClient` (relevante Methoden)
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IApplicationApiClient.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `GetEndpointByIdAsync` | `int id` | `Task<Endpoint?>` | Einzelner Endpunkt via REST |
| `AddEndpointAsync` | `Endpoint` | `Task<Endpoint>` | Anlegen via `POST /api/endpoints` |
| `UpdateEndpointAsync` | `Endpoint` | `Task<Endpoint>` | Update via `PUT /api/endpoints/{id}` inkl. Vollmengen-Kinder |
| `DeleteEndpointAsync` | `int id` | `Task` | Löschen |
| `AddHeaderAsync` | `EndpointHeader` | `Task<EndpointHeader>` | Einzel-Header via REST |
| `DeleteHeaderAsync` | `int id` | `Task` | Einzel-Header löschen via REST |
| `AddQueryParameterAsync` | `EndpointQueryParameter` | `Task<EndpointQueryParameter>` | Einzel-Parameter via REST |
| `DeleteQueryParameterAsync` | `int id` | `Task` | Einzel-Parameter löschen via REST |
| `ApplyODataDiffAsync` / `ApplySwaggerDiffAsync` | `int applicationId, ImportDiff` | `Task` | Import-Apply via REST-Endpunkte |

## Implementierungen

| Interface | Implementierung | Datei |
|-----------|-----------------|-------|
| `IEndpointRepository` | `EndpointRepository` | `src/Schnittstellenzentrale.Infrastructure/Repositories/EndpointRepository.cs` |
| `IApplicationApiClient` | `ApplicationApiClient` | `src/Schnittstellenzentrale/Services/ApplicationApiClient.cs` |
