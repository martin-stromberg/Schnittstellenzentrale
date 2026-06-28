# Bestandsaufnahme - Status der Anfrage

## Kurzfazit

Die betroffene Oberflaeche ist die Blazor-Komponente `EndpointPage`. Sie startet Endpunkt-Anfragen ueber den Button `Anfrage senden`, wartet synchron auf `IEndpointExecutionService.ExecuteAsync(...)` und rendert erst nach Rueckkehr des Ergebnisses den Antwortbereich. Ein eigener UI-Zustand fuer "Anfrage laeuft" existiert nicht.

Detaildokumente:

- [Komponenten- und Ablaufdetails](inventory/endpoint-page.md)
- [Tests, Styles und Dokumentation](inventory/tests-styles-docs.md)

## Relevante Dateien

| Bereich | Datei | Relevanz |
|---------|-------|----------|
| Blazor-Komponente | `src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor` | Zentrale Endpunkt-Bearbeitung und Ausloesen der Anfrage |
| Ausfuehrungsservice | `src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs` | Fuehrt HTTP-Request, Pre/Post-Skripte, Logging und History aus |
| Ergebnis-Cache | `src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionResultCache.cs` | Speichert letztes Ergebnis pro Endpunkt im UI-Scope |
| Styles | `src/Schnittstellenzentrale/wwwroot/app.css` | Globale Styles fuer EndpointPage und Response-Bereich |
| Lokalisierung | `src/Schnittstellenzentrale/Resources/SharedResources.resx` und `.de.resx` | Texte fuer Senden-Button, Antwortbereich und Fehler |
| bUnit-Tests | `src/Schnittstellenzentrale.Tests/Components/EndpointPageTests.cs` | Komponententests fuer Ergebnisanzeige und Cache |
| Playwright-Tests | `src/Schnittstellenzentrale.Tests/Playwright/EndpointExecutionTests.cs` | E2E-Ausfuehrung ueber die UI |
| Service-Tests | `src/Schnittstellenzentrale.Tests/Services/EndpointExecutionServiceTests.cs` | Technische Ausfuehrungslogik und Fehlerfaelle |
| Integrationstest | `src/Schnittstellenzentrale.Tests/Integration/EndpointExecutionIntegrationTests.cs` | Echter EndpointExecutionService gegen Testserver |
| Anwender-Doku | `docs/help/endpunkte/ablauf-anwender.md` | Beschreibt Senden und Ergebnispruefung |
| Technische Doku | `docs/help/endpunkte/ablauf-technisch.md` | Beschreibt `SendRequestAsync` und Ausfuehrungsfluss |

## Aktueller UI-Ablauf

1. Der Button in `EndpointPage.razor:44` ruft `SendRequestAsync` auf.
2. `SendRequestAsync` speichert bei Dirty-State zuerst automatisch (`EndpointPage.razor:476-483`).
3. Danach wird der aktuelle Endpunkt per API neu geladen (`EndpointPage.razor:485-490`).
4. Die Anwendung wird ggf. nachgeladen (`EndpointPage.razor:492-495`).
5. Die Komponente wartet auf `ExecutionService.ExecuteAsync(refreshed)` (`EndpointPage.razor:497`).
6. Erst danach werden `_result` und der Cache gesetzt (`EndpointPage.razor:498-499`).
7. Der Antwortbereich wird nur bei `_result != null` gerendert (`EndpointPage.razor:95-131`).

Damit gibt es waehrend Schritt 2 bis 5 keine sichtbare Statusanzeige. Der Send-Button bleibt aktuell aktiv, weil weder ein `disabled`-Zustand noch ein Running-Flag vorhanden ist.

## Bestehende Fehler- und Ergebnisanzeige

- Allgemeine Lade-/Speicherfehler erscheinen in `_errorMessage` als `.sz-error` im Header (`EndpointPage.razor:26-29`).
- HTTP-/Post-Skript-Ergebnisse erscheinen im Antwortbereich, sobald `_result` gesetzt ist (`EndpointPage.razor:95-131`).
- `EndpointExecutionService` gibt bei internen Fehlern ein `EndpointExecutionResult` mit `Success = false` und `ErrorMessage` zurueck (`EndpointExecutionService.cs:125-135`).
- Pre-Skript-Fehler liefern ebenfalls ein `EndpointExecutionResult` ohne HTTP-Request (`EndpointExecutionService.cs:101-110`).
- `OperationCanceledException` wird im Service bewusst weitergeworfen (`EndpointExecutionService.cs:121-124`). Die UI faengt diese aktuell nicht separat ab.

## Geeigneter Aenderungspunkt

Der kleinste fachliche Aenderungspunkt liegt in `EndpointPage.razor`:

- neues privates Flag, z. B. `_isExecuting`;
- Flag vor der langen Ausfuehrung setzen und per `try/finally` garantiert zuruecksetzen;
- `StateHasChanged()` direkt nach dem Setzen erwaegen, damit die Anzeige vor dem Await sichtbar wird;
- sichtbaren Status im Bereich der Adresszeile oder oberhalb des Response-Bereichs rendern;
- optional den Senden-Button waehrend der laufenden Ausfuehrung deaktivieren, falls die offene Anforderung entsprechend entschieden wird.

Backend-Logik, Request-Aufbau, Ergebnisformat, Logging und History muessen fuer die Anforderung voraussichtlich nicht geaendert werden.

## Testluecken zur Anforderung

Bestehende Tests pruefen, dass ein Ergebnis nach Abschluss sichtbar wird, aber nicht, dass waehrend einer laufenden Anfrage ein Status sichtbar ist:

- bUnit: `EndpointPageTests.cs:83-126` prueft Response-Body und Statuscode nach Klick.
- bUnit: `EndpointPageTests.cs:128-186` prueft Cache-Verhalten beim Endpunktwechsel.
- Playwright: `EndpointExecutionTests.cs:22-46` und `EndpointExecutionTests.cs:184-189` warten auf den Response-Bereich nach Ausfuehrung.
- Service-Tests decken Ausfuehrungslogik, Fehler, Skripte, Logging und History ab, sind aber nicht fuer UI-Running-State zustaendig.

Noetig waere mindestens ein bUnit-Test mit verzogerter `ExecuteAsync`-Task, der unmittelbar nach Klick den laufenden Status sieht und nach Task-Abschluss dessen Verschwinden sowie die Ergebnisanzeige prueft. Ein Fehlerfall-Test sollte sicherstellen, dass das Flag auch bei Fehlern zurueckgesetzt wird.

## Offene Punkte aus der Anforderung, anhand des Codes eingeordnet

1. Welche konkrete Oberflaeche loest die Anfrage aus?
   - Primaer `EndpointPage` ueber `button.sz-btn-send`.
   - Listen-Buttons in `ApplicationTopEndpointsTable`, `CollectionContentView` und `FolderContentView` zeigen Ausfuehren-Icons, besitzen aber in den gelesenen Dateien keinen `@onclick` fuer direkte Ausfuehrung.
2. Soll erneutes Ausloesen verhindert werden?
   - Technisch einfach ueber `disabled="@_isExecuting"` am Send-Button moeglich.
   - Fachlich offen, weil die Anforderung nur Unterscheidbarkeit fordert, nicht zwingend Sperren.
3. Gibt es mehrere betroffene Endpunkte?
   - Die Komponente ist generisch fuer alle Endpunkte; eine Umsetzung in `EndpointPage` wirkt auf alle dort gesendeten Endpunkt-Anfragen.

## Keine untersuchten Codeaenderungen

Es wurden keine Quellcodeaenderungen vorgenommen. Die Bestandsaufnahme basiert auf statischer Analyse der genannten Dateien; Tests wurden nicht ausgefuehrt.
