# Anforderung

## Ausgangslage

Wenn ein Anwender eine Anfrage an einen Endpunkt ausloest, kann die Ausfuehrung je nach Dauer der Verarbeitung laenger dauern. Aktuell ist waehrend dieser Wartezeit nicht erkennbar, ob die Anfrage tatsaechlich gestartet wurde und noch ausgefuehrt wird. Erst wenn das Ergebnis angezeigt wird, wird fuer den Anwender sichtbar, dass die Anfrage verarbeitet wurde.

## Ziel

Der Anwender soll unmittelbar nach dem Ausloesen einer Endpunkt-Anfrage eine sichtbare Rueckmeldung erhalten, dass die Anfrage ausgefuehrt wird. Die Rueckmeldung soll bestehen bleiben, bis die Anfrage abgeschlossen ist und ein Ergebnis oder Fehler angezeigt wird.

## Funktionale Anforderungen

1. Beim Ausloesen einer Anfrage an einen Endpunkt muss der Ausfuehrungszustand sichtbar werden.
2. Waehrend der laufenden Anfrage muss klar erkennbar sein, dass die Verarbeitung noch nicht abgeschlossen ist.
3. Nach erfolgreichem Abschluss der Anfrage muss die Ausfuehrungsanzeige beendet und das Ergebnis angezeigt werden.
4. Bei einem Fehler muss die Ausfuehrungsanzeige beendet und eine passende Fehlerrueckmeldung angezeigt werden.
5. Die Anzeige darf nicht dauerhaft stehen bleiben, wenn die Anfrage abgeschlossen, fehlgeschlagen oder abgebrochen wurde.

## Akzeptanzkriterien

1. Wenn der Anwender eine Endpunkt-Anfrage startet, erscheint unmittelbar eine sichtbare Statusanzeige fuer die laufende Ausfuehrung.
2. Wenn die Anfrage laenger dauert, bleibt die Statusanzeige bis zum Abschluss der Anfrage sichtbar.
3. Wenn die Anfrage erfolgreich abgeschlossen wird, verschwindet die Statusanzeige und das Ergebnis wird angezeigt.
4. Wenn die Anfrage fehlschlaegt, verschwindet die Statusanzeige und der Fehlerzustand wird angezeigt.
5. Der Anwender kann anhand der Oberflaeche unterscheiden, ob keine Anfrage laeuft oder eine Anfrage gerade ausgefuehrt wird.

## Nicht-Ziele

1. Es wird keine Aenderung an der fachlichen Verarbeitung der Endpunkt-Anfrage gefordert.
2. Es wird keine Aenderung am Ergebnisformat der Anfrage gefordert.
3. Es wird keine konkrete visuelle Ausgestaltung der Statusanzeige vorgegeben.
4. Es wird kein Fortschrittswert in Prozent gefordert.

## Offene Punkte

1. Welche konkrete Oberflaeche oder welcher Dialog loest die betroffene Endpunkt-Anfrage aus?
2. Soll waehrend einer laufenden Anfrage das erneute Ausloesen derselben Anfrage verhindert werden?
3. Gibt es mehrere betroffene Endpunkte oder nur einen konkreten Endpunkt?
