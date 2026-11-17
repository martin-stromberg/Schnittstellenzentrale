# Übersetzte Anforderung

Issue-Bezug: #88 (Entwicklungsschritt 2 aus Projekt `issue-78-…-code-review-orchestrierung-meh`)

## Ziel

Das Anwenden eines berechneten Swagger-Import-Diffs läuft über einen eigenen, validierten
REST-Endpunkt — analog zum bestehenden OData-Import-Apply. Die Oberfläche nutzt
denselben Pfad über den API-Client statt den Import-Service direkt.

## Funktionale Anforderungen

### F1: Neuer REST-Endpunkt `POST /api/applications/{id}/swagger-import/apply`

- `401`, wenn das Bearer-Token fehlt oder ungültig ist.
- `404`, wenn die Zielanwendung nicht existiert.
- `422`, wenn die Anwendung nicht den Schnittstellentyp `Rest` hat.
- `204`, wenn der übergebene `ImportDiff` erfolgreich angewendet wurde.

### F2: Aggregat-Validierung in der Apply-Logik des Swagger-Imports

- Beim Anwenden eines Diffs wird geprüft, dass alle Endpunkte des Diffs (neu, geändert,
  entfernt) dieselbe Anwendungs-ID tragen wie die Zielanwendung des Aufrufs.
- Bei Verletzung wird der Diff **nicht** angewendet; ein verständlicher Fehler wird
  zurückgegeben.

### F3: API-Client-Erweiterung

- `IApplicationApiClient` erhält eine Aufrufmethode für den neuen Endpunkt.
- Fehler werden wie im OData-Pendant (`ApplyODataDiffAsync`) als verständliche
  Fehlermeldung durchgereicht.

### F4: UI-Umstellung

- `SwaggerImportDialog.razor` ruft künftig den API-Pfad über `IApplicationApiClient`
  statt `ISwaggerImportService` direkt.
- Damit gelten für den UI-Import dieselben Nebenbedingungen (Token-Validierung,
  Existenz- und Typprüfung) wie für externe Aufrufer.

## Nicht-funktionale Anforderungen

- **Tests:** Unit-Tests mit gemocktem `HttpMessageHandler` für den neuen Client-Aufruf;
  Integrationstests mit `WebApplicationFactory` für den Endpunkt (204/401/404/422);
  bestehende Suiten inkl. Playwright bleiben grün.
- **Dokumentation:** `docs/help/api/api.md` wird um den neuen Endpunkt ergänzt.
- **Lokalisierung:** Neue UI-Texte (falls neue Fehlermeldungen im Dialog entstehen)
  über das zentrale Ressourcen-Paket `SharedResources.resx` + `SharedResources.de.resx`
  nach dem Schlüsselschema `{KomponentenName}_{Rolle}` mit Kontext-Kommentar je Sprache.
- **Architektur:** API-First — UI-Komponenten greifen ausschließlich über
  `IApplicationApiClient` auf Daten zu.
