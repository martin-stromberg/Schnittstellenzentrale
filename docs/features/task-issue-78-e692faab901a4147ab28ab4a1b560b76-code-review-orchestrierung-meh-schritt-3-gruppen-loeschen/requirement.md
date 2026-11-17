# Übersetzte Anforderung: Atomares Löschen von Anwendungsgruppen mit Inhalt

Issue-Bezug: #90 (Teilschritt 3 von Issue #78)

## Fachliche Zusammenfassung

Das Löschen einer `ApplicationGroup` samt enthaltener `Application`-Objekte soll serverseitig als atomare Einheit ausgeführt werden — statt als mehrstufige, bei Teilerfolg inkonsistente Abfolge von Einzelaufrufen aus der Oberfläche. Der bestehende Endpunkt `DELETE /api/application-groups/{id}` wird um einen optionalen Query-Parameter `content` erweitert (`delete` | `detach`), der das Schicksal der enthaltenen Anwendungen steuert. Der `IApplicationApiClient` wird um die parametrisierte Lösch-Variante erweitert; die UI-Umstellung des Löschdialogs erfolgt erst in Schritt 4 und ist nicht Teil dieser Anforderung.

## Fachlicher Scope

- **Verarbeitungseinheit:** Eine Löschoperation auf genau eine `ApplicationGroup` einschließlich der Behandlung aller zugeordneten `Application`-Objekte (und transitiv deren `Endpoint`- und `EndpointGroup`-Objekte), ausgeführt als eine atomare, fehlersichere Einheit.
- **Auslösende Ereignisse:** HTTP-Aufruf `DELETE /api/application-groups/{id}?content={wert}` — ausgelöst durch den `IApplicationApiClient` (perspektivisch durch beide Bestätigungsvarianten des Löschdialogs in `WorkspacesLayout`: „Alles löschen" → `delete`, „Nur Gruppe löschen" → `detach`) oder durch beliebige externe API-Clients.
- **Erwartete Wirkung:**
  - `content=delete`: Die Gruppe und alle enthaltenen Anwendungen inklusive deren Endpunkte und Endpunktgruppen sind vollständig aus dem Bestand entfernt; bei Erfolg wird die bestehende Änderungs-Benachrichtigung (`NotifyTreeChangedAsync` im Team-Modus) unverändert ausgelöst.
  - `content=detach` (oder fehlender Parameter): Die enthaltenen Anwendungen werden explizit in die ungruppierte Menge umgehängt (`ApplicationGroupId` = `null`) und bleiben erhalten; nur die Gruppe wird gelöscht.
  - Bei Fehlschlag der Operation bleibt der Bestand vollständig erhalten (Rollback, kein Teilerfolg).
  - Fehlerfälle: `404` bei nicht vorhandener Gruppe, `403` bei Systemgruppen (`IsSystem`), `400` bei ungültigem `content`-Wert, `401` bei fehlendem/ungültigem Token (unverändert).
- **Umgang mit fehlenden Objekten:**
  - Anwendungsgruppe existiert nicht → `404` (verwerfen, kein Ersatzobjekt).
  - Gruppe enthält keine Anwendungen → beide Varianten löschen nur die Gruppe (kein Sonderfall nötig).
  - `content`-Parameter fehlt → bisheriges Verhalten gilt (faktisch `detach`) — Abwärtskompatibilität.
- **Scope-Ausschlüsse:**
  - Die Umstellung der UI-Mehrschritt-Aufrufe in `WorkspacesLayout` (`OnDeleteGroupConfirmedAll`/`OnDeleteGroupConfirmedGroupOnly`) erfolgt erst in Entwicklungsschritt 4 — in diesem Schritt wird nur der Endpunkt und der API-Client bereitgestellt. (Status: offen — in der Anforderung selbst explizit als separater Schritt ausgewiesen)
  - Der OData-Endpunkt (`ODataApplicationGroupsController`) erhält keinen `content`-Parameter; er behält das bisherige Detach-Verhalten. (Status: offen — nicht Teil der Anforderung)

## Betroffene Klassen und Komponenten

- `ApplicationGroupsController` (`src/Schnittstellenzentrale/Controllers/`) — `DeleteAsync` um `content`-Parameter erweitern.
- `IApplicationRepository` / `ApplicationRepository` (`Core/Interfaces`, `Infrastructure/Repositories`) — Löschoperation um Modus und Atomarität erweitern.
- `IApplicationApiClient` / `ApplicationApiClient` (`Core/Interfaces`, `Schnittstellenzentrale/Services`) — parametrisierte Lösch-Variante.
- Neuer Enum (z. B. in `src/Schnittstellenzentrale.Core/Enums/`) für die beiden Varianten.
- `AppDbContext` — keine Änderung erwartet; vorhandene `DeleteBehavior`-Regeln (SetNull, Cascade, Restrict bei `EndpointGroup.ChildGroups`) sind zu beachten.
- Tests: `ApplicationApiClientTests` (Unit, gemockter `HttpMessageHandler`), `ApplicationGroupsControllerIntegrationTests`/`ApplicationApiClientIntegrationTests` (WebApplicationFactory), `ApplicationRepositoryIntegrationTests` (beide Varianten, Atomarität, Rollback).
- `docs/help/api/api.md` — Dokumentation des neuen Parameters.

## Implementierungsansatz

- Der Controller validiert den `content`-Parameter (`null`/`detach`/`delete`; sonst `400`) und delegiert in einem Aufruf an das Repository.
- Das Repository führt die Löschoperation in einer DB-Transaktion aus: bei `delete` werden die Anwendungen der Gruppe inklusive abhängiger Endpunkte/Endpunktgruppen (Achtung: `EndpointGroup.ChildGroups` ist `DeleteBehavior.Restrict` — Kindgruppen müssen vor Elterngruppen entfernt werden) gelöscht; bei `detach` werden die `ApplicationGroupId`-Werte der enthaltenen Anwendungen auf `null` gesetzt. Die Gruppe selbst wird in beiden Varianten im selben Transaktionskontext entfernt.
- Der API-Client baut die URL mit `?content=...` und sendet den DELETE-Request über die vorhandene `BuildDeleteRequest`-/`SendWithTokenNoContentAsync`-Infrastruktur.

## Konfiguration

Keine Konfigurationsänderungen erforderlich.

## Offene Fragen

Keine.
