# Plan-Gegenprüfung

## Ergebnis

**Status:** Plan vollständig

## Abgleich Akzeptanzkriterien

| Akzeptanzkriterium | Umsetzung im Plan | Testnachweis im Plan | Status |
|--------------------|-------------------|----------------------|--------|
| Eine gemeinsame serverseitige Aggregat-Operation (Skalare + Kinder in einem Schritt, eindeutige Semantik) | `IEndpointRepository.UpdateEndpointAsync` als die eine Operation; XML-Doc-Präzisierung; alle Pfade delegieren darauf | `UpdateEndpointAsync_ReplacesScalarsAndChildren`, `UpdateEndpointAsync_EmptyChildren_ClearsExisting` | Abgedeckt |
| REST-Update-Endpunkt nutzt die Operation ohne redundante Einzel-Kind-Aufrufe | Schritt 2 der Umsetzungsreihenfolge (`EndpointsController.UpdateAsync`) | `PutEndpoint_WithHeadersAndQueryParameters_ReplacesChildren`, `PutEndpoint_WithoutChildren_ClearsExisting` | Abgedeckt |
| OData Put/Patch pflegen mitgelieferte Kinder über denselben Pfad (nicht mehr still ignoriert) | Schritt 3: `Put` auf `JsonElement` + `ODataEndpointUpdateHelper`; `TryApplyPatch` mit `headers`/`queryparameters`-Fällen | `PutEndpoint_WithHeaders_ReplacesSuppliedSet`, `PutEndpoint_WithoutChildren_PreservesExisting`, `PatchEndpoint_WithQueryParameters_ReplacesSet`, `PatchEndpoint_WithMalformedHeaders_Returns400` | Abgedeckt |
| Import-Apply-Pfade nutzen dieselbe Operation | Bestandsaufnahme bestätigt `UpdateEndpointAsync`-Nutzung in beiden `ApplyDiffAsync`; plan explizit „unverändert, dokumentiert" | Bestehende `ODataImportServiceIntegrationTests`/Import-Tests bleiben grün; Aggregat-Semantik durch Repository-Tests abgedeckt | Abgedeckt |
| REST-Vertrag unverändert (keine Flags, Vollmengen-Ersetzung bereits Vertrag) | Keine Änderung an `UpdateEndpointRequest`; Scope-Ausschluss 1 | `PutEndpoint_WithoutChildren_ClearsExisting` (unverändertes Leeren bei fehlenden Kindern) | Abgedeckt |
| Optimistic-Concurrency-Verhalten unverändert | Kein Code-Change am Concurrency-Pfad; `RowVersion` weiter `OriginalValue` | `UpdateEndpointAsync_ConcurrencyConflict_LeavesChildrenUnchanged`; bestehender `SaveEndpoint_ConcurrentWrite_DetectsConflict` | Abgedeckt |
| Redundante Sofort-Kinder-Pflege in `EndpointPage.razor` darf entfallen | Schritt 4: Entfernung der 4 Methoden + Verdrahtungen; `OnChanged`/`MarkDirty` + Vollmengen-Save bleiben | `EndpointPageTests` (Save-Pfad unverändert grün) | Abgedeckt |
| Integrations-Tests der Aggregat-Operation + OData-/REST-Update-Tests | Tests-Abschnitt listet alle neuen Testmethoden konkret | siehe links | Abgedeckt |
| Bestehende Suiten bleiben grün | Betroffene bestehende Tests benannt (`PutEndpoint_*` wegen Signaturwechsel) | Vollständiger Suite-Lauf in Schritt 10 | Abgedeckt |
| Einheitliche Semantik in API-/OData-API-Doku festgehalten | Schritt 7 Umsetzungsreihenfolge → `docs/help/api/api.md`, `docs/help/api/odata-api.md` | Dokumentationsschritt 12 | Abgedeckt |

## Fehlende oder unvollständige Testanforderungen

—

## E2E-Abdeckung

| Benutzerfluss / Akzeptanzkriterium | Geplanter E2E-Test | Status |
|------------------------------------|--------------------|--------|
| UI-Speichern Endpunkt (Vollmenge) | Kein neuer E2E-Test: kein neuer/geänderter Benutzerfluss — sichtbares Verhalten (Dirty → Save → Vollmengen-PUT) unverändert; bUnit-Tests des Save-Pfads + bestehende Playwright-Suiten bleiben grün; reale Abnahme in Schritt 10 spielt Speichern bzw. API-Aufrufe real durch | Nicht erforderlich mit Begründung |

## Fehlende oder unvollständige Planbestandteile

—

## Hinweise

- Der Signaturwechsel `Put(Endpoint)` → `Put(JsonElement)` ist technisch nötig, weil das EDM `Headers`/`QueryParameters` ignoriert und der OData-Formatter sie niemals in `entity` bindet — „mitgelieferte" Kinder sind sonst nicht erkennbar. `Patch` nutzt bereits `JsonElement` (Präzedenz). Deserialisierung muss `JsonStringEnumConverter` + `PropertyNameCaseInsensitive` verwenden, damit die von OData-/System.Text.Json-Clients gesendeten Feldformate (String-Enums wie `"GET"`, numerische Enums, PascalCase- und camelCase-Namen) funktionieren.
- Scope-Ausschlüsse 1–6 sind keine vom Plan erfundenen Einschränkungen, sondern aus dem Anforderungstext übernommene Festlegungen und wurden entsprechend als `bestätigt` geführt.
