# Offene Aufgaben

Erstellt am: 2026-05-24
Abbruchgrund: Kein Fortschritt zwischen den letzten zwei Iterationen (Iteration 1: 3 Befunde, Iteration 2: 5 Befunde)

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

Keine (Plan-Review: Vollständig umgesetzt).

## Code-Review-Befunde

- [ ] **EndpointPage.razor — Doppelter Code:** `ResolveDisplayUrl()` enthält nahezu identische Logik wie `BuildRequest()` in `EndpointExecutionService.cs`. Empfehlung: Statische Hilfsmethode `EndpointUrlBuilder.Resolve(...)` extrahieren und in beiden Stellen verwenden.
- [ ] **EndpointPage.razor — Fehlende Validierung / falsches Dirty-Flag:** `OnPathBlur()` ruft `MarkDirty()` bedingungslos auf, auch ohne inhaltliche Änderung. Empfehlung: Vor `MarkDirty()` prüfen, ob Pfad oder `_queryParameters` sich tatsächlich geändert haben.
- [ ] **EndpointExecutionServiceTests.cs — Doppelter Code:** Beide neuen Testmethoden (`BuildRequest_ErsetztPfadPlatzhalterDurchGespeicherteWerte`, `BuildRequest_HaengtNurNichtPlatzhalterParameterAlsQueryStringAn`) richten Mock-Setup vollständig inline ein, obwohl `CreateService()` bereits existiert. Empfehlung: Tests auf `CreateService()` umstellen.
- [ ] **RequestQueryParamsPanel.razor — Redundante Initialisierung:** `public bool IsPathParameter { get; set; } = false;` — Initialwert `= false` hat keinen Effekt. Empfehlung: Entfernen.
- [ ] **EndpointPageTests.cs — Doppelter Code:** `CreateEndpoint()` und `CreateEndpointWithPath()` duplizieren 8 identische Felder. Empfehlung: `CreateEndpoint()` um optionale Parameter erweitern, `CreateEndpointWithPath()` entfernen.
