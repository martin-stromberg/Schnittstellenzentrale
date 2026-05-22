# Logik

## `EndpointExecutionService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs`

Implementiert `IEndpointExecutionService`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `ExecuteAsync` | `public` | Haupteinstieg: delegiert an `ExecuteImpersonatedAsync` oder `ExecuteWithAuthAsync` je nach `AuthenticationType` |
| `ExecuteWithAuthAsync` | `private` | Wählt den richtigen `HttpClient` (`negotiate` vs. Standard), ruft `SendAndBuildResultAsync` auf |
| `ExecuteImpersonatedAsync` | `private` | Führt die Anfrage unter Windows-Impersonation aus |
| `SendAndBuildResultAsync` | `private` | Baut `HttpRequestMessage`, sendet und liefert `EndpointExecutionResult` |
| `BuildResult` | `private static` | Liest `StatusCode` und `ResponseBody` aus `HttpResponseMessage`; befüllt `RequestDetails` |
| `BuildRequest` | `private` | Erstellt `HttpRequestMessage` aus `Endpoint`-Daten inkl. Query-Params, Headers, Body und Content-Type |
| `ApplyAuthentication` | `private` | Setzt `Authorization`-Header für `Basic` und `BearerToken` |
| `BuildCredentialTarget` | `private static` | Erzeugt den Schlüssel für den Windows Credential Manager |

**Nicht befüllt laut Anforderung:** `BuildResult` befüllt noch keine `ResponseHeaders`, `DurationMs` oder `ResponseSizeBytes`. Es gibt keine `Stopwatch`-Messung.

---

## `SignalRNotificationService<THub>`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/SignalRNotificationService.cs`

Implementiert `ISignalRNotificationService`. Generisch über `THub : Hub`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `NotifyApplicationChangedAsync` | `public` | Sendet `ApplicationChanged`-Event an SignalR-Gruppe `application:{applicationId}` |
| `NotifyGroupChangedAsync` | `public` | Sendet `GroupChanged`-Event an SignalR-Gruppe `group:{groupId}` |

**Fehlende Methoden laut Anforderung:** `NotifyEndpointChangedAsync` und `NotifyEndpointGroupChangedAsync` sind noch nicht implementiert.

---

## `EndpointRepository`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/EndpointRepository.cs`

Implementiert `IEndpointRepository`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `GetEndpointsAsync` | `public` | Lädt alle Endpunkte einer Anwendung inkl. Headers, QueryParams und Gruppe |
| `GetUngroupedEndpointsAsync` | `public` | Wie oben, aber nur Endpunkte mit `EndpointGroupId == null` |
| `GetEndpointByIdAsync` | `public` | Einzelner Endpunkt per Id, inkl. Headers, QueryParams und Gruppe |
| `AddEndpointAsync` | `public` | Fügt neuen Endpunkt ein und speichert |
| `UpdateEndpointAsync` | `public` | Aktualisiert bestehenden Endpunkt |
| `DeleteEndpointAsync` | `public` | Löscht Endpunkt per Id via `DeleteByIdAsync` |
| `GetEndpointGroupsAsync` | `public` | Lädt alle Gruppen einer Anwendung (ohne Endpunkte) |
| `GetEndpointGroupByIdAsync` | `public` | Einzelne Gruppe per Id, inkl. `Endpoints` |
| `AddEndpointGroupAsync` | `public` | Fügt neue Gruppe ein und speichert |
| `UpdateEndpointGroupAsync` | `public` | Aktualisiert bestehende Gruppe |
| `DeleteEndpointGroupAsync` | `public` | Löscht Gruppe per Id via `DeleteByIdAsync` |
| `AddHeaderAsync` | `public` | Fügt neuen Header ein und speichert |
| `DeleteHeaderAsync` | `public` | Löscht Header per Id |
| `AddQueryParameterAsync` | `public` | Fügt neuen Query-Parameter ein und speichert |
| `DeleteQueryParameterAsync` | `public` | Löscht Query-Parameter per Id |
| `DeleteByIdAsync<T>` | `private` | Generische Hilfsmethode: Entity per Id suchen und entfernen |

**Hinweis Kaskadenlöschung:** `DeleteEndpointGroupAsync` löscht nur die Gruppe selbst. Die EF-Core-Konfiguration in `AppDbContext.OnModelCreating` setzt für `EndpointGroup → Endpoint` `OnDelete(DeleteBehavior.SetNull)` — d. h. beim Löschen einer Gruppe werden enthaltene Endpunkte nicht kaskadierend gelöscht, sondern ihr `EndpointGroupId` wird auf `null` gesetzt.

---

## `EndpointHub`
Datei: `src/Schnittstellenzentrale/Hubs/EndpointHub.cs`

Erbt von `Hub`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `SubscribeToApplication` | `public` | Fügt Connection zu SignalR-Gruppe `application:{applicationId}` hinzu |
| `UnsubscribeFromApplication` | `public` | Entfernt Connection aus SignalR-Gruppe `application:{applicationId}` |
| `SubscribeToGroup` | `public` | Fügt Connection zu SignalR-Gruppe `group:{groupId}` hinzu |
| `UnsubscribeFromGroup` | `public` | Entfernt Connection aus SignalR-Gruppe `group:{groupId}` |

**Hinweis:** Der Hub verfügt aktuell über keine Gruppen für `endpoint:{id}` oder `endpointgroup:{id}`.

---

## `AppDbContext`
Datei: `src/Schnittstellenzentrale.Infrastructure/Data/AppDbContext.cs`

Relevante Konfiguration in `OnModelCreating`:

- `Application → Endpoint`: `OnDelete(DeleteBehavior.Cascade)` — Endpunkte werden beim Löschen der Anwendung kaskadierend gelöscht.
- `Application → EndpointGroup`: `OnDelete(DeleteBehavior.Cascade)` — Gruppen werden beim Löschen der Anwendung kaskadierend gelöscht.
- `EndpointGroup → Endpoint`: `OnDelete(DeleteBehavior.SetNull)` — Beim Löschen einer Gruppe wird `EndpointGroupId` der Endpunkte auf `null` gesetzt (kein Kaskadenlöschen).
- `Endpoint → EndpointHeader`: `OnDelete(DeleteBehavior.Cascade)`
- `Endpoint → EndpointQueryParameter`: `OnDelete(DeleteBehavior.Cascade)`

**Hinweis:** Das `BodyMode`-Feld fehlt noch im Schema und in der EF-Konfiguration.
