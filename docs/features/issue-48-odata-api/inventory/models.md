# Datenmodell

## `Application`
Datei: `src/Schnittstellenzentrale.Core/Models/Application.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename der Anwendung |
| `IsSystem` | `bool` | Schützt System-Anwendungen vor Bearbeitung |
| `Description` | `string` | Freitext-Beschreibung |
| `BaseUrl` | `string` | Basis-URL für API-Aufrufe |
| `InterfaceUrl` | `string?` | URL zum CSDL-Metadaten-Dokument (OData) oder zur Swagger-Definition |
| `InterfaceType` | `InterfaceType` | Erkannter Schnittstellentyp (via `DetectInterfaceType`) |
| `Owner` | `string?` | Besitzer-Bezeichner |
| `ApplicationGroupId` | `int?` | FK zur zugehörigen Gruppe |
| `ApplicationGroup` | `ApplicationGroup?` | Navigationseigenschaft zur Gruppe |
| `Subtitle` | `string?` | Untertitel |
| `IconData` | `byte[]?` | Icon als Binärdaten |
| `RowVersion` | `byte[]` | Concurrency-Token |
| `Endpoints` | `ICollection<Endpoint>` | Navigationseigenschaft zu den Endpunkten |
| `Links` | `ICollection<ApplicationLink>` | Navigationseigenschaft zu den Links |
| `EndpointGroups` | `ICollection<EndpointGroup>` | Navigationseigenschaft zu den Endpunkt-Gruppen |

Statische Methoden:
- `DetectInterfaceType(string? url)` — leitet `InterfaceType.OData` ab, wenn die URL `$metadata` enthält; `InterfaceType.Rest` bei `swagger`/`openapi`; sonst `Unknown`.

---

## `ImportDiff`
Datei: `src/Schnittstellenzentrale.Core/Models/ImportDiff.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `NewEndpoints` | `IList<Endpoint>` | Neu importierte Endpunkte, die noch nicht im Bestand sind |
| `ChangedEndpoints` | `IList<Endpoint>` | Endpunkte, deren Eigenschaften sich geändert haben |
| `RemovedEndpoints` | `IList<Endpoint>` | Im Bestand vorhandene, aber im Import nicht mehr enthaltene Endpunkte |
| `ErrorMessage` | `string?` | Fehlermeldung bei HTTP- oder Parse-Fehler; `null` bei Erfolg |
| `BearerTokens` | `IDictionary<string, string>` | Token-Map aus Swagger-Erweiterungen (wird von `ODataImportService` nicht befüllt) |

Methoden:
- `WithBearerTokens(IDictionary<string, string>)` — gibt eine neue Instanz mit geänderter Token-Map zurück (wird vom `SwaggerImportService` genutzt, nicht von `ODataImportService`).

---

## `Endpoint`
Datei: `src/Schnittstellenzentrale.Core/Models/Endpoint.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename des Endpunkts |
| `Method` | `HttpMethod` | HTTP-Methode (GET, POST, …) |
| `RelativePath` | `string` | Pfad relativ zur BaseUrl |
| `Body` | `string?` | Request-Body-Template |
| `BodyMode` | `BodyMode` | Darstellungsart des Body |
| `AuthenticationType` | `AuthenticationType` | Authentifizierungstyp |
| `ApplicationId` | `int` | FK zur zugehörigen Anwendung |
| `Application` | `Application` | Navigationseigenschaft zur Anwendung |
| `EndpointGroupId` | `int?` | FK zur Endpunkt-Gruppe |
| `EndpointGroup` | `EndpointGroup?` | Navigationseigenschaft zur Gruppe |
| `RowVersion` | `byte[]` | Concurrency-Token |
| `PreRequestScript` | `string?` | Script vor dem Request |
| `PostRequestScript` | `string?` | Script nach dem Request |
| `Headers` | `ICollection<EndpointHeader>` | Navigationseigenschaft zu den Headern |
| `QueryParameters` | `ICollection<EndpointQueryParameter>` | Navigationseigenschaft zu den Query-Parametern |
