# Kundenanforderung analysieren und übersetzen

Du bekommst eine Kundenanforderung als Eingabe. Analysiere sie und übersetze sie in die technische Fachsprache des Projekts.

## Schritt 1: Anforderung verstehen

Lies die Anforderung sorgfältig durch. Identifiziere:

- **Fachlicher Kern:** Was soll das System tun? Was ist der Auslöser, was das Ergebnis?
- **Fachliche Beobachtungs-/Verarbeitungseinheit:** Welches fachliche Objekt oder Ereignisbündel wird beobachtet bzw. verarbeitet (z. B. „eine Bestellung", „ein Abo-Zeitraum", „eine Synchronisation")? Die Einheit muss so benannt sein, dass fachlich eindeutig ist, was zum Scope gehört und was nicht.
- **Auslösende Ereignisse:** Welche konkreten Ereignisse starten den Ablauf — Nutzeraktion, externer Trigger, Zeitpunkt, Systemereignis?
- **Erwartete Wirkung:** Was ist das für den Nutzer oder nachgelagerte Systeme sichtbare Ergebnis? „Intern verarbeitet" ohne sichtbare Wirkung ist keine vollständige Anforderung.
- **Fehlende fachliche Objekte:** Welche fachlichen Objekte, Konfigurationen oder Sitzungen setzt der Ablauf voraus, könnten aber zur Laufzeit noch nicht existieren? Für jedes ist zu klären, wie damit umzugehen ist: erzeugen, wiederverwenden, aktivieren oder verwerfen.
- **Scope-Ausschluss-Kandidaten:** Welche naheliegenden Nutzererwartungen werden von der Anforderung bewusst nicht erfüllt? Jede solche Einschränkung ist als expliziter Ausschluss zu notieren — stille Annahmen sind nicht zulässig.
- **Betroffene Objekte:** Welche Entitäten, Daten oder Abläufe sind betroffen?
- **Benutzerinteraktion:** Wird eine Aktion, ein automatischer Ablauf oder eine Anzeige verlangt?
- **Auswahl-/Identifikationsanforderungen:** Verlangt die Anforderung, dass Anwender einen Datensatz aus einer bestehenden Menge auswählen oder identifizieren (z. B. zuordnen, hinzufügen, referenzieren)? Solche Interaktionen sind in der übersetzten Anforderung explizit als eigener Fachpunkt zu benennen — sie gehen in wörtlichen Übersetzungen sonst unter und werden bei der Implementierung mit der technisch einfachsten Lösung (z. B. Eingabe interner Kennungen) „erfüllt".
- **Konfigurationsbedarf:** Muss das Verhalten konfigurierbar sein?

## Schritt 2: Projektkontext herstellen

Lies `docs/features.md` sowie alle relevanten Detaildateien unter `docs/features/`, um zu verstehen:

- Welche vergleichbaren Features bereits existieren
- Welche Namenskonventionen für Klassen, Methoden und Eigenschaften im Projekt verwendet werden
- Ob ein bestehender Mechanismus wiederverwendet werden kann

## Schritt 3: Übersetzte Anforderungsbeschreibung ausgeben

Gib eine strukturierte Beschreibung aus mit folgenden Abschnitten:

### Fachliche Zusammenfassung
Kurze Beschreibung (2–4 Sätze) in technischer Fachsprache: Was wird erweitert, welches Verhalten wird hinzugefügt?

### Fachlicher Scope
Verbindliche Abgrenzung vor der Umsetzung:
- **Verarbeitungseinheit:** {fachliches Objekt/Ereignisbündel, das beobachtet/verarbeitet wird}
- **Auslösende Ereignisse:** {konkrete Auslöser — Nutzeraktion, externer Trigger, Systemereignis}
- **Erwartete Wirkung:** {sichtbares Ergebnis für Nutzer oder nachgelagerte Systeme}
- **Umgang mit fehlenden Objekten:** {je erwartetem Objekt: erzeugen / wiederverwenden / aktivieren / verwerfen}
- **Scope-Ausschlüsse:** {Liste bewusster Einschränkungen gegenüber naheliegenden Nutzererwartungen — „Keine." wenn zutreffend}

### Betroffene Klassen und Komponenten
Liste der voraussichtlich betroffenen oder neu zu erstellenden Artefakte:
- Datenmodellklassen (neue Klassen oder neue Eigenschaften in bestehenden Klassen)
- Logikklassen / Services
- Interfaces
- Enums
- UI-Komponenten / Controller (falls zutreffend)
- Tests

### Implementierungsansatz
Kurze Beschreibung des technischen Vorgehens:
- Welche Events, Hooks oder Erweiterungspunkte sind relevant?
- Wird eine neue Klasse, eine Erweiterung einer bestehenden Klasse oder ein neues Interface benötigt?
- Gibt es Abhängigkeiten zu bestehenden Klassen oder Komponenten?

### Konfiguration
Falls das Feature konfigurierbar sein soll: Vorschlag für die Konfigurationsebene (z. B. Anwendungseinstellungen, benutzerspezifisch, pro Datensatz).

### Offene Fragen
Liste von Punkten, die vor der Implementierung mit dem Kunden oder intern geklärt werden müssen.

## Hinweise

- Technische Bezeichnungen (Klassen- und Methodennamen) immer im Original und in Backticks.
- Keine Implementierungsdetails erfinden — halte dich an das, was aus der Anforderung ableitbar ist, und kennzeichne Annahmen explizit als solche.
- Wenn die Anforderung unklar oder mehrdeutig ist, stelle gezielte Rückfragen, bevor du eine Übersetzung ausgibst.
