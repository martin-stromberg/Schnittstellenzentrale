# Impressum — Installation und Konfiguration

## Voraussetzungen

Die Schnittstellenzentrale ist installiert. Für die Standardkonfiguration ist kein weiterer Einrichtungsschritt nötig — das Feature ist ab Version mit Issue #40 automatisch vorhanden und wartet nur auf die Markdown-Datei.

## Konfiguration

Der Konfigurationsabschnitt `Impressum` in `appsettings.json` steuert den Dateipfad:

```json
"Impressum": {
  "FilePath": ""
}
```

| Parameter | Typ | Standardwert | Beschreibung |
|-----------|-----|--------------|--------------|
| `Impressum:FilePath` | `string` | `""` (leer) | Pfad zur Impressum-Markdown-Datei. Leer = `AppContext.BaseDirectory/impressum.md`. Relative Pfade werden relativ zu `AppContext.BaseDirectory` aufgelöst. Absolute Pfade werden direkt verwendet. |

### Beispiele für `FilePath`

| Wert | Aufgelöster Pfad |
|------|-----------------|
| `""` (leer) | `<Programmverzeichnis>/impressum.md` |
| `"mein-impressum.md"` | `<Programmverzeichnis>/mein-impressum.md` |
| `"C:/Daten/impressum.md"` | `C:/Daten/impressum.md` |
| `"/opt/app/impressum.md"` | `/opt/app/impressum.md` |

## Installationsschritte

1. Öffnen Sie `appsettings.json` und passen Sie `Impressum:FilePath` an, falls die Datei nicht im Programmverzeichnis liegen soll. Bei Standardpfad ist dieser Schritt nicht erforderlich.
2. Legen Sie eine Datei `impressum.md` (oder den konfigurierten Namen) am festgelegten Ort ab.
3. Ein Neustart der Anwendung ist nicht erforderlich.

## Überprüfung

Öffnen Sie die Schnittstellenzentrale im Browser. Im Footer der linken Seitenleiste erscheint der Link **"Impressum"** / **"Imprint"**. Ein Klick auf den Link öffnet die Seite `/impressum` mit dem gerenderten Inhalt.

Falls der Link nicht erscheint: Prüfen Sie, ob die Datei am konfigurierten Pfad vorhanden ist und ob die Anwendung Lesezugriff auf die Datei hat.
