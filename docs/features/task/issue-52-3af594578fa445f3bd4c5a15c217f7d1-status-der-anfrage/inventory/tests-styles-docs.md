# Tests, Styles und Dokumentation

## Styles

EndpointPage verwendet globale Styles in `src/Schnittstellenzentrale/wwwroot/app.css`.

Relevante Klassen:

- `.sz-endpoint-page` fuer Layout und Scrollbereich (`app.css:2102-2108`)
- `.sz-endpoint-header` fuer Kopfbereich (`app.css:2110-2118`)
- `.sz-endpoint-address-row` fuer Methode/Pfad/Senden-Zeile (`app.css:2165-2173`)
- `.sz-endpoint-address-row .sz-btn-primary` fuer den Send-Button in der Adresszeile (`app.css:2204-2208`)
- `.sz-btn-send` ist nur eine leere Identifikationsklasse (`app.css:2210-2211`)
- `.sz-endpoint-response` fuer den Antwortbereich (`app.css:2263-2268`)
- `.sz-endpoint-response-header` und `.sz-endpoint-response-meta` fuer Response-Metadaten (`app.css:2270-2290`)
- `.sz-error` fuer Fehleranzeigen (`app.css:302`)

Es gibt keine vorhandene EndpointPage-spezifische Loading- oder Spinner-Klasse. Eine Statusanzeige kann entweder neue globale Klassen in `app.css` bekommen oder bestehende Muster wie `.sz-error`, `.sz-endpoint-warning`, `.sz-section-label` und Button-Styles wiederverwenden.

## Lokalisierung

Die Komponente lokalisiert alle sichtbaren Texte ueber `IStringLocalizer<SharedResources>`.

Relevante Keys:

- `EndpointPage_SendButton`: EN "Send Request", DE "Anfrage senden" (`SharedResources.resx:445-448`, `SharedResources.de.resx:445-448`)
- `EndpointPage_Response_Label`: EN "Response", DE "Antwort" (`SharedResources.resx:485-488`, `SharedResources.de.resx:485-488`)
- `EndpointPage_Response_Status`, `EndpointPage_Response_Duration`, `EndpointPage_Response_Size` (`SharedResources.resx:489-499`, `SharedResources.de.resx:489-499`)
- `EndpointPage_Error_LoadFailed` (`SharedResources.resx:517-520`, `SharedResources.de.resx:517-520`)

Eine neue sichtbare Statusanzeige braucht neue Resource-Keys in beiden `.resx`-Dateien, z. B. fuer "Anfrage wird ausgefuehrt..." und ggf. einen Buttontext im laufenden Zustand.

## bUnit-Tests

`src/Schnittstellenzentrale.Tests/Components/EndpointPageTests.cs` registriert die benoetigten Mocks und den Ergebnis-Cache (`EndpointPageTests.cs:24-45`).

Bestehende relevante Tests:

- Ohne Ergebnis kein Antwortbereich (`EndpointPageTests.cs:74-81`)
- Ergebnis zeigt Response-Body (`EndpointPageTests.cs:83-105`)
- Ergebnis zeigt Statuscode (`EndpointPageTests.cs:107-126`)
- Ergebnis wird beim Endpunktwechsel wiederhergestellt (`EndpointPageTests.cs:128-145`)
- Fremde Ergebnisse werden nicht angezeigt (`EndpointPageTests.cs:147-164`)
- erneute Ausfuehrung aktualisiert gespeichertes Ergebnis (`EndpointPageTests.cs:166-186`)

Luecke:

- Kein Test haelt `ExecuteAsync` kuenstlich offen und prueft den Zwischenzustand.
- Kein Test prueft, dass ein laufender Zustand nach Erfolg oder Fehler wieder verschwindet.
- Kein Test prueft, ob der Senden-Button waehrenddessen deaktiviert ist.

Empfohlene bUnit-Testform:

1. `TaskCompletionSource<EndpointExecutionResult>` fuer `_executionMock.ExecuteAsync(...)` verwenden.
2. Nach Klick auf `button.sz-btn-send` unmittelbar auf Status-Markup pruefen.
3. Optional pruefen, ob `button.sz-btn-send` `disabled` traegt, falls die Planung diese Sperre vorsieht.
4. Task abschliessen und Render abwarten.
5. Pruefen, dass Status-Markup verschwunden und Response sichtbar ist.
6. Separat Fehlerfall ueber `EndpointExecutionResult { Success = false, ErrorMessage = "..." }` oder Exception/abgebrochenen Pfad pruefen.

## Playwright-Tests

`src/Schnittstellenzentrale.Tests/Playwright/EndpointExecutionTests.cs` prueft echte UI-Ausfuehrungen:

- Erfolgsausfuehrung und sichtbarer Statuscode (`EndpointExecutionTests.cs:22-46`)
- Umgebung/Variable und Ausfuehrung (`EndpointExecutionTests.cs:52-98`)
- Authenticate-Endpunkt (`EndpointExecutionTests.cs:100-129`)
- Platzhalter + Query-String und Antwortbereich (`EndpointExecutionTests.cs:135-189`)

Luecke:

- Diese Tests warten auf den finalen Response-Bereich. Sie pruefen keinen sichtbaren Zwischenzustand.
- Fuer einen stabilen E2E-Test waere ein kontrolliert langsamer Test-Endpunkt hilfreich. Ohne dedizierten langsamen Endpoint kann die Statusanzeige bei schnellen Antworten zu kurz sichtbar sein und der Test flaky werden.

## Service- und Integrationstests

`EndpointExecutionServiceTests.cs` deckt Request-Aufbau, Authentifizierung, Response-Metadaten, Skripte, Logging, Fehler und History ab. Beispiele:

- Header, Dauer und Response-Groesse (`EndpointExecutionServiceTests.cs:255-305`)
- Verbindungsfehler (`EndpointExecutionServiceTests.cs:307-324`)
- Pre-Skript-Fehler verhindert HTTP-Request (`EndpointExecutionServiceTests.cs:650-666`)
- Post-Skript-Fehler bleibt als Ergebnis sichtbar (`EndpointExecutionServiceTests.cs:689-706`)
- Logging-Kategorien (`EndpointExecutionServiceTests.cs:780-822`)
- History bei Post-Skript-Fehler (`EndpointExecutionServiceTests.cs:993-1018`)

`EndpointExecutionIntegrationTests.cs` prueft echten Service gegen Testserver mit Bearer Token (`EndpointExecutionIntegrationTests.cs:36-122`).

Diese Tests muessen fuer die UI-Statusanzeige voraussichtlich nicht angepasst werden, solange der Service-Vertrag unveraendert bleibt.

## Dokumentation

`docs/help/endpunkte/ablauf-anwender.md` beschreibt:

- Klick auf `Anfrage senden` und automatisches Speichern (`ablauf-anwender.md:58-60`)
- Ergebnisbereich mit Statuscode, Dauer, Groesse, Body und Headern (`ablauf-anwender.md:66-75`)
- Cache des letzten Ergebnisses pro Endpunkt (`ablauf-anwender.md:77-83`)

Es fehlt eine Beschreibung, dass waehrend einer laufenden Anfrage ein Status angezeigt wird.

`docs/help/endpunkte/ablauf-technisch.md` beschreibt:

- `EndpointPage.SendRequestAsync()` als Ausloeser und Nachladen des Endpunkts (`ablauf-technisch.md:106-110`)
- Service-Ablauf bis Ergebnis (`ablauf-technisch.md:112-156`)
- Fehlerbehandlung (`ablauf-technisch.md:469-477`)

Nach Umsetzung sollte dort ein kurzer Schritt fuer UI-Running-State ergaenzt werden, falls die Dokumentation im Lifecycle-Schritt 9 aktualisiert wird.

## Dokumentationshinweis fuer spaetere Umsetzung

Die Anwenderdoku sollte nach Umsetzung ergaenzen, dass nach dem Klick sofort eine sichtbare Ausfuehrungsanzeige erscheint und bis Antwort oder Fehler bestehen bleibt. Falls der Button waehrenddessen deaktiviert wird, sollte auch das erwaehnt werden.
