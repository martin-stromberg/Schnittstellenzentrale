# Übersetzte Anforderung – Einheitlicher Swagger-Import-Ausführungspfad inklusive Startabgleich (Issue #87)

## Fachliche Zusammenfassung

Der automatische Endpunktabgleich der Systemanwendung beim App-Start (`SystemEndpointSyncService`) soll denselben serverseitigen Ausführungspfad nutzen wie der manuelle Swagger-Import — von einem bereits vorliegenden `OpenApiDocument` bis zur persistierten Endpunktmenge. Dazu wird `ISwaggerImportService` um einen Pass-Einstieg `SyncFromDocumentAsync` erweitert (Map → Bestand laden → Diff berechnen → anwenden, inkl. Bearer-Token-Übernahme und Duplikatschlüssel-Erkennung). Der Hintergrunddienst wird auf Dokumentbeschaffung plus Service-Aufruf reduziert; seine handgebaute Differenzbildung entfällt.

## Fachlicher Scope

### Verarbeitungseinheit

Ein Swagger-Import-Durchlauf für ein bereits vorliegendes `OpenApiDocument`: Endpunkte aus dem Dokument ableiten, Endpunktbestand der Zielanwendung laden, Differenz (neu / geändert / entfernt) berechnen und anwenden. Nicht zum Scope gehört der Abruf des Dokuments (bleibt `ISwaggerProvider.GetSwagger("v1")` im Hintergrunddienst bzw. HTTP-Fetch in `ImportAsync`).

### Auslösende Ereignisse

- App-Start: `SystemEndpointSyncService` (registrierter `IHostedService`) führt den einmaligen Abgleich der Systemanwendung durch.

### Erwartete Wirkung

- Geänderte System-Endpunkte werden beim Start aktualisiert (`ChangedEndpoints`: Name, Body, `AuthenticationType`, Pre-/Post-Request-Skripte) — bisher wurden nur neue und entfernte Endpunkte verarbeitet.
- Duplikatschlüssel (`{Method}:{RelativePath}`) werden erkannt und als Fehler behandelt statt still per `ToHashSet()` ignoriert zu werden.
- Im Dokument mitgelieferte Bearer-Tokens (`x-sz-bearer-token`) werden wie im manuellen Import übernommen (Credential Store).
- Ein Dokument mit Fehler (`ImportDiff.ErrorMessage` gesetzt, z. B. Duplikatschlüssel) führt zu keiner Anwendung der Änderungen; der Fehler wird geloggt, der Start läuft weiter.
- Die in den Entwicklungskonventionen (`CLAUDE.md`) dokumentierte Ausnahme vom API-First-Prinzip wird dem neuen Ausführungsort angepasst: `SystemEndpointSyncService` greift nicht mehr auf `IEndpointRepository` zu; verbleibend ist nur noch der direkte `IApplicationRepository`-Zugriff zum Auflösen von Systemgruppe/-anwendung (kein zuverlässiger HTTP-Roundtrip beim Start).

### Umgang mit fehlenden Objekten

- Fehlende Systemgruppe oder fehlende Systemanwendung: Warnung loggen, Abgleich überspringen (unverändert).
- `ImportDiff` mit `ErrorMessage`: nichts anwenden, Fehler loggen, Start läuft weiter.
- Exceptions im Abgleich: loggen, niemals werfen (unverändert — die Anwendung startet in jedem Fall).

### Scope-Ausschlüsse

- Keine Änderung am manuellen Import-UI-Pfad (`SwaggerImportDialog` ruft weiterhin `ApplyDiffAsync`; die API-Umstellung erfolgt in Schritt 2 / Issue #88).
- Keine neuen API-Endpunkte; kein Verhalten am `ApplicationImportController`.
- `ImportAsync` (HTTP-Fetch + Parse) bleibt unverändert.
- Keine UI-Änderungen.

## Betroffene Klassen und Komponenten

### Interfaces (Schnittstellenzentrale.Core)

- `ISwaggerImportService` — neue Methode `SyncFromDocumentAsync(OpenApiDocument document, int applicationId)` als Pass-Einstieg. Das Core-Projekt benötigt dafür eine Paketreferenz auf `Microsoft.OpenApi` (Typ liegt im `Microsoft.OpenApi`-Paket, Version analog Infrastructure: 3.8.0).

### Services / Logik

- `SwaggerImportService` (Infrastructure) — Implementierung von `SyncFromDocumentAsync` unter Wiederverwendung von `SwaggerOperationHelper.MapDocumentToEndpoints`, `IEndpointRepository.GetEndpointsAsync`, `ImportDiffCalculator.Calculate`, `WithBearerTokens` und `ApplyDiffAsync`; `ErrorMessage` im Diff → nichts anwenden, Warnung loggen.
- `SystemEndpointSyncService` (Webprojekt) — vereinfacht auf: Systemgruppe/Systemanwendung auflösen, `ISwaggerProvider.GetSwagger("v1")`, `SyncFromDocumentAsync` aufrufen; eigene Diff-Bildung (Zeilen 67–87) und `IEndpointRepository`-Bezug entfallen.
- `ImportDiffCalculator` — bleibt `internal`, unverändert; wird nun auch vom Startabgleich über den Service mitgenutzt (Duplikatschlüssel-Erkennung inklusive).

### Entwicklungskonventionen

- `CLAUDE.md` — die dokumentierte `IEndpointRepository`-Ausnahme für `SystemEndpointSyncService` ist auf den neuen Ort umzuschreiben: direkter Persistenz-Zugriff verbleibt nur beim Auflösen der Systemgruppe (`IApplicationRepository`), die Endpunkt-Persistenz liegt im Infrastructure-Service.

### Tests (Schnittstellenzentrale.Tests)

- `SystemEndpointSyncServiceTests` — Anpassung auf die neue Semantik: Service-Pfad (`SyncFromDocumentAsync`) statt eigener Differenzbildung; `IEndpointRepository`-Mock entfällt aus der DI (belegt, dass der Dienst die Persistenz nicht mehr direkt nutzt).
- `SwaggerImportServiceTests` — neue Tests für `SyncFromDocumentAsync` (neu/geändert/entfernt angewendet, Bearer-Token gespeichert, Fehler-Diff → keine Anwendung).
- `ImportDiffCalculatorTests` — Ergänzung Duplikatschlüssel-Erkennung (bisher nicht testseitig abgedeckt).

## Implementierungsansatz

- `SyncFromDocumentAsync` gibt das angewendete bzw. verworfene `ImportDiff` zurück (`Task<ImportDiff>`), damit Aufrufer und Tests das Ergebnis inkl. `ErrorMessage` prüfen können.
- `ApplyDiffAsync` erhält eine Defensiv-Prüfung: `ErrorMessage` gesetzt → Warnung loggen, nichts anwenden (schützt alle Aufrufer vor dem Versand eines Fehler-Diffs).
- Fehlerbehandlung im Hintergrunddienst bleibt wie bisher: catch + log, kein Rethrow.

## Konfiguration

Keine neue Konfiguration erforderlich.

## Offene Fragen

Keine. Methodenname (`SyncFromDocumentAsync`) und Issue-Bezug (#87) sind im Projektkontext bereits entschieden.
