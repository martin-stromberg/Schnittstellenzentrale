# Datenmodell

## `ApplicationGroup`
Datei: `src/Schnittstellenzentrale.Core/Models/ApplicationGroup.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename der Gruppe |
| `IsSystem` | `bool` | Kennzeichnet die interne Systemgruppe |
| `RowVersion` | `byte[]` | Optimistic-Concurrency-Token |
| `Applications` | `ICollection<Application>` | Navigationseigenschaft zu zugehörigen Anwendungen |

Fehlend (laut Anforderung): `Description`, `Subtitle`, `IconData`

---

## `Application`
Datei: `src/Schnittstellenzentrale.Core/Models/Application.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename der Anwendung |
| `IsSystem` | `bool` | Kennzeichnet Systemanwendungen |
| `Description` | `string` | Beschreibungstext (bereits vorhanden) |
| `BaseUrl` | `string` | Basis-URL der Anwendung |
| `InterfaceUrl` | `string?` | Swagger- oder OData-Metadaten-URL |
| `InterfaceType` | `InterfaceType` | Erkannter Schnittstellentyp (Rest/OData/Unknown) |
| `Owner` | `string?` | Eigentümer im User-Modus |
| `ApplicationGroupId` | `int?` | FK zur zugehörigen `ApplicationGroup` |
| `ApplicationGroup` | `ApplicationGroup?` | Navigationseigenschaft |
| `RowVersion` | `byte[]` | Optimistic-Concurrency-Token |
| `Endpoints` | `ICollection<Endpoint>` | Navigationseigenschaft zu Endpunkten |
| `EndpointGroups` | `ICollection<EndpointGroup>` | Navigationseigenschaft zu Ordnern |

Fehlend (laut Anforderung): `Subtitle`, `IconData`

Methoden:
- `DetectInterfaceType(string?)` — statische Hilfsmethode zur URL-Typbestimmung
- `Clone()` — erzeugt eine flache Kopie (ohne Navigation)

---

## `SystemEnvironment`
Datei: `src/Schnittstellenzentrale.Core/Models/SystemEnvironment.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename der Umgebung |
| `Mode` | `StorageMode` | Speichermodus (Team/User) |
| `Owner` | `string?` | Eigentümer im User-Modus |
| `Variables` | `ICollection<EnvironmentVariable>` | Navigationseigenschaft zu Variablen |

Fehlend (laut Anforderung): `Description`

---

## `ActivityLogEntry`
Datei: `src/Schnittstellenzentrale.Core/Models/ActivityLogEntry.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Timestamp` | `DateTime` | Zeitstempel des Eintrags |
| `Category` | `ActivityLogCategory` | Kategorie des Ereignisses |
| `Message` | `string` | Kurzbeschreibung |
| `Details` | `string?` | Optionaler Detailtext |

Hinweis: Nur In-Memory-Speicherung; keine Persistenz in der Datenbank.

---

## `EndpointGroup`
Datei: `src/Schnittstellenzentrale.Core/Models/EndpointGroup.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename des Ordners |
| `ApplicationId` | `int` | FK zur übergeordneten Anwendung |
| `Application` | `Application` | Navigationseigenschaft |
| `ParentGroupId` | `int?` | FK zur übergeordneten Gruppe (Hierarchie) |
| `ParentGroup` | `EndpointGroup?` | Navigationseigenschaft |
| `RowVersion` | `byte[]` | Optimistic-Concurrency-Token |
| `Endpoints` | `ICollection<Endpoint>` | Endpunkte direkt in diesem Ordner |
| `ChildGroups` | `ICollection<EndpointGroup>` | Unterordner |

---

## `Endpoint`
Datei: `src/Schnittstellenzentrale.Core/Models/Endpoint.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename |
| `Method` | `HttpMethod` | HTTP-Methode |
| `RelativePath` | `string` | Relativer Pfad (kann Platzhalter enthalten) |
| `Body` | `string?` | Request-Body |
| `BodyMode` | `BodyMode` | Modus des Request-Body |
| `AuthenticationType` | `AuthenticationType` | Authentifizierungstyp |
| `ApplicationId` | `int` | FK zur übergeordneten Anwendung |
| `Application` | `Application` | Navigationseigenschaft |
| `EndpointGroupId` | `int?` | FK zum Ordner |
| `EndpointGroup` | `EndpointGroup?` | Navigationseigenschaft |
| `RowVersion` | `byte[]` | Optimistic-Concurrency-Token |
| `PreRequestScript` | `string?` | Skript vor der Anfrage |
| `PostRequestScript` | `string?` | Skript nach der Anfrage |
| `Headers` | `ICollection<EndpointHeader>` | Request-Header |
| `QueryParameters` | `ICollection<EndpointQueryParameter>` | Query-Parameter |
