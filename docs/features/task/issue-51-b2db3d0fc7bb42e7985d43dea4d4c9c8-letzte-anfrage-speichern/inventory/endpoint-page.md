# Detail: EndpointPage und Komponenten-State

## Aktuelle Verantwortlichkeiten

`EndpointPage` ist die zentrale UI-Komponente fuer Bearbeitung, Ausfuehrung und Response-Anzeige eines Endpunkts. Sie injiziert unter anderem `IApplicationApiClient`, `IEndpointExecutionService`, `NavigationManager`, `IStorageModeService`, `ISignalRNotificationService` und `IActiveEnvironmentService`.

Wichtige State-Felder in `src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor`:

- `_model`: lokales, editierbares Endpoint-Modell.
- `_headers` und `_queryParameters`: lokale Darstellungen der Request-Parameter.
- `_isDirty`, `_errorMessage`, `_showConcurrencyWarning`: Bearbeitungs- und Fehlerzustand.
- `_result`: aktuelles Ausfuehrungsergebnis.
- `_activeRequestTab`, `_activeResponseTab`: aktive Tabs.
- `_lastLoadedEndpointId`: Erkennung eines Endpunktwechsels.

## Relevante Codepfade

- Zeile 94: Response-Sektion wird nur bei `_result != null` gerendert.
- Zeilen 100-107: Status, Dauer und Response-Groesse werden aus `_result` angezeigt.
- Zeilen 111-113: `ErrorMessage` aus `_result` wird angezeigt.
- Zeilen 122 und 126: Body und Header werden an `ResponseBodyPanel` und `ResponseHeadersPanel` weitergegeben.
- Zeile 148: `_result` ist lokaler Komponenten-State.
- Zeilen 163-173: `OnParametersSetAsync()` erkennt `Endpoint.Id`-Wechsel, laedt das Modell neu und setzt `_result = null`.
- Zeilen 475-496: `SendRequestAsync()` speichert das Ergebnis der Ausfuehrung in `_result`.

## Bedeutung fuer die Anforderung

Der Ist-Zustand ist nicht persistierend pro Endpunkt. Es gibt genau einen Ergebnis-Slot (`_result`) fuer die aktuell dargestellte Komponente. Beim Wechsel auf eine andere `Endpoint.Id` wird dieser Slot geleert. Damit existiert kein Ort, an dem die letzte Response von Endpunkt A erhalten bleibt, wenn Endpunkt B geladen wird.

Fuer die geplante Aenderung gibt es zwei naheliegende Ansatzpunkte:

- In `EndpointPage`: Ein Dictionary `Endpoint.Id -> EndpointExecutionResult` als lokaler Cache. Das ist minimal, aber nur so langlebig wie die Komponenteninstanz.
- Scoped UI-Service: Ein kleiner Cache-Service fuer `EndpointExecutionResult` pro `Endpoint.Id`. Das ist robuster bei Navigation innerhalb der Blazor-Session, erfordert aber Registrierung und Tests fuer einen neuen Service.

## Abgrenzung

`EndpointPage` braucht fuer die reine Wiederanzeige keine Aenderungen an Response-Panels. Diese konsumieren bereits die Daten aus `_result`.

