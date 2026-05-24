# Umsetzungsplan: URL-Platzhalter und Query-Parameter

## Übersicht

`EndpointPage` und `RequestQueryParamsPanel` werden um die Unterscheidung zwischen nicht löschbaren Pfad-Platzhaltern und löschbaren regulären Query-Parametern erweitert. Neue Methoden in `EndpointPage` analysieren `RelativePath` per Regex, extrahieren enthaltene Query-Strings und zeigen die aufgelöste URL im Pfadfeld an. `EndpointExecutionService.BuildRequest` wird angepasst, sodass Pfad-Platzhalter durch ihre gespeicherten Werte ersetzt werden, bevor der Query-String angehängt wird.

---

## Programmabläufe

### Laden eines Endpunkts

1. `EndpointPage.OnParametersSetAsync()` ruft `LoadModelFromParameter()` auf.
2. `LoadModelFromParameter()` befüllt `_model` aus dem geladenen `Endpoint`.
3. `LoadModelFromParameter()` befüllt `_queryParameters` aus `Endpoint.QueryParameters` mit `IsPathParameter = false` als Startwert.
4. `LoadModelFromParameter()` ruft `ExtractAndStripQueryString()` auf, um einen etwaigen Query-String-Anteil in `_model.RelativePath` zu extrahieren und den Pfad zu bereinigen.
5. `LoadModelFromParameter()` ruft `SyncPathParameters()` auf, um Platzhalter aus dem bereinigten `_model.RelativePath` zu erkennen und in `_queryParameters` als `IsPathParameter = true` zu markieren bzw. fehlende hinzuzufügen.
6. Die Komponente rendert das Pfadfeld mit dem Rückgabewert von `ResolveDisplayUrl()`.

Beteiligte Klassen/Komponenten: `EndpointPage`, `RequestQueryParamsPanel`, `QueryParamEntry`

---

### Bearbeitung des Pfadfelds (`onblur`)

1. Der Anwender verlässt das Pfad-Eingabefeld.
2. `OnPathBlur()` wird ausgelöst.
3. `OnPathBlur()` ruft `ExtractAndStripQueryString()` auf — Query-String-Anteil wird extrahiert und `_model.RelativePath` auf den reinen Pfad reduziert.
4. `OnPathBlur()` ruft `SyncPathParameters()` auf — Platzhalter werden neu abgeglichen: neue Platzhalter werden hinzugefügt, entfernte Platzhalter werden aus `_queryParameters` gelöscht, vorhandene Werte bleiben erhalten.
5. `OnPathBlur()` ruft `MarkDirty()` auf.
6. Die Komponente rendert das Pfadfeld neu mit dem aktualisierten Rückgabewert von `ResolveDisplayUrl()`.

Beteiligte Klassen/Komponenten: `EndpointPage`, `QueryParamEntry`

---

### Änderung eines Query-Parameter-Werts (`onblur`)

1. Der Anwender verlässt ein Name- oder Wert-Eingabefeld im `RequestQueryParamsPanel`.
2. `OnKeyChanged` bzw. `OnValueChanged` ruft `OnChanged` auf (zusätzlich zum bestehenden `oninput`-Trigger).
3. `EndpointPage` empfängt das `OnChanged`-Event und rendert das Pfadfeld neu mit dem aktualisierten Rückgabewert von `ResolveDisplayUrl()`.

Beteiligte Klassen/Komponenten: `RequestQueryParamsPanel`, `EndpointPage`

---

### Speichern eines Endpunkts

1. `EndpointPage.SaveAsync()` baut `_model.QueryParameters` aus `_queryParameters` auf — sowohl Pfad-Platzhalter-Einträge als auch reguläre Query-Parameter werden als gleichartige `EndpointQueryParameter`-Objekte übernommen (kein `IsPathParameter`-Flag in der Datenbank).
2. `SaveAsync()` ruft `PersistAsync()` auf (unveränderte Logik).

Beteiligte Klassen/Komponenten: `EndpointPage`, `EndpointQueryParameter`

---

### Ausführen einer HTTP-Anfrage

1. `EndpointPage.SendRequestAsync()` ruft `ExecutionService.ExecuteAsync()` auf.
2. `EndpointExecutionService.ExecuteAsync()` ruft intern `BuildRequest()` auf.
3. `BuildRequest()` iteriert über alle `endpoint.QueryParameters` und prüft für jeden Eintrag, ob `{Key}` als Platzhalter in `endpoint.RelativePath` vorkommt.
4. Für Einträge, deren Key als Platzhalter vorkommt: `{Key}` im Pfad wird durch `Uri.EscapeDataString(Value)` ersetzt.
5. Für alle übrigen Einträge (kein Platzhalter-Treffer): werden wie bisher als `key=value`-Paar im Query-String angehängt.
6. `BuildRequest()` gibt die zusammengesetzte `HttpRequestMessage` zurück.

Beteiligte Klassen/Komponenten: `EndpointExecutionService`, `Endpoint`, `EndpointQueryParameter`

---

## Neue Klassen

Keine neuen Klassen erforderlich.

---

## Änderungen an bestehenden Klassen

### `QueryParamEntry` (verschachtelte Klasse in `RequestQueryParamsPanel`)

- **Neue Eigenschaften:** `IsPathParameter` (`bool`, Default `false`) — steuert, ob der Löschen-Button für diesen Eintrag gerendert wird.

---

### `RequestQueryParamsPanel` (Razor-Komponente)

- **Geänderte Methoden:** `OnKeyChanged` — löst `OnChanged` zusätzlich per `@onblur` aus (bisher nur `@oninput`).
- **Geänderte Methoden:** `OnValueChanged` — löst `OnChanged` zusätzlich per `@onblur` aus (bisher nur `@oninput`).
- **Geändertes Rendering:** Löschen-Button wird nur gerendert, wenn `!param.IsPathParameter`.
- **Geändertes Rendering:** Sortierung der Einträge — Pfad-Platzhalter-Einträge (`IsPathParameter = true`) erscheinen vor regulären Einträgen.

---

### `EndpointPage` (Razor-Komponente, Code-Behind)

- **Neue Methoden:** `SyncPathParameters()` — wendet Regex `\{[^}]+\}` auf `_model.RelativePath` an; fügt fehlende Platzhalter als `QueryParamEntry` mit `IsPathParameter = true` am Listenanfang ein, erhält vorhandene Werte, entfernt Einträge zu weggefallenen Platzhaltern.
- **Neue Methoden:** `ExtractAndStripQueryString()` — prüft, ob `_model.RelativePath` ein `?` enthält; extrahiert bei Treffer den Query-String per `string.Split('?', 2)`, zerlegt ihn in Schlüssel-Wert-Paare, fügt fehlende als `QueryParamEntry` mit `IsPathParameter = false` hinzu (keine Duplikate), setzt `_model.RelativePath` auf den Anteil vor dem `?`.
- **Neue Methoden:** `OnPathBlur()` — `onblur`-Handler für das Pfad-Eingabefeld; ruft nacheinander `ExtractAndStripQueryString()`, `SyncPathParameters()` und `MarkDirty()` auf.
- **Neue Methoden:** `ResolveDisplayUrl()` — gibt die aufgelöste URL zurück: Platzhalter in `_model.RelativePath` werden durch die zugehörigen Werte der `IsPathParameter = true`-Einträge ersetzt (leerer Wert ergibt leeren String); Query-Parameter mit `IsPathParameter = false` werden als `?key=value`-Anhang ergänzt (ohne `?` wenn keine vorhanden).
- **Geänderte Methoden:** `LoadModelFromParameter()` — ruft nach dem Befüllen von `_queryParameters` zusätzlich `ExtractAndStripQueryString()` und anschließend `SyncPathParameters()` auf.
- **Geändertes Rendering:** Pfad-Eingabefeld bindet `value="@ResolveDisplayUrl()"` statt `value="@_model.RelativePath"` und registriert `@onblur="OnPathBlur"`.

---

### `EndpointExecutionService` (Logikklasse)

- **Geänderte Methoden:** `BuildRequest` — vor dem Anhängen des Query-Strings werden alle `endpoint.QueryParameters` auf Platzhalter-Treffer in `endpoint.RelativePath` geprüft; Treffer ersetzen `{Key}` im Pfad per `string.Replace` mit `Uri.EscapeDataString(Value)`; nicht treffende Einträge werden wie bisher als Query-String-Parameter angehängt.

---

## Datenbankmigrationen

Keine.

---

## Validierungsregeln

Keine.

---

## Konfigurationsänderungen

Keine.

---

## Seiteneffekte und Risiken

- **`EndpointPage` — Pfadfeld-Anzeige:** Das Pfad-Eingabefeld zeigt nach der Änderung die aufgelöste URL (Platzhalter ersetzt, Query-Parameter angehängt) statt des reinen Templates. Ein Anwender, der den Pfad manuell bearbeitet, sieht die aufgelöste URL und gibt ein neues Template ein — diese Diskrepanz ist potenziell verwirrend. Die Anforderung sieht dieses Verhalten ausdrücklich vor; eine Klärung ist unter Offenen Punkten aufgeführt.
- **`EndpointPage` — `LoadModelFromParameter()` verändert `_model.RelativePath`:** `ExtractAndStripQueryString()` schreibt beim Laden in `_model.RelativePath`, auch ohne Benutzeraktion. Wenn der gespeicherte Pfad einen Query-String enthält, wird dieser beim ersten Laden entfernt und `_isDirty` bleibt vorerst `false`. Erst beim nächsten Speichern wird der bereinigte Pfad persistiert. Dieses Verhalten ist in der Anforderung beschrieben und kein Fehler, muss jedoch beim Testen berücksichtigt werden.
- **`EndpointExecutionService.BuildRequest` — URL-Encoding bei Pfad-Platzhaltern:** Pfad-Platzhalter-Werte werden mit `Uri.EscapeDataString` kodiert, was für Pfad-Segmente semantisch korrekt ist (z. B. Leerzeichen → `%20`). Dieses Verhalten ist in der Anforderung impliziert; die Entscheidung ist unter Offenen Punkten vermerkt.
- **Bestehende Playwright-Tests:** `EndpointExecutionTests.ExecuteEndpoint_ReturnsSuccessResponse` verwendet einen Pfad ohne Platzhalter (`/api/application-groups`) und ist von der Änderung nicht betroffen.

---

## Umsetzungsreihenfolge

1. `QueryParamEntry.IsPathParameter` (`bool`-Eigenschaft) hinzufügen.
2. `RequestQueryParamsPanel` anpassen: Löschen-Button-Kondition, Sortierung, `onblur`-Trigger für `OnChanged`.
3. `EndpointPage` — `ResolveDisplayUrl()` implementieren.
4. `EndpointPage` — `ExtractAndStripQueryString()` implementieren.
5. `EndpointPage` — `SyncPathParameters()` implementieren.
6. `EndpointPage` — `OnPathBlur()` implementieren.
7. `EndpointPage` — `LoadModelFromParameter()` anpassen (Aufrufe von `ExtractAndStripQueryString()` und `SyncPathParameters()`).
8. `EndpointPage` — Pfad-Eingabefeld im Razor-Template anpassen (`ResolveDisplayUrl()`, `@onblur`).
9. `EndpointExecutionService.BuildRequest` anpassen (Platzhalter-Ersetzung vor Query-String-Aufbau).
10. `EndpointPageTests` — neue Testmethoden ergänzen.
11. `EndpointExecutionServiceTests` — neue Testmethode ergänzen.
12. `EndpointExecutionTests` (Playwright) — neues E2E-Szenario ergänzen.

---

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|---|---|---|
| `PfadMitPlatzhalter_WirdBeimLadenAlsNichtLoeschbarerEintragAngezeigt` | `EndpointPageTests` | Platzhalter in `RelativePath` erscheinen beim Laden als `IsPathParameter = true`-Einträge in `_queryParameters`. |
| `PfadMitPlatzhalter_VorhandenerWertBleibtErhalten_WennPlatzhalterUnveraendert` | `EndpointPageTests` | Beim erneuten Aufruf von `SyncPathParameters()` bleiben gespeicherte Werte für unveränderte Platzhalter erhalten. |
| `GeaenderterPfad_EntferntWeggefalleneUndFuegtNeueHinzu` | `EndpointPageTests` | Nach `OnPathBlur()` mit geändertem Pfad werden entfernte Platzhalter aus der Liste gelöscht und neue hinzugefügt. |
| `PfadMitQueryString_WirdExtrahiertUndPfadBereinigt` | `EndpointPageTests` | `ExtractAndStripQueryString()` trennt Query-String vom Pfad, fügt Einträge als `IsPathParameter = false` hinzu und bereinigt `_model.RelativePath`. |
| `AufgeloesteUrl_WirdImPfadfeldAngezeigt` | `EndpointPageTests` | `ResolveDisplayUrl()` gibt den Pfad mit ersetzten Platzhaltern und angehängten Query-Parametern zurück. |
| `BuildRequest_ErsetztPfadPlatzhalterDurchGespeicherteWerte` | `EndpointExecutionServiceTests` | Platzhalter in `RelativePath` werden durch den zugehörigen `QueryParameter.Value` ersetzt; fehlende Werte ergeben leere Strings. |
| `BuildRequest_HaengtNurNichtPlatzhalterParameterAlsQueryStringAn` | `EndpointExecutionServiceTests` | Parameter ohne Platzhalter-Treffer landen im Query-String, Platzhalter-Werte nicht. |
| E2E-Szenario: Endpunkt mit Platzhalter und Query-String | `EndpointExecutionTests` (Playwright) | Eingabe `/api/{id}?filter=active`, Prüfung: Pfad wird auf `/api/{id}` bereinigt, `filter=active` ist löschbarer Eintrag, `id` ist nicht löschbarer Eintrag, gesendete URL ist `/api/42?filter=active` (mit Wert `42`). |
| `CreateEndpoint(string relPath, QueryParamEntry[]?)` (Hilfsmethode, Erweiterung) | `EndpointPageTests` | Ermöglicht die Erstellung von Endpunkten mit konfigurierbarem `RelativePath` und optionalen `QueryParameters` für neue Testszenarien. |
| `CreateEndpoint(AuthenticationType, EndpointQueryParameter[]?)` (Hilfsmethode, Erweiterung) | `EndpointExecutionServiceTests` | Ermöglicht die Übergabe von `QueryParameters` an den Test-Endpunkt für Platzhalter-Szenarien. |

### Betroffene bestehende Tests

Keine.

---

## Offene Punkte

| # | Offener Punkt | Empfohlener Vorschlag |
|---|---|---|
| 1 | **`IsPathParameter` im Datenbankmodell:** Bei nachträglicher Umbenennung eines Platzhalters im Pfad kann ein gespeicherter Parameterwert nicht mehr eindeutig zugeordnet werden. Soll `EndpointQueryParameter` ein persistiertes `IsPathParameter`-Flag erhalten? | Kein persistiertes Flag. Die Unterscheidung wird zur Laufzeit aus dem Template abgeleitet (wie in der Anforderung beschrieben). Nicht zuordenbare Werte bleiben als reguläre Query-Parameter erhalten und können manuell gelöscht werden. |
| 2 | **Blur vs. Input für Pfadfeld:** Soll `_model.RelativePath` weiterhin per `oninput` (sofortige Dirty-Markierung) aktualisiert werden, und nur Platzhalter-Synchronisation und Anzeige per `onblur`? | `_model.RelativePath` wird weiterhin per `oninput` aktualisiert (`OnPathChanged` bleibt unverändert). `OnPathBlur()` übernimmt nur die Extraktion, Synchronisation und `MarkDirty()`-Aufruf — keine doppelte Dirty-Markierung, da `MarkDirty()` idempotent ist. |
| 3 | **Reihenfolge bei Extraktion bei Duplikaten:** Wenn beim Extrahieren eines Query-Strings ein Key bereits als manuell angelegter Parameter vorhanden ist, soll der extrahierte Wert den vorhandenen überschreiben oder soll ein Duplikat verhindert werden? | Duplikat verhindern: der vorhandene Eintrag bleibt unverändert. Der extrahierte Wert wird nur dann als neuer Eintrag hinzugefügt, wenn kein Eintrag mit demselben Key bereits vorhanden ist. |
| 4 | **Leere Parameternamen/-werte:** Sollen Einträge mit leerem Key beim Aufbau des Query-Strings und bei der Platzhalter-Ersetzung übersprungen werden? | Einträge mit leerem Key werden beim Aufbau des Query-Strings und bei der Platzhalter-Ersetzung übersprungen (entspricht gängiger Praxis und verhindert ungültige URLs). |
| 5 | **URL-Encoding für Pfad-Platzhalter:** Soll `Uri.EscapeDataString` auch für Pfad-Platzhalter-Ersetzungen in `BuildRequest` verwendet werden? | Ja — `Uri.EscapeDataString` für Platzhalter-Ersetzungen verwenden, konsistent zum bestehenden Query-String-Encoding. |
| 6 | **Anzeige im Pfadfeld bei Bearbeitung:** Das Pfadfeld zeigt die aufgelöste URL; der Anwender bearbeitet aber das Template. Soll ein separates schreibgeschütztes Vorschaufeld die aufgelöste URL anzeigen? | Kein separates Vorschaufeld. Das bestehende Eingabefeld zeigt die aufgelöste URL (wie in der Anforderung beschrieben). Sollte die Benutzerfreundlichkeit in der Praxis als unzureichend bewertet werden, kann ein Vorschaufeld in einem Folgeticket ergänzt werden. |
