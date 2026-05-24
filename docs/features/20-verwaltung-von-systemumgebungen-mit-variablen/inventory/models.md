# Datenmodell

## `Application`
Datei: `src/Schnittstellenzentrale.Core/Models/Application.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Name der Anwendung |
| `IsSystem` | `bool` | Systemmarkierung |
| `Description` | `string` | Beschreibungstext |
| `BaseUrl` | `string` | Basis-URL (Ziel der `{{...}}`-Auflösung) |
| `InterfaceUrl` | `string?` | URL der Schnittstellendefinition |
| `InterfaceType` | `InterfaceType` | Typ der Schnittstelle (OData, Rest, …) |
| `Owner` | `string?` | Besitzer-Kennung (Windows-Benutzername) — analog geplant für `SystemEnvironment.Owner` |
| `ApplicationGroupId` | `int?` | Fremdschlüssel auf `ApplicationGroup` |
| `RowVersion` | `byte[]` | Optimistisches Sperren |
| `Endpoints` | `ICollection<Endpoint>` | Navigationseigenschaft |
| `EndpointGroups` | `ICollection<EndpointGroup>` | Navigationseigenschaft |

Hinweis: Das `Owner`-Feld und das `StorageMode`-Filtermuster in `ApplicationRepository` dienen als Referenzimplementierung für die geplanten `SystemEnvironment`- und `EnvironmentVariable`-Modelle.

---

## `Endpoint`
Datei: `src/Schnittstellenzentrale.Core/Models/Endpoint.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Name des Endpunkts |
| `Method` | `HttpMethod` | HTTP-Methode |
| `RelativePath` | `string` | Relativer Pfad (enthält `{Platzhalter}`) |
| `Body` | `string?` | Request-Body (Ziel der `{{...}}`-Auflösung) |
| `BodyMode` | `BodyMode` | Darstellungsmodus des Body |
| `AuthenticationType` | `AuthenticationType` | Authentifizierungstyp |
| `ApplicationId` | `int` | Fremdschlüssel auf `Application` |
| `EndpointGroupId` | `int?` | Fremdschlüssel auf `EndpointGroup` |
| `RowVersion` | `byte[]` | Optimistisches Sperren |
| `Headers` | `ICollection<EndpointHeader>` | Header-Sammlung (Ziel der `{{...}}`-Auflösung) |
| `QueryParameters` | `ICollection<EndpointQueryParameter>` | Query-Parameter (Ziel der `{{...}}`-Auflösung und `{...}`-Auflösung) |

---

## `EndpointHeader`
Datei: `src/Schnittstellenzentrale.Core/Models/EndpointHeader.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel |
| `Key` | `string` | Header-Name (Ziel der `{{...}}`-Auflösung) |
| `Value` | `string` | Header-Wert (Ziel der `{{...}}`-Auflösung) |
| `EndpointId` | `int` | Fremdschlüssel auf `Endpoint` |
| `Endpoint` | `Endpoint` | Navigationseigenschaft |

---

## `EndpointQueryParameter`
Datei: `src/Schnittstellenzentrale.Core/Models/EndpointQueryParameter.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel |
| `Key` | `string` | Parametername (Ziel der `{{...}}`-Auflösung) |
| `Value` | `string` | Parameterwert (Ziel der `{{...}}`-Auflösung) |
| `EndpointId` | `int` | Fremdschlüssel auf `Endpoint` |
| `Endpoint` | `Endpoint` | Navigationseigenschaft |

---

## Fehlende Modellklassen

Die Anforderung sieht folgende neue Klassen vor, die noch nicht existieren:

- `SystemEnvironment` — Systemumgebung mit `Name`, `Mode` (`StorageMode`), `Owner` (`string?`) und Variablenliste
- `EnvironmentVariable` — Variable innerhalb einer `SystemEnvironment` mit `Name`, `Value` und `IsValueMasked`
