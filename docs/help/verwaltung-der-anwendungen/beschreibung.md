# Verwaltung der Anwendungen — Beschreibung

## Zweck

Anwendungen und Anwendungsgruppen können über die Benutzeroberfläche angelegt werden. Bisher war die linke Seitenleiste zwar vorhanden, jedoch mangels Erfassungsmasken stets leer. Die neuen Formulare füllen diese Lücke und erlauben es, die Struktur aus Gruppen und Anwendungen aufzubauen, ohne die Datenbank direkt bearbeiten zu müssen.

## Funktionsweise

In der `ApplicationGroupTree`-Seitenleiste befinden sich zwei neue Schaltflächen: **Neue Gruppe** und **Neue Anwendung**. Ein Klick auf eine der Schaltflächen blendet das zugehörige Inline-Formular direkt unterhalb der Schaltflächen ein; das jeweils andere Formular wird dabei geschlossen.

**Neue Gruppe** öffnet den `ApplicationGroupEditor`. Einziges Pflichtfeld ist der Name der Gruppe. Nach dem Speichern wird die Gruppenliste automatisch aktualisiert.

**Neue Anwendung** öffnet den `ApplicationEditor`. Pflichtfelder sind Name und Basis-URL. Optional können Beschreibung, Swagger-URL und Metadaten-URL eingetragen werden. Über ein Auswahlfeld kann die Anwendung einer vorhandenen Gruppe zugeordnet werden; die Option „Ohne Gruppe" ist ebenfalls wählbar.

Nach dem Speichern wird das Formular geschlossen, und die Seitenleiste lädt die aktuellen Daten neu. Über die Schaltfläche **Abbrechen** lässt sich das Formular jederzeit ohne Datenverlust schließen.

## Beispiele

- Eine neue Gruppe „Backend-Services" anlegen und anschließend eine Anwendung „Bestellservice" mit der Basis-URL `https://intern/bestellservice` dieser Gruppe zuordnen.
- Eine Anwendung ohne Gruppenauswahl anlegen — sie erscheint dann im Bereich „Ohne Gruppe" der Seitenleiste.

## Einschränkungen

- Diese Funktion deckt ausschließlich das **Anlegen** ab. Das Bearbeiten und Löschen von Gruppen und Anwendungen ist in dieser Version nicht enthalten.
- Die Gruppenauswahl im `ApplicationEditor` zeigt nur Gruppen, die zum Zeitpunkt des Öffnens des Formulars bereits vorhanden sind. Eine neu angelegte Gruppe erscheint erst nach dem nächsten Öffnen des Formulars.
