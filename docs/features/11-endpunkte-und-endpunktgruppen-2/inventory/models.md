# Datenmodell

## `Endpoint`
Datei: `src/Schnittstellenzentrale.Core/Models/Endpoint.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename des Endpunkts |
| `Method` | `HttpMethod` (Enum) | HTTP-Methode; Teil des Identifikationsschlüssels `Method:RelativePath` |
| `RelativePath` | `string` | Relativer Pfad; Teil des Identifikationsschlüssels `Method:RelativePath` |
| `Body` | `string?` | Anfragekörper (manuell konfiguriert, soll beim Sync erhalten bleiben) |
| `BodyMode` | `BodyMode` (Enum) | Modus des Anfragekörpers |
| `AuthenticationType` | `AuthenticationType` (Enum) | Authentifizierungstyp (manuell konfiguriert, soll beim Sync erhalten bleiben) |
| `ApplicationId` | `int` | Fremdschlüssel zur zugehörigen `Application` |
| `Application` | `Application` | Navigationseigenschaft |
| `EndpointGroupId` | `int?` | Optionaler Fremdschlüssel zur `EndpointGroup` |
| `EndpointGroup` | `EndpointGroup?` | Navigationseigenschaft |
| `RowVersion` | `byte[]` | Optimistic-Concurrency-Token |
| `Headers` | `ICollection<EndpointHeader>` | Manuell konfigurierte Header (sollen beim Sync erhalten bleiben) |
| `QueryParameters` | `ICollection<EndpointQueryParameter>` | Manuell konfigurierte Query-Parameter (sollen beim Sync erhalten bleiben) |

## `ImportDiff`
Datei: `src/Schnittstellenzentrale.Core/Models/ImportDiff.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `NewEndpoints` | `IList<Endpoint>` | Endpunkte, die in der Swagger-Definition neu sind und in der Datenbank noch nicht existieren |
| `ChangedEndpoints` | `IList<Endpoint>` | Endpunkte, bei denen sich `Name`, `Body` oder `AuthenticationType` geändert haben; sollen vom `SystemEndpointSyncService` ignoriert werden |
| `RemovedEndpoints` | `IList<Endpoint>` | Endpunkte, die in der Datenbank existieren, aber nicht mehr in der Swagger-Definition vorhanden sind |
| `ErrorMessage` | `string?` | Fehlermeldung bei HTTP- oder Parse-Fehler; nicht-null signalisiert, dass der Diff nicht vollständig ist |

## `Application`
Datei: `src/Schnittstellenzentrale.Core/Models/Application.cs`

Relevante Eigenschaften für den Sync:

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `IsSystem` | `bool` | Markiert die Systemanwendung, die vom `SystemEndpointSyncService` gesucht wird |
| `InterfaceUrl` | `string?` | Wird durch `SystemEntryInitializer` auf `{Api:BaseUrl}/swagger/v1/swagger.json` gesetzt; ist die URL, die `SwaggerImportService` abruft |
| `InterfaceType` | `InterfaceType` (Enum) | Typ der Schnittstelle |
| `BaseUrl` | `string` | Basis-URL der Anwendung |
