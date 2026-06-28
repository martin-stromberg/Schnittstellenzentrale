# Bestandsaufnahme

## Geltungsbereich

Die Anforderung betrifft den Skript-Lauf zur Endpunkt-Ausfuehrung sowie die Importpfade fuer REST/Swagger und OData.

## Beobachtete Bestandteile

- [Skript-Laufzeit und `sz`-API](inventory/script-runtime.md)
- [Ausfuehrungsfluss von Endpunkten](inventory/endpoint-execution-flow.md)
- [REST-Import aus Swagger/OpenAPI](inventory/swagger-import.md)
- [OData-Import aus CSDL-Metadaten](inventory/odata-import.md)

## Kurzfazit

Die aktuelle Implementierung stellt `sz.execute(name)` bereits als synchronen Aufruf bereit, gibt aber ein Objekt mit `success`, `statusCode`, `responseBody` und `errorMessage` zurueck. Die neue Anforderung verlangt stattdessen einen Boolean-Rueckgabewert und eine daran gekoppelte Wiederholung des aktuellen Endpunkts ueber `sz.repeat()`.

Die Importer fuer Swagger und OData erzeugen Post-Skripte beziehungsweise Authentifizierungsmetadaten bereits an zentralen Stellen, sodass die Aenderung an der Skript-API dort mitgezogen werden muss. Die Authenticate-Endpunkte sind in beiden API-Welten separat vorhanden und muessen von der neuen Wiederholungslogik ausgeschlossen bleiben.
