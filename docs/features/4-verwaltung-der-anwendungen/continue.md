# Offene Aufgaben

Erstellt am: 2026-05-14
Abbruchgrund: Maximale Iterationsanzahl erreicht und kein weiterer Fortschritt zwischen Iteration 2 und 3 (Befunde: 2 → 4)

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

Keine — Plan-Review hat den Status „Vollständig umgesetzt".

## Code-Review-Befunde

### ApplicationGroupTree.razor (`OnModeChanged`)

- **Fehlerbehandlung** — Der `catch`-Block in `OnModeChanged` (Zeile 91–93) schluckt alle Exceptions stillschweigend: Er ruft nur `StateHasChanged()` auf, ohne den Fehler zu protokollieren oder dem Benutzer anzuzeigen. Ein Ladefehler bleibt damit unsichtbar, und die Anzeige kann veraltet sein, ohne dass der Benutzer davon erfährt.

  Empfehlung: Im `catch`-Block eine aussagekräftige Fehlermeldung in einer `_errorMessage`-Zustandsvariable speichern und diese im Template anzeigen — analog zum Muster in `ApplicationGroupEditor.razor` und `ApplicationEditor.razor`.

### ApplicationGroupTree.razor (Template)

- **Doppelter Code** — Das Markup zum Rendern eines Anwendungs-Buttons ist in den Zeilen 30–35 (gruppierte Anwendungen) und 43–48 (ungrupierte Anwendungen) wortgleich wiederholt. Empfehlung: Das wiederholte Markup in ein separates Razor-Fragment oder eine eigene Komponente auslagern.

### ApplicationGroupTree.razor (`SelectApplication`)

- **Toter Durchleitungs-Wrapper** — Die Methode `SelectApplication` delegiert ausschließlich an `OnApplicationSelected.InvokeAsync(applicationId)` ohne eigene Logik. Empfehlung: Den Event-Handler direkt im Template inline aufrufen und die Methode entfernen, oder die Methode mit einem erklärenden Kommentar versehen.

### ApplicationRepositoryIntegrationTests.cs

- **Fehlende Testabdeckung** — Für die folgenden öffentlichen Methoden von `IApplicationRepository` fehlen Tests vollständig: `UpdateApplicationAsync`, `DeleteApplicationAsync`, `UpdateGroupAsync`, `DeleteGroupAsync`, `GetGroupByIdAsync`. Empfehlung: Für jede Methode mindestens einen Happy-Path-Integrationstest ergänzen.
