# Systemumgebungen — Ablauf für Anwender

## Voraussetzungen

Die Schnittstellenzentrale ist geöffnet. Im Header ist der gewünschte Modus (Team oder Benutzer) eingestellt.

---

## Neue Umgebung anlegen

### 1. Verwaltungsoverlay öffnen

Im Header auf das Zahnrad-Icon neben der Umgebungsauswahlbox klicken. Das Overlay „Systemumgebungen verwalten" öffnet sich.

### 2. Neue Umgebung erstellen

Auf die Schaltfläche **Neu** klicken. Das Formular zum Anlegen einer Umgebung wird angezeigt.

### 3. Namen eingeben

Einen eindeutigen Namen für die Umgebung eingeben (z. B. „Entwicklung", „Test", „Produktion").

### 4. Variablen hinzufügen

Auf **+ Variable hinzufügen** klicken. Es erscheint eine neue Zeile in der Tabelle mit den Feldern „Name" und „Wert".

- Im Feld **Name** den Variablennamen eingeben (z. B. `host`).
- Im Feld **Wert** den zugehörigen Wert eingeben (z. B. `http://dev.intern:8080`).
- Über das Icon in der Spalte **Maskiert** kann der Wert verborgen werden: Ein Schloss-Icon zeigt an, dass der Wert als Passwortfeld dargestellt wird. Ein Auge-Icon stellt den Klartext wieder her.

Weitere Variablen können durch erneutes Klicken auf **+ Variable hinzufügen** angelegt werden. Nicht mehr benötigte Variablen werden über die **×**-Schaltfläche in der jeweiligen Zeile entfernt.

### 5. Speichern

Auf **Speichern** klicken. Die Umgebung erscheint in der Liste. Das Overlay kann über das **×** in der Titelzeile geschlossen werden.

> **Hinweis:** Der Umgebungsname muss im aktuellen Modus eindeutig sein. Ist ein Name bereits vergeben, erscheint die Meldung „Eine Umgebung mit diesem Namen existiert bereits."

---

## Umgebung bearbeiten

1. Im Overlay die gewünschte Umgebung aus der Liste auswählen und auf **Bearbeiten** klicken.
2. Namen oder Variablen ändern.
3. Auf **Speichern** klicken.

---

## Umgebung löschen

1. Im Overlay auf **Löschen** neben der gewünschten Umgebung klicken.
2. Den Bestätigungsdialog mit **Löschen** bestätigen oder mit **Abbrechen** verwerfen.

> **Hinweis:** Beim Löschen werden alle Variablen der Umgebung ebenfalls entfernt. War die gelöschte Umgebung die aktive, wird die Auswahl sofort auf „— Keine Umgebung —" zurückgesetzt.

---

## Aktive Umgebung auswählen

In der Auswahlbox im Header die gewünschte Umgebung wählen. Ab diesem Moment werden alle `{{variablenname}}`-Platzhalter in Endpunkten durch die Werte dieser Umgebung ersetzt. Die Auswahl bleibt nach dem Schließen und Neuladen des Browsers erhalten.

Die Auswahl „— Keine Umgebung —" deaktiviert die Variablenauflösung; alle `{{...}}`-Platzhalter werden beim Senden durch leere Strings ersetzt.

---

## Platzhalter in Endpunkten verwenden

In Feldern wie Basis-URL, relativem Pfad, Headers, Query-Parametern, Bearer-Token oder Body kann auf eine Umgebungsvariable verwiesen werden:

```
{{variablenname}}
```

Beispiel: Ist in der aktiven Umgebung `host = https://api.example.com` definiert und der relative Pfad lautet `/{{host}}/v1/users`, wird beim Senden `/https://api.example.com/v1/users` als Pfad verwendet.

> **Hinweis:** Die Platzhaltersyntax mit doppelten geschwungenen Klammern `{{name}}` ist unabhängig von der einfachen Pfadparameter-Syntax `{name}`. Beide können in einem Endpunkt gleichzeitig vorkommen; die `{{...}}`-Auflösung erfolgt zuerst.

## Ergebnis

Nach Auswahl einer aktiven Umgebung werden alle konfigurierten Endpunkte mit den Variablenwerten dieser Umgebung ausgeführt. Im Pfadfeld der Endpunktansicht wird die aufgelöste URL angezeigt, sobald eine Umgebung aktiv ist.
