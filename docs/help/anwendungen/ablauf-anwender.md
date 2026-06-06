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

## Endpunkte und Ordner in einer Anwendung verwalten

### Anwendung aufklappen

Klicken Sie auf den Pfeil (▶) links neben dem Anwendungsnamen **oder** direkt auf den Anwendungsnamen. Die zugehörigen Ordner und Endpunkte erscheinen eingerückt darunter. Ein zweiter Klick auf den Namen klappt die Anwendung wieder zu.

> **Hinweis:** Ein Klick auf den Anwendungsnamen wählt die Anwendung gleichzeitig aus und klappt sie auf bzw. zu.

### Ordner auf- und zuklappen

Endpunktordner sind beim Laden der Seite initial zugeklappt. Klicken Sie auf den Pfeil (▶) links neben dem Ordnernamen **oder** direkt auf den Ordnernamen, um ihn aufzuklappen. Ein zweiter Klick klappt ihn wieder zu.

> **Hinweis:** Nach einem Wechsel des Modus (Team/Benutzer) oder einem Datenneu-Laden sind alle Ordner wieder zugeklappt.

### Ordner anlegen

Fahren Sie mit der Maus auf eine Anwendungszeile und öffnen Sie das ⚙️-Menü. Wählen Sie **Ordner anlegen**. Der neue Ordner erscheint sofort mit dem Namen „Neuer Ordner" im Baum.

### Ordner umbenennen

Fahren Sie mit der Maus auf einen Ordner und öffnen Sie das ⚙️-Menü. Wählen Sie **Ordner umbenennen**. Ein Formular öffnet sich mit dem aktuellen Namen vorausgefüllt. Geben Sie den neuen Namen ein und klicken Sie auf **Speichern**.

> **Hinweis:** Der Name darf nicht leer sein.

### Ordner löschen

Fahren Sie mit der Maus auf einen Ordner und öffnen Sie das ⚙️-Menü. Wählen Sie **Ordner löschen**. Ein Dialog erscheint.

- Enthält der Ordner Endpunkte, weist der Dialog auf die kaskadierende Löschung hin: alle enthaltenen Endpunkte werden ebenfalls gelöscht.
- Klicken Sie auf **Löschen** oder **Abbrechen**.

> **Hinweis:** Das Löschen kann nicht rückgängig gemacht werden.

---

## Endpunkt anlegen

Es gibt zwei Wege:

- **Über eine Anwendung:** Fahren Sie auf die Anwendungszeile, öffnen Sie das ⚙️-Menü und wählen Sie **Endpunkt anlegen**. Der Endpunkt wird direkt auf Anwendungsebene (ohne Ordner) angelegt.
- **Über einen Ordner:** Fahren Sie auf den Ordner, öffnen Sie das ⚙️-Menü und wählen Sie **Endpunkt anlegen**. Der Endpunkt wird innerhalb des Ordners angelegt.

In beiden Fällen wird der neue Endpunkt mit dem Namen „Neuer Endpunkt" angelegt und sofort zur Bearbeitung geöffnet.

---

## Endpunkt bearbeiten

Klicken Sie auf einen Endpunkt-Knoten im Navigationsbaum. Die Bearbeitungsansicht öffnet sich im rechten Bereich.

### Name ändern

Klicken Sie in das Namensfeld im Kopfbereich und ändern Sie den Text. Ein Badge **geändert** erscheint, solange ungespeicherte Änderungen vorliegen.

### HTTP-Methode und Pfad festlegen

Wählen Sie die HTTP-Methode (GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS) aus dem Dropdown. Geben Sie im Textfeld rechts daneben den relativen Pfad ein (z. B. `/api/orders`).

### Authentifizierung konfigurieren (Register „Autorisierung")

Wählen Sie den Authentifizierungstyp:
- **None** — keine Authentifizierung.
- **Basic** — Benutzername und Passwort eingeben (Format: `benutzer:passwort`); wird sicher im Windows Credential Manager gespeichert.
- **Negotiate** — Windows-Authentifizierung; keine weiteren Felder erforderlich.
- **BearerToken** — Token eingeben; wird im Windows Credential Manager gespeichert.
- **NegotiateWithImpersonation** — Windows-Authentifizierung mit Impersonation; keine weiteren Felder erforderlich.

### Header konfigurieren (Register „Headers")

Klicken Sie auf **+ Header**, um einen neuen Eintrag hinzuzufügen. Geben Sie Name und Wert ein. Klicken Sie auf **✕**, um einen Header zu entfernen.

> **Hinweis:** Der `Content-Type`-Header wird automatisch gesetzt, wenn Sie im Register „Body" ein Body-Format auswählen. Er erscheint ausgegraut und kann manuell überschrieben werden.

### Query-Parameter konfigurieren (Register „Query-Parameter")

Klicken Sie auf **+ Parameter**, um einen neuen Eintrag hinzuzufügen.

### Body konfigurieren (Register „Body")

Wählen Sie das Body-Format aus:
- **None** — kein Body wird gesendet; das Textfeld ist deaktiviert.
- **Json** — JSON-Body; `Content-Type: application/json` wird automatisch gesetzt.
- **Xml** — XML-Body; `Content-Type: application/xml` wird automatisch gesetzt.
- **PlainText** — Nur-Text-Body; `Content-Type: text/plain` wird automatisch gesetzt.

Klicken Sie auf **Formatieren** (bei JSON oder XML), um den Body-Text automatisch einzurücken. Bei syntaktisch ungültigem JSON oder XML erscheint eine Fehlermeldung.

### Speichern

Klicken Sie auf **Speichern** oder drücken Sie **Strg+S**.

> **Hinweis:** Versuchen Sie, zu einem anderen Endpunkt zu wechseln oder die Seite zu verlassen, während ungespeicherte Änderungen vorliegen, erscheint eine Bestätigungsabfrage.

---

## Anfrage senden

Klicken Sie in der Adressleiste auf **Anfrage senden**.

- Falls noch ungespeicherte Änderungen vorliegen, werden diese zuerst automatisch gespeichert.
- Die Antwort erscheint unterhalb der Adressleiste mit Statuscode, Dauer und Größe.
- Register **Body** — formatierter oder roher Antwort-Body (Pretty/Raw umschalten; Formatauswahl JSON/XML).
- Register **Headers** — alle Antwort-Header als Tabelle.

> **Hinweis:** Bei einem Verbindungsfehler erscheint die Fehlermeldung im Antwortbereich.

---

## Endpunkt löschen

Fahren Sie mit der Maus auf einen Endpunkt-Knoten und öffnen Sie das ⚙️-Menü. Wählen Sie **Endpunkt löschen**. Ein Browser-Dialog erscheint zur Bestätigung.

> **Hinweis:** Das Löschen kann nicht rückgängig gemacht werden.

---

## Sidebar-Breite anpassen

Ziehen Sie den schmalen Trennstrich am rechten Rand der Seitenleiste nach links oder rechts, um die Breite anzupassen. Die eingestellte Breite wird im Browser gespeichert und beim nächsten Öffnen automatisch wiederhergestellt.

---

## Anwendungs-Detailansicht aufrufen

Klicken Sie auf den Namen einer Anwendung im Navigationsbaum. Die Detailkarte öffnet sich rechts und zeigt Name, Beschreibung, Basis-URL und Schnittstellen-URL der Anwendung.

Im Kopfbereich der Karte erscheinen typabhängige Schaltflächen:

- **REST-Anwendungen**: **Swagger-Import**, **Health Check**
- **OData-Anwendungen**: **OData-Import**, **Health Check**

Den OData-Import-Ablauf finden Sie in der Endpunkte-Dokumentation unter [OData-Endpunkte importieren](../endpunkte/ablauf-anwender.md#odata-endpunkte-importieren).

---

## Moduswechsel (Team / Benutzer)

Wenn Sie den Modus wechseln, werden alle geöffneten Dialoge und Formulare automatisch geschlossen und der Navigationsbaum mit den Daten des neuen Modus neu geladen. Der Detailbereich auf der rechten Seite wird ebenfalls ausgeblendet.

---

## Systemeinträge

Die Gruppe und die Anwendung **„Schnittstellenzentrale"** werden beim Start automatisch angelegt und repräsentieren die eigene REST-API der Schnittstellenzentrale. Diese Einträge sind systemseitig verwaltet und können nicht verändert oder gelöscht werden.

Im Zahnrad-Menü dieser Einträge sind die Schaltflächen **Umbenennen** / **Bearbeiten** und **Löschen** sichtbar, aber deaktiviert. Systemanwendungen lassen sich nicht per Drag & Drop verschieben.

> **Hinweis:** Normale Anwendungen können weiterhin per Drag & Drop in die Systemgruppe hinein- oder herausbewegt werden.
