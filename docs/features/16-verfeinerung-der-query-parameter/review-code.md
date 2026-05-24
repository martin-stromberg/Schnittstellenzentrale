# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### EndpointExecutionServiceTests.cs (`BuildRequest_ErsetztPfadPlatzhalterDurchGespeicherteWerte`, `BuildRequest_HaengtNurNichtPlatzhalterParameterAlsQueryStringAn`)

- **Doppelter Code** — Beide neuen Testmethoden (Zeilen 263 und 290) enthalten identischen Setup-Code: `Uri? sentUri = null`, `CreateService(...)`, und dann ein erneutes vollständiges `handlerMock.Protected().Setup(...)` mit Callback. Das sind je ca. 8 Zeilen Boilerplate, die in beiden Methoden identisch sind.

  Empfehlung: Eine Hilfsmethode `CreateServiceWithUriCapture(out Uri? sentUri)` extrahieren — oder `CreateService()` um einen optionalen Callback-Parameter erweitern — und beide Tests darauf umstellen.

### RequestQueryParamsPanel.razor (`QueryParamEntry.IsPathParameter`)

- **Toter Code (redundante Initialisierung)** — `public bool IsPathParameter { get; set; } = false;` (Zeile 41): Der explizite Initialwert `= false` ist identisch mit dem C#-Standardwert von `bool` und hat keinen Effekt.

  Empfehlung: Initialwert entfernen: `public bool IsPathParameter { get; set; }`.

## Geprüfte Dateien

- `src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs`
- `src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor`
- `src/Schnittstellenzentrale/Components/Shared/RequestQueryParamsPanel.razor`
- `src/Schnittstellenzentrale/Components/Shared/ApplicationContextMenu.razor`
- `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupContextMenu.razor`
- `src/Schnittstellenzentrale/Components/Shared/EndpointContextMenu.razor`
- `src/Schnittstellenzentrale/Components/Shared/EndpointGroupContextMenu.razor`
- `src/Schnittstellenzentrale.Tests/Components/EndpointPageTests.cs`
- `src/Schnittstellenzentrale.Tests/Services/EndpointExecutionServiceTests.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/EndpointExecutionTests.cs`
