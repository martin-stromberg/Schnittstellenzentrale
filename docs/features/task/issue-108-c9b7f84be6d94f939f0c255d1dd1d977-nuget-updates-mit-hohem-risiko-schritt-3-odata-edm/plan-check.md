# Planprüfung – Schritt 3 (F5)

**Status: Plan vollständig**

## Abgleich Anforderung ↔ Plan

| Akzeptanzkriterium (requirement.md) | Plan-Abdeckung |
|---|---|
| Kein Edm-Versions-Split (`--include-transitive` einheitlich) | S1 + Umsetzungsschritt 3 — prüft explizit, dass weder `8.4.3` noch `9.0.0-preview.4` verbleibt |
| `ODataControllerIntegrationTests` (78) grün | S3 |
| `dotnet test --filter "Category!=OsInterface"` grün (Baseline 641) | S4 |
| OData-Funktionsverifikation (Import-Integrationstests + `[EnableQuery]`-Endpunkte) | S2 + S3 |
| `dotnet list package --vulnerable` ohne Befund | S5 |
| Strategie + Exit-Kriterium in `decisions.md` | Umsetzungsschritt 4 + S7 |
| Edm-9-API-Prüfung von `ODataImportService.cs` (`CsdlReader.Parse`, `IEdmModel`/`IEdmEntitySet`/`IEdmOperation`/`IEdmAction`) | Umsetzungsschritt 2 (Build zwingt API-Verifikation; zusätzlich `ODataEdmModelBuilder.cs` im Web-Projekt abgedeckt — kompiliert ebenfalls gegen Edm) |
| Reale E2E-Abnahme (Kestrel + OData-Endpunkt) | S6 mit konkretem Auth-Flow und dokumentiertem Fallback |
| Minimal-Fallback 8.4.4 | Risiken/Fallback-Abschnitt |

## Testbedarf-Prüfung

- Keine neuen Tests nötig: Änderung ist eine Versionshebung; bestehende 78 Controller- + 25 Import-Tests + E2E bilden die Absicherung. Korrekt geplant.
- OsInterface-Lane korrekt ausgeschlossen (Auftragsvorgabe).
- E2E-Szenario ist vollständig (Start → Auth → Endpunkt → sichtbare Wirkung) und hat einen dokumentierten Fallback-Pfad für Negotiate.

## Geprüfte Nebenaspekte

- NU1510-/Warnungsrisiko durch neue direkte Referenz: gering (Edm wird real in `ODataImportService.cs` genutzt); S1 prüft NU-Warnungen explizit.
- `Microsoft.Spatial` wird von Edm 9.0.0 transitiv mitgezogen — keine direkte Referenz nötig; Auflösung via S1 sichtbar.
- Testprojekt löst Edm transitiv über die ProjectReferences auf 9.0.0 auf — unter „einheitlich" in S1 implizit abgedeckt; ausdrücklich nicht gefordert, aber der `--include-transitive`-Bericht der Lösung deckt es ab.
- Semver-Detail korrekt: `9.0.0` (stabil) > `9.0.0-preview.4` → erfüllt AspNetCore.OData-Mindestanforderung.

## Befunde

Keine. Plan ist umsetzbar und deckt alle Akzeptanzkriterien ab.
