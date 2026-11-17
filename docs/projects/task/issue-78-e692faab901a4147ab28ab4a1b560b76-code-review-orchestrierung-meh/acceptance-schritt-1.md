# Abnahmeprüfung – Entwicklungsschritt 1

## Ergebnis

**Status:** Anforderung vollständig erfüllt

## Abweichungen

Keine.

## Hinweise

- Neuer Service-Einstieg vorhanden: `ISwaggerImportService.SyncFromDocumentAsync(OpenApiDocument, int)` (`src/Schnittstellenzentrale.Core/Interfaces/ISwaggerImportService.cs:18`), implementiert in `src/Schnittstellenzentrale.Infrastructure/Services/SwaggerImportService.cs:64-79`. Der Pfad nutzt exakt die Bausteine des manuellen Imports: `SwaggerOperationHelper.MapDocumentToEndpoints` (Ableitung inkl. `x-sz-bearer-token`, `x-sz-pre-request-script`, `x-sz-post-request-script`), `IEndpointRepository.GetEndpointsAsync` (Bestand laden), `ImportDiffCalculator.Calculate` (neu/geändert/entfernt inkl. Duplikatschlüssel-Erkennung) und `ApplyDiffAsync` (Anwendung inkl. Bearer-Token-Ablage im Credential Store).
- Hintergrunddienst vereinfacht wie gefordert: `src/Schnittstellenzentrale/SystemEndpointSyncService.cs` löst nur noch Systemgruppe/Systemanwendung auf, beschafft das Dokument via `ISwaggerProvider.GetSwagger("v1")` und ruft `SyncFromDocumentAsync`. Kein `IEndpointRepository`-Zugriff und keine handgebaute Differenzbildung mehr; die Tests registrieren `IEndpointRepository` bewusst nicht im DI-Container (`SystemEndpointSyncServiceTests.cs:33-36`), sodass ein Rückfall sofort auffiele.
- Fachliche Wirkung gegeben: `ImportDiffCalculator.HasChanged`/`MergeExistingIdentity` (`ImportDiffCalculator.cs:61-88`) aktualisiert Name, Body, `AuthenticationType` und Vor-/Nachher-Skripte; `ApplyDiffAsync` ruft `UpdateEndpointAsync`. Duplikatschlüssel in Import **oder** Bestand führen zu `ImportDiff.ErrorMessage` mit leerem Diff — nichts wird angewendet, Warnung wird geloggt, der Start läuft weiter (zusätzlich abgesichert durch die neue Fehler-Guard in `ApplyDiffAsync`, `SwaggerImportService.cs:105-109`, und die catch-Blöcke im `SystemEndpointSyncService`).
- API-First-Ausnahme korrekt umgeschrieben: `CLAUDE.md` dokumentiert jetzt `IApplicationRepository` (Auflösen von Systemgruppe/-anwendung) als Ausnahme; die Endpunkt-Persistenz liegt serverseitig in `SwaggerImportService`. Hilfeseiten (`ablauf-technisch.md`, `business-rules.md`) konsistent nachgezogen.
- Testabdeckung: neue Tests für Duplikatschlüssel (`ImportDiffCalculatorTests`), für `SyncFromDocumentAsync` (neu/geändert/entfernt, Bearer-Token, Fehlerfall — `SwaggerImportServiceTests`) sowie umgebaute `SystemEndpointSyncServiceTests`. Verifiziert lokal: `dotnet test --filter "FullyQualifiedName!~Playwright"` → 565/565 grün; `dotnet test --filter "FullyQualifiedName~Playwright"` → 64/64 grün.
- Microsoft.OpenApi-Pin auf 2.10.0 (Infrastructure + neu in Core, da die Schnittstelle `OpenApiDocument` exponiert) ist der dokumentierte, fachlich begründete Begleitbefund — kein Mangel.
