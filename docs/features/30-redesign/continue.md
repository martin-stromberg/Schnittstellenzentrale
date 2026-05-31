# Offene Aufgaben

Erstellt am: 2026-05-31
Abbruchgrund: Maximale Iterationsanzahl erreicht

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

(keine — Plan vollständig umgesetzt)

## Code-Review-Befunde

- [ ] **`HistoryContentView.razor:141` — DateTime-Timezone-Regression**: Filterfelder werden mit `DateTimeKind.Unspecified` an `GetPagedAsync` übergeben, während `ExecutedAt` als UTC gespeichert ist. Auf Servern außerhalb UTC ist das Filterfenster um den UTC-Offset verschoben. Die zuvor entfernte `SpecifyKind + ToUniversalTime`-Konvertierung war korrekt — sie muss wiederhergestellt oder durch eine äquivalente Lösung ersetzt werden.
- [ ] **`HistoryContentView.razor:141` — Redundante Ternary-Ausdrücke**: `_filterFrom.HasValue ? _filterFrom.Value : (DateTime?)null` ist äquivalent zu `_filterFrom`. Überbleibsel der entfernten `SpecifyKind`-Konvertierung — vereinfachen zu `var filterFrom = _filterFrom; var filterTo = _filterTo;`.
- [ ] **`HistoryService.cs:66` — GroupJoin nach Take erzwingt client-seitige Auswertung auf SQL Server**: EF Core kann `GroupBy+Take+GroupJoin` über zwei Tabellen nicht als einzelnes SQL übersetzen — `Take(count)` wirkt auf SQL Server nicht als DB-seitiges LIMIT. Lösung: Query umstrukturieren (z. B. Raw SQL oder zwei getrennte Abfragen).
- [ ] **`EndpointScriptRunner.cs:214` — Sync-over-Async blockiert Thread-Pool** (architektonisch nicht behebbar ohne Jint-API-Änderung, im Code dokumentiert): `Task.Run(PersistVariableAsync).GetAwaiter().GetResult()` blockiert einen Thread-Pool-Thread innerhalb eines synchronen Jint-Callbacks. Bei vielen parallelen Endpunkt-Ausführungen droht Thread-Pool-Erschöpfung.
- [ ] **`EndpointExecutionService.cs:113` — Stack-Trace im Activity-Log**: `ex.ToString()` schreibt vollständige Stack-Traces inkl. interner Netzwerkdetails in den sessionweit sichtbaren Activity-Log. Stattdessen nur `ex.Message` verwenden.
