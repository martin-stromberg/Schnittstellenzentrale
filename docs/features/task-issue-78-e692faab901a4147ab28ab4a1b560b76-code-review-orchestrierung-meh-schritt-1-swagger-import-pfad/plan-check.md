# Planprüfung – Einheitlicher Swagger-Import-Ausführungspfad (Issue #87)

**Status: Plan vollständig**

## Abdeckung Anforderung ↔ Plan

| Anforderungsaspekt | Abgedeckt durch |
|--------------------|-----------------|
| Neuer Einstieg `SyncFromDocumentAsync` (Dokument → Endpunkte → Bestand → Diff → Apply) | Schritt 1 (Interface) + Schritt 2 (Implementierung); Signatur `Task<ImportDiff>` liefert Ergebnis inkl. `ErrorMessage` an Aufrufer/Tests |
| Bearer-Token-Übernahme aus dem Dokument | `MapDocumentToEndpoints` + `WithBearerTokens` + `SaveBearerTokenIfPresent` (bereits vorhanden, wird durchgereicht); Test „Bearer-Token-Übernahme" |
| Duplikatschlüssel-Erkennung | `ImportDiffCalculator.Calculate` (vorhanden); neue Tests in `ImportDiffCalculatorTests` + Service-Test „Fehler-Diff → keine Anwendung" |
| Fehlerhaftes Dokument → keine Anwendung, Fehler geloggt, Start läuft weiter | `ErrorMessage`-Prüfung in `SyncFromDocumentAsync` (LogWarning + Return) + Defensiv-Prüfung in `ApplyDiffAsync`; Worker-Catch bleibt |
| Worker vereinfacht (nur Gruppe/App auflösen, Dokument holen, Service rufen; Hand-Diff entfällt) | Schritt 3 |
| Geänderte Endpunkte (Name, Body, Auth, Skripte) werden aktualisiert | Automatisch durch `ImportDiffCalculator`/`ApplyDiffAsync`; dedizierter Test + E2E-Szenario 2 |
| CLAUDE.md-Ausnahme prüfen/pflegen | Schritt 4 — Ausnahme wandert: `IEndpointRepository` entfällt, verbleibend `IApplicationRepository` (Systemgruppe/-anwendung, kein HTTP-Roundtrip beim Start); Text wird auf den verbleibenden Ort umgeschrieben |
| Angepasste + neue Unit-Tests; bestehende Suiten grün | Schritt 5 |

## Kritische Prüfpunkte

1. **`OpenApiDocument` im Core-Interface:** Erfordert `Microsoft.OpenApi`-Paketreferenz im Core-Projekt — im Plan als eigener Schritt enthalten (Version 3.8.0 identisch zu Infrastructure, kein Versionsdrift). ✓
2. **Swashbuckle-Typkompatibilität:** `ISwaggerProvider.GetSwagger` liefert `Microsoft.OpenApi.OpenApiDocument` — identischer Typ, kein Mapping nötig. ✓
3. **Rückgabetyp `Task<ImportDiff>` vs `Task`:** `Task<ImportDiff>` gewählt, damit Aufrufer/Tests das Ergebnis (inkl. `ErrorMessage`) prüfen können; konsistent zum vorhandenen `ImportAsync`-Shape. Kein Vertragsbruch, da neue Methode. ✓
4. **`ApplyDiffAsync`-Defensivprüfung:** Verändert Bestandsverhalten nicht (alle bisherigen Aufrufer übergeben nur fehlerfreie Diffs); verhindert künftig stille Anwendung eines Fehler-Diffs. ✓
5. **Test-Nachweis „kein direkter Repository-Zugriff":** Entfernen des `IEndpointRepository`-Mocks aus der Test-DI ist ein Compile-/Laufzeit-Nachweis (Auflösung würde `InvalidOperationException` werfen und vom inneren Catch geschluckt → der `SyncFromDocumentAsync`-Verify würde fehlschlagen). ✓
6. **Reale E2E-Abnahme:** Persistenz-relevant → Pflicht. Zwei-Lauf-Verfahren (Erststart legt Endpunkte an, Mutation + Neustart zeigt `ChangedEndpoints`-Wirkung) ist mit SQLite real durchführbar. ✓
7. **Playwright:** Kein UI-Anteil; Suite muss grün bleiben — im Plan enthalten. ✓
8. **Kein Konflikt mit Folgeschritten:** Schritt 2 (#88) ergänzt später `swagger-import/apply` und Aggregat-Validierung — die hier geplante `ApplyDiffAsync`-Prüfung kollidiert nicht damit. ✓

## Erkannte Lücken

Keine.
