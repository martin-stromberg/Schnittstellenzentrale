# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

- [x] Feld `IsPathParameter` (`bool`, Default `false`) in `QueryParamEntry` — vorhanden
- [x] `OnKeyChanged` in `RequestQueryParamsPanel` — löst `OnChanged` zusätzlich per `@onblur` aus
- [x] `OnValueChanged` in `RequestQueryParamsPanel` — löst `OnChanged` zusätzlich per `@onblur` aus
- [x] Löschen-Button-Kondition in `RequestQueryParamsPanel` — wird nur gerendert wenn `!param.IsPathParameter`
- [x] Sortierung in `RequestQueryParamsPanel` — Einträge werden per `OrderByDescending(p => p.IsPathParameter)` sortiert
- [x] Methode `SyncPathParameters()` in `EndpointPage` — vorhanden; Regex `\{([^}]+)\}` auf `_model.RelativePath`, fehlende Platzhalter werden mit `IsPathParameter = true` am Listenanfang eingefügt, entfernte Platzhalter werden aus `_queryParameters` gelöscht, vorhandene Werte bleiben erhalten
- [x] Methode `ExtractAndStripQueryString()` in `EndpointPage` — vorhanden; prüft auf `?`, zerlegt Query-String, fügt fehlende Einträge als `IsPathParameter = false` hinzu (keine Duplikate), bereinigt `_model.RelativePath`
- [x] Methode `OnPathBlur()` in `EndpointPage` — vorhanden; ruft `ExtractAndStripQueryString()`, `SyncPathParameters()` und `MarkDirty()` auf
- [x] Methode `ResolveDisplayUrl()` in `EndpointPage` — vorhanden; ersetzt Platzhalter durch Werte der `IsPathParameter = true`-Einträge, hängt reguläre Query-Parameter als `?key=value`-Anhang an
- [x] `LoadModelFromParameter()` in `EndpointPage` angepasst — ruft nach Befüllen von `_queryParameters` zusätzlich `ExtractAndStripQueryString()` und `SyncPathParameters()` auf
- [x] Pfad-Eingabefeld in `EndpointPage` Razor-Template angepasst — bindet `value="@ResolveDisplayUrl()"` und registriert `@onblur="OnPathBlur"`
- [x] `BuildRequest` in `EndpointExecutionService` angepasst — iteriert über `QueryParameters`, prüft auf Platzhalter-Treffer in `relativePath`, ersetzt Treffer per `Uri.EscapeDataString(Value)`, hängt übrige Einträge als Query-String an; leere Keys werden übersprungen
- [x] Testmethode `PfadMitPlatzhalter_WirdBeimLadenAlsNichtLoeschbarerEintragAngezeigt` in `EndpointPageTests` — vorhanden
- [x] Testmethode `PfadMitPlatzhalter_VorhandenerWertBleibtErhalten_WennPlatzhalterUnveraendert` in `EndpointPageTests` — vorhanden
- [x] Testmethode `GeaenderterPfad_EntferntWeggefalleneUndFuegtNeueHinzu` in `EndpointPageTests` — vorhanden
- [x] Testmethode `PfadMitQueryString_WirdExtrahiertUndPfadBereinigt` in `EndpointPageTests` — vorhanden
- [x] Testmethode `AufgeloesteUrl_WirdImPfadfeldAngezeigt` in `EndpointPageTests` — vorhanden
- [x] Hilfsmethode `CreateEndpointWithPath(string relPath, QueryParamEntry[]?)` in `EndpointPageTests` — vorhanden (entspricht dem im Plan beschriebenen `CreateEndpoint(string relPath, QueryParamEntry[]?)`)
- [x] Testmethode `BuildRequest_ErsetztPfadPlatzhalterDurchGespeicherteWerte` in `EndpointExecutionServiceTests` — vorhanden
- [x] Testmethode `BuildRequest_HaengtNurNichtPlatzhalterParameterAlsQueryStringAn` in `EndpointExecutionServiceTests` — vorhanden
- [x] Hilfsmethode `CreateEndpoint(AuthenticationType, string relPath, EndpointQueryParameter[]?)` in `EndpointExecutionServiceTests` — vorhanden
- [x] E2E-Szenario `EndpunktMitPlatzhalterUndQueryString_ZeigtKorrekteEintraegeUndSendetAufgeloestUrl` in `EndpointExecutionTests` (Playwright) — vorhanden

## Offene Aufgaben

Keine.

## Hinweise

Keine.
