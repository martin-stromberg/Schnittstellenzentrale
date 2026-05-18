# Umsetzungsplan: Eigene REST-API-Schnittstelle

## Übersicht

Die Schnittstellenzentrale erhält eine eigene REST-API mit vollständigem CRUD für `ApplicationGroup`- und `Application`-Datensätze sowie einem Authentifizierungsendpunkt (`POST /authenticate`). Das Interface `IApplicationApiClient` spiegelt `IApplicationRepository` vollständig — gleiche Methodensignaturen, damit alle Blazor-Komponenten mit minimalem Aufwand auf den REST-Client umgestellt werden können. Nach der Umstellung greifen alle Komponenten ausschließlich über `IApplicationApiClient` auf Daten zu; direkter Zugriff auf `IApplicationRepository` in Blazor-Komponenten entfällt vollständig.

Die Authentifizierung für Loopback-Aufrufe erfolgt über ein Token-Verfahren: Der Client ruft zunächst `/authenticate` auf (Windows-Authentifizierung wird dabei vom ASP.NET Core-Framework ausgewertet), erhält ein kurzlebiges Token (GUID, 5 Minuten Ablaufzeit) und verwendet dieses als Bearer-Token für alle nachfolgenden API-Aufrufe. Nach jedem erfolgreichen Datenendpunkt-Aufruf wird das verwendete Token ungültig gemacht und ein neuer Token in der Response zurückgegeben (Header `X-New-Token`).

DTOs liegen in `Schnittstellenzentrale.Core` unter dem Unterordner `Contracts`. API-Routen verwenden kein Versionspräfix (`/api/`, nicht `/api/v1/`).

---

## Programmabläufe

### Authentifizierung: POST /authenticate

*(bereits umgesetzt)*

1. Der Client sendet `POST /authenticate`. Der Request enthält keinen Body. Die Windows-Identität des aufrufenden Benutzers wird durch die ASP.NET Core-Authentifizierungsmiddleware (Windows-Authentifizierung) ermittelt.
2. `AuthController.AuthenticateAsync` liest den Windows-Benutzernamen aus `HttpContext.User.Identity.Name`.
3. Der Controller ruft `ITokenStore.CreateTokenAsync(windowsUsername)` auf. Der `TokenStore` erstellt eine neue `AuthToken`-Instanz (GUID als Token-Wert, Ablaufzeit jetzt + 5 Minuten, Windows-Benutzername) und speichert sie im Singleton-Dictionary.
4. Der Controller gibt `200 OK` mit dem serialisierten `AuthenticateResponse` (Feld `Token` als GUID-String) zurück.

Beteiligte Klassen/Komponenten: `AuthController`, `ITokenStore`, `TokenStore`, `AuthToken`, `AuthenticateResponse`

---

### Token-Validierung in Datenendpunkten

*(bereits umgesetzt)*

Vor der eigentlichen Verarbeitung jedes Datenendpunkts läuft folgende Prüfung:

1. Der Controller liest den `Authorization`-Header (Bearer-Schema) und extrahiert den Token-String.
2. Der Controller ruft `ITokenStore.ValidateAndRotateAsync(tokenString)` auf.
3. Der `TokenStore` prüft, ob der Token im Dictionary existiert und nicht abgelaufen ist.
   - Falls der Token nicht existiert oder abgelaufen ist: Der Controller gibt `401 Unauthorized` zurück. Die weitere Verarbeitung wird abgebrochen.
   - Falls der Token gültig ist: Der `TokenStore` löscht den alten Token, erstellt einen neuen Token (neuer GUID-Wert, neue Ablaufzeit jetzt + 5 Minuten, gleicher Windows-Benutzername) und gibt ihn zurück.
4. Der Controller setzt den neuen Token in den Response-Header `X-New-Token`.
5. Die eigentliche Endpunkt-Verarbeitung läuft weiter.

Beteiligte Klassen/Komponenten: `ITokenStore`, `TokenStore`, `AuthToken`

---

### GET /api/application-groups — Liste aller Gruppen mit Anwendungen

1. Der Client sendet `GET /api/application-groups` mit Authorization-Header (Bearer-Token) und den Headern `X-Storage-Mode` (Wert `Team` oder `User`) und `X-Owner`.
2. `ApplicationGroupsController.GetAllAsync` führt die Token-Validierung durch. Bei ungültigem Token: `401 Unauthorized`.
3. Der Controller liest `X-Storage-Mode` und `X-Owner` aus den Request-Headern.
4. Der Controller ruft `IApplicationRepository.GetGroupsAsync(storageMode, owner)` auf.
5. Der Controller mappt die Ergebnisliste auf `IList<ApplicationGroupResponse>`, wobei jede Gruppe ihre `Applications` als eingebettete `IList<ApplicationResponse>` enthält.
6. Der Controller gibt `200 OK` mit `X-New-Token`-Header und der serialisierten Liste zurück.

Beteiligte Klassen/Komponenten: `ApplicationGroupsController`, `ApplicationGroupResponse`, `ApplicationResponse`, `IApplicationRepository`, `ITokenStore`

---

### GET /api/application-groups/{id} — Einzelne Gruppe mit Anwendungen

1. Der Client sendet `GET /api/application-groups/{id}` mit Authorization-Header.
2. `ApplicationGroupsController.GetByIdAsync` führt die Token-Validierung durch.
3. Der Controller ruft `IApplicationRepository.GetGroupByIdAsync(id)` auf.
   - Ergebnis ist `null`: `404 Not Found`.
   - Ergebnis vorhanden: Mapping auf `ApplicationGroupResponse` (inkl. `Applications`).
4. Der Controller gibt `200 OK` mit `X-New-Token`-Header und der serialisierten `ApplicationGroupResponse` zurück.

Beteiligte Klassen/Komponenten: `ApplicationGroupsController`, `ApplicationGroupResponse`, `IApplicationRepository`, `ITokenStore`

---

### POST /api/application-groups — Anlage einer Gruppe

*(bereits umgesetzt)*

1. Der Client sendet `POST /api/application-groups` mit Authorization-Header (Bearer-Token), JSON-Body (`CreateApplicationGroupRequest`) und Header `X-Storage-Mode`.
2. `ApplicationGroupsController.CreateAsync` führt die Token-Validierung durch. Bei ungültigem Token: `401 Unauthorized`.
3. Der Controller lässt ASP.NET Core Model-Validation den Body prüfen. Bei Validierungsfehlern: `400 Bad Request`.
4. Der Controller liest `X-Storage-Mode` und bildet den Request auf eine neue `ApplicationGroup`-Instanz ab.
5. Der Controller ruft `IApplicationRepository.AddGroupAsync(group)` auf.
6. Nach erfolgreichem Speichern ruft der Controller `ISignalRNotificationService.NotifyGroupChangedAsync(group.Id)` auf (nur wenn `StorageMode == Team`).
7. Der Controller gibt `201 Created` mit `Location`-Header, `X-New-Token`-Header und der serialisierten `ApplicationGroupResponse` zurück.

Beteiligte Klassen/Komponenten: `ApplicationGroupsController`, `CreateApplicationGroupRequest`, `ApplicationGroupResponse`, `IApplicationRepository`, `ISignalRNotificationService`, `ITokenStore`

---

### PUT /api/application-groups/{id} — Umbenennung einer Gruppe

1. Der Client sendet `PUT /api/application-groups/{id}` mit Authorization-Header, JSON-Body (`UpdateApplicationGroupRequest`) und Header `X-Storage-Mode`.
2. `ApplicationGroupsController.UpdateAsync` führt die Token-Validierung durch. Bei ungültigem Token: `401 Unauthorized`.
3. Der Controller lässt ASP.NET Core Model-Validation den Body prüfen. Bei Validierungsfehlern: `400 Bad Request`.
4. Der Controller ruft `IApplicationRepository.GetGroupByIdAsync(id)` auf. Ergebnis ist `null`: `404 Not Found`.
5. Der Controller überschreibt die geänderten Felder (`Name`) am geladenen Objekt.
6. Der Controller ruft `IApplicationRepository.UpdateGroupAsync(group)` auf.
7. Nach erfolgreichem Speichern ruft der Controller `ISignalRNotificationService.NotifyGroupChangedAsync(group.Id)` auf (nur wenn `StorageMode == Team`).
8. Der Controller gibt `200 OK` mit `X-New-Token`-Header und der serialisierten `ApplicationGroupResponse` zurück.

Beteiligte Klassen/Komponenten: `ApplicationGroupsController`, `UpdateApplicationGroupRequest`, `ApplicationGroupResponse`, `IApplicationRepository`, `ISignalRNotificationService`, `ITokenStore`

---

### DELETE /api/application-groups/{id} — Löschen einer Gruppe

1. Der Client sendet `DELETE /api/application-groups/{id}` mit Authorization-Header und Header `X-Storage-Mode`.
2. `ApplicationGroupsController.DeleteAsync` führt die Token-Validierung durch. Bei ungültigem Token: `401 Unauthorized`.
3. Der Controller ruft `IApplicationRepository.GetGroupByIdAsync(id)` auf. Ergebnis ist `null`: `404 Not Found`.
4. Der Controller ruft `IApplicationRepository.DeleteGroupAsync(id)` auf.
5. Nach erfolgreichem Löschen ruft der Controller `ISignalRNotificationService.NotifyGroupChangedAsync(id)` auf (nur wenn `StorageMode == Team`).
6. Der Controller gibt `204 No Content` mit `X-New-Token`-Header zurück.

Beteiligte Klassen/Komponenten: `ApplicationGroupsController`, `IApplicationRepository`, `ISignalRNotificationService`, `ITokenStore`

---

### GET /api/applications — Liste aller Anwendungen

1. Der Client sendet `GET /api/applications` mit Authorization-Header, `X-Storage-Mode` und `X-Owner`.
2. `ApplicationsController.GetAllAsync` führt die Token-Validierung durch.
3. Der Controller ruft `IApplicationRepository.GetApplicationsAsync(storageMode, owner)` auf.
4. Der Controller mappt auf `IList<ApplicationResponse>` und gibt `200 OK` mit `X-New-Token`-Header zurück.

Beteiligte Klassen/Komponenten: `ApplicationsController`, `ApplicationResponse`, `IApplicationRepository`, `ITokenStore`

---

### GET /api/applications/ungrouped — Liste der Anwendungen ohne Gruppe

1. Der Client sendet `GET /api/applications/ungrouped` mit Authorization-Header, `X-Storage-Mode` und `X-Owner`.
2. `ApplicationsController.GetUngroupedAsync` führt die Token-Validierung durch.
3. Der Controller ruft `IApplicationRepository.GetUngroupedApplicationsAsync(storageMode, owner)` auf.
4. Der Controller mappt auf `IList<ApplicationResponse>` und gibt `200 OK` mit `X-New-Token`-Header zurück.

Beteiligte Klassen/Komponenten: `ApplicationsController`, `ApplicationResponse`, `IApplicationRepository`, `ITokenStore`

---

### GET /api/applications/{id} — Einzelne Anwendung

1. Der Client sendet `GET /api/applications/{id}` mit Authorization-Header.
2. `ApplicationsController.GetByIdAsync` führt die Token-Validierung durch.
3. Der Controller ruft `IApplicationRepository.GetApplicationByIdAsync(id)` auf.
   - Ergebnis ist `null`: `404 Not Found`.
   - Ergebnis vorhanden: Mapping auf `ApplicationResponse` (alle Felder).
4. Der Controller gibt `200 OK` mit `X-New-Token`-Header und der serialisierten `ApplicationResponse` zurück.

Beteiligte Klassen/Komponenten: `ApplicationsController`, `ApplicationResponse`, `IApplicationRepository`, `ITokenStore`

---

### POST /api/applications — Anlage einer Anwendung

*(bereits umgesetzt)*

1. Der Client sendet `POST /api/applications` mit Authorization-Header (Bearer-Token), JSON-Body (`CreateApplicationRequest`) und Header `X-Storage-Mode`.
2. `ApplicationsController.CreateAsync` führt die Token-Validierung durch. Bei ungültigem Token: `401 Unauthorized`.
3. Der Controller lässt ASP.NET Core Model-Validation den Body prüfen. Bei Validierungsfehlern: `400 Bad Request`.
4. Der Controller liest `X-Storage-Mode` und bildet den Request auf eine neue `Application`-Instanz ab; `InterfaceType` wird via `Application.DetectInterfaceType(request.InterfaceUrl)` abgeleitet.
5. Der Controller ruft `IApplicationRepository.AddApplicationAsync(application)` auf.
6. Nach erfolgreichem Speichern ruft der Controller `ISignalRNotificationService.NotifyApplicationChangedAsync(application.Id)` auf (nur wenn `StorageMode == Team`).
7. Der Controller gibt `201 Created` mit `Location`-Header, `X-New-Token`-Header und der serialisierten `ApplicationResponse` zurück.

Beteiligte Klassen/Komponenten: `ApplicationsController`, `CreateApplicationRequest`, `ApplicationResponse`, `IApplicationRepository`, `ISignalRNotificationService`, `Application`, `ITokenStore`

---

### PUT /api/applications/{id} — Bearbeitung einer Anwendung

1. Der Client sendet `PUT /api/applications/{id}` mit Authorization-Header, JSON-Body (`UpdateApplicationRequest`) und Header `X-Storage-Mode`.
2. `ApplicationsController.UpdateAsync` führt die Token-Validierung durch. Bei ungültigem Token: `401 Unauthorized`.
3. Der Controller lässt ASP.NET Core Model-Validation den Body prüfen. Bei Validierungsfehlern: `400 Bad Request`.
4. Der Controller ruft `IApplicationRepository.GetApplicationByIdAsync(id)` auf. Ergebnis ist `null`: `404 Not Found`.
5. Der Controller überschreibt alle änderbaren Felder am geladenen Objekt (`Name`, `BaseUrl`, `Description`, `InterfaceUrl`, `InterfaceType`, `ApplicationGroupId`, `Owner`). `InterfaceType` wird via `Application.DetectInterfaceType(request.InterfaceUrl)` neu abgeleitet.
6. Der Controller ruft `IApplicationRepository.UpdateApplicationAsync(application)` auf.
7. Nach erfolgreichem Speichern ruft der Controller `ISignalRNotificationService.NotifyApplicationChangedAsync(application.Id)` auf (nur wenn `StorageMode == Team`).
8. Der Controller gibt `200 OK` mit `X-New-Token`-Header und der serialisierten `ApplicationResponse` zurück.

Beteiligte Klassen/Komponenten: `ApplicationsController`, `UpdateApplicationRequest`, `ApplicationResponse`, `IApplicationRepository`, `ISignalRNotificationService`, `Application`, `ITokenStore`

---

### DELETE /api/applications/{id} — Löschen einer Anwendung

1. Der Client sendet `DELETE /api/applications/{id}` mit Authorization-Header und Header `X-Storage-Mode`.
2. `ApplicationsController.DeleteAsync` führt die Token-Validierung durch. Bei ungültigem Token: `401 Unauthorized`.
3. Der Controller ruft `IApplicationRepository.GetApplicationByIdAsync(id)` auf. Ergebnis ist `null`: `404 Not Found`.
4. Der Controller ruft `IApplicationRepository.DeleteApplicationAsync(id)` auf.
5. Nach erfolgreichem Löschen ruft der Controller `ISignalRNotificationService.NotifyApplicationChangedAsync(id)` auf (nur wenn `StorageMode == Team`).
6. Der Controller gibt `204 No Content` mit `X-New-Token`-Header zurück.

Beteiligte Klassen/Komponenten: `ApplicationsController`, `IApplicationRepository`, `ISignalRNotificationService`, `ITokenStore`

---

### Ablauf in ApplicationGroupEditor (umgestellt auf REST-Client)

*(Anlage bereits umgesetzt; Signatur-Umstellung auf neue IApplicationApiClient-Signatur ausstehend)*

1. `ApplicationGroupEditor.SaveAsync` ruft `IApplicationApiClient.AddGroupAsync(new ApplicationGroup { Name = _model.Name })` auf.
2. `ApplicationApiClient.AddGroupAsync`:
   a. Falls noch kein gültiger Token vorhanden ist: Ruft `POST /authenticate` auf und speichert den erhaltenen Token in-memory.
   b. Liest `StorageMode` aus `IStorageModeService` und setzt `X-Storage-Mode`-Header.
   c. Serialisiert das `ApplicationGroup`-Objekt als `CreateApplicationGroupRequest` und sendet es via `HttpClient` an `POST /api/application-groups`.
   d. Liest den `X-New-Token`-Header aus der Response und ersetzt den gespeicherten Token.
   e. Deserialisiert die `ApplicationGroupResponse`, mappt sie auf `ApplicationGroup` und gibt sie zurück.
3. Bei Erfolg löst `ApplicationGroupEditor` den `OnSaved`-Callback aus.
4. Die SignalR-Benachrichtigung liegt im Controller — die Komponente löst keine aus.

Beteiligte Klassen/Komponenten: `ApplicationGroupEditor`, `IApplicationApiClient`, `ApplicationApiClient`, `CreateApplicationGroupRequest`, `ApplicationGroupResponse`

---

### Ablauf in ApplicationEditor (vollständig auf REST-Client umgestellt)

*(Anlage-Pfad teilweise umgesetzt; Lese- und Bearbeitungs-Pfad ausstehend)*

1. `ApplicationEditor.OnInitializedAsync` ruft `IApplicationApiClient.GetGroupsAsync(storageMode, owner)` auf, um die Gruppenauswahlliste zu füllen.
2. `ApplicationEditor.SaveAsync` im Anlage-Modus ruft `IApplicationApiClient.AddApplicationAsync(new Application { ... })` auf.
3. `ApplicationEditor.SaveAsync` im Bearbeitungs-Modus ruft `IApplicationApiClient.UpdateApplicationAsync(_model)` auf.
4. `IApplicationRepository`-Injektion und alle direkten `ISignalRNotificationService`-Aufrufe werden aus der Komponente entfernt.

Beteiligte Klassen/Komponenten: `ApplicationEditor`, `IApplicationApiClient`, `ApplicationApiClient`

---

### Ablauf in ApplicationGroupTree (vollständig auf REST-Client umgestellt)

1. `ApplicationGroupTree.LoadDataAsync` ruft `IApplicationApiClient.GetGroupsAsync(storageMode, owner)` und `IApplicationApiClient.GetUngroupedApplicationsAsync(storageMode, owner)` auf.
2. `OnRemoveFromGroupRequested` ruft `IApplicationApiClient.UpdateApplicationAsync(application)` auf (Gruppe auf `null` gesetzt).
3. `OnDrop` ruft `IApplicationApiClient.UpdateApplicationAsync(_draggedApplication)` auf.
4. `IApplicationRepository`-Injektion und alle direkten `ISignalRNotificationService`-Aufrufe werden entfernt.

Beteiligte Klassen/Komponenten: `ApplicationGroupTree`, `IApplicationApiClient`, `ApplicationApiClient`

---

### Ablauf in ApplicationCard (vollständig auf REST-Client umgestellt)

1. `ApplicationCard.OnParametersSetAsync` ruft `IApplicationApiClient.GetApplicationByIdAsync(ApplicationId)` auf.
2. `RemoveApplication` ruft `IApplicationApiClient.DeleteApplicationAsync(_application.Id)` auf.
3. `IApplicationRepository`-Injektion wird entfernt.

Beteiligte Klassen/Komponenten: `ApplicationCard`, `IApplicationApiClient`, `ApplicationApiClient`

---

### Ablauf in Home (vollständig auf REST-Client umgestellt)

1. `OnGroupRenamed` ruft `IApplicationApiClient.UpdateGroupAsync(group)` auf.
2. `OnDeleteGroupConfirmedAll` ruft für jede Anwendung `IApplicationApiClient.DeleteApplicationAsync(app.Id)` auf, danach `IApplicationApiClient.DeleteGroupAsync(group.Id)`.
3. `OnDeleteGroupConfirmedGroupOnly` ruft für jede Anwendung `IApplicationApiClient.UpdateApplicationAsync(app)` auf (Gruppe auf `null` gesetzt), danach `IApplicationApiClient.DeleteGroupAsync(group.Id)`.
4. `OnDeleteApplicationConfirmed` ruft `IApplicationApiClient.DeleteApplicationAsync(application.Id)` auf.
5. `IApplicationRepository`-Injektion und alle direkten `ISignalRNotificationService`-Aufrufe werden entfernt.

Beteiligte Klassen/Komponenten: `Home`, `IApplicationApiClient`, `ApplicationApiClient`

---

## Neue Klassen

| Klasse | Typ | Projekt | Zweck |
|--------|-----|---------|-------|
| `AuthToken` | Datenklasse | `Schnittstellenzentrale.Core` | Speichert GUID-Tokenwert, Ablaufzeit (`DateTime`) und Windows-Benutzernamen eines aktiven Tokens *(umgesetzt)* |
| `ITokenStore` | Interface | `Schnittstellenzentrale.Core` | Vertrag für Token-Verwaltung: `CreateTokenAsync(username)`, `ValidateAndRotateAsync(tokenString)` *(umgesetzt)* |
| `TokenStore` | Klasse (Singleton) | `Schnittstellenzentrale` | Implementiert `ITokenStore`; verwaltet aktive Token in einem Singleton-Dictionary (GUID-String → `AuthToken`); bereinigt abgelaufene Token bei Zugriff *(umgesetzt)* |
| `AuthenticateResponse` | DTO | `Schnittstellenzentrale.Core/Contracts` | Response-Body für `POST /authenticate`; enthält `Token` als GUID-String *(umgesetzt)* |
| `AuthController` | ASP.NET Core Controller | `Schnittstellenzentrale` | Exponiert `POST /authenticate`; liest Windows-Identität, ruft `ITokenStore.CreateTokenAsync` auf, gibt `AuthenticateResponse` zurück *(umgesetzt)* |
| `CreateApplicationGroupRequest` | DTO | `Schnittstellenzentrale.Core/Contracts` | Request-Body für `POST /api/application-groups`; enthält `Name` als Pflichtfeld *(umgesetzt)* |
| `UpdateApplicationGroupRequest` | DTO | `Schnittstellenzentrale.Core/Contracts` | Request-Body für `PUT /api/application-groups/{id}`; enthält `Name` (`[Required]`, `[MaxLength(200)]`) |
| `ApplicationGroupResponse` | DTO | `Schnittstellenzentrale.Core/Contracts` | Response-Body für Gruppen-Endpunkte; enthält `Id`, `Name` sowie `IList<ApplicationResponse> Applications` *(Id und Name umgesetzt; Applications ausstehend)* |
| `CreateApplicationRequest` | DTO | `Schnittstellenzentrale.Core/Contracts` | Request-Body für `POST /api/applications`; enthält `Name`, `BaseUrl`, `Description`, `InterfaceUrl`, `ApplicationGroupId`, `Owner` *(umgesetzt)* |
| `UpdateApplicationRequest` | DTO | `Schnittstellenzentrale.Core/Contracts` | Request-Body für `PUT /api/applications/{id}`; enthält `Name`, `BaseUrl` (`[Required]`), `Description`, `InterfaceUrl`, `ApplicationGroupId`, `Owner` |
| `ApplicationResponse` | DTO | `Schnittstellenzentrale.Core/Contracts` | Response-Body für Anwendungs-Endpunkte; enthält `Id`, `Name`, `BaseUrl`, `ApplicationGroupId`, `Description`, `InterfaceUrl`, `InterfaceType`, `Owner` *(Id, Name, BaseUrl, ApplicationGroupId umgesetzt; weitere Felder ausstehend)* |
| `ApplicationGroupsController` | ASP.NET Core Controller | `Schnittstellenzentrale` | Exponiert alle `/api/application-groups`-Endpunkte; validiert Token, delegiert an `IApplicationRepository` *(POST umgesetzt; GET/PUT/DELETE ausstehend)* |
| `ApplicationsController` | ASP.NET Core Controller | `Schnittstellenzentrale` | Exponiert alle `/api/applications`-Endpunkte; validiert Token, delegiert an `IApplicationRepository` *(POST umgesetzt; GET/PUT/DELETE ausstehend)* |
| `IApplicationApiClient` | Interface | `Schnittstellenzentrale.Core` | Spiegelt `IApplicationRepository` vollständig; definiert alle CRUD-Methoden als Vertrag für den REST-Client *(AddGroupAsync und AddApplicationAsync umgesetzt; restliche Methoden ausstehend)* |
| `ApplicationApiClient` | HTTP-Client | `Schnittstellenzentrale` | Implementiert `IApplicationApiClient`; führt bei Bedarf `/authenticate` durch, liest `StorageMode` und Owner intern aus `IStorageModeService` und `ICurrentUserService` (für Schreiboperationen) bzw. aus den Methodenparametern (für Leseoperationen), setzt `X-Storage-Mode`- und `X-Owner`-Header, liest `X-New-Token` aus Responses *(AddGroupAsync und AddApplicationAsync umgesetzt; restliche Methoden ausstehend)* |

---

## Änderungen an bestehenden Klassen

### `IApplicationApiClient` (Interface)

- **Neue Signatur** — ersetzt die bisherigen DTO-basierten Methoden vollständig. Das Interface spiegelt `IApplicationRepository`:

```csharp
Task<IList<ApplicationGroup>> GetGroupsAsync(StorageMode storageMode, string owner);
Task<ApplicationGroup?> GetGroupByIdAsync(int id);
Task<ApplicationGroup> AddGroupAsync(ApplicationGroup group);
Task<ApplicationGroup> UpdateGroupAsync(ApplicationGroup group);
Task DeleteGroupAsync(int id);
Task<IList<Application>> GetUngroupedApplicationsAsync(StorageMode storageMode, string owner);
Task<Application?> GetApplicationByIdAsync(int id);
Task<Application> AddApplicationAsync(Application application);
Task<Application> UpdateApplicationAsync(Application application);
Task DeleteApplicationAsync(int id);
```

- Die bisherigen Methoden `AddGroupAsync(CreateApplicationGroupRequest, StorageMode)` und `AddApplicationAsync(CreateApplicationRequest, StorageMode)` werden durch die obigen domänenmodell-basierten Varianten ersetzt.

---

### `ApplicationApiClient` (HTTP-Client-Implementierung)

- **Neue Injektion:** `IStorageModeService` und `ICurrentUserService` werden im Konstruktor injiziert.
- **Leseoperationen** (`GetGroupsAsync`, `GetGroupByIdAsync`, `GetUngroupedApplicationsAsync`, `GetApplicationByIdAsync`): Lesen `StorageMode` und `owner` aus den Methodenparametern; setzen `X-Storage-Mode`- und `X-Owner`-Header.
- **Schreiboperationen** (`AddGroupAsync`, `UpdateGroupAsync`, `DeleteGroupAsync`, `AddApplicationAsync`, `UpdateApplicationAsync`, `DeleteApplicationAsync`): Lesen `StorageMode` intern aus `IStorageModeService`; `Owner` bei Bedarf aus `ICurrentUserService`. Setzen `X-Storage-Mode`-Header.
- **Mapping:** Domänenobjekte werden intern auf DTOs gemappt (Request) und von DTOs zurück auf Domänenobjekte (Response). Kein DTO-Typ ist mehr nach außen sichtbar.
- **Token-Rotation:** Bleibt unverändert — nach jedem erfolgreichen Aufruf wird `X-New-Token` gespeichert.

---

### `ApplicationGroupResponse` (DTO)

- **Neues Feld:** `IList<ApplicationResponse> Applications` wird hinzugefügt (wird bei GET-Endpunkten befüllt, bei POST/PUT als leere Liste zurückgegeben).

---

### `ApplicationResponse` (DTO)

- **Neue Felder:** `Description`, `InterfaceUrl`, `InterfaceType` (Enum-Wert als String oder Integer), `Owner` werden hinzugefügt.

---

### `ApplicationGroupEditor` (Blazor-Komponente)

- **Geänderte Methode:** `SaveAsync` ruft `IApplicationApiClient.AddGroupAsync(new ApplicationGroup { Name = _model.Name })` auf statt des bisherigen DTO-basierten Aufrufs. Die Injektion von `IStorageModeService` kann entfernt werden, da `ApplicationApiClient` den `StorageMode` intern liest.

---

### `ApplicationEditor` (Blazor-Komponente)

- **Entfernte Injektion:** `IApplicationRepository` und `ISignalRNotificationService`.
- **Geänderte Methode:** `OnInitializedAsync` — Gruppenladung via `IApplicationApiClient.GetGroupsAsync(StorageModeService.CurrentMode, CurrentUserService.GetCurrentUserName())`.
- **Geänderte Methode:** `SaveAsync` im Anlage-Modus — `IApplicationApiClient.AddApplicationAsync(_model)` statt DTO-basiertem Aufruf.
- **Geänderte Methode:** `SaveAsync` im Bearbeitungs-Modus — `IApplicationApiClient.UpdateApplicationAsync(_model)` statt direktem Repository-Aufruf; SignalR-Aufruf entfällt.

---

### `ApplicationGroupTree` (Blazor-Komponente)

- **Entfernte Injektion:** `IApplicationRepository`, `ISignalRNotificationService`.
- **Neue Injektion:** `IApplicationApiClient`.
- **Geänderte Methode:** `LoadDataAsync` — Aufrufe an `IApplicationApiClient.GetGroupsAsync` und `IApplicationApiClient.GetUngroupedApplicationsAsync`.
- **Geänderte Methode:** `OnRemoveFromGroupRequested` — Aufruf an `IApplicationApiClient.UpdateApplicationAsync`; SignalR-Aufruf entfällt.
- **Geänderte Methode:** `OnDrop` — Aufruf an `IApplicationApiClient.UpdateApplicationAsync`; SignalR-Aufruf entfällt.

---

### `ApplicationCard` (Blazor-Komponente)

- **Entfernte Injektion:** `IApplicationRepository`.
- **Neue Injektion:** `IApplicationApiClient`.
- **Geänderte Methode:** `OnParametersSetAsync` — `IApplicationApiClient.GetApplicationByIdAsync(ApplicationId)`.
- **Geänderte Methode:** `RemoveApplication` — `IApplicationApiClient.DeleteApplicationAsync(_application.Id)`.

---

### `Home` (Blazor-Seite)

- **Entfernte Injektion:** `IApplicationRepository`, `ISignalRNotificationService`.
- **Neue Injektion:** `IApplicationApiClient`.
- **Geänderte Methode:** `OnGroupRenamed` — `IApplicationApiClient.UpdateGroupAsync(group)`; SignalR-Aufruf entfällt.
- **Geänderte Methode:** `OnDeleteGroupConfirmedAll` — alle Repository- und SignalR-Aufrufe durch `IApplicationApiClient`-Aufrufe ersetzen.
- **Geänderte Methode:** `OnDeleteGroupConfirmedGroupOnly` — alle Repository- und SignalR-Aufrufe durch `IApplicationApiClient`-Aufrufe ersetzen.
- **Geänderte Methode:** `OnDeleteApplicationConfirmed` — `IApplicationApiClient.DeleteApplicationAsync(application.Id)`; SignalR-Aufruf entfällt.

---

### `Program.cs` (DI-Registrierung und Pipeline)

*(größtenteils umgesetzt)*

- **Bereits registriert:** `builder.Services.AddControllers()`, `app.MapControllers()`, `builder.Services.AddSingleton<ITokenStore, TokenStore>()`, `AddHttpClient<IApplicationApiClient, ApplicationApiClient>`.
- **Ausstehend:** `IStorageModeService` und `ICurrentUserService` müssen im `ApplicationApiClient`-Konstruktor verfügbar sein (werden über DI bereitgestellt; prüfen, ob Scoping korrekt ist).

---

### `appsettings.json`

*(bereits umgesetzt)*

- Konfigurationsabschnitt `Api` mit Schlüssel `BaseUrl` ist vorhanden.

---

## Datenbankmigrationen

Keine.

---

## Validierungsregeln

| Feld / Objekt | Regel | Fehlerfall |
|---------------|-------|------------|
| Bearer-Token (alle Datenendpunkte) | Muss vorhanden, gültig und nicht abgelaufen sein | HTTP 401 Unauthorized |
| `CreateApplicationGroupRequest.Name` | `[Required]`, `[MaxLength(200)]` | HTTP 400 mit Validierungsfehlern im Problem-Details-Format |
| `UpdateApplicationGroupRequest.Name` | `[Required]`, `[MaxLength(200)]` | HTTP 400 mit Validierungsfehlern im Problem-Details-Format |
| `CreateApplicationRequest.Name` | `[Required]`, `[MaxLength(200)]` | HTTP 400 mit Validierungsfehlern im Problem-Details-Format |
| `CreateApplicationRequest.BaseUrl` | `[Required]`, `[MaxLength(500)]` | HTTP 400 mit Validierungsfehlern im Problem-Details-Format |
| `CreateApplicationRequest.InterfaceUrl` | `[MaxLength(500)]` (optional) | HTTP 400 mit Validierungsfehlern im Problem-Details-Format |
| `CreateApplicationRequest.Owner` | `[MaxLength(256)]` (optional) | HTTP 400 mit Validierungsfehlern im Problem-Details-Format |
| `UpdateApplicationRequest.Name` | `[Required]`, `[MaxLength(200)]` | HTTP 400 mit Validierungsfehlern im Problem-Details-Format |
| `UpdateApplicationRequest.BaseUrl` | `[Required]`, `[MaxLength(500)]` | HTTP 400 mit Validierungsfehlern im Problem-Details-Format |
| `UpdateApplicationRequest.InterfaceUrl` | `[MaxLength(500)]` (optional) | HTTP 400 mit Validierungsfehlern im Problem-Details-Format |
| `UpdateApplicationRequest.Owner` | `[MaxLength(256)]` (optional) | HTTP 400 mit Validierungsfehlern im Problem-Details-Format |
| GET /api/application-groups/{id}, GET /api/applications/{id} | Ressource muss existieren | HTTP 404 Not Found |
| PUT /api/application-groups/{id}, PUT /api/applications/{id} | Ressource muss existieren | HTTP 404 Not Found |
| DELETE /api/application-groups/{id}, DELETE /api/applications/{id} | Ressource muss existieren | HTTP 404 Not Found |

Die Body-Validierung erfolgt automatisch über ASP.NET Core Model-Validation. Die Token-Validierung wird manuell in jedem Datenendpunkt-Controller durchgeführt.

---

## Konfigurationsänderungen

| Eintrag | Typ | Standardwert | Zweck |
|---------|-----|--------------|-------|
| `Api:BaseUrl` | `string` | `https://localhost:5001` | Basis-URL der eigenen REST-API für `ApplicationApiClient`; ermöglicht Loopback-Konfiguration ohne Hardcodierung *(umgesetzt)* |

---

## Seiteneffekte und Risiken

- **SignalR-Benachrichtigungen:** Bisher wurden SignalR-Aufrufe in den Blazor-Komponenten nach dem Speichern ausgelöst. Nach der vollständigen Umstellung übernehmen die Controller diese Aufgabe für alle Schreiboperationen. Der `StorageMode` wird aus dem `X-Storage-Mode`-Request-Header entnommen und muss vom `ApplicationApiClient` korrekt mitgesendet werden.
- **Token-Ablauf:** Ein Token ist 5 Minuten gültig und wird nach jedem erfolgreichen Datenendpunkt-Aufruf rotiert. Wenn ein Token zwischen dem letzten Aufruf und dem nächsten abläuft, gibt der Endpunkt `401` zurück. Der `ApplicationApiClient` ruft in diesem Fall erneut `/authenticate` auf und wiederholt den Datenaufruf (Retry-Logik).
- **Nebenläufigkeit im `TokenStore`:** Da `TokenStore` als Singleton läuft, muss der Zugriff auf das interne Dictionary thread-sicher sein (`ConcurrentDictionary`).
- **IApplicationRepository in Blazor-Komponenten:** Nach der Umstellung darf `IApplicationRepository` in keiner Blazor-Komponente mehr injiziert sein. Dies ist per Compile-Zeit nicht erzwingbar, sollte aber durch Code-Review sichergestellt werden.
- **`ApplicationContextMenuTests`:** Die bUnit-Tests für `ApplicationContextMenu` injizieren kein Repository und sind vom Feature nicht direkt betroffen; sie sollten unverändert grün bleiben.
- **Scoping von IStorageModeService und ICurrentUserService:** Diese Dienste sind Scoped oder Singleton — beim Injizieren in `ApplicationApiClient` (registriert als typisierter HTTP-Client) muss das Scoping korrekt sein. Ggf. ist `IHttpContextAccessor` oder eine Singleton-sichere Alternative zu verwenden.

---

## Umsetzungsreihenfolge

1. *(umgesetzt)* DTOs anlegen: `AuthenticateResponse`, `CreateApplicationGroupRequest`, `ApplicationGroupResponse`, `CreateApplicationRequest`, `ApplicationResponse`.
2. *(umgesetzt)* Token-Infrastruktur anlegen: `AuthToken`, `ITokenStore`, `TokenStore`.
3. *(umgesetzt)* `Program.cs` anpassen: `AddControllers()`, `MapControllers()`, `AddSingleton<ITokenStore, TokenStore>()`, `AddHttpClient<IApplicationApiClient, ApplicationApiClient>`.
4. *(umgesetzt)* `appsettings.json` anpassen: `Api:BaseUrl`.
5. *(umgesetzt)* Controller-Grundgerüst und POST-Endpunkte: `AuthController`, `ApplicationGroupsController.CreateAsync`, `ApplicationsController.CreateAsync`.
6. *(umgesetzt)* `IApplicationApiClient` (nur `AddGroupAsync`, `AddApplicationAsync`) und `ApplicationApiClient` implementieren.
7. *(umgesetzt)* `ApplicationGroupEditor` auf `IApplicationApiClient.AddGroupAsync` umstellen.
8. DTOs erweitern: `ApplicationGroupResponse` um `IList<ApplicationResponse> Applications`; `ApplicationResponse` um `Description`, `InterfaceUrl`, `InterfaceType`, `Owner`.
9. Neue DTOs anlegen: `UpdateApplicationGroupRequest`, `UpdateApplicationRequest`.
10. `IApplicationApiClient` vollständig neu gestalten — alle Methoden analog zu `IApplicationRepository` (domänenmodell-basiert, StorageMode/Owner als Parameter für Leseoperationen).
11. `ApplicationApiClient` vollständig implementieren: alle neuen Methoden, Injektion von `IStorageModeService` und `ICurrentUserService`, internes Mapping zwischen Domänenobjekten und DTOs.
12. `ApplicationGroupsController` um `GetAllAsync`, `GetByIdAsync`, `UpdateAsync`, `DeleteAsync` erweitern.
13. `ApplicationsController` um `GetAllAsync`, `GetUngroupedAsync`, `GetByIdAsync`, `UpdateAsync`, `DeleteAsync` erweitern.
14. `ApplicationGroupEditor` auf neue `IApplicationApiClient`-Signatur umstellen (`AddGroupAsync(new ApplicationGroup { Name = ... })`).
15. `ApplicationEditor` vollständig umstellen: `IApplicationRepository`- und `ISignalRNotificationService`-Injektion entfernen; alle Methoden auf `IApplicationApiClient` umstellen.
16. `ApplicationGroupTree` vollständig umstellen: `IApplicationRepository`- und `ISignalRNotificationService`-Injektion entfernen; alle Methoden auf `IApplicationApiClient` umstellen.
17. `ApplicationCard` vollständig umstellen: `IApplicationRepository`-Injektion entfernen; alle Methoden auf `IApplicationApiClient` umstellen.
18. `Home` vollständig umstellen: `IApplicationRepository`- und `ISignalRNotificationService`-Injektion entfernen; alle Methoden auf `IApplicationApiClient` umstellen.
19. Integrationstests für alle neuen Controller-Endpunkte anlegen.
20. Unit-Tests für `ApplicationApiClient` um alle neuen Methoden erweitern.

---

## Tests

### Neue Tests

| Test | Testklasse | Was wird geprüft? |
|------|------------|-------------------|
| `Authenticate_WithValidWindowsIdentity_Returns200WithToken` | `AuthControllerIntegrationTests` | Erfolgreicher Aufruf von `/authenticate`; Response-Status 200, `AuthenticateResponse.Token` ist ein gültiger GUID-String *(umgesetzt)* |
| `Authenticate_CreatesTokenInTokenStore` | `AuthControllerIntegrationTests` | Nach dem Aufruf ist der Token im `ITokenStore` vorhanden und nicht abgelaufen *(umgesetzt)* |
| `PostApplicationGroup_WithValidTokenAndRequest_Returns201AndLocation` | `ApplicationGroupsControllerIntegrationTests` | Erfolgreiche Anlage einer Gruppe; Response-Status 201, `Location`-Header vorhanden, `ApplicationGroupResponse` korrekt befüllt, `X-New-Token`-Header vorhanden *(umgesetzt)* |
| `PostApplicationGroup_WithoutToken_Returns401` | `ApplicationGroupsControllerIntegrationTests` | Aufruf ohne Authorization-Header gibt HTTP 401 zurück *(umgesetzt)* |
| `PostApplicationGroup_WithExpiredToken_Returns401` | `ApplicationGroupsControllerIntegrationTests` | Aufruf mit abgelaufenem Token gibt HTTP 401 zurück *(umgesetzt)* |
| `PostApplicationGroup_WithMissingName_Returns400` | `ApplicationGroupsControllerIntegrationTests` | Fehlende Pflichtfelder führen zu HTTP 400 *(umgesetzt)* |
| `PostApplicationGroup_RotatesToken_OldTokenIsInvalid` | `ApplicationGroupsControllerIntegrationTests` | Nach erfolgreichem Aufruf ist der ursprüngliche Token ungültig; der neue Token aus `X-New-Token` ist gültig *(umgesetzt)* |
| `GetApplicationGroups_WithValidToken_Returns200WithList` | `ApplicationGroupsControllerIntegrationTests` | GET /api/application-groups gibt 200 mit korrekter Liste zurück; `ApplicationGroupResponse` enthält `Applications` |
| `GetApplicationGroups_WithoutToken_Returns401` | `ApplicationGroupsControllerIntegrationTests` | Aufruf ohne Authorization-Header gibt HTTP 401 zurück |
| `GetApplicationGroupById_WithValidId_Returns200` | `ApplicationGroupsControllerIntegrationTests` | GET /api/application-groups/{id} gibt 200 mit korrekter `ApplicationGroupResponse` zurück |
| `GetApplicationGroupById_WithInvalidId_Returns404` | `ApplicationGroupsControllerIntegrationTests` | GET /api/application-groups/{id} mit unbekannter Id gibt 404 zurück |
| `PutApplicationGroup_WithValidRequest_Returns200AndRotatesToken` | `ApplicationGroupsControllerIntegrationTests` | Erfolgreiche Umbenennung; Response-Status 200, Name aktualisiert, `X-New-Token`-Header vorhanden |
| `PutApplicationGroup_WithInvalidId_Returns404` | `ApplicationGroupsControllerIntegrationTests` | PUT mit unbekannter Id gibt 404 zurück |
| `PutApplicationGroup_WithMissingName_Returns400` | `ApplicationGroupsControllerIntegrationTests` | Fehlender Pflichtfeldwert gibt 400 zurück |
| `DeleteApplicationGroup_WithValidId_Returns204AndRotatesToken` | `ApplicationGroupsControllerIntegrationTests` | Erfolgreiches Löschen; Response-Status 204, `X-New-Token`-Header vorhanden |
| `DeleteApplicationGroup_WithInvalidId_Returns404` | `ApplicationGroupsControllerIntegrationTests` | DELETE mit unbekannter Id gibt 404 zurück |
| `PostApplication_WithValidTokenAndRequest_Returns201AndLocation` | `ApplicationsControllerIntegrationTests` | Erfolgreiche Anlage einer Anwendung; Response-Status 201, `Location`-Header vorhanden, `ApplicationResponse` korrekt befüllt *(umgesetzt)* |
| `PostApplication_WithoutToken_Returns401` | `ApplicationsControllerIntegrationTests` | Aufruf ohne Authorization-Header gibt HTTP 401 zurück *(umgesetzt)* |
| `PostApplication_WithMissingName_Returns400` | `ApplicationsControllerIntegrationTests` | Fehlendes Pflichtfeld `Name` führt zu HTTP 400 *(umgesetzt)* |
| `PostApplication_WithMissingBaseUrl_Returns400` | `ApplicationsControllerIntegrationTests` | Fehlendes Pflichtfeld `BaseUrl` führt zu HTTP 400 *(umgesetzt)* |
| `GetApplications_WithValidToken_Returns200WithList` | `ApplicationsControllerIntegrationTests` | GET /api/applications gibt 200 mit korrekter Liste zurück |
| `GetApplications_WithoutToken_Returns401` | `ApplicationsControllerIntegrationTests` | Aufruf ohne Authorization-Header gibt HTTP 401 zurück |
| `GetUngroupedApplications_WithValidToken_Returns200WithList` | `ApplicationsControllerIntegrationTests` | GET /api/applications/ungrouped gibt 200 mit korrekter Liste zurück |
| `GetApplicationById_WithValidId_Returns200WithAllFields` | `ApplicationsControllerIntegrationTests` | GET /api/applications/{id} gibt 200 mit vollständig befüllter `ApplicationResponse` zurück (alle Felder inkl. Description, InterfaceUrl, InterfaceType, Owner) |
| `GetApplicationById_WithInvalidId_Returns404` | `ApplicationsControllerIntegrationTests` | GET /api/applications/{id} mit unbekannter Id gibt 404 zurück |
| `PutApplication_WithValidRequest_Returns200AndRotatesToken` | `ApplicationsControllerIntegrationTests` | Erfolgreiche Bearbeitung; Response-Status 200, geänderte Felder korrekt, `X-New-Token`-Header vorhanden |
| `PutApplication_WithInvalidId_Returns404` | `ApplicationsControllerIntegrationTests` | PUT mit unbekannter Id gibt 404 zurück |
| `PutApplication_WithMissingBaseUrl_Returns400` | `ApplicationsControllerIntegrationTests` | Fehlendes Pflichtfeld `BaseUrl` gibt 400 zurück |
| `DeleteApplication_WithValidId_Returns204AndRotatesToken` | `ApplicationsControllerIntegrationTests` | Erfolgreiches Löschen; Response-Status 204, `X-New-Token`-Header vorhanden |
| `DeleteApplication_WithInvalidId_Returns404` | `ApplicationsControllerIntegrationTests` | DELETE mit unbekannter Id gibt 404 zurück |
| `CreateTokenAsync_ReturnsValidToken` | `TokenStoreTests` | Neu erstellter Token ist im Store vorhanden, nicht abgelaufen, enthält korrekten Benutzernamen *(umgesetzt)* |
| `ValidateAndRotateAsync_WithValidToken_ReturnsNewToken` | `TokenStoreTests` | Gültiger Token wird validiert; alter Token ist danach nicht mehr gültig; neuer Token ist gültig *(umgesetzt)* |
| `ValidateAndRotateAsync_WithExpiredToken_ReturnsNull` | `TokenStoreTests` | Abgelaufener Token gibt `null` zurück *(umgesetzt)* |
| `ValidateAndRotateAsync_WithUnknownToken_ReturnsNull` | `TokenStoreTests` | Unbekannter Token-String gibt `null` zurück *(umgesetzt)* |
| `AddGroupAsync_AuthenticatesAndSendsCorrectRequest_ReturnsResponse` | `ApplicationApiClientTests` | `AddGroupAsync` ruft zuerst `/authenticate` auf, sendet korrekt serialisierten Request mit Bearer-Token und `X-Storage-Mode`-Header, mappt Response auf `ApplicationGroup` *(umgesetzt)* |
| `AddGroupAsync_RotatesTokenAfterSuccessfulCall` | `ApplicationApiClientTests` | Nach dem Aufruf hat der Client den neuen Token aus `X-New-Token` gespeichert *(umgesetzt)* |
| `AddApplicationAsync_AuthenticatesAndSendsCorrectRequest_ReturnsResponse` | `ApplicationApiClientTests` | `AddApplicationAsync` verhält sich analog zu `AddGroupAsync` *(umgesetzt)* |
| `GetGroupsAsync_SendsCorrectHeadersAndReturnsMappedList` | `ApplicationApiClientTests` | `GetGroupsAsync` sendet `X-Storage-Mode`- und `X-Owner`-Header, mappt `ApplicationGroupResponse`-Liste auf `IList<ApplicationGroup>` |
| `GetUngroupedApplicationsAsync_SendsCorrectHeadersAndReturnsMappedList` | `ApplicationApiClientTests` | Analoger Test für `GetUngroupedApplicationsAsync` |
| `GetApplicationByIdAsync_ReturnsNullOn404` | `ApplicationApiClientTests` | `GetApplicationByIdAsync` gibt `null` zurück, wenn der Server 404 antwortet |
| `UpdateGroupAsync_SendsCorrectPutRequestAndReturnsMappedGroup` | `ApplicationApiClientTests` | `UpdateGroupAsync` sendet PUT-Request mit `UpdateApplicationGroupRequest`-Body, mappt Response auf `ApplicationGroup` |
| `DeleteGroupAsync_SendsCorrectDeleteRequest` | `ApplicationApiClientTests` | `DeleteGroupAsync` sendet DELETE-Request und wirft keine Ausnahme bei 204 |
| `UpdateApplicationAsync_SendsCorrectPutRequestAndReturnsMappedApplication` | `ApplicationApiClientTests` | `UpdateApplicationAsync` sendet PUT-Request mit `UpdateApplicationRequest`-Body, mappt alle Felder korrekt |
| `DeleteApplicationAsync_SendsCorrectDeleteRequest` | `ApplicationApiClientTests` | `DeleteApplicationAsync` sendet DELETE-Request und wirft keine Ausnahme bei 204 |

### Betroffene bestehende Tests

| Test | Klasse | Anpassung |
|------|--------|-----------|
| Tests in `ApplicationGroupTreeTests` (sofern vorhanden) | bUnit | Mock von `IApplicationRepository` durch Mock von `IApplicationApiClient` ersetzen |
| Tests in `ApplicationEditorTests` (sofern vorhanden) | bUnit | Mock von `IApplicationRepository` durch Mock von `IApplicationApiClient` ersetzen; `ISignalRNotificationService`-Mock entfernen |
| Tests in `ApplicationCardTests` (sofern vorhanden) | bUnit | Mock von `IApplicationRepository` durch Mock von `IApplicationApiClient` ersetzen |
| `ApplicationContextMenuTests` | bUnit | Keine Anpassung — nicht betroffen |
