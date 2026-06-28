# Detail: Ausfuehrungsergebnis und Response-Anzeige

## Datenmodell

`EndpointExecutionResult` liegt in `src/Schnittstellenzentrale.Core/Models/EndpointExecutionResult.cs` und enthaelt:

- `Success`
- `HttpSuccess`
- `StatusCode`
- `RequestDetails`
- `ResponseBody`
- `ErrorMessage`
- `ResponseHeaders`
- `DurationMs`
- `ResponseSizeBytes`

Das Objekt ist ein einfaches mutable DTO ohne Persistenzlogik. Es kann direkt in einem UI-seitigen Cache gespeichert werden.

## Erzeugung

`IEndpointExecutionService.ExecuteAsync(Endpoint endpoint)` gibt ein `EndpointExecutionResult` zurueck.

`EndpointExecutionService` baut das Ergebnis in mehreren Pfaden:

- Bei fehlender Application oder Skriptfehlern wird ein Ergebnis mit `Success = false` und `ErrorMessage` erzeugt.
- Bei HTTP-Ausfuehrung wird in `BuildResult` Statuscode, Body, Header, Dauer und Response-Groesse gesetzt.
- Erfolgreiche HTTP- und Skript-Ausfuehrungen werden zusaetzlich in History/Activity Log erfasst.

Die Anforderung betrifft nicht die Erzeugung des Ergebnisses, sondern dessen UI-seitige Wiederverwendung nach Navigation.

## Verwendung in EndpointPage

`SendRequestAsync()` in `EndpointPage`:

1. Speichert bei Dirty-State zuerst den Endpunkt.
2. Laedt den Endpunkt ueber `ApplicationApiClient.GetEndpointByIdAsync(_model.Id)` frisch.
3. Fuellt bei Bedarf `refreshed.Application`.
4. Weist `_result = await ExecutionService.ExecuteAsync(refreshed)` zu.

Die Response-Sektion rendert anschliessend direkt aus `_result`.

## Response-Panels

`ResponseBodyPanel` und `ResponseHeadersPanel` sind bereits entkoppelte Anzeige-Komponenten. Sie erhalten nur Body bzw. Header aus `_result`. Fuer die Wiederherstellung alter Ergebnisse muss dort voraussichtlich nichts angepasst werden.

## Cache-Inhalt

Fuer einen Ergebnis-Cache reicht das komplette `EndpointExecutionResult`. Es sind keine weiteren Daten erforderlich, um die bestehende Anzeige wiederherzustellen.

Pruefenswert bei der Planung:

- Ob `ErrorMessage`-Ergebnisse ebenfalls gespeichert werden sollen.
- Ob gecachte Ergebnisse beim erneuten erfolgreichen Senden ersetzt werden.
- Ob gecachte Ergebnisse bei Endpunktloeschung oder Speichern mit geaenderter URL verworfen werden sollen.

