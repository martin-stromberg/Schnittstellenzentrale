# Logik

## `EndpointExecutionService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionService.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `ExecuteAsync` | `public` | Dispatcht je nach `AuthenticationType` an `ExecuteWithAuthAsync` oder `ExecuteImpersonatedAsync` |
| `ExecuteWithAuthAsync` | `private` | Erstellt HTTP-Client, baut Request über `BuildRequest`, wendet Authentifizierung an |
| `ExecuteImpersonatedAsync` | `private` | Wie `ExecuteWithAuthAsync`, aber mit Windows-Impersonation |
| `SendAndBuildResultAsync` | `private static` | Sendet den Request, stoppt Zeitmessung, baut Ergebnis |
| `BuildResult` | `private static` | Liest Response-Body und -Header, befüllt `EndpointExecutionResult` |
| `BuildRequest` | `private` | Erstellt `HttpRequestMessage`; löst `{Platzhalter}` via `EndpointUrlBuilder.Resolve` auf; trägt Header und Body ein |
| `ApplyAuthentication` | `private` | Setzt `Authorization`-Header für Basic, BearerToken |

Abhängigkeiten:
- `IHttpClientFactory` — HTTP-Client-Erstellung
- `IHealthCheckService` — injiziert, aber in `BuildRequest`/`ExecuteAsync` aktuell nicht direkt aufgerufen (Health-Check-Verantwortung liegt in der UI)
- `ICredentialService` — Lesen von gespeicherten Passwörtern/Tokens
- `EndpointUrlBuilder.Resolve` — Auflösung von `{Pfadparameter}` und Aufbau des Query-Strings

Noch nicht vorhanden:
- Abhängigkeit von `IActiveEnvironmentService` — soll im Konstruktor ergänzt werden
- Methode `ResolvePlaceholders(string input, IReadOnlyDictionary<string, string> variables)` — soll `{{...}}`-Muster vor der bestehenden `{...}`-Auflösung ersetzen
- `BuildRequest` ruft `ResolvePlaceholders` noch nicht auf; betrifft Basis-URL, `RelativePath`, Header-Name/-Wert, Query-Parameter-Name/-Wert, Bearer-Token und Body

---

## `StorageModeService`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/StorageModeService.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `SetMode` | `public` | Setzt `CurrentMode`; feuert `OnModeChanged` wenn geändert |

Abonnierte Events: keine
Publizierte Events: `OnModeChanged` (`Action?`)

Registrierung: `AddScoped<IStorageModeService, StorageModeService>()` — Circuit-scoped in Blazor Server.
Dient als strukturelle Vorlage für den zu erstellenden `ActiveEnvironmentService`.

---

## `SignalRNotificationService<THub>`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/SignalRNotificationService.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `NotifyApplicationChangedAsync` | `public` | Sendet `ApplicationChanged` an Gruppe `application:{id}` |
| `NotifyGroupChangedAsync` | `public` | Sendet `GroupChanged` an Gruppe `group:{id}` |
| `NotifyEndpointChangedAsync` | `public` | Sendet `EndpointChanged` an Gruppe `application:{applicationId}` |
| `NotifyEndpointGroupChangedAsync` | `public` | Sendet `EndpointGroupChanged` an Gruppe `application:{applicationId}` |

Fehlend: Methode `NotifyEnvironmentChangedAsync()` — soll für Änderungen an `SystemEnvironment` im Team-Modus ergänzt werden.

---

## `ApplicationRepository`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/ApplicationRepository.cs`

Dient als Referenzimplementierung für das zu erstellende `SystemEnvironmentRepository`. Relevante Muster:

- `IDbContextFactory<AppDbContext>` als Abhängigkeit (Short-Lived Contexts per Operation)
- Private Hilfsmethode `ApplyOwnerFilter(IQueryable, StorageMode, string owner)` — filtert im User-Modus nach `Owner`
- Vollständige CRUD-Operationen mit `RowVersion`-basiertem optimistischem Sperren

---

## `EndpointUrlBuilder`
Datei: `src/Schnittstellenzentrale.Core/Helpers/EndpointUrlBuilder.cs`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `Resolve` | `public static` | Ersetzt `{Key}`-Platzhalter im Pfad durch Parameterwerte; hängt verbleibende Parameter als Query-String an; `keepEmptyPlaceholders`-Flag für Anzeige-Modus |

Löst ausschließlich einfache `{...}`-Platzhalter auf. Die `{{...}}`-Auflösung (Umgebungsvariablen) ist noch nicht implementiert und soll in `EndpointExecutionService.ResolvePlaceholders` erfolgen — vor dem `EndpointUrlBuilder.Resolve`-Aufruf.

---

## `MainLayout`
Datei: `src/Schnittstellenzentrale/Components/Layout/MainLayout.razor`

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---------|-------------|------------------|
| `OnInitialized` | `protected override` | Abonniert `StorageModeService.OnModeChanged` und `ThemeService.OnThemeChanged` |
| `OnAfterRenderAsync` | `protected override async` | Initialisiert Theme beim ersten Render |
| `OnStorageModeChanged` | `private` | Parst den gewählten Enum-Wert und ruft `StorageModeService.SetMode` auf |
| `OnStateChanged` | `private` | Ruft `InvokeAsync(StateHasChanged)` auf |
| `Dispose` | `public` | Meldet Event-Handler ab |

Abonnierte Events: `StorageModeService.OnModeChanged`, `ThemeService.OnThemeChanged`

Fehlend:
- Integration von `EnvironmentSelector`-Komponente (Auswahlbox für aktive Umgebung)
- Zahnrad-Icon als Trigger für `EnvironmentManagementOverlay`
- `localStorage`-Lesen beim Initialisieren und nach Moduswechsel (via `IJSRuntime`)
- Rückfall auf leere Auswahl beim Löschen der aktiven Umgebung
