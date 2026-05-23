# Offene Aufgaben

Erstellt am: 2026-05-22
Abbruchgrund: Kein Fortschritt zwischen den letzten zwei Iterationen (3 Befunde in Iteration 1, 3 Befunde in Iteration 2)

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

Keine.

## Code-Review-Befunde

### SystemEndpointSyncServiceTests.cs

1. **Irreführende Testnamen mit Suffix `AndStarts`**: Die Tests `ExecuteAsync_WhenImportReturnsError_LogsErrorAndStarts` und `ExecuteAsync_WhenDbThrows_LogsErrorAndStarts` verwenden den Begriff „starts" als Suffix, der kein tatsächlich verifiziertes Verhalten beschreibt. Umbenennen in aussagekräftigere Namen (z. B. `_MakesNoRepositoryCalls`).

2. **Fehlende Logger-Verifikation in `WhenImportReturnsError`-Test**: Der Test behauptet im Namen `LogsError`, verzichtet aber auf jede Verifikation, dass tatsächlich ein Fehler geloggt wurde. Logger-Aufruf (`LogError`) im Test verifizieren.

### EndpointRepository.cs

3. **Fehlender Detach-Block in `UpdateEndpointAsync` und `UpdateEndpointGroupAsync`**: `ApplicationRepository` setzt vorhandene Tracking-Einträge vor dem `Update()`-Aufruf auf `Detached`. Dieses Muster fehlt in `UpdateEndpointAsync` und `UpdateEndpointGroupAsync` und kann zu Tracking-Konflikten im selben Scope führen. Detach-Block analog zu `ApplicationRepository` ergänzen.
