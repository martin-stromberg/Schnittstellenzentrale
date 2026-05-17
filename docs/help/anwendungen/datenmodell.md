# Anwendungen — Datenmodell

## Entitäten

Die Entitäten `ApplicationGroup` und `Application` sind in `Schnittstellenzentrale.Core` definiert.

### `ApplicationGroup`

| Eigenschaft | Typ | Beschreibung |
|-------------|-----|--------------|
| `Id` | `int` | Primärschlüssel (automatisch vergeben) |
| `Name` | `string` | Anzeigename der Gruppe (Pflichtfeld) |
| `RowVersion` | `byte[]` | Optimistische Nebenläufigkeitskontrolle |
| `Applications` | `IList<Application>` | Zugeordnete Anwendungen (Navigationseigenschaft) |

### `Application`

| Eigenschaft | Typ | Beschreibung |
|-------------|-----|--------------|
| `Id` | `int` | Primärschlüssel (automatisch vergeben) |
| `Name` | `string` | Anzeigename der Anwendung (Pflichtfeld) |
| `BaseUrl` | `string` | Basis-URL des Dienstes (Pflichtfeld) |
| `Description` | `string?` | Optionale Beschreibung |
| `SwaggerUrl` | `string?` | Optionale URL zur Swagger/OpenAPI-Beschreibung |
| `MetadataUrl` | `string?` | Optionale URL zu weiteren Metadaten |
| `ApplicationGroupId` | `int?` | Fremdschlüssel zur zugeordneten Gruppe (optional) |
| `Owner` | `string?` | Windows-Benutzername des Eigentümers; nur im Benutzermodus gesetzt |
| `RowVersion` | `byte[]` | Optimistische Nebenläufigkeitskontrolle |

## Beziehungen

Eine `ApplicationGroup` kann keine oder beliebig viele `Application`-Einträge enthalten. Die Zuordnung ist optional: Eine `Application` kann auch ohne Gruppe (`ApplicationGroupId = null`) existieren.

Beim Löschen einer Gruppe setzt EF Core `ApplicationGroupId` der enthaltenen Anwendungen auf `null` (`DeleteBehavior.SetNull`). Das UI-seitige explizite Entkoppeln in `ApplicationGroupTree` ist trotzdem erforderlich, damit SignalR-Benachrichtigungen für die einzelnen Anwendungen ausgelöst werden.

## Diagramm

```mermaid
erDiagram
    ApplicationGroup {
        int Id
        string Name
        byte[] RowVersion
    }
    Application {
        int Id
        string Name
        string BaseUrl
        string Description
        string SwaggerUrl
        string MetadataUrl
        int ApplicationGroupId
        string Owner
        byte[] RowVersion
    }
    ApplicationGroup ||--o{ Application : "enthält"
```
