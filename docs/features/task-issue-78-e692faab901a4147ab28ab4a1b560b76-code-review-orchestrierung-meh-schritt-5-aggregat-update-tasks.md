# Tasks: Einheitliches Aggregat-Update für Endpunkte

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---------|---------|--------|--------------|
| 1 | OData-Infrastruktur | `ODataEndpointUpdateHelper` anlegen (`TryDeserializeEndpoint`, `ContainsProperty`, `TryReadChildren`) | Offen | — |
| 2 | REST | `EndpointsController.UpdateAsync` auf reine Aggregat-Operation umstellen (Kinder ins `Endpoint`-Objekt mappen, manuelle Einzel-Kind-Aufrufe entfernen) | Offen | — |
| 3 | OData | `ODataEndpointsController.Put` auf `JsonElement`-Signatur + Kind-Mengen-Ersetzung bei Vorhandensein umstellen | Offen | — |
| 4 | OData | `ODataEndpointsController.ApplyPatch` → `TryApplyPatch` mit Fehlerkanal + `headers`/`queryparameters`-Fällen | Offen | — |
| 5 | UI | `EndpointPage.razor`: Sofort-Kind-Persistenz entfernen (4 Methoden + 4 Callback-Verdrahtungen) | Offen | — |
| 6 | Dokumentation Code | XML-Doc zu `UpdateEndpointAsync` in `IEndpointRepository`/`EndpointRepository` auf Aggregat-Semantik präzisieren | Offen | — |
| 7 | Tests | `EndpointRepositoryIntegrationTests`: Aggregate-Update-Tests (Ersetzen, Leeren, Konflikt lässt Kinder unverändert) | Offen | — |
| 8 | Tests | `ApplicationApiClientIntegrationTests`: REST PUT Vollmengen-Ersetzung + Leeren | Offen | — |
| 9 | Tests | `ODataControllerIntegrationTests`: Put/Patch Kind-Mengen-Semantik + Fehlerfall | Offen | — |
| 10 | Dokumentation | `docs/help/api/api.md` + `docs/help/api/odata-api.md`: Aggregat-/Kind-Mengen-Semantik festhalten | Offen | — |
