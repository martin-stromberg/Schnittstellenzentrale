# Projektplan-Gegenprüfung

## Ergebnis

**Status:** Projektplan lückenhaft

## Abgleich Anforderung ↔ Entwicklungsschritte

| Anforderungspunkt | Abgedeckt durch Schritt(e) | Status |
|--------------------|-----------------------------|--------|
| `ISwaggerImportService.SyncFromDocumentAsync` (Map → Diff → Apply inkl. `WithBearerTokens`, Duplikatschlüssel-Erkennung) | 1 | Abgedeckt |
| `SystemEndpointSyncService` vereinfachen (Systemgruppe/-anwendung auflösen, `ISwaggerProvider.GetSwagger("v1")`, Service-Aufruf; eigene Diff-Bildung und `IEndpointRepository`-Bezug entfallen) | 1 | Abgedeckt |
| Startabgleich verarbeitet `ChangedEndpoints` (Name, Body, `AuthenticationType`, Pre-/Post-Skripte) | 1 | Abgedeckt |
| `ImportDiff` mit `ErrorMessage` beim Startabgleich: kein Apply, Fehler loggen, Start läuft weiter | 1 | Abgedeckt |
| Neuer Endpunkt `POST /api/applications/{id}/swagger-import/apply` (Token `401`, `404` bei fehlender Anwendung, `422` bei `InterfaceType != Rest`, `204` bei Erfolg) | 2 | Abgedeckt |
| `ApplyDiffAsync`-Validierung: InterfaceType-/Aggregat-Zugehörigkeit des Diffs | 2 | Abgedeckt (InterfaceType über Endpunkt-`422`, Aggregat-Validierung in der Apply-Logik — Ort der InterfaceType-Prüfung als Umsetzungsdetail akzeptabel) |
| `IApplicationApiClient`/`ApplicationApiClient.ApplySwaggerDiffAsync` inkl. Fehlerdurchreichung | 2 | Abgedeckt |
| `SwaggerImportDialog` ruft ApiClient statt `ISwaggerImportService` | 2 | Abgedeckt |
| `DELETE /api/application-groups/{id}?content=delete|detach` serverseitig atomar (EF-Transaktion, `404`, `403` Systemgruppe, Rollback bei Fehlschlag) | 3 | Abgedeckt |
| `content`-Parameter-Semantik: fehlend = bisheriges Verhalten (`detach`), ungültig = `400` | 3 | Abgedeckt |
| `IApplicationRepository`-Erweiterung für transaktionales Löschen mit Inhalt | 3 | Abgedeckt (atomare Transaktion beschrieben; Methodenname ist Umsetzungsdetail) |
| `IApplicationApiClient.DeleteGroupAsync` um `content`-Variante erweitern | 3 | Abgedeckt |
| Enum/Parameter-Typ für `content` (`GroupContentDeleteMode` o. ä.) | 3 | Abgedeckt (Umsetzungsdetail) |
| `WorkspacesLayout`: `IEndpointRepository`- und `ISignalRNotificationService`-Injections entfernen, alle Endpunkt-/Endpunktgruppen-Operationen über `IApplicationApiClient`, `StorageMode`-Notify-Prüfungen entfallen | 4 | Abgedeckt |
| `WorkspacesLayout`: Löschdialog-Varianten über einen `content`-Delete-Endpunkt; Mehrschritt-Abläufe entfallen | 4 (auf Basis von 3) | Abgedeckt |
| `IActivityLogService`-Logging bleibt in der Komponente | 4 | Abgedeckt |
| `EndpointGroupsController` weiterhin Ziel des ApiClient-Zugriffs (kein Verhaltensumbau) | 4 | Abgedeckt |
| `IEndpointRepository.UpdateAggregateAsync` (Skalare + Kinder in einer Operation) | 5 | Abgedeckt (gemeinsame serverseitige Operation beschrieben; Methodenname ist Umsetzungsdetail) |
| `EndpointsController.UpdateAsync` auf Aggregat-Pfad umstellen (Komposition entfällt) | 5 | Abgedeckt |
| `ODataEndpointsController.Put`/`Patch` pflegen Kinder über denselben Aggregat-Pfad (entschiedene Variante, kein stilles Ignorieren) | 5 | Abgedeckt |
| REST-Vertrag `UpdateEndpointRequest` unverändert (Vollmengen-Ersetzung, keine Flags); Optimistic Concurrency über `RowVersion` unverändert | 5 | Abgedeckt |
| `EndpointPage`: redundante Sofort-Kinder-Pflege prüfen/entfernen | 5 | Abgedeckt |
| Zentrale Notify-Bedingung (`StorageMode == Team`), an einer Stelle definiert und in allen mutierenden Pfaden angewendet | 6 | Abgedeckt |
| OData-Notify für alle vier Entity-Sets (Baum-Notify für Anwendungen/Anwendungsgruppen, entitätsspezifisch für Endpunkte/Endpunktgruppen) | 6 | Abgedeckt |
| `EndpointPage.OnSaveSuccessAsync`: Doppel-Notify entfernen | 6 | Abgedeckt |
| Notify-Transport (SignalR/`EndpointHub`) unverändert | 6 | Abgedeckt |
| Kein neuer `IQueryable`-Repository-Pfad; keine Contract-Änderungen außer erforderlichen Erweiterungen; Einzel-Endpunkte für Header/QueryParameter bleiben | Vorgehensentscheidungen (REST-Vertrag stabil, keine Flags); implizit durch nicht geplante Umbauten | Abgedeckt |
| Keine neue Konfiguration | Vorgehensentscheidungen (fest verdrahtetes Verhalten) | Abgedeckt |
| Tests: `SystemEndpointSyncServiceTests`, `SwaggerImportServiceTests` | 1 | Abgedeckt |
| Tests: `ApplicationApiClientTests` (gemockter HttpMessageHandler) für `ApplySwaggerDiffAsync` und Gruppen-Delete mit `content` | 2, 3 | Abgedeckt |
| Tests: `ApplicationImportControllerIntegrationTests` (`swagger-import/apply`, `401`/`404`/`422`) | 2 | Abgedeckt |
| Tests: `ApplicationGroupsControllerIntegrationTests` (`content=delete|detach` inkl. Atomarität/Rollback) | 3 | Abgedeckt |
| Tests: `ODataControllerIntegrationTests` (Aggregat- und Notify-Verhalten) | 5, 6 | Abgedeckt |
| Tests: `EndpointRepositoryIntegrationTests` (`UpdateAggregateAsync`, Versions-Konflikt) | 5 | Abgedeckt |
| Playwright-Suite bleibt ohne neue Tests grün | 1–6 (explizit in 4 und 6) | Abgedeckt |
| Doku: `content`-Parameter in `docs/help/api/api.md` | 3 | Abgedeckt |
| Doku: OData-Aggregat-Semantik in `docs/help/api/odata-api.md` | 5 | Abgedeckt |
| Doku: neuer Endpunkt `swagger-import/apply` in `docs/help/api/api.md` | — | **Lücke** |
| `CLAUDE.md`: `IEndpointRepository`-Ausnahme bleibt korrekt bzw. wandert mit der Logik in den Service (Prüfung/Anpassung) | — | **Lücke** |
| Ein gemeinsamer PR gegen `staging`, Commit-Messages mit Closing-Keywords (`Closes #N`) | Vorgehensentscheidung „Auslieferung" | Abgedeckt |

## Abhängigkeitsprüfung

| Prüfpunkt | Befund |
|-----------|--------|
| Zyklen in Abhängigkeiten | Keine gefunden (2→1, 4→3, 5→1+2, 6→5; alle Verweise auf frühere Schritte auflösbar) |
| Reihenfolge konsistent zu inhaltlichen Abhängigkeiten | Ja — UI-Umstellungen folgen den serverseitigen Endpunkten (Schritt 4 nach 3, Schritt 2-Dialog nach Schritt-1-Service); die zentralisierte Notify-Politik (Schritt 6) steht hinter allen mutierenden Pfaden (Schritte 1–5), inhaltliche Abhängigkeit zu Schritt 3/4 ist durch die Position am Ende ebenfalls gewahrt |

## Fehlende oder unvollständige Punkte

- [ ] API-Dokumentation des neuen Endpunkts `POST /api/applications/{id}/swagger-import/apply` in `docs/help/api/api.md` ist keinem Entwicklungsschritt zugeordnet (Anforderung: „neue Endpunkte (`swagger-import/apply`, `content`-Parameter) … dokumentieren"). Schritt 3 deckt nur den `content`-Parameter ab, Schritt 5 nur die Aggregat-Semantik — sinnvoll in Schritt 2 nachzutragen.
- [ ] `CLAUDE.md`-Aktualität zur `IEndpointRepository`-Ausnahme ist keinem Schritt zugeordnet (Anforderung Dokumentation: Ausnahme gilt weiterhin nur für den Startabgleich bzw. wandert mit der Logik in den Service). Nach Schritt 1 nutzt `SystemEndpointSyncService` das Repository nicht mehr direkt — die dokumentierte Ausnahme muss geprüft und ggf. auf den neuen Ausführungsort umgeschrieben werden.

## Hinweise

- Die konkreten Methodennamen (`SyncFromDocumentAsync`, `UpdateAggregateAsync`, `DeleteGroupWithContentAsync`, `ApplySwaggerDiffAsync`) und der neue Enum `GroupContentDeleteMode` sind fachlich durch die Schrittbeschreibungen abgedeckt; die Benennung gilt als Umsetzungsdetail und muss nicht im Projektplan stehen.
- Lokalisierung: Die Anforderung verlangt neue UI-Texte (z. B. Fehlermeldungen aus Schritt 2) über das `SharedResources`-Schlüsselschema `{KomponentenName}_{Rolle}` mit `Comment`-Eintrag — bedingt („falls"), kein Schritt erwähnt es explizit; bei Bedarf in Schritt 2 mitführen.
- Die InterfaceType-Validierung ist im Plan am Endpunkt (`422`) verankert, die Anforderung nennt sie zusätzlich für `ApplyDiffAsync`; fachlich ist die Wirkung abgedeckt, der Ort ist innerhalb von Schritt 2 zu entscheiden.
