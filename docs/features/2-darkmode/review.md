# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

- [x] `ColorScheme` (Enum) — angelegt unter `src/Schnittstellenzentrale.Core/Enums/ColorScheme.cs`; Werte `Light` und `Dark` vorhanden
- [x] `IThemeService` (Interface) — angelegt unter `src/Schnittstellenzentrale.Core/Interfaces/IThemeService.cs`; Member `CurrentScheme`, `OnThemeChanged`, `SetTheme` vorhanden
- [x] `ThemeService` (Klasse, Service) — angelegt unter `src/Schnittstellenzentrale.Infrastructure/Services/ThemeService.cs`; implementiert `IThemeService`; nutzt `IJSRuntime` für `localStorage`-Zugriff; feuert `OnThemeChanged` nach tatsächlichem Wechsel
- [x] `ThemeToggle` (Blazor-Komponente) — angelegt unter `src/Schnittstellenzentrale/Components/Layout/ThemeToggle.razor`; injiziert `IThemeService`; rendert Toggle-Button mit Icon; ruft `ThemeService.SetTheme()` auf
- [x] `wwwroot/theme.js` — angelegt unter `src/Schnittstellenzentrale/wwwroot/theme.js`; exportiert `getStoredTheme`, `setStoredTheme`, `applyTheme`, `getAndApplyStoredTheme`
- [x] `App.razor` — `data-bs-theme`-Attribut wird beim initialen Laden auf `<html>` gesetzt und verhindert Flash of Unstyled Content
- [x] CSS Custom Properties in `wwwroot/app.css` — Variablen für `[data-bs-theme="light"]` und `[data-bs-theme="dark"]` definiert; alle sechs Theme-Variablen (`--sz-sidebar-bg-start/end`, `--sz-toprow-bg`, `--sz-nav-link-color`, `--sz-nav-icon-fill`, `--sz-nav-toprow-bg`) vorhanden
- [x] `MainLayout.razor.css` umgestellt — `.sidebar`-Hintergrundfarbe auf `var(--sz-sidebar-bg-start/end)` umgestellt; `.top-row`-Hintergrundfarbe auf `var(--sz-toprow-bg)` umgestellt; `#blazor-error-ui` auf `color-scheme: light dark` geändert
- [x] `NavMenu.razor.css` umgestellt — `.top-row` auf `var(--sz-nav-toprow-bg)` umgestellt; `.nav-item ::deep .nav-link` auf `var(--sz-nav-link-color)` umgestellt; SVG-Icons theme-adaptiv (dunkle Varianten über `[data-bs-theme="dark"]`-Selektoren)
- [x] `IThemeService`/`ThemeService` in `Program.cs` registriert — `AddScoped<IThemeService, ThemeService>()` vorhanden (analog zu `IStorageModeService`)
- [x] Feld `[Inject] IThemeService ThemeService` in `MainLayout` — vorhanden (`@inject IThemeService ThemeService`)
- [x] Methode `OnThemeChanged()` in `MainLayout` — vorhanden; ruft `InvokeAsync(StateHasChanged)` auf
- [x] `OnInitialized()` in `MainLayout` erweitert — registriert `OnThemeChanged`-Handler auf `IThemeService`
- [x] `Dispose()` in `MainLayout` erweitert — meldet `OnThemeChanged`-Handler ab
- [x] `ThemeToggle`-Komponente in die Top-Row von `MainLayout` eingebettet — `<ThemeToggle />` vorhanden
- [x] Testmethode `InitialTheme_IsLight_WhenNoStoredPreference` in `ThemeServiceTests` — vorhanden
- [x] Testmethode `InitialTheme_IsStoredValue_WhenPreferenceExists` in `ThemeServiceTests` — vorhanden
- [x] Testmethode `SetTheme_FiresOnThemeChanged` in `ThemeServiceTests` — vorhanden
- [x] Testmethode `SetTheme_DoesNotFire_WhenValueUnchanged` in `ThemeServiceTests` — vorhanden
- [x] Testmethode `SetTheme_PersistsValueToLocalStorage` in `ThemeServiceTests` — vorhanden

## Offene Aufgaben

Keine.

## Hinweise

- **`App.razor`: Inline-Script statt JS-Interop-Call.** Der Plan sah vor, `data-bs-theme` auf `<html>` via Blazor-JS-Interop zu setzen. Die Implementierung verwendet stattdessen ein eingebettetes `<script>`-Block im `<head>`, das `localStorage` direkt ausliest und das Attribut synchron setzt. Das Ergebnis (kein Flash of Unstyled Content) ist identisch; die Implementierungstechnik weicht vom Plan ab, ist aber in der Praxis der übliche und empfohlene Ansatz für FOUC-Prävention in Blazor.

- **`NavMenu.razor.css`: SVG-Icon-Anpassung via duplizierte Data-URIs statt CSS Custom Property auf `fill`.** Der Plan nannte „`fill`-Werte werden auf CSS Custom Properties umgestellt". Da `fill` in inline-Data-URI-SVGs nicht per CSS-Variable gesetzt werden kann, wurden stattdessen vollständige dunkle SVG-Kopien unter `[data-bs-theme="dark"]`-Selektoren hinterlegt. Die CSS-Variable `--sz-nav-icon-fill` ist in `app.css` definiert, wird in `NavMenu.razor.css` aber nicht für den `fill`-Wert der SVG-Bilder genutzt. Das funktionale Ziel (theme-adaptive Icons) ist erreicht; die technische Umsetzung weicht von der Planvorgabe ab.
