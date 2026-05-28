# Offene Aufgaben

Erstellt am: 2026-05-28
Abbruchgrund: Kein Fortschritt zwischen den letzten zwei Iterationen

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

Keine — Plan ist vollständig umgesetzt.

## Code-Review-Befunde

- [x] **ActivityLogPanel.razor:86 — Resize-Handle nach Moduswechsel wirkungslos**: `ToggleDisplayMode` setzt `_needsResizeInit` nicht auf `true` zurück. Nach Dock↔Overlay-Wechsel bleibt der Resize-Handle wirkungslos, da `initializePanelResize` für das neue DOM-Element nie aufgerufen wird.
- [x] **activity-log-panel.js:34 — Event-Listener-Leak**: `mousemove`/`mouseup`-Listener werden bei jedem Panel-Öffnen an `document` angehängt, aber in `DisposeAsync` nicht entfernt — akkumulieren über Open/Close-Zyklen.
- [x] **MainLayout.razor:37 — `_activityLogPanelHeight` nach JS-Resize veraltet (Known Limitation)**: JS-Resize löst kein .NET-Callback aus; `padding-bottom` auf `<article>` stimmt nach Drag-Resize nicht mehr mit der tatsächlichen Panel-Höhe überein. Bereits als Kommentar dokumentiert.
- [x] **MainLayout.razor:239 — `catch (JSException)` zu eng in `DisposeAsync`**: Andere Laufzeitfehler aus `HubConnection.DisposeAsync()` (z. B. `ObjectDisposedException`) propagieren unkontrolliert. Empfehlung: auf `catch (Exception)` erweitern.
- [x] **ApplicationGroupTree.razor:257 — `SelectAndToggleApplication` soll nur beim Aufklappen auswählen**: Entscheidung des Anwenders: `SelectApplication` soll nur aufgerufen werden, wenn die Anwendung gerade aufgeklappt wird (war zugeklappt → wird aufgeklappt). Beim Zuklappen (war aufgeklappt → wird zugeklappt) soll keine Auswahl stattfinden.
- [x] **EndpointExecutionServiceTests.cs:1729 — Assertion für Negotiate-Client fehlt**: `factoryMock.Verify(f => f.CreateClient("negotiate"), Times.Once())` wurde entfernt; der Test verifiziert das Routing zum Negotiate-Handler nicht mehr.
- [x] **EndpointScriptRunner.cs:206 — `Task.Run(...).GetAwaiter().GetResult()` blockiert Thread-Pool (Known Limitation)**: Synchrones Blockieren in `ApplyEnvironmentSet` kann bei Last zu Thread-Pool-Erschöpfung führen. Bereits als Kommentar dokumentiert; vollständige Behebung erfordert Refactoring der Jint-Callback-Schnittstelle.
