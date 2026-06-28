# Anforderungen

## Ausgangslage

Bei der automatisch geladenen Anwendung der eigenen API sollen die Post-Skripte erweitert werden.

## Ziel

Der Unauthorized-Status soll im Post-Skript abgefangen werden. Wird dieser erkannt, soll der Authenticate-Endpunkt ausgefuehrt werden.

## Funktionale Anforderungen

1. In den Skripten ist die Nutzung von `sz.execute(name)` vorzusehen.
2. Die vordefinierte Skriptmethode `sz.execute(name)` wird um einen Rueckgabetyp `Boolean` erweitert.
3. Der Rueckgabewert zeigt an, ob die Ausfuehrung erfolgreich war.
4. War der Aufruf erfolgreich, soll `sz.repeat()` die Ausfuehrung des aktuellen Endpunkts wiederholen.
5. Diese Skriptergaenzungen gelten nicht fuer den Authenticate-Endpunkt selbst.
6. Die Anforderungen gelten fuer die Swagger-Definition der REST-API.
7. Die Anforderungen gelten ebenfalls fuer die Metadata-Definition der OData-API.

## Randbedingungen

- Die automatische Wiederholung darf nur nach erfolgreichem Authenticate-Aufruf ausgelost werden.
- Fuer den Authenticate-Endpunkt selbst darf keine rekursive Wiederholung durch die neuen Skriptergaenzungen entstehen.
