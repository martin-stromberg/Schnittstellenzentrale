# Offene Aufgaben

Erstellt am: 2026-05-17
Abbruchgrund: Maximale Iterationsanzahl erreicht

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

Keine — Plan vollständig umgesetzt.

## Code-Review-Befunde

1. **`TokenStoreTests.cs` — `ValidateAndRotateAsync_WithValidToken_ReturnsNewToken`** prüft zwei fachlich getrennte Szenarien in einem `[Fact]`: Token-Rotation und Ungültigkeit des alten Tokens. In zwei separate Tests aufteilen.

2. **`ApplicationGroupsControllerIntegrationTests.cs` — `PostApplicationGroup_RotatesToken_OldTokenIsInvalid`** prüft drei Szenarien in einem `[Fact]`: neues Token im Response-Header, altes Token ungültig, neues Token nutzbar. In drei separate Tests aufteilen.

Beide Befunde betreffen ausschließlich Testqualität (Trennung fachlicher Fälle pro `[Fact]`) und haben keinen Einfluss auf die Funktionalität.
