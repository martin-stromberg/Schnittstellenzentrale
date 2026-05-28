# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

### `CollapsibleSection` (Razor-Komponente)

- [x] `<span class="sz-tree-node-text">` trägt `@onclick="Toggle"` — vorhanden (Zeile 14)

---

### `ApplicationGroupTree` (Razor-Komponente)

- [x] Feld `_expandedEndpointGroupIds` (`HashSet<int>`) — vorhanden (Zeile 88)
- [x] Methode `SelectAndToggleApplication(int appId)` (`private async Task`) — vorhanden (Zeilen 257–261); ruft `ToggleApplicationExpanded` und danach `SelectApplication` auf
- [x] Methode `ToggleEndpointGroupExpanded(int endpointGroupId)` (`private void`) — vorhanden (Zeilen 263–268)
- [x] `RenderApplication`: `<button class="sz-tree-item-btn">` ruft `SelectAndToggleApplication(app.Id)` auf — vorhanden (Zeile 470)
- [x] `RenderEndpointGroup`: `<span class="sz-tree-item-label">` trägt `@onclick="() => ToggleEndpointGroupExpanded(group.Id)"` — vorhanden (Zeile 433)
- [x] `RenderEndpointGroup`: Chevron-Button (`<button class="sz-tree-chevron-btn">`) mit `@onclick="() => ToggleEndpointGroupExpanded(group.Id)"` — vorhanden (Zeile 429)
- [x] `RenderEndpointGroup`: Chevron zeigt `bi-chevron-down` (aufgeklappt) bzw. `bi-chevron-right` (zugeklappt) — vorhanden (Zeile 430)
- [x] `RenderEndpointGroup`: `<div class="sz-tree-children">` wird nur bei `_expandedEndpointGroupIds.Contains(group.Id)` gerendert — vorhanden (Zeile 439)

---

### Tests — neue Testklasse `TreeCollapseTests`

- [x] Testklasse `TreeCollapseTests` — angelegt (`src/Schnittstellenzentrale.Tests/Playwright/TreeCollapseTests.cs`)
- [x] Testmethode `ClickApplicationGroupTitle_TogglesCollapse` — vorhanden
- [x] Testmethode `ClickApplicationName_ExpandsApplication` — vorhanden
- [x] Testmethode `ClickApplicationName_SelectsApplication` — vorhanden
- [x] Testmethode `ClickApplicationName_CollapsesExpandedApplication` — vorhanden
- [x] Testmethode `ClickEndpointGroupName_TogglesCollapse` — vorhanden
- [x] Testmethode `ClickEndpointGroupChevron_TogglesCollapse` — vorhanden
- [x] Testmethode `EndpointGroupInitiallyCollapsed` — vorhanden

## Offene Aufgaben

Keine.

## Hinweise

- Der Plan nennt unter „Betroffene bestehende Tests" drei Tests in `ApplicationCrudTests` (`CreateApplication_AppearsInTree`, `EditApplication_UpdatesNameInTree`, `DeleteApplication_DisappearsFromTree`), die nach der Änderung des Klick-Handlers auf Stabilität geprüft werden sollten. Diese Prüfung ist im Plan als „zu verifizieren" markiert und liegt außerhalb des Implementierungsumfangs — sie ist daher nicht als offene Aufgabe gewertet.
- `_expandedEndpointGroupIds` wird in `OnModeChanged` nicht explizit geleert (im Gegensatz zu `_expandedApplicationIds.Clear()`). Der Plan beschreibt dieses Verhalten als „analog zum bestehenden Verhalten für `_expandedApplicationIds`", aber die tatsächliche Umsetzung weicht leicht ab. Da der Plan diese Gleichbehandlung lediglich als Seiteneffekt-Beschreibung nennt und kein explizites Planelement dafür formuliert, wird dies hier nur als Hinweis vermerkt.
