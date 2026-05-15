# Datenmodell

## `Application`
Datei: `src/Schnittstellenzentrale.Core/Models/Application.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Pflichtfeld, Anzeigename der Anwendung |
| `Description` | `string` | Beschreibung (leer per Default, nicht nullable) |
| `BaseUrl` | `string` | Basis-URL der Anwendung (Pflichtfeld) |
| `SwaggerUrl` | `string?` | Optionale Swagger/OpenAPI-URL |
| `MetadataUrl` | `string?` | Optionale OData-Metadaten-URL |
| `Owner` | `string?` | Windows-Benutzername bei `StorageMode.User`, sonst `null` |
| `ApplicationGroupId` | `int?` | Optionaler Fremdschlüssel auf `ApplicationGroup` |
| `ApplicationGroup` | `ApplicationGroup?` | Navigationseigenschaft zur Gruppe |
| `RowVersion` | `byte[]` | Optimistische Nebenläufigkeitskontrolle |
| `Endpoints` | `ICollection<Endpoint>` | Navigationseigenschaft — zugehörige Endpunkte |
| `EndpointGroups` | `ICollection<EndpointGroup>` | Navigationseigenschaft — zugehörige Endpunktgruppen |

## `ApplicationGroup`
Datei: `src/Schnittstellenzentrale.Core/Models/ApplicationGroup.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Pflichtfeld, Anzeigename der Gruppe |
| `RowVersion` | `byte[]` | Optimistische Nebenläufigkeitskontrolle |
| `Applications` | `ICollection<Application>` | Navigationseigenschaft — zugehörige Anwendungen |
