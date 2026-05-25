# Datenmodell

## `Endpoint`
Datei: `src/Schnittstellenzentrale.Core/Models/Endpoint.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename des Endpunkts |
| `Method` | `HttpMethod` (Enum) | HTTP-Methode |
| `RelativePath` | `string` | Relativer Pfad des Endpunkts |
| `Body` | `string?` | Request-Body (optional) |
| `BodyMode` | `BodyMode` (Enum) | Modus des Request-Bodys |
| `AuthenticationType` | `AuthenticationType` (Enum) | Authentifizierungstyp |
| `ApplicationId` | `int` | Fremdschlüssel auf `Application` |
| `Application` | `Application` | Navigationseigenschaft zur Anwendung |
| `EndpointGroupId` | `int?` | Optionaler Fremdschlüssel auf `EndpointGroup` |
| `EndpointGroup` | `EndpointGroup?` | Navigationseigenschaft zur Gruppe |
| `RowVersion` | `byte[]` | Optimistic-Concurrency-Token |
| `PreRequestScript` | `string?` | Optionales JavaScript-Skript, das vor dem Request ausgeführt wird |
| `PostRequestScript` | `string?` | Optionales JavaScript-Skript, das nach dem Request ausgeführt wird |
| `Headers` | `ICollection<EndpointHeader>` | Request-Header |
| `QueryParameters` | `ICollection<EndpointQueryParameter>` | Query-Parameter |

Hinweis: Es gibt **kein** Feld `BearerTokenValue` oder `BearerTokenHint` im Modell. Der Bearer-Token-Wert wird ausschließlich über den Windows Credential Manager verwaltet (siehe `ICredentialService`).

---

## `ImportDiff`
Datei: `src/Schnittstellenzentrale.Core/Models/ImportDiff.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `NewEndpoints` | `IList<Endpoint>` | Endpunkte, die neu angelegt werden sollen |
| `ChangedEndpoints` | `IList<Endpoint>` | Endpunkte, bei denen sich Felder geändert haben |
| `RemovedEndpoints` | `IList<Endpoint>` | Endpunkte, die in der Swagger-Definition nicht mehr vorhanden sind |
| `ErrorMessage` | `string?` | Fehlermeldung bei Parse- oder Diff-Fehler |
