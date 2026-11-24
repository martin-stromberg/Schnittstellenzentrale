# Schnittstellenzentrale – Entwicklungskonventionen

## Stack

ASP.NET Core 11 / Blazor Server · Entity Framework Core · ShadcnBlazor · xUnit · Playwright

## CSS

Eigenes `.sz-*`-System, Bootstrap **nicht** geladen. Details und Variablen-Mapping: [.claude/css-conventions.md](.claude/css-conventions.md)

Ein PostToolUse-Hook prüft nach jedem Schreiben automatisch fehlende Klassen und ungenutzte CSS-Dateien.

## Architekturprinzip: API-First

Alle Datenentitäten (Anwendungsgruppen, Anwendungen, Endpunktgruppen, Endpunkte) werden aus UI-Komponenten ausschließlich über `IApplicationApiClient` abgerufen. Direktzugriffe auf Repository-Interfaces aus Blazor-Komponenten sind nicht erlaubt.

Explizite Ausnahme: `SystemEndpointSyncService` verwendet `IApplicationRepository` direkt beim Startup-Abgleich (Auflösen von Systemgruppe und Systemanwendung), da zu diesem Zeitpunkt kein zuverlässiger HTTP-Roundtrip möglich ist. Der eigentliche Endpunktabgleich inklusive Persistenz läuft serverseitig in `SwaggerImportService.SyncFromDocumentAsync`.


## Tests

- Unit-Tests: `dotnet test -- --filter "Category!=OsInterface"` (reguläre Lane, blockierend)
- OS-Schnittstellen-Tests (Playwright, echte Kestrel-Bindung, Temp-FS): `dotnet test -- --filter "Category=OsInterface"` — läuft in der CI als best-effort-Job `os-interface-tests`, siehe `docs/help/stabilitaet/os-interface-tests.md`
- Das Testprojekt läuft auf xunit.v3 (Microsoft.Testing.Platform, Opt-in via `test.runner` in `global.json`): `--filter` bleibt VSTest-Syntax, wird aber als Test-App-Argument hinter `--` übergeben; TRX kommt über `--report-trx`, Coverage über `--coverage --coverage-output-format cobertura` (statt `--collect:"XPlat Code Coverage"`).
- Playwright läuft über `PlaywrightServer` (Kestrel, Port 5099) — kein manueller App-Start nötig
- `LayoutSmokeTests` prüft via `BoundingBoxAsync()` ob das Layout nicht kollabiert ist

## Testanforderungen für API-Clients

Für jeden API-Client und jeden neuen Controller sind sowohl Unit-Tests mit gemocktem `HttpMessageHandler` als auch Integrationstests mit `WebApplicationFactory` bereitzustellen.

## Lokalisierung (resx-Konvention)

Pro Projekt wird genau ein resx-Paket angelegt — keine komponentenindividuellen resx-Dateien:

- Hauptprojekt: `src/Schnittstellenzentrale/Resources/SharedResources.cs` + `SharedResources.resx` (EN Fallback) + `SharedResources.de.resx` (DE)
- Core-Projekt: `src/Schnittstellenzentrale.Core/Resources/CoreResources.cs` + `CoreResources.resx` + `CoreResources.de.resx`

Schlüsselschema: `{KomponentenName}_{Rolle}` (Beispiele: `ApplicationEditor_SaveButton`, `ConfirmDeleteGroupDialog_Title`)

Jeder Schlüssel erhält einen ausgefüllten `Comment`-Eintrag in der resx, der den UI-Kontext beschreibt.

Einbindung in Razor-Komponenten: `@inject IStringLocalizer<SharedResources> L` — der `@using Microsoft.Extensions.Localization`-Eintrag ist global in `_Imports.razor` registriert.
