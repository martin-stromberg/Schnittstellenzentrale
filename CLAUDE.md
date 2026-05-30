# Schnittstellenzentrale – Entwicklungskonventionen

## Stack

ASP.NET Core 9 / Blazor Server · Entity Framework Core · ShadcnBlazor · xUnit · Playwright

## CSS

Eigenes `.sz-*`-System, Bootstrap **nicht** geladen. Details und Variablen-Mapping: [.claude/css-conventions.md](.claude/css-conventions.md)

Ein PostToolUse-Hook prüft nach jedem Schreiben automatisch fehlende Klassen und ungenutzte CSS-Dateien.

## Tests

- Unit-Tests: `dotnet test --filter "FullyQualifiedName!~Playwright"`
- Playwright läuft über `PlaywrightServer` (Kestrel, Port 5099) — kein manueller App-Start nötig
- `LayoutSmokeTests` prüft via `BoundingBoxAsync()` ob das Layout nicht kollabiert ist
