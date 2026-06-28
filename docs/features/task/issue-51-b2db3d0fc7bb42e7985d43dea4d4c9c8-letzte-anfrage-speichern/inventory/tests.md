# Detail: Testabdeckung und Testluecken

## Testprojekt

Das Testprojekt `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj` nutzt:

- bUnit 2.7.2 fuer Komponenten-Tests.
- xUnit 2.9.3.
- Moq 4.20.72.
- Microsoft.Playwright 1.52.0 fuer Browser-Tests.
- Target Framework `net9.0`.
- Dokumentationswarnungen `CS1591` als Fehler.

## Vorhandene bUnit-Tests fuer EndpointPage

`src/Schnittstellenzentrale.Tests/Components/EndpointPageTests.cs` deckt unter anderem ab:

- Ohne Anfrageergebnis ist die Response-Sektion nicht sichtbar.
- Nach Ausfuehrung wird Response-Body angezeigt.
- Nach Ausfuehrung wird HTTP-Statuscode angezeigt.
- Body-, Query-Parameter-, Pfadplatzhalter- und Skript-Tab-Verhalten.
- Aufgeloeste URL mit Platzhalter- und Query-Parameterwerten.

Relevante Stellen:

- Zeilen 67-70: Response-Bereich ist ohne Ergebnis nicht sichtbar.
- Zeilen 76-93: Anfrageergebnis zeigt Body.
- Zeilen 100-115: Anfrageergebnis zeigt Statuscode.
- Zeilen 328-353: aufgeloeste URL im Pfadfeld.

## Fehlende bUnit-Abdeckung fuer diese Anforderung

Es gibt aktuell keinen Test, der `EndpointPage` mit Endpunkt A ausfuehrt, per `SetParametersAndRender` oder Parent-Render auf Endpunkt B wechselt und danach A wiederherstellt.

Geeignete neue Tests:

- `Endpunktwechsel_StelltLetztesErgebnisWiederHer`: A ausfuehren, B laden, A laden, A-Response ist wieder sichtbar.
- `Endpunktwechsel_ZeigtKeinFremdesErgebnis`: A ausfuehren, B laden, B zeigt nicht A-Response.
- `ErneuteAusfuehrung_AktualisiertGespeichertesErgebnis`: A ausfuehren, A erneut ausfuehren, Cache enthaelt neue Response.
- Optional: Fehlerergebnis wird wiederhergestellt, falls fachlich gewuenscht.

## Vorhandene Playwright-Tests

`src/Schnittstellenzentrale.Tests/Playwright/EndpointExecutionTests.cs` prueft reale Browser-Flows:

- `ExecuteEndpoint_ReturnsSuccessResponse`: Endpunkt anlegen, `/app.css` ausfuehren, Response-Status sichtbar.
- `UmgebungMitVariable_Aktivieren_EndpunktSendetAufgeloestUrl`: Umgebung mit Variable, Platzhalterauflösung und Ausfuehrung.
- `AuthenticateEndpunkt_Auswaehlen_GibtTokenZurueck`: System-Endpunkt auswaehlen und ausfuehren.
- `EndpunktMitPlatzhalterUndQueryString_ZeigtKorrekteEintraegeUndSendetAufgeloestUrl`: Query-/Path-Parameter und Response-Bereich.

## Geeignete Testkommandos

Fokussiert:

```powershell
dotnet test src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj --filter EndpointPageTests
```

Falls Playwright relevant erweitert wird:

```powershell
dotnet test src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj --filter EndpointExecutionTests
```

Vollstaendig:

```powershell
dotnet test src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj
```

