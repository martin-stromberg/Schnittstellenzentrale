# Plan-Prüfung – F3: Microsoft.OpenApi 3.11.1 + Swashbuckle.AspNetCore 11.0.1

**Status: Plan vollständig**

*(Dokumentierte Abweichung: Planprüfung wurde wegen nicht startbarer Unteragenten vom Agenten selbst durchgeführt — mit explizit adversarialem Blick gegen die Anforderung.)*

## Prüfung gegen die Kundenanforderung

| Anforderung | Im Plan abgedeckt? |
|-------------|--------------------|
| Microsoft.OpenApi 2.10.0 → 3.11.1 in Core UND Infrastructure | Ja — Arbeitsschritt 1 listet beide csproj-Dateien |
| Swashbuckle.AspNetCore 10.1.7 → 11.0.1 im Web-Projekt | Ja — Arbeitsschritt 1 |
| Gemeinsamer Schritt (kein OpenApi-2/3-Mix) | Ja — beide Paketupdates in einem Schritt; Kohärenz-Nachweis in Schritt 3 |
| Zielversionen erneut konkretisieren + Wahl begründen | Ja — inventory.md dokumentiert: 3.11.1/11.0.1 sind die aktuellen stabilen Releases, keine neuere stabile Linie |
| API-Migration SwaggerImportService.cs (Reader) | Ja — Arbeitsschritt 2 |
| API-Migration SwaggerOperationHelper.cs | Ja — Arbeitsschritt 2 |
| API-Migration der drei Filter + Program.cs | Ja — Arbeitsschritt 2 |
| Test-Fixtures (SwaggerImportServiceTests, SystemEndpointSyncServiceTests, SystemEntryInitializerTests) | Ja — Arbeitsschritt 2 |
| Swashbuckle-11-Breaking-Changes gesondert prüfen | Ja — eigener Unterpunkt in Arbeitsschritt 2 (ISwaggerProvider, Filter-Interfaces, anyOf-Response-Kombinierung, Redoc) |
| `dotnet test --filter "Category!=OsInterface"` grün (Baseline 641) | Ja — Schritt 4 + Szenario 3 |
| Swagger-Funktionsverifikation (swagger.json valide, UI erreichbar) | Ja — Schritt 5 + Szenario 1 |
| Startup-Abgleich verifiziert | Ja — Schritt 5 + Szenario 2 |
| Kein OpenApi-2/3-Mix (transitiv geprüft) | Ja — Schritt 3 + Szenario 4 |
| `dotnet list package --vulnerable` ohne Befund | Ja — Schritt 3 + Szenario 4 |
| Reale E2E-Abnahme (Kestrel, GET-Endpunkte, Dokumentation in test-results.md) | Ja — Schritt 5 |
| OsInterface-Lane nicht erforderlich | Ja — Scope-Ausschluss dokumentiert |
| XML-Docs (CS1591) beachten | Ja — in inventory.md als Rahmenbedingung festgehalten |

## Prüfung auf Vollständigkeit des Plan-Schemas

- **Akzeptanzszenarien mit Happy Path vom Auslöser bis zur Wirkung:** vorhanden (Szenario 1: App-Start → HTTP-Request → sichtbares Dokument; Szenario 2: Startup → Abgleich → persistierte Endpunkte).
- **Fehler-/Randfälle:** Szenario 5 deckt den Parse-Fehlerpfad ab; zusätzlich ist der Fehlerpfad der Reader-Migration explizit als „Fehlerbehandlung beibehalten" markiert.
- **Kein UI-Flow** → keine geometrischen E2E-Assertions erforderlich; Sichtprüfung von Screenshots entfällt (keine Screenshots — CLI-/HTTP-Abnahme).
- **Testbedarf:** Unit-/Integrationstests existieren bereits und werden migriert statt neu erfunden; CLAUDE.md-Testanforderung (gemockter HttpMessageHandler + WebApplicationFactory) ist für die bestehenden Fixtures eingehalten.

## Gefundene Risiken / Hinweise für die Implementierung

1. Compilergetriebene Migration ist der Kern — die antizipierte Liste in Schritt 2 ist bewusst als „zu verifizieren" formuliert, da der v3-Upgrade-Guide nicht jede Signaturänderung einzeln aufzählt.
2. `AddOperation(HttpMethod, …)`-Signatur und `Operations`-Key-Typ in 3.x nicht dokumentiert — Fallback in Schritt 2 enthalten (Key-ToString-Anpassung).
3. `dotnet list package --include-transitive` muss nach dem Update auch die `Swashbuckle.AspNetCore.SwaggerGen`-Auflösung zeigen (diese zieht Microsoft.OpenApi 3.11 transitiv — Deck mit direkter 3.11.1-Referenz).

## Fazit

Der Plan deckt jeden Satz der Kundenanforderung und alle Akzeptanzkriterien ab; Akzeptanzszenarien sind überprüfbar, Testbedarf (inkl. realer E2E-Abnahme) ist verankert. **Plan vollständig.**
