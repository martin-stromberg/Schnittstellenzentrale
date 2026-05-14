# Bestandsaufnahme: Frischeres Layout mit Dark Mode

Analysiert wurde die Präsentations- und Serviceschicht der Blazor Server App „Schnittstellenzentrale" bezogen auf die Anforderung zur Einführung eines Dark Mode und eines modernisierten Layouts.

## Zusammenfassung

- **Bootstrap 5.3.3** ist bereits eingebunden — nativer Dark Mode via `data-bs-theme` wird unterstützt, ohne Bootstrap-Upgrade.
- **`StorageModeService` / `IStorageModeService`** existieren vollständig und bilden das direkte Implementierungsmuster für den zu erstellenden `ThemeService`: Scoped-DI, `Action`-Event, Setter-Methode.
- **`MainLayout`** abonniert bereits `IStorageModeService.OnModeChanged` und ruft `StateHasChanged` auf — exakt das Muster, das für die Theme-Integration übernommen werden kann.
- **`ColorScheme`-Enum** fehlt noch (kein `Light`/`Dark`-Enum im Projekt).
- **`IThemeService`-Interface** fehlt noch.
- **`ThemeService`-Implementierung** fehlt noch.
- **`ThemeToggle`-Komponente** fehlt noch.
- **`App.razor`** setzt kein `data-bs-theme`-Attribut auf dem `<html>`-Tag.
- **`app.css`** enthält keine CSS Custom Properties für Farbschemata; alle Layout-Farben sind hart kodiert.
- **`MainLayout.razor.css`** und **`NavMenu.razor.css`** verwenden hart kodierte Farben ohne Dark-Mode-Gegenstücke.
- **SVG-Icons** in `NavMenu.razor.css` haben hart kodiertes `fill='white'` und sind nicht theme-adaptiv.
- **`theme.js`** (optionales JS-Interop-Modul) existiert nicht.
- **Keine Tests** für `StorageModeService`, Theme-Logik oder UI-Layout-Komponenten vorhanden.
- **`appsettings.json`** enthält keinen `DefaultColorScheme`-Eintrag.

## Details

- [Logik](inventory/logic.md) — `StorageModeService`, `MainLayout`, DI-Registrierung in `Program.cs`
- [Enums](inventory/enums.md) — `StorageMode` als Vorlagemuster; `ColorScheme` fehlt
- [Interfaces](inventory/interfaces.md) — `IStorageModeService` als Vorlagemuster; `IThemeService` fehlt
- [UI-Komponenten und CSS](inventory/ui-components.md) — `MainLayout`, `NavMenu`, `App.razor`, `app.css`; Bootstrap-Version; fehlende Theme-Infrastruktur
- [Tests](inventory/tests.md) — vorhandene Testklassen; keine Theme-Tests
