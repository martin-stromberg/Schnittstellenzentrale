# Schnittstellenzentrale — Ablauf für Anwender

## Voraussetzungen

- Die Anwendung ist im IIS veröffentlicht und über den Browser erreichbar.
- Der Anwender ist mit seinem Windows-Benutzerkonto am Rechner angemeldet.
- Eine Datenbank ist konfiguriert und die Anwendung wurde mindestens einmal gestartet.

---

## Bereich wechseln

Die Titelleiste (**TopBar**) enthält drei Tabs: **Workspaces**, **Environments** und **History**. Ein Klick auf einen Tab wechselt den angezeigten Bereich sofort — ohne Neuladen der Seite.

---

## Speichermodus wählen

In der Titelleiste befindet sich ein Auswahlfeld **Modus** mit den Optionen **Team** und **Benutzer**.

- **Team:** Alle Anwendungen und Endpunkte sind für alle Benutzer sichtbar und gemeinsam bearbeitbar.
- **Benutzer:** Es werden nur die eigenen Anwendungen und Umgebungen angezeigt.

Der Modus kann jederzeit gewechselt werden; die Ansicht aktualisiert sich sofort.

---

## Anwendung aufrufen (Bereich Workspaces)

### 1. Anwendung in der Seitenleiste auswählen

Die linke Seitenleiste zeigt alle Sammlungen als zugeklappte Abschnitte. Ein Klick auf einen Sammlungsnamen klappt die Gruppe auf und zeigt die enthaltenen Anwendungen. Nicht gruppierten Anwendungen erscheinen unter „Ohne Gruppe".

Ein Klick auf einen Anwendungsnamen öffnet die Inhaltsansicht der Anwendung rechts.

### 2. Breadcrumb nutzen

Über dem Inhaltsbereich zeigt die Breadcrumb-Leiste den aktuellen Navigationspfad, z. B. **MeineSammlung / MeineAnwendung / MeinOrdner**. Ein Klick auf ein übergeordnetes Element in der Breadcrumb-Leiste navigiert direkt dorthin zurück.

### 3. Anwendungsdetails ansehen

Die Inhaltsansicht zeigt:
- Kopfbereich mit Name, optionalem Untertitel und Icon
- Block **Beschreibung**
- Block **URLs** (Basis-URL, optional Swagger/OData-URL)
- Block **Letzte Aufrufe** (die jüngsten 5 Einträge aus der Aufrufhistorie)
- Block **Links** (verwaltete URL-Links zur Anwendung)
- Block **Top-5-Endpunkte** (die meistaufgerufenen Endpunkte)

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

---

## Name oder Untertitel bearbeiten (In-place-Editing)

In der Inhaltsansicht einer Sammlung oder Anwendung kann der Name bzw. Untertitel direkt bearbeitet werden:

1. Auf den Namen oder Untertitel klicken — ein Eingabefeld erscheint.
2. Text eingeben. Ein leerer Name wird nicht akzeptiert (Fehlermeldung erscheint inline).
3. Mit **Enter** oder Klick außerhalb speichern; mit **Escape** abbrechen.

---

## Icon hochladen

Im Kopfbereich einer Sammlung oder Anwendung befindet sich ein Upload-Symbol (↑):

1. Auf das Upload-Symbol klicken.
2. Eine Datei im Format **PNG** oder **JPEG** auswählen (max. 512 KB).
3. Das Icon erscheint sofort im Kopfbereich. Bei ungültigem Format oder zu großer Datei erscheint eine Fehlermeldung; kein Upload wird durchgeführt.

---

## Links einer Anwendung verwalten

Im Block **Links** der Anwendungsansicht:

- **Hinzufügen:** „+ Link hinzufügen" klicken, URL (Pflichtfeld, muss mit `http://` oder `https://` beginnen) und optionale Beschriftung (max. 200 Zeichen) eingeben, **Speichern** klicken.
- **Bearbeiten:** Stift-Icon (✏) neben dem Link anklicken, Werte ändern, **Speichern** klicken.
- **Löschen:** Papierkorb-Icon (🗑) neben dem Link anklicken.

---

## Aufrufhistorie einsehen (Bereich History)

1. Tab **History** in der Titelleiste anklicken.
2. Die Liste zeigt alle vergangenen Endpunktaufrufe mit Zeitpunkt, Methode, Pfad, Statuscode und Dauer in Millisekunden.
3. Optionaler Filter: Zeitraum über die beiden Datumsfelder „Von" und „Bis" einschränken, dann **Filtern** klicken.
4. Über die Pfeiltasten (← →) kann zwischen Seiten navigiert werden.
5. **Zurücksetzen** entfernt den Zeitraumfilter und zeigt wieder alle Einträge.

---

## Systemumgebungen verwalten (Bereich Environments)

1. Tab **Environments** in der Titelleiste anklicken.
2. Die Seitenleiste zeigt alle Umgebungen. Ein Klick auf einen Umgebungsnamen öffnet die Inhaltsansicht.
3. In der Inhaltsansicht können **Name** (Pflichtfeld, max. 200 Zeichen) und **Beschreibung** (optional) direkt durch Anklicken des Texts bearbeitet werden; Speichern bei Blur oder Enter.
4. Darunter befindet sich die Variablentabelle zum Anlegen, Bearbeiten und Löschen von Umgebungsvariablen.
5. **Neue Umgebung anlegen:** „+ Neue Umgebung"-Button klicken, Name eingeben und **Anlegen** bestätigen.
6. **Umgebung löschen:** Papierkorb-Icon (🗑) neben dem Eintrag in der Seitenleiste anklicken.

---

## Ergebnis

Nach Abschluss eines Imports oder einer Bearbeitung werden die Endpunkte der Anwendung automatisch aktualisiert. Im Team-Modus sehen alle anderen Benutzer, die dieselbe Anwendung geöffnet haben, die Änderungen in Echtzeit.
