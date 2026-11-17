# Plan-Check

Status: **Plan vollständig**

Geprüft: `plan.md` gegen `requirement.md` und `inventory.md`
(kein Unteragent verfügbar — Selbstprüfung mit explizitem Negativabgleich).

## Abdeckung der Anforderung

| Anforderung | Plan-Element | Abgedeckt |
|---|---|---|
| `POST /api/applications/{id}/swagger-import/apply` mit 401/404/422(Rest)/204 | S2 | Ja — Reihenfolge und Fehlervertrag exakt wie OData-Vorbild |
| Aggregat-Validierung: alle Diff-Endpunkte = Ziel-App-ID, sonst kein Apply + verständlicher Fehler | S1 + S2 | Ja — Service wirft `InvalidOperationException`, Controller mappt auf `422 { errorMessage }` |
| `IApplicationApiClient`-Methode, Fehler wie OData durchgereicht | S3 | Ja — identisches Fehlerschema (`InvalidOperationException` mit Server-`errorMessage`) |
| `SwaggerImportDialog` ruft API-Pfad statt Service | S4 | Ja |
| Unit-Tests gemockter HTTP-Handler (Client) | S5 | Ja — 2 Tests (Pfad/Token, Fehlerdurchreichung) |
| Integrationstests `WebApplicationFactory` (204/401/404/422) | S5 | Ja — 5 Tests inkl. Aggregat-Ablehnung |
| Bestehende Suiten inkl. Playwright grün | Teststrategie | Ja — explizit SwaggerImportTests + LayoutSmokeTests |
| `docs/help/api/api.md`-Ergänzung | S6 | Ja |
| resx-Pflege bei neuen UI-Texten | S4/S6 | Ja — begründeter Verzicht: keine neuen Texte, bestehender `ImportDialog_Error_Apply` zeigt Servermeldung |

## Kritische Gegenprüfung

- **Signatur-Bruch `ApplyDiffAsync(diff, applicationId)`:** Alle Aufrufer erfasst?
  `SyncFromDocumentAsync` (Z. 77), `SwaggerImportDialog.razor` (entfällt durch S4),
  `SwaggerImportServiceTests` (5 Stellen). Grep-Befund zeigt keine weiteren Aufrufer.
  `SystemEndpointSyncService` nutzt nur `SyncFromDocumentAsync` — unberührt. ✔
- **Playwright-Abhängigkeit:** `SwaggerImportTests` ruft den Apply über den
  geänderten Dialog — ein Bruch des Client-Pfads fiele hier auf. ✔
- **Risiko `[ValidateNever]`:** übernommen wie OData — nötig wegen rekursiver
  Navigationen im `Endpoint`-Modell. ✔
- **bUnit:** `ImportDialogTests` registriert nur `IApplicationApiClient`-Mock;
  `SwaggerImportDialog` benötigt nach Umstellung keinen `ISwaggerImportService`
  mehr — kein fehlendes DI-Setup in Tests. ✔
- **E2E-Abnahme:** UI-Fluss real über PlaywrightServer abgedeckt (kein
  headless-Verzicht nötig, da .NET/Playwright in dieser Umgebung laufen). ✔

## Ergebnis

Keine Lücken, keine offenen Punkte, keine unbestätigten Scope-Ausschlüsse.
Plan ist zur Implementierung freigegeben.
