# Datenmodell

## `Application`
Datei: `src/Schnittstellenzentrale.Core/Models/Application.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename der Anwendung |
| `Description` | `string` | Beschreibungstext |
| `BaseUrl` | `string` | Basis-URL der Anwendung |
| `SwaggerUrl` | `string?` | Optionale Swagger-URL |
| `MetadataUrl` | `string?` | Optionale OData-Metadaten-URL |
| `Owner` | `string?` | Windows-Benutzername des Eigentümers (relevant im `StorageMode.User`) |
| `ApplicationGroupId` | `int?` | Fremdschlüssel zur Gruppe; `null` = gruppenlos |
| `ApplicationGroup` | `ApplicationGroup?` | Navigationseigenschaft zur Gruppe |
| `RowVersion` | `byte[]` | Optimistische Nebenläufigkeitskontrolle (Concurrency Token) |
| `Endpoints` | `ICollection<Endpoint>` | Navigationseigenschaft zu den Endpunkten |
| `EndpointGroups` | `ICollection<EndpointGroup>` | Navigationseigenschaft zu den Endpunktgruppen |

Konfiguration in `AppDbContext`: `Name` max. 200 Zeichen (required), `BaseUrl` max. 500 Zeichen (required), `Owner` max. 256 Zeichen, `RowVersion` als ConcurrencyToken. Beziehung zu `ApplicationGroup` mit `OnDelete(DeleteBehavior.SetNull)`.

---

## `ApplicationGroup`
Datei: `src/Schnittstellenzentrale.Core/Models/ApplicationGroup.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename der Gruppe |
| `RowVersion` | `byte[]` | Optimistische Nebenläufigkeitskontrolle (Concurrency Token) |
| `Applications` | `ICollection<Application>` | Navigationseigenschaft zu den enthaltenen Anwendungen |

Konfiguration in `AppDbContext`: `Name` max. 200 Zeichen (required), `RowVersion` als ConcurrencyToken. FK-Beziehung zu `Application` mit `OnDelete(DeleteBehavior.SetNull)` — beim Löschen einer Gruppe werden die Anwendungen **nicht** kaskadierend gelöscht, sondern ihr `ApplicationGroupId` wird auf `null` gesetzt.
