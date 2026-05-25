# Offene Aufgaben

Erstellt am: 2026-05-25
Abbruchgrund: Maximale Iterationsanzahl erreicht

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

Keine.

## Code-Review-Befunde

- [ ] **ScriptContext.cs — Kopplung**: Zwei Infrastruktur-Services (`ISystemEnvironmentRepository`, `ISignalRNotificationService`) sind im DTO enthalten, werden aber nur vom Runner gebraucht. Empfehlung: per Konstruktor-Injektion in `EndpointScriptRunner` einbringen.
- [ ] **EndpointScriptRunner.cs — `BuildEnvironmentObject`**: Enthält ein ~40-zeiliges Lambda mit vollständiger Domänenlogik. Empfehlung: In eine separate Methode `ApplyEnvironmentSet` auslagern.
- [ ] **ScriptRequestData.cs / ScriptResponseData.cs — Gemeinsame Hilfsmethoden**: `ConvertJsonElement` und `ConvertXmlToObject` sind als `internal static` auf `ScriptRequestData` definiert, werden aber von `ScriptResponseData` genutzt. Empfehlung: in eine eigene Hilfsklasse verschieben.
- [ ] **EndpointExecutionService.cs — Platzhalter in BuildScriptContext**: `BuildScriptContext` baut `ScriptRequestData.Url` ohne Platzhalter-Auflösung, `BuildRequest` löst Platzhalter dagegen auf. Das Skript erhält eine andere URL als der HTTP-Request. Empfehlung: `ResolvePlaceholders` auch in `BuildScriptContext` anwenden.
- [ ] **Tests — Qualitätsprobleme**: Implizite `method`-Überschreibung in `CreateEndpoint` wenn `body != null`, und vier doppelte Mock-Hilfsmethoden die sich nur im Rückgabetyp unterscheiden.
