# Enums

## `StorageMode`
Datei: `src/Schnittstellenzentrale.Core/Enums/StorageMode.cs`

| Wert | Bedeutung |
|------|-----------|
| `Team` | Team-Speicherkontext; einzige Bedingung für SignalR-Notifications in Controllern und UI |
| `User` | Benutzerkontext; Notifications werden nicht gefeuert |

## `InterfaceType`
Datei: `src/Schnittstellenzentrale.Core/Enums/InterfaceType.cs`

| Wert | Bedeutung |
|------|-----------|
| `Unknown = 0` | Kein erkennbarer Schnittstellentyp → `POST /import` liefert `422` |
| `Rest = 1` | REST/Swagger → Dispatch auf `ISwaggerImportService` |
| `OData = 2` | OData → Dispatch auf `IODataImportService`; Pflicht für `odata-import/apply` |

## `AuthenticationType`
Datei: `src/Schnittstellenzentrale.Core/Enums/AuthenticationType.cs`

Relevant: `BearerToken` löst in `SwaggerImportService.ApplyDiffAsync`/`ODataImportService` den Credential-Store-Schreibpfad aus; `Negotiate`/`Basic`/`None` sind weitere Werte (in `ODataImportService.ResolveAuthType` gemappt, Z. 267–274).

## `HttpMethod`
Datei: `src/Schnittstellenzentrale.Core/Enums/HttpMethod.cs`

`GET`, `POST`, `PUT`, `DELETE`, `PATCH`, `HEAD`, `OPTIONS` — Teil des Endpunktschlüssels (`EndpointKeyHelper.BuildKey` = `"{Method}:{RelativePath}"`).

## `BodyMode`
Datei: `src/Schnittstellenzentrale.Core/Enums/BodyMode.cs`

`None`, `Json`, `Xml`, `PlainText` (steuert u. a. Auto-`Content-Type`-Header in `EndpointPage.SyncAutoContentType`).

## Nicht vorhanden

Ein Enum für den geplanten `content`-Parameter (`delete`/`detach`, z. B. `GroupContentDeleteMode`) existiert **nicht** — die Enum-Dateiliste unter `src/Schnittstellenzentrale.Core/Enums/` enthält keinen entsprechenden Typ.
