# Impressum — Beschreibung

## Zweck

Die Impressum-Seite ermöglicht es Betreibern der Schnittstellenzentrale, ein Impressum bereitzustellen, ohne dafür eine neue Softwareversion einspielen zu müssen. Der Inhalt wird als einfache Markdown-Datei im Programmverzeichnis abgelegt und automatisch in HTML gerendert. Für Installationen ohne Impressumspflicht ist das Feature vollständig unsichtbar.

## Funktionsweise

Die Anwendung prüft bei jedem Seitenaufruf und bei jedem Aufbau der Sidebar, ob die konfigurierte Markdown-Datei existiert:

- **Datei vorhanden:** Der Footer-Link "Impressum" (DE) / "Imprint" (EN) erscheint im unteren Bereich der linken Sidebar. Die Seite `/impressum` zeigt den gerenderten Markdown-Inhalt unter der Seitenüberschrift.
- **Datei nicht vorhanden:** Der Footer-Link wird nicht gerendert. Die Seite `/impressum` ist über die Navigation nicht erreichbar; ein direkter URL-Aufruf zeigt den Hinweistext "Kein Impressum verfügbar." (DE) / "No imprint available." (EN).

Änderungen an der Markdown-Datei (Erstellen, Löschen, Inhalt bearbeiten) werden sofort beim nächsten Seitenaufruf wirksam — kein Neustart der Anwendung erforderlich.

## Beispiele

**Einfaches Impressum als `impressum.md`:**
```markdown
# Impressum

Muster GmbH  
Musterstraße 1  
12345 Musterstadt

Geschäftsführer: Max Mustermann  
E-Mail: kontakt@beispiel.de
```

Dieses Markdown wird serverseitig in HTML konvertiert und auf der Seite `/impressum` angezeigt. Der Footer-Link in der Sidebar ist nach dem Ablegen der Datei sofort sichtbar.

## Einschränkungen

- Das aus der Markdown-Datei erzeugte HTML wird ohne HTML-Sanitisierung als `MarkupString` gerendert. Der Inhalt der Datei liegt vollständig in der Verantwortung des Betreibers. Für Installationen, bei denen Dritte die Datei bearbeiten können, ist ein eigener Sanitisierungsschritt erforderlich.
- Es wird genau eine Impressum-Datei unterstützt. Mehrsprachige Impressa oder mehrere Dokumente sind nicht vorgesehen.
- Die Seite verfügt über keinen eigenen Bearbeitungsdialog; die Datei muss direkt im Dateisystem gepflegt werden.
