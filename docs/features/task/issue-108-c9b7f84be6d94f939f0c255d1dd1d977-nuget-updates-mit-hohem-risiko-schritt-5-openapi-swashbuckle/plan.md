# Umsetzungsplan – F3: Microsoft.OpenApi 3.11.1 + Swashbuckle.AspNetCore 11.0.1

## Zielbild

`Microsoft.OpenApi` liegt in `Core` und `Infrastructure` einheitlich auf **3.11.1**, `Swashbuckle.AspNetCore` im Web-Projekt auf **11.0.1**. Sämtlicher Code, der das OpenAPI-Objektmodell oder die Reader-API nutzt, kompiliert gegen das 3.x-Modell. Funktional ändert sich nichts: Swagger-UI, `swagger.json`, `x-sz-*`-Erweiterungen, Context-Header-Parameter, Security-Filter und der Startup-Endpunktabgleich bleiben verhaltensgleich.

## Scope-Klärung

- **Verarbeitungseinheit:** NuGet-Referenzen in drei csproj-Dateien plus API-Migration aller Compile-Bruchstellen in Produktiv- und Testcode.
- **Auslösendes Ereignis:** Build (`dotnet build`), reguläre Test-Lane, realer App-Start.
- **Erwartete Wirkung:** Build ohne OpenApi-2/3-Mix; alle Akzeptanzkriterien erfüllt.
- **Scope-Ausschlüsse:**
  - Kein Opt-in auf OpenAPI 3.1/3.2-Dokumenterzeugung (`OpenApiVersion` bleibt Default = OpenAPI 3.0) — Status: **bestätigt** (dokumentierte Annahme im Auftrag: „keine Scope-Erweiterung"; das Akzeptanzkriterium verlangt ausdrücklich ein „OpenAPI-3-Dokument", Default erfüllt das).
  - Keine Änderung der OsInterface-/Playwright-Lane — Status: **bestätigt** (explizit im Auftrag ausgenommen).
  - Kein Merge/Push/PR, kein Issue-Kommentar — Status: **bestätigt** (im Auftrag ausgeschlossen).

## Arbeitsschritte

### 1. Paketversionen heben

- `src/Schnittstellenzentrale.Core/Schnittstellenzentrale.Core.csproj`: `Microsoft.OpenApi` 2.10.0 → **3.11.1**
- `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj`: `Microsoft.OpenApi` 2.10.0 → **3.11.1**
- `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`: `Swashbuckle.AspNetCore` 10.1.7 → **11.0.1** (mit Kommentar-Block zu F3 analog der F4/F5/F6-Kommentare)
- Testprojekt: keine direkte Änderung (transitiv).

### 2. API-Migration (compilergetrieben, antizipierte Stellen)

Nach `dotnet restore` + `dotnet build` alle Fehler iterativ beheben. Erwartete Anpassungen:

- **`SwaggerImportService.cs`:** Reader-Aufruf auf 3.x-API prüfen (`OpenApiJsonReader`/`ReadAsync`/`OpenApiReaderSettings`/`ReadResult.Diagnostic`/`Document`); ggf. auf `OpenApiDocument.Parse`/`OpenApiModelFactory`-Äquivalent umstellen. Fehlerbehandlung (Diagnostic-Errors → `ImportDiff.ErrorMessage`) beibehalten.
- **`SwaggerOperationHelper.cs`:** `Extensions`-Dictionary-Typ, `IOpenApiParameter`/`IOpenApiExtension`/`JsonNodeExtension`-Bezüge, `Operations`-Iteration und `OpenApiSecuritySchemeReference` gegen 3.x prüfen; `MapHttpMethod(operation.Key.ToString())` je nach Key-Typ anpassen.
- **Filter (3 Dateien):** `OpenApiSecurityRequirement`-Key-Typ (`IOpenApiSecurityScheme` vs. `OpenApiSecuritySchemeReference`), `OpenApiParameter`/`OpenApiSchema` (`JsonSchemaType`, `JsonValue`-Enum/Default), `JsonNodeExtension`-Zuweisungen.
- **`Program.cs`:** `OpenApiInfo`/`OpenApiSecurityScheme`/`AddSwaggerGen`/`UseSwagger`/`UseSwaggerUI`/`IncludeXmlComments` — voraussichtlich unverändert, Verifikation per Build.
- **`SystemEndpointSyncService.cs`:** voraussichtlich unverändert (`GetSwagger` liefert weiter `OpenApiDocument`).
- **Tests:** `SwaggerImportServiceTests.cs`, `SystemEndpointSyncServiceTests.cs`, `SystemEntryInitializerTests.cs` — Fixture-Helper (`DocumentWithPaths`, `AddOperation`, `OpenApiSecuritySchemeReference`, `JsonNodeExtension`) an 3.x anpassen.
- **Swashbuckle-11-Breaking-Changes gesondert:** `ISwaggerProvider`-Signatur, Filter-Schnittstellen (`IOperationFilter.Apply`), Redoc nicht genutzt; `anyOf`-Response-Kombinierung: per Volltextsuche auf doppelte `ProducesResponseType`-Attribute auf identischem Statuscode prüfen.

### 3. Versionkohärenz + Sicherheit

- `dotnet list package --include-transitive` → `Microsoft.OpenApi` überall 3.x.
- `dotnet list package --vulnerable` → kein Befund.

### 4. Tests

- `dotnet test --filter "Category!=OsInterface"` → alle grün (Baseline 641; Anzahl darf bei unverändertem Funktionsumfang nicht sinken).

### 5. Reale End-to-End-Abnahme

- App unter Kestrel real starten (Testumgebung/Temp-DB analog Schritt-4-Vorgehen).
- `GET /swagger/v1/swagger.json` → 200, JSON mit `openapi`-Feld, Stichprobe: `x-sz-bearer-token`-Extension und Security-Requirements vorhanden.
- `GET /swagger/index.html` → 200.
- Startup-Abgleich: Log-Eintrag/Zustand des `SystemEndpointSyncService` prüfen (Endpunkte der Systemanwendung in DB bzw. Testlage über die Integrationstests `SystemEntryInitializerTests`/`SystemEndpointSyncServiceTests`).

### 6. Dokumentation

- `docs/projects/task/issue-108-…-nuget-updates-mit-hohem-risiko/decisions.md`: F3-Entscheidung ergänzen.
- `docs/help/…`: betroffene Doku-Stellen (API-/Swagger-Beschreibung) prüfen und ggf. Versionshinweis aktualisieren.
- `docs/RELEASE_NOTES.md` aktualisieren; README nur, falls Versionsangaben betroffen.

## Akzeptanzszenarien

**Szenario 1 (Happy Path – Swagger-Dokument):** Nach dem Update liefert die real gestartete App `GET /swagger/v1/swagger.json` mit HTTP 200, das JSON enthält ein `openapi`-Feld (3.x) sowie die `x-sz-*`-Erweiterungen; `GET /swagger/index.html` → 200 (Swagger-UI erreichbar).

**Szenario 2 (Startup-Abgleich):** Der `SystemEndpointSyncService` holt beim Start das Dokument via `ISwaggerProvider.GetSwagger("v1")` (3.x-Modell) und gibt es an `ISwaggerImportService.SyncFromDocumentAsync` weiter — der Abgleich legt/aktualisiert Systemendpunkte ohne Fehler (Nachweis über `SystemEndpointSyncServiceTests` + `SystemEntryInitializerTests`).

**Szenario 3 (Regression):** `dotnet test --filter "Category!=OsInterface"` — 641/641 grün.

**Szenario 4 (Kohärenz):** `dotnet list package --include-transitive` zeigt keine Microsoft.OpenApi-2.x-Auflösung; `dotnet list package --vulnerable` ohne Befund.

**Szenario 5 (Import-Fehlerpfad):** Ungültige Swagger-Definition liefert weiterhin `ImportDiff.ErrorMessage` (abgedeckt durch `SwaggerImportServiceTests`).

## Offene Punkte

Keine — alle Entscheidungen wurden als dokumentierte Annahmen getroffen (Zielversionen bestätigt, kein Opt-in auf 3.1/3.2, kein OsInterface-Lauf).
