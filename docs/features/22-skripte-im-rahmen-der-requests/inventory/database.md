# Datenbankschicht

## Aktuelles Schema der `Endpoints`-Tabelle

Laut `AppDbContextModelSnapshot.cs` (Datei: `src/Schnittstellenzentrale.Infrastructure/Data/Migrations/AppDbContextModelSnapshot.cs`) enthält die Tabelle `Endpoints` folgende Spalten:

| Spalte | Typ (SQLite) | Nullable | Besonderheit |
|---|---|---|---|
| `Id` | `INTEGER` | nein | PK, Autoincrement |
| `ApplicationId` | `INTEGER` | nein | FK zu `Applications` |
| `AuthenticationType` | `INTEGER` | nein | Enum-Wert |
| `Body` | `TEXT` | ja | |
| `BodyMode` | `INTEGER` | nein | Enum-Wert |
| `EndpointGroupId` | `INTEGER` | ja | FK zu `EndpointGroups` |
| `Method` | `INTEGER` | nein | Enum-Wert |
| `Name` | `TEXT` (max 200) | nein | |
| `RelativePath` | `TEXT` (max 500) | nein | |
| `RowVersion` | `BLOB` | nein | Concurrency-Token |

Die Spalten `PreRequestScript` und `PostRequestScript` sind **noch nicht vorhanden** — weder im Modell noch im Schema.

## Vorhandene Migrationen (SQLite)

| Migration | Inhalt |
|---|---|
| `20260514123204_InitialCreate` | Initiales Schema mit `Applications`, `ApplicationGroups`, `Endpoints`, `EndpointHeaders`, `EndpointQueryParameters` |
| `20260517183433_AddInterfaceUrl` | Spalte `InterfaceUrl` zu `Applications` |
| `20260518000000_AddIsSystemToApplicationGroupAndApplication` | Spalte `IsSystem` zu `Applications` und `ApplicationGroups` |
| `20260519000000_AddBodyModeToEndpoint` | Spalte `BodyMode` zu `Endpoints` |
| `20260519000001_CascadeDeleteEndpointGroup` | Cascade-Delete für `EndpointGroup`-Beziehung |
| `20260523000000_AddParentGroupIdToEndpointGroup` | Spalte `ParentGroupId` zu `EndpointGroups` |
| `20260524143437_AddSystemEnvironments` | Neue Tabellen `SystemEnvironments` und `EnvironmentVariables` |

Für `PreRequestScript` und `PostRequestScript` muss eine neue Migration erstellt werden.
