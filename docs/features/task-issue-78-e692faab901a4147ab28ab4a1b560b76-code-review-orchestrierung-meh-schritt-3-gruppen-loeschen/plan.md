# Umsetzungsplan: Atomares Löschen von Anwendungsgruppen mit Inhalt (`content`-Parameter)

## Übersicht

`DELETE /api/application-groups/{id}` wird um den optionalen Query-Parameter `content` (`delete` | `detach`) erweitert. Beide Varianten laufen serverseitig als eine atomare DB-Transaktion im `ApplicationRepository`. Der `IApplicationApiClient` erhält eine parametrisierte Lösch-Variante. Die REST-API-Doku (`docs/help/api/api.md`) wird um den bisher fehlenden DELETE-Abschnitt inklusive Parameter ergänzt. Die UI-Umstellung erfolgt separat in Schritt 4.

## Scope-Klärung

| Feld | Festlegung |
|------|------------|
| Verarbeitungseinheit | Eine Löschoperation auf eine `ApplicationGroup` inkl. Behandlung aller enthaltenen `Application`-Objekte (transitiv `Endpoint`, `EndpointGroup`, `EndpointHeader`, `EndpointQueryParameter`, `ApplicationLink`) als eine atomare Einheit |
| Auslösende Ereignisse | `DELETE /api/application-groups/{id}?content={wert}` via HTTP — durch `IApplicationApiClient` oder externe Clients |
| Erwartete Wirkung | `delete`: Gruppe + Anwendungen + deren Endpunkte/Endpunktgruppen vollständig entfernt; `detach`/fehlender Parameter: Anwendungen explizit ungruppiert (`ApplicationGroupId = null`), nur Gruppe gelöscht; Erfolg → 204 + `NotifyTreeChangedAsync` (Team); Fehlschlag → Bestand unverändert (Rollback) |
| Umgang mit fehlenden Objekten | Gruppe fehlt → 404 (verwerfen); Gruppe ohne Anwendungen → nur Gruppe löschen; `content` fehlt → Legacy-Verhalten = `detach` |

### Scope-Ausschlüsse

| # | Einschränkung / Ausschluss | Begründung | Status |
|---|----------------------------|------------|--------|
| 1 | UI-Umstellung des Löschdialogs (`WorkspacesLayout`) erfolgt nicht | Anforderung weist das explizit Entwicklungsschritt 4 zu; der Endpunkt muss zuerst existieren | bestätigt (2026-11-17, per Anforderungstext) |
| 2 | `ODataApplicationGroupsController` erhält keinen `content`-Parameter | Nicht Teil der Anforderung; OData behält bisheriges Detach-Verhalten | bestätigt (2026-11-17, per Anforderungstext) |

## Designentscheidungen

| Komponente / Bereich | Gewählter Ansatz | Begründung |
|----------------------|-----------------|------------|
| `IApplicationRepository.DeleteGroupAsync` | Bestehende Signatur `DeleteGroupAsync(int id)` beibehalten + Overload `DeleteGroupAsync(int id, ApplicationGroupDeleteMode mode)`; `DeleteGroupAsync(id)` delegiert an `(id, Detach)` | Rückwärtskompatibel zu `ODataApplicationGroupsController` und Fake-Implementierung in `SystemEntryInitializerTests`; ein einziger transaktionaler Implementierungspfad |
| Transaktion | Explizites `context.Database.BeginTransactionAsync()` um Laden, Mutieren und `SaveChangesAsync` | Anforderung verlangt „eine Transaktion" und garantierten Rollback bei Fehlschlag; explizit statt nur impliziter SaveChanges-Transaktion |
| `delete`-Kaskade | Alle `Endpoint`- und `EndpointGroup`-Entitäten der betroffenen Anwendungen laden und per `RemoveRange` entfernen, dann `Applications`, dann die Gruppe | `EndpointGroup.ChildGroups` ist `DeleteBehavior.Restrict` — DB-Kaskade über `Application` könnte an verschachtelten Gruppen scheitern; getrackte Entitäten werden von EF topologisch geordnet (Kinder vor Eltern) gelöscht. Headers/QueryParameters/Links/CallHistory regelt die DB (`Cascade`/`SetNull`) |
| `content`-Binding | `[FromQuery] string? content`, case-insensitive Auswertung (`delete`/`detach`), ungültig → `400` vor DB-Zugriff | String-Binding statt Enum-Binding, damit unbekannte Werte kontrolliert als 400 statt ModelBinding-Fehler behandelt werden |
| API-Client | Bestehendes `DeleteGroupAsync(int id)` unverändert (kein Query-Param = Server-Default) + `DeleteGroupAsync(int id, ApplicationGroupDeleteMode mode)` mit `?content=` aus `mode.ToString().ToLowerInvariant()` | Parametrisierte Erweiterung ohne Bruch bestehender Aufrufer; Enum-Namen decken sich 1:1 mit den erlaubten Werten |

## Programmabläufe

### Löschen mit `content=delete` (atomare Vollkaskade)

1. `ApplicationGroupsController.DeleteAsync(id, content)` — Tokenvalidierung (`401`), `content` parsen (`400` bei ungültig), `GetGroupByIdAsync` (`404`), `IsSystem`-Prüfung (`403`).
2. `_applicationRepository.DeleteGroupAsync(id, ApplicationGroupDeleteMode.Delete)`.
3. Repository: `BeginTransactionAsync` → Gruppe inkl. `Applications` laden (fehlend → Rückkehr ohne Änderung) → alle `EndpointGroups` und `Endpoints` der Member-Applications laden → `RemoveRange(Endpoints)` → `RemoveRange(EndpointGroups)` (EF ordnet Kinder vor Eltern) → `RemoveRange(Applications)` → `Remove(Group)` → `SaveChangesAsync` → `CommitAsync`.
4. Bei Exception: Transaktion verworfen (`await using`), Bestand unverändert; der Fehler propagiert zum Controller → 500.
5. Erfolg im Team-Modus: `NotifyTreeChangedAsync` → `204 NoContent`.

### Löschen mit `content=detach` bzw. ohne Parameter (atomares Aushängen)

1. Wie oben Schritt 1–2 mit `ApplicationGroupDeleteMode.Detach`.
2. Repository: `BeginTransactionAsync` → Gruppe inkl. `Applications` laden → für jede Anwendung `ApplicationGroupId = null` setzen → `Remove(Group)` → `SaveChangesAsync` → `CommitAsync`.
3. Fehlschlag → Rollback; Erfolg im Team-Modus → `NotifyTreeChangedAsync` → `204`.

### API-Client-Aufruf

`ApplicationApiClient.DeleteGroupAsync(id, mode)` baut `{base}/api/application-groups/{id}?content={delete|detach}` via `BuildDeleteRequest` und sendet über `SendWithTokenNoContentAsync` (inkl. Token-Rotation/Retry). `DeleteGroupAsync(id)` bleibt unverändert ohne Query-Parameter (Server-Default = detach).

## Neue Klassen

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `ApplicationGroupDeleteMode` (`src/Schnittstellenzentrale.Core/Enums/ApplicationGroupDeleteMode.cs`) | Enum | Werte `Detach`, `Delete`; steuert das Schicksal enthaltener Anwendungen beim Gruppen-Delete |

## Änderungen an bestehenden Klassen

### `IApplicationRepository` (Interface)

- **Neue Methoden:** `DeleteGroupAsync(int id, ApplicationGroupDeleteMode mode)` — parametrisierte, transaktionale Löschvariante. `DeleteGroupAsync(int id)` bleibt bestehen (delegiert auf `Detach`).

### `ApplicationRepository` (EF-Repository)

- **Neue Methoden:** `DeleteGroupAsync(int id, ApplicationGroupDeleteMode mode)` — transaktionale Implementierung wie in Programmabläufen beschrieben.
- **Geänderte Methoden:** `DeleteGroupAsync(int id)` — delegiert an `DeleteGroupAsync(id, ApplicationGroupDeleteMode.Detach)`.

### `ApplicationGroupsController` (Controller)

- **Geänderte Methoden:** `DeleteAsync(int id)` → `DeleteAsync(int id, [FromQuery] string? content = null)` — Validierung: `null`/leer/`"detach"` → `Detach`, `"delete"` → `Delete` (jeweils `OrdinalIgnoreCase`), sonst `BadRequest`. Aufruf `_applicationRepository.DeleteGroupAsync(id, mode)`. XML-Doc + `ProducesResponseType(400)` ergänzen.

### `IApplicationApiClient` (Interface)

- **Neue Methoden:** `DeleteGroupAsync(int id, ApplicationGroupDeleteMode mode)` — DELETE mit `?content=`-Parameter.

### `ApplicationApiClient` (HTTP-Client)

- **Neue Methoden:** `DeleteGroupAsync(int id, ApplicationGroupDeleteMode mode)` — URL mit `?content=` + vorhandene `BuildDeleteRequest`/`SendWithTokenNoContentAsync`-Infrastruktur.

## Datenbankmigrationen

Keine — nur Verhalten in einer Transaktion; Schema und `DeleteBehavior`-Regeln bleiben unverändert.

## Validierungsregeln

| Feld / Objekt | Regel | Fehlerfall |
|---------------|-------|------------|
| `content` (Query) | `null`, leer, `detach`, `delete` (case-insensitiv) | anderer Wert → `400 Bad Request` |
| `id` (Route) | Gruppe muss existieren | `404 Not Found` |
| Gruppe | `IsSystem == false` | `403 Forbidden` |

## Konfigurationsänderungen

Keine.

## Seiteneffekte und Risiken

- **`ODataApplicationGroupsController.DeleteAsync`:** Nutzt weiterhin `DeleteGroupAsync(id)` = `Detach` — bestehendes Verhalten unverändert.
- **`EndpointGroup.ChildGroups` (`Restrict`):** Löschen verschachtelter Endpunktgruppen bei `content=delete` muss kind-zuerst erfolgen — durch Laden aller betroffenen `EndpointGroup`-Entitäten gelöst (EF ordnet die DELETEs).
- **Systemanwendungen (`Application.IsSystem`):** Werden beim `delete`-Modus ebenfalls mitgelöscht — die Anforderung verlangt kaskadierend „alle zugeordneten Anwendungen"; nur die System*gruppe* ist per `403` geschützt. Konsistent zum bisherigen UI-Pfad, der jede Anwendung einzeln löschte (Controller-Einzeldelete blockt `IsSystem`, der UI-Pfad rief es trotzdem auf — dort wäre ein Teilerfolg entstanden; der neue atomare Pfad löscht mit der Gruppe komplett). 
- **`WorkspacesLayout`:** Unverändert; ruft weiterhin `DeleteGroupAsync(id)` → `detach`-Semantik (identisch zum bisherigen „Nur Gruppe löschen"-Endzustand; „Alles löschen" löscht weiterhin mehrstufig — wird in Schritt 4 umgestellt).

## Umsetzungsreihenfolge

1. **Enum `ApplicationGroupDeleteMode` anlegen**
   - Voraussetzungen: Keine
   - Beschreibung: Neue Datei `src/Schnittstellenzentrale.Core/Enums/ApplicationGroupDeleteMode.cs` mit `Detach` und `Delete` (XML-Doc im Projektstil).

2. **`IApplicationRepository` + `ApplicationRepository` erweitern**
   - Voraussetzungen: Schritt 1
   - Beschreibung: Overload `DeleteGroupAsync(int id, ApplicationGroupDeleteMode mode)` im Interface deklarieren und transaktional implementieren; `DeleteGroupAsync(int id)` delegiert auf `Detach`.

3. **`ApplicationGroupsController.DeleteAsync` um `content` erweitern**
   - Voraussetzungen: Schritt 2
   - Beschreibung: `[FromQuery] string? content`, Validierung (400), Modus-Mapping, `ProducesResponseType(400)` + XML-Doc.

4. **`IApplicationApiClient` + `ApplicationApiClient` erweitern**
   - Voraussetzungen: Schritt 1
   - Beschreibung: `DeleteGroupAsync(int id, ApplicationGroupDeleteMode mode)` im Interface + Implementierung mit `?content=`-Query.

5. **Tests implementieren**
   - Voraussetzungen: Schritte 1–4
   - Beschreibung: Alle neuen Tests gemäß Abschnitt „Tests".

6. **`docs/help/api/api.md` ergänzen**
   - Voraussetzungen: Schritt 3
   - Beschreibung: Neuen Abschnitt `DELETE /api/application-groups/{id}` mit `content`-Parameter dokumentieren.

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `DeleteGroupAsync_WithDeleteMode_SendsContentQueryParam` | `ApplicationApiClientTests` | DELETE-Request an `/api/application-groups/{id}` mit `content=delete` im Query-String |
| `DeleteGroupAsync_WithDetachMode_SendsContentQueryParam` | `ApplicationApiClientTests` | `content=detach` im Query-String |
| `DeleteGroupAsync_WithoutMode_SendsNoQueryParam` | `ApplicationApiClientTests` | bestehender Test deckt: kein `?content` (unverändert) |
| `DeleteGroup_WithDeleteMode_DeletesApplicationsEndpointsAndGroups` | `ApplicationRepositoryIntegrationTests` | Gruppe + Anwendung + Endpunkt + verschachtelte Endpunktgruppen → alles entfernt |
| `DeleteGroup_WithDetachMode_KeepsApplicationsUngrouped` | `ApplicationRepositoryIntegrationTests` | Anwendungen/Endpunkte bleiben, `ApplicationGroupId == null` |
| `DeleteGroup_WithDeleteMode_OnSaveFailure_RollsBackEverything` | `ApplicationRepositoryIntegrationTests` | `SaveChangesAsync` wirft → Gruppe/Anwendungen/Endpunkte unverändert vorhanden |
| `DeleteGroup_WithDetachMode_OnSaveFailure_RollsBackEverything` | `ApplicationRepositoryIntegrationTests` | analog für `detach` (Anwendung bleibt gruppiert) |
| `DeleteApplicationGroup_WithContentDelete_RemovesGroupAndApplications` | `ApplicationGroupsControllerIntegrationTests` | `?content=delete` → 204; Bestand via `IApplicationRepository` leer |
| `DeleteApplicationGroup_WithContentDetach_KeepsApplicationsUngrouped` | `ApplicationGroupsControllerIntegrationTests` | `?content=detach` → 204; Anwendung vorhanden mit `ApplicationGroupId == null` |
| `DeleteApplicationGroup_WithoutContent_BehavesLikeDetach` | `ApplicationGroupsControllerIntegrationTests` | kein Parameter → 204; Anwendung ungruppiert erhalten |
| `DeleteApplicationGroup_WithInvalidContent_Returns400` | `ApplicationGroupsControllerIntegrationTests` | `?content=bogus` → 400, Bestand unverändert |
| `DeleteApplicationGroup_WithDeleteModeAndSystemGroup_Returns403` | `ApplicationGroupsControllerIntegrationTests` | `?content=delete` auf Systemgruppe → 403 |
| `FailingSaveDbContext` / `FailingSaveDbContextFactory` | `ApplicationRepositoryIntegrationTests` (privat) | `AppDbContext`-Ableitung, deren `SaveChangesAsync` wirft; Factory für Repository-Rollback-Tests |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| `SystemEntryInitializerTests`-Fake (`IApplicationRepository`) | Nur falls Interface-Signatur geändert würde — bei Overload-Ansatz **nicht** betroffen |
| Keine weiteren | Overload- und Optional-Design lässt alle bestehenden Aufrufe unverändert |

### E2E-Tests (primärer Funktionsnachweis)

**Keine E2E-Tests erforderlich.** Die Anforderung berührt in diesem Schritt keine Benutzerinteraktion: Der UI-Löschdialog wird ausdrücklich erst in Schritt 4 umgestellt; die aktuelle UI ruft weiterhin `DeleteGroupAsync(id)` (Server-Default `detach` = bisheriges Endverhalten). Es entsteht kein neuer oder geänderter Benutzerfluss — der neue Parameter ist nur über die HTTP-API erreichbar und wird durch WebApplicationFactory-Integrationstests (echter HTTP-Stack inkl. Routing, Token-Rotation, EF/SQLite) sowie die reale Abnahme über HTTP-Aufruf abgedeckt.

## Akzeptanzszenarien

| # | Szenario | Auslöser | Ausgangszustand | Schritte | Sichtbare Wirkung | Nachweis |
|---|----------|----------|------------------|----------|-------------------|----------|
| 1 | Happy Path `delete` | `DELETE /api/application-groups/{id}?content=delete` | Gruppe mit 1 Anwendung, die Endpunkt + verschachtelte Endpunktgruppe besitzt | Aufruf → atomare Transaktion | 204; Gruppe, Anwendung, Endpunktgruppen, Endpunkt vollständig entfernt; `X-New-Token` gesetzt; `NotifyTreeChangedAsync` im Team-Modus | Integrationstest + reale Abnahme (HTTP-Aufruf) |
| 2 | Happy Path `detach` | `DELETE /api/application-groups/{id}?content=detach` | Gruppe mit 1 Anwendung + Endpunkt | Aufruf → atomare Transaktion | 204; Gruppe entfernt; Anwendung mit `ApplicationGroupId=null` samt Endpunkt erhalten | Integrationstest + reale Abnahme |
| 3 | Abwärtskompatibilität | `DELETE /api/application-groups/{id}` ohne `content` | Gruppe mit Anwendung | Aufruf | 204; Anwendung ungruppiert erhalten (bisheriges Verhalten) | Integrationstest |
| 4 | Fehlerfall ungültiger Parameter | `DELETE ...?content=bogus` | Gruppe mit Anwendung | Aufruf | 400; nichts verändert | Integrationstest |
| 5 | Fehlerfall fehlende Gruppe | `DELETE /api/application-groups/999999?content=delete` | keine Gruppe | Aufruf | 404 | Integrationstest (bestehend + Varianten) |
| 6 | Fehlerfall Systemgruppe | `DELETE ...?content=delete` auf `IsSystem`-Gruppe | Systemgruppe | Aufruf | 403; Bestand unverändert | Integrationstest |
| 7 | Rollback bei Fehlschlag | Repository-Aufruf mit `delete`/`detach` bei fehlschlagendem `SaveChangesAsync` | gefüllter Bestand | Aufruf wirft | Gesamtbestand (Gruppe, Anwendungen, Endpunkte) unverändert | Repository-Test mit `FailingSaveDbContext` |

## Offene Punkte

Keine.
