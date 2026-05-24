# Endpunkte — Beschreibung

## Zweck

Endpunkte repräsentieren einzelne HTTP-Anfragen einer Anwendung. Sie können direkt aus der Schnittstellenzentrale heraus ausgeführt werden, ohne dass der Anwender die URL manuell zusammensetzen muss. Pfad-Platzhalter und Query-Parameter werden getrennt verwaltet und vor dem Senden automatisch in die URL eingebaut.

## Funktionsweise

### Pfad-Platzhalter

Ein Pfad wie `/api/{id}/items` enthält den Platzhalter `{id}`. Die Schnittstellenzentrale erkennt alle Platzhalter der Form `{name}` im relativen Pfad automatisch und zeigt sie im Tab **Query-Parameter** als eigene Einträge an. Platzhalter-Einträge sind nicht löschbar und erscheinen immer an erster Stelle der Liste.

Wird ein Wert für einen Platzhalter eingetragen (z. B. `42`), zeigt das Pfadfeld die aufgelöste URL `/api/42/items`.

### Query-String-Extraktion

Gibt der Anwender einen Pfad mit Query-String ein (z. B. `/api/items?filter=active&page=1`), wird der Query-String beim Verlassen des Pfadfelds automatisch extrahiert:

- `RelativePath` wird auf `/api/items` gekürzt.
- `filter=active` und `page=1` erscheinen als löschbare Einträge im Query-Parameter-Tab.

Dieser Vorgang findet auch beim erstmaligen Laden eines gespeicherten Endpunkts statt, falls der gespeicherte Pfad noch einen Query-String enthält.

### Aufgelöste URL-Anzeige

Das Pfadfeld zeigt stets die aufgelöste URL:

- Platzhalter werden durch den eingetragenen Wert ersetzt (leerer Wert ergibt leerer String).
- Reguläre Query-Parameter werden als `?key=value`-Anhang ergänzt.
- Das `?` entfällt, wenn keine regulären Query-Parameter vorhanden sind.

Beim Bearbeiten des Pfadfelds gibt der Anwender das Template ein (z. B. `/api/{id}/items`); die aufgelöste Darstellung wird erst nach dem Verlassen des Felds aktualisiert.

### Ausführung

Beim Senden einer Anfrage baut der `EndpointExecutionService` die URL wie folgt zusammen:

1. Platzhalter-Parameter werden aus `QueryParameters` gesucht und per `string.Replace` in den Pfad eingesetzt (URL-kodiert via `Uri.EscapeDataString`).
2. Alle übrigen Parameter werden als Query-String angehängt.

### Speicherung

Alle Parameter — sowohl Platzhalter-Werte als auch reguläre Query-Parameter — werden als gleichartige `EndpointQueryParameter`-Datensätze gespeichert. Das `IsPathParameter`-Kennzeichen existiert nur im Arbeitsspeicher (Klasse `QueryParamEntry`) und wird nicht in der Datenbank persistiert. Beim Laden wird es aus dem Template abgeleitet.

## Beispiele

**Pfad mit Platzhalter eingeben:**

Eingabe im Pfadfeld: `/api/applications/{id}`

Nach dem Verlassen des Felds erscheint im Query-Parameter-Tab ein Eintrag `id` ohne Löschen-Button. Das Pfadfeld zeigt weiter `/api/applications/` (leerer Platzhalter) bzw. `/api/applications/42`, sobald `42` eingetragen ist.

**Pfad mit Query-String eingeben:**

Eingabe: `/api/applications/{id}?filter=active`

Nach dem Verlassen: Pfad wird zu `/api/{id}` bereinigt; im Tab erscheinen `id` (nicht löschbar) und `filter=active` (löschbar).

**Senden der Anfrage:**

Bei eingetragenem Wert `42` für `id` und `active` für `filter` lautet die tatsächlich gesendete URL:
`http://example.com/api/applications/42?filter=active`

## Einschränkungen

- Das Pfadfeld zeigt die aufgelöste URL, nicht das Template. Wer den Pfad bearbeiten möchte, sieht also die ersetzten Werte, nicht die `{name}`-Platzhalter — nach dem Verlassen des Felds wird der neue Pfad analysiert und neu aufgelöst.
- Wird ein Platzhalter im Pfad umbenannt, kann ein vorhandener Parameterwert nicht mehr automatisch zugeordnet werden; er verbleibt als regulärer (löschbarer) Query-Parameter.
- Einträge mit leerem Key werden beim Aufbau des Query-Strings und bei der Platzhalter-Ersetzung übersprungen.
- Ein im extrahierten Query-String vorhandener Key, der bereits manuell angelegt wurde, überschreibt den vorhandenen Wert nicht — der bestehende Eintrag bleibt erhalten.
