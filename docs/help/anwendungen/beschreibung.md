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

## Systemeinträge

Beim Start der Anwendung legt die Schnittstellenzentrale automatisch eine Gruppe und eine Anwendung mit dem Namen „Schnittstellenzentrale" an, die die eigene REST-API repräsentieren. Diese Einträge sind als Systemeinträge markiert (`IsSystem = true`) und werden vom `SystemEntryInitializer` verwaltet.

Systemeinträge können nicht über die Benutzeroberfläche oder die REST-API verändert oder gelöscht werden:

- Das Zahnrad-Menü zeigt die Schaltflächen **Umbenennen** und **Löschen** (Gruppe) bzw. **Bearbeiten** und **Löschen** (Anwendung) weiterhin an, jedoch deaktiviert.
- Systemanwendungen können nicht per Drag & Drop verschoben werden — das `draggable`-Attribut ist auf `false` gesetzt, und der `OnDragStart`-Handler bricht bei `IsSystem == true` ab.
- DELETE- und PUT-Anfragen an die REST-API für Systemeinträge werden mit `403 Forbidden` abgewiesen.

Normale Anwendungen dürfen per Drag & Drop in die Systemgruppe hinein- und wieder herausbewegt werden.

## Endpunktgruppen und Endpunkte

Jede Anwendung kann Endpunktgruppen (`EndpointGroup`) und einzelne Endpunkte (`Endpoint`) enthalten. Diese erscheinen nach dem Aufklappen des Anwendungsknotens im Navigationsbaum. Endpunkte ohne Gruppe werden direkt unterhalb der Anwendung aufgelistet; Endpunkte in einer Gruppe erscheinen unterhalb des Ordner-Knotens.

**Navigationsbaum-Icons:**
| Knotentyp | Icon |
|---|---|
| `ApplicationGroup` | `bi-collection` |
| `Application` | `bi-window` |
| `EndpointGroup` | `bi-folder` |
| `Endpoint` | `bi-lightning` |

**Endpunktgruppen verwalten** — Das Zahnrad-Menü eines Ordner-Knotens (`EndpointGroupContextMenu`) bietet:
- **Endpunkt anlegen** — legt einen neuen Endpunkt innerhalb des Ordners an.
- **Ordner umbenennen** — öffnet `RenameEndpointGroupDialog` mit dem aktuellen Namen.
- **Ordner löschen** — öffnet `ConfirmDeleteEndpointGroupDialog`; enthält der Ordner Endpunkte, wird auf die kaskadierende Löschung hingewiesen.

**Endpunkte verwalten** — Das Zahnrad-Menü eines Endpunkt-Knotens (`EndpointContextMenu`) bietet:
- **Endpunkt löschen** — löscht den Endpunkt nach Bestätigung über einen Browser-Dialog.

Das `ApplicationContextMenu` einer Anwendung wurde um zwei neue Einträge ergänzt:
- **Ordner anlegen** — legt eine neue `EndpointGroup` mit dem Namen „Neuer Ordner" an.
- **Endpunkt anlegen** — legt einen neuen Endpunkt direkt auf Anwendungsebene (ohne Gruppe) an.

## Endpunkt bearbeiten (`EndpointPage`)

Ein Klick auf einen Endpunkt-Knoten öffnet `EndpointPage` im rechten Bereich der Startseite. Die Seite bietet:

- **Kopfbereich** — inline editierbarer Name; Badge „geändert", solange ungespeicherte Änderungen vorliegen; Schaltfläche **Speichern**.
- **Adressleiste** — Dropdown für die HTTP-Methode (GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS), Eingabefeld für den relativen Pfad, Schaltfläche **Anfrage senden**.
- **Anfrage-Register:**
  - **Autorisierung** (`RequestAuthPanel`) — `AuthenticationType`-Auswahl; kontextsensitive Felder für Basic (Benutzername:Passwort) und BearerToken (Token-Feld); gespeichert im Windows Credential Manager.
  - **Headers** (`RequestHeadersPanel`) — editierbare Tabelle mit beliebig vielen Name/Wert-Paaren; der automatisch gesetzte `Content-Type`-Eintrag wird ausgegraut dargestellt.
  - **Query-Parameter** (`RequestQueryParamsPanel`) — editierbare Tabelle mit beliebig vielen Name/Wert-Paaren.
  - **Body** (`RequestBodyPanel`) — Freitextfeld; `BodyMode`-Auswahl (None, Json, Xml, PlainText); Schaltfläche **Formatieren** (für JSON und XML).
- **Antwort-Bereich** (erscheint nach einer Anfrage):
  - Statuszeile mit Statuscode, Anfragedauer (ms) und Antwortgröße (Bytes).
  - Register **Body** (`ResponseBodyPanel`) — Pretty/Raw-Umschalter; Formatauswahl (JSON/XML) im Pretty-Modus.
  - Register **Headers** (`ResponseHeadersPanel`) — schreibgeschützte Tabelle der Antwort-Header.

**Tastaturkürzel:** `Strg+S` löst das Speichern aus.

**Ungespeicherte Änderungen:** Versucht der Anwender, zu einem anderen Endpunkt zu navigieren oder die Seite zu verlassen, erscheint eine Bestätigungsabfrage.

## Sidebar-Resize

Die Breite der linken Seitenleiste ist per Drag am rechten Rand frei anpassbar. Die eingestellte Breite wird im `localStorage` des Browsers gespeichert und bei der nächsten Sitzung automatisch wiederhergestellt.

## Einschränkungen

- Die Gruppenauswahl im `ApplicationEditor` zeigt nur Gruppen, die beim Öffnen des Formulars bereits vorhanden sind.
- Die Reihenfolge von Anwendungen innerhalb einer Gruppe wird durch Drag & Drop nicht verändert — nur die Gruppenzugehörigkeit.
- Berechtigungen im Benutzermodus (darf ein Benutzer nur eigene oder alle sichtbaren Anwendungen bearbeiten?) sind noch nicht implementiert; alle sichtbaren Einträge zeigen das Zahnrad-Menü.
- Bei einem Nebenläufigkeitskonflikt (`RowVersion`) auf einem Endpunkt erscheint der `ConcurrencyWarningDialog`; „Änderungen trotzdem speichern" lädt die aktuelle `RowVersion` nach und speichert erneut.
- Endpunkte können nicht zwischen Anwendungen oder Ordnern per Drag & Drop verschoben werden.
- Das Formatieren im `RequestBodyPanel` prüft nur die syntaktische Korrektheit (JSON/XML) — semantische Validierung findet nicht statt.
- Die Sidebar-Breite wird nur client-seitig im `localStorage` gespeichert; auf einem anderen Gerät oder Browser wird die Standardbreite verwendet.
