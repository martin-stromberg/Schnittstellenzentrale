# Umsetzungsplan

## Ziel

Die Skript-API soll so erweitert werden, dass `sz.execute(name)` nur noch einen Boolean zurueckgibt und `sz.repeat()` den aktuellen Endpunkt erneut ausfuehren kann, sobald der Authenticate-Aufruf erfolgreich war. Die neue Logik muss fuer REST/Swagger und OData gleichermaßen gelten und den Authenticate-Endpunkt selbst von der Wiederholungslogik ausnehmen.

## Vorgehen

1. Skript-Kontext und Jint-Bindings erweitern
   - `ScriptContext` um einen expliziten Repeat-Callback bzw. ein Repeat-Signal fuer den aktuell laufenden Endpunkt erweitern.
   - `EndpointScriptRunner` so anpassen, dass `sz.execute(name)` nur noch `true`/`false` liefert.
   - `sz.repeat()` als neues `sz`-API registrieren und nur im dafuer vorgesehenen Kontext bereitstellen.

2. Endpunktwiederholung im Ausfuehrungsfluss verankern
   - `EndpointExecutionService` so erweitern, dass eine erfolgreiche Authenticate-Ausfuehrung den nachfolgenden Aufruf von `sz.repeat()` fuer den aktuellen Endpunkt ausloesen kann.
   - Den bestehenden Rekursionsschutz weiterverwenden, damit Wiederholungen nicht in Endlosschleifen muenden.
   - Authenticate-Endpunkte ueber eine zentrale Sonderfall-Pruefung von der Wiederholungslogik ausschliessen.

3. Testabdeckung aktualisieren
   - `EndpointScriptRunnerTests`: Boolean-Rueckgabewert von `sz.execute(name)`, Registrierung von `sz.repeat()`, Fehlerfaelle.
   - `EndpointExecutionServiceTests`: Wiederholung des aktuellen Endpunkts nach erfolgreichem Authenticate-Aufruf, keine Wiederholung fuer Authenticate selbst.
   - Import-nahe Tests nur dort anpassen, wo harte Erwartungen an die alte Objektform von `sz.execute(...)` oder an die erzeugten Skripte existieren.

4. Anschlusspruefung
   - Die vorhandenen Swagger- und OData-Pfade gegen die neue Skript-API pruefen, ohne die Importlogik unnoetig umzubauen.
   - Sicherstellen, dass vorhandene Authenticate-Sonderfaelle in REST und OData unveraendert bleiben.

## Offene Punkte

Keine offenen Punkte.
