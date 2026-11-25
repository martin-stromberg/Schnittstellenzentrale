# Interfaces

## `IApplicationApiClient`

Datei: `src/Schnittstellenzentrale.Core/Interfaces/IApplicationApiClient.cs`

Einziger erlaubter Datenzugriff aus UI-Komponenten (API-First-Prinzip). Im Testpfad durchlaufene Methoden:

| Methode | Parameter | Rückgabewert | Zweck im Testfluss |
|---------|-----------|--------------|---------------------|
| `GetGroupsAsync` | `StorageMode storageMode`, `string owner` | `Task<IList<ApplicationGroup>>` | `ApplicationGroupTree.LoadDataAsync` — initiales Baum-Laden (inkl. Systemgruppe) |
| `GetUngroupedApplicationsAsync` | `StorageMode`, `string` | `Task<IList<Application>>` | `LoadDataAsync` — ungruppierte Anwendungen |
| `GetEndpointGroupsAsync` | `int applicationId` | `Task<IList<EndpointGroup>>` | `ReloadApplicationDataAsync` beim Expand der Anwendungszeile — liefert die Ordnerzeile „Initial-Collapsed-Ordner" |
| `GetEndpointsAsync` | `int applicationId`, `int? endpointGroupId` | `Task<IList<Endpoint>>` | `ReloadApplicationDataAsync` — Endpunkte der Anwendung |
| `AddEndpointGroupAsync` | `EndpointGroup group` | `Task<EndpointGroup>` | `OnCreateEndpointGroupConfirmed` — „Ordner anlegen" |
| `AddEndpointAsync` | `Endpoint endpoint` | `Task<Endpoint>` | `HandleCreateEndpointRequested` / `EndpointPage.CreateEndpointWithDetailsAsync` — „Endpunkt anlegen" bzw. Speichern |
| `GetEndpointByIdAsync` | `int id` | `Task<Endpoint?>` | `HandleCreateEndpointRequested` — Reload nach Anlage; `EndpointPage` beim Senden/ForceSave |
| `GetApplicationByIdAsync` | `int id` | `Task<Application?>` | Breadcrumb-Pfad in `HandleCreateEndpointRequested`; `WorkspacesSidebar.OnApplicationSelected` |
| `GetGroupByIdAsync` | `int id` | `Task<ApplicationGroup?>` | Breadcrumb-Pfad (Gruppenebene) |
| `GetEndpointGroupByIdAsync` | `int id` | `Task<EndpointGroup?>` | Breadcrumb-Pfad (Ordnerebene, nur bei `EndpointGroupId != null`) |
| `UpdateEndpointAsync` | `Endpoint endpoint` | `Task<Endpoint>` | `EndpointPage.PersistAsync` bei Bestands-Endpunkten (`Id != 0`) |
| `UpdateEndpointGroupAsync` / `DeleteEndpointGroupAsync` / `DeleteEndpointAsync` | jeweils Modell/`int id` | `Task<…>`/`Task` | Ordner-Umbenennen/-Löschen, Endpunkt-Löschen (nicht im Testpfad, gleiche Komponenten) |
| `AddHeaderAsync` / `AddQueryParameterAsync` | Modell | `Task<…>` | `CreateEndpointWithDetailsAsync` bei Neuanlage mit Details |
| `UpdateApplicationAsync` / `AddApplicationAsync` / weitere | — | — | Drag&Drop-Verschieben, „Neue Anwendung" (in anderen Tests der Klasse genutzt) |

## `Xunit.v3.ITraitAttribute` (extern, xunit.v3)

Von den Repo-Attributen `OsInterfaceAttribute`, `OsInterfaceFactAttribute`, `OsInterfaceTheoryAttribute` und `FlakyAttribute` implementiert.

| Methode | Parameter | Rückgabewert | Zweck |
|---------|-----------|--------------|-------|
| `GetTraits` | — | `IReadOnlyCollection<KeyValuePair<string,string>>` | Liefert das Trait-Paar selbst (`("Category", "OsInterface")` bzw. `("Category", "Flaky")`); xUnit v3 kennt keinen `ITraitDiscoverer` mehr, daher beschreiben Attribute ihre Traits direkt |
