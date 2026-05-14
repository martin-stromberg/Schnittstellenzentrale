# UI-Komponenten – Bestandsaufnahme

## `MainLayout`
Datei: `src/Schnittstellenzentrale/Components/Layout/MainLayout.razor`
CSS: `src/Schnittstellenzentrale/Components/Layout/MainLayout.razor.css`

Haupt-Layout der Anwendung. Enthält bereits den Modus-Selektor (`StorageMode`-Dropdown) in der Top-Row sowie das Abonnement-Muster für Service-Events.

Relevante Strukturmerkmale:
- Root-Element ist `<div class="page">` — kein `<html>`- oder `<body>`-Attribut wird hier gesetzt.
- Die Top-Row enthält ein `<select>`-Element für den Storage-Modus-Wechsel (Vorbild für Theme-Umschalter-Platzierung).
- Implementiert `IDisposable` zur sauberen Event-Abmeldung.

CSS-Eigenschaften (`MainLayout.razor.css`):

| Selektor | Relevante Deklarationen |
|----------|------------------------|
| `.sidebar` | `background-image: linear-gradient(180deg, rgb(5, 39, 103) 0%, #3a0647 70%)` — hart kodierte Farbe, nicht via CSS-Variable. |
| `.top-row` | `background-color: #f7f7f7` — hart kodierte Hellfarbe, kein Dark-Mode-Gegenstück. |
| `#blazor-error-ui` | `color-scheme: light only` — explizit auf Light Mode festgelegt. |

---

## `NavMenu`
Datei: `src/Schnittstellenzentrale/Components/Layout/NavMenu.razor`
CSS: `src/Schnittstellenzentrale/Components/Layout/NavMenu.razor.css`

Navigationsleiste. Derzeit statisches HTML ohne Theme-Logik.

Relevante CSS-Eigenschaften (`NavMenu.razor.css`):

| Selektor | Relevante Deklarationen |
|----------|------------------------|
| `.top-row` | `background-color: rgba(0,0,0,0.4)` — dunkler Hintergrund (passt zum aktuellen Gradient-Sidebar). |
| `.nav-item ::deep .nav-link` | `color: #d7d7d7` — hart kodierte helle Schriftfarbe. |
| `.bi-*` Icon-Klassen | SVG-Icons mit hart kodiertem `fill='white'` — nicht theme-adaptiv. |

---

## `App.razor`
Datei: `src/Schnittstellenzentrale/Components/App.razor`

Root-HTML-Dokument der Blazor-App. Das `<html>`-Tag hat kein `lang`-Attribut außer `lang="en"` und kein `data-bs-theme`-Attribut. Bootstrap wird über `lib/bootstrap/dist/css/bootstrap.min.css` eingebunden (Version 5.3.3 — unterstützt nativen Dark Mode via `data-bs-theme`).

| Element | Aktueller Zustand |
|---------|------------------|
| `<html>` | Kein `data-bs-theme`-Attribut vorhanden. |
| Bootstrap-Version | **5.3.3** — Dark Mode via `data-bs-theme` nativ unterstützt. |
| `theme.js` | Nicht vorhanden. |

---

## `wwwroot/app.css`
Datei: `src/Schnittstellenzentrale/wwwroot/app.css`

Zentrales Anwendungs-Stylesheet. Enthält keine CSS Custom Properties (CSS-Variablen) für Farbschemata. Alle Farben sind hart kodiert.

| Aspekt | Befund |
|--------|--------|
| CSS Custom Properties für Themes | Nicht vorhanden. |
| Dark-Mode-spezifische Regeln (`@media (prefers-color-scheme: dark)`) | Nicht vorhanden. |
| `.form-floating`-Platzhalter nutzt `var(--bs-secondary-color)` | Bootstrap-Variable bereits in Verwendung — zeigt Kompatibilität mit BS-Variablen. |

---

## `ThemeToggle`
Noch nicht vorhanden. Kein entsprechendes `.razor`-File in `Components/`.
