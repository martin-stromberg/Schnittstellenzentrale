# Endpunkte — API

## Übersicht

Der `EndpointExecutionService` implementiert `IEndpointExecutionService` und stellt die Ausführungslogik für Endpunkte bereit. Die zentrale Methode `ExecuteAsync` baut intern über `BuildRequest` die HTTP-Anfrage zusammen, wobei Pfad-Platzhalter ersetzt und Query-Parameter angehängt werden.

---

## Methoden

### `ExecuteAsync`

**Beschreibung:** Führt einen Endpunkt aus und gibt ein `EndpointExecutionResult` mit Statusinformationen und Antwortdaten zurück.

**Parameter:**

| Name | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| `endpoint` | `Core.Models.Endpoint` | Ja | Der auszuführende Endpunkt; muss `Application` (mit `BaseUrl`) und `QueryParameters` enthalten. |

**Rückgabe:**

| Typ | Beschreibung |
|-----|--------------|
| `Task<EndpointExecutionResult>` | Ergebnis mit `Success`, `StatusCode`, `ResponseBody`, `ResponseHeaders`, `DurationMs`, `ResponseSizeBytes` und ggf. `ErrorMessage`. |

**URL-Aufbau durch `BuildRequest`:**

1. Alle `endpoint.QueryParameters` werden iteriert (leere Keys werden übersprungen).
2. Ist `{Key}` in `endpoint.RelativePath` enthalten: Platzhalter wird durch `Uri.EscapeDataString(Value)` ersetzt.
3. Nicht passende Parameter werden als `key=value`-Paar im Query-String gesammelt.
4. Finale URL: `baseUrl.TrimEnd('/') + "/" + resolvedPath.TrimStart('/')` + optionaler `?...`-Query-String.

**Fehler:**

| Situation | Verhalten |
|-----------|-----------|
| `endpoint.Application == null` | Gibt `EndpointExecutionResult { Success = false, ErrorMessage = "..." }` zurück, ohne HTTP-Anfrage. |
| Netzwerkfehler / Exception | Wird gefangen; `EndpointExecutionResult { Success = false, ErrorMessage = ex.Message }` wird zurückgegeben. |
| `OperationCanceledException` | Wird nicht gefangen, sondern weitergegeben. |

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
