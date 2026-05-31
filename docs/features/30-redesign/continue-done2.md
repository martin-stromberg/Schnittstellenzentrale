# Offene Aufgaben

Erstellt am: 2026-05-31
Abbruchgrund: Kein Fortschritt zwischen den letzten zwei Iterationen

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

(keine — Plan vollständig umgesetzt)

## Code-Review-Befunde

- [x] **`EndpointExecutionService.cs:150` — Maskierte Variablenwerte im message-Feld ungeschützt**: `result.RequestDetails` (vollständig aufgelöste URL inkl. Variablen) wird ohne Maskierung als `message`-Parameter an `ActivityLogService.Log` übergeben. `BuildMaskedDetails` maskiert nur den `details`-Parameter. Maskierte Variablenwerte (z. B. `?token=secret123`) erscheinen im Protokoll-Eintragstitel im Klartext. Betrifft auch Zeilen 157 und 164 (HttpError- und Post-Script-Fehler-Einträge).
- [x] **`EndpointExecutionService.cs:137` — Irreführender EndpointExecuted-Eintrag bei Post-Script-Fehler**: Wenn HTTP 200 erfolgreich war, aber das Post-Script fehlschlägt, wird zuerst ein `EndpointExecuted`-Eintrag geloggt (`result.HttpSuccess == true`), dann ein `InternalError`. Der `EndpointExecuted`-Eintrag suggeriert dem Nutzer eine erfolgreiche Ausführung, obwohl der Gesamtaufruf fehlschlug.
- [x] **`EndpointScriptRunner.cs:210` — Sync-over-Async blockiert Thread-Pool** (architektonisch nicht behebbar ohne Jint-API-Änderung, im Code dokumentiert): `Task.Run(PersistVariableAsync).GetAwaiter().GetResult()` blockiert einen Thread-Pool-Thread innerhalb eines synchronen Jint-Callbacks. Bei vielen parallelen Endpunkt-Ausführungen droht Thread-Pool-Erschöpfung.
- [x] **`ActivityLogService.cs:43` — Unbegrenzte In-Memory-Liste**: `_entries` hat keine Größenbeschränkung. Bei langlebigen Blazor-Circuits mit vielen Endpunkt-Aufrufen wächst der Speicherverbrauch unbegrenzt bis zum Schließen der Verbindung.
- [x] **`EndpointScriptRunner.cs:48` — Pre/Post-Script nicht im Log unterschieden**: `ScriptExecuted`-Einträge enthalten keinen Hinweis auf den Script-Typ (Pre/Post). Wenn ein Endpunkt beide Script-Typen hat, ist im Protokoll nicht erkennbar, welches der beiden Skripte ausgeführt wurde.

## Rückmeldungen vom Kunden

- [x] Die Inhaltsseite für eine Systemumgebungmuss schöner werden. In der Datei stitch-environments.html ist ein Designentwurf dafür. Orientiere dich daran und pass die Seite an.
- [x] Die Seite der "Historie" soll angepasst werden. In der Datei stitch-history.html ist ein Designentwurd dafür. Orientiere dich daran und pass die Seite an.
