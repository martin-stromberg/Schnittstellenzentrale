# Blazor-Komponenten

## `ApplicationGroupTree.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor`

Injiziert: `IApplicationApiClient`, `IEndpointRepository`, `IStorageModeService`, `ICurrentUserService`, `NavigationManager`, `IJSRuntime`, `ILogger<ApplicationGroupTree>`, `IActivityLogService`.

### Betroffene Stelle: `ReloadApplicationDataAsync`

```csharp
private async Task ReloadApplicationDataAsync(int applicationId)
{
    _endpointGroups[applicationId] = await EndpointRepository.GetEndpointGroupsAsync(applicationId);
    _endpoints[applicationId] = await EndpointRepository.GetEndpointsAsync(applicationId);
}
```

`IEndpointRepository` wird nur in dieser Methode verwendet. Wird aufgerufen aus:
- `LoadDataAsync` (foreach über alle Anwendungen)
- SignalR-Handler `EndpointChanged` und `EndpointGroupChanged`

Alle anderen Datenzugriffe (Gruppen, Anwendungen) laufen bereits über `IApplicationApiClient`.

---

## `FolderContentView.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/FolderContentView.razor`

Injiziert: `IEndpointRepository` (einzige Injektion).

### Betroffene Stelle: `OnParametersSetAsync`

```csharp
protected override async Task OnParametersSetAsync()
{
    var all = await EndpointRepository.GetEndpointsAsync(EndpointGroup.ApplicationId);
    _endpoints = all.Where(e => e.EndpointGroupId == EndpointGroup.Id).ToList();
}
```

Parameter: `EndpointGroup EndpointGroup` (EditorRequired). Die Filterung nach `EndpointGroupId` erfolgt clientseitig.

---

## `ApplicationContentView.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationContentView.razor`

Injiziert: `IApplicationService`, `IEndpointRepository`, `ISwaggerImportService`, `IHealthCheckService`, `IApplicationApiClient`.

### Betroffene Stelle: `OnParametersSetAsync`

```csharp
protected override async Task OnParametersSetAsync()
{
    var endpoints = await EndpointRepository.GetEndpointsAsync(Application.Id);
    _endpointCount = endpoints.Count;
}
```

`IEndpointRepository` wird nur für diesen Zählwert (KPI) genutzt. Alle anderen Aktionen (DeleteApplication, Swagger-Import, Health-Check) verwenden andere Services.

---

## `EndpointPage.razor`
Datei: `src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor`

Injiziert: `IEndpointRepository`, `IEndpointExecutionService`, `IJSRuntime`, `NavigationManager`, `IStorageModeService`, `ISignalRNotificationService`.

### Betroffene Stellen

**`ForceSaveAsync`** – liest aktuellen `RowVersion`-Wert vor dem Überschreiben:
```csharp
var current = await EndpointRepository.GetEndpointByIdAsync(_model.Id);
if (current != null)
    _model.RowVersion = current.RowVersion;
```

**`PersistAsync`** – Add/Update:
```csharp
if (endpoint.Id == 0)
    await EndpointRepository.AddEndpointAsync(endpoint);
else
    await EndpointRepository.UpdateEndpointAsync(endpoint);
```

**`SendRequestAsync`** – Reload vor Ausführung:
```csharp
var refreshed = await EndpointRepository.GetEndpointByIdAsync(_model.Id);
```

`IEndpointRepository` wird an drei Stellen verwendet. `IEndpointExecutionService` ist nicht betroffen (bleibt direkter Service-Aufruf).
