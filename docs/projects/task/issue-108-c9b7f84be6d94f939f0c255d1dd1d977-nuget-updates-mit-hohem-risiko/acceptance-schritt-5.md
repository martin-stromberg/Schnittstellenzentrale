# Abnahmeprüfung – Entwicklungsschritt 5

## Ergebnis
**Status:** Anforderung vollständig erfüllt

## Abweichungen
- Keine.

## Hinweise

**Diff-Scope (`git diff {basis}...HEAD`, 8 Dateien, +50/−7):**
- `Schnittstellenzentrale.Core.csproj` + `Schnittstellenzentrale.Infrastructure.csproj`: `Microsoft.OpenApi` 2.10.0 → 3.11.1; `Schnittstellenzentrale.csproj`: `Swashbuckle.AspNetCore` 10.1.7 → 11.0.1 (mit begründendem Kommentar + Verweis auf decisions.md).
- Einzige Code-Änderung: `src/Schnittstellenzentrale/Filters/SecurityOperationFilter.cs` (Z. 28–31) — `OpenApiSecuritySchemeReference` erhält jetzt `context.Document` als Host-Dokument. Fix fachlich korrekt: ohne Host-Dokument serialisiert die Requirement unter 3.x als `{}`; live verifiziert als `{"Negotiate": []}` / `{"Bearer": []}`.
- Test-Fixture `SystemEntryInitializerTests.cs` (Z. 229–258) analog migriert; `OpenApiDocument`-Instanz wird vorab erzeugt und als Host an die Referenz übergeben — produktive Form.
- Dokumentation: `RELEASE_NOTES.md` (EN+DE), `ablauf-technisch.md` (OpenApiStreamReader→OpenApiJsonReader korrigiert), `decisions.md` (F3-Eintrag vollständig: Entscheidung, Begründung, Alternativen, Konsequenzen).

**Build & Tests:**
- `dotnet build Schnittstellenzentrale.slnx`: 0 Fehler, 35 Warnungen — ausschließlich vorbestehende Blazor-Analyzer-Warnungen (BL0012/BL0016), keine OpenAPI-/Swashbuckle-/NU-Warnung.
- `dotnet test --filter "Category!=OsInterface"`: **641/641 bestanden, 0 Fehler** — Baseline exakt getroffen.

**Paketstände (`dotnet list package --include-transitive`):**
- `Microsoft.OpenApi` einheitlich **3.11.1** in allen vier Projekten (direkt in Core + Infrastructure, transitiv in Web + Tests) — kein 2.x-Rest, kein OpenApi-2/3-Mix.
- `Swashbuckle.AspNetCore(.Swagger/.SwaggerGen/.SwaggerUI)` einheitlich **11.0.1**.
- `dotnet list package --vulnerable`: **kein Befund** in allen vier Projekten.

**Live-Stichprobe (Kestrel, Environment=Playwright, Port 5099):**
- `GET /swagger/v1/swagger.json` → 200, `openapi: 3.0.4` (gültiges OpenAPI-3-Dokument, Swashbuckle-Default, kein 3.1/3.2-Opt-in — konsistent mit decisions.md).
- 67 `x-sz-`-Extensions im Dokument — deckt sich exakt mit der Schritt-Meldung.
- `components.securitySchemes`: `Bearer` + `Negotiate`; Security-Requirements korrekt serialisiert.
- `GET /swagger/index.html` → 200 (`/swagger/` → 301 auf index.html): Swagger-UI erreichbar.
- Startup-Abgleich `SystemEndpointSyncService`: erzeugte SQLite-DB enthält exakt **34 Endpunkte** (1× Negotiate, 33× BearerToken) — Negotiate-Erkennung über die korrigierte Referenz funktioniert durchgängig; zusätzlich testseitig via `InitializeAsync_AndSyncService_CreatesEndpointsWithCorrectAuthorizationAndCredentials` abgesichert.

**Swashbuckle-11-Breaking-Changes (Gegenprobe):**
- Kein `ProducesResponseType` mit doppeltem Statuscode pro Action (programmatische Volltextprüfung aller Controller-Attributblöcke — keine Fundstelle).
- Redoc ungenutzt (kein `UseReDoc`/Redoc-Verweis im Bestand).
- `Program.cs` Z. 89–112: `AddSwaggerGen`-/Filter-Registrierung unverändert und sinnvoll; `UseSwagger`/`UseSwaggerUI` unverändert (Z. 202–203).
- Keine 2.x-API-Reste im Quellcode (`OpenApiStreamReader`, `Microsoft.OpenApi.Readers`, `IOpenApiAny`, `OpenApiString` — keine Treffer); `SwaggerImportService` nutzt bereits die 3.x-taugliche `Microsoft.OpenApi.Reader`-API (`OpenApiJsonReader`), die Behauptung „Reader-API quellkompatibel" ist gegen die Basisversion verifiziert.

**Schritt-Meldung:** Alle Angaben (einzige Migration SecurityOperationFilter + analoge Fixture, openapi 3.0.4, 67 Extensions, 34 Systemendpunkte, einheitliche Versionen) konnten unabhängig reproduziert werden.
