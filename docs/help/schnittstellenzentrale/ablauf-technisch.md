# Schnittstellenzentrale — Technischer Ablauf

## Übersicht

Die Anwendung folgt einem klassischen Blazor-Server-Muster: UI-Komponenten rufen Services über Dependency Injection auf, Services kommunizieren mit der Datenbankschicht über Repositories und mit externen Systemen über `IHttpClientFactory`. Schreiboperationen im Team-Modus lösen anschließend SignalR-Broadcasts aus. Der Ablauf gliedert sich in die folgenden Hauptbereiche: Anwendungsstart, Bereichsnavigation, Workspaces-Navigation und Inhaltsdarstellung, In-place-Editing, Icon-Upload, Links-Verwaltung, History-Persistenz, Environments-Bereich, Datenzugriff, Endpunktausführung und Import.

---

## Ablauf: Anwendungsstart

### 1. Konfiguration lesen und Dienste registrieren

`Program.cs` liest `appsettings.json` und registriert alle Dienste in der DI:

- `DatabaseProviderFactory.RegisterDbContext` wertet `DatabaseProvider` aus und registriert `IDbContextFactory<AppDbContext>` mit SQLite oder SQL Server.
- Windows-Authentifizierung wird über `AddNegotiate()` konfiguriert.
- Alle bisherigen Services werden als Scoped oder Singleton registriert.
- Neue Scoped-Services: `INavigationStateService`, `IApplicationGroupService`, `IApplicationService`, `IApplicationLinkService`, `IApplicationLinkRepository`, `IHistoryService`.
- Neue Konfigurationsklassen werden gebunden: `UploadSettings` aus `Upload`-Sektion, `HistorySettings` aus `History`-Sektion.
- `builder.Services.AddShadcnBlazor()` registriert die ShadcnBlazor-Bibliothek.
- `SystemEndpointSyncService` wird als `IHostedService` via `AddHostedService<SystemEndpointSyncService>()` registriert.
- `EndpointHub` wird unter `/hubs/endpoint` gemappt.
- Serilog wird als Logging-Provider mit EventLog- und Datei-Sink konfiguriert.

### 2. Datenbankinitialisierung und Systemeinträge

Nach `app.Build()` und vor `app.Run()` werden zwei vorbereitende Schritte ausgeführt:

- `EnsureDatabaseInitializedAsync` führt EF-Core-Migrationen aus (`dbContext.Database.MigrateAsync()`).
- `SystemEntryInitializer.InitializeAsync` legt — sofern `Api:BaseUrl` konfiguriert ist — die Systemgruppe und -anwendung „Schnittstellenzentrale" an oder aktualisiert deren `BaseUrl` / `InterfaceUrl`.

### 3. Automatischer Endpunktabgleich (SystemEndpointSyncService)

Nach `app.Run()` startet der Host den registrierten `SystemEndpointSyncService`. `ExecuteAsync` wird einmalig ausgeführt:

1. Ein neuer DI-Scope wird über `IServiceScopeFactory.CreateScope()` erzeugt.
2. `IApplicationRepository.GetSystemGroupAsync()` liefert die Systemgruppe. Gibt sie `null` zurück, wird eine Warnung geloggt und der Abgleich übersprungen.
3. Aus der Systemgruppe wird die Anwendung mit `IsSystem == true` ermittelt. Ist keine vorhanden, analog: Warnung loggen und beenden.
4. `ISwaggerImportService.ImportAsync(systemApp)` ruft die Swagger-Definition ab und berechnet den Diff.
5. Ist `diff.ErrorMessage != null`, wird ein Fehler geloggt und `ExecuteAsync` beendet.
6. Für jeden Eintrag in `diff.NewEndpoints` → `IEndpointRepository.AddEndpointAsync`.
7. Für jeden Eintrag in `diff.RemovedEndpoints` → `IEndpointRepository.DeleteEndpointAsync`.
8. Für jeden Eintrag in `diff.ChangedEndpoints` → `IEndpointRepository.UpdateEndpointNameAsync` (nur der `Name` wird überschrieben).
9. Unerwartete Exceptions werden im `catch`-Block auf `Error`-Level geloggt; die Anwendung startet in jedem Fall normal weiter.

Beteiligte Komponenten: `SystemEndpointSyncService`, `IServiceScopeFactory`, `IApplicationRepository`, `ISwaggerImportService`, `IEndpointRepository`, `ImportDiff`

```mermaid
flowchart TD
    A[app.Run] --> B[SystemEndpointSyncService.ExecuteAsync]
    B --> C[GetSystemGroupAsync]
    C --> D{Systemgruppe vorhanden?}
    D -- Nein --> E[Warnung loggen\nAbbruch]
    D -- Ja --> F[IsSystem-Anwendung ermitteln]
    F --> G{Systemanwendung vorhanden?}
    G -- Nein --> E
    G -- Ja --> H[SwaggerImportService.ImportAsync]
    H --> I{diff.ErrorMessage != null?}
    I -- Ja --> J[Fehler loggen\nAbbruch]
    I -- Nein --> K[NewEndpoints anlegen\nAddEndpointAsync]
    K --> L[RemovedEndpoints löschen\nDeleteEndpointAsync]
    L --> M[ChangedEndpoints umbenennen\nUpdateEndpointNameAsync]
    M --> N[Scope disposed\nExecuteAsync endet]
```

---

## Ablauf: Bereichsnavigation (TopBar → Bereich wechseln)

1. Benutzer klickt auf einen der drei Tabs in `TopBar` (Workspaces / Environments / History).
2. `TopBar` ruft `INavigationStateService.SetAreaAsync(NavigationArea)` auf.
3. `NavigationStateService` aktualisiert `CurrentArea` und feuert `OnAreaChanged`.
4. `AppShell` reagiert auf `OnAreaChanged` und rendert das entsprechende Layout (`WorkspacesLayout`, `EnvironmentsLayout` oder `HistoryLayout`).

Beteiligte Komponenten: `TopBar`, `INavigationStateService`, `NavigationStateService`, `AppShell`, `NavigationArea` (Enum: `Workspaces`, `Environments`, `History`)

---

## Ablauf: Workspaces-Navigation (Baumelement → Inhaltsbereich)

1. Benutzer klickt in `WorkspacesSidebar` auf ein Element (Sammlung, Anwendung, Ordner, Endpunkt).
2. `ApplicationGroupTree` löst den entsprechenden `EventCallback` aus.
3. `WorkspacesSidebar` ruft `INavigationStateService.SetWorkspaceSelectionAsync(new WorkspaceSelection(item, path))` auf.
4. `NavigationStateService` aktualisiert `CurrentSelection` und feuert `OnSelectionChanged`.
5. `WorkspacesLayout` reagiert auf `OnSelectionChanged` und rendert die passende Content-View (`CollectionContentView`, `ApplicationContentView`, `FolderContentView`, `EndpointPage`) oder `EmptyContentView`.
6. `ContentBreadcrumb` leitet seinen Zustand aus `INavigationStateService.CurrentSelectionPath` ab und stellt bis zu vier klickbare Ebenen dar. Ein Klick auf ein Breadcrumb-Element ruft erneut `SetWorkspaceSelectionAsync` mit dem gekürzten Pfad auf.

Beteiligte Komponenten: `WorkspacesSidebar`, `ApplicationGroupTree`, `INavigationStateService`, `WorkspacesLayout`, `ContentBreadcrumb`, `WorkspaceSelection` (record), `CollectionContentView`, `ApplicationContentView`, `FolderContentView`, `EmptyContentView`

---

## Ablauf: In-place-Editing (Name / Untertitel)

1. Benutzer klickt auf Name oder Untertitel im `ContentHeader`.
2. `ContentHeader` schaltet in den Bearbeitungsmodus: zeigt ein Inline-`<input>`-Element.
3. Benutzer gibt Text ein. Bei leerem Pflichtfeld wird die Inline-Fehlermeldung „Name darf nicht leer sein." angezeigt und `SaveNameAsync` kehrt ohne Persistierung zurück.
4. Benutzer bestätigt mit Enter oder Blur → `ContentHeader` ruft `OnNameChanged` bzw. `OnSubtitleChanged` `EventCallback` auf. Die übergeordnete Content-View ruft den zugehörigen Service auf (`IApplicationGroupService.UpdateNameAsync`, `UpdateSubtitleAsync` oder `IApplicationService.UpdateNameAsync`, `UpdateSubtitleAsync`).
5. Bei Escape: `CancelNameEdit` / `CancelSubtitleEdit` beendet den Bearbeitungsmodus ohne Änderung.

Beteiligte Komponenten: `ContentHeader`, `IApplicationGroupService`, `ApplicationGroupService`, `IApplicationService`, `ApplicationService`

---

## Ablauf: Icon-Upload

1. Benutzer klickt auf das Upload-Icon in `ContentHeader`.
2. `ContentHeader` öffnet den unsichtbaren `<InputFile>` per JS-Interop (`eval document.querySelector... .click()`).
3. Benutzer wählt eine Datei aus.
4. `OnFileSelected` prüft `file.ContentType` (nur `image/png` oder `image/jpeg`) und `file.Size` (≤ `UploadSettings.MaxIconSizeBytes`, Standard 524 288 Bytes).
5. Bei ungültiger Datei: `_uploadError` wird gesetzt, kein Upload.
6. Bei gültiger Datei: Bytes werden via `MemoryStream` gelesen und der `OnIconChanged`-`EventCallback` aufgerufen. Die Content-View ruft `IApplicationGroupService.UpdateIconAsync` bzw. `IApplicationService.UpdateIconAsync` auf.
7. `ContentHeader` zeigt das neue Icon als `<img src="data:{mimeType};base64,{base64}">`.

Beteiligte Komponenten: `ContentHeader`, `UploadSettings`, `IApplicationGroupService`, `IApplicationService`

---

## Ablauf: Links-Verwaltung (CRUD in ApplicationContentView)

1. `ApplicationContentView` initialisiert `LinksManager` mit `ApplicationId`.
2. `LinksManager.OnParametersSetAsync` ruft `IApplicationLinkService.GetLinksAsync(applicationId)` auf und zeigt die vorhandenen Links an.
3. **Neu:** Benutzer klickt „+ Link hinzufügen" → Inline-Formular mit URL und Beschriftungsfeld erscheint. Bei Speichern wird URL validiert (Pflichtfeld, muss mit `http://` oder `https://` beginnen) und Beschriftung auf max. 200 Zeichen geprüft; danach `IApplicationLinkService.AddLinkAsync(link)`.
4. **Bearbeiten:** Benutzer klickt den Bearbeiten-Button (✏) → Inline-Formular mit vorausgefüllten Werten; bei Speichern `IApplicationLinkService.UpdateLinkAsync(link)`.
5. **Löschen:** Benutzer klickt den Löschen-Button (🗑) → `IApplicationLinkService.DeleteLinkAsync(linkId)`.
6. Nach jeder Schreiboperation wird die Liste neu geladen.

Beteiligte Komponenten: `ApplicationContentView`, `LinksManager`, `IApplicationLinkService`, `ApplicationLinkService`, `IApplicationLinkRepository`, `ApplicationLinkRepository`

---

## Ablauf: History-Persistenz (Endpunkt wird ausgeführt)

1. Benutzer löst eine Endpunktausführung aus.
2. `EndpointExecutionService.ExecuteAsync` führt den HTTP-Aufruf durch.
3. Nach der Ausführung (Erfolg oder Fehler) wird ein `EndpointCallHistoryEntry` befüllt (`ApplicationId`, `EndpointId`, `ExecutedAt`, `HttpMethod`, `RelativePath`, `StatusCode`, `DurationMs`) und via `IHistoryService.AddEntryAsync(entry)` persistiert.

Beteiligte Komponenten: `EndpointExecutionService`, `IHistoryService`, `HistoryService`, `EndpointCallHistoryEntry`

---

## Ablauf: History-Anzeige (HistoryContentView)

1. Benutzer wechselt in den History-Bereich.
2. `HistoryContentView.OnInitializedAsync` ruft `IHistoryService.GetPagedAsync(filter, page, pageSize)` auf (`pageSize` aus `HistorySettings.DefaultPageSize`).
3. `HistoryService` fragt die `EndpointCallHistory`-Tabelle ab (Filterung nach `From`/`To`, absteigende Sortierung nach `ExecutedAt`) und gibt `(Items, TotalCount)` zurück.
4. `HistoryContentView` rendert die Tabelle mit Paginierungssteuerung (← / →, „Seite X von Y").
5. Bei Filtereingabe oder Seitenwechsel wird `LoadAsync` erneut aufgerufen.

Beteiligte Komponenten: `HistoryContentView`, `IHistoryService`, `HistoryService`, `HistoryFilter`, `HistorySettings`

---

## Ablauf: Top-5-Endpunkte (ApplicationContentView)

1. `ApplicationContentView.OnInitializedAsync` ruft `IHistoryService.GetPagedAsync` (5 Einträge, für Statusblock) und `ApplicationTopEndpointsTable.OnParametersSetAsync` ruft `IHistoryService.GetTopEndpointsAsync(applicationId, 5)` auf.
2. `HistoryService` aggregiert die Aufrufhäufigkeit aus `EndpointCallHistory` für die gegebene `ApplicationId`.
3. `ApplicationTopEndpointsTable` rendert die Ergebniszeilen mit Methode, Pfad und Anzahl der Aufrufe.

Beteiligte Komponenten: `ApplicationContentView`, `ApplicationTopEndpointsTable`, `IHistoryService`, `TopEndpointResult`

---

## Ablauf: Environments-Bereich

1. Benutzer wechselt in den Environments-Bereich.
2. `EnvironmentsLayout` rendert `EnvironmentsSidebar` und — falls eine Umgebung ausgewählt ist — `EnvironmentContentView`.
3. `EnvironmentsSidebar` lädt alle Umgebungen via `ISystemEnvironmentRepository.GetEnvironmentsAsync(mode, owner)` und zeigt sie als Liste. Ein Klick wählt die Umgebung aus (`OnEnvironmentSelected`-Callback).
4. `EnvironmentContentView` lädt die Umgebung via `ISystemEnvironmentRepository.GetByIdAsync(id)` und zeigt Name, Beschreibung (inline editierbar) und die `EnvironmentEditor`-Komponente (Variablentabelle).
5. Name-Speichern: Pflichtfeld, max. 200 Zeichen, bei Enter oder Blur → `ISystemEnvironmentRepository.UpdateAsync`.
6. Beschreibung-Speichern: Optional, bei Blur → `ISystemEnvironmentRepository.UpdateAsync`.
7. **Neue Umgebung:** Button „+ Neue Umgebung" → Inline-Formular in `EnvironmentsSidebar` → bei Speichern `ISystemEnvironmentRepository.AddAsync`.
8. **Löschen:** Button (🗑) neben Eintrag → `ISystemEnvironmentRepository.DeleteAsync`.

Beteiligte Komponenten: `EnvironmentsLayout`, `EnvironmentsSidebar`, `EnvironmentContentView`, `EnvironmentEditor`, `ISystemEnvironmentRepository`

---

## Ablauf: Datenzugriff (StorageMode)

### 1. Modus bestimmen

`StorageModeService` (Scoped, pro Blazor-Circuit) hält den aktiven `StorageMode` (`Team` oder `User`). Standardwert: `Team`.

Beteiligte Komponenten:
- `StorageModeService.CurrentMode` — aktueller Modus der Sitzung
- `StorageModeService.SetMode` — ändert den Modus und feuert `OnModeChanged`

### 2. Benutzernamen ermitteln

`WindowsCurrentUserService.GetCurrentUserName()` liefert den Windows-Benutzernamen über `WindowsIdentity.GetCurrent().Name`.

### 3. Datenbankabfrage (storageMode-bewusst)

`ApplicationRepository.GetGroupsAsync` und `GetUngroupedApplicationsAsync` filtern bei `StorageMode.User` zusätzlich nach `Application.Owner == owner`. Im `Team`-Modus werden alle Datensätze zurückgegeben.

Beteiligte Komponenten:
- `ApplicationRepository.GetGroupsAsync(StorageMode, string owner)` — Gruppen mit zugehörigen Anwendungen
- `ApplicationRepository.GetUngroupedApplicationsAsync(StorageMode, string owner)` — Anwendungen ohne Gruppe
- `AppDbContext.Applications`, `AppDbContext.ApplicationGroups` — EF-Core-DbSets

### 4. UI-Aktualisierung bei Moduswechsel

Alle Komponenten, die `IStorageModeService.OnModeChanged` abonniert haben (`ApplicationGroupTree`, `MainLayout`), rufen `LoadDataAsync` erneut auf und lösen `StateHasChanged` aus.

---

## Ablauf: Endpunktausführung

### 1. Benutzer klickt „Ausführen"

`EndpointExecutionPanel.ExecuteAsync()` ruft `IEndpointExecutionService.ExecuteAsync(endpoint)` auf.

### 2. Authentifizierungsstrategie wählen

`EndpointExecutionService.ExecuteAsync` verzweigt nach `endpoint.AuthenticationType`:

- `NegotiateWithImpersonation` → `ExecuteImpersonatedAsync` (nutzt `WindowsIdentity.RunImpersonated`)
- `Negotiate` → Named HttpClient `"negotiate"` (mit `UseDefaultCredentials = true`)
- Alle anderen → Standard-HttpClient

### 3. Request aufbauen

`BuildRequest` baut die vollständige URL aus `BaseUrl` + `RelativePath` + URL-kodierten Query-Parametern. Header werden per `TryAddWithoutValidation` gesetzt. Body wird als `StringContent` mit `Content-Type` aus den Headern (Fallback: `application/json`) gesetzt.

### 4. Authentifizierung anwenden

`ApplyAuthentication` liest Credentials für Basic und BearerToken aus dem Windows Credential Manager (Schlüssel: `Schnittstellenzentrale:{ApplicationId}:{AuthenticationType}`) und setzt den `Authorization`-Header.

Beteiligte Komponenten:
- `WindowsCredentialService.GetPassword(target)` — liest Passwort/Token via DPAPI

### 5. Request senden und Ergebnis aufbereiten

`BuildResult` liest Response-Body und erstellt `EndpointExecutionResult` mit `Success`, `StatusCode`, `RequestDetails` und `ResponseBody`.

### 6. Verbindungsfehler → Health-Check

Wenn `_result.Success == false && _result.StatusCode == null` (kein HTTP-Status → Verbindungsfehler), ruft `EndpointExecutionPanel` `IHealthCheckService.CheckAsync(application)` auf und zeigt `HealthCheckDialog`.

```mermaid
flowchart TD
    A[Benutzer klickt Ausführen] --> B[EndpointExecutionService.ExecuteAsync]
    B --> C{AuthenticationType}
    C -- NegotiateWithImpersonation --> D[ExecuteImpersonatedAsync\nWindowsIdentity.RunImpersonated]
    C -- Negotiate --> E[HttpClient negotiate\nUseDefaultCredentials]
    C -- Basic/Bearer --> F[ApplyAuthentication\nCredentialService.GetPassword]
    C -- None --> G[Standard HttpClient]
    D & E & F & G --> H[BuildRequest\nURL + Header + Body]
    H --> I[HttpClient.SendAsync]
    I --> J{Verbindungsfehler?\nStatusCode == null}
    J -- Ja --> K[HealthCheckService.CheckAsync\nHealthCheckDialog anzeigen]
    J -- Nein --> L[EndpointExecutionResult anzeigen]
```

## Fehlerbehandlung

- `EndpointExecutionService` fängt alle Ausnahmen und gibt `EndpointExecutionResult.ErrorMessage` zurück; kein unkontrolliertes Weiterwerfen.
- `HealthCheckService` fängt `HttpRequestException`, `TaskCanceledException` und allgemeine Ausnahmen; loggt sie als Warning und gibt `false` zurück.
- `EndpointEditor` fängt `DbUpdateConcurrencyException` und zeigt `ConcurrencyWarningDialog` an.

---

## Ablauf: Swagger- und OData-Import

### 1. Benutzer klickt „Swagger-Import" oder „OData-Import"

`ApplicationCard` ruft `SwaggerImportService.ImportAsync(application)` bzw. `ODataImportService.ImportAsync(application)` auf.

### 2. Definition abrufen

Der Service lädt die Definition über `IHttpClientFactory` (HTTP GET auf `SwaggerUrl` bzw. `MetadataUrl`).

### 3. Definition parsen

- `SwaggerImportService`: Deserialisiert OpenAPI-Dokument via `OpenApiStreamReader` (Microsoft.OpenApi). Iteriert über `document.Paths` und deren `Operations`.
- `ODataImportService`: Parst XML via `CsdlReader.Parse` (Microsoft.OData.Edm). Erzeugt Endpunkte für `EntitySets` (GET + POST) und `IEdmOperation`-Elemente (Actions → POST, Functions → GET).

### 4. Diff berechnen

`ImportDiffCalculator.Calculate(existingEndpoints, importedEndpoints)` vergleicht bestehende und importierte Endpunkte anhand des Schlüssels `{Method}:{RelativePath}`:
- Nur in importiert → `NewEndpoints`
- In beiden, Name unterschiedlich → `ChangedEndpoints`
- Nur in bestehend → `RemovedEndpoints`

### 5. Diff-Vorschau anzeigen

`ImportDialog` zeigt neue (grün), geänderte (gelb) und entfernte (rot) Endpunkte mit Checkboxen. Alle Einträge sind standardmäßig ausgewählt.

### 6. Übernehmen

`ImportDialog.ApplyAsync` ruft für ausgewählte neue Endpunkte `AddEndpointAsync`, für geänderte `UpdateEndpointAsync` und für entfernte `DeleteEndpointAsync` auf.

---

## Ablauf: SignalR-Benachrichtigung (Team-Modus)

### 1. Schreiboperation auslösen

Nach einer Schreiboperation im Team-Modus ruft die UI-Komponente `ISignalRNotificationService.NotifyApplicationChangedAsync(applicationId)` oder `NotifyGroupChangedAsync(groupId)` auf.

Beteiligte Komponenten:
- `SignalRNotificationService<EndpointHub>.NotifyApplicationChangedAsync` — sendet `"ApplicationChanged"` an Gruppe `"application:{id}"`
- `SignalRNotificationService<EndpointHub>.NotifyGroupChangedAsync` — sendet `"GroupChanged"` an Gruppe `"group:{id}"`

### 2. Hub-Abonnement

`EndpointHub` verwaltet Gruppen-Mitgliedschaften:
- `SubscribeToApplication(int applicationId)` → `Groups.AddToGroupAsync("application:{id}")`
- `SubscribeToGroup(int groupId)` → `Groups.AddToGroupAsync("group:{id}")`
