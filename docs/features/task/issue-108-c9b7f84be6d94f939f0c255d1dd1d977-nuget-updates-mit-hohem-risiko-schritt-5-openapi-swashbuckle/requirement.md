# Übersetzte Anforderung

## Quelle

Projekt Issue #108 „NuGet-Updates mit hohem Risiko" — Entwicklungsschritt 5 (F3):
Koordiniertes Update `Microsoft.OpenApi` → 3.11.1 + `Swashbuckle.AspNetCore` → 11.0.1.

## Ausgangslage

- `Microsoft.OpenApi` ist aktuell in Version **2.10.0** direkt in `Schnittstellenzentrale.Core.csproj` und `Schnittstellenzentrale.Infrastructure.csproj` referenziert.
- `Swashbuckle.AspNetCore` ist aktuell in Version **10.1.7** in `Schnittstellenzentrale.csproj` (Web) referenziert.
- Die öffentliche Core-Signatur `ISwaggerImportService.SyncFromDocumentAsync(OpenApiDocument, int)` exponiert den Typ `Microsoft.OpenApi.OpenApiDocument`.
- `SystemEndpointSyncService` (Web) bezieht das Dokument zur Laufzeit über Swashbuckles `ISwaggerProvider.GetSwagger("v1")` und reicht es an `SyncFromDocumentAsync` durch.
- Ein getrenntes Update der beiden Pakete würde einen OpenApi-2/3-Mix erzeugen (Swashbuckle 11 setzt Microsoft.OpenApi 3.x voraus) — daher ist das Update zwingend **in einem gemeinsamen Schritt** durchzuführen.

## Ziel

`Microsoft.OpenApi` von 2.10.0 auf **3.11.1** und `Swashbuckle.AspNetCore` von 10.1.7 auf **11.0.1** koordiniert heben. Zielversionen zum Umsetzungszeitpunkt erneut konkretisieren (neuere stabile Linien prüfen und Wahl begründen).

### Zu migrierende Codestellen (API-Migration auf das 3.x-Modell)

- `src/Schnittstellenzentrale.Infrastructure/Services/SwaggerImportService.cs` — Reader-API (`OpenApiJsonReader`/`ReadAsync` → 3.x-Äquivalent)
- `src/Schnittstellenzentrale.Infrastructure/Helpers/SwaggerOperationHelper.cs` — `Paths`, `Operations`, `IOpenApiExtension`, `OpenApiSecuritySchemeReference`, `IOpenApiParameter`
- `src/Schnittstellenzentrale/Filters/SecurityOperationFilter.cs`
- `src/Schnittstellenzentrale/Filters/ContextHeadersOperationFilter.cs`
- `src/Schnittstellenzentrale/Filters/SzExtensionsOperationFilter.cs` (`x-sz-*`-Erweiterungen)
- `src/Schnittstellenzentrale/Program.cs` — `AddSwaggerGen`, `UseSwagger`/`UseSwaggerUI`, `IncludeXmlComments`
- Test-Fixtures: `SwaggerImportServiceTests`, `SystemEndpointSyncServiceTests`, `SystemEntryInitializerTests` (und ggf. weitere OpenApi-Bezüge in Tests)

Eigenständige Swashbuckle-11-Breaking-Changes sind gesondert zu prüfen.

## Akzeptanzkriterien

1. `dotnet test --filter "Category!=OsInterface"` vollständig grün (Baseline: 641 Tests).
2. Swagger-Funktionsverifikation: `/swagger/v1/swagger.json` liefert ein gültiges OpenAPI-3-Dokument; Swagger-UI ist erreichbar.
3. Startup-Abgleich des `SystemEndpointSyncService` über die vorhandenen Tests verifiziert.
4. Kein OpenApi-2/3-Mix: einheitliche Microsoft.OpenApi-Major-Version (3.x) über alle Projekte, inklusive transitiver Auflösung.
5. `dotnet list package --vulnerable` ohne Befund.

## Reale End-to-End-Abnahme

App real unter Kestrel starten:
- `GET /swagger/v1/swagger.json` → HTTP 200 + valides JSON mit `openapi`-Feld
- `GET /swagger/index.html` → HTTP 200
- Ergebnis in `test-results.md` dokumentieren.

## Rahmenbedingungen / Einschränkungen

- OsInterface-Test-Lane ist **nicht** erforderlich — nur die reguläre Lane.
- Kein Issue-Kommentar, kein Merge, kein Push, kein PR.
- Git-Hooks und CLAUDE.md-Konventionen beachten (XML-Docs auf öffentlichen APIs erforderlich, `CS1591` ist `WarningsAsErrors`).
- Keine Rückfragen — technisch begründbarste Entscheidungen treffen und als Annahmen dokumentieren; keine Scope-Erweiterung.
