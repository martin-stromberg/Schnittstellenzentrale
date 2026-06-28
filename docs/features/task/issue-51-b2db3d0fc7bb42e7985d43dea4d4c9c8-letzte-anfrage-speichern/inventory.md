# Bestandsaufnahme: Letzte Anfrage je Endpunkt speichern

## Kurzfazit

Die betroffene Logik sitzt zentral in `EndpointPage`. Beim Parameterwechsel erkennt `OnParametersSetAsync()` eine neue `Endpoint.Id`, lädt das lokale Bearbeitungsmodell neu und setzt `_result` explizit auf `null`. Dadurch verschwindet das zuletzt erzeugte Ausführungsergebnis, sobald ein anderer Endpunkt ausgewählt wird. Die Ausführung selbst liefert bereits ein vollständiges `EndpointExecutionResult`, das in `_result` gespeichert und von den bestehenden Response-Panels gerendert wird.

Ein UI-seitiger Cache pro `Endpoint.Id` kann ohne Änderungen an `IEndpointExecutionService`, `EndpointExecutionService` oder `EndpointExecutionResult` ansetzen. Offen bleibt fachlich, ob der Zustand nur innerhalb der aktuellen `EndpointPage`-Instanz oder ueber Komponentenwechsel innerhalb der laufenden Workspace-Sitzung erhalten bleiben soll.

## Detaildokumente

- [EndpointPage und Komponenten-State](inventory/endpoint-page.md)
- [Endpunktwechsel und Navigationsfluss](inventory/endpunktwechsel.md)
- [Ausfuehrungsergebnis und Response-Anzeige](inventory/ausfuehrungsergebnis.md)
- [Testabdeckung und Testluecken](inventory/tests.md)

## Relevante Dateien

| Bereich | Datei | Bedeutung |
|--------|-------|-----------|
| UI-Komponente | `src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor` | Laedt Endpoint-Parameter, verwaltet `_result`, fuehrt Anfrage aus und rendert Response. |
| Elternlayout | `src/Schnittstellenzentrale/Components/Layout/WorkspacesLayout.razor` | Rendert `EndpointPage` fuer die aktuelle Workspace-Auswahl. |
| Navigation | `src/Schnittstellenzentrale/Components/Shared/WorkspacesSidebar.razor` | Setzt bei Endpunktauswahl `NavigationStateService.CurrentSelection`. |
| Ergebnisdaten | `src/Schnittstellenzentrale.Core/Models/EndpointExecutionResult.cs` | Datenobjekt fuer Status, Body, Header, Fehler und Metriken. |
| Service-Vertrag | `src/Schnittstellenzentrale.Core/Interfaces/IEndpointExecutionService.cs` | Liefert `Task<EndpointExecutionResult> ExecuteAsync(Endpoint endpoint)`. |
| Service-Implementierung | `src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs` | Erzeugt das Ergebnis, persistiert History nur bei erfolgreicher Ausfuehrung. |
| bUnit-Tests | `src/Schnittstellenzentrale.Tests/Components/EndpointPageTests.cs` | Testet Response-Anzeige und EndpointPage-Verhalten, aber noch keine Ergebniswiederherstellung beim Endpunktwechsel. |
| Playwright-Tests | `src/Schnittstellenzentrale.Tests/Playwright/EndpointExecutionTests.cs` | Testet reale Endpunktausfuehrung und Response-Anzeige im Browser. |

## Beobachteter Ist-Zustand

- `EndpointPage` besitzt ein privates Feld `_result` vom Typ `EndpointExecutionResult?`.
- Die Response-Sektion wird nur gerendert, wenn `_result != null` ist.
- `SendRequestAsync()` ruft `ExecutionService.ExecuteAsync(refreshed)` auf und weist das Ergebnis direkt `_result` zu.
- `OnParametersSetAsync()` vergleicht `Endpoint.Id` mit `_lastLoadedEndpointId`; bei Wechsel wird `_result = null` gesetzt.
- `EndpointExecutionResult` ist ein einfaches Datenmodell ohne Persistenz- oder Kopierlogik.
- Die Elternkomponenten wechseln den ausgewaehlten Endpunkt ueber `NavigationStateService`; fuer `EndpointPage` kommt der Wechsel als neuer `Endpoint`-Parameter an.
- Bestehende Tests pruefen, dass ein Ergebnis angezeigt wird, aber nicht, dass ein altes Ergebnis je Endpunkt erhalten bleibt.

## Risiken fuer die Umsetzung

- Ein Cache nur als Feld in `EndpointPage` ueberlebt nur, solange dieselbe Komponenteninstanz erhalten bleibt. Wechsel zu Application-, Group- oder Empty-Views kann die Komponente aus dem Renderbaum entfernen.
- Ein Cache in einem scoped UI-Service waere robuster fuer Navigation innerhalb derselben Browser-/App-Sitzung, fuehrt aber einen neuen State-Baustein ein.
- Ergebnisse koennen veraltet wirken, wenn ein Endpunkt nach der Ausfuehrung editiert, gespeichert oder geloescht wird. Die Anforderung klaert nicht, ob dann invalidiert werden muss.
- Fehlgeschlagene Ausfuehrungen werden ebenfalls als `EndpointExecutionResult` dargestellt; fachlich ist offen, ob diese ebenso wiederhergestellt werden sollen.

## Geeignete Testpunkte

- bUnit: `EndpointPage` mit Endpunkt A rendern, Anfrage ausfuehren, auf Endpunkt B wechseln, Anfrage ausfuehren oder leer lassen, zurueck zu A wechseln und Response-Inhalt pruefen.
- bUnit: pruefen, dass ein erfolgreich neu erzeugtes Ergebnis den Cache fuer dieselbe `Endpoint.Id` ersetzt.
- bUnit: pruefen, dass Endpunkt B nicht das Ergebnis von Endpunkt A anzeigt.
- Optional Playwright: Endpunkt A ausfuehren, Endpunkt B im Baum auswaehlen, wieder A auswaehlen und sichtbare Response pruefen.

