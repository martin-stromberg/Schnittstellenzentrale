# Datenmodell

## `ApplicationGroup`
Datei: `src/Schnittstellenzentrale.Core/Models/ApplicationGroup.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Gruppenname (max. 200 Zeichen, Pflichtfeld) |
| `RowVersion` | `byte[]` | Optimistisches Concurrency-Token |
| `Applications` | `ICollection<Application>` | Navigationseigenschaft zur Kindmenge |

`IsSystem` ist noch **nicht** vorhanden.

---

## `Application`
Datei: `src/Schnittstellenzentrale.Core/Models/Application.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anwendungsname (max. 200 Zeichen, Pflichtfeld) |
| `Description` | `string` | Beschreibungstext |
| `BaseUrl` | `string` | Basis-URL (max. 500 Zeichen, Pflichtfeld) |
| `InterfaceUrl` | `string?` | URL zur Schnittstellenbeschreibung (max. 500 Zeichen, optional) |
| `InterfaceType` | `InterfaceType` | Erkannter Schnittstellentyp (Enum) |
| `Owner` | `string?` | Besitzername für User-Modus (max. 256 Zeichen) |
| `ApplicationGroupId` | `int?` | Fremdschlüssel zur übergeordneten Gruppe |
| `ApplicationGroup` | `ApplicationGroup?` | Navigationseigenschaft zur Gruppe |
| `RowVersion` | `byte[]` | Optimistisches Concurrency-Token |
| `Endpoints` | `ICollection<Endpoint>` | Navigationseigenschaft zu Endpunkten |
| `EndpointGroups` | `ICollection<EndpointGroup>` | Navigationseigenschaft zu Endpunktgruppen |

Statische Methoden: `DetectInterfaceType(string? url)` — leitet `InterfaceType` aus der URL ab.
Instanzmethode: `Clone()` — erzeugt eine flache Kopie ohne Navigationseigenschaften.

`IsSystem` ist noch **nicht** vorhanden.

---

## `ApplicationGroupResponse`
Datei: `src/Schnittstellenzentrale.Core/Contracts/ApplicationGroupResponse.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Gruppen-ID |
| `Name` | `string` | Gruppenname |
| `Applications` | `IList<ApplicationResponse>` | Enthaltene Anwendungen |

`IsSystem` ist noch **nicht** vorhanden.

---

## `ApplicationResponse`
Datei: `src/Schnittstellenzentrale.Core/Contracts/ApplicationResponse.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Anwendungs-ID |
| `Name` | `string` | Anwendungsname |
| `BaseUrl` | `string` | Basis-URL |
| `ApplicationGroupId` | `int?` | ID der zugeordneten Gruppe |
| `Description` | `string` | Beschreibungstext |
| `InterfaceUrl` | `string?` | Schnittstellenbeschreibungs-URL |
| `InterfaceType` | `int` | Schnittstellentyp (Enum-Wert als int) |
| `Owner` | `string?` | Besitzername |

`IsSystem` ist noch **nicht** vorhanden.

---

## `CreateApplicationGroupRequest`
Datei: `src/Schnittstellenzentrale.Core/Contracts/CreateApplicationGroupRequest.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Name` | `string` | Gruppenname (Pflichtfeld, max. 200 Zeichen) |

`IsSystem` ist bewusst **nicht** enthalten und soll auch nicht hinzugefügt werden.

---

## `UpdateApplicationGroupRequest`
Datei: `src/Schnittstellenzentrale.Core/Contracts/UpdateApplicationGroupRequest.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Name` | `string` | Neuer Gruppenname (Pflichtfeld, max. 200 Zeichen) |

`IsSystem` ist bewusst **nicht** enthalten und soll auch nicht hinzugefügt werden.

---

## `CreateApplicationRequest`
Datei: `src/Schnittstellenzentrale.Core/Contracts/CreateApplicationRequest.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Name` | `string` | Anwendungsname (Pflichtfeld, max. 200 Zeichen) |
| `BaseUrl` | `string` | Basis-URL (Pflichtfeld, max. 500 Zeichen) |
| `Description` | `string?` | Beschreibungstext (optional) |
| `InterfaceUrl` | `string?` | Schnittstellenbeschreibungs-URL (optional, max. 500 Zeichen) |
| `ApplicationGroupId` | `int?` | Gruppen-Zuordnung (optional) |
| `Owner` | `string?` | Besitzername (optional, max. 256 Zeichen) |

`IsSystem` ist bewusst **nicht** enthalten und soll auch nicht hinzugefügt werden.

---

## `UpdateApplicationRequest`
Datei: `src/Schnittstellenzentrale.Core/Contracts/UpdateApplicationRequest.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Name` | `string` | Anwendungsname (Pflichtfeld, max. 200 Zeichen) |
| `BaseUrl` | `string` | Basis-URL (Pflichtfeld, max. 500 Zeichen) |
| `Description` | `string?` | Beschreibungstext (optional) |
| `InterfaceUrl` | `string?` | Schnittstellenbeschreibungs-URL (optional, max. 500 Zeichen) |
| `ApplicationGroupId` | `int?` | Gruppen-Zuordnung (optional) |
| `Owner` | `string?` | Besitzername (optional, max. 256 Zeichen) |

`IsSystem` ist bewusst **nicht** enthalten und soll auch nicht hinzugefügt werden.
