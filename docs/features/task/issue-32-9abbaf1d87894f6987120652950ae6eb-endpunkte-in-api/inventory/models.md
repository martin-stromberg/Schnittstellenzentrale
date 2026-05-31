# Datenmodell

## `Endpoint`
Datei: `src/Schnittstellenzentrale.Core/Models/Endpoint.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Bezeichnung des Endpunkts |
| `Method` | `HttpMethod` (Enum) | HTTP-Methode (GET, POST, PUT, DELETE, …) |
| `RelativePath` | `string` | Relativer Pfad (z. B. `/api/items`) |
| `Body` | `string?` | Request-Body |
| `BodyMode` | `BodyMode` (Enum) | Steuert Body-Format und Content-Type |
| `AuthenticationType` | `AuthenticationType` (Enum) | Autorisierungstyp (None, Basic, Negotiate, BearerToken, …) |
| `ApplicationId` | `int` | Fremdschlüssel zur zugehörigen `Application` |
| `Application` | `Application` | Navigationseigenschaft zur Anwendung |
| `EndpointGroupId` | `int?` | Optionaler Fremdschlüssel zur `EndpointGroup` |
| `EndpointGroup` | `EndpointGroup?` | Navigationseigenschaft zur Endpunktgruppe |
| `RowVersion` | `byte[]` | Optimistic-Concurrency-Token |
| `PreRequestScript` | `string?` | Skript, das vor dem Request ausgeführt wird |
| `PostRequestScript` | `string?` | Skript, das nach dem Request ausgeführt wird |
| `Headers` | `ICollection<EndpointHeader>` | Liste der Request-Header |
| `QueryParameters` | `ICollection<EndpointQueryParameter>` | Liste der Query-Parameter |

---

## `EndpointGroup`
Datei: `src/Schnittstellenzentrale.Core/Models/EndpointGroup.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Bezeichnung der Gruppe |
| `ApplicationId` | `int` | Fremdschlüssel zur zugehörigen `Application` |
| `Application` | `Application` | Navigationseigenschaft zur Anwendung |
| `ParentGroupId` | `int?` | Optionaler Fremdschlüssel zur übergeordneten Gruppe (hierarchische Struktur) |
| `ParentGroup` | `EndpointGroup?` | Navigationseigenschaft zur übergeordneten Gruppe |
| `RowVersion` | `byte[]` | Optimistic-Concurrency-Token |
| `Endpoints` | `ICollection<Endpoint>` | Endpunkte in dieser Gruppe |
| `ChildGroups` | `ICollection<EndpointGroup>` | Untergruppen |

---

## `EndpointHeader`
Datei: `src/Schnittstellenzentrale.Core/Models/EndpointHeader.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Key` | `string` | Header-Name |
| `Value` | `string` | Header-Wert |
| `EndpointId` | `int` | Fremdschlüssel zum Endpunkt |
| `Endpoint` | `Endpoint` | Navigationseigenschaft |

---

## `EndpointQueryParameter`
Datei: `src/Schnittstellenzentrale.Core/Models/EndpointQueryParameter.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Key` | `string` | Parameter-Name |
| `Value` | `string` | Parameter-Wert |
| `EndpointId` | `int` | Fremdschlüssel zum Endpunkt |
| `Endpoint` | `Endpoint` | Navigationseigenschaft |

---

## Vorhandene Contracts (DTOs) – Referenzmuster

Die folgenden Contracts existieren bereits und dienen als Vorlage für die neuen `EndpointGroupResponse`, `EndpointResponse` usw.:

### `ApplicationGroupResponse`
Datei: `src/Schnittstellenzentrale.Core/Contracts/ApplicationGroupResponse.cs`

| Eigenschaft | Typ |
|---|---|
| `Id` | `int` |
| `Name` | `string` |
| `IsSystem` | `bool` |
| `Description` | `string?` |
| `Subtitle` | `string?` |
| `IconData` | `byte[]?` |
| `RowVersion` | `byte[]` |
| `Applications` | `IList<ApplicationResponse>` |

### `ApplicationResponse`
Datei: `src/Schnittstellenzentrale.Core/Contracts/ApplicationResponse.cs`

| Eigenschaft | Typ |
|---|---|
| `Id` | `int` |
| `Name` | `string` |
| `IsSystem` | `bool` |
| `BaseUrl` | `string` |
| `ApplicationGroupId` | `int?` |
| `Description` | `string` |
| `InterfaceUrl` | `string?` |
| `InterfaceType` | `int` |
| `Owner` | `string?` |
| `Subtitle` | `string?` |
| `IconData` | `byte[]?` |
| `RowVersion` | `byte[]` |

### `CreateApplicationGroupRequest` / `UpdateApplicationGroupRequest`
Datei: `src/Schnittstellenzentrale.Core/Contracts/`

Beide enthalten jeweils nur `Name` (`string`).

### `CreateApplicationRequest` / `UpdateApplicationRequest`
Datei: `src/Schnittstellenzentrale.Core/Contracts/`

Enthalten `Name`, `BaseUrl`, `Description`, `InterfaceUrl`, `ApplicationGroupId`, `Owner`.

---

## Fehlende Contracts (müssen neu erstellt werden)

- `EndpointGroupResponse`
- `EndpointResponse`
- `CreateEndpointGroupRequest`
- `UpdateEndpointGroupRequest`
- `CreateEndpointRequest`
- `UpdateEndpointRequest`
