# Offene Aufgaben

Erstellt am: 2026-05-28
Abbruchgrund: Kein Fortschritt zwischen den letzten zwei Iterationen

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

Keine — Plan ist vollständig umgesetzt.

## Code-Review-Befunde

- [ ] **ActivityLogPanel.razor:86 — Resize-Handle nach Moduswechsel wirkungslos**: `ToggleDisplayMode` setzt `_needsResizeInit` nicht auf `true` zurück. Nach Dock↔Overlay-Wechsel bleibt der Resize-Handle wirkungslos, da `initializePanelResize` für das neue DOM-Element nie aufgerufen wird.
- [ ] **activity-log-panel.js:34 — Event-Listener-Leak**: `mousemove`/`mouseup`-Listener werden bei jedem Panel-Öffnen an `document` angehängt, aber in `DisposeAsync` nicht entfernt — akkumulieren über Open/Close-Zyklen.
- [ ] **MainLayout.razor:37 — `_activityLogPanelHeight` nach JS-Resize veraltet (Known Limitation)**: JS-Resize löst kein .NET-Callback aus; `padding-bottom` auf `<article>` stimmt nach Drag-Resize nicht mehr mit der tatsächlichen Panel-Höhe überein. Bereits als Kommentar dokumentiert.
- [ ] **MainLayout.razor:239 — `catch (JSException)` zu eng in `DisposeAsync`**: Andere Laufzeitfehler aus `HubConnection.DisposeAsync()` (z. B. `ObjectDisposedException`) propagieren unkontrolliert. Empfehlung: auf `catch (Exception)` erweitern.
- [ ] **ApplicationGroupTree.razor:257 — `SelectAndToggleApplication` wählt beim Zuklappen immer aus**: Klick auf Anwendungsname wählt die Anwendung auch beim Zuklappen aus. Dies war eine explizite Designentscheidung; der Reviewer sieht darin ein potenzielles UX-Problem (unbeabsichtigte Auswahl beim Aufräumen des Baums). Zur Klärung mit dem Anwender.
- [ ] **EndpointExecutionServiceTests.cs:1729 — Assertion für Negotiate-Client fehlt**: `factoryMock.Verify(f => f.CreateClient("negotiate"), Times.Once())` wurde entfernt; der Test verifiziert das Routing zum Negotiate-Handler nicht mehr.
- [ ] **EndpointScriptRunner.cs:206 — `Task.Run(...).GetAwaiter().GetResult()` blockiert Thread-Pool (Known Limitation)**: Synchrones Blockieren in `ApplyEnvironmentSet` kann bei Last zu Thread-Pool-Erschöpfung führen. Bereits als Kommentar dokumentiert; vollständige Behebung erfordert Refactoring der Jint-Callback-Schnittstelle.
