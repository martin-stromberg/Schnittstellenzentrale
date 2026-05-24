# Endpunkte — Business Rules

## Platzhalter-Erkennung aus dem Pfad-Template

**Beschreibung:** Die Unterscheidung zwischen Pfad-Platzhaltern und regulären Query-Parametern wird nicht in der Datenbank gespeichert, sondern beim Laden und bei jeder Pfad-Bearbeitung dynamisch aus dem Template `RelativePath` abgeleitet.

**Bedingungen:**
- `RelativePath` enthält Ausdrücke der Form `{name}` (Regex: `\{([^}]+)\}`).

**Verhalten:**
- Wenn `{name}` im Pfad vorkommt: der zugehörige `QueryParamEntry` erhält `IsPathParameter = true` und kann nicht gelöscht werden.
- Wenn `{name}` nicht mehr im Pfad vorkommt: der zugehörige Eintrag mit `IsPathParameter = true` wird aus der Liste entfernt.
- Wird ein Platzhalter umbenannt, verbleibt der alte Parameterwert als regulärer (löschbarer) Query-Parameter in der Liste.

**Umsetzung:** `EndpointPage.SyncPathParameters()` — die dynamische Ableitung vermeidet ein persistiertes Flag und hält das Datenbankmodell (`EndpointQueryParameter`) frei von UI-spezifischen Metadaten.

---

## Keine Duplikate bei Query-String-Extraktion

**Beschreibung:** Wenn ein extrahierter Query-String-Key bereits als Parameter in der Liste vorhanden ist, wird kein Duplikat erzeugt.

**Bedingungen:**
- `_model.RelativePath` enthält ein `?`.
- Mindestens ein extrahierter Key ist bereits in `_queryParameters` vorhanden.

**Verhalten:**
- Wenn Key bereits vorhanden: vorhandener Eintrag bleibt unverändert; der extrahierte Wert wird verworfen.
- Wenn Key nicht vorhanden: neuer Eintrag mit `IsPathParameter = false` wird hinzugefügt.

**Umsetzung:** `EndpointPage.ExtractAndStripQueryString()` — der vorhandene Wert hat Vorrang, damit manuell eingetragene Werte nicht durch eine erneute Eingabe überschrieben werden.

---

## Leere Keys werden übersprungen

**Beschreibung:** Parameter mit leerem Key werden weder beim Aufbau des Query-Strings noch bei der Platzhalter-Ersetzung berücksichtigt.

**Bedingungen:**
- `string.IsNullOrWhiteSpace(param.Key)` ergibt `true`.

**Verhalten:**
- Der Eintrag wird in `BuildRequest()` vollständig ignoriert (kein Platzhalter-Ersatz, kein Query-String-Eintrag).
- In `ResolveDisplayUrl()` werden Einträge mit leerem Key ebenfalls übersprungen.

**Umsetzung:** `EndpointExecutionService.BuildRequest()` und `EndpointPage.ResolveDisplayUrl()` — verhindert ungültige URLs mit leeren `key=value`-Paaren.

---

## Pfad-Platzhalter-Werte werden URL-kodiert

**Beschreibung:** Werte, die Pfad-Platzhalter ersetzen, werden mit `Uri.EscapeDataString` kodiert — konsistent zum bestehenden Query-String-Encoding.

**Bedingungen:**
- Ein `QueryParameter.Key` entspricht einem `{Key}`-Platzhalter in `RelativePath`.

**Verhalten:**
- Der Wert wird vor dem Einsetzen in den Pfad via `Uri.EscapeDataString(Value)` kodiert.
- Leerzeichen werden zu `%20`, andere Sonderzeichen entsprechend kodiert.

**Umsetzung:** `EndpointExecutionService.BuildRequest()` — `Uri.EscapeDataString` ist für Pfad-Segmente semantisch korrekt (entspricht Prozent-Kodierung gemäß RFC 3986).

---

## Sortierung: Pfad-Platzhalter vor regulären Parametern

**Beschreibung:** Einträge mit `IsPathParameter = true` erscheinen in der Parameter-Liste immer vor den regulären Query-Parametern.

**Umsetzung:** `RequestQueryParamsPanel` — Sortierung via `QueryParameters.OrderByDescending(p => p.IsPathParameter)` im Razor-Template.

---

## RelativePath speichert ausschließlich das Pfad-Template

**Beschreibung:** In der Datenbank wird in `Endpoint.RelativePath` ausschließlich der Pfad-Anteil ohne Query-String gespeichert.

**Bedingungen:**
- Der Anwender gibt einen Pfad mit Query-String ein.

**Verhalten:**
- `ExtractAndStripQueryString()` bereinigt `RelativePath` beim Laden oder bei `onblur`.
- Beim Speichern wird der bereits bereinigte `_model.RelativePath` persistiert.
- Ein Endpunkt mit gespeichertem Query-String im Pfad wird beim nächsten Laden automatisch bereinigt; `_isDirty` wird dabei nicht gesetzt (keine Benutzeraktion nötig zum Speichern der Bereinigung).

**Umsetzung:** `EndpointPage.LoadModelFromParameter()` und `EndpointPage.OnPathBlur()`.
