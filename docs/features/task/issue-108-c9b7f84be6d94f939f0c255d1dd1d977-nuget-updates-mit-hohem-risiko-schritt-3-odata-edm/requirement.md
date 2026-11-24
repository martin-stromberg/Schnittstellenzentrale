# Anforderung – Schritt 3 (F5): Microsoft.OData.Edm → 9.0.0 + AspNetCore.OData-Strategie

**Quelle:** Projekt Issue #108, Entwicklungsschritt 3 (siehe `docs/projects/task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko/steps.md`), Befund F5.

**Basisbranch:** `task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko` (Schritte 1+2 gemergt: net11.0, SDK 11.0.100, Floatings 11.0.*, FrameworkReference Microsoft.AspNetCore.App in Infrastructure).

## Ziel

`Microsoft.OData.Edm` in `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj` von `8.4.3` auf die stabile `9.0.0` heben. Damit wird die Paketanforderung von `Microsoft.AspNetCore.OData 10.0.0-preview.2` (`>= 9.0.0-preview.4`) erfüllt und der binärfragile Versions-Split aufgehoben (Infrastructure kompilierte gegen 8.4.3, lief transitiv gegen 9.0.0-preview.4). Minimal-Fallback bei Problemen: `8.4.4`.

Zusätzlich: `Infrastructure/Services/ODataImportService.cs` (Verwendung von `CsdlReader.Parse`, `IEdmModel`, `IEdmEntitySet`, `IEdmOperation`, `IEdmAction`) gegen die Edm-9-API prüfen und ggf. anpassen.

## Entscheidung zur Microsoft.AspNetCore.OData-Strategie (vorgegeben)

In `docs/projects/task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko/decisions.md` dokumentieren:

- **GEWÄHLT (dokumentierte Annahme):** Auf `10.0.0-preview.2` verharren.
- **Begründung:** Kein stabiler 10.x-/11.x-Release existiert; preview.2 läuft unter net11 bereits seit Schritt 1 verifiziert (78/78 ODataControllerIntegrationTests); Preview-Tracking ohne Zielrelease erhöht nur das Risiko.
- **Exit-Kriterium:** Umstieg auf die stabile Linie, sobald ein stabiler 10.x- oder 11.x-Release verfügbar ist.

## Akzeptanzkriterien

1. Kein Versions-Split bei `Microsoft.OData.Edm` über die Projekte hinweg — `dotnet list package --include-transitive` zeigt einheitlich `9.0.0` (direkte Referenz in Infrastructure und transitive Auflösung über `Microsoft.AspNetCore.OData` einheitlich).
2. `ODataControllerIntegrationTests` (78 Tests) grün.
3. `dotnet test --filter "Category!=OsInterface"` vollständig grün (Baseline 641).
4. OData-Funktionsverifikation: `ODataImportServiceIntegrationTests` und `[EnableQuery]`-Endpunkte via `ODataControllerIntegrationTests`.
5. `dotnet list package --vulnerable` ohne Befund.
6. Strategie + Exit-Kriterium in `decisions.md` dokumentiert.

## Reale E2E-Abnahme

App real starten (Kestrel) und einen OData-Endpunkt abfragen (z. B. `GET /odatav4/Applications` → HTTP 200, JSON-Antwort). Ergebnis in `test-results.md` dokumentieren; bei Nicht-Durchführbarkeit begründet dokumentieren.

## Rahmenbedingungen / Scope

- OsInterface-/Playwright-Lane **nicht** erforderlich — nur die reguläre Lane.
- Kein Issue-Kommentar, kein Merge, kein Push, kein PR — Endzustand ist der Abschlusscommit auf dem Schritt-Branch.
- Keine Scope-Erweiterung: `Microsoft.OData.Core`/`ModelBuilder` bleiben transitiv; `Microsoft.AspNetCore.OData` bleibt auf 10.0.0-preview.2 (kein Preview-Tracking).
- Fallback `8.4.4` nur, wenn `9.0.0` konkrete Probleme verursacht.
