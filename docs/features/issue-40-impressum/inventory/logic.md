# Logik – Bestandsaufnahme

## `WorkspacesSidebar`
Datei: `src/Schnittstellenzentrale/Components/Shared/WorkspacesSidebar.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OnInitialized()` | `protected override` | Liest den aktuellen Benutzernamen und setzt den Copyright-Text aus dem Assembly-Attribut |
| `RefreshTreeAsync()` | `internal async` | Delegiert `RefreshAsync()` an den `ApplicationGroupTree` |
| `ExpandGroup(int groupId)` | `internal` | Delegiert `ExpandGroupAsync(groupId)` an den `ApplicationGroupTree` |
| `OnApplicationSelected(int)` | `private async` | Lädt Application per `IApplicationApiClient`, baut den Navigationspfad auf und setzt die Workspace-Selektion |
| `OnSelectionCleared()` | `private async` | Setzt die Workspace-Selektion auf `null` |
| `OnEndpointSelected(Endpoint)` | `private async` | Baut den Navigationspfad für einen Endpunkt auf und setzt die Workspace-Selektion |
| `OnApplicationGroupSelected(ApplicationGroup)` | `private async` | Setzt die Workspace-Selektion auf die gewählte Gruppe |
| `OnEndpointGroupSelected(EndpointGroup)` | `private async` | Baut den Navigationspfad für eine Endpunktgruppe auf und setzt die Workspace-Selektion |
| `ForwardCreateGroupRequested()` | `private` | Delegiert `OnCreateCollectionRequested` nach außen |

**Relevanter Befund:** Der Footer der Sidebar enthält bereits einen statisch verdrahteten Link auf `/impressum`:

```razor
<a href="/impressum" class="sz-sidebar-footer-link">@L["WorkspacesSidebar_ImpressumLink"]</a>
```

Der Link wird immer gerendert — die bedingte Sichtbarkeit (abhängig von `IImpressumService`) ist noch nicht implementiert.

---

## `HistorySettings`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/HistorySettings.cs`

Referenzimplementierung für das Muster einer Konfigurationsklasse. Zeigt, wie Settings-Klassen im Infrastructure-Projekt angelegt und über `builder.Services.Configure<T>(...)` in `Program.cs` registriert werden.

| Eigenschaft | Typ | Standardwert | Beschreibung |
|-------------|-----|-------------|--------------|
| `DefaultPageSize` | `int` | `50` | Standard-Seitengröße für die History-Ansicht |
