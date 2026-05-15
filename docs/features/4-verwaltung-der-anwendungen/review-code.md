# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### ApplicationGroupTree.razor (`OnModeChanged`)

- **Fehlerbehandlung** — Der `catch`-Block in `OnModeChanged` (Zeile 91–93) schluckt alle Exceptions stillschweigend: Er ruft nur `StateHasChanged()` auf, ohne den Fehler zu protokollieren oder dem Benutzer anzuzeigen. Ein Ladefehler bleibt damit unsichtbar, und die Anzeige kann veraltet sein, ohne dass der Benutzer davon erfährt.

  Empfehlung: Im `catch`-Block eine aussagekräftige Fehlermeldung in einer `_errorMessage`-Zustandsvariable speichern und diese im Template anzeigen — analog zum Muster in `ApplicationGroupEditor.razor` und `ApplicationEditor.razor`.

### ApplicationGroupTree.razor (Template)

- **Doppelter Code** — Das Markup zum Rendern eines Anwendungs-Buttons ist in den Zeilen 30–35 (gruppierte Anwendungen) und 43–48 (ungrupierte Anwendungen) wortgleich wiederholt:
  ```razor
  <div class="tree-leaf">
      <button class="btn btn-link" @onclick="() => SelectApplication(app.Id)">
          @app.Name
      </button>
  </div>
  ```
  Empfehlung: Das wiederholte Markup in ein separates Razor-Fragment (z. B. `RenderFragment<Application>`) oder eine eigene Komponente auslagern, um Änderungen nur an einer Stelle vornehmen zu müssen.

### ApplicationGroupTree.razor (`SelectApplication`)

- **Fehlende Kapselung / toter Durchleitungs-Wrapper** — Die Methode `SelectApplication` (Zeile 76–79) delegiert ausschließlich an `OnApplicationSelected.InvokeAsync(applicationId)` und enthält keine eigene Logik. Sie ist ein reiner Durchleitungs-Wrapper, der keinen Mehrwert bietet.

  Empfehlung: Den Event-Handler direkt im Template inline aufrufen (`@onclick="() => OnApplicationSelected.InvokeAsync(app.Id)"`) und die Methode entfernen, oder die Methode beibehalten und erklärend kommentieren, falls zukünftige Logik (z. B. Navigation) dort hinzukommen soll.

### ApplicationRepositoryIntegrationTests.cs (`ApplicationRepositoryIntegrationTests`)

- **Fehlende Testabdeckung** — Für die folgenden öffentlichen Methoden der Schnittstelle `IApplicationRepository` fehlen Tests vollständig: `UpdateApplicationAsync`, `DeleteApplicationAsync`, `UpdateGroupAsync`, `DeleteGroupAsync`, `GetGroupByIdAsync`. Fehler in diesen Methoden werden durch die vorhandene Testsuite nicht aufgedeckt.

  Empfehlung: Für jede der genannten Methoden mindestens einen Integrationstest ergänzen, der den Erfolgsfall (Happy Path) abdeckt, analog zu den bestehenden Tests.

## Geprüfte Dateien

- `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor`
- `src/Schnittstellenzentrale.Tests/Integration/ApplicationRepositoryIntegrationTests.cs`
