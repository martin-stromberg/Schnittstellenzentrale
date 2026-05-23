# Offene Aufgaben

Erstellt am: 2026-05-23
Abbruchgrund: Kein Fortschritt zwischen den letzten zwei Iterationen (Iteration 1: 2 Befunde → Iteration 2: 2 neue Befunde)

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

Keine — Plan vollständig umgesetzt.

## Code-Review-Befunde

- [x] **Befund 1 (pre-existing):** `SystemEndpointSyncServiceTests` — zwei Tests schlagen fehl (`ExecuteAsync_WithoutNegotiateSecurityScheme_SetsNoneAuthenticationType`, `ExecuteAsync_WithNegotiateSecurityScheme_SetsNegotiateAuthenticationType`). Ursache: `DetectAuthenticationType` erkennt Security-Schemata nicht korrekt, wenn sie nur global im Dokument, nicht auf einzelnen Operationen gesetzt sind. Diese Failures bestanden bereits vor diesem Refactoring (eingeführt in Branch `11-endpunkte-und-endpunktgruppen-2`). Kein Handlungsbedarf im Rahmen dieses Refactorings.
- [x] **Befund 2 (Minor):** `ControllerTestFactory` verwendet `AddDbContext<AppDbContext>` statt `AddDbContextFactory<AppDbContext>`. Funktioniert technisch, ist aber inkonsistent mit dem restlichen Produktions- und Testcode. Empfehlung: auf `AddDbContextFactory` umstellen. **Erledigt:** auf `AddDbContextFactory` umgestellt.
