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

## Pre/Post-Request-Skripte

Jeder Endpunkt kann optional zwei JavaScript-Skripte enthalten:

- **Pre-Request-Skript** (`PreRequestScript`): Wird ausgeführt, bevor die `{{...}}`-Platzhalter in URL, Headern und Body aufgelöst werden. Schlägt das Skript fehl, wird der HTTP-Request nicht gesendet.
- **Post-Request-Skript** (`PostRequestScript`): Wird ausgeführt, nachdem der HTTP-Request abgeschlossen ist. Schlägt das Skript fehl, bleibt das HTTP-Ergebnis erhalten — die Fehlermeldung wird ergänzend angezeigt.

Innerhalb der Skripte steht ein `sz`-API-Objekt bereit:

- `sz.environment.get(name)` — liest eine Umgebungsvariable aus der aktiven Systemumgebung.
- `sz.environment.set(name, value)` — setzt eine Umgebungsvariable in der aktiven Systemumgebung. Ist eine Systemumgebung aktiv, wird die Änderung sofort in der Datenbank persistiert und alle verbundenen Clients werden über SignalR benachrichtigt. Ist keine Systemumgebung aktiv, gilt die Änderung nur für die Laufzeit des aktuellen Requests (In-Memory). Nachfolgende `{{...}}`-Auflösungen sehen den neuen Wert in beiden Fällen sofort.
- `sz.request.url` / `sz.request.method` / `sz.request.headers` — Zugriff auf die Request-Daten.
- `sz.request.body.raw` / `sz.request.body.asJson()` / `sz.request.body.asXml()` — Zugriff auf den Request-Body.
- `sz.response.body.raw` / `sz.response.body.asJson()` / `sz.response.body.asXml()` / `sz.response.headers` — im Post-Skript: Zugriff auf die HTTP-Antwort.
- `sz.execute(name)` — führt einen anderen Endpunkt der gleichen Anwendung synchron aus und gibt dessen Ergebnis zurück (`success`, `statusCode`, `responseBody`, `errorMessage`).

Endpunkte ohne Skript verhalten sich unverändert wie bisher.

## Beispiele

**Token aus Response lesen und als Umgebungsvariable speichern:**

Post-Request-Skript:
```javascript
var body = sz.response.body.asJson();
sz.environment.set("token", body.access_token);
```

**Zweiten Endpunkt aufrufen (z. B. Login vor eigentlichem Request):**

Pre-Request-Skript:
```javascript
var result = sz.execute("Login");
sz.environment.set("bearer", result.responseBody);
```

## Swagger-Import mit Erweiterungsfeldern

Beim Import einer Swagger/OpenAPI-Definition kann jeder Endpunkt mit OpenAPI-Erweiterungsfeldern versehen werden. Der `SwaggerImportService` erkennt die folgenden Felder und überträgt sie automatisch auf die erzeugten Endpunkte:

| Erweiterungsfeld | Zielfeld | Beschreibung |
|------------------|----------|--------------|
| `x-sz-pre-request-script` | `PreRequestScript` | JavaScript-Code, der vor dem HTTP-Request ausgeführt wird. |
| `x-sz-post-request-script` | `PostRequestScript` | JavaScript-Code, der nach dem HTTP-Request ausgeführt wird. |
| `x-sz-bearer-token` | `AuthenticationType = BearerToken` + Credential Manager | Token-Wert (z. B. `{{schnittstellenzentrale.authToken}}`), der im Windows Credential Manager abgelegt wird. |

Alle Endpunkte werden einheitlich nach diesem Muster behandelt — es gibt keine hartcodierten Sonderfälle nach Pfad oder Endpunktname. Die Zuordnung erfolgt ausschließlich über die Erweiterungsfelder in der Swagger-Definition.

**Re-Import:** Beim erneuten Import werden Skripte und `AuthenticationType` mit den Werten aus der Swagger-Definition überschrieben. Fehlen die Erweiterungsfelder im Re-Import, werden `PreRequestScript`, `PostRequestScript` und `AuthenticationType` auf ihre Standardwerte (`null` bzw. `None`) zurückgesetzt — auch wenn diese Felder zuvor manuell gesetzt wurden.

## Einschränkungen

- Das Pfadfeld zeigt die aufgelöste URL, nicht das Template. Wer den Pfad bearbeiten möchte, sieht also die ersetzten Werte, nicht die `{name}`-Platzhalter — nach dem Verlassen des Felds wird der neue Pfad analysiert und neu aufgelöst.
- Wird ein Platzhalter im Pfad umbenannt, kann ein vorhandener Parameterwert nicht mehr automatisch zugeordnet werden; er verbleibt als regulärer (löschbarer) Query-Parameter.
- Einträge mit leerem Key werden beim Aufbau des Query-Strings und bei der Platzhalter-Ersetzung übersprungen.
- Ein im extrahierten Query-String vorhandener Key, der bereits manuell angelegt wurde, überschreibt den vorhandenen Wert nicht — der bestehende Eintrag bleibt erhalten.
- Skripte haben ein Ausführungs-Timeout von 5 Sekunden; Endlosschleifen werden nach dieser Zeit abgebrochen.
- `sz.environment.set()` persistiert die Änderung in der Datenbank, wenn eine Systemumgebung aktiv ist. Schlägt die Datenbankoperation oder die SignalR-Benachrichtigung fehl, wird das Post-Request-Skript als fehlgeschlagen gewertet. Ist keine Systemumgebung aktiv, ist die Änderung nur für die Laufzeit des Requests im Arbeitsspeicher vorhanden.
- `sz.execute()` schlägt fehl, wenn der angegebene Name innerhalb der Anwendung nicht eindeutig ist (mehrere Treffer). Ein Rekursionsschutz verhindert, dass derselbe Endpunkt mehr als zweimal im gleichen Aufrufbaum aufgerufen wird.
