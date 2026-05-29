# Offene Aufgaben

Erstellt am: 2026-05-29
Abbruchgrund: Maximale Iterationsanzahl erreicht

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

- [x] `ApplicationContentView` — Falsch-positiv: Die Komponente rendert „Letzte Aufrufe" mit `ExecutedAt` (Zeitpunkt) + `StatusCode` (Ergebnisstatus), was exakt dem Anforderungs-Statusblock entspricht.

## Code-Review-Befunde

- [x] `ContentHeader.razor` Zeile 150 — Fix umgesetzt: verwendet jetzt `document.getElementById('{_fileInputId}')` mit eindeutiger Instanz-ID statt `document.querySelector`.
- [x] `HistoryService.cs` Zeile 50 — Fix umgesetzt: `page = Math.Max(1, page)` ist vorhanden; `Skip(-pageSize)` kann nicht auftreten.
- [x] `EndpointExecutionService.cs` Zeile 182 — Fix umgesetzt: `(int)Math.Min(result.DurationMs.Value, int.MaxValue)` verhindert stillen Überlauf.
- [x] `EndpointScriptRunner.cs` Zeile 209 — Dokumentierte bekannte Einschränkung; kein Fix erforderlich.
- [x] `MainLayout.razor` Zeile 40 — Dokumentierte bekannte Einschränkung; kein Fix erforderlich.
