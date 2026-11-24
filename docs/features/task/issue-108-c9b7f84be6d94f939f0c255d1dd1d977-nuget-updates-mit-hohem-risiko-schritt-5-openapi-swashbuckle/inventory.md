# Bestandsaufnahme – F3: Microsoft.OpenApi 3.11.1 + Swashbuckle.AspNetCore 11.0.1

## Paketbezüge (Ist)

| Paket | Version | Projekt |
|-------|---------|---------|
| `Microsoft.OpenApi` | 2.10.0 | `src/Schnittstellenzentrale.Core/Schnittstellenzentrale.Core.csproj` |
| `Microsoft.OpenApi` | 2.10.0 | `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj` |
| `Swashbuckle.AspNetCore` | 10.1.7 | `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj` |

Keine weiteren direkten `Microsoft.OpenApi`-/`Swashbuckle`-Referenzen (Testprojekt bezieht beides transitiv über die ProjectReferences). `Microsoft.OpenApi.YamlReader` wird nicht verwendet.

## Verfügbare Zielversionen (Stand der Umsetzung)

- `Microsoft.OpenApi` **3.11.1** (NuGet, 16.11.2026) — neueste stabile 3.x-Linie; davor 3.11.0 (19.10.2026). Ältere 3.x-Stände (≤3.10.x) tragen teils High-CVSS-Befunde laut Sonatype-Listing; 3.11.1 hat keinen Befund.
- `Swashbuckle.AspNetCore` **11.0.1** (NuGet, 12.11.2026) — neueste stabile 11.x-Linie; hängt laut Release-Notes an OpenAPI.NET v3.11 und targetiert net10.0/net11.0.
- Wahl: **3.11.1 / 11.0.1** — entsprechen den in der Anforderung genannten Zielständen und sind gleichzeitig die aktuellen stabilen Releases. Keine neuere stabile Linie vorhanden.

## Betroffene Produktiv-Codestellen (2.x-API-Nutzung)

| Datei | Genutzte 2.x-API-Elemente |
|-------|---------------------------|
| `src/Schnittstellenzentrale.Infrastructure/Services/SwaggerImportService.cs` | `Microsoft.OpenApi.Reader.OpenApiJsonReader`, `ReadAsync(stream, uri, OpenApiReaderSettings, ct)`, `ReadResult.Diagnostic`/`Document`, `Microsoft.OpenApi.OpenApiDocument` |
| `src/Schnittstellenzentrale.Infrastructure/Helpers/SwaggerOperationHelper.cs` | `IOpenApiExtension`, `JsonNodeExtension` (+`Node`/`JsonValue`), `OpenApiDocument.Paths`, `OpenApiPathItem.Operations` (`IDictionary`-Iteration, `operation.Key.ToString()`), `OpenApiOperation` (`OperationId`, `Extensions`, `Security`, `Parameters`), `OpenApiSecurityRequirement.Keys`, `OpenApiSecuritySchemeReference` (`Reference.Id`), `IOpenApiParameter` (`In`, `Name`, `Schema.Default` als `JsonValue`), `ParameterLocation.Header` |
| `src/Schnittstellenzentrale.Core/Interfaces/ISwaggerImportService.cs` | `OpenApiDocument` in öffentlicher Signatur `SyncFromDocumentAsync` |
| `src/Schnittstellenzentrale/Filters/SecurityOperationFilter.cs` | `OpenApiOperation.Security`, `OpenApiSecurityRequirement`, `new OpenApiSecuritySchemeReference(schemeId, null)` |
| `src/Schnittstellenzentrale/Filters/ContextHeadersOperationFilter.cs` | `OpenApiParameter`, `ParameterLocation.Header`, `OpenApiSchema` (`Type = JsonSchemaType.String`, `Enum`/`Default` als `JsonValue`) |
| `src/Schnittstellenzentrale/Filters/SzExtensionsOperationFilter.cs` | `OpenApiOperation.Extensions` (`IDictionary<string, IOpenApiExtension>`), `JsonNodeExtension` |
| `src/Schnittstellenzentrale/Program.cs` | `AddSwaggerGen`, `OpenApiInfo`, `OpenApiSecurityScheme` (`SecuritySchemeType.Http`, `Scheme`, `ParameterLocation`), `OperationFilter<…>`, `IncludeXmlComments`, `UseSwagger`, `UseSwaggerUI` |
| `src/Schnittstellenzentrale/SystemEndpointSyncService.cs` | `ISwaggerProvider.GetSwagger("v1")` → `OpenApiDocument`, `ISwaggerImportService.SyncFromDocumentAsync` |

## Betroffene Test-Codestellen

| Datei | OpenApi-/Swashbuckle-Bezug |
|-------|---------------------------|
| `src/Schnittstellenzentrale.Tests/Services/SwaggerImportServiceTests.cs` | `OpenApiDocument`, `OpenApiPaths`, `OpenApiPathItem` (`AddOperation(HttpMethod, OpenApiOperation)`), `OpenApiOperation` (`Extensions` mit `JsonNodeExtension`), `IOpenApiExtension` |
| `src/Schnittstellenzentrale.Tests/Services/SystemEndpointSyncServiceTests.cs` | `OpenApiDocument`/`OpenApiPaths`/`OpenApiPathItem`/`AddOperation`, `ISwaggerProvider.GetSwagger`-Mocks |
| `src/Schnittstellenzentrale.Tests/Integration/SystemEntryInitializerTests.cs` | `OpenApiOperation`, `OpenApiSecurityRequirement`, `OpenApiSecuritySchemeReference("Negotiate", null)`, `JsonNodeExtension`, `IOpenApiExtension`, `OpenApiPaths`/`OpenApiPathItem`, `ISwaggerProvider` |
| `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightApiFactory.cs`, `PlaywrightServer.cs` | Nur `ISwaggerImportService`-Mocks (Interface unverändert) + Swagger-Warmup |
| `src/Schnittstellenzentrale.Tests/Playwright/SwaggerImportTests.cs` | OsInterface-Lane — nicht im Scope dieser Abnahme |

## Erwartete 3.x-Umbrüche (aus OpenAPI.NET-Upgrade-Guide v3 + Swashbuckle-v11-Migrationsguide)

Der offizielle Guide beschreibt den 2.x→3.x-Umbruch als deutlich kleiner als 1.x→2.x:

1. `OpenApiMediaType` → `IOpenApiMediaType` in Collection-Properties (`OpenApiResponse.Content`, `OpenApiRequestBody.Content`) — **nicht genutzt** im Bestand.
2. `OpenApiXml.Attribute`/`Wrapped` → `NodeType` — **nicht genutzt**.
3. Visitor-Methoden akzeptieren Interfaces — **nicht genutzt**.
4. `IOpenApiSchema.Example` obsolet → `Examples` — **nicht genutzt**.
5. `IOpenApiResponse` erweitert `IOpenApiSummarizedElement` — **nicht genutzt**.
6. OpenAPI-3.2-Support ist Opt-in; Swashbuckle 11 erzeugt weiterhin **OpenAPI-3.0-Dokumente als Default** (`options.OpenApiVersion` bleibt Standard).
7. Swashbuckle 11: Drop von net8.0/net9.0 (irrelevant — net11.0), Redoc-3-Upgrade (nicht genutzt), mehrfache Response-Typen je Statuscode werden zu `anyOf` kombiniert (nur relevant bei mehreren `ProducesResponseType` auf denselben Statuscode — zu prüfen).

Mögliche weitere Kompilierbrüche (Compiler-getrieben zu klären): Signatur von `OpenApiJsonReader.ReadAsync`/`OpenApiReaderSettings`, `JsonNodeExtension`-Existenz bzw. `Extensions`-Dictionary-Typ, `OpenApiSecurityRequirement`-Key-Typ, `OpenApiPathItem.AddOperation`-Signatur, `Operations`-Dictionary-Key-Typ (`HttpMethod` vs. `string`), `OpenApiSecuritySchemeReference`-Konstruktor.

## Baseline

- `dotnet test --filter "Category!=OsInterface"`: **641 Tests** (gemäß Anforderung/Projektstand).
- Staging-Workflow: Basisbranch `task/…-nuget-updates-mit-hohem-risiko`; Schritte 1–4 bereits gemergt (net11.0, SDK 11.0.100, FrameworkReference, OData-Stack, Serilog 11.0.0).
- `CS1591` ist `WarningsAsErrors` in allen vier Projekten → XML-Docs auf öffentlichen APIs Pflicht.
- Architekturprinzip: `SystemEndpointSyncService` nutzt `ISwaggerProvider` + `ISwaggerImportService` — Signaturen bleiben öffentlich stabil (`OpenApiDocument`-Typ behält Namen, nur Paket-Major ändert sich).
