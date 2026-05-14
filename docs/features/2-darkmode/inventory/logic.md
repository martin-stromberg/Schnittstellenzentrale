# Logik – Bestandsaufnahme

## `StorageModeService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/StorageModeService.cs`

Implementiert `IStorageModeService`. Dient als direktes Referenzmuster für den zu erstellenden `ThemeService`.

| Methode / Eigenschaft | Sichtbarkeit | Kurzbeschreibung |
|-----------------------|-------------|------------------|
| `CurrentMode` | `public` | Eigenschaft; gibt den aktuellen `StorageMode`-Wert zurück. Initial: `StorageMode.Team`. |
| `SetMode(StorageMode mode)` | `public` | Setzt den neuen Modus; gibt keine Aktion aus, wenn der Wert bereits gesetzt ist; feuert `OnModeChanged`. |

Publizierte Events:
- `OnModeChanged` (`event Action?`) — wird nach jedem tatsächlichen Moduswechsel ausgelöst.

---

## `MainLayout`
Datei: `src/Schnittstellenzentrale/Components/Layout/MainLayout.razor`

Blazor-Layout-Komponente; abonniert `IStorageModeService.OnModeChanged` und aktualisiert die Ansicht via `StateHasChanged`. Dieses Muster ist das direkte Vorbild für die zukünftige Theme-Integration.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OnInitialized()` | `protected override` | Registriert `OnModeChanged`-Handler auf `IStorageModeService`. |
| `OnStorageModeChanged(ChangeEventArgs e)` | `private` | Liest den neuen `StorageMode` aus dem `<select>`-Event und ruft `StorageModeService.SetMode()` auf. |
| `OnModeChanged()` | `private` | Callback für das `OnModeChanged`-Event; ruft `InvokeAsync(StateHasChanged)` auf. |
| `Dispose()` | `public` | Meldet den `OnModeChanged`-Handler ab (`IDisposable`). |

Abonnierte Events:
- `IStorageModeService.OnModeChanged`

---

## `Program.cs`
Datei: `src/Schnittstellenzentrale/Program.cs`

Registriert `IStorageModeService` als Scoped-Service — das Muster, das für `IThemeService`/`ThemeService` übernommen werden soll.

| Registrierung | Scope |
|---------------|-------|
| `IStorageModeService` → `StorageModeService` | `Scoped` |
