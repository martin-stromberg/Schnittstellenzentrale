# Offene Aufgaben

Erstellt am: 2026-05-27
Abbruchgrund: Kein Fortschritt zwischen den letzten zwei Iterationen

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

Keine — Plan ist vollständig umgesetzt.

## Code-Review-Befunde

- [x] **ActivityLogPanel.razor — Fehlerbehandlung**: Der `initializePanelResize`-Aufruf in `OnAfterRenderAsync` liegt außerhalb des try-catch-Blocks, der die `JSException` bei Server-Prerendering abfängt. Eine Exception dort würde unkontrolliert propagieren.
- [x] **ActivityLogPanel.razor — Kopplung/Erweiterbarkeit**: `OnPanelHeightChanged` wird nur beim ersten Render ausgelöst. Nach einem JS-gesteuerten Resize bleibt `MainLayout._activityLogPanelHeight` veraltet, da der JS-Code nur localStorage aktualisiert, aber kein .NET-Callback auslöst.
- [x] **ActivityLogService.cs — Fehlerbehandlung**: Der `catch (Exception)`-Block in `Log` verschluckt alle Ausnahmen einschließlich schwerwiegender Laufzeitfehler ohne jedes Logging.
- [x] **EndpointExecutionServiceTests.cs — Doppelter Code**: Der Test `SzExecute_RekursionsschutzGreiftBeimDrittenAufruf` dupliziert ~20 Zeilen Setup-Code für HTTP-Infrastruktur, anstatt die vorhandene `CreateService`-Hilfsmethode zu nutzen.
