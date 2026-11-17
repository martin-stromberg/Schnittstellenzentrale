# Umsetzungsplan – Einheitlicher Swagger-Import-Ausführungspfad inkl. Startabgleich (Issue #87)

## Zielbild

Der automatische Endpunktabgleich der Systemanwendung beim App-Start läuft über denselben serverseitigen Ausführungspfad wie der manuelle Swagger-Import: `ISwaggerImportService.SyncFromDocumentAsync(OpenApiDocument, int)` kapselt die gesamte Strecke vom vorliegenden Dokument bis zur persistierten Endpunktmenge (Map → Bestand laden → Diff → Apply inkl. Bearer-Tokens und Duplikatschlüssel-Erkennung). `SystemEndpointSyncService` reduziert sich auf Systemauflösung + Dokumentbeschaffung + Service-Aufruf und greift nicht mehr auf `IEndpointRepository` zu.

## Scope-Klärung

- **Verarbeitungseinheit:** Ein Sync-Durchlauf pro `OpenApiDocument` + Zielanwendung.
- **Auslöser:** App-Start (`IHostedService`), einmalig.
- **Wirkung:** `ChangedEndpoints` werden künftig aktualisiert (Name, Body, `AuthenticationType`, Skripte); Duplikatschlüssel → Fehler, keine Anwendung, Log + Start läuft weiter.
- **Fehlende Objekte:** Fehlende Systemgruppe/-anwendung → überspringen (unverändert). `ErrorMessage` im Diff → nichts anwenden.

### Scope-Ausschlüsse

| Ausschluss | Status | Begründung |
|------------|--------|------------|
| UI-Import (`SwaggerImportDialog`) bleibt auf `ApplyDiffAsync` | bestätigt (Projektleiter, Projektplan) | API-Umstellung ist eigener Schritt 2 / Issue #88 |
| Keine neue REST-API, kein Controller-Umbau | bestätigt | Scope dieses Schritts ist nur der Service-Einstieg + Worker |
| `ImportAsync` (HTTP-Fetch/Parse) unverändert | bestätigt | Scope beginnt am vorliegenden Dokument |

## Akzeptanzszenarien

1. **Happy Path Startabgleich (neu/geändert/entfernt):** Gegeben sei eine Systemanwendung mit persistiertem Endpunktbestand; das von `ISwaggerProvider` gelieferte Dokument enthält einen neuen, einen unverändert geänderten (z. B. anderer Name) und lässt einen Bestands-Endpunkt weg → beim Start wird `SyncFromDocumentAsync` aufgerufen und der Bestand enthält danach den neuen Endpunkt, den aktualisierten Endpunkt und den entfernten nicht mehr. (Nachweis: Unit-Tests + reale E2E-Abnahme, s. u.)
2. **Geänderter Endpunkt wird aktualisiert:** Bestands-Endpunkt mit abweichendem `Name`/`AuthenticationType`/Skripten zum Dokument → `UpdateEndpointAsync` wird mit den Dokumentwerten aufgerufen (`ChangedEndpoints`-Pfad, bisher nicht existent).
3. **Fehler-Diff → keine Anwendung:** Bestand oder Dokument erzeugen Duplikatschlüssel → `ImportDiff.ErrorMessage` gesetzt → kein `Add/Update/Delete`, Warnung im Log, `ExecuteAsync` endet ohne Exception.
4. **Bearer-Token-Übernahme:** Dokument mit `x-sz-bearer-token` → beim Anlegen/Aktualisieren wird `ICredentialService.SavePassword` aufgerufen.
5. **Robustheit:** `ISwaggerProvider` oder `SyncFromDocumentAsync` wirft → Fehler geloggt, `ExecuteAsync` wirft nicht, App-Start läuft weiter. Fehlende Systemgruppe/-anwendung → Warnung, kein Service-Aufruf.
6. **Konvention gepflegt:** `SystemEndpointSyncService` referenziert `IEndpointRepository` nicht mehr; `CLAUDE.md` beschreibt die Ausnahme am neuen Ort korrekt.

## Umsetzungsschritte

### 1. Core: Interface + Paketreferenz

- `src/Schnittstellenzentrale.Core/Schnittstellenzentrale.Core.csproj`: `PackageReference Microsoft.OpenApi` Version `3.8.0` hinzufügen (gleiche Version wie Infrastructure).
- `ISwaggerImportService`: Methode ergänzen:
  ```csharp
  /// <summary>Synchronisiert die Endpunkte einer Anwendung aus einem bereits vorliegenden OpenAPI-Dokument.</summary>
  /// <param name="document">Das OpenAPI-Dokument.</param>
  /// <param name="applicationId">ID der Zielanwendung.</param>
  /// <returns>Die ermittelte (und ggf. angewendete) Importdifferenz.</returns>
  Task<ImportDiff> SyncFromDocumentAsync(OpenApiDocument document, int applicationId);
  ```
  (`using Microsoft.OpenApi;` ergänzen.)

### 2. Infrastructure: `SwaggerImportService`

- `SyncFromDocumentAsync` implementieren:
  ```csharp
  var (importedEndpoints, bearerTokens) = SwaggerOperationHelper.MapDocumentToEndpoints(document, applicationId);
  var existingEndpoints = await _endpointRepository.GetEndpointsAsync(applicationId);
  var diff = ImportDiffCalculator.Calculate(existingEndpoints, importedEndpoints).WithBearerTokens(bearerTokens);
  if (!string.IsNullOrEmpty(diff.ErrorMessage))
  {
      _logger.LogWarning("Endpunktabgleich für Anwendung {ApplicationId} übersprungen: {Error}", applicationId, diff.ErrorMessage);
      return diff;
  }
  await ApplyDiffAsync(diff);
  return diff;
  ```
- `ApplyDiffAsync` zu Beginn defensive `ErrorMessage`-Prüfung ergänzen (Warnung + Return) — schützt alle Aufrufer.

### 3. Web: `SystemEndpointSyncService` vereinfachen

- `SyncEndpointsAsync` entfernen; in `ExecuteAsync` im inneren `try`:
  ```csharp
  var swaggerProvider = scope.ServiceProvider.GetRequiredService<ISwaggerProvider>();
  var swaggerImportService = scope.ServiceProvider.GetRequiredService<ISwaggerImportService>();
  var document = swaggerProvider.GetSwagger("v1");
  await swaggerImportService.SyncFromDocumentAsync(document, systemApp.Id);
  ```
- `IEndpointRepository`-Auflösung und ungenutzte Usings (`Schnittstellenzentrale.Infrastructure.Helpers`, `Core.Helpers` falls nur für Keys) entfernen. `IApplicationRepository` bleibt (Systemgruppe/-anwendung auflösen).

### 4. Konventionen pflegen

- `CLAUDE.md`: Ausnahme-Text anpassen auf: „`SystemEndpointSyncService` verwendet `IApplicationRepository` direkt beim Startup-Abgleich (Auflösen von Systemgruppe/-anwendung), da zu diesem Zeitpunkt kein zuverlässiger HTTP-Roundtrip möglich ist. Die eigentliche Endpunkt-Persistenz läuft innerhalb von `SwaggerImportService.SyncFromDocumentAsync` (Infrastructure-Schicht)."

### 5. Tests

**`SystemEndpointSyncServiceTests` (überarbeitet):**
- `IEndpointRepository` aus der Test-DI entfernen (Compile-Nachweis, dass kein direkter Zugriff mehr erfolgt).
- Neu/angepasst:
  - `ExecuteAsync_CallsSyncFromDocumentAsync_WithDocumentAndApplicationId` — Verifikation `SyncFromDocumentAsync(doc, 1)` genau einmal, Dokument-Instanzidentität.
  - `ExecuteAsync_WhenSystemGroupMissing_LogsWarningAndSkips` (besteht, anpassen: kein `ApplyDiffAsync`→`SyncFromDocumentAsync` Verify).
  - `ExecuteAsync_WhenSystemAppMissing_LogsWarningAndSkips` (analog).
  - `ExecuteAsync_WhenSwaggerProviderThrows_LogsErrorAndDoesNotThrow` (anpassen).
  - `ExecuteAsync_WhenSyncFromDocumentAsyncThrows_LogsErrorAndDoesNotThrow`.
  - Idempotenz-/Return-Value-Tests entfallen (Semantik liegt jetzt im Service).

**`SwaggerImportServiceTests` (neu, `SyncFromDocumentAsync`):**
- Neue Endpunkte → `AddEndpointAsync` je neuem Endpunkt (inkl. Gruppenauflösung).
- Geänderter Bestands-Endpunkt (anderer `Name`) → `UpdateEndpointAsync` mit Dokument-Werten.
- Entfernter Endpunkt → `DeleteEndpointAsync`.
- Dokument mit `x-sz-bearer-token` → `SavePassword` aufgerufen.
- Duplikatschlüssel im Bestand → `ErrorMessage` im Rückgabe-Diff, keine Schreibaufrufe.
- Dokumentaufbau via `OpenApiDocument`-Objektgraph (Helper analog bisheriger Testdatei; `Microsoft.OpenApi` ist durch die Infrastructure-Referenz verfügbar).

**`ImportDiffCalculatorTests` (ergänzt):**
- `Calculate_WhenImportedContainsDuplicateKeys_ReturnsErrorMessage`
- `Calculate_WhenExistingContainsDuplicateKeys_ReturnsErrorMessage`

**Bestehende Suites:** `dotnet test --filter "FullyQualifiedName!~Playwright"` sowie die Playwright-Suite müssen grün bleiben; keine neuen Playwright-/Integrations-Tests (kein neuer API-Vertrag, kein UI).

### 6. Dokumentation

- `docs/help/schnittstellenzentrale/ablauf-technisch.md`: Abschnitt „Automatischer Endpunktabgleich" auf den neuen Ablauf bringen (Schritte 4–8 ersetzen: `GetSwagger("v1")` → `SyncFromDocumentAsync`; Komponentenliste + Mermaid aktualisieren).
- `docs/help/schnittstellenzentrale/business-rules.md`: Regel „Selektiver Endpunktabgleich beim App-Start" auf neue Semantik aktualisieren (ChangedEndpoints vollständig aktualisiert; Duplikatschlüssel → kein Apply; Umsetzung über `SyncFromDocumentAsync`; den veralteten `UpdateEndpointNameAsync`-Hinweis korrigieren).
- `README.md`: kein Update nötig (kein nutzerseitig sichtbarer neuer Ablauf/Vertrag).
- Release Notes: werden aus Conventional Commits generiert (`release.config.cjs`) — der `feat:`-Abschlusscommit mit `Closes #87` ist die Release-Notes-Quelle; keine separate Datei.

## Reale End-to-End-Abnahme

Die Anforderung berührt Persistenz → reale Abnahme erforderlich:

1. App real starten (`dotnet run`, SQLite-DB `schnittstellenzentrale.db`), `SystemEndpointSyncService` legt System-Endpunkte an (Happy Path, Szenario 1).
2. Einen persistierten System-Endpunkt in der DB mutieren (z. B. `Name` ändern), App erneut starten → Endpunkt wird auf den Dokumentwert zurückaktualisiert (Szenario 2 — die neue `ChangedEndpoints`-Wirkung, die es bisher nicht gab).

## Offene Punkte

Keine.
