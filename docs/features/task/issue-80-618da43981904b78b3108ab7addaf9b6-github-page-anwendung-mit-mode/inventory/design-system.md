# Design-System

## Dateien

| Datei | Inhalt |
|-------|--------|
| `src/Schnittstellenzentrale/wwwroot/app.css` | Eigenes `sz-*`-Klassensystem, 2639 Zeilen; `:root`- und `.dark`-Variablen, alle Layout-/Komponentenklassen |
| `src/Schnittstellenzentrale/wwwroot/shadcn/default-theme.css` | shadcn-Token-Set (oklch) für `:root` (light) und `.dark`; `@theme inline`-Mapping auf Tailwind-Namen |
| `src/Schnittstellenzentrale/wwwroot/shadcn/scrollbar.css` | `.scrollbar-thin` / `.scrollbar-none` Utilities in `@layer utilities` |
| `src/Schnittstellenzentrale/wwwroot/theme.js` | ES-Modul: `getStoredTheme`, `setStoredTheme`, `applyTheme`, `getAndApplyStoredTheme` |
| `src/Schnittstellenzentrale/wwwroot/theme-init.js` | Einzeiler: `import('./theme.js').then(m => m.getAndApplyStoredTheme())` |
| `src/Schnittstellenzentrale/Components/App.razor` | Lädt `shadcn/default-theme.css`, `shadcn/scrollbar.css`, `app.css`, `Schnittstellenzentrale.styles.css` (Scoped CSS) und `theme-init.js` als `<script type="module">` im `<head>`; `favicon.png` verlinkt |
| `src/Schnittstellenzentrale/Components/Layout/ThemeToggle.razor` | Umschalt-Button in der TopBar (Sonne/Mond-Glyphen), ruft `IThemeService.SetTheme` |
| `src/Schnittstellenzentrale.Infrastructure/Services/ThemeService.cs` | Implementiert `IThemeService` per JSInterop (`import "./theme.js"`); `CurrentScheme`, `OnThemeChanged`, `SetTheme`, `InitializeAsync` |
| `src/Schnittstellenzentrale.Core/Enums/ColorScheme.cs` | Enum `Light`, `Dark` |
| `src/Schnittstellenzentrale.Core/Interfaces/IThemeService.cs` | Contract: `ColorScheme CurrentScheme`, `event Action? OnThemeChanged`, `Task SetTheme(ColorScheme)`, `Task InitializeAsync()` |
| `.claude/css-conventions.md` | Regelwerk: Bootstrap **nicht** geladen (obwohl unter `wwwroot/lib/bootstrap/` eingecheckt), `sz-*`-Klassen müssen in `app.css` oder `.razor.css` definiert sein, PostToolUse-Hook prüft |

## Dark-Mode-Mechanik

- **Trigger:** `.dark`-Klasse auf `document.documentElement` (`<html>`) — kein `data-theme`-Attribut (explizit in `.claude/css-conventions.md` dokumentiert).
- **Persistenz:** `localStorage`-Schlüssel **`colorScheme`** (Werte `"Light"`/`"Dark"`, `applyTheme` vergleich lowercase `'dark'`). Schlüssel steht **nicht** in `LocalStorageKeys.cs` (dort nur `storageMode`, `activityLogDisplayMode`, `activityLogPanelHeight`, `selectedEnvironmentId_{mode}`), sondern hartkodiert in `theme.js`.
- **Initialisierung:** `theme-init.js` (Modul-Import im `<head>`) → `getAndApplyStoredTheme()` → `applyTheme(stored ?? 'light')`. **Ohne gespeicherten Wert wird immer Light angewendet — `prefers-color-scheme` wird in der App nirgends ausgewertet** (`grep` über `src/` findet es nur in den Bootstrap-Map-Dateien).
- **C#-Seite:** `ThemeService.SetTheme` persistiert via `setStoredTheme` + `applyTheme`; `ThemeToggle.razor` rendert in `TopBar.razor`.

## Token / Variablen

### shadcn (`default-theme.css`)

- Light (`:root`): `--radius: 0.625rem`; `--background oklch(1 0 0)`, `--foreground oklch(0.145 0 0)`, `--card`, `--primary oklch(0.205 0 0)`, `--primary-foreground`, `--secondary`, `--muted`, `--muted-foreground oklch(0.556 0 0)`, `--accent`, `--destructive oklch(0.577 0.245 27.325)`, `--border oklch(0.922 0 0)`, `--input`, `--ring`, `--chart-1..5`, `--sidebar*`-Satz.
- `.dark`: invertiertes Set (`--background oklch(0.145 0 0)`, `--foreground oklch(0.985 0 0)`, `--card oklch(0.205 0 0)`, `--primary oklch(0.922 0 0)`, `--border oklch(1 0 0 / 10%)`, `--accent oklch(0.371 0 0)`, …).
- `@theme inline` mappt `var(--color-*)`/`--radius-*` für Tailwind-Klassen.

### sz-Variablen (`app.css`, Zeilen 1–19)

| Variable | Light | Dark |
|----------|-------|------|
| `--sz-sidebar-bg-start` | `rgb(5, 39, 103)` | `#0d1b2a` |
| `--sz-sidebar-bg-end` | `#3a0647` | `#1a0a1e` |
| `--sz-toprow-bg` | `#f7f7f7` | `#1e1e2e` |
| `--sz-toprow-border` | `#d6d5d5` | `#3a3a4a` |
| `--sz-nav-link-color` | `#d7d7d7` | `#c0c0d0` |
| `--sz-nav-icon-fill` | `white` | `#c0c0d0` |
| `--sz-nav-toprow-bg` | `rgba(0,0,0,0.4)` | `rgba(0,0,0,0.6)` |

**Befund:** Alle sieben `--sz-*`-Variablen sind definiert, werden aber im gesamten `src/` **nicht referenziert** (grep: nur die Definitionszeilen selbst) — Relikte des früheren NavMenu-Layouts. Der dokumentierte Sidebar-Gradient `rgb(5,39,103)` → `#3a0647` ist damit nur noch als Farbreferenz vorhanden, nicht aktives Styling.

### Typografie

- Basis-Font (app.css Z. 25): `'Helvetica Neue', Helvetica, Arial, sans-serif`
- Mono-Font (`.sz-info-url-value`): `'JetBrains Mono', 'Cascadia Code', 'Consolas', monospace`
- Keine externen Font-Imports (kein Google Fonts o. ä.).

## app.css — Struktur (Sections)

`:root`/`.dark` → App Shell / TopBar → Workspaces-Layout → Content-Panel-Scroll → Breadcrumb → Content-Views + Error/Alert → Hero (`.sz-hero`, `.sz-hero-bg-pattern` mit `repeating-linear-gradient`-Raster) → Dashboard-Grid/KPI-Cards → Info-Block/URL-Cards → Links-Manager → Formularelemente (`.sz-input`, `.sz-btn-secondary`, `.sz-btn-icon`) → Endpunkttabelle (`.sz-table`, `.sz-method-badge--{get,post,put,patch,delete}`) → History → leere Ansicht → Buttons → Icons → „Bootstrap-Shims (für noch nicht migrierte Komponenten)" → Endpoint-Seite → Panel-Tabellen → Formulargruppen → Body-/Response-Panels → Overlay-Dialog (`.sz-overlay`/`.sz-dialog`) → Alert-Blöcke → Tree-Sidebar (`.sz-tree-*`, `.dark .sz-tree-row:hover` als Beispiel für explizite `.dark`-Selektoren) → Settings-Bereich.

- **Keine `@media`-Queries** in `app.css` oder den drei `.razor.css`-Dateien — die App ist eine Desktop-Fullscreen-Shell (`html, body { overflow: hidden }`, `height: 100vh`); Responsivität ist dort nicht implementiert.
- Scoped CSS nur in drei Dateien: `ImpressumPage.razor.css`, `ActivityLogPanel.razor.css`, `ImportDialog.razor.css`.
- `color-mix(in srgb, …)` wird häufig für transparente Akzentflächen genutzt.
- Bootstrap-Dateien liegen unter `wwwroot/lib/bootstrap/` (vollständige Distribution), sind aber **nicht** per `<link>` in `App.razor` eingebunden — `wwwroot` liefert sie nur statisch aus.
