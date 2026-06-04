# Datenmodell

## `UpdateApplicationRequest`
Datei: `src/Schnittstellenzentrale.Core/Contracts/UpdateApplicationRequest.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Name` | `string` | `[Required]`, `[MaxLength(200)]` — kein `ErrorMessage`-Parameter |
| `BaseUrl` | `string` | `[Required]`, `[MaxLength(500)]` — kein `ErrorMessage`-Parameter |
| `Description` | `string?` | Kein Validierungsattribut |
| `InterfaceUrl` | `string?` | `[MaxLength(500)]` — kein `ErrorMessage`-Parameter |
| `ApplicationGroupId` | `int?` | Kein Validierungsattribut |
| `Owner` | `string?` | `[MaxLength(256)]` — kein `ErrorMessage`-Parameter |

## `CreateApplicationRequest`
Datei: `src/Schnittstellenzentrale.Core/Contracts/CreateApplicationRequest.cs`

Erbt von `UpdateApplicationRequest`, fügt keine eigenen Eigenschaften hinzu.

## `CreateApplicationGroupRequest`
Datei: `src/Schnittstellenzentrale.Core/Contracts/CreateApplicationGroupRequest.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Name` | `string` | `[Required]`, `[MaxLength(200)]` — kein `ErrorMessage`-Parameter |

## `UpdateApplicationGroupRequest`
Datei: `src/Schnittstellenzentrale.Core/Contracts/UpdateApplicationGroupRequest.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Name` | `string` | `[Required]`, `[MaxLength(200)]` — kein `ErrorMessage`-Parameter |

## `CreateEndpointGroupRequest`
Datei: `src/Schnittstellenzentrale.Core/Contracts/CreateEndpointGroupRequest.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Name` | `string` | `[Required]`, `[MaxLength(200)]` — kein `ErrorMessage`-Parameter |
| `ApplicationId` | `int` | `[Range(1, int.MaxValue)]` — kein `ErrorMessage`-Parameter |
| `ParentGroupId` | `int?` | Kein Validierungsattribut |

## `UpdateEndpointGroupRequest`
Datei: `src/Schnittstellenzentrale.Core/Contracts/UpdateEndpointGroupRequest.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Name` | `string` | `[Required]`, `[MaxLength(200)]` — kein `ErrorMessage`-Parameter |
| `RowVersion` | `byte[]` | Kein Validierungsattribut |

## `UpdateEndpointRequest`
Datei: `src/Schnittstellenzentrale.Core/Contracts/UpdateEndpointRequest.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Name` | `string` | `[Required]`, `[MaxLength(200)]` — kein `ErrorMessage`-Parameter |
| `RelativePath` | `string` | `[Required]`, `[MaxLength(500)]` — kein `ErrorMessage`-Parameter |
| `EndpointGroupId` | `int?` | Kein Validierungsattribut |
| `Method` | `HttpMethod` | Kein Validierungsattribut |
| `BodyMode` | `BodyMode` | Kein Validierungsattribut |
| `Body` | `string?` | Kein Validierungsattribut |
| `AuthenticationType` | `AuthenticationType` | Kein Validierungsattribut |
| `PreRequestScript` | `string?` | Kein Validierungsattribut |
| `PostRequestScript` | `string?` | Kein Validierungsattribut |
| `RowVersion` | `byte[]` | Kein Validierungsattribut |

## `CreateEndpointRequest`
Datei: `src/Schnittstellenzentrale.Core/Contracts/CreateEndpointRequest.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Name` | `string` | `[Required]`, `[MaxLength(200)]` — kein `ErrorMessage`-Parameter |
| `RelativePath` | `string` | `[Required]`, `[MaxLength(500)]` — kein `ErrorMessage`-Parameter |
| `ApplicationId` | `int` | `[Range(1, int.MaxValue)]` — kein `ErrorMessage`-Parameter |
| `EndpointGroupId` | `int?` | Kein Validierungsattribut |
| `Method` | `HttpMethod` | Kein Validierungsattribut |
| `BodyMode` | `BodyMode` | Kein Validierungsattribut |
| `Body` | `string?` | Kein Validierungsattribut |
| `AuthenticationType` | `AuthenticationType` | Kein Validierungsattribut |
| `PreRequestScript` | `string?` | Kein Validierungsattribut |
| `PostRequestScript` | `string?` | Kein Validierungsattribut |

## `AddEndpointKeyValueRequest`
Datei: `src/Schnittstellenzentrale.Core/Contracts/AddEndpointKeyValueRequest.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Key` | `string` | `[Required]` — kein `ErrorMessage`-Parameter |
| `Value` | `string` | Kein Validierungsattribut |
| `EndpointId` | `int` | `[Range(1, int.MaxValue)]` — kein `ErrorMessage`-Parameter |
