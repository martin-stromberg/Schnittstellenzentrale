# UI-Komponenten

## `ImpressumPage`
Datei: `src/Schnittstellenzentrale/Components/Pages/ImpressumPage.razor`

Route: `/impressum`

| Feld / Parameter | Typ | Beschreibung |
|-----------------|-----|--------------|
| `_isAvailable` | `private bool` | Wird in `OnInitializedAsync` mit `ImpressumService.IsAvailable()` gesetzt |
| `_htmlContent` | `private string?` | Wird in `OnInitializedAsync` mit `ImpressumService.GetContentAsHtmlAsync()` befüllt |

Injizierte Services: `IStringLocalizer<SharedResources>`, `IImpressumService`, `INavigationStateService`

**Sprachverhalten:** Die Komponente übergibt **keine Sprachinformation** an `ImpressumService`. Der Aufruf `ImpressumService.IsAvailable()` und `ImpressumService.GetContentAsHtmlAsync()` erfolgt ohne Kulturkontext. `OnInitializedAsync` wird im Blazor-Server-Kontext während des initialen HTTP-Requests ausgeführt, zu dem `CultureInfo.CurrentUICulture` durch `RequestLocalizationMiddleware` bereits gesetzt ist — dieser Wert wird aber aktuell nicht genutzt.

Fehlerbehandlung in `OnInitializedAsync`: `IOException`, `UnauthorizedAccessException` und `FileNotFoundException` werden abgefangen; bei Ausnahme wird `_htmlContent = null` und `_isAvailable = false` gesetzt.

---

## `WorkspacesSidebar`
Datei: `src/Schnittstellenzentrale/Components/Shared/WorkspacesSidebar.razor`

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `_impressumAvailable` | `private bool` | Wird in `OnInitialized` mit `ImpressumService.IsAvailable()` gesetzt |

Injizierte Services: `IImpressumService` (u. a.), `IStringLocalizer<SharedResources>`

**Sprachverhalten:** `OnInitialized` (synchron, kein `async`) ruft `ImpressumService.IsAvailable()` ohne Sprachkontext auf. Der Footer-Link `<a href="/impressum">` wird nur gerendert, wenn `_impressumAvailable == true`. Eine sprachabhängige Sichtbarkeit des Links ist nicht implementiert.
