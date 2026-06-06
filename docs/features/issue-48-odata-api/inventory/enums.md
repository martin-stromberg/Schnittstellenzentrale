# Enums

## `InterfaceType`
Datei: `src/Schnittstellenzentrale.Core/Enums/InterfaceType.cs`

| Wert | Bedeutung |
|------|-----------|
| `Unknown = 0` | Schnittstellentyp nicht erkannt |
| `Rest = 1` | REST/Swagger-Schnittstelle |
| `OData = 2` | OData-Schnittstelle |

Der Wert wird in `Application.DetectInterfaceType(string? url)` automatisch aus der `InterfaceUrl` abgeleitet: `$metadata` → `OData`, `swagger`/`openapi` → `Rest`.

---

## `HttpMethod`
Datei: `src/Schnittstellenzentrale.Core/Enums/HttpMethod.cs`

| Wert | Bedeutung |
|------|-----------|
| `GET` | HTTP GET |
| `POST` | HTTP POST |
| `PUT` | HTTP PUT |
| `DELETE` | HTTP DELETE |
| `PATCH` | HTTP PATCH |
| `HEAD` | HTTP HEAD |
| `OPTIONS` | HTTP OPTIONS |

Der `ODataImportService` erzeugt für Entity-Sets `GET` und `POST`, für `IEdmAction`-Operationen `POST` und für `IEdmFunction`-Operationen `GET`.
