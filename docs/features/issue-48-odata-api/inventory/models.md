# Datenmodell

## `Application`
Datei: `src/Schnittstellenzentrale.Core/Models/Application.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename der Anwendung |
| `IsSystem` | `bool` | Kennzeichen für Systemanwendungen (schreibgeschützt via REST) |
| `Description` | `string` | Freitextbeschreibung |
| `BaseUrl` | `string` | Basis-URL der Anwendung |
| `InterfaceUrl` | `string?` | URL des API-Schnittstellendokuments (z. B. `$metadata` oder Swagger) |
| `InterfaceType` | `InterfaceType` | Automatisch aus `InterfaceUrl` erkannter Typ (OData, Rest, Unknown) |
| `Owner` | `string?` | Besitzer im User-Modus |
| `ApplicationGroupId` | `int?` | Fremdschlüssel auf `ApplicationGroup` |
| `ApplicationGroup` | `ApplicationGroup?` | Navigationseigenschaft zur Gruppe |
| `Subtitle` | `string?` | Untertitel für die Anzeige |
| `IconData` | `byte[]?` | Binärdaten des Icons |
| `RowVersion` | `byte[]` | Optimistic-Concurrency-Stempel |
| `Endpoints` | `ICollection<Endpoint>` | Navigationseigenschaft: zugehörige Endpunkte |
| `Links` | `ICollection<ApplicationLink>` | Navigationseigenschaft: zugehörige Links |
| `EndpointGroups` | `ICollection<EndpointGroup>` | Navigationseigenschaft: zugehörige Endpunktgruppen |

Statische Methode `DetectInterfaceType(string? url)`: Erkennt anhand der URL automatisch `InterfaceType.OData` (bei `$metadata`-Substring) oder `InterfaceType.Rest` (bei `swagger`/`openapi`-Substring).

---

## `ApplicationGroup`
Datei: `src/Schnittstellenzentrale.Core/Models/ApplicationGroup.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Gruppenname |
| `IsSystem` | `bool` | Kennzeichen für Systemgruppen (schreibgeschützt via REST) |
| `Description` | `string?` | Freitextbeschreibung |
| `Subtitle` | `string?` | Untertitel für die Anzeige |
| `IconData` | `byte[]?` | Binärdaten des Icons |
| `RowVersion` | `byte[]` | Optimistic-Concurrency-Stempel |
| `Applications` | `ICollection<Application>` | Navigationseigenschaft: enthaltene Anwendungen |

---

## `Endpoint`
Datei: `src/Schnittstellenzentrale.Core/Models/Endpoint.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename |
| `Method` | `HttpMethod` | HTTP-Methode (GET, POST, PUT, PATCH, DELETE, …) |
| `RelativePath` | `string` | Pfad relativ zur BaseUrl |
| `Body` | `string?` | Request-Body-Template |
| `BodyMode` | `BodyMode` | Steuert Content-Type (None, Json, Xml, PlainText) |
| `AuthenticationType` | `AuthenticationType` | Authentifizierungstyp |
| `ApplicationId` | `int` | Fremdschlüssel auf `Application` |
| `Application` | `Application` | Navigationseigenschaft |
| `EndpointGroupId` | `int?` | Fremdschlüssel auf `EndpointGroup` |
| `EndpointGroup` | `EndpointGroup?` | Navigationseigenschaft |
| `RowVersion` | `byte[]` | Optimistic-Concurrency-Stempel |
| `PreRequestScript` | `string?` | Skript vor dem Request |
| `PostRequestScript` | `string?` | Skript nach dem Request |
| `Headers` | `ICollection<EndpointHeader>` | Navigationseigenschaft: HTTP-Header |
| `QueryParameters` | `ICollection<EndpointQueryParameter>` | Navigationseigenschaft: Query-Parameter |

---

## `EndpointGroup`
Datei: `src/Schnittstellenzentrale.Core/Models/EndpointGroup.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Gruppenname |
| `ApplicationId` | `int` | Fremdschlüssel auf `Application` |
| `Application` | `Application` | Navigationseigenschaft |
| `ParentGroupId` | `int?` | Self-Referenz: übergeordnete Gruppe |
| `ParentGroup` | `EndpointGroup?` | Navigationseigenschaft: Elterngruppe |
| `RowVersion` | `byte[]` | Optimistic-Concurrency-Stempel |
| `Endpoints` | `ICollection<Endpoint>` | Navigationseigenschaft: enthaltene Endpunkte |
| `ChildGroups` | `ICollection<EndpointGroup>` | Navigationseigenschaft: Untergruppen (Self-Referenz) |

---

## `ImportDiff`
Datei: `src/Schnittstellenzentrale.Core/Models/ImportDiff.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `NewEndpoints` | `IList<Endpoint>` | Neu hinzuzufügende Endpunkte |
| `ChangedEndpoints` | `IList<Endpoint>` | Geänderte Endpunkte |
| `RemovedEndpoints` | `IList<Endpoint>` | Zu entfernende Endpunkte |
| `ErrorMessage` | `string?` | Fehlermeldung bei Parse-Fehler |
| `BearerTokens` | `IDictionary<string, string>` | Bearer-Token-Zuordnungen (aus Swagger-Import) |

Verwendet von `ODataImportService.ImportAsync` als Rückgabewert.
