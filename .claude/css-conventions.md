# CSS-Konventionen

## Klassennamensystem

Das Projekt verwendet ein eigenes `.sz-*`-System. Bootstrap CSS ist **nicht** geladen.

### Pflichtprüfung bei neuen Razor-Komponenten

Jede neue `.sz-*`-Klasse muss **vor Abschluss der Implementierung** in einer der folgenden Dateien definiert sein:

- `src/Schnittstellenzentrale/wwwroot/app.css` — globale Stile (Layout, Buttons, Icons, Utilities)
- `src/Schnittstellenzentrale/Components/**/*.razor.css` — Scoped-CSS der jeweiligen Komponente

Der PostToolUse-Hook (`check_css_classes.py`) meldet fehlende Definitionen automatisch.

### Verwendete CSS-Variablen (shadcn)

| Bootstrap (nicht verwenden) | shadcn-Äquivalent |
|-----------------------------|-------------------|
| `var(--bs-border-color)` | `var(--border)` |
| `var(--bs-body-bg)` | `var(--background)` |
| `var(--bs-body-color)` | `var(--foreground)` |
| `var(--bs-secondary-color)` | `var(--muted-foreground)` |
| `var(--bs-danger)` | `var(--destructive)` |
| `[data-bs-theme="dark"]` | `.dark` |

Theme-Aktivierung: `theme.js` setzt `.dark`-Klasse auf `<html>` (kein `data-theme`-Attribut).

## CSS-Datei-Regeln

- Jede globale CSS-Datei in `wwwroot/` (außer `lib/`) muss per `<link>` in einer Razor- oder HTML-Datei referenziert sein
- Jede `.razor.css`-Datei muss eine gleichnamige `.razor`-Komponente haben
- Der Hook meldet Verstöße nach jedem Schreiben/Bearbeiten von CSS- oder Razor-Dateien

## Layout-Verifikation

Nach Implementierungen mit UI-Änderungen:

1. `dotnet build` — fehlerfrei
2. `dotnet test --filter "FullyQualifiedName!~Playwright"` — alle Unit-Tests grün
3. App starten und prüfen: strukturiertes Layout, kein Kollaps in die obere linke Ecke

`LayoutSmokeTests.cs` prüft automatisch via `BoundingBoxAsync()`, dass alle Hauptbereiche tatsächlich Fläche einnehmen.
