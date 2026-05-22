# Tests

## Testklassen

### `EndpointRepositoryIntegrationTests`
Datei: `src/Schnittstellenzentrale.Tests/Integration/EndpointRepositoryIntegrationTests.cs`

- `SaveEndpoint_ConcurrentWrite_DetectsConflict` — Prüft, dass bei gleichzeitigen Schreibvorgängen auf denselben Endpunkt eine `DbUpdateConcurrencyException` ausgelöst wird (Optimistic Concurrency via `RowVersion`).

**Noch fehlend laut Anforderung:** Szenarien für `DeleteEndpointGroupAsync` mit enthaltenen Endpunkten (Kaskadenlöschung).

---

### `EndpointExecutionServiceTests`
Datei: `src/Schnittstellenzentrale.Tests/Services/EndpointExecutionServiceTests.cs`

- `Execute_WithAuthTypeNone_SendsRequestWithoutCredentials` — Prüft, dass kein Authorization-Header gesetzt wird.
- `Execute_WithAuthTypeBasic_SendsBasicAuthHeader` — Prüft, dass ein `Basic`-Authorization-Header aus dem Credential Manager gesetzt wird.
- `Execute_WithAuthTypeNegotiate_UsesNegotiateHandler` — Prüft, dass der `negotiate`-HttpClient verwendet wird.
- `Execute_WithAuthTypeBearerToken_SendsBearerHeader` — Prüft, dass ein `Bearer`-Authorization-Header gesetzt wird.
- `Execute_WithAuthTypeNegotiateWithImpersonation_RunsImpersonated` — Prüft, dass der `negotiate`-HttpClient unter Impersonation verwendet wird.
- `Execute_OnConnectionError_DoesNotCallHealthCheck` — Prüft, dass bei Verbindungsfehlern kein HealthCheck aufgerufen wird und das Ergebnis `Success = false` ohne `StatusCode` liefert.

**Noch fehlend laut Anforderung:** Tests, die prüfen, dass `EndpointExecutionResult` korrekt mit `ResponseHeaders`, `DurationMs` und `ResponseSizeBytes` befüllt wird.

---

### `ApplicationContextMenuTests`
Datei: `src/Schnittstellenzentrale.Tests/Components/ApplicationContextMenuTests.cs`

bUnit-Komponententests für `ApplicationContextMenu`:

- `AusGruppeEntfernen_NurSichtbar_WennAnwendungInGruppe` — Prüft, dass der Eintrag „Aus Gruppe entfernen" nur bei gruppierten Anwendungen erscheint.
- `AusGruppeEntfernen_NichtSichtbar_WennAnwendungOhneGruppe` — Prüft das Fehlen des Eintrags bei ungruppierten Anwendungen.
- `AusGruppeEntfernen_LöstCallbackAus_UndSchliestMenu` — Prüft, dass `OnRemoveFromGroupRequested` ausgelöst und das Menü geschlossen wird.
- `Bearbeiten_Deaktiviert_WennIsSystem` — Prüft, dass „Bearbeiten" für Systemeinträge deaktiviert ist.
- `Löschen_Deaktiviert_WennIsSystem` — Prüft, dass „Löschen" für Systemeinträge deaktiviert ist.

Diese Testklasse dient laut Anforderung als Vorlage für neue Tests zu `EndpointContextMenu` und `EndpointGroupContextMenu`.

---

## Hilfsmethoden

### `TestHelpers`
Datei: `src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs`

- `CreateInMemoryDbContext()` — Erstellt einen `AppDbContext` mit SQLite In-Memory-Provider und gibt Kontext + Verbindung zurück. Wird von Repository-Integrationstests verwendet.
- `ExecuteWithTwoContextsAsync(Func<ApplicationRepository, ApplicationRepository, Task> test)` — Führt einen Test mit zwei unabhängigen `ApplicationRepository`-Instanzen über dieselbe SQLite-Verbindung aus. Wird für Concurrency-Tests genutzt. Aktuell auf `ApplicationRepository` spezialisiert (kein Pendant für `EndpointRepository`).
