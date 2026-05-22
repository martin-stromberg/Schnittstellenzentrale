# Offene Aufgaben

Erstellt am: 2026-05-20
Abbruchgrund: Kein Fortschritt zwischen den letzten zwei Iterationen (Iteration 1: 7 offene Punkte, Iteration 2: 10 offene Punkte)

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

_Keine._

## Code-Review-Befunde

1. **Doppelter Code: `BuildCredentialTarget`** (`EndpointExecutionService.cs` / `RequestAuthPanel.razor`) — Identischer Format-String an zwei unabhängigen Stellen. Empfehlung: Gemeinsame statische Hilfsklasse `CredentialTargetBuilder` im Core- oder Infrastructure-Projekt, von beiden Stellen aufgerufen.

2. **Fehlerbehandlung: `switch`-Default in `BuildRequest`** (`EndpointExecutionService.cs`, Zeile 138) — Unbekannte `HttpMethod`-Werte fallen stillschweigend auf GET zurück. Empfehlung: `default`-Fall durch `ArgumentOutOfRangeException` ersetzen.

3. **Toter Code: Null-Check in `BuildResult`** (`EndpointExecutionService.cs`, Zeile 115) — `body == null ? 0 :` ist toter Code, da `ReadAsStringAsync()` nie `null` zurückgibt. Empfehlung: Vereinfachen auf `Encoding.UTF8.GetByteCount(body)`.

4. **Toter Code: `GetUngroupedEndpointsAsync`** (`IEndpointRepository` / `EndpointRepository`) — Die Methode wird im gesamten Branch nie aufgerufen. Empfehlung: Prüfen, ob sie benötigt wird; falls nicht, aus Interface und Implementierung entfernen.

5. **Fehlende Fehlerbehandlung: `ConnectHubAsync`** (`ApplicationGroupTree.razor`, Zeile 141–144) — `catch`-Block schluckt SignalR-Verbindungsausnahmen still. Empfehlung: Ausnahme per `ILogger` auf Debug-Level protokollieren.

6. **Doppelter DB-Zugriff: `GetEndpointsAsync`** (`Home.razor`, Zeilen 341 und 357) — `GetEndpointsAsync` wird für dieselbe `applicationId` in `HandleDeleteEndpointGroupRequested` und erneut in `OnEndpointGroupDeleteConfirmed` aufgerufen. Empfehlung: Snapshot als Feld `_deleteTargetEndpoints` speichern und wiederverwenden.

7. **Fehlende Kapselung: Reset-Muster in `Home.razor`** — Das Muster `CloseAllPanels(); _selectedApplicationId = null; _selectedEndpoint = null;` tritt an mehreren Stellen in leicht abweichender Reihenfolge auf. Empfehlung: `CloseAllPanels` um das Zurücksetzen beider Felder erweitern oder `ClearAll`-Methode einführen.

8. **Doppelter Code in `EndpointExecutionServiceTests`** — `Execute_WithAuthTypeNegotiate_*` und `Execute_WithAuthTypeNegotiateWithImpersonation_*` haben identisches Setup. Empfehlung: Zu einem `[Theory] [InlineData(...)]`-Test zusammenführen. Ebenso `CreateService`/`CreateServiceWithBody` auf einen optionalen `body`-Parameter konsolidieren.

9. **Toter Code: `ExecuteWithTwoEndpointContextsAsync`** (`TestHelpers.cs`) — Wird von keinem Test aufgerufen. Empfehlung: `SaveEndpoint_ConcurrentWrite_DetectsConflict` auf diese Hilfsmethode umstellen oder die Methode entfernen.
