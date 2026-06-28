# Detail: Endpunktwechsel und Navigationsfluss

## Auswahlfluss

Die Auswahl eines Endpunkts beginnt im Workspace-Baum:

- `WorkspacesSidebar.razor` verarbeitet `OnEndpointSelected`.
- Dort wird aus dem ausgewaehlten `Endpoint` ein Pfad aufgebaut und per `NavigationStateService.SetWorkspaceSelectionAsync(new WorkspaceSelection(endpoint, path))` gesetzt.
- `WorkspacesLayout.razor` reagiert auf `NavigationStateService.OnSelectionChanged` mit `StateHasChanged`.
- Wenn `CurrentSelection.SelectedItem` ein `Endpoint` ist, rendert `WorkspacesLayout` `EndpointPage Endpoint="selectedEndpoint"`.

Relevante Stellen:

- `src/Schnittstellenzentrale/Components/Shared/WorkspacesSidebar.razor`, Zeilen 95-109: Endpunkt wird als Workspace-Auswahl gesetzt.
- `src/Schnittstellenzentrale/Components/Layout/WorkspacesLayout.razor`, Zeilen 104-106: `EndpointPage` wird fuer den ausgewaehlten Endpunkt gerendert.
- `src/Schnittstellenzentrale/Components/Layout/WorkspacesLayout.razor`, Zeilen 116-136 und 407-410: Layout abonniert Selection-Events und triggert Re-Render.

## Wechselerkennung in EndpointPage

`EndpointPage.OnParametersSetAsync()` vergleicht `Endpoint.Id` mit `_lastLoadedEndpointId`.

Bei abweichender Id passiert aktuell:

1. `_lastLoadedEndpointId` wird aktualisiert.
2. `LoadModelFromParameter()` kopiert den neuen Endpoint in `_model`.
3. `_isDirty` wird auf `false` gesetzt.
4. `_errorMessage` wird auf `null` gesetzt.
5. `_result` wird auf `null` gesetzt.
6. Navigation Guards werden deaktiviert bzw. abgemeldet.

Der Reset von `_result` ist die direkte Ursache fuer die verschwundene Ergebnisanzeige.

## Lebensdauerfrage

Ein wichtiger technischer Punkt ist die Lebensdauer von `EndpointPage`:

- Beim Wechsel von Endpunkt A zu Endpunkt B innerhalb derselben Renderposition kann Blazor dieselbe Komponente mit neuem Parameter weiterverwenden. Dann reicht ein Feld-Cache in `EndpointPage`.
- Beim Wechsel zu anderen Auswahltypen, etwa Application oder EndpointGroup, wird `EndpointPage` nicht mehr gerendert. Ein lokaler Feld-Cache in der Komponente kann dabei verloren gehen.
- Ein scoped Service wuerde Ergebnisse innerhalb derselben Blazor-Session auch dann erhalten, wenn `EndpointPage` zwischenzeitlich entfernt und spaeter erneut erzeugt wird.

Die Anforderung sagt "zwischen mehreren Endpunkten navigieren", aber nicht ausdruecklich, ob ein Zwischenwechsel auf andere Views eingeschlossen ist.

## Auswirkungen auf Dirty-State

Der Endpunktwechsel setzt `_isDirty` bereits zurueck und entfernt Navigation Guards. Die Ergebnis-Wiederherstellung sollte diesen Bearbeitungszustand nicht veraendern. Insbesondere sollte das Anzeigen eines gecachten Ergebnisses kein `MarkDirty()` ausloesen.

