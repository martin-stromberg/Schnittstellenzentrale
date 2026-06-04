# Offene Aufgaben

Erstellt am: 2026-06-04
Abbruchgrund: Maximale Iterationsanzahl erreicht (3 Iterationen)

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

(keine — Plan-Review hat Status `Vollständig umgesetzt`)

## Code-Review-Befunde

- [ ] **[Mittel] Grammatisch gebrochene EN-Bestätigungsmeldungen** — `SharedResources.resx` Zeilen 275/287/300/330: `ConfirmDeleteGroupDialog_Message` = `"Really delete collection"` + `MessageSuffix` = `"really delete?"` ergibt zur Laufzeit *"Really delete collection XYZ really delete?"* — "really delete" doppelt. Gleicher Fehler in `ConfirmDeleteApplicationDialog` und `ConfirmDeleteEndpointGroupDialog`. Fix: Suffix auf `"?"` kürzen oder Prefix auf `"Delete collection"` (ohne "Really") ändern. DE-Seite korrekt.
- [ ] **[Niedrig] `UseRequestLocalization` nach Auth-Middleware registriert** — `Program.cs` Zeile 149: Microsoft empfiehlt Lokalisierungs-Middleware vor `UseAuthentication`/`UseAuthorization`. Auth-Challenge-Meldungen könnten trotz `de`-Browser immer auf Englisch antworten.
