# Endpunkte — API

## Übersicht

Der `EndpointExecutionService` implementiert `IEndpointExecutionService` und stellt die Ausführungslogik für Endpunkte bereit. Die zentrale Methode `ExecuteAsync` führt optionale Pre/Post-Request-Skripte aus, baut intern über `BuildRequest` die HTTP-Anfrage zusammen und gibt ein vollständiges `EndpointExecutionResult` zurück.

Das Interface `IEndpointScriptRunner` (implementiert durch `EndpointScriptRunner`) kapselt die JavaScript-Ausführung über die `Jint`-Engine.

---

## Methoden

### `IEndpointExecutionService.ExecuteAsync`

**Beschreibung:** Führt einen Endpunkt aus, inklusive Pre/Post-Request-Skripten, und gibt ein `EndpointExecutionResult` zurück.

**Parameter:**

| Name | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| `endpoint` | `Core.Models.Endpoint` | Ja | Der auszuführende Endpunkt; muss `Application` (mit `BaseUrl`) und `QueryParameters` enthalten. |

**Rückgabe:**

| Typ | Beschreibung |
|-----|--------------|
| `Task<EndpointExecutionResult>` | Ergebnis mit `Success`, `StatusCode`, `ResponseBody`, `ResponseHeaders`, `DurationMs`, `ResponseSizeBytes` und ggf. `ErrorMessage`. |

**Ausführungsreihenfolge:**

1. Pre-Request-Skript ausführen (falls vorhanden) — bei Fehler: Abbruch ohne HTTP-Anfrage.
2. `{{...}}`-Platzhalterauflösung in URL, Headern, Body via aktive Umgebungsvariablen.
3. HTTP-Anfrage senden.
4. Post-Request-Skript ausführen (falls vorhanden) — bei Fehler: `ErrorMessage` ergänzen, Ergebnis trotzdem zurückgeben.

**Fehler:**

| Situation | Verhalten |
|-----------|-----------|
| `endpoint.Application == null` | Gibt `EndpointExecutionResult { Success = false, ErrorMessage = "..." }` zurück, ohne HTTP-Anfrage. |
| Pre-Skript-Fehler | Gibt `EndpointExecutionResult { Success = false, ErrorMessage = "..." }` zurück; HTTP-Request wird nicht gesendet. |
| Netzwerkfehler / Exception | Wird gefangen; `EndpointExecutionResult { Success = false, ErrorMessage = ex.Message }` wird zurückgegeben. |
| Post-Skript-Fehler | `EndpointExecutionResult.ErrorMessage` wird gesetzt oder ergänzt; HTTP-Ergebnis bleibt erhalten. |
| Rekursionsschutz (`callDepth >= 2`) | Gibt `EndpointExecutionResult { Success = false, ErrorMessage = "..." }` zurück. |
| `OperationCanceledException` | Wird nicht gefangen, sondern weitergegeben. |

---

### `IEndpointScriptRunner.ExecuteAsync`

**Beschreibung:** Führt ein JavaScript-Skript im Jint-Interpreter aus und gibt das Ergebnis zurück.

**Parameter:**

| Name | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| `script` | `string` | Ja | Der auszuführende JavaScript-Quelltext. |
| `context` | `ScriptContext` | Ja | Kapselt Umgebungsservice, Request-Daten, optionale Response-Daten, den `ExecuteEndpoint`-Callback und den `CallDepth`-Zähler. |

**Rückgabe:**

| Typ | Beschreibung |
|-----|--------------|
| `Task<ScriptExecutionResult>` | `Success = true` bei Erfolg; `Success = false` und `ErrorMessage` bei Syntaxfehler, Runtime-Exception oder Timeout. |

**Das `sz`-Objekt im Skript:**

| Eigenschaft / Methode | Typ | Beschreibung |
|-----------------------|-----|--------------|
| `sz.environment.get(name)` | `string \| null` | Liest den Wert einer Umgebungsvariablen. |
| `sz.environment.set(name, value)` | `void` | Setzt eine Umgebungsvariable im Arbeitsspeicher (nicht persistiert). |
| `sz.request.url` | `string` | URL des Requests (vor Platzhalterauflösung im Pre-Skript). |
| `sz.request.method` | `string` | HTTP-Methode. |
| `sz.request.headers` | `object` | Request-Header als Schlüssel-Wert-Objekt. |
| `sz.request.body.raw` | `string \| null` | Body-Text. |
| `sz.request.body.asJson()` | `object \| null` | Body als geparste JSON-Struktur. |
| `sz.request.body.asXml()` | `object \| null` | Body als geparste XML-Struktur (verschachtelte Objekte). |
| `sz.response.body.raw` | `string \| null` | Antwort-Body (nur im Post-Skript). |
| `sz.response.body.asJson()` | `object \| null` | Antwort-Body als JSON (nur im Post-Skript). |
| `sz.response.body.asXml()` | `object \| null` | Antwort-Body als XML (nur im Post-Skript). |
| `sz.response.headers` | `object` | Antwort-Header (nur im Post-Skript). |
| `sz.execute(name)` | `object` | Führt den Endpunkt mit dem angegebenen Namen in der gleichen Anwendung aus; gibt `{ success, statusCode, responseBody, errorMessage }` zurück. |

---

## Komponenten-Events

### `RequestQueryParamsPanel.OnChanged`

**Beschreibung:** Wird ausgelöst, wenn ein Name- oder Wert-Eingabefeld verlassen wird (`@onchange`) oder wenn ein Parameter hinzugefügt oder gelöscht wird. `EndpointPage` empfängt dieses Event und rendert neu, damit `ResolveDisplayUrl()` das Pfadfeld aktualisiert.

**Parameter:** keine

**Auslöser:**
- `OnFieldChanged` bei `@onchange` auf Name- oder Wert-Input
- `AddParam` nach Hinzufügen eines neuen Eintrags
- `RemoveParam` nach Löschen eines Eintrags

---

## Datenklassen

### `RequestQueryParamsPanel.QueryParamEntry`

In-Memory-Klasse ohne Datenbankentsprechung.

| Eigenschaft | Typ | Standardwert | Beschreibung |
|-------------|-----|--------------|--------------|
| `Key` | `string` | `""` | Parametername |
| `Value` | `string` | `""` | Parameterwert |
| `IsPathParameter` | `bool` | `false` | `true` für Pfad-Platzhalter; steuert Sichtbarkeit des Löschen-Buttons und Sortierreihenfolge |

### `EndpointQueryParameter`

Datenbankentität (keine `IsPathParameter`-Spalte — die Unterscheidung wird zur Laufzeit aus dem Template abgeleitet).

| Eigenschaft | Typ | Beschreibung |
|-------------|-----|--------------|
| `Key` | `string` | Parametername |
| `Value` | `string` | Parameterwert |
| `EndpointId` | `int` | Fremdschlüssel zum Endpunkt |
