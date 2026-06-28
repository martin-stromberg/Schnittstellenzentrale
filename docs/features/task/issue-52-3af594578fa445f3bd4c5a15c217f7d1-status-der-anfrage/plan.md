# Umsetzungsplan - Status der Anfrage

## Zielbild

Beim Klick auf `Anfrage senden` in `EndpointPage` wird sofort ein sichtbarer Ausfuehrungszustand angezeigt. Der Zustand bleibt sichtbar, bis die Anfrage erfolgreich abgeschlossen ist, ein fachliches Fehlerergebnis angezeigt wird oder der Ablauf vorzeitig abbricht. Waehrend der Ausfuehrung wird der Senden-Button deaktiviert, um parallele Requests und Race Conditions beim Ergebnis-Cache zu vermeiden.

Die Umsetzung bleibt auf die UI-Schicht begrenzt. `EndpointExecutionService`, Ergebnisformat, Logging, History und Request-Aufbau werden nicht geaendert.

## Relevante Dateien

| Datei | Aufgabe |
|-------|---------|
| `src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor` | Running-State einfuehren, Statusanzeige rendern, Button deaktivieren, `SendRequestAsync` mit `try/finally` absichern |
| `src/Schnittstellenzentrale/wwwroot/app.css` | Styles fuer die laufende Anfrage ergaenzen |
| `src/Schnittstellenzentrale/Resources/SharedResources.resx` | Englischen Text fuer die Statusanzeige ergaenzen |
| `src/Schnittstellenzentrale/Resources/SharedResources.de.resx` | Deutschen Text fuer die Statusanzeige ergaenzen |
| `src/Schnittstellenzentrale.Tests/Components/EndpointPageTests.cs` | bUnit-Tests fuer Zwischenzustand, Abschluss und Fehlerpfad ergaenzen |
| `docs/help/endpunkte/ablauf-anwender.md` | Spaeter im Dokumentationsschritt: laufende Statusanzeige und deaktivierten Button beschreiben |
| `docs/help/endpunkte/ablauf-technisch.md` | Spaeter im Dokumentationsschritt: UI-Running-State im Ablauf von `SendRequestAsync` erwaehnen |

## Umsetzungsschritte

1. In `EndpointPage.razor` ein privates Feld `private bool _isExecuting;` neben `_result` einfuehren.
2. Den Senden-Button in der Adresszeile an den Running-State binden:
   - `disabled="@_isExecuting"` setzen.
   - Der sichtbare Buttontext kann unveraendert bleiben, damit nur ein neuer Status-Text lokalisiert werden muss.
3. Direkt unter der Adresszeile oder unmittelbar vor dem Response-Bereich eine Statusanzeige rendern, wenn `_isExecuting` true ist.
   - Empfohlene Klasse: `.sz-endpoint-execution-status`.
   - Text ueber neuen Resource-Key, z. B. `EndpointPage_Execution_Running`.
   - Die Anzeige soll auch sichtbar sein, wenn noch kein `_result` vorhanden ist.
4. `SendRequestAsync` gegen parallele Aufrufe schuetzen:
   - Am Methodenanfang `if (_isExecuting) return;`.
   - `_isExecuting = true;` unmittelbar nach dem Guard setzen, also vor Dirty-State-Speichern, Nachladen und `ExecuteAsync`.
   - Danach `await InvokeAsync(StateHasChanged);` ausfuehren, damit der Status vor laenger laufenden Awaits gerendert wird.
   - Den bisherigen Ablauf in `try` legen und in `finally` immer `_isExecuting = false;` setzen.
5. Fehler- und Abbruchpfade beibehalten:
   - Bei fehlgeschlagenem Speichern bleibt der bisherige Ruecksprung erhalten.
   - Bei `refreshed == null` wird weiterhin `_errorMessage = L["EndpointPage_Error_LoadFailed"]` gesetzt.
   - Fachliche Ausfuehrungsfehler bleiben `EndpointExecutionResult` mit `ErrorMessage` und werden im Response-Bereich angezeigt.
   - Exceptions, die aktuell aus `SendRequestAsync` herauslaufen koennen, sollen nicht in neue fachliche Ergebnisse umgewandelt werden; wichtig ist nur, dass `finally` die Statusanzeige beendet.
6. Keine Aenderung am `EndpointExecutionResultCache` vornehmen.
   - Der Running-State ist kein Ergebnis und darf nicht beim Endpunktwechsel wiederhergestellt werden.
7. Styles in `app.css` ergaenzen.
   - Die Anzeige soll in das bestehende EndpointPage-Layout passen, ohne den Response-Bereich als Ergebnis vorwegzunehmen.
   - Beispielhafte Ausgestaltung: dezenter Hinweis mit kleinem animierten Indikator, `display: flex`, `gap`, klare Kontrastwerte, kein Layout-Sprung durch stabile Hoehe.
8. Lokalisierung in beiden Resource-Dateien ergaenzen.
   - EN: sinngemaess "Request is running..."
   - DE: sinngemaess "Anfrage wird ausgefuehrt..."

## Teststrategie

### bUnit

Neue Tests in `EndpointPageTests.cs`:

1. `LaufendeAnfrage_ZeigtStatusUndDeaktiviertSendenButton`
   - `TaskCompletionSource<EndpointExecutionResult>` fuer `_executionMock.ExecuteAsync(...)` verwenden.
   - Nach Klick auf `button.sz-btn-send` unmittelbar pruefen:
     - `.sz-endpoint-execution-status` ist vorhanden.
     - Der Status-Text ist sichtbar.
     - `button.sz-btn-send` hat `disabled`.
   - Die Task noch nicht abschliessen, damit der Zwischenzustand stabil pruefbar ist.
2. `AbgeschlosseneAnfrage_BlendetStatusAusUndZeigtErgebnis`
   - Dieselbe verzogerte Task verwenden.
   - Nach Abschluss per `SetResult(...)` und Render-Warten pruefen:
     - Status-Markup ist verschwunden.
     - `.sz-endpoint-response` ist sichtbar.
     - Ergebnisinhalt oder Statuscode wird angezeigt.
     - Senden-Button ist wieder aktiv.
3. `FehlerhafteAnfrage_BlendetStatusAusUndZeigtFehler`
   - `ExecuteAsync` liefert ein `EndpointExecutionResult` mit `Success = false` und `ErrorMessage`.
   - Pruefen:
     - Status-Markup verschwindet.
     - Fehlertext wird im Response-Bereich angezeigt.
     - Senden-Button ist wieder aktiv.
4. Optionaler Guard-Test `MehrfachklickWaerendAusfuehrung_StartetKeineZweiteAnfrage`
   - Waehrend die erste Task offen ist, einen zweiten Klick ausloesen.
   - Verifizieren, dass `ExecuteAsync` nur einmal aufgerufen wurde.

### Bestehende Tests

Bestehende `EndpointPageTests` muessen weiterhin erfolgreich bleiben, insbesondere:

- Ergebnisanzeige nach erfolgreicher Anfrage.
- Ergebnis-Cache beim Endpunktwechsel.
- erneute Ausfuehrung nach abgeschlossener Anfrage ersetzt das gespeicherte Ergebnis.

Service-Tests und Integrationstests muessen nicht erweitert werden, weil kein Service-Vertrag geaendert wird.

### Auszufuehrende Kommandos

1. `dotnet test src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj --filter EndpointPageTests`
2. `dotnet test Schnittstellenzentrale.slnx`

Playwright-Tests sind fuer diese Aenderung nicht zwingend, weil der Zwischenzustand bei schnellen Endpunkten fluechtig ist. Ein Playwright-Test sollte nur ergaenzt werden, wenn ein kontrolliert langsamer Test-Endpunkt verfuegbar gemacht wird; sonst waere der Test voraussichtlich flaky.

## Risiken

- Wenn `_isExecuting` erst nach `SaveAsync` gesetzt wird, fehlt die unmittelbare Rueckmeldung bei automatischem Speichern. Deshalb muss das Flag vor dem ersten Await gesetzt werden.
- Ohne `try/finally` kann die Statusanzeige bei Ladefehlern, Speicherkonflikten oder Exceptions stehen bleiben.
- Ohne Button-Sperre koennen parallele Ausfuehrungen konkurrierende Ergebnisse in `_result` und `ExecutionResultCache` schreiben.
- `StateHasChanged` nach dem Setzen des Flags ist wichtig, weil Blazor sonst bei laengeren synchronen Vorarbeiten oder direkt folgenden Awaits den Zwischenzustand zu spaet rendern kann.
- Neue sichtbare Texte muessen in beiden Resource-Dateien stehen, sonst zeigen Tests mit FakeLocalizer zwar Keys, die echte UI aber unvollstaendige Lokalisierung.
- Die Statusanzeige darf nicht als gecachtes Ergebnis behandelt werden und darf beim Wechsel auf einen anderen Endpunkt nicht wieder erscheinen.

## Konservative Annahmen

- Betroffene Oberflaeche ist `EndpointPage`; die dortige Umsetzung gilt fuer alle Endpunkte, die ueber diese Komponente gesendet werden.
- Erneutes Ausloesen derselben Anfrage wird waehrend der laufenden Ausfuehrung verhindert, weil dies Race Conditions vermeidet und die Anforderung keine parallelen Ausfuehrungen verlangt.
- Es wird kein Fortschrittswert angezeigt, sondern nur ein laufender Zustand.
- Die fachliche Ausfuehrungslogik im Backend bleibt unveraendert.

## Offene Punkte

