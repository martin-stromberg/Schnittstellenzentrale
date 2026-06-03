# Schnittstellenzentrale – Entwicklungskonventionen

## Stack

ASP.NET Core 9 / Blazor Server · Entity Framework Core · ShadcnBlazor · xUnit · Playwright

## CSS

Eigenes `.sz-*`-System, Bootstrap **nicht** geladen. Details und Variablen-Mapping: [.claude/css-conventions.md](.claude/css-conventions.md)

Ein PostToolUse-Hook prüft nach jedem Schreiben automatisch fehlende Klassen und ungenutzte CSS-Dateien.

## Architekturprinzip: API-First

Alle Datenentitäten (Anwendungsgruppen, Anwendungen, Endpunktgruppen, Endpunkte) werden aus UI-Komponenten ausschließlich über `IApplicationApiClient` abgerufen. Direktzugriffe auf Repository-Interfaces aus Blazor-Komponenten sind nicht erlaubt.

Explizite Ausnahme: `SystemEndpointSyncService` verwendet `IEndpointRepository` direkt beim Startup-Abgleich, da zu diesem Zeitpunkt kein zuverlässiger HTTP-Roundtrip möglich ist.

## Tests

- Unit-Tests: `dotnet test --filter "FullyQualifiedName!~Playwright"`
- Playwright läuft über `PlaywrightServer` (Kestrel, Port 5099) — kein manueller App-Start nötig
- `LayoutSmokeTests` prüft via `BoundingBoxAsync()` ob das Layout nicht kollabiert ist

## Testanforderungen für API-Clients

Für jeden API-Client und jeden neuen Controller sind sowohl Unit-Tests mit gemocktem `HttpMessageHandler` als auch Integrationstests mit `WebApplicationFactory` bereitzustellen.
