# REST-API — Business Rules

## Token-Rotation nach jedem Datenendpunkt-Aufruf

**Beschreibung:** Jeder Token darf nur einmal für einen Datenendpunkt-Aufruf verwendet werden. Nach dem Aufruf wird der alte Token invalidiert und ein neuer Token ausgegeben. Damit wird verhindert, dass ein abgefangenes Token mehrfach verwendet werden kann.

**Bedingungen:** Betrifft ausschließlich die Datenendpunkte `POST /api/application-groups` und `POST /api/applications`; `/authenticate` ist davon ausgenommen.

**Verhalten:**
- Gültiger Token: alter Token wird aus `_tokens` entfernt, neuer Token erstellt und im Header `X-New-Token` zurückgegeben.
- Ungültiger oder abgelaufener Token: `401 Unauthorized`; kein neuer Token wird ausgestellt.

**Umsetzung:** `TokenStore.ValidateAndRotateAsync` — verwendet `ConcurrentDictionary.TryRemove`, um atomares Entfernen sicherzustellen; anschließend Ablaufzeitprüfung.

---

## Token-Ablaufzeit: 5 Minuten

**Beschreibung:** Tokens sind auf 5 Minuten Gültigkeit beschränkt, um das Risiko bei einem Token-Verlust zu minimieren.

**Verhalten:**
- `TokenStore.CreateTokenAsync`: `ExpiresAt = DateTime.UtcNow + 5 min` (konfigurierbar über den Konstruktor-Parameter `tokenLifetime`).
- `TokenStore.ValidateAndRotateAsync`: Prüft `existingToken.ExpiresAt <= DateTime.UtcNow`; abgelaufene Tokens werden wie unbekannte behandelt.

**Umsetzung:** `TokenStore` — `_tokenLifetime` ist über den Sekundärkonstruktor `TokenStore(TimeSpan tokenLifetime)` anpassbar; Produktionscode verwendet den Standardkonstruktor mit 5 Minuten.

---

## Automatische Bereinigung abgelaufener Tokens

**Beschreibung:** `TokenStore` bereinigt abgelaufene Tokens bei jedem Zugriff, um Speicherlecks bei hohem Aufrufvolumen zu vermeiden.

**Verhalten:** `RemoveExpiredTokens()` wird am Anfang von `CreateTokenAsync` und `ValidateAndRotateAsync` aufgerufen; es iteriert über alle Keys des `ConcurrentDictionary` und entfernt abgelaufene Einträge via `TryRemove`.

**Umsetzung:** `TokenStore.RemoveExpiredTokens`.

---

## Retry bei 401 im ApplicationApiClient

**Beschreibung:** Wenn ein Token zwischen `/authenticate` und dem ersten Datenendpunkt-Aufruf abläuft (z. B. bei sehr langer Ladezeit im Browser), antwortet der Datenendpunkt mit `401`. Der `ApplicationApiClient` behandelt diesen Fall durch eine einmalige Wiederholung.

**Verhalten:**
- `401`-Response: `_currentToken = null`, erneuter Aufruf von `EnsureTokenAsync()` (→ neues Token), Wiederholung des ursprünglichen Requests.
- Schlägt auch der zweite Versuch mit `401` fehl: `response.EnsureSuccessStatusCode()` wirft `HttpRequestException`.

**Umsetzung:** `ApplicationApiClient.SendWithTokenAsync<TResponse>`.

---

## StorageMode-Übergabe via X-Storage-Mode-Header

**Beschreibung:** Die Blazor-Komponenten kennen den aktuellen `StorageMode` (`Team`/`User`), die Controller nicht. Der Modus wird deshalb als HTTP-Header übertragen, damit der Controller entscheiden kann, ob eine SignalR-Benachrichtigung ausgelöst werden soll.

**Verhalten:**
- Header `X-Storage-Mode: Team` → `StorageMode.Team` → nach erfolgreichem Speichern wird `ISignalRNotificationService.NotifyGroupChangedAsync`, `NotifyApplicationChangedAsync`, `NotifyEndpointGroupChangedAsync` bzw. `NotifyEndpointChangedAsync` aufgerufen.
- Header fehlt oder hat anderen Wert → `StorageMode.User` → keine SignalR-Benachrichtigung.

**Umsetzung:** `ApiControllerBase.ParseStorageMode` — Vergleich des Header-Werts mit dem Literal `"Team"`.

---

## Token-Rotation betrifft alle Datenendpunkte

**Beschreibung:** Die Token-Rotation gilt für alle Controller-Endpunkte unter `/api/*`, einschließlich der neuen Endpunkte für Endpunktgruppen (`/api/endpoint-groups`) und Endpunkte (`/api/endpoints`). Jeder erfolgreiche Aufruf invalidiert das verwendete Token und gibt einen neuen Token im `X-New-Token`-Header zurück.

**Umsetzung:** `ApiControllerBase.ParseRequestContextAsync` — gemeinsame Basisklasse für alle Controller.

---

## Atomare Header- und Query-Parameter-Operationen

**Beschreibung:** Header und Query-Parameter eines Endpunkts werden einzeln über dedizierte Routen hinzugefügt und gelöscht (`POST /api/endpoints/headers`, `DELETE /api/endpoints/headers/{id}`, `POST /api/endpoints/query-parameters`, `DELETE /api/endpoints/query-parameters/{id}`). Es ist nicht nötig, den gesamten Endpunkt für diese Änderungen erneut zu senden.

**Verhalten:**
- `POST /api/endpoints/headers`: legt genau einen `EndpointHeader` an; gibt `201 Created` mit `EndpointHeaderResponse` zurück.
- `DELETE /api/endpoints/headers/{id}`: löscht genau einen `EndpointHeader` per ID; gibt `204 No Content` zurück.
- Analog für `QueryParameter`.
- Kein `404` bei `DELETE`-Aufrufen, wenn die ID nicht existiert — die Datenbankoperation schlägt dann intern fehl.

**Umsetzung:** `EndpointsController.AddHeaderAsync`, `DeleteHeaderAsync`, `AddQueryParameterAsync`, `DeleteQueryParameterAsync` — direkte Delegation an `IEndpointRepository`.

---

## OData-PATCH erfordert RowVersion (Optimistic Concurrency)

**Beschreibung:** Alle vier PATCH-Endpunkte der OData-API erfordern `rowVersion` im Request-Body, um gleichzeitige Änderungen von mehreren Clients zu erkennen und zu verhindern, dass Änderungen unbemerkt überschrieben werden.

**Bedingungen:** Betrifft `PATCH /odatav4/Applications({key})`, `PATCH /odatav4/ApplicationGroups({key})`, `PATCH /odatav4/Endpoints({key})` und `PATCH /odatav4/EndpointGroups({key})`.

**Verhalten:**
- `rowVersion` fehlt im Body: `400 Bad Request`.
- `rowVersion` ist vorhanden, stimmt aber nicht mit dem gespeicherten Wert überein (Concurrency-Konflikt): `409 Conflict`.
- `rowVersion` stimmt überein: Änderungen werden übernommen, Response `200 OK` mit dem aktualisierten Objekt.

**Umsetzung:** `ODataPatchHelper.ContainsRowVersion` / `TryExtractRowVersion` — Prüfung vor der Datenbankoperation; `DbUpdateConcurrencyException` wird in den OData-Controllern abgefangen und in `409 Conflict` überführt.

---

## Thread-Sicherheit im TokenStore

**Beschreibung:** `TokenStore` ist als Singleton registriert und wird unter gleichzeitigen Requests zugreifbar. Das interne Dictionary muss daher thread-sicher sein.

**Umsetzung:** `ConcurrentDictionary<string, AuthToken>` — alle Lese- und Schreiboperationen (`TryRemove`, Indexer-Zuweisung) sind atomar. `RemoveExpiredTokens` iteriert über `_tokens.Keys` (Snapshot) und nutzt `TryRemove`, sodass konkurrierende Zugriffe keine Ausnahme verursachen.
