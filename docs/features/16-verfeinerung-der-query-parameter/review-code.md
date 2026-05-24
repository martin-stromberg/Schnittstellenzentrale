# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### EndpointPage.razor (`ResolveDisplayUrl`)

- **Doppelter Code** — `ResolveDisplayUrl()` (Zeilen 406–425) enthält nahezu identische Logik wie `BuildRequest()` in `EndpointExecutionService.cs` (Zeilen 121–143): Beide iterieren über Query-Parameter, prüfen auf `{key}`-Platzhalter im Pfad, ersetzen Treffer und hängen verbleibende Parameter als Query-String an. Die Implementierungen können künftig auseinanderlaufen.

  Empfehlung: Eine statische Hilfsmethode (z. B. `EndpointUrlBuilder.Resolve(string relativePath, IEnumerable<(string Key, string Value)> parameters)`) in `Core` oder `Infrastructure` extrahieren und in beiden Stellen verwenden.

### EndpointPage.razor (`OnPathBlur`)

- **Fehlende Validierung / falsches Dirty-Flag** — `OnPathBlur()` ruft `MarkDirty()` bedingungslos auf (Zeile 482), auch wenn der Benutzer das Pfadfeld nur fokussiert und ohne Änderung verlässt. Das führt dazu, dass der Speichern-Dialog beim Verlassen der Seite erscheint, obwohl keine inhaltliche Änderung vorgenommen wurde.

  Empfehlung: Vor `MarkDirty()` prüfen, ob sich der Pfad tatsächlich geändert hat. Den Pfadwert und die Anzahl der `_queryParameters` vor `ExtractAndStripQueryString()` merken und `MarkDirty()` nur aufrufen, wenn danach eine Abweichung besteht.

### EndpointExecutionServiceTests.cs (`BuildRequest_ErsetztPfadPlatzhalterDurchGespeicherteWerte`, `BuildRequest_HaengtNurNichtPlatzhalterParameterAlsQueryStringAn`)

- **Doppelter Code** — Beide neuen Testmethoden (ab Zeilen 262 und 295) richten `handlerMock`, `httpClient` und `factoryMock` vollständig inline ein, obwohl die Klasse bereits die Hilfsmethode `CreateService()` bereitstellt, die den `handlerMock` als zweiten Rückgabewert liefert. Das ergibt je ca. 10 Zeilen redundanten Setup-Code.

  Empfehlung: Beide Tests auf `CreateService()` umstellen und den zurückgegebenen `handlerMock` per `.Callback<HttpRequestMessage, CancellationToken>((req, _) => sentUri = req.RequestUri)` in `Protected().Setup(...)` nutzen — oder `CreateService()` so anpassen, dass ein optionaler Callback übergeben werden kann.

### RequestQueryParamsPanel.razor (`QueryParamEntry.IsPathParameter`)

- **Toter Code (redundante Initialisierung)** — `public bool IsPathParameter { get; set; } = false;` (Zeile 41): Der Initialwert `= false` ist identisch mit dem Standardwert von `bool` und hat keinen Effekt.

  Empfehlung: Initialwert entfernen: `public bool IsPathParameter { get; set; }`.

### EndpointPageTests.cs (`CreateEndpoint`, `CreateEndpointWithPath`)

- **Doppelter Code** — `CreateEndpoint()` (Zeile 38) und `CreateEndpointWithPath()` (Zeile 51) unterscheiden sich nur in wenigen Feldern (`RelativePath`, `QueryParameters`, `Body`), duplizieren aber die restliche Initialisierung (8 identische Felder: `Id`, `Name`, `Method`, `ApplicationId`, `Application`, `Headers` u. a.).

  Empfehlung: `CreateEndpoint()` um optionale Parameter `string relPath = "/test"` und `RequestQueryParamsPanel.QueryParamEntry[]? queryParameters = null` erweitern und `CreateEndpointWithPath()` entfernen.

## Geprüfte Dateien

- `src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs`
- `src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor`
- `src/Schnittstellenzentrale/Components/Shared/RequestQueryParamsPanel.razor`
- `src/Schnittstellenzentrale.Tests/Components/EndpointPageTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/EndpointExecutionServiceTests.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/EndpointExecutionTests.cs`
