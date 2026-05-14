# Anforderung: Frischeres Layout mit Dark Mode

## Fachliche Zusammenfassung

Das visuelle Erscheinungsbild der Blazor Server App „Schnittstellenzentrale" wird vom Microsoft-Standardtemplate-Design auf ein moderneres, eigenständiges Layout umgestellt. Kernbestandteil ist die Einführung eines Dark Mode, der neben dem bisherigen Light Mode verfügbar sein soll. Die Umstellung betrifft ausschließlich die Präsentationsschicht (CSS, Blazor-Layout-Komponenten); Geschäftslogik, Domänenmodell und Datenhaltung bleiben unverändert. Der Benutzer soll zwischen den Farbschemata wechseln können; ob die Präferenz dauerhaft gespeichert wird, ist noch zu klären.

---

## Betroffene Klassen und Komponenten

### Datenmodellklassen

| Klasse / Eigenschaft | Änderung |
|---|---|
| *(keine neue Klasse zwingend erforderlich)* | Falls die Theme-Präferenz benutzerspezifisch gespeichert werden soll, wäre eine neue Eigenschaft `ThemePreference` (Enum oder String) in einem Benutzerpräferenz-Modell denkbar. |

### Enums (neu, optional)

| Enum | Beschreibung |
|---|---|
| `ColorScheme` | Werte: `Light`, `Dark` — repräsentiert das aktive Farbschema. |

### Services / Logikklassen

| Klasse / Interface | Beschreibung |
|---|---|
| `IThemeService` | Interface für den Zugriff auf das aktive `ColorScheme` sowie für den Wechsel. |
| `ThemeService` | Implementierung von `IThemeService`; hält das aktive Farbschema als Scoped-DI-Instanz pro Blazor-Circuit; optional mit Persistierung via `LocalStorage`/Cookie oder serverseitiger Speicherung. |

### UI-Komponenten (Blazor)

| Komponente | Änderung |
|---|---|
| `MainLayout` | Bindet `IThemeService` ein; setzt ein CSS-Klassen-Attribut (`data-bs-theme` oder eine eigene CSS-Klasse) auf dem Root-Element, das das aktive Farbschema steuert. Enthält den Theme-Umschalter (Toggle/Button). |
| `NavMenu` | Visuelles Redesign (Farben, Schriftschnitte, Abstände) passend zum neuen Layout; Theme-abhängige Stile. |
| `ThemeToggle` *(neu, optional)* | Eigenständige Blazor-Komponente für den Hell/Dunkel-Umschalter (z. B. Toggle-Button mit Icon); kann in `MainLayout` oder `NavMenu` eingebettet werden. |

### CSS / Styling

| Artefakt | Beschreibung |
|---|---|
| `wwwroot/app.css` | Zentrales Stylesheet: wird um CSS-Custom-Properties (CSS Variables) für beide Farbschemata erweitert; alternativ wird eine separate `theme-dark.css` eingebunden. |
| Bootstrap-Integration | Bootstrap 5 unterstützt Dark Mode nativ via `data-bs-theme="dark"` auf dem `<html>`- oder `<body>`-Element. Diese Konvention soll genutzt werden, sofern kein komplett eigenes Theme-System eingeführt wird. |

### JavaScript (optional)

| Artefakt | Beschreibung |
|---|---|
| `wwwroot/theme.js` *(optional)* | Kleines JS-Interop-Modul zum Lesen/Schreiben der Theme-Präferenz aus `localStorage` oder zum Setzen des `data-bs-theme`-Attributs vor dem ersten Render (verhindert Flash of unstyled content). |

### Tests

- Unit-Tests für `ThemeService` (Initialisierung, Wechsel, ggf. Persistierung)
- *(UI-Tests optional, da rein visuell)*

---

## Implementierungsansatz

### Theme-Steuerung via Bootstrap 5 Dark Mode

Bootstrap 5.3+ bietet nativen Dark Mode über das Attribut `data-bs-theme="dark"` am `<html>`-Tag. Dieses Attribut wird von `MainLayout` oder per JS-Interop in `App.razor` gesetzt. CSS Custom Properties in `app.css` überschreiben bei Bedarf Bootstrap-Variablen für projektspezifische Anpassungen.

### Zustandshaltung

`ThemeService` wird als Scoped-Service registriert (analoges Muster zu `StorageModeService`). Er hält das aktive `ColorScheme` und stellt ein `OnThemeChanged`-Event bereit. `MainLayout` abonniert dieses Event und aktualisiert das Layout via `StateHasChanged` — exakt wie der bestehende `StorageModeService`-Mechanismus.

### Persistierung der Theme-Präferenz

Die Präferenz des Benutzers kann über `localStorage` (Browser) persistiert werden. Da Blazor Server keinen direkten `localStorage`-Zugriff hat, wird ein JS-Interop-Call benötigt (`IJSRuntime`). Alternativ genügt eine rein session-basierte (nicht persistierte) Haltung im Scoped-Service.

*Annahme: Persistierung via `localStorage` ist gewünscht; ein kleines JS-Interop-Modul wird ergänzt.*

### Visuelles Redesign ("frischeres Layout")

Neben dem Dark Mode soll das allgemeine Layout modernisiert werden. Konkrete Designentscheidungen (Farbpalette, Typografie, Abstände, Icons) sind noch zu definieren. Als Ausgangspunkt bieten sich Bootstrap 5-Themes (z. B. Bootswatch) oder eine eigene CSS-Variablen-Palette an.

### Abhängigkeiten

- `IJSRuntime` für optionale `localStorage`-Interaktion
- Bootstrap 5.3+ (bereits eingebunden, Dark Mode ab dieser Version nativ unterstützt); zu prüfen, ob die aktuelle Bootstrap-Version im Projekt >= 5.3 ist.
- `IStorageModeService` bleibt unberührt.

---

## Konfiguration

| Schlüssel | Ebene | Beschreibung |
|---|---|---|
| Theme-Präferenz | Benutzerspezifisch (Browser `localStorage`) | Speichert `Light` oder `Dark` zwischen Sitzungen; kein Eintrag in `appsettings.json` notwendig. |
| Standard-Farbschema | Anwendungseinstellung (`appsettings.json`, optional) | Legt das Fallback-Farbschema fest, wenn keine Benutzerpräferenz gespeichert ist (z. B. `"DefaultColorScheme": "Dark"`). |

---

## Offene Fragen

1. **Persistierung der Theme-Präferenz:** Soll die Auswahl (Hell/Dunkel) dauerhaft im Browser (`localStorage`) gespeichert werden, oder ist eine rein sitzungsbasierte Haltung ausreichend?

2. **System-Präferenz übernehmen:** Soll das initiale Farbschema automatisch aus der Betriebssystem-/Browser-Einstellung (`prefers-color-scheme`) abgeleitet werden?

3. **Umfang des visuellen Redesigns:** Wie weit geht das „frischere Layout"? Handelt es sich nur um Farbanpassungen (Dark Mode + Akzentfarben), oder sind auch strukturelle Layout-Änderungen (Sidebar-Breite, Typografie, Icon-Set, Abstände) gewünscht?

4. **Design-Vorgaben:** Gibt es einen Styleguide, Mockups oder eine Farbpalette, an der sich das neue Design orientieren soll? Oder soll ein Bootstrap-Theme (z. B. Bootswatch) als Basis gewählt werden?

5. **Bootstrap-Version:** Die aktuelle Bootstrap-Version im Projekt muss geprüft werden. Nativer Dark Mode (via `data-bs-theme`) erfordert Bootstrap >= 5.3.

6. **Abdeckung des Dark Mode:** Sollen alle Shared-Komponenten (`ApplicationCard`, `EndpointEditor`, `EndpointExecutionPanel`, Dialoge etc.) vollständig im Dark Mode getestet und angepasst werden, oder ist ein Best-Effort-Ansatz über Bootstrap-Variablen ausreichend?

7. **Umschalter-Position:** Wo soll der Theme-Umschalter platziert werden — in der Top-Row (neben dem Modus-Selektor) oder im `NavMenu`?
