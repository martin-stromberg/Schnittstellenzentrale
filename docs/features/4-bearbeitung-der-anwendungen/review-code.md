# Code-Review: UI-Korrekturen — Kontextmenü und Drag & Drop (Iteration 3)

**Status: Keine Befunde**

Reviewte Dateien: `app.css`, `ApplicationContextMenu.razor`, `ApplicationGroupContextMenu.razor`, `ApplicationGroupTree.razor`, `CollapsibleSection.razor`.

---

## Behobene Befunde aus Iteration 2

- ✅ `.context-menu-dropdown` hat `top: 100%; right: 0;`
- ✅ `_dropTargetIsUngrouped` wird in `OnDrop` (Erfolg- und Fehlerfall) zurückgesetzt
- ✅ `OnDragEnterUngrouped`/`OnDragLeaveUngrouped` verwenden Counter-Logik (`_dragEnterCountUngrouped`)
- ✅ Nullable-Warnung CS8602 in `OnDrop`-Catch-Block behoben (`if (_draggedApplication != null)`)

---

## Bewertung

**app.css:** Alle CSS-Regeln korrekt — `position: relative` für Container, `position: absolute; top: 100%; right: 0; z-index: 1000` für Dropdown, `opacity: 0`/Sichtbarkeits-Selektoren für Toggle, `menu-open`-Klasse, `.tree-leaf`-Flexbox, `.drag-over`-Highlight.

**ApplicationContextMenu / ApplicationGroupContextMenu:** Icon `⚙️`, `menu-open`-Klasse conditional, Overlay/Dropdown-Struktur korrekt. Menüaktionen setzen `_isOpen = false` vor dem EventCallback-Aufruf.

**ApplicationGroupTree:** `OnDragStart` ohne überflüssigen Parameter; Counter-Logik (`_dragEnterCount`, `_dragEnterCountUngrouped`) konsistent für Gruppen- und Ungrouped-Bereich; alle Drag-State-Felder werden in Erfolg- und Fehlerfall vollständig zurückgesetzt.

**CollapsibleSection:** Unverändert — `@ondrop`/`@ondragover:preventDefault` korrekt an `OnDrop.HasDelegate` gebunden.
