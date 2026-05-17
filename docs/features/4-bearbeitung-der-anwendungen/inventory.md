# Bestandsaufnahme: UI-Korrekturen — Kontextmenü und Drag & Drop im ApplicationGroupTree

Analysiert wurden die Blazor-Komponenten `ApplicationContextMenu`, `ApplicationGroupContextMenu`, `ApplicationGroupTree` und `CollapsibleSection` sowie die globale CSS-Datei `app.css`, bezogen auf die Anforderung zur Korrektur von Kontextmenü-Darstellung, -Funktionalität, Zahnrad-Icon-Position und Sichtbarkeit sowie Drag-&-Drop-Verhalten.

## Zusammenfassung

**Razor-Komponenten (Ist-Zustand):**

- `ApplicationContextMenu` und `ApplicationGroupContextMenu` sind strukturell identisch aufgebaut: Zahnrad-Toggle-Button, unsichtbares Overlay zum Schließen, Dropdown-Panel mit Aktions-Buttons. Beide Komponenten sind vorhanden und vollständig implementiert.
- Das Zahnrad-Icon ist `⚙` (U+2699) ohne Emoji-Variation-Selector — keine Scoped-CSS-Datei für diese Komponenten vorhanden.
- Das Overlay (`context-menu-overlay`) hat `position: fixed; inset: 0; z-index: 999` — es liegt zwischen Benutzer und Dropdown. Das Dropdown-Panel (`.context-menu-dropdown`) hat **keine** CSS-Positionierung in `app.css`. Dadurch fängt das Overlay Klicks auf die Menü-Einträge ab, bevor deren `@onclick`-Handler ausgelöst wird.
- `ApplicationGroupTree` enthält vollständige Logik für Drag-&-Drop (Start, Drop, Gruppen-Wechsel), Kontextmenü-Callbacks (Bearbeiten, Löschen, Umbenennen, Aus Gruppe entfernen), SignalR-Benachrichtigung und Fehlerbehandlung.
- Drag-&-Drop-Start: `@ondragstart="() => OnDragStart(app)"` ohne `DragEventArgs` — kein `e.DataTransfer.SetData()` wird aufgerufen (Firefox-Kompatibilitätsproblem).
- Kein `ondragenter`/`ondragleave`-Handler in `ApplicationGroupTree` oder `CollapsibleSection` — kein visuelles Drag-over-Feedback, kein `_dropTargetGroupId`-Feld.
- `CollapsibleSection` leitet `ondrop` korrekt über `HandleDrop` weiter; `ondragover:preventDefault` ist conditionally gesetzt.
- Auf `.tree-leaf`-Divs ist `display: flex` und `align-items: center` **nicht** als CSS-Regel in `app.css` vorhanden — Inline-Ausrichtung von Anwendungsname und Zahnrad-Icon nicht sichergestellt.

**CSS (`app.css`):**

- `.context-menu-overlay`: vorhanden mit `position: fixed; inset: 0; z-index: 999` — verursacht den Overlay-Defekt (fängt Menü-Klicks ab).
- `.context-menu-dropdown`: **nicht vorhanden** — keine `position: absolute`, kein `z-index`, kein visueller Stil.
- `.context-menu-container`: **nicht vorhanden** — kein `position: relative`.
- `.context-menu-toggle`: **nicht vorhanden** — kein `opacity: 0` für Hover/Fokus-Sichtbarkeit.
- `.tree-leaf`: **nicht vorhanden** — kein `display: flex; align-items: center`.
- `.drag-over`: **nicht vorhanden** — kein visuelles Drop-Target-Feedback.
- Keine Scoped-CSS-Dateien (`*.razor.css`) für die betroffenen Shared-Komponenten.

**Tests:**

- Keine UI-Tests (Blazor-Komponenten-Tests) für die betroffenen Komponenten vorhanden.
- Integrationstests für `ApplicationRepository` vollständig, inkl. Update- und Delete-Szenarien sowie Concurrency-Tests.

## Details

- [Datenmodell](inventory/models.md)
- [Interfaces](inventory/interfaces.md)
- [Enums](inventory/enums.md)
- [Logik und Komponenten](inventory/logic.md)
- [Tests](inventory/tests.md)
