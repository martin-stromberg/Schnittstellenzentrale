# Impressum — Ablauf für Anwender

## Voraussetzungen

- Die Schnittstellenzentrale ist installiert und läuft.
- Der Betreiber hat Schreibzugriff auf das Programmverzeichnis (oder den konfigurierten Dateipfad).

## Impressum aktivieren

### 1. Markdown-Datei erstellen

Legen Sie eine Datei mit dem Namen `impressum.md` im Programmverzeichnis der Anwendung ab. Der Inhalt kann beliebiges Markdown enthalten — Überschriften, Absätze, Fettdruck, Listen usw.

> **Hinweis:** Wird ein anderer Speicherort gewünscht, kann der Pfad in der Konfigurationsdatei `appsettings.json` unter dem Schlüssel `Impressum:FilePath` angepasst werden. Siehe [Installation & Konfiguration](installation.md).

### 2. Ergebnis prüfen

Laden Sie die Seite der Schnittstellenzentrale neu. Im unteren Bereich der linken Seitenleiste erscheint jetzt der Link **"Impressum"** (DE) bzw. **"Imprint"** (EN). Ein Klick auf den Link öffnet die Seite `/impressum` mit dem gerenderten Inhalt Ihrer Datei.

## Impressum bearbeiten

Öffnen Sie die Datei `impressum.md` in einem Texteditor, nehmen Sie Ihre Änderungen vor und speichern Sie die Datei. Die Änderungen sind beim nächsten Seitenaufruf sofort sichtbar — ein Neustart der Anwendung ist nicht erforderlich.

## Impressum deaktivieren

Löschen oder verschieben Sie die Datei `impressum.md` aus dem Programmverzeichnis. Beim nächsten Seitenaufruf verschwindet der Link in der Seitenleiste automatisch. Ein direkter Aufruf von `/impressum` zeigt dann den Hinweis "Kein Impressum verfügbar." (DE) / "No imprint available." (EN).

## Ergebnis

Nach dem Ablegen der Datei:
- Der Link **"Impressum"** / **"Imprint"** erscheint im Footer der linken Seitenleiste.
- Die Seite `/impressum` zeigt die Überschrift und den formatierten Inhalt der Markdown-Datei.
