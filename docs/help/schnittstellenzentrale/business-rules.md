# Schnittstellenzentrale — Business Rules

## StorageMode-Filterung

**Beschreibung:** Welche Datensätze ein Benutzer sieht, hängt vom aktiven Speichermodus ab.

**Bedingungen:**
- `StorageModeService.CurrentMode == StorageMode.Team`
- `StorageModeService.CurrentMode == StorageMode.User`

**Verhalten:**
- Wenn `Team`: Alle `Application`-Einträge werden zurückgegeben, unabhängig von `Owner`.
- Wenn `User`: Nur Einträge, bei denen `Application.Owner == WindowsCurrentUserService.GetCurrentUserName()`, werden zurückgegeben.

**Umsetzung:** `ApplicationRepository.GetApplicationsAsync`, `GetGroupsAsync`, `GetUngroupedApplicationsAsync` — EF-Core-LINQ-Filter auf `Owner`-Feld.

---

## Optimistic Concurrency (RowVersion)

**Beschreibung:** Gleichzeitige Schreibzugriffe mehrerer Benutzer auf dieselbe Entität werden erkannt und dem Benutzer gemeldet.

**Bedingungen:**
- Alle Entitäten mit Schreibzugriff (`Application`, `ApplicationGroup`, `EndpointGroup`, `Endpoint`) tragen ein `RowVersion`-Feld.
- Das `RowVersion`-Feld wird bei jeder Schreiboperation in `AppDbContext.SaveChangesAsync` durch eine neue GUID ersetzt.
- EF Core erkennt einen Konflikt, wenn beim Update das gespeicherte `RowVersion` nicht mit dem in der Datenbank übereinstimmt.

**Verhalten:**
- Wenn kein Konflikt: Speichern erfolgreich, `RowVersion` wird aktualisiert.
- Wenn Konflikt: `DbUpdateConcurrencyException` wird geworfen → `ConcurrencyWarningDialog` erscheint.
- Wenn Benutzer „Überschreiben (Force-Save)" wählt: Aktuelle `RowVersion` wird aus der Datenbank neu geladen (`GetEndpointByIdAsync`), dann wird erneut gespeichert.

**Umsetzung:** `AppDbContext.UpdateRowVersions()` (privat in `SaveChangesAsync`), `EndpointEditor.SaveAsync` und `ForceSaveAsync`, `ImportDialog.ApplyAsync`.

---

## Health-Check-Cooldown

**Beschreibung:** Verhindert, dass ein Health-Check für dieselbe Anwendung innerhalb eines kurzen Zeitraums mehrfach ausgelöst wird, um die Zielanwendung nicht mit Anfragen zu überlasten.

**Bedingungen:**
- `HealthCheckService` hält ein In-Memory-Dictionary `_lastCheckTimes` mit `applicationId → DateTime`.
- Konfigurierter Schwellenwert: `HealthCheck:CooldownSeconds` (Standard: 60).

**Verhalten:**
- Wenn seit dem letzten Check weniger als `CooldownSeconds` Sekunden vergangen sind: `CheckAsync` gibt `null` zurück, ohne eine HTTP-Anfrage zu senden.
- Wenn der Cooldown abgelaufen ist oder kein vorheriger Check existiert: HTTP-Anfrage wird gesendet; Ergebnis (`true`/`false`) wird zurückgegeben.

**Umsetzung:** `HealthCheckService.CheckAsync` — Thread-sicher durch `lock (_lock)` auf das Dictionary.

---

## Verbindungsfehler löst Health-Check aus

**Beschreibung:** Wenn ein Endpunkt aufgerufen wird und keine HTTP-Antwort eingeht (Verbindungsfehler), wird automatisch die Erreichbarkeit der zugehörigen Anwendung geprüft.

**Bedingungen:**
- `EndpointExecutionResult.Success == false`
- `EndpointExecutionResult.StatusCode == null` (kein HTTP-Status erhalten)

**Verhalten:**
- Wenn Verbindungsfehler erkannt: `HealthCheckService.CheckAsync(Endpoint.Application)` wird aufgerufen und `HealthCheckDialog` angezeigt.
- Wenn kein Verbindungsfehler (HTTP-Fehlercode vorhanden): Kein Health-Check.

**Umsetzung:** `EndpointExecutionPanel.ExecuteAsync` — `IsConnectionError`-Hilfsmethode prüft `StatusCode == null`.

---

## Selektiver Endpunktabgleich beim App-Start

**Beschreibung:** Beim Start der Anwendung werden die in der Datenbank gespeicherten Endpunkte der Systemanwendung automatisch mit der eigenen Swagger-Definition abgeglichen — ohne manuell konfigurierten Felder zu überschreiben.

**Bedingungen:**
- Die Systemgruppe und -anwendung müssen in der Datenbank vorhanden sein (angelegt durch `SystemEntryInitializer`).
- Die Swagger-Definition muss unter `Application.InterfaceUrl` abrufbar sein.

**Verhalten:**
- Wenn ein Endpunkt nur in der Swagger-Definition vorhanden ist (neuer Endpunkt): wird via `AddEndpointAsync` angelegt.
- Wenn ein Endpunkt nur in der Datenbank vorhanden ist (entfernter Endpunkt): wird via `DeleteEndpointAsync` gelöscht.
- Wenn ein Endpunkt in beiden vorhanden ist, aber `Name` unterschiedlich ist (geänderter Name): wird via `UpdateEndpointNameAsync` nur der `Name` aktualisiert; `AuthenticationType`, `Body`, `Headers` und `QueryParameters` bleiben erhalten.
- Wenn ein Endpunkt in beiden vorhanden ist und `Name` gleich ist: kein Eingriff.
- Wenn `GetSystemGroupAsync()` `null` zurückgibt oder keine Systemanwendung gefunden wird: Abgleich wird übersprungen, Warnung wird geloggt.
- Wenn die Swagger-Definition nicht abrufbar ist (`diff.ErrorMessage != null`) oder eine unerwartete Exception auftritt: Fehler wird geloggt, die Anwendung startet trotzdem normal.

**Umsetzung:** `SystemEndpointSyncService.ExecuteAsync` — verwendet `ISwaggerImportService.ImportAsync` für den Diff und ruft selektiv `IEndpointRepository.AddEndpointAsync`, `DeleteEndpointAsync` und `UpdateEndpointNameAsync` auf; `ApplyDiffAsync` wird bewusst nicht verwendet, da es auch `ChangedEndpoints` vollständig überschreiben würde.

---

## Import-Diff-Berechnung

**Beschreibung:** Beim Swagger- oder OData-Import wird der Unterschied zwischen importierten und bestehenden Endpunkten berechnet, bevor Änderungen in die Datenbank geschrieben werden.

**Bedingungen:**
- Schlüssel für den Vergleich: `{HttpMethod}:{RelativePath}` (z. B. `GET:/api/orders`)
- Namensvergleich entscheidet über „geändert"

**Verhalten:**
- Wenn Schlüssel nur in importiert: → `ImportDiff.NewEndpoints`
- Wenn Schlüssel in beiden, aber `Name` unterschiedlich: → `ImportDiff.ChangedEndpoints`
- Wenn Schlüssel nur in bestehend: → `ImportDiff.RemovedEndpoints`
- Wenn Schlüssel in beiden und `Name` gleich: kein Eintrag im Diff

**Umsetzung:** `ImportDiffCalculator.Calculate` (intern in `Schnittstellenzentrale.Infrastructure`).

---

## Authentifizierungs-Strategie-Auswahl

**Beschreibung:** Die Authentifizierungsstrategie für einen HTTP-Request wird ausschließlich durch den gespeicherten `AuthenticationType` des Endpunkts bestimmt.

**Verhalten:**

| `AuthenticationType` | Strategie |
|----------------------|-----------|
| `None` | Standard-HttpClient, kein Authentifizierungs-Header |
| `Basic` | `Authorization: Basic {Base64(username:password)}` — Credentials aus Windows Credential Manager |
| `Negotiate` | Named HttpClient `"negotiate"` mit `UseDefaultCredentials = true` |
| `BearerToken` | `Authorization: Bearer {token}` — Token aus Windows Credential Manager |
| `NegotiateWithImpersonation` | Named HttpClient `"negotiate"`, ausgeführt unter `WindowsIdentity.RunImpersonated` |

**Umsetzung:** `EndpointExecutionService.ExecuteAsync`, `ExecuteWithAuthAsync`, `ExecuteImpersonatedAsync`, `ApplyAuthentication`.

---

## Credential-Schlüsselformat

**Beschreibung:** Credentials im Windows Credential Manager werden einem Endpunkt eindeutig zugeordnet über einen strukturierten Schlüssel.

**Format:** `Schnittstellenzentrale:{ApplicationId}:{AuthenticationType}`

**Beispiel:** `Schnittstellenzentrale:42:Basic`

**Umsetzung:** `EndpointExecutionService.ApplyAuthentication` — der `target`-String wird lokal aus den Endpunkt-Eigenschaften aufgebaut.
