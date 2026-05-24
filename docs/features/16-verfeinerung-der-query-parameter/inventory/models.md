# Datenmodell

## `EndpointQueryParameter`
Datei: `src/Schnittstellenzentrale.Core/Models/EndpointQueryParameter.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel |
| `Key` | `string` | Name des Query-Parameters |
| `Value` | `string` | Wert des Query-Parameters |
| `EndpointId` | `int` | Fremdschlüssel zu `Endpoint` |
| `Endpoint` | `Endpoint` | Navigationseigenschaft zum zugehörigen Endpunkt |

Kein `IsPathParameter`-Feld vorhanden. Pfad-Platzhalter-Werte und reguläre Query-Parameter werden aktuell gleichartig gespeichert.

---

## `Endpoint`
Datei: `src/Schnittstellenzentrale.Core/Models/Endpoint.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename des Endpunkts |
| `Method` | `HttpMethod` | HTTP-Methode (Enum) |
| `RelativePath` | `string` | Relativer Pfad-Template (z. B. `/api/applications/{id}`) |
| `Body` | `string?` | Request-Body |
| `BodyMode` | `BodyMode` | Art des Body-Inhalts (Enum) |
| `AuthenticationType` | `AuthenticationType` | Authentifizierungsart (Enum) |
| `ApplicationId` | `int` | Fremdschlüssel zu `Application` |
| `Application` | `Application` | Navigationseigenschaft zur zugehörigen Anwendung |
| `EndpointGroupId` | `int?` | Optionaler Fremdschlüssel zu `EndpointGroup` |
| `EndpointGroup` | `EndpointGroup?` | Navigationseigenschaft zur zugehörigen Endpunktgruppe |
| `RowVersion` | `byte[]` | Optimistic-Concurrency-Token |
| `Headers` | `ICollection<EndpointHeader>` | Zugeordnete Request-Header |
| `QueryParameters` | `ICollection<EndpointQueryParameter>` | Zugeordnete Query-Parameter |

`RelativePath` speichert bereits das Pfad-Template; kein separates Feld für Query-String-Anteil.
