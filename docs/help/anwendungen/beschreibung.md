# Anwendungen — Beschreibung

## Zweck

Anwendungen und Anwendungsgruppen können über die Benutzeroberfläche angelegt, bearbeitet, verschoben und gelöscht werden. Die linke Seitenleiste zeigt den aktuellen Bestand als Navigationsbaum; alle Änderungen daran sind ohne direkten Datenbankzugriff möglich.

## Anlegen

In der `ApplicationGroupTree`-Seitenleiste befinden sich zwei Schaltflächen: **Neue Gruppe** und **Neue Anwendung**. Ein Klick blendet das zugehörige Inline-Formular direkt unterhalb der Schaltflächen ein; das jeweils andere Formular wird dabei geschlossen.

**Neue Gruppe** öffnet den `ApplicationGroupEditor`. Einziges Pflichtfeld ist der Name der Gruppe.

**Neue Anwendung** öffnet den `ApplicationEditor`. Pflichtfelder sind Name und Basis-URL. Optional können Beschreibung und Schnittstellen-URL eingetragen werden; aus der Schnittstellen-URL wird der Typ (REST/OData) automatisch erkannt und als Badge angezeigt. Über ein Auswahlfeld kann die Anwendung einer vorhandenen Gruppe zugeordnet werden; die Option „Ohne Gruppe" ist ebenfalls wählbar.

Nach dem Speichern wird das Formular geschlossen und die Seitenleiste aktualisiert. Über **Abbrechen** lässt sich das Formular jederzeit ohne Datenverlust schließen.

## Bearbeiten und Verwalten

Jede Gruppen- und Anwendungszeile im Navigationsbaum trägt ein Zahnrad-Symbol (⚙️). Das Symbol ist standardmäßig ausgeblendet und erscheint, sobald der Mauszeiger über die jeweilige Zeile bewegt wird oder der Tastaturfokus darauf liegt. Ein Klick darauf öffnet ein Dropdown-Menü mit den verfügbaren Aktionen.

**Gruppen** (`ApplicationGroupContextMenu`):
- **Umbenennen** — öffnet den modalen `RenameGroupDialog`, der den aktuellen Namen vorausfüllt. Nach dem Speichern wird der Baum aktualisiert.
- **Löschen** — öffnet `ConfirmDeleteGroupDialog` mit der Anzahl enthaltener Anwendungen und drei Optionen: Gruppe mit allen Anwendungen löschen, nur die Gruppe löschen (Anwendungen bleiben gruppenlos erhalten) oder abbrechen.

**Anwendungen** (`ApplicationContextMenu`):
- **Bearbeiten** — öffnet `ApplicationEditor` im Edit-Modus mit allen vorhandenen Felddaten vorausgefüllt. Dieselben Validierungsregeln wie beim Anlegen gelten.
- **Aus Gruppe entfernen** — nur sichtbar, wenn die Anwendung einer Gruppe zugeordnet ist. Setzt die Anwendung sofort auf gruppenlos, ohne einen Dialog zu öffnen.
- **Löschen** — öffnet `ConfirmDeleteApplicationDialog` zur Bestätigung.

**Drag & Drop**: Jede Anwendungszeile ist ziehbar. Gruppen-Header und der Bereich „Ohne Gruppe" sind Drop-Ziele. Beim Überfahren eines Drop-Ziels wird dieses mit einem gestrichelten blauen Rahmen hervorgehoben. Beim Ablegen wird die Gruppenzugehörigkeit der Anwendung angepasst.

## Moduswechsel

Wechselt der Anwender zwischen Team- und Benutzermodus, werden alle offenen Dialoge und Formulare automatisch geschlossen, der Detailbereich ausgeblendet und der Baum mit den Daten des neuen Modus neu geladen.

Im **Team-Modus** lösen alle persistierenden Operationen SignalR-Benachrichtigungen aus, damit andere verbundene Clients den aktuellen Stand sehen.

## Beispiele

- Eine neue Gruppe „Backend-Services" anlegen und eine Anwendung „Bestellservice" dieser Gruppe zuordnen.
- Eine Anwendung ohne Gruppenauswahl anlegen — sie erscheint im Bereich „Ohne Gruppe".
- Eine Gruppe, die mit einem Tippfehler im Namen angelegt wurde, über das Zahnrad-Menü direkt umbenennen.
- Eine Anwendung, die versehentlich der falschen Gruppe zugeordnet wurde, per Drag & Drop in die richtige Gruppe ziehen.
- Beim Löschen einer Gruppe mit fünf Anwendungen wählen, ob alle mitgelöscht oder in den gruppenlosen Bereich verschoben werden.

## Einschränkungen

- Die Gruppenauswahl im `ApplicationEditor` zeigt nur Gruppen, die beim Öffnen des Formulars bereits vorhanden sind.
- Die Reihenfolge von Anwendungen innerhalb einer Gruppe wird durch Drag & Drop nicht verändert — nur die Gruppenzugehörigkeit.
- Berechtigungen im Benutzermodus (darf ein Benutzer nur eigene oder alle sichtbaren Anwendungen bearbeiten?) sind noch nicht implementiert; alle sichtbaren Einträge zeigen das Zahnrad-Menü.
- Bei einem Nebenläufigkeitskonflikt (`RowVersion`) wird eine Fehlermeldung angezeigt; ein automatisches Zusammenführen findet nicht statt.
