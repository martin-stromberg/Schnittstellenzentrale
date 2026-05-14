# Dark Mode — API

## Übersicht

Die Theme-Infrastruktur ist über das `IThemeService`-Interface zugänglich. Blazor-Komponenten können den Service per Dependency Injection einbinden und das aktive Farbschema lesen sowie ändern.

## Interface `IThemeService`

Namespace: `Schnittstellenzentrale.Core.Interfaces`

### `CurrentScheme`

**Beschreibung:** Gibt das aktuell aktive Farbschema zurück.

| Eigenschaft | Typ | Beschreibung |
|-------------|-----|--------------|
| `CurrentScheme` | `ColorScheme` | Aktuelles Farbschema; Standardwert: `ColorScheme.Light` |

### `OnThemeChanged`

**Beschreibung:** Wird ausgelöst, wenn das Farbschema geändert wurde. Komponenten abonnieren dieses Event, um sich nach einem Theme-Wechsel neu zu rendern.

| Member | Typ | Beschreibung |
|--------|-----|--------------|
| `OnThemeChanged` | `event Action?` | Wird nach jedem erfolgreichen Aufruf von `SetTheme` mit einem neuen Wert ausgelöst. |

### `InitializeAsync()`

**Beschreibung:** Liest den gespeicherten Farbschema-Wert aus `localStorage` und setzt `CurrentScheme` entsprechend. Muss einmalig nach dem ersten Render einer Komponente aufgerufen werden, damit der persistierte Wert eingelesen wird.

**Parameter:** keine

**Rückgabe:** `Task`

**Hinweis:** `MainLayout` ruft diese Methode in `OnAfterRenderAsync(firstRender: true)` auf. Andere Komponenten müssen sie nicht erneut aufrufen.

### `SetTheme(ColorScheme scheme)`

**Beschreibung:** Wechselt das aktive Farbschema, persistiert die Auswahl in `localStorage` und löst `OnThemeChanged` aus.

**Parameter:**

| Name | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| `scheme` | `ColorScheme` | Ja | Das neue Farbschema (`ColorScheme.Light` oder `ColorScheme.Dark`). |

**Rückgabe:** `Task`

**Verhalten:** Wenn `scheme` mit dem aktuellen `CurrentScheme` identisch ist, wird weder `localStorage` geschrieben noch `OnThemeChanged` ausgelöst.

**Beispiel:**
```csharp
@inject IThemeService ThemeService

await ThemeService.SetTheme(ColorScheme.Dark);
```

## Enum `ColorScheme`

Namespace: `Schnittstellenzentrale.Core.Enums`

| Wert | Beschreibung |
|------|--------------|
| `Light` | Helles Farbschema (Standard) |
| `Dark` | Dunkles Farbschema |

## JavaScript-Modul `theme.js`

Das Modul wird ausschliesslich von `ThemeService` via JS-Interop aufgerufen. Es ist kein öffentliches API, kann aber bei Bedarf direkt in anderen JS-Modulen importiert werden.

| Funktion | Parameter | Rückgabe | Beschreibung |
|----------|-----------|----------|--------------|
| `getStoredTheme()` | — | `string \| null` | Liest den gespeicherten Schemawert aus `localStorage` (Schlüssel: `colorScheme`). |
| `setStoredTheme(scheme)` | `scheme: string` | `void` | Schreibt den Schemawert in `localStorage`. |
| `applyTheme(scheme)` | `scheme: string` | `void` | Setzt `data-bs-theme` am `<html>`-Element. |
| `getAndApplyStoredTheme()` | — | `string \| null` | Liest den gespeicherten Wert, setzt `data-bs-theme` (Fallback: `'light'`) und gibt den gespeicherten Wert zurück. Wird von `theme-init.js` aufgerufen. |
