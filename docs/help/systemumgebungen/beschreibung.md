# Systemumgebungen — Beschreibung

## Zweck

Systemumgebungen ermöglichen es, Variablensätze zu definieren, die beim Ausführen von Endpunkten automatisch eingesetzt werden. Damit lassen sich Konfigurationsunterschiede zwischen Umgebungen (z. B. verschiedene Basis-URLs, API-Schlüssel, Pfade) zentral verwalten, ohne jeden Endpunkt einzeln anpassen zu müssen.

## Funktionsweise

### Variablendefinition

Eine Systemumgebung (`SystemEnvironment`) besteht aus einem Namen und einer Liste von Variablen. Jede Variable (`EnvironmentVariable`) hat einen Namen und einen Wert. Optional kann der Wert maskiert werden (`IsValueMasked`): Er wird dann in der Benutzeroberfläche als Passwortfeld dargestellt und erscheint nicht im Klartext.

### Platzhalterauflösung

In Endpunkten können Variablen über die Syntax `{{variablenname}}` referenziert werden. Platzhalter dieser Form werden in folgenden Feldern erkannt und aufgelöst:

- Basis-URL der Anwendung
- Relativer Pfad des Endpunkts
- Header-Namen und -Werte
- Query-Parameter-Namen und -Werte
- Bearer-Token
- Request-Body

Die Auflösung erfolgt ausschließlich zur Laufzeit beim Absenden einer Anfrage. Die gespeicherten Endpunktdaten bleiben unverändert. Ist keine Umgebung aktiv oder ist eine Variable nicht definiert, wird der Platzhalter durch einen leeren String ersetzt.

### Modus-Zuordnung

Jede Systemumgebung ist einem `StorageMode` zugeordnet (`Team` oder `Benutzer`). Im Team-Modus sind alle Umgebungen für alle Nutzer sichtbar. Im Benutzermodus sieht jeder Nutzer nur seine eigenen Umgebungen; die Zuordnung erfolgt über den Windows-Benutzernamen (`Owner`).

### Auswahl der aktiven Umgebung

Im Header der Anwendung befindet sich neben dem Modus-Schalter eine Auswahlbox mit allen verfügbaren Umgebungen des aktiven Modus. Die Auswahl wird pro Modus im `localStorage` des Browsers gespeichert und beim nächsten Öffnen oder beim Moduswechsel wiederhergestellt.

### Verwaltung

Über das Zahnrad-Icon neben der Auswahlbox öffnet sich das Verwaltungsoverlay. Dort können Umgebungen angelegt, bearbeitet und gelöscht werden. Änderungen anderer Benutzer werden im Team-Modus in Echtzeit über SignalR übertragen.

## Beispiele

**Variable für Basis-URL definieren:**

Umgebung „Entwicklung" mit Variable `host = http://dev.intern:8080`.

Endpunkt mit Basis-URL `{{host}}` und relativem Pfad `/api/users` sendet die Anfrage an `http://dev.intern:8080/api/users`.

**Variable im Pfad verwenden:**

Variable `version = v2`. Endpunkt mit relativem Pfad `/api/{{version}}/items` wird zu `/api/v2/items` aufgelöst.

**Bearer-Token aus Umgebung:**

Variable `token = abc123`. Endpunkt mit Bearer-Token `{{token}}` sendet `Authorization: Bearer abc123`.

## Einschränkungen

- Maskierte Variablenwerte werden nur in der Benutzeroberfläche verborgen. In der Datenbank und bei der Übertragung im Request sind sie unverschlüsselt.
- Ist keine Umgebung aktiv, werden alle `{{...}}`-Platzhalter durch leere Strings ersetzt. Die Anfrage wird dennoch abgesendet.
- Die Platzhaltersyntax `{{name}}` unterscheidet sich von der Pfadparameter-Syntax `{name}`. Beide Systeme sind unabhängig und lassen sich kombinieren; die `{{...}}`-Auflösung erfolgt zuerst.
- Variablennamen sind innerhalb einer Umgebung eindeutig (Groß-/Kleinschreibung beachten); Umgebungsnamen sind pro Modus und Benutzer eindeutig.
