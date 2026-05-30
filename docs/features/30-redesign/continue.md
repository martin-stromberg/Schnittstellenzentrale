# Offene Aufgaben

Erstellt am: 2026-05-31
Abbruchgrund: Kein Fortschritt zwischen den letzten zwei Iterationen

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

(keine — Plan vollständig umgesetzt)

## Code-Review-Befunde

- [ ] **`EndpointExecutionService.cs:150` — Maskierte Variablenwerte im message-Feld ungeschützt**: `result.RequestDetails` (vollständig aufgelöste URL inkl. Variablen) wird ohne Maskierung als `message`-Parameter an `ActivityLogService.Log` übergeben. `BuildMaskedDetails` maskiert nur den `details`-Parameter. Maskierte Variablenwerte (z. B. `?token=secret123`) erscheinen im Protokoll-Eintragstitel im Klartext. Betrifft auch Zeilen 157 und 164 (HttpError- und Post-Script-Fehler-Einträge).
- [ ] **`EndpointExecutionService.cs:137` — Irreführender EndpointExecuted-Eintrag bei Post-Script-Fehler**: Wenn HTTP 200 erfolgreich war, aber das Post-Script fehlschlägt, wird zuerst ein `EndpointExecuted`-Eintrag geloggt (`result.HttpSuccess == true`), dann ein `InternalError`. Der `EndpointExecuted`-Eintrag suggeriert dem Nutzer eine erfolgreiche Ausführung, obwohl der Gesamtaufruf fehlschlug.
- [ ] **`EndpointScriptRunner.cs:210` — Sync-over-Async blockiert Thread-Pool** (architektonisch nicht behebbar ohne Jint-API-Änderung, im Code dokumentiert): `Task.Run(PersistVariableAsync).GetAwaiter().GetResult()` blockiert einen Thread-Pool-Thread innerhalb eines synchronen Jint-Callbacks. Bei vielen parallelen Endpunkt-Ausführungen droht Thread-Pool-Erschöpfung.
- [ ] **`ActivityLogService.cs:43` — Unbegrenzte In-Memory-Liste**: `_entries` hat keine Größenbeschränkung. Bei langlebigen Blazor-Circuits mit vielen Endpunkt-Aufrufen wächst der Speicherverbrauch unbegrenzt bis zum Schließen der Verbindung.
- [ ] **`EndpointScriptRunner.cs:48` — Pre/Post-Script nicht im Log unterschieden**: `ScriptExecuted`-Einträge enthalten keinen Hinweis auf den Script-Typ (Pre/Post). Wenn ein Endpunkt beide Script-Typen hat, ist im Protokoll nicht erkennbar, welches der beiden Skripte ausgeführt wurde.
