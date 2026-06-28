# Ausfuehrungsfluss von Endpunkten

## Relevante Stellen

- `src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs`
- `src/Schnittstellenzentrale.Core/Models/Endpoint.cs`
- `src/Schnittstellenzentrale.Core/Enums/ScriptType.cs`
- `src/Schnittstellenzentrale.Core/Interfaces/IEndpointRepository.cs`
- `src/Schnittstellenzentrale.Core/Interfaces/IEndpointExecutionService.cs`

## Aktueller Zustand

`EndpointExecutionService.ExecuteAsync(...)` fuehrt den Ablauf in dieser Reihenfolge aus:

1. Pre-Request-Skript ausfuehren, falls vorhanden
2. HTTP-Request senden
3. Post-Request-Skript ausfuehren, falls vorhanden
4. Logging und History nur bei vollstaendig erfolgreichem Ablauf

Der Skriptkontext enthaelt dabei:

- `Request`
- `Response` nur fuer Post-Request-Skripte
- `ExecuteEndpoint` als Callback fuer `sz.execute(name)`
- `CallDepth` als Rekursionsschutz
- `EndpointName`
- `ScriptType`

Der Rekursionsschutz ist aktuell rein auf Aufruftiefe pro Endpunkt-ID ausgelegt. Er verhindert dieselbe Endpunkt-ID nach `MaxCallDepth`, aber es gibt keine eigene Semantik fuer eine manuelle Wiederholung des aktuell laufenden Endpunkts.

## Relevante Folgerung fuer die Aenderung

Fuer `sz.repeat()` muss der Ablauf den aktuellen Endpunkt erneut ausfuehren koennen, ohne eine neue Rekursion in den Authenticate-Pfad zu erzeugen. Das betrifft den Trennpunkt zwischen Skript und Ausfuehrungsservice.

## Testabdeckung

Vorhandene Tests decken bereits ab:

- Pre-Skript blockiert den HTTP-Request bei Fehlern
- Post-Skript kann Response-Daten lesen
- `sz.execute` loest einen weiteren Endpunkt auf
- Rekursionsschutz greift bei wiederholtem Aufruf desselben Endpunkts

Es fehlt aktuell eine Abdeckung fuer:

- erneute Ausfuehrung des aktuellen Endpunkts nach erfolgreichem Authenticate-Aufruf
- Unterdrueckung einer Wiederholung beim Authenticate-Endpunkt selbst
