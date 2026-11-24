# Umsetzungsplan – Schritt 3 (F5): Microsoft.OData.Edm 9.0.0 + AspNetCore.OData-Strategie

## Scope-Klärung

- **Verarbeitungseinheit:** NuGet-Paketversions-Hebung `Microsoft.OData.Edm` 8.4.3 → 9.0.0 (direkte Referenz nur in `Schnittstellenzentrale.Infrastructure.csproj`) + API-Verifikation der Edm-Nutzung + Dokumentation der AspNetCore.OData-Strategie.
- **Auslösendes Ereignis:** Entwicklungsschritt 3 des Projekts Issue #108 (Paketprüfungsbefund F5: binärfragiler Versions-Split 8.4.3/9.0.0-preview.4).
- **Erwartete Wirkung:** Einheitliche Edm-Auflösung 9.0.0 in allen Projekten; kompilierter und laufender OData-Pfad (Import + Controller) gegen eine einzige Edm-Version; dokumentierte Strategie mit Exit-Kriterium.
- **Scope-Ausschlüsse:** Keine.

  - Annahmen (keine Ausschlüsse, keine Bestätigung nötig — im Auftrag vorgegeben oder technisch zwingend):
    - `Microsoft.AspNetCore.OData` bleibt auf `10.0.0-preview.2` (vorgegebene Entscheidung).
    - `Microsoft.OData.Core`/`ModelBuilder` bleiben transitive Auflösung (kein Pinning — keine direkte Code-Referenz nötig, Floating-/Patch-Strategie konsistent zu Schritt 1/2).
    - Keine Änderungen an resx, Razor, UI — reine Build-/Dokumentations-Änderung.
    - Fallback `8.4.4` nur bei konkretem Befund.

## Umsetzungsschritte

1. `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj:36`: `Version="8.4.3"` → `Version="9.0.0"` (kein Floating — externe Paketlinie wie übrige Third-Party-Pins, konsistent mit `Microsoft.OpenApi 2.10.0` etc.).
2. `dotnet restore --force` + `dotnet build` — Compilerfehler in `ODataImportService.cs` / `ODataEdmModelBuilder.cs` zeigen Edm-9-API-Breaks; ggf. minimal anpassen (keine Verhaltensänderung).
3. `dotnet list package --include-transitive` — verifizieren: `Microsoft.OData.Edm` einheitlich `9.0.0` (Infrastructure direkt + Web transitiv, AspNetCore.OData-Anforderung `>= 9.0.0-preview.4` erfüllt).
4. `docs/projects/task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko/decisions.md`: Abschnitt „F5 – OData-Stack-Strategie" ergänzen (Status, Entscheidung „auf 10.0.0-preview.2 verharren", Begründung, Exit-Kriterium „stabiler 10.x-/11.x-Release", Konsequenz Edm 9.0.0 direkt referenziert).
5. `docs/RELEASE_NOTES.md`: Eintrag analog Schritt-2-Stil.

## Akzeptanzszenarien

- **S1 (Happy Path, Build):** `dotnet restore` + `dotnet build` → 0 Fehler, keine neuen NU-Warnungen; `dotnet list package --include-transitive` zeigt Edm `9.0.0` einheitlich (kein `8.4.3`, kein `9.0.0-preview.4` mehr).
- **S2 (Funktionsverifikation Import):** `ODataImportServiceTests` (23), `ODataImportServiceRealMetadataTests` (1), `ODataImportServiceIntegrationTests` (1) grün — `CsdlReader.Parse`-Pfad gegen Edm 9.0.0 verifiziert.
- **S3 (Funktionsverifikation Controller):** `ODataControllerIntegrationTests` — 78 Tests grün (`[EnableQuery]`-Endpunkte, `$metadata`, Query-Optionen, Schreiboperationen).
- **S4 (Regressionsfrei):** `dotnet test --filter "Category!=OsInterface"` → 641/641 (Baseline Schritt 2).
- **S5 (Sicherheit):** `dotnet list package --vulnerable` → kein Befund.
- **S6 (Reale E2E-Abnahme):** App via Kestrel starten (`dotnet run --project src/Schnittstellenzentrale --launch-profile http` → `http://localhost:5128`), dann:
  - `GET /odatav4/$metadata` → 200, CSDL-XML mit EntitySets `Applications`, `ApplicationGroups`, `Endpoints`, `EndpointGroups` (verifiziert `ODataEdmModelBuilder` gegen Edm 9.0.0 end-to-end).
  - `GET /odatav4/Authenticate()` mit Windows-Default-Credentials (Negotiate) → 200 + Token; anschließend `GET /odatav4/Applications` mit `Authorization: Bearer <token>` → 200 JSON. Fallback falls Negotiate im Kontext nicht greift: dokumentiert mit Begründung.
- **S7 (Dokumentation):** `decisions.md` enthält F5-Entscheidung inkl. Exit-Kriterium.

## Test-Strategie

Keine neuen Tests — der Befund ist ein Paket-Versions-Split; Verifikation über bestehende Suite (78 OData-Controller-Integrationstests + Import-Tests) und Paket-Auflösungsbericht. OsInterface-Lane nicht erforderlich (Auftragsvorgabe).

## Offene Punkte

Keine.

## Risiken / Fallback

- Edm-9-Breaks → Code-Anpassung; unlösbar → `8.4.4` + dokumentierte Abweichung in decisions.md.
- Transitive `OData.Core 9.0.0-preview.4`/`ModelBuilder 3.0.0-preview.1` inkompatibel mit Edm 9.0.0 → Laufzeitfehler in S2/S3/S6 sichtbar; dann Entscheidung neu treffen (z. B. Edm-Preview-Pin `9.0.0-preview.4` oder Fallback).
