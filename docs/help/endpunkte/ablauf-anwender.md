# Endpunkte — Ablauf für Anwender

## Voraussetzungen

- Eine Anwendung ist angelegt und im Navigationsbaum sichtbar.
- Ein Endpunkt ist bereits vorhanden oder wird neu angelegt.

---

## Schritt-für-Schritt-Anleitung

### 1. Endpunkt öffnen oder anlegen

Klicken Sie im Navigationsbaum auf einen vorhandenen Endpunkt oder legen Sie über das Kontextmenü einer Anwendung einen neuen an. Die Endpunkt-Seite öffnet sich auf der rechten Seite.

### 2. Pfad eingeben

Geben Sie im Pfadfeld den relativen Pfad ein. Beispiele:

- `/api/applications` — einfacher Pfad ohne Parameter
- `/api/applications/{id}` — Pfad mit Platzhalter
- `/api/applications/{id}?filter=active` — Pfad mit Platzhalter und Query-String

> **Hinweis:** Sie können den Pfad inklusive Query-String eingeben. Die Schnittstellenzentrale trennt den Query-String automatisch heraus, sobald Sie das Feld verlassen.

### 3. Feld verlassen (Tab oder Klick außerhalb)

Sobald Sie das Pfadfeld verlassen:

- Enthielt der Pfad einen Query-String (z. B. `?filter=active`), wird er entfernt und als separater Eintrag im Tab **Query-Parameter** angezeigt.
- Enthielt der Pfad Platzhalter (z. B. `{id}`), erscheinen sie als neue Einträge im Tab **Query-Parameter** — ohne Löschen-Button.
- Das Pfadfeld aktualisiert sich und zeigt die aufgelöste URL an (Platzhalter werden durch eingetragene Werte ersetzt, Query-Parameter werden angehängt).

### 4. Query-Parameter-Tab öffnen

Klicken Sie auf den Tab **Query-Parameter**. Sie sehen jetzt alle Parameter:

- **Pfad-Platzhalter** (z. B. `id`) erscheinen oben ohne Löschen-Button — diese Parameter sind Teil der Pfadstruktur.
- **Reguläre Query-Parameter** (z. B. `filter`) erscheinen darunter mit Löschen-Button.

### 5. Werte eintragen

Tragen Sie in der Spalte **Wert** die gewünschten Werte ein. Verlassen Sie das Eingabefeld, damit das Pfadfeld die aufgelöste URL sofort aktualisiert.

> **Hinweis:** Bei Pfad-Platzhaltern wird der eingetragene Wert in den Pfad eingebaut. Bei leerem Wert bleibt der Platzhalter leer in der angezeigten URL.

### 6. Skripte eingeben (optional)

Wenn Sie vor oder nach dem HTTP-Request JavaScript-Code ausführen möchten, wechseln Sie zu einem der Skript-Tabs:

- **Pre-Request-Skript**: Wird vor dem Senden ausgeführt. Über `sz.environment.set(name, value)` können Sie Umgebungsvariablen setzen, die dann in der `{{...}}`-Platzhalterauflösung des Requests verwendet werden.
- **Post-Request-Skript**: Wird nach dem Empfangen der Antwort ausgeführt. Über `sz.response.body.asJson()` können Sie die Antwort auswerten und z. B. ein Token in eine Umgebungsvariable schreiben.

Geben Sie den JavaScript-Code in das mehrzeilige Eingabefeld ein. Sie können den Code direkt eintippen — ein Syntaxcheck findet erst zur Laufzeit statt.

> **Hinweis:** Das `sz`-Objekt stellt folgende Funktionen bereit: `sz.environment.get(name)`, `sz.environment.set(name, value)`, `sz.request.*`, `sz.response.*` (nur im Post-Skript) und `sz.execute(name)` zum Aufrufen anderer Endpunkte.

### 7. Anfrage senden

Klicken Sie auf **Anfrage senden**. Wenn noch ungespeicherte Änderungen vorhanden sind, werden diese automatisch gespeichert, bevor die Anfrage abgeschickt wird.

Die tatsächlich gesendete URL enthält:
- Den Pfad mit allen ersetzten Platzhaltern.
- Alle regulären Query-Parameter als `?key=value`-Anhang.

### 8. Ergebnis prüfen

Unterhalb der Endpunkt-Seite erscheint der Antwortbereich mit:
- HTTP-Statuscode
- Antwortdauer in Millisekunden
- Antwortgröße in Bytes
- Antwort-Body (Register **Body**)
- Antwort-Header (Register **Headers**)

Wurde ein Skript ausgeführt und ist dabei ein Fehler aufgetreten, erscheint eine Fehlermeldung im Antwortbereich. Bei einem Pre-Skript-Fehler wird keine Antwort angezeigt, da der HTTP-Request nicht gesendet wurde.

---

## Ergebnis

Nach dem Senden sehen Sie den HTTP-Statuscode und den Antwort-Body der Anfrage. Der gespeicherte Endpunkt enthält den bereinigten Pfad (ohne Query-String) sowie alle Parameter als separate Einträge. Enthält der Endpunkt Skripte, werden diese bei jedem Senden automatisch ausgeführt.
