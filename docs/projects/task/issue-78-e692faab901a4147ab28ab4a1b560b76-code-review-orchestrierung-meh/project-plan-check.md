# Projektplan-Gegenprüfung

## Ergebnis

**Status:** Projektplan vollständig

## Abgleich Anforderung ↔ Entwicklungsschritte

| Anforderungspunkt | Abgedeckt durch Schritt(e) | Status |
|--------------------|-----------------------------|--------|
| `ISwaggerImportService.SyncFromDocumentAsync` (Map → Diff → Apply inkl. `WithBearerTokens`, Duplikatschlüssel-Erkennung) | 1 | Abgedeckt |
| `SystemEndpointSyncService` vereinfachen (Systemgruppe/-anwendung auflösen, `ISwaggerProvider.GetSwagger("v1")`, Service-Aufruf; eigene Diff-Bildung und `IEndpointRepository`-Bezug entfallen) | 1 | Abgedeckt |
| Startabgleich verarbeitet `ChangedEndpoints` (Name, Body, `AuthenticationType`, Pre-/Post-Skripte) | 1 | Abgedeckt |
| `ImportDiff` mit `ErrorMessage` beim Startabgleich: kein Apply, Fehler loggen, Start läuft weiter | 1 | Abgedeckt |
| `CLAUDE.md`: `IEndpointRepository`-Ausnahme prüfen/pflegen — wandert mit der Logik zum neuen Ausführungsort bzw. wird gestrichen | 1 | Abgedeckt (Lücke aus Runde 1 geschlossen) |
| Neuer Endpunkt `POST /api/applications/{id}/swagger-import/apply` (Token `401`, `404` bei fehlender Anwendung, `422` bei `InterfaceType != Rest`, `204` bei Erfolg) | 2 | Abgedeckt |
| `ApplyDiffAsync`-Validierung: InterfaceType-/Aggregat-Zugehörigkeit des Diffs | 2 | Abgedeckt (InterfaceType am Endpunkt via `422`, Aggregat-Validierung in der Apply-Logik) |
| `IApplicationApiClient`/`ApplicationApiClient.ApplySwaggerDiffAsync` inkl. Fehlerdurchreichung | 2 | Abgedeckt |
| `SwaggerImportDialog` ruft ApiClient statt `ISwaggerImportService` | 2 | Abgedeckt |
| API-Dokumentation des neuen Endpunkts `swagger-import/apply` in `docs/help/api/api.md` | 2 | Abgedeckt (Lücke aus Runde 1 geschlossen: „Die REST-API-Dokumentation wird um den neuen Endpunkt ergänzt") |
| Lokalisierung neuer UI-Texte über `SharedResources`-Schlüsselschema mit `Comment`-Eintrag | 2 | Abgedeckt (explizit in Schritt 2 verankert) |
| `DELETE /api/application-groups/{id}?content=delete|detach` serverseitig atomar (EF-Transaktion, `404`, `403` Systemgruppe, Rollback bei Fehlschlag) | 3 | Abgedeckt |
| `content`-Parameter-Semantik: fehlend = bisheriges Verhalten (`detach`), ungültig = `400` | 3 + Vorgehensentscheidung „API-Kompatibilität" | Abgedeckt |
| `IApplicationRepository`-Erweiterung für transaktionales Löschen mit Inhalt | 3 | Abgedeckt (atomare Transaktion beschrieben; Methodenname ist Umsetzungsdetail) |
| `IApplicationApiClient.DeleteGroupAsync` um `content`-Variante erweitern | 3 | Abgedeckt |
| Enum/Parameter-Typ für `content` (`GroupContentDeleteMode` o. ä.) | 3 | Abgedeckt (Umsetzungsdetail) |
| Doku: `content`-Parameter in `docs/help/api/api.md` | 3 | Abgedeckt |
| `WorkspacesLayout`: `IEndpointRepository`- und `ISignalRNotificationService`-Injections entfernen, alle Endpunkt-/Endpunktgruppen-Operationen über `IApplicationApiClient`, `StorageMode`-Notify-Prüfungen entfallen | 4 | Abgedeckt |
| `WorkspacesLayout`: Löschdialog-Varianten über einen `content`-Delete-Endpunkt; Mehrschritt-Abläufe entfallen | 4 (auf Basis von 3) | Abgedeckt |
| `IActivityLogService`-Logging bleibt in der Komponente | 4 + Vorgehensentscheidung „Aktivitätsprotokoll" | Abgedeckt |
| `EndpointGroupsController` weiterhin Ziel des ApiClient-Zugriffs (kein Verhaltensumbau) | 4 | Abgedeckt |
| Reine Lesezugriffe anderer Komponenten (`CollectionContentView` etc.) bleiben unberührt | 4 | Abgedeckt |
| `IEndpointRepository.UpdateAggregateAsync` (Skalare + Kinder in einer Operation) | 5 | Abgedeckt (Methodenname ist Umsetzungsdetail) |
| `EndpointsController.UpdateAsync` auf Aggregat-Pfad umstellen (Komposition entfällt) | 5 | Abgedeckt |
| `ODataEndpointsController.Put`/`Patch` pflegen Kinder über denselben Aggregat-Pfad (entschiedene Variante, kein stilles Ignorieren) | 5 + Vorgehensentscheidung „Endpunkt-Aggregat" | Abgedeckt |
| REST-Vertrag `UpdateEndpointRequest` unverändert (Vollmengen-Ersetzung, keine Flags); Optimistic Concurrency über `RowVersion` unverändert | 5 | Abgedeckt |
| `EndpointPage`: redundante Sofort-Kinder-Pflege prüfen/entfernen | 5 | Abgedeckt |
| Doku: OData-Aggregat-Semantik in `docs/help/api/odata-api.md` | 5 | Abgedeckt |
| Zentrale Notify-Bedingung (`StorageMode == Team`), an einer Stelle definiert und in allen mutierenden Pfaden angewendet | 6 | Abgedeckt |
| OData-Notify für alle vier Entity-Sets (Baum-Notify für Anwendungen/Anwendungsgruppen, entitätsspezifisch für Endpunkte/Endpunktgruppen) | 6 + Vorgehensentscheidung „Benachrichtigungspolitik" | Abgedeckt |
| `EndpointPage.OnSaveSuccessAsync`: Doppel-Notify entfernen | 6 | Abgedeckt |
| Notify-Transport (SignalR/`EndpointHub`) unverändert | 6 | Abgedeckt |
| Scope-Ausschlüsse: kein `IQueryable`-Pfad, keine Contract-Änderungen außer erforderlichen Erweiterungen, Einzel-Endpunkte für Header/QueryParameter bleiben, OData-`PUT`/`PATCH` auf Nicht-Endpoint-Sets unverändert | Vorgehensentscheidungen; keine widersprechende Planung | Abgedeckt |
| Keine neue Konfiguration | Vorgehensentscheidungen (fest verdrahtetes Verhalten: `StorageMode.Team`, `delete`/`detach`) | Abgedeckt |
| Tests: `SystemEndpointSyncServiceTests`, `SwaggerImportServiceTests` | 1 | Abgedeckt |
| Tests: `ApplicationApiClientTests` (gemockter HttpMessageHandler) für `ApplySwaggerDiffAsync` und Gruppen-Delete mit `content` | 2, 3 | Abgedeckt |
| Tests: `ApplicationImportControllerIntegrationTests` (`swagger-import/apply`, `401`/`404`/`422`) | 2 | Abgedeckt |
| Tests: `ApplicationGroupsControllerIntegrationTests` (`content=delete|detach` inkl. Atomarität/Rollback) | 3 | Abgedeckt |
| Tests: `ODataControllerIntegrationTests` (Aggregat- und Notify-Verhalten) | 5, 6 | Abgedeckt |
| Tests: `EndpointRepositoryIntegrationTests` (`UpdateAggregateAsync`, Versions-Konflikt) | 5 | Abgedeckt |
| Playwright-Suite bleibt ohne neue Tests grün (`GroupCrudTests`, `EndpointGroupCrudTests`, `SwaggerImportTests`, `SignalRSyncTests`, `LayoutSmokeTests` mit `BoundingBoxAsync()`) | 1–6 (explizit in 4 und 6) | Abgedeckt |
| Ein gemeinsamer PR gegen `staging`, Commit-Messages mit Closing-Keywords (`Closes #N`) | Vorgehensentscheidung „Auslieferung" | Abgedeckt |
| Umsetzungsreihenfolge #87 → #88 → #90 → #89 → #92 → #91 (Offene Frage 7) | Schritte 1–6 + Vorgehensentscheidung „Reihenfolge" | Abgedeckt |
| Alle übrigen offenen Fragen aus requirement.md (Variantenwahl #92, Notify-Umfang #91, `content`-Semantik, `ApplyDiffAsync`-Validierung, `UpdateEndpointRequest`-Vertrag, ActivityLog) | Vorgehensentscheidungen | Abgedeckt (alle entschieden und in die Schritte eingearbeitet) |

## Abhängigkeitsprüfung

| Prüfpunkt | Befund |
|-----------|--------|
| Zyklen in Abhängigkeiten | Keine gefunden (2→1, 4→3, 5→{1,2}, 6→5; azyklischer Graph, alle Verweise auf frühere Schritte auflösbar) |
| Reihenfolge konsistent zu inhaltlichen Abhängigkeiten | Ja — Schritt 2 baut auf dem Service-Pfad aus Schritt 1 auf; Schritt 4 setzt den `content`-Endpunkt aus Schritt 3 voraus; Schritt 5 nutzt die aus Schritten 1/2 finalen Import-Pfade; Schritt 6 (zentrale Notify-Politik) steht bewusst hinter allen mutierenden Pfaden (Schritte 1–5), sodass inhaltliche Abhängigkeiten zu Schritten 3/4 durch die Endposition ebenfalls gewahrt sind |

## Hinweise

- Die beiden Lücken aus Prüfrunde 1 (fehlende API-Doku für `swagger-import/apply`, fehlende `CLAUDE.md`-Pflege zur `IEndpointRepository`-Ausnahme) wurden in den Plan eingearbeitet und sind jetzt Schritt 2 bzw. Schritt 1 zugeordnet.
- Die konkreten Methodennamen (`SyncFromDocumentAsync`, `UpdateAggregateAsync`, `DeleteGroupWithContentAsync`, `ApplySwaggerDiffAsync`) und der neue Enum `GroupContentDeleteMode` sind fachlich durch die Schrittbeschreibungen abgedeckt; die Benennung gilt als Umsetzungsdetail und muss nicht im Projektplan stehen.
- Schritt 4 benennt die UI-Umstellung des Löschdialogs explizit als eigenen Teil; die zeitliche Trennung zwischen Endpunkt-Bereitstellung (Schritt 3) und UI-Umstellung (Schritt 4) ist über die Abhängigkeit 4→3 sauber geregelt.
