# Schnittstellenzentrale — Ablauf für Anwender

## Voraussetzungen

- Die Anwendung ist im IIS veröffentlicht und über den Browser erreichbar.
- Der Anwender ist mit seinem Windows-Benutzerkonto am Rechner angemeldet.
- Eine Datenbank ist konfiguriert und die Anwendung wurde mindestens einmal gestartet.

---

## Speichermodus wählen

In der Kopfzeile der Anwendung befindet sich ein Auswahlfeld „Modus" mit den Optionen **Team** und **Benutzer**.

- **Team:** Alle Anwendungen und Endpunkte sind für alle Benutzer sichtbar und gemeinsam bearbeitbar.
- **Benutzer:** Es werden nur die eigenen Anwendungen angezeigt.

Der Modus kann jederzeit gewechselt werden; die Ansicht aktualisiert sich sofort.

---

## Anwendung aufrufen

### 1. Anwendung in der Seitenleiste auswählen

Die linke Seitenleiste zeigt alle Anwendungsgruppen als zugeklappte Abschnitte. Ein Klick auf einen Gruppennamen klappt die Gruppe auf und zeigt die enthaltenen Anwendungen. Nicht gruppierten Anwendungen erscheinen unter „Ohne Gruppe".

Ein Klick auf einen Anwendungsnamen öffnet die Detailansicht rechts.

### 2. Anwendungsdetails ansehen

Die Detailansicht zeigt Name, Beschreibung, Basis-URL sowie optional Swagger-URL und Metadata-URL der Anwendung. Darunter werden die Endpunkte der Anwendung, gruppiert in Abschnitte, angezeigt.

---

## Endpunkt ausführen

### 1. Endpunkt im Ausführungspanel aufrufen

Jeder Endpunkt wird mit seiner HTTP-Methode (farbiges Abzeichen), dem Namen und dem relativen Pfad angezeigt.

### 2. „Ausführen" klicken

Der Button **Ausführen** sendet den Request an den Endpunkt. Nach kurzer Zeit erscheinen unterhalb:
- **Request:** vollständige URL mit Methode
- **Status:** HTTP-Statuscode
- **Antwort:** Antworttext (scrollbares Feld)

Falls die Anwendung nicht erreichbar ist, erscheint ein **Fehler**-Hinweis und es öffnet sich automatisch der Health-Check-Dialog.

---

## Endpunkt anlegen oder bearbeiten

### 1. „Bearbeiten" klicken

Der Button **Bearbeiten** neben einem Endpunkt öffnet das Bearbeitungsformular direkt unterhalb des Endpunkteintrags.

### 2. Felder ausfüllen

| Feld | Beschreibung |
|------|-------------|
| Name | Frei wählbare Bezeichnung des Endpunkts |
| Methode | HTTP-Methode (GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS) |
| Relativer Pfad | Pfad relativ zur Basis-URL der Anwendung (z. B. `/api/orders`) |
| Authentifizierung | Authentifizierungstyp (None, Basic, Negotiate, BearerToken, NegotiateWithImpersonation) |
| Body | Anfrage-Body (z. B. JSON), nur relevant für POST/PUT/PATCH |
| Header | Beliebig viele Schlüssel-Wert-Paare für HTTP-Header |
| Query-Parameter | Beliebig viele Schlüssel-Wert-Paare für URL-Parameter |

Über **+ Header** bzw. **+ Parameter** werden neue Zeilen hinzugefügt; mit **✕** werden einzelne Einträge entfernt.

### 3. Speichern

Klick auf **Speichern** übernimmt die Änderungen.

> **Hinweis bei Schreibkonflikt:** Haben zwei Benutzer denselben Endpunkt gleichzeitig bearbeitet, erscheint der Dialog „Schreibkonflikt erkannt". Mit **Überschreiben (Force-Save)** werden die eigenen Änderungen erzwungen; mit **Abbrechen** wird die eigene Änderung verworfen.

---

## Endpunkte aus Swagger importieren

### Voraussetzung

Die Anwendung muss eine Swagger-URL konfiguriert haben. In diesem Fall ist der Button **Swagger-Import** in der Detailansicht sichtbar.

### 1. „Swagger-Import" klicken

Der Dialog **Swagger-Import-Vorschau** öffnet sich und zeigt die erkannten Unterschiede:
- **Neue Endpunkte** (grün): Endpunkte, die in der Swagger-Definition vorhanden sind, aber noch nicht in der Anwendung existieren.
- **Geänderte Endpunkte** (gelb): Endpunkte, deren Name sich geändert hat.
- **Entfernte Endpunkte** (rot): Endpunkte, die in der Swagger-Definition nicht mehr vorhanden sind.

### 2. Auswahl treffen

Alle Einträge sind standardmäßig ausgewählt (Checkbox). Nicht gewünschte Änderungen können abgewählt werden.

### 3. „Übernehmen" klicken

Die ausgewählten Änderungen werden in die Datenbank geschrieben. Der Dialog schließt sich.

---

## Endpunkte aus OData-Metadaten importieren

Funktioniert analog zum Swagger-Import, erfordert jedoch eine konfigurierte Metadata-URL. Der Button **OData-Import** erscheint in der Detailansicht.

---

## Health-Check ausführen

### 1. „Health-Check" klicken

Der Button **Health-Check** in der Anwendungsdetailansicht sendet eine Anfrage an die Swagger-URL, Metadata-URL oder Basis-URL der Anwendung.

### 2. Ergebnis ablesen

| Anzeige | Bedeutung |
|---------|-----------|
| „Die Anwendung ist erreichbar." (grün) | HTTP-Anfrage erfolgreich |
| „Die Anwendung ist nicht erreichbar." (rot) | HTTP-Anfrage fehlgeschlagen |
| „Health-Check wurde übersprungen (Cooldown aktiv)." (gelb) | Letzter Check liegt weniger als 60 Sekunden zurück |

### 3. Anwendung entfernen (optional)

Wenn die Anwendung nicht erreichbar ist, erscheint der Button **Anwendung entfernen**. Ein Klick löscht die Anwendung und alle zugehörigen Endpunkte unwiderruflich.

---

## Ergebnis

Nach Abschluss eines Imports oder einer Bearbeitung werden die Endpunkte der Anwendung automatisch aktualisiert. Im Team-Modus sehen alle anderen Benutzer, die dieselbe Anwendung geöffnet haben, die Änderungen in Echtzeit.
