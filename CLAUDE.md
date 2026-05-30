# Schnittstellenzentrale – Entwicklungskonventionen für Claude

## Technologie-Stack

- ASP.NET Core 9 / Blazor Server
- Entity Framework Core (SQLite für Tests, SQL Server für Produktion)
- ShadcnBlazor-Komponenten (NuGet)
- Playwright für E2E-Tests, xUnit für Unit-Tests

## CSS-Konvention (kritisch)

**Alle custom CSS-Klassen müssen definiert sein, bevor eine Implementierung als abgeschlossen gilt.**

Das Projekt verwendet ein eigenes `.sz-*`-CSS-System. Bootstrap ist **nicht** mehr geladen.

### Pflichtprüfung bei neuen Razor-Komponenten

Wenn eine neue Razor-Komponente oder ein neues HTML-Fragment `.sz-*`-Klassen verwendet, müssen diese in einer der folgenden Dateien definiert sein:

- `src/Schnittstellenzentrale/wwwroot/app.css` — globale Stile (Layout, Buttons, Icons, Utilities)
- `src/Schnittstellenzentrale/Components/**/*.razor.css` — Scoped-CSS der jeweiligen Komponente

**Konkrete Prüfschritte nach Implementierung:**

1. Alle in `.razor`-Dateien verwendeten CSS-Klassen auflisten:
   ```
   grep -rh 'class="[^"]*"' src/Schnittstellenzentrale/Components --include="*.razor"
   ```
2. Für jede neue `.sz-*`-Klasse prüfen ob sie in `app.css` oder einer `.razor.css`-Datei definiert ist.
3. Fehlende Klassen **vor dem nächsten Schritt** ergänzen — nicht als Folgeaufgabe aufheben.

### CSS-Variablen

Shadcn-Variablen verwenden (Bootstrap ist nicht geladen):

| Bootstrap (nicht verwenden) | Shadcn-Äquivalent |
|-----------------------------|-------------------|
| `var(--bs-border-color)` | `var(--border)` |
| `var(--bs-body-bg)` | `var(--background)` |
| `var(--bs-body-color)` | `var(--foreground)` |
| `var(--bs-secondary-color)` | `var(--muted-foreground)` |
| `var(--bs-danger)` | `var(--destructive)` |
| `[data-bs-theme="dark"]` | `.dark` |

Theme-Aktivierung: `theme.js` setzt die Klasse `.dark` auf `<html>` (nicht `data-theme`-Attribut).

## Playwright-Tests

- Tests laufen über `PlaywrightServer` (Kestrel direkt, kein WebApplicationFactory)
- `ContentRootPath` wird auf das Quellverzeichnis `src/Schnittstellenzentrale/` gesetzt — dort liegt `wwwroot/`
- Der Smoke-Test `LayoutSmokeTests.cs` prüft, dass das App-Shell-Layout tatsächlich Platz einnimmt (nicht kollabiert)

## Verifikation nach Implementierung

Nach jeder Implementierung, die UI-Änderungen enthält:

1. `dotnet build` — muss fehlerfrei sein
2. `dotnet test --filter "FullyQualifiedName!~Playwright"` — alle Unit-Tests müssen grün sein
3. App starten und im Browser prüfen: Layout strukturiert, kein weißer Hintergrund, kein Kollaps in die obere linke Ecke
