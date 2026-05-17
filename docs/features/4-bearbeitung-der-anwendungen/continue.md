# Offene Aufgaben

Erstellt am: 2026-05-15
Abbruchgrund: Maximale Iterationsanzahl erreicht

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

Keine — Plan-Review ergibt „Vollständig umgesetzt".

## Code-Review-Befunde

1. ~~**`OnDrop` in ApplicationGroupTree.razor — Fehlerbehandlung unvollständig**
   ~~Der `throw` im inneren catch-Block wird vom Blazor-Event-Dispatcher still verschluckt,
   ~~weil kein äußerer try/catch vorhanden ist. Bei einem Drag-and-Drop-Fehler erhält der
   ~~Benutzer keine Fehlermeldung.
   ~~→ Den `throw` entfernen und stattdessen direkt `_errorMessage` setzen (analog zu den
   ~~anderen Handlern mit try/catch ohne rethrow). **Erledigt.**

2. ~~**`OnApplicationRemoved` / `OnSelectionCleared` in Home.razor — duplizierte Logik**
   ~~Beide Methoden sind identisch (`_selectedApplicationId = null`). Die Logik sollte in
   ~~eine gemeinsame private Methode ausgelagert werden. **Erledigt.**

3. ~~**`OnInitializedAsync` in ApplicationEditor.razor — fehlende Fehlerbehandlung**
   ~~Der Datenbankaufruf `GetGroupsAsync` ist nicht durch try/catch abgesichert. Eine
   ~~Datenbankausnahme beim Initialisieren des Editors bleibt für den Benutzer unsichtbar.
   ~~→ try/catch ergänzen und `_errorMessage` setzen. **Erledigt.**

4. ~~**Fehlende Testabdeckung: `GetUngroupedApplicationsAsync` mit `StorageMode.User`**~~
   ~~Es gibt keinen dedizierten Integrationstest, der `GetUngroupedApplicationsAsync` im~~
   ~~`StorageMode.User` prüft.~~
   ~~→ Test in `ApplicationRepositoryIntegrationTests` ergänzen.~~ **Erledigt.**
