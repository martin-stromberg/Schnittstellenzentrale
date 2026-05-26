# Aktivitätsprotokoll — Ablauf für Anwender

## Voraussetzungen

Es sind keine besonderen Voraussetzungen erforderlich. Das Protokoll wird automatisch befüllt, sobald Aktionen in der Anwendung durchgeführt werden.

## Schritt-für-Schritt-Anleitung

### 1. Protokoll öffnen

Klicken Sie in der oberen Leiste auf die Schaltfläche mit dem Listen-Symbol (`≡`). Das Protokoll-Panel öffnet sich am unteren Bildschirmrand (Dock-Modus).

> **Hinweis:** Beim ersten Öffnen wird der zuletzt verwendete Anzeigemodus und die zuletzt eingestellte Höhe automatisch wiederhergestellt.

### 2. Einträge lesen

Die Einträge werden in umgekehrt chronologischer Reihenfolge angezeigt — der neueste Eintrag steht oben. Jeder Eintrag zeigt:

- **Uhrzeit** (Stunden:Minuten:Sekunden)
- **Symbol** für die Art der Aktion
- **Nachricht** (z. B. „GET https://api.example.com — 200" oder „Endpunkt angelegt: Login")

Einträge mit zusätzlichen Informationen (z. B. Request- und Response-Details bei HTTP-Anfragen oder Fehlermeldungen) haben einen „Details"-Bereich, der per Klick auf „Details" auf- und zugeklappt werden kann.

### 3. Anzeigemodus wechseln

Klicken Sie auf die Schaltfläche „Overlay" (wenn das Panel angedockt ist) oder „Angedockt" (wenn das Panel als Overlay angezeigt wird), um den Anzeigemodus zu wechseln.

- **Angedockt**: Das Panel schiebt den Inhaltsbereich nach oben, sodass nichts überlappt.
- **Overlay**: Das Panel schwebt über dem Inhalt — nützlich, wenn Sie den Inhaltsbereich gleichzeitig lesen möchten.

Der gewählte Modus wird gespeichert und beim nächsten Öffnen wiederverwendet.

### 4. Panelhöhe anpassen

Greifen Sie den Balken am oberen Rand des angedockten Panels und ziehen Sie ihn nach oben oder unten, um die Höhe anzupassen. Die Höhe wird automatisch gespeichert.

### 5. Protokoll leeren

Klicken Sie auf die Schaltfläche mit dem Papierkorb-Symbol im Protokoll-Panel. Alle aktuellen Einträge werden entfernt. Neue Aktionen werden weiterhin aufgezeichnet.

### 6. Protokoll schließen

Klicken Sie erneut auf das Listen-Symbol in der oberen Leiste, um das Protokoll-Panel zu schließen.

## Ergebnis

Das Protokoll zeigt alle Aktionen der aktuellen Sitzung: HTTP-Anfragen mit Status und Ergebnis, Skriptausführungen und deren Ausgaben, Anlage oder Umbenennung von Gruppen, Ordnern und Endpunkten sowie Wechsel der Systemumgebung oder des Speichermodus. Beim Schließen des Browsers oder der Verbindung werden alle Einträge verworfen.
