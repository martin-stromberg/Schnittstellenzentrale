# Datenmodell und DTOs

## `Endpoint`
Datei: `src/Schnittstellenzentrale.Core/Models/Endpoint.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Name des Endpunkts |
| `Method` | `Enums.HttpMethod` | HTTP-Methode (Enum: `GET`, `POST`, `PUT`, `DELETE`, `PATCH`, `HEAD`, `OPTIONS`) |
| `RelativePath` | `string` | Relativer Pfad |
| `Body` | `string?` | Request-Body (optional) |
| `BodyMode` | `BodyMode` | Body-Format (`Json`, `Xml`, `PlainText`, `None` o. ä.) |
| `AuthenticationType` | `AuthenticationType` | Authentifizierungstyp |
| `ApplicationId` | `int` | FK zur Anwendung |
| `Application` | `Application?` | Navigation |
| `EndpointGroupId` | `int?` | FK zur Endpunktgruppe (optional) |
| `EndpointGroup` | `EndpointGroup?` | Navigation |
| `RowVersion` | `byte[]` | Concurrency-Token (Optimistic Concurrency, Default `[]`) |
| `PreRequestScript` | `string?` | Pre-Request-Skript |
| `PostRequestScript` | `string?` | Post-Request-Skript |
| `Headers` | `ICollection<EndpointHeader>` | Kind-Menge Header (Default `[]`) |
| `QueryParameters` | `ICollection<EndpointQueryParameter>` | Kind-Menge Query-Parameter (Default `[]`) |

## `EndpointHeader` / `EndpointQueryParameter`
Dateien: `src/Schnittstellenzentrale.Core/Models/EndpointHeader.cs`, `EndpointQueryParameter.cs`

Kind-Entitäten mit `Id`, `Key`, `Value`, `EndpointId` (FK) und Navigation `Endpoint`.

## `UpdateEndpointRequest`
Datei: `src/Schnittstellenzentrale.Core/Contracts/UpdateEndpointRequest.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Name` | `string` | `[Required]`, `[MaxLength(200)]` |
| `RelativePath` | `string` | `[Required]`, `[MaxLength(500)]` |
| `EndpointGroupId` | `int?` | Optional |
| `Method` | `Enums.HttpMethod` | Enum |
| `BodyMode` | `BodyMode` | Enum |
| `Body` | `string?` | Optional |
| `AuthenticationType` | `AuthenticationType` | Enum |
| `PreRequestScript` / `PostRequestScript` | `string?` | Optional |
| `RowVersion` | `byte[]` | Concurrency-Token |
| `Headers` | `IList<UpdateEndpointKeyValueItem>` | **Vollmenge** der Header (Default `[]`) |
| `QueryParameters` | `IList<UpdateEndpointKeyValueItem>` | **Vollmenge** der Query-Parameter (Default `[]`) |

## `UpdateEndpointKeyValueItem`
Datei: `src/Schnittstellenzentrale.Core/Contracts/UpdateEndpointKeyValueItem.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Key` | `string` | Schlüssel |
| `Value` | `string` | Wert |

## `CreateEndpointRequest`
Datei: `src/Schnittstellenzentrale.Core/Contracts/CreateEndpointRequest.cs`

Skalare inkl. `ApplicationId` — **ohne** `Headers`/`QueryParameters` (Kinder werden bei Neuanlage separat via `POST /api/endpoints/headers` bzw. `/query-parameters` angelegt).

## `EndpointResponse`
Datei: `src/Schnittstellenzentrale.Core/Contracts/EndpointResponse.cs`

Response-DTO inkl. `Headers`/`QueryParameters` als `IList<EndpointKeyValueResponse>`.

## `ImportDiff`
Datei: `src/Schnittstellenzentrale.Core/Models/ImportDiff.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `NewEndpoints` | `IList<Endpoint>` | Neu erkannte Endpunkte |
| `ChangedEndpoints` | `IList<Endpoint>` | Geänderte Endpunkte (tragen `Id` + `RowVersion` des Bestands, Skalare des Imports, Kinder des Bestands via `MergeExistingIdentity`) |
| `RemovedEndpoints` | `IList<Endpoint>` | Entfernte Endpunkte |
| `ErrorMessage` | `string?` | Fehler |
| `BearerTokens` | `IDictionary<string, string>` | Bearer-Tokens pro Endpunkt-Schlüssel |
