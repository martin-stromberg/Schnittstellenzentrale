# Umsetzungsplan: Frischeres Layout mit Dark Mode

## Übersicht

Die Blazor Server App „Schnittstellenzentrale" erhält einen schaltbaren Dark Mode auf Basis des nativen Bootstrap-5.3-Mechanismus (`data-bs-theme`). Dazu werden ein `ColorScheme`-Enum, ein `IThemeService`-Interface, eine `ThemeService`-Implementierung sowie eine `ThemeToggle`-Komponente neu angelegt. Bestehende CSS-Dateien (`app.css`, `MainLayout.razor.css`, `NavMenu.razor.css`) werden von hart kodierten Farben auf CSS Custom Properties umgestellt; `MainLayout` und `App.razor` werden um die Theme-Steuerung erweitert. Die Präferenz des Benutzers wird via JS-Interop in `localStorage` persistiert.

---

## Neue Klassen

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `ColorScheme` | Enum | Repräsentiert das aktive Farbschema; Werte: `Light`, `Dark`. |
| `IThemeService` | Interface | Definiert den Zugriff auf das aktive `ColorScheme`, das `OnThemeChanged`-Event und die Setter-Methode. |
| `ThemeService` | Klasse (Service) | Implementiert `IThemeService`; hält das aktive `ColorScheme` als Scoped-DI-Instanz; liest und schreibt die Präferenz via JS-Interop in `localStorage`. |
| `ThemeToggle` | Blazor-Komponente | Eigenständiger Hell/Dunkel-Umschalter (Toggle-Button mit Icon); wird in die Top-Row von `MainLayout` eingebettet. |
| `ThemeServiceTests` | Testklasse | Unit-Tests für `ThemeService` (Initialisierung, Wechsel, Persistierung). |

---

## Änderungen an bestehenden Klassen

### `App.razor` (Blazor-Komponente)

- **Geänderte Eigenschaften:** Das `<html>`-Tag erhält ein dynamisches `data-bs-theme`-Attribut, das den initialen Farbschema-Wert aus `localStorage` (via JS-Interop) setzt — verhindert einen Flash of Unstyled Content beim ersten Render.

### `MainLayout` (Blazor-Komponente)

- **Neue Eigenschaften:** `[Inject] IThemeService ThemeService` — injizierter Service für das aktive Farbschema.
- **Neue Methoden:** `OnThemeChanged()` — Callback für das `IThemeService.OnThemeChanged`-Event; ruft `InvokeAsync(StateHasChanged)` auf (analog zu `OnModeChanged()`).
- **Geänderte Methoden:** `OnInitialized()` — registriert zusätzlich den `OnThemeChanged`-Handler auf `IThemeService`. `Dispose()` — meldet zusätzlich den `OnThemeChanged`-Handler ab.
- **Neue Event-Handler:** Abonniert `IThemeService.OnThemeChanged` zum Aktualisieren der Ansicht.

### `MainLayout.razor.css` (CSS)

- **Geänderte Deklarationen:** `.sidebar`-Hintergrundfarbe (derzeit `linear-gradient` mit hart kodierten Farbwerten) und `.top-row`-Hintergrundfarbe (derzeit `#f7f7f7`) werden auf CSS Custom Properties umgestellt, die je nach aktivem Theme (`[data-bs-theme="light"]` / `[data-bs-theme="dark"]`) unterschiedliche Werte annehmen. `.blazor-error-ui` wird von `color-scheme: light only` auf `color-scheme: light dark` geändert.

### `NavMenu.razor.css` (CSS)

- **Geänderte Deklarationen:** `.top-row`-Hintergrundfarbe, `.nav-item ::deep .nav-link`-Schriftfarbe und die SVG-Icon-`fill`-Werte (`fill='white'`) werden von hart kodierten Werten auf CSS Custom Properties umgestellt, die im Dark- und Light-Kontext jeweils korrekte Werte liefern.

### `wwwroot/app.css` (CSS)

- **Neue Deklarationen:** CSS Custom Properties für beide Farbschemata werden als Wurzeldeklarationen unter den Selektoren `[data-bs-theme="light"]` und `[data-bs-theme="dark"]` (alternativ `:root` und `[data-bs-theme="dark"]`) angelegt. Bestehende hart kodierte Farbwerte werden durch die entsprechenden Custom-Property-Referenzen ersetzt.

### `Program.cs` (Anwendungskonfiguration)

- **Neue Registrierung:** `IThemeService` → `ThemeService` als `Scoped`-Service (analog zur bestehenden `IStorageModeService`-Registrierung).

---

## Umsetzungsreihenfolge

1. **`ColorScheme`-Enum anlegen** (`src/Schnittstellenzentrale.Core/Enums/ColorScheme.cs`) — muss vor `IThemeService` und `ThemeService` existieren.
2. **`IThemeService`-Interface anlegen** (`src/Schnittstellenzentrale.Core/Interfaces/IThemeService.cs`) — muss vor `ThemeService` existieren.
3. **`wwwroot/theme.js` anlegen** — JS-Interop-Modul zum Lesen und Schreiben der Theme-Präferenz in `localStorage` sowie zum Setzen des `data-bs-theme`-Attributs auf `<html>`. Wird von `ThemeService` und `App.razor` benötigt.
4. **`ThemeService` anlegen** (`src/Schnittstellenzentrale.Infrastructure/Services/ThemeService.cs`) — implementiert `IThemeService`; nutzt `IJSRuntime` für `localStorage`-Zugriff.
5. **`IThemeService`/`ThemeService` in `Program.cs` registrieren** — Scoped-Registrierung, analog zu `IStorageModeService`.
6. **`App.razor` anpassen** — `data-bs-theme`-Attribut auf `<html>` via JS-Interop-Call aus `localStorage` initial setzen.
7. **CSS Custom Properties in `wwwroot/app.css` einführen** — Variablen für Light- und Dark-Theme definieren; bestehende hart kodierte Farben ersetzen.
8. **`MainLayout.razor.css` umstellen** — `.sidebar` und `.top-row` auf CSS Custom Properties umstellen; `color-scheme`-Deklaration bei `#blazor-error-ui` anpassen.
9. **`NavMenu.razor.css` umstellen** — `.top-row`, `.nav-link` und SVG-Icon-`fill` auf CSS Custom Properties umstellen.
10. **`ThemeToggle`-Komponente anlegen** (`src/Schnittstellenzentrale/Components/Layout/ThemeToggle.razor`) — injiziert `IThemeService`; rendert einen Toggle-Button; ruft `ThemeService.SetTheme()` auf.
11. **`MainLayout` erweitern** — `IThemeService` injizieren; `OnThemeChanged`-Handler registrieren und abmelden; `ThemeToggle`-Komponente in die Top-Row einbetten.
12. **`ThemeServiceTests` anlegen** (`src/Schnittstellenzentrale.Tests/Services/ThemeServiceTests.cs`) — Unit-Tests für `ThemeService`.

---

## Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `InitialTheme_IsLight_WhenNoStoredPreference` | `ThemeServiceTests` | `ThemeService` liefert `ColorScheme.Light` als Standard, wenn `localStorage` keinen Wert enthält. |
| `InitialTheme_IsStoredValue_WhenPreferenceExists` | `ThemeServiceTests` | `ThemeService` liest den gespeicherten `ColorScheme`-Wert aus `localStorage` korrekt ein. |
| `SetTheme_FiresOnThemeChanged` | `ThemeServiceTests` | Nach `SetTheme()` mit einem neuen Wert wird `OnThemeChanged` ausgelöst. |
| `SetTheme_DoesNotFire_WhenValueUnchanged` | `ThemeServiceTests` | `OnThemeChanged` wird nicht ausgelöst, wenn `SetTheme()` mit dem bereits aktiven Wert aufgerufen wird. |
| `SetTheme_PersistsValueToLocalStorage` | `ThemeServiceTests` | Nach `SetTheme()` wird der neue Wert via JS-Interop in `localStorage` geschrieben. |

---

## Offene Punkte

1. **Umfang des visuellen Redesigns:** Die Anforderung nennt ein „frischeres Layout" über den Dark Mode hinaus (Farbpalette, Typografie, Abstände). Konkrete Designvorgaben (Mockups, Styleguide, Bootswatch-Theme) liegen noch nicht vor — dieser Plan beschränkt sich auf die Theme-Infrastruktur und die Umstellung auf CSS Custom Properties. Strukturelle Layout-Änderungen sind separat zu beauftragen.

2. **System-Präferenz (`prefers-color-scheme`):** Es ist ungeklärt, ob beim erstmaligen Besuch (kein `localStorage`-Eintrag) automatisch die Betriebssystem-/Browser-Präferenz übernommen werden soll. Der Plan sieht `Light` als statischen Fallback vor; bei Bedarf ist `theme.js` um einen `matchMedia`-Check zu erweitern.

3. **Umschalter-Position:** Die Platzierung des `ThemeToggle` in der Top-Row (neben dem Storage-Modus-Dropdown) ist als Vorzugslösung gewählt, da das Muster dort bereits etabliert ist. Eine alternative Platzierung im `NavMenu` ist ohne Planänderung möglich.

4. **Abdeckung weiterer Komponenten im Dark Mode:** Ob alle Feature-Komponenten (`ApplicationCard`, `EndpointEditor`, `EndpointExecutionPanel`, Dialoge) explizit für den Dark Mode angepasst werden müssen oder ob der Bootstrap-5.3-Mechanismus ausreicht, ist noch nicht entschieden. Der Plan setzt auf den Bootstrap-Basisansatz; komponentenspezifische Anpassungen werden bei Bedarf nachgezogen.
