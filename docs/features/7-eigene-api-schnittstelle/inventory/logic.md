# Logik

## `ApplicationRepository`
Datei: `src/Schnittstellenzentrale.Infrastructure/Repositories/ApplicationRepository.cs`

Implementiert `IApplicationRepository`. Abhängigkeit: `AppDbContext` (per Konstruktorinjektion).

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `GetGroupsAsync(StorageMode, string)` | `public` | Lädt alle Gruppen inkl. Applications; im User-Modus nur Gruppen, die eigene Applications enthalten |
| `GetGroupByIdAsync(int)` | `public` | Lädt eine Gruppe per Id inkl. Applications |
| `AddGroupAsync(ApplicationGroup)` | `public` | Legt eine neue Gruppe an und speichert |
| `UpdateGroupAsync(ApplicationGroup)` | `public` | Aktualisiert eine bestehende Gruppe (detacht ggf. vorhandenes Tracking) |
| `DeleteGroupAsync(int)` | `public` | Löscht eine Gruppe per Id |
| `GetApplicationsAsync(StorageMode, string)` | `public` | Lädt alle Applications mit Gruppennavigation; User-Modus filtert nach Owner |
| `GetUngroupedApplicationsAsync(StorageMode, string)` | `public` | Lädt Applications ohne Gruppe; User-Modus filtert nach Owner |
| `GetApplicationByIdAsync(int)` | `public` | Lädt eine Application per Id inkl. Gruppe, Endpoints (mit Headers/QueryParameters) und EndpointGroups |
| `AddApplicationAsync(Application)` | `public` | Legt eine neue Application an und speichert |
| `UpdateApplicationAsync(Application)` | `public` | Aktualisiert eine bestehende Application; behandelt Tracking-Konflikte mit Application und ApplicationGroup |
| `DeleteApplicationAsync(int)` | `public` | Löscht eine Application per Id |
| `ApplyOwnerFilter(IQueryable<Application>, StorageMode, string)` | `private static` | Hilfsmethode: filtert per `Where(a => a.Owner == owner)` bei `StorageMode.User` |

---

## `ApplicationGroupEditor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupEditor.razor`

Blazor-Komponente. Injiziert: `IApplicationRepository`, `ISignalRNotificationService`, `IStorageModeService`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `SaveAsync()` | `private` | Ruft `ApplicationRepository.AddGroupAsync` auf; bei `StorageMode.Team` wird `SignalRNotificationService.NotifyGroupChangedAsync` aufgerufen |
| `Cancel()` | `private` | Löst `OnCancel`-Callback aus |

Parameter: `OnSaved` (`EventCallback`), `OnCancel` (`EventCallback`).

---

## `ApplicationEditor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationEditor.razor`

Blazor-Komponente. Injiziert: `IApplicationRepository`, `ISignalRNotificationService`, `IStorageModeService`, `ICurrentUserService`.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `OnInitializedAsync()` | `protected override` | Lädt Gruppen via `ApplicationRepository.GetGroupsAsync`; befüllt `_groups`-Liste; übernimmt `ExistingApplication` in Bearbeitungsmodus |
| `OnInterfaceUrlChanged(ChangeEventArgs)` | `private` | Aktualisiert `InterfaceUrl` und leitet `InterfaceType` via `Application.DetectInterfaceType` ab |
| `SaveAsync()` | `private` | Anlage-Modus: `AddApplicationAsync`; Bearbeitungs-Modus: `UpdateApplicationAsync`; bei `StorageMode.Team`: `NotifyApplicationChangedAsync`; im User-Modus: setzt `Owner` |
| `Cancel()` | `private` | Löst `OnCancel`-Callback aus |

Parameter: `OnSaved` (`EventCallback`), `OnCancel` (`EventCallback`), `ExistingApplication` (`Application?`).

---

## `SignalRNotificationService<THub>`
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/SignalRNotificationService.cs`

Implementiert `ISignalRNotificationService`. Generischer Parameter `THub` muss von `Hub` ableiten.

| Methode | Sichtbarkeit | Kurzbeschreibung |
|---|---|---|
| `NotifyApplicationChangedAsync(int)` | `public` | Sendet `"ApplicationChanged"`-Event an SignalR-Gruppe `application:{applicationId}` |
| `NotifyGroupChangedAsync(int)` | `public` | Sendet `"GroupChanged"`-Event an SignalR-Gruppe `group:{groupId}` |

---

## `Program.cs` — DI-Registrierung
Datei: `src/Schnittstellenzentrale/Program.cs`

Relevante Registrierungen:

- `AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate()` — Windows-Authentifizierung aktiv
- `AddAuthorization()` — Autorisierungs-Middleware vorhanden
- `AddHttpClient()` — generische `IHttpClientFactory` registriert
- `AddHttpClient("negotiate")` mit `UseDefaultCredentials = true` — benannter HTTP-Client für Windows-Authentifizierung
- `AddScoped<IApplicationRepository, ApplicationRepository>()` — Repository als Scoped registriert
- `AddScoped<ISignalRNotificationService, SignalRNotificationService<EndpointHub>>()`
- `UseAuthentication()` / `UseAuthorization()` — Middleware in der Pipeline aktiv
- Kein `AddControllers()` / `MapControllers()` — keine Controller-Pipeline vorhanden
