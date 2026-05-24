# Bestandsaufnahme: URL-Platzhalter und Query-Parameter

Analysiert wurden die Datenmodell-, UI- und Logikklassen, die laut Anforderung von der Unterscheidung zwischen Pfad-Platzhaltern und regulären Query-Parametern betroffen sind. Grundlage ist `docs/features/16-verfeinerung-der-query-parameter/requirement.md`.

## Zusammenfassung

- `EndpointQueryParameter` besitzt `Key`, `Value` und `EndpointId` — kein `IsPathParameter`-Feld. Die Datenstruktur ist ausreichend für die Persistierung, jedoch fehlt die Laufzeit-Unterscheidung.
- `Endpoint.RelativePath` speichert den Pfad als Template. Keine automatische Bereinigung eines enthaltenen Query-Strings beim Laden oder Bearbeiten.
- `RequestQueryParamsPanel.QueryParamEntry` hat nur `Key` und `Value` — kein `IsPathParameter`-Feld. Der Löschen-Button wird für alle Einträge bedingungslos gerendert.
- `EndpointPage` hat keinen `onblur`-Handler am Pfad-Eingabefeld. Die Methoden `SyncPathParameters()`, `ExtractAndStripQueryString()`, `OnPathBlur()` und `ResolveDisplayUrl()` existieren nicht. `LoadModelFromParameter()` befüllt `_queryParameters` nur mit `Key`/`Value` ohne Pfad-Platzhalter-Erkennung.
- `EndpointExecutionService.BuildRequest` hängt alle `QueryParameters` bedingungslos als Query-String an. Pfad-Platzhalter in `RelativePath` werden nicht aufgelöst.
- Bestehende Tests decken Pfad-Platzhalter-Erkennung und Query-String-Extraktion in keiner Klasse ab. Weder `EndpointPageTests`, `EndpointExecutionServiceTests` noch die Playwright-Tests enthalten entsprechende Szenarien.

## Details

- [Datenmodell](inventory/models.md)
- [Logik](inventory/logic.md)
- [Interfaces](inventory/interfaces.md)
- [Tests](inventory/tests.md)
