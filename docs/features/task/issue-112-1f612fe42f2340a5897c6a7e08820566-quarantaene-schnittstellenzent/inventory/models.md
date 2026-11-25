# Datenmodell

Für den Testfluss relevante Modellklassen. Nur Eigenschaften aufgeführt, die der Test bzw. der betroffene Codepfad berührt.

## `EndpointGroup`

Datei: `src/Schnittstellenzentrale.Core/Models/EndpointGroup.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel; wird in `ApplicationGroupTree` für `_expandedEndpointGroupIds` und `@key="eg-{Id}"` genutzt |
| `Name` | `string` | Anzeigename der Ordnerzeile (Test sucht via `HasText`/`GetByText`) |
| `ApplicationId` | `int` | Zuordnung zur Anwendung; wird beim Anlegen in `WorkspacesLayout.OnCreateEndpointGroupConfirmed` gesetzt |
| `Application` | `Application` | Navigation; wird in `WorkspacesSidebar.OnEndpointGroupSelected` für den Breadcrumb-Pfad gelesen |
| `ParentGroupId` | `int?` | Verschachtelung: `null` = Root-Ordner (Test legt Root-Ordner an); Baum gruppiert Kinder über `childrenByParent` |
| `ParentGroup` | `EndpointGroup?` | Navigation zur übergeordneten Gruppe |
| `RowVersion` | `byte[]` | Optimistic Concurrency |
| `Endpoints` | `ICollection<Endpoint>` | Endpunkte der Gruppe (Baum filtert `e.EndpointGroupId == group.Id`) |
| `ChildGroups` | `ICollection<EndpointGroup>` | Untergruppen |

## `Endpoint`

Datei: `src/Schnittstellenzentrale.Core/Models/Endpoint.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel; `0` = Neuanlage (steuert `PersistAsync`: `AddEndpointAsync` vs. `UpdateEndpointAsync`) |
| `Name` | `string` | Pflichtfeld; Speichern-Button ist bei leerem Namen deaktiviert (`EndpointPage.razor` Zeile 21) |
| `Method` | `HttpMethod` | HTTP-Methode; Default `GET` beim Anlegen (`WorkspacesLayout.razor` Zeile 274) |
| `RelativePath` | `string` | Test füllt `/api/test` über `GetByPlaceholder("Relativer Pfad")`; Anlage setzt `"/"` |
| `Body` / `BodyMode` | `string?` / `BodyMode` | Anlage setzt `BodyMode.None` |
| `ApplicationId` | `int` | Zuordnung; `HandleCreateEndpointRequested` setzt `args.Application.Id` |
| `EndpointGroupId` | `int?` | Ordner-Zuordnung; im Testpfad `null` (Kontextmenü der Anwendung → `Group = null`) — der Endpunkt landet **außerhalb** des Ordners |
| `RowVersion` | `byte[]` | Optimistic Concurrency (`DbUpdateConcurrencyException` → `ConcurrencyWarningDialog`) |
| `Headers` / `QueryParameters` | `ICollection<…>` | Werden bei Neuanlage einzeln persistiert (`CreateEndpointWithDetailsAsync`) |

## `Application`

Datei: `src/Schnittstellenzentrale.Core/Models/Application.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel; Schlüssel für `_expandedApplicationIds`, `_endpointGroups`-/`_endpoints`-Dictionaries |
| `Name` | `string` | Test-Lokator `SystemAppRow` sucht `HasText = "Schnittstellenzentrale"` (Systemanwendung) |
| `IsSystem` | `bool` | Systemanwendung `true`; sperrt Drag & Kontextmenü-Einträge teilweise (`disabled` bei Bearbeiten/Löschen) |
| `ApplicationGroupId` | `int?` | Gruppenzugehörigkeit; Systemanwendung liegt in der Systemgruppe |
| `Endpoints` / `EndpointGroups` | `ICollection<…>` | Werden lazy beim Expand über `ReloadApplicationDataAsync` nachgeladen |

## `ApplicationGroup`

Datei: `src/Schnittstellenzentrale.Core/Models/ApplicationGroup.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel; Schlüssel für `_expandedGroupIds` und `ExpandGroupAsync` |
| `Name` | `string` | Titel der `CollapsibleSection` (Systemgruppe heißt „Schnittstellenzentrale") |
| `IsSystem` | `bool` | Systemgruppe `true` (durch `SystemEntryInitializer`/`TestDatabaseSeeder` angelegt) |
| `Applications` | `ICollection<Application>` | Enthaltene Anwendungen (werden innerhalb der Section gerendert) |
