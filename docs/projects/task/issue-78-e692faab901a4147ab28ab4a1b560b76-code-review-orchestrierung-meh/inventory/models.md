# Datenmodell und Contracts

## `ImportDiff`
Datei: `src/Schnittstellenzentrale.Core/Models/ImportDiff.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `NewEndpoints` | `IList<Endpoint>` | Neu erkannte Endpunkte (Z. 7) |
| `ChangedEndpoints` | `IList<Endpoint>` | Geänderte Endpunkte (Z. 10) |
| `RemovedEndpoints` | `IList<Endpoint>` | Entfernte Endpunkte (Z. 13) |
| `ErrorMessage` | `string?` | Fehlermeldung beim Import, optional (Z. 16) |
| `BearerTokens` | `IDictionary<string, string>` | Bearer-Tokens pro Endpunktschlüssel (`EndpointKeyHelper.BuildKey`) (Z. 20) |
| `WithBearerTokens(...)` | Methode → `ImportDiff` | Kopie mit ersetzten Tokens; trägt kein `ApplicationId`-Feld — Anwendungsbezug nur implizit über die enthaltenen Endpunkte (Z. 25–32) |

## `Endpoint`
Datei: `src/Schnittstellenzentrale.Core/Models/Endpoint.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel (Z. 9) |
| `Name` | `string` | Endpunktname (Z. 12) |
| `Method` | `Enums.HttpMethod` | HTTP-Methode (Z. 15) |
| `RelativePath` | `string` | Relativer Pfad (Z. 18) |
| `Body` | `string?` | Request-Body (Z. 21) |
| `BodyMode` | `BodyMode` | Body-Format (Z. 24) |
| `AuthenticationType` | `AuthenticationType` | Auth-Typ (Z. 27) |
| `ApplicationId` / `Application` | `int` / `Application?` | Aggregat-Anwendung (Z. 30–33) |
| `EndpointGroupId` / `EndpointGroup` | `int?` / `EndpointGroup?` | Optionale Gruppe (Z. 36–39) |
| `RowVersion` | `byte[]` | Optimistic-Concurrency-Token (Z. 42) |
| `PreRequestScript` / `PostRequestScript` | `string?` | Skripte (Z. 45, 48) |
| `Headers` | `ICollection<EndpointHeader>` | Kind-Aggregat (Z. 51) |
| `QueryParameters` | `ICollection<EndpointQueryParameter>` | Kind-Aggregat (Z. 54) |

## `EndpointHeader` / `EndpointQueryParameter`
Dateien: `src/Schnittstellenzentrale.Core/Models/EndpointHeader.cs`, `src/Schnittstellenzentrale.Core/Models/EndpointQueryParameter.cs`

Beide identisch aufgebaut: `Id` (int), `Key` (string), `Value` (string), `EndpointId` (int), `Endpoint` (Navigation, `[JsonIgnore]`). Cascade-Delete am `Endpoint` (AppDbContext Z. 126–133).

## `Application`
Datei: `src/Schnittstellenzentrale.Core/Models/Application.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Name |
| `IsSystem` | `bool` | System-Flag (schützt vor Update/Delete in Controllern) |
| `Description` / `BaseUrl` / `InterfaceUrl` | `string` / `string` / `string?` | Metadaten |
| `InterfaceType` | `InterfaceType` | `Unknown`/`Rest`/`OData`; steuert Import-Dispatch im `ApplicationImportController` |
| `Owner` | `string?` | Eigentümer (User-Modus-Filter) |
| `ApplicationGroupId` / `ApplicationGroup` | `int?` / Navigation | Gruppenzugehörigkeit; EF `OnDelete(SetNull)` |
| `Subtitle` / `IconData` | `string?` / `byte[]?` | Anzeige-Metadaten |
| `RowVersion` | `byte[]` | Concurrency-Token |
| `Endpoints` / `Links` / `EndpointGroups` | `ICollection<…>` | Kinder (alle `OnDelete(Cascade)`) |
| `DetectInterfaceType(url)` | static → `InterfaceType` | URL-Heuristik: `$metadata`→OData, `swagger`/`openapi`→Rest (Z. 59–66) |
| `Clone()` | → `Application` | Flache Kopie (Z. 70–81) |

## `ApplicationGroup`
Datei: `src/Schnittstellenzentrale.Core/Models/ApplicationGroup.cs`

`Id`, `Name`, `IsSystem`, `Description`, `Subtitle`, `IconData`, `RowVersion`, `Applications` (ICollection). `HasMany(Applications).OnDelete(SetNull)` — Gruppenlöschung setzt `ApplicationGroupId` der Kinder auf `null` (AppDbContext Z. 80–83).

## `EndpointGroup`
Datei: `src/Schnittstellenzentrale.Core/Models/EndpointGroup.cs`

`Id`, `Name`, `ApplicationId`, `Application` (required), `ParentGroupId`/`ParentGroup` (optional), `RowVersion`, `Endpoints` (`OnDelete(Cascade)`), `ChildGroups` (`OnDelete(Restrict)` — AppDbContext Z. 105–118).

## `UpdateEndpointRequest` (Contract)
Datei: `src/Schnittstellenzentrale.Core/Contracts/UpdateEndpointRequest.cs`

`Name` (Required, ≤200), `RelativePath` (Required, ≤500), `EndpointGroupId?`, `Method`, `BodyMode`, `Body?`, `AuthenticationType`, `PreRequestScript?`, `PostRequestScript?`, `RowVersion` (byte[]), `Headers` (`IList<UpdateEndpointKeyValueItem>`), `QueryParameters` (ebenso). Vollmengen-Ersetzung der Kinder ist bereits Teil des Contracts — keine Flags.

## `RequestContext` (Record)
Datei: `src/Schnittstellenzentrale/Controllers/ApiControllerBase.cs` (Z. 154)

`RequestContext(AuthToken NewToken, StorageMode StorageMode, string Owner)` — Rückgabe von `ApiControllerBase.ParseRequestContextAsync`.

## Weitere Contracts im Scope
Dateien unter `src/Schnittstellenzentrale.Core/Contracts/`: `CreateEndpointRequest`, `CreateEndpointGroupRequest`, `UpdateEndpointGroupRequest` (nur `Name` + `RowVersion`), `CreateApplicationGroupRequest`, `UpdateApplicationGroupRequest`, `AddEndpointKeyValueRequest` (`Key`, `Value`, `EndpointId`), `EndpointKeyValueResponse` (`Id`, `Key`, `Value`, `EndpointId`), `EndpointResponse` (enthält `Headers`/`QueryParameters` als `EndpointKeyValueResponse`-Listen).
