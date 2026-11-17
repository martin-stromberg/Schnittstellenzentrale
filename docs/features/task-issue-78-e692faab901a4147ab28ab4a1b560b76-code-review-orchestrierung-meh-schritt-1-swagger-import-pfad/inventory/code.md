# Code-Fundstellen

## `SystemEndpointSyncService`
Datei: `src/Schnittstellenzentrale/SystemEndpointSyncService.cs`

- `ExecuteAsync` (Z. 28–65): Scope → `IApplicationRepository.GetSystemGroupAsync` → `IsSystem`-App suchen → `SyncEndpointsAsync`; Fehler nur geloggt.
- `SyncEndpointsAsync` (Z. 67–87): `ISwaggerProvider.GetSwagger("v1")` → `SwaggerOperationHelper.MapDocumentToEndpoints(document, systemApp.Id)` → **handgebauter Diff** nur `New`/`Removed` (Z. 76–84: `existingEndpoints` via `IEndpointRepository`, `ToHashSet`-Vergleiche) → `.WithBearerTokens(bearerTokens)` → `ApplyDiffAsync(diff)`.
- Direkter `IEndpointRepository`-Bezug: Z. 52 (Auflösung), Z. 76 (`GetEndpointsAsync`).

## `ISwaggerImportService`
Datei: `src/Schnittstellenzentrale.Core/Interfaces/ISwaggerImportService.cs`

- `ImportAsync(Application)` → `Task<ImportDiff>` (Z. 11)
- `ApplyDiffAsync(ImportDiff)` → `Task` (Z. 15)
- Kein `SyncFromDocumentAsync`.

## `SwaggerImportService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/SwaggerImportService.cs`

- DI: `IHttpClientFactory`, `IEndpointRepository`, `ICredentialService`, `ILogger<SwaggerImportService>`.
- `ImportAsync` (Z. 33–61): InterfaceUrl leer → leerer Diff; `FetchSwaggerStreamAsync` (HTTP) → `ParseSwaggerDocumentAsync` (Fehler → `ImportDiff.ErrorMessage`) → `MapDocumentToEndpoints` → `GetEndpointsAsync` → `ImportDiffCalculator.Calculate(...).WithBearerTokens(...)`. **Ab Z. 57 exakt die Strecke, die `SyncFromDocumentAsync` kapseln soll.**
- `ApplyDiffAsync` (Z. 85–110): keine `ErrorMessage`-Prüfung — ein Diff mit Fehler würde angewendet, wenn ein Aufrufer ihn übergibt.
- `SaveBearerTokenIfPresent` (Z. 112–130): Token via `CredentialTargetHelper.Build` → `ICredentialService.SavePassword`; Fehler → Warnung.

## `ImportDiffCalculator`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/ImportDiffCalculator.cs`

- `internal static`; `Calculate(existing, imported)` (Z. 8–36).
- Duplikatschlüssel in Bestand oder Import → `ImportDiff` nur mit `ErrorMessage` (Z. 11–15).
- `HasChanged`: `Name`, `Body`, `AuthenticationType`, `PreRequestScript`, `PostRequestScript` (Z. 61–68).
- `MergeExistingIdentity`: `Id`, `EndpointGroupId`, `RowVersion` vom Bestand; Header/QueryParameters als Kopien vom Bestand (Z. 70–88).
- Aufrufer: `SwaggerImportService.ImportAsync`, `ODataImportService.ImportAsync`. `SystemEndpointSyncService` nutzt ihn **nicht**.

## `SwaggerOperationHelper`
Datei: `src/Schnittstellenzentrale.Infrastructure/Helpers/SwaggerOperationHelper.cs`

- `public static`; `MapDocumentToEndpoints(OpenApiDocument, int applicationId)` → `(List<Endpoint>, Dictionary<string,string>)` (Z. 51–89): `x-sz-bearer-token`, `x-sz-pre-request-script`, `x-sz-post-request-script` (mit Authenticate-Namensersetzung), `Negotiate`-Erkennung, Header-Defaults.

## Weitere Fundstellen

- `Program.cs`: `AddScoped<ISwaggerImportService, SwaggerImportService>()` (Z. 137), `AddHostedService<SystemEndpointSyncService>()` (Z. 145), `AddSwaggerGen` mit `SwaggerDoc("v1", …)` (Z. 89–91).
- `CLAUDE.md`: „Explizite Ausnahme: `SystemEndpointSyncService` verwendet `IEndpointRepository` direkt beim Startup-Abgleich, da zu diesem Zeitpunkt kein zuverlässiger HTTP-Roundtrip möglich ist."
- `docs/help/schnittstellenzentrale/ablauf-technisch.md` (Abschnitt „Automatischer Endpunktabgleich", Z. 32–64) und `business-rules.md` (Abschnitt „Selektiver Endpunktabgleich beim App-Start", Z. 69–85) beschreiben ein teilweise veraltetes Verhalten (`UpdateEndpointNameAsync` existiert im Code nicht mehr) — bei der Dokumentation mitzupflegen.
- `src/Schnittstellenzentrale.Core/Schnittstellenzentrale.Core.csproj`: keine Paketreferenzen — für `OpenApiDocument` im Interface ist `Microsoft.OpenApi` (3.8.0, analog Infrastructure) hinzuzufügen.
