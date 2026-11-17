# Plan-Gegenprüfung

## Ergebnis

**Status:** Plan vollständig

## Abgleich Akzeptanzkriterien

| Akzeptanzkriterium | Umsetzung im Plan | Testnachweis im Plan | Status |
|--------------------|-------------------|----------------------|--------|
| `DELETE /api/application-groups/{id}` erhält `content`-Parameter | `DeleteAsync(id, [FromQuery] string? content)` mit Validierung + Mapping (Schritt 3) | `ApplicationGroupsControllerIntegrationTests`-Varianten | Abgedeckt |
| `content=delete`: Gruppe + alle Anwendungen inkl. Endpunkte/Endpunktgruppen kaskadierend, atomar | Transaktionale Repository-Implementierung inkl. explizitem Laden der `EndpointGroup`-Entitäten (Restrict-sicher) | `DeleteGroup_WithDeleteMode_DeletesApplicationsEndpointsAndGroups` + Controller-Test | Abgedeckt |
| `content=detach`: Anwendungen explizit ungruppiert, dann Gruppe gelöscht, atomar | Gleiche Transaktion, `ApplicationGroupId=null` + Gruppen-Remove | `DeleteGroup_WithDetachMode_KeepsApplicationsUngrouped` + Controller-Test | Abgedeckt |
| Fehlschlag → kein Teilerfolg (Rollback) | `BeginTransactionAsync` ohne Commit bei Exception | `..._OnSaveFailure_RollsBackEverything` für beide Modi | Abgedeckt |
| Fehlende Gruppe → 404 | unverändert `NotFound` vor Repository-Aufruf | bestehender `DeleteApplicationGroup_WithInvalidId_Returns404` | Abgedeckt |
| Systemgruppen → 403 | unverändert `IsSystem`-Check | `DeleteApplicationGroup_WithDeleteModeAndSystemGroup_Returns403` | Abgedeckt |
| Änderungs-Benachrichtigung unverändert | `NotifyTreeChangedAsync` bei Team bleibt | implizit durch unveränderten Codepfad + Token-/204-Assertions | Abgedeckt |
| Fehlender `content` → bisheriges Verhalten (= detach) | Default-Mapping `null` → `Detach`; Server-Default unverändert | `DeleteApplicationGroup_WithoutContent_BehavesLikeDetach` | Abgedeckt |
| Ungültiger `content`-Wert → 400 | Validierung vor DB-Zugriff | `DeleteApplicationGroup_WithInvalidContent_Returns400` | Abgedeckt |
| API-Client parametrisierte Lösch-Variante | `DeleteGroupAsync(int id, ApplicationGroupDeleteMode mode)` in Interface + Client | `DeleteGroupAsync_WithDeleteMode_/WithDetachMode_SendsContentQueryParam` | Abgedeckt |
| api.md-Dokumentation | Schritt 6 der Umsetzungsreihenfolge, Task 11 | manueller Nachweis | Abgedeckt |
| Bestehende Suiten bleiben grün | Overload-Design bricht keine Aufrufer (`ODataApplicationGroupsController`, Fakes, UI) | Baseline 577/577 dokumentiert; Abschlusslauf | Abgedeckt |

## E2E-Abdeckung

| Benutzerfluss / Akzeptanzkriterium | Geplanter E2E-Test | Status |
|------------------------------------|--------------------|--------|
| UI-Löschdialog | — | Nicht erforderlich mit Begründung: Kein geänderter Benutzerfluss in diesem Schritt; UI-Umstellung explizit in Schritt 4 ausgelagert. `WorkspacesLayout` ruft unverändert `DeleteGroupAsync(id)` → detach-Semantik wie bisher. |

## Hinweise

- Der Plan weist korrekt darauf hin, dass `EndpointGroup.ChildGroups` auf `Restrict` steht; die geforderte Kaskade bei `content=delete` ist nur mit geladenen Entitäten (EF-Löschreihenfolge) sicher — im Plan explizit berücksichtigt.
- `docs/help/api/api.md` dokumentiert `DELETE /api/application-groups/{id}` bisher gar nicht — der Abschnitt wird neu angelegt, was die Anforderung „um den neuen Parameter ergänzen" vollständig abdeckt.
