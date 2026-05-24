# Anforderung: URL-Platzhalter und Query-Parameter

## Fachliche Zusammenfassung

`EndpointPage` und `RequestQueryParamsPanel` werden um die Unterscheidung zwischen Pfad-Platzhaltern (Path Parameters, nicht löschbar) und regulären Query-Parametern (löschbar) erweitert. Beim Laden und beim Verlassen des Pfadfelds (`onblur`) wird `Endpoint.RelativePath` per Regex auf Platzhalter der Form `{name}` analysiert; gefundene Platzhalter erscheinen als nicht löschbare Einträge im Query-Parameter-Tab. Enthält `RelativePath` beim Laden oder beim Bearbeiten einen Query-String, wird dieser automatisch extrahiert, als löschbare Einträge in den Tab übernommen und der gespeicherte Pfad auf den reinen Pfad-Anteil reduziert. Das Pfad-Eingabefeld zeigt stets die aufgelöste URL (Platzhalter durch Wert ersetzt, Query-Parameter als `?key=value`-Anhang), aktualisiert sich `onblur` und wird zur Laufzeit für die HTTP-Anfrage in `EndpointExecutionService.BuildRequest` verwendet. Alle Parameterwerte werden zusammen mit dem Endpunkt in `EndpointQueryParameter`-Datensätzen persistiert; `Endpoint.RelativePath` speichert ausschließlich das Template ohne Query-String.

---

## Betroffene Klassen und Komponenten

### Datenmodellklassen — keine Änderung erforderlich (`Schnittstellenzentrale.Core`)

| Klasse | Bemerkung |
|---|---|
| `EndpointQueryParameter` | Felder `Key`, `Value`, `EndpointId` sind ausreichend. Pfad-Platzhalter-Werte und reguläre Query-Parameter werden als gleichartige `EndpointQueryParameter`-Einträge gespeichert. |
| `Endpoint` | `RelativePath` speichert weiterhin ausschließlich das Pfad-Template (z. B. `/api/applications/{id}`). Keine neuen Felder. |

### UI-Hilfstypen — zu erweitern (`Schnittstellenzentrale`)

| Typ | Änderung |
|---|---|
| `RequestQueryParamsPanel.QueryParamEntry` | Neues Feld `IsPathParameter` (`bool`, Default `false`). Steuert, ob der Löschen-Button für diesen Eintrag angezeigt wird. |

### UI-Komponenten — zu erweitern (`Schnittstellenzentrale`)

| Komponente | Änderung |
|---|---|
| `RequestQueryParamsPanel` | Löschen-Button wird nur gerendert, wenn `!param.IsPathParameter`. Sortierung: Pfad-Platzhalter-Einträge zuerst, dann löschbare Query-Parameter. Der `onblur`-Event auf den Eingabefeldern (Name und Wert) löst `OnChanged` aus (bisher nur `oninput`). |
| `EndpointPage` | Neue private Methoden `SyncPathParameters()` und `ExtractAndStripQueryString()`. Beide werden in `LoadModelFromParameter()` und `OnPathBlur()` aufgerufen. `OnPathBlur()` ist ein neuer `onblur`-Handler für das Pfad-Eingabefeld. Das Pfadfeld rendert eine aufgelöste URL (`ResolveDisplayUrl()`) als Anzeigewert statt `_model.RelativePath` direkt. `SendRequestAsync` und `EndpointExecutionService.BuildRequest` verwenden bereits `endpoint.QueryParameters` und `endpoint.RelativePath`; nach der Umstellung arbeiten sie mit dem Template-Pfad und den gespeicherten Parametern — keine Änderung an der Ausführungslogik erforderlich, da `BuildRequest` den Query-String bereits aus `QueryParameters` aufbaut und `RelativePath` nur als Pfad-Anteil verwendet. |

### Logikklassen / Services — zu prüfen (`Schnittstellenzentrale.Infrastructure`)

| Klasse | Änderung |
|---|---|
| `EndpointExecutionService.BuildRequest` | Pfad-Platzhalter müssen vor dem Zusammensetzen der URL durch ihren Wert ersetzt werden. Aktuell wird `RelativePath` unverändert übernommen. Neue Logik: Werte aus `endpoint.QueryParameters`, deren `Key` einem Platzhalter im Template entspricht, werden per `string.Replace("{key}", value)` eingesetzt; restliche Parameter werden wie bisher als Query-String angehängt. *Annahme: Die Unterscheidung Pfad-Parameter vs. Query-Parameter wird zur Laufzeit anhand des Template-Inhalts ermittelt — kein neues Flag im Datenbankmodell erforderlich.* |

### Tests — zu erweitern/neu zu erstellen (`Schnittstellenzentrale.Tests`)

| Artefakt | Beschreibung |
|---|---|
| `EndpointPageTests` (Erweiterung) | Szenarien: (1) Pfad-Platzhalter werden beim Laden als nicht löschbare Einträge angezeigt. (2) Bestehende Werte bleiben erhalten, wenn Platzhalter sich nicht ändern. (3) Geänderter Pfad entfernt weggefallene Platzhalter und fügt neue hinzu. (4) Query-String im Pfadfeld wird extrahiert und Pfad wird bereinigt. (5) Aufgelöste URL wird im Pfadfeld angezeigt. |
| `EndpointExecutionServiceTests` (Erweiterung) | Szenario: Pfad-Platzhalter in `RelativePath` werden bei `BuildRequest` durch den gespeicherten Parameterwert ersetzt; fehlende Werte ergeben leere Strings. |
| Playwright-Tests (`EndpointExecutionTests`) | E2E-Szenario: Endpunkt mit `/api/{id}?filter=active` eingeben, prüfen, dass Pfad auf `/api/{id}` bereinigt wird, `filter=active` als löschbarer Eintrag erscheint, `id` als nicht löschbarer Eintrag erscheint und die gesendete URL `/api/42?filter=active` ist (mit eingetragenem Wert `42`). |

---

## Implementierungsansatz

### Pfad-Platzhalter-Erkennung und Synchronisation (`EndpointPage`)

`SyncPathParameters()` wendet den Regex `\{[^}]+\}` auf `_model.RelativePath` an. Für jeden gefundenen Platzhalternamen wird geprüft, ob ein `QueryParamEntry` mit `IsPathParameter = true` und diesem `Key` bereits in `_queryParameters` vorhanden ist. Ist er vorhanden, bleibt sein `Value` erhalten. Fehlt er, wird er an den Anfang der Liste eingefügt. Nicht mehr im Pfad vorhandene Platzhalter-Einträge (`IsPathParameter = true`) werden entfernt.

### Query-String-Extraktion (`EndpointPage`)

`ExtractAndStripQueryString()` prüft, ob `_model.RelativePath` ein `?` enthält. Falls ja, wird der Query-String per `Uri.TryCreate` oder einfachem `string.Split('?', 2)` extrahiert, in Schlüssel-Wert-Paare zerlegt und als neue `QueryParamEntry`-Instanzen mit `IsPathParameter = false` in `_queryParameters` aufgenommen — ohne Duplikate zu vorhandenen Einträgen. `_model.RelativePath` wird auf den Anteil vor dem `?` gesetzt.

### Aufgelöste URL-Anzeige (`EndpointPage`)

`ResolveDisplayUrl()` gibt eine Zeichenkette zurück, in der alle `{name}`-Platzhalter in `_model.RelativePath` durch die zugehörigen `Value`-Felder der entsprechenden Pfad-Platzhalter-Einträge ersetzt sind (leerer Wert → leerer String). Anschließend werden alle Query-Parameter mit `IsPathParameter = false` als `?key=value`-Anhang ergänzt; der `?` entfällt, wenn keine solchen Parameter vorhanden sind. Das Pfad-Eingabefeld bindet auf diesen Wert: `value="@ResolveDisplayUrl()"`.

### Blur-Handler (`EndpointPage`)

`OnPathBlur()` ruft nacheinander `ExtractAndStripQueryString()`, `SyncPathParameters()` und `MarkDirty()` auf. Er wird per `@onblur="OnPathBlur"` am Pfad-Eingabefeld registriert. `RequestQueryParamsPanel` löst `OnChanged` zusätzlich via `onblur` aus, damit `EndpointPage` die Anzeige aktualisieren kann.

### Persistierung

Beim Speichern (`SaveAsync`) wird `_model.QueryParameters` aus `_queryParameters` aufgebaut — identisch zur bisherigen Logik. Pfad-Platzhalter-Werte und reguläre Query-Parameter werden als gleichartige `EndpointQueryParameter`-Einträge gespeichert (kein `IsPathParameter`-Flag in der Datenbank). Beim Laden wird `SyncPathParameters()` erneut ausgeführt, um die `IsPathParameter`-Markierung aus dem Template abzuleiten.

### Ausführungslogik (`EndpointExecutionService.BuildRequest`)

Die bestehende Methode baut die URL als `baseUrl + "/" + relPath`. Nach der Anpassung wird `relPath` zunächst durch Iteration über alle `QueryParameters`, deren `Key` als `{key}` im Template vorkommt, aufgelöst (Platzhalter ersetzen). Die verbleibenden Parameter (kein Treffer im Template) werden wie bisher als Query-String angehängt. *Annahme: Die Unterscheidung erfolgt rein dynamisch per Template-Analyse ohne persistiertes Flag.*

---

## Konfiguration

Kein zusätzlicher Konfigurationsbedarf. Das Feature arbeitet ausschließlich mit den bereits vorhandenen Datenstrukturen (`Endpoint.RelativePath`, `EndpointQueryParameter`) und erfordert keine neue Datenbankmigration.

---

## Offene Fragen

1. **`IsPathParameter` im Datenbankmodell:** Aktuell wird vorgeschlagen, die Unterscheidung zwischen Pfad-Platzhalter-Wert und Query-Parameter zur Laufzeit aus dem Template abzuleiten (kein persistiertes Flag). Falls ein Anwender einen Platzhalternamen nachträglich im Pfad ändert, kann ein bereits gespeicherter Parameter nicht mehr eindeutig zugeordnet werden. Soll `EndpointQueryParameter` ein Feld `IsPathParameter` (`bool`) erhalten, das persistiert wird und eine Migration erfordert?

2. **Blur vs. Input für Pfadfeld:** Die Anforderung schreibt `onblur` für die URL-Auflösung vor. Soll `_model.RelativePath` weiterhin per `oninput` (sofortige Dirty-Markierung) aktualisiert werden, und nur die Platzhalter-Synchronisation und Anzeige-Auflösung per `onblur`? Oder wird der interne Modellwert ebenfalls erst bei `onblur` übernommen?

3. **Reihenfolge der Query-Parameter nach Extraktion:** Wenn eine URL `?a=1&b=2` enthält und bereits ein manuell angelegter Parameter `b` vorhanden ist, soll der extrahierte Wert den vorhandenen Wert überschreiben oder soll ein Duplikat verhindert werden?

4. **Leere Parameternamen und -werte:** Die Anforderung besagt, dass auch Parameter mit leerem Namen oder leerem Wert in die URL übernommen werden. Soll dies auch für Pfad-Platzhalter gelten (d. h. `{id}` → `?`-loser Ersatz durch leer), oder soll bei leerem Parameternamen der Eintrag beim Aufbau des Query-Strings übersprungen werden?

5. **URL-Encoding der Parameterwerte:** `EndpointExecutionService.BuildRequest` verwendet bereits `Uri.EscapeDataString` für Query-Parameter. Soll dasselbe Encoding für Pfad-Platzhalter-Ersetzungen gelten (d. h. Leerzeichen in einem `id`-Wert werden URL-encodiert im Pfad)?

6. **Anzeige im Pfadfeld bei Bearbeitung:** Das Pfad-Eingabefeld zeigt die aufgelöste URL. Wenn der Anwender den Wert im Feld editiert (z. B. einen neuen Pfad eingibt), sieht er eine aufgelöste URL, speichert aber das Template. Ist dieses Verhalten für den Anwender verständlich genug, oder soll das Eingabefeld den Rohwert (Template) zeigen und nur ein separates, schreibgeschütztes Vorschaufeld die aufgelöste URL darstellen?
