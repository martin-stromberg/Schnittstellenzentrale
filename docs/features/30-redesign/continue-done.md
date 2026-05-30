# Offene Aufgaben

Erstellt am: 2026-05-30
Abbruchgrund: Kein Fortschritt zwischen den letzten zwei Iterationen

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

(keine — Plan vollständig umgesetzt)

## Code-Review-Befunde

- [x] **`ApplicationApiClient.cs:310` — `IsSystem` nicht gemappt**: Bereits korrekt gemappt — `IsSystem = response.IsSystem` ist in beiden Mapping-Methoden vorhanden.
- [x] **`EndpointExecutionService.cs:130` — Aktivitätslog vor Post-Script-Ausführung**: Post-Script wird jetzt vor dem Activity-Log ausgeführt.
- [x] **`LinksManager.razor:149` — In-Memory-Mutation vor Concurrency-Check**: `SaveEditAsync` erstellt jetzt eine separate `updated`-Instanz und mutiert `existing` nicht mehr vor dem DB-Aufruf.
- [x] **`EndpointScriptRunner.cs:209` — Sync-over-Async blockiert Thread-Pool**: Architektonisch nicht behebbar ohne Jint-API-Änderung; im Code dokumentiert. Persist wird jetzt nur noch aufgerufen wenn `activeEnv.Id > 0`.
- [x] **`ContentHeader.razor:177` — Abhängigkeit auf `endpoint-page.js`**: `clickElement()` in `content-header.js` ausgelagert; `ContentHeader` importiert jetzt `content-header.js`.

## Testergebnisse vom Anwender

- [x] `sz.environment.set` fehlgeschlagen bei `Id=0`: `ApplyEnvironmentSet` persistiert nur noch wenn `activeEnv != null && activeEnv.Id > 0`. Bei null bleibt die Variable rein in-memory.
- [x] Navigationsleiste: "Neue Sammlung" und "Neue Anwendung" aus `ApplicationGroupTree` entfernt. "+ Neue Anwendung"-Button in `CollectionContentView` im `sz-hero-content`-Div rechtsbündig ergänzt.
- [x] Formular-Design: `ApplicationGroupEditor` und `ApplicationEditor` auf `sz-*`-Designsystem umgestellt (Hero-Header, `sz-form-group`, `sz-input`, `sz-form-select`, `sz-editor-actions`).
