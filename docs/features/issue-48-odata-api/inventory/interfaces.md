# Interfaces

## `IODataImportService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IODataImportService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `ImportAsync` | `Application application` | `Task<ImportDiff>` | Ruft CSDL-Metadaten ab, parst sie und berechnet den Diff zum Bestand |
| `ApplyDiffAsync` | `ImportDiff diff` | `Task` | Schreibt die Endpunkte aus dem Diff in das Repository |

Implementiert durch: `ODataImportService` (Infrastructure).

---

## `ISwaggerImportService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ISwaggerImportService.cs`

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `ImportAsync` | `Application application` | `Task<ImportDiff>` | Ruft Swagger-Definition ab, parst sie und berechnet den Diff |
| `ApplyDiffAsync` | `ImportDiff diff` | `Task` | Schreibt Endpunkte aus dem Diff in das Repository (mit Gruppen-Auflösung) |

Referenz-Pattern für `IODataImportService`; die Signaturen sind identisch.

---

## `IEndpointRepository`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/IEndpointRepository.cs`

Wird von `ODataImportService` verwendet (über `GetEndpointsAsync`, `AddEndpointAsync`, `UpdateEndpointAsync`, `DeleteEndpointAsync`).

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `GetEndpointsAsync` | `int applicationId` | `Task<IList<Endpoint>>` | Alle Endpunkte einer Anwendung — Basis für Diff-Berechnung |
| `GetEndpointsByApplicationIdsAsync` | `IEnumerable<int> applicationIds` | `Task<IList<Endpoint>>` | Endpunkte mehrerer Anwendungen |
| `GetByGroupIdAsync` | `int endpointGroupId` | `Task<IList<Endpoint>>` | Endpunkte einer Gruppe |
| `GetEndpointByIdAsync` | `int id` | `Task<Endpoint?>` | Einzelner Endpunkt per ID |
| `GetEndpointByNameAsync` | `int applicationId, string name` | `Task<IList<Endpoint>>` | Endpunkte nach Name |
| `AddEndpointAsync` | `Endpoint endpoint` | `Task<Endpoint>` | Neuen Endpunkt anlegen |
| `UpdateEndpointAsync` | `Endpoint endpoint` | `Task<Endpoint>` | Bestehenden Endpunkt aktualisieren |
| `DeleteEndpointAsync` | `int id` | `Task` | Endpunkt löschen |
| `GetEndpointGroupsAsync` | `int applicationId` | `Task<IList<EndpointGroup>>` | Gruppen einer Anwendung |
| `GetAllEndpointGroupsAsync` | — | `Task<IList<EndpointGroup>>` | Alle Gruppen |
| `GetEndpointGroupByIdAsync` | `int id` | `Task<EndpointGroup?>` | Gruppe per ID |
| `AddEndpointGroupAsync` | `EndpointGroup group` | `Task<EndpointGroup>` | Gruppe anlegen |
| `UpdateEndpointGroupAsync` | `EndpointGroup group` | `Task<EndpointGroup>` | Gruppe aktualisieren |
| `DeleteEndpointGroupAsync` | `int id` | `Task` | Gruppe löschen |
| `GetHeaderByIdAsync` | `int id` | `Task<EndpointHeader?>` | Header per ID |
| `AddHeaderAsync` | `EndpointHeader header` | `Task<EndpointHeader>` | Header anlegen |
| `DeleteHeaderAsync` | `int id` | `Task` | Header löschen |
| `GetQueryParameterByIdAsync` | `int id` | `Task<EndpointQueryParameter?>` | Query-Parameter per ID |
| `AddQueryParameterAsync` | `EndpointQueryParameter parameter` | `Task<EndpointQueryParameter>` | Query-Parameter anlegen |
| `DeleteQueryParameterAsync` | `int id` | `Task` | Query-Parameter löschen |
