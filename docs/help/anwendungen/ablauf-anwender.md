# Anwendungen — Ablauf für Anwender

## Voraussetzungen

Die Anwendung ist geöffnet und die Startseite ist sichtbar. Die linke Seitenleiste zeigt den Navigationsbaum mit den Schaltflächen **Neue Gruppe** und **Neue Anwendung**.

---

## Neue Gruppe anlegen

### 1. Formular öffnen

Klicken Sie auf **Neue Gruppe**. Das Formular wird direkt unterhalb der Schaltflächen eingeblendet.

### 2. Name eingeben

Geben Sie im Feld **Name** den gewünschten Gruppennamen ein (Pflichtfeld).

### 3. Speichern

Klicken Sie auf **Speichern**. Die Gruppe wird angelegt, das Formular schließt sich, und die Seitenleiste wird aktualisiert.

> **Hinweis:** Ist das Feld „Name" leer, erscheint eine Fehlermeldung direkt neben dem Feld.

### Abbrechen

Klicken Sie auf **Abbrechen**, um das Formular ohne Speichern zu schließen.

---

## Neue Anwendung anlegen

### 1. Formular öffnen

Klicken Sie auf **Neue Anwendung**. Das Formular wird direkt unterhalb der Schaltflächen eingeblendet.

### 2. Pflichtfelder ausfüllen

- **Name** — Anzeigename der Anwendung in der Seitenleiste.
- **Basis-URL** — Basisadresse des Dienstes (z. B. `https://intern/meinservice`).

### 3. Optionale Felder ausfüllen (nach Bedarf)

- **Beschreibung** — Kurze Erläuterung zum Zweck der Anwendung.
- **Schnittstellen-URL** — URL zur API-Beschreibung (z. B. Swagger/OpenAPI oder OData-Metadaten). Sobald eine URL eingetragen ist, erkennt die Anwendung den Typ automatisch und zeigt ein Badge an: `REST (Swagger/OpenAPI)` oder `OData`. Bei nicht erkanntem Format erscheint `Typ nicht erkannt`.
- **Gruppe** — Wählen Sie eine vorhandene Gruppe. Die Option **Ohne Gruppe** belässt die Anwendung außerhalb jeder Gruppe.

### 4. Speichern

Klicken Sie auf **Speichern**. Die Anwendung wird angelegt und die Seitenleiste aktualisiert. Im Benutzermodus wird sie automatisch Ihrem Konto zugeordnet.

> **Hinweis:** Sind „Name" oder „Basis-URL" leer, erscheint eine Fehlermeldung direkt neben dem jeweiligen Feld.

---

## Gruppe umbenennen

### 1. Zahnrad-Menü öffnen

Fahren Sie mit der Maus auf den Gruppennamen — das ⚙️-Symbol erscheint rechts neben dem Namen. Klicken Sie darauf.

### 2. „Umbenennen" wählen und Namen eingeben

Klicken Sie auf **Umbenennen**. Ein Dialog öffnet sich mit dem aktuellen Namen vorausgefüllt. Ändern Sie den Namen und klicken Sie auf **Speichern**.

> **Hinweis:** Das Namensfeld darf nicht leer sein.

### Ergebnis

Der Gruppenname wird sofort aktualisiert. Im Team-Modus sehen andere verbundene Benutzer die Änderung in Echtzeit.

---

## Gruppe löschen

### 1. Zahnrad-Menü öffnen und „Löschen" wählen

Fahren Sie mit der Maus auf den Gruppennamen und klicken Sie auf das ⚙️-Symbol, dann auf **Löschen**.

### 2. Löschoption wählen

Ein Dialog erscheint mit der Anzahl der enthaltenen Anwendungen. Sie haben drei Möglichkeiten:

| Schaltfläche | Auswirkung |
|---|---|
| **Mitlöschen** | Die Gruppe und alle enthaltenen Anwendungen werden dauerhaft gelöscht. |
| **Nur Gruppe löschen** | Die Gruppe wird gelöscht; die Anwendungen bleiben unter „Ohne Gruppe" erhalten. |
| **Abbrechen** | Es wird nichts verändert. |

> **Hinweis:** Das Löschen kann nicht rückgängig gemacht werden.

---

## Anwendung bearbeiten

### 1. Zahnrad-Menü öffnen und „Bearbeiten" wählen

Fahren Sie mit der Maus auf die Anwendungszeile — das ⚙️-Symbol erscheint rechts. Klicken Sie darauf, dann auf **Bearbeiten**.

### 2. Felder ändern

Ein Bearbeitungsformular erscheint mit folgenden vorausgefüllten Feldern:
- **Name** (Pflichtfeld)
- **Basis-URL** (Pflichtfeld)
- **Beschreibung**, **Schnittstellen-URL**
- **Gruppe** (Dropdown; „Ohne Gruppe" wählen, um die Anwendung keiner Gruppe zuzuweisen)

### 3. Speichern oder Abbrechen

Klicken Sie auf **Speichern** oder auf **Abbrechen**, um die Änderungen zu verwerfen.

> **Hinweis:** Falls die Anwendung zwischenzeitlich von jemand anderem geändert wurde, erscheint die Meldung „Die Daten wurden zwischenzeitlich geändert. Bitte laden Sie die Seite neu und versuchen Sie es erneut."

---

## Anwendung aus einer Gruppe entfernen

### 1. Zahnrad-Menü öffnen und „Aus Gruppe entfernen" wählen

Fahren Sie mit der Maus auf die Anwendungszeile und klicken Sie auf das ⚙️-Symbol. Die Option **Aus Gruppe entfernen** ist nur sichtbar, wenn die Anwendung einer Gruppe angehört.

> **Hinweis:** Es erscheint kein Bestätigungsdialog — die Aktion wird sofort ausgeführt.

### Ergebnis

Die Anwendung erscheint jetzt unter „Ohne Gruppe" im Navigationsbaum.

---

## Anwendung löschen

### 1. Zahnrad-Menü öffnen und „Löschen" wählen

Fahren Sie mit der Maus auf die Anwendungszeile und klicken Sie auf das ⚙️-Symbol, dann auf **Löschen**.

### 2. Bestätigen

Ein Dialog erscheint mit der Meldung „Anwendung '[Name]' wirklich löschen?". Klicken Sie auf **Löschen** oder auf **Abbrechen**.

> **Hinweis:** Das Löschen kann nicht rückgängig gemacht werden.

---

## Anwendung per Drag & Drop verschieben

### 1. Anwendung ziehen

Klicken und halten Sie eine Anwendungszeile und ziehen Sie sie auf einen Gruppenheader oder in den Bereich „Ohne Gruppe". Das aktuelle Drop-Ziel wird mit einem gestrichelten blauen Rahmen hervorgehoben.

### 2. Loslassen

Lassen Sie die Maustaste über dem Zielbereich los.

> **Hinweis:** Es wird nur die Gruppenzugehörigkeit geändert — die Reihenfolge innerhalb der Gruppe bleibt unverändert. Drag & Drop funktioniert zuverlässig in Chromium-basierten Browsern (Chrome, Edge).

---

## Moduswechsel (Team / Benutzer)

Wenn Sie den Modus wechseln, werden alle geöffneten Dialoge und Formulare automatisch geschlossen und der Navigationsbaum mit den Daten des neuen Modus neu geladen. Der Detailbereich auf der rechten Seite wird ebenfalls ausgeblendet.
