# Bestandsaufnahme – Einheitlicher Swagger-Import-Ausführungspfad (Schritt 1 / Issue #87)

## Kernbefund

Die Swagger-Import-Strecke ist zweigleisig implementiert:

- `SwaggerImportService.ImportAsync` bildet die Kette Fetch → Parse → `SwaggerOperationHelper.MapDocumentToEndpoints` → `ImportDiffCalculator.Calculate` → `WithBearerTokens` vollständig ab (`src/Schnittstellenzentrale.Infrastructure/Services/SwaggerImportService.cs`, Z. 33–61).
- `SystemEndpointSyncService.SyncEndpointsAsync` dupliziert diese Kette ab dem vorliegenden Dokument, baut den `ImportDiff` aber von Hand: nur `NewEndpoints`/`RemovedEndpoints` via `EndpointKeyHelper`-HashSets (`src/Schnittstellenzentrale/SystemEndpointSyncService.cs`, Z. 67–87). `ChangedEndpoints` wird nicht erzeugt, die Duplikatschlüssel-Erkennung des `ImportDiffCalculator` wird umgangen, und der Dienst greift direkt auf `IEndpointRepository` zu (Z. 52, 76) — die in `CLAUDE.md` dokumentierte Ausnahme vom API-First-Prinzip.

Eine Methode `SyncFromDocumentAsync` existiert an `ISwaggerImportService` noch nicht (`src/Schnittstellenzentrale.Core/Interfaces/ISwaggerImportService.cs` — nur `ImportAsync` + `ApplyDiffAsync`).

## Detaildokumente

- [inventory/code.md](inventory/code.md) — Fundstellen im Code (Methoden, Zeilen, Aufrufer)
- [inventory/tests.md](inventory/tests.md) — Bestehende Testabdeckung und Anpassungsbedarf
- [inventory/projekt-kontext.md](inventory/projekt-kontext.md) — Verweis auf die Projekt-Bestandsaufnahme des übergeordneten Vorhabens (#78)

## Rahmenbedingungen

- `ISwaggerImportService` liegt im Core-Projekt; `Microsoft.OpenApi` (Typ `OpenApiDocument`) ist bisher nur in Infrastructure referenziert (Version 3.8.0). Core hat aktuell **keine** Paketreferenzen.
- `ISwaggerProvider.GetSwagger("v1")` (Swashbuckle) liefert denselben Typ `Microsoft.OpenApi.OpenApiDocument`, den `SwaggerOperationHelper.MapDocumentToEndpoints` entgegennimmt — das Dokument liegt beim Startabgleich bereits in Prozessform vor.
- `ImportDiffCalculator` ist `internal static` in Infrastructure; `Calculate(existing, imported)` erkennt Duplikatschlüssel in beiden Mengen (`ErrorMessage`), vergleicht `Name`/`Body`/`AuthenticationType`/Skripte und merged Bestands-`Id`/`EndpointGroupId`/`RowVersion` sowie Header/QueryParameters-Kopien in `ChangedEndpoints`.
- `ApplyDiffAsync` (Z. 85–110) löst für `NewEndpoints` Gruppen per `EndpointGroupHelper.ResolveGroupIdAsync` auf, ruft `AddEndpointAsync`/`UpdateEndpointAsync`/`DeleteEndpointAsync` und speichert Bearer-Tokens via `ICredentialService.SavePassword`. Es prüft **nicht** `diff.ErrorMessage`.
- `ISwaggerImportService` ist als Scoped in `Program.cs` (Z. 137) registriert; `SystemEndpointSyncService` als `IHostedService` (Z. 145) und arbeitet über einen eigenen DI-Scope.
- Fehlerbehandlung im Hintergrunddienst: doppelter `try/catch`, nur Logging — der App-Start läuft immer weiter.
