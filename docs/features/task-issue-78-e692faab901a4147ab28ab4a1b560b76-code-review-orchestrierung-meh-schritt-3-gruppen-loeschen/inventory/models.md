# Bestandsaufnahme: Datenmodelle

## `ApplicationGroup`
Datei: `src/Schnittstellenzentrale.Core/Models/ApplicationGroup.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Pflicht, max. 200 |
| `IsSystem` | `bool` | Systemgruppen-Schutz (Delete/Update → 403) |
| `Description` | `string?` | optional |
| `Subtitle` | `string?` | optional |
| `IconData` | `byte[]?` | optional |
| `RowVersion` | `byte[]` | Concurrency-Token |
| `Applications` | `ICollection<Application>` | enthaltene Anwendungen; FK `ApplicationGroupId`, `DeleteBehavior.SetNull` |

## `Application`
Datei: `src/Schnittstellenzentrale.Core/Models/Application.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Pflicht, max. 200 |
| `IsSystem` | `bool` | Systemkennzeichen |
| `ApplicationGroupId` | `int?` | FK zur Gruppe; `null` = ungruppiert |
| `ApplicationGroup` | `ApplicationGroup?` | Navigation |
| `Endpoints` | `ICollection<Endpoint>` | `DeleteBehavior.Cascade` |
| `EndpointGroups` | `ICollection<EndpointGroup>` | `DeleteBehavior.Cascade` |
| `Links` | `ICollection<ApplicationLink>` | `DeleteBehavior.Cascade` |
| `Owner` | `string?` | Besitzerfilter im User-Modus |
| `RowVersion` | `byte[]` | Concurrency-Token |
| weitere | … | `BaseUrl`, `Description`, `InterfaceUrl`, `InterfaceType`, `Subtitle`, `IconData` |

## `EndpointGroup`
Datei: `src/Schnittstellenzentrale.Core/Models/EndpointGroup.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel |
| `ApplicationId` | `int` | FK zur Anwendung (`Cascade` über `Application`) |
| `ParentGroupId` | `int?` | Selbstreferenz für verschachtelte Gruppen |
| `ChildGroups` | `ICollection<EndpointGroup>` | **Wichtig:** `DeleteBehavior.Restrict` — Kindgruppen verhindern das Löschen der Elterngruppe, solange sie existieren |
| `Endpoints` | `ICollection<Endpoint>` | `DeleteBehavior.Cascade` |

## `Endpoint` (relevante Teile)
Datei: `src/Schnittstellenzentrale.Core/Models/Endpoint.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `ApplicationId` | `int` | FK (`Cascade` über `Application`) |
| `EndpointGroupId` | `int?` | FK (`Cascade` über `EndpointGroup`) |
| `Headers` / `QueryParameters` | `ICollection<…>` | `DeleteBehavior.Cascade` |

## `EndpointCallHistoryEntry` (relevante Teile)
`AppDbContext.cs` Zeilen 180–193: `ApplicationId` und `EndpointId` sind `DeleteBehavior.SetNull` — Historieneinträge überleben das Löschen von Anwendung/Endpunkt.

## EF-Kaskaden (AppDbContext.cs, OnModelCreating)

| Beziehung | Verhalten |
|-----------|-----------|
| `ApplicationGroup` → `Applications` | `SetNull` (bisheriges „detach"-Verhalten beim Gruppen-Delete) |
| `Application` → `Endpoints` | `Cascade` |
| `Application` → `EndpointGroups` | `Cascade` |
| `Application` → `Links` | `Cascade` |
| `EndpointGroup` → `Endpoints` | `Cascade` |
| `EndpointGroup` → `ChildGroups` | `Restrict` (!) |
| `Endpoint` → `Headers` / `QueryParameters` | `Cascade` |
| `EndpointCallHistoryEntry` → `Application` / `Endpoint` | `SetNull` |
