# Umsetzungsplan: Neues modernes Design (Redesign)

## Übersicht

Die gesamte UI der Schnittstellenzentrale wird von Bootstrap 5 auf die Komponentenbibliothek **ShadcnBlazor** (NuGet: `ShadcnBlazor` v1.0.14) migriert. Das bestehende Einzel-Layout (`MainLayout`) wird durch ein neues Drei-Bereich-Layout (`AppShell`/`TopBar`) ersetzt; die Bereiche **Workspaces**, **Environments** und **History** werden über einen internen `INavigationStateService` ohne URL-Wechsel gesteuert. Betroffen sind Datenmodell, Datenbankmigrationen, Repository, vier neue Services, ca. 15 neue Razor-Komponenten sowie das vollständige Ersetzen von `MainLayout` und `Home.razor`.

---

## Programmabläufe

### Bereichsnavigation (TopBar → Bereich wechseln)

1. Benutzer klickt auf einen der drei Tabs in `TopBar` (Workspaces / Environments / History).
2. `TopBar` ruft `INavigationStateService.SetAreaAsync(NavigationArea)` auf.
3. `INavigationStateService` aktualisiert `CurrentArea` und feuert `OnAreaChanged`.
4. `AppShell` reagiert auf `OnAreaChanged` und rendert das entsprechende Layout (`WorkspacesLayout`, `EnvironmentsLayout` oder `HistoryLayout`).

Beteiligte Klassen/Komponenten: `TopBar`, `INavigationStateService`, `NavigationStateService`, `AppShell`

---

### Workspaces-Navigation (Baumelement → Inhaltsbereich)

1. Benutzer klickt in `WorkspacesSidebar` auf ein Element (Sammlung, Anwendung, Ordner, Endpunkt).
2. `ApplicationGroupTree` löst den entsprechenden `EventCallback` aus (`OnApplicationGroupSelected`, `OnApplicationSelected`, `OnEndpointGroupSelected`, `OnEndpointSelected`).
3. `WorkspacesSidebar` leitet den Aufruf an `INavigationStateService.SetWorkspaceSelectionAsync(selectedItem)` weiter.
4. `INavigationStateService` aktualisiert `CurrentSelection` und feuert `OnSelectionChanged`.
5. `WorkspacesLayout` reagiert auf `OnSelectionChanged` und rendert die passende Content-View (`CollectionContentView`, `ApplicationContentView`, `FolderContentView`, `EndpointContentView`) oder `EmptyContentView`.
6. `ContentBreadcrumb` leitet seinen Zustand aus `INavigationStateService.CurrentSelectionPath` ab und stellt bis zu vier klickbare Ebenen dar.

Beteiligte Klassen/Komponenten: `WorkspacesSidebar`, `ApplicationGroupTree`, `INavigationStateService`, `WorkspacesLayout`, `ContentBreadcrumb`, `CollectionContentView`, `ApplicationContentView`, `FolderContentView`, `EndpointContentView`, `EmptyContentView`

---

### Breadcrumb-Klick (zurücknavigieren)

1. Benutzer klickt auf ein Element im `ContentBreadcrumb`.
2. `ContentBreadcrumb` ruft `INavigationStateService.SetWorkspaceSelectionAsync(clickedItem)` auf.
3. Weiter wie in „Workspaces-Navigation" ab Schritt 4.

Beteiligte Klassen/Komponenten: `ContentBreadcrumb`, `INavigationStateService`, `WorkspacesLayout`

---

### In-place-Editing (Name / Untertitel)

1. Benutzer fährt mit der Maus über Name oder Untertitel im `ContentHeader`.
2. `ContentHeader` zeigt das Bleistift-Icon (Hover-Zustand via CSS).
3. Benutzer klickt das Bleistift-Icon.
4. `ContentHeader` schaltet in den Bearbeitungsmodus: zeigt ein Inline-`<input>`-Element.
5. Benutzer gibt Text ein. Bei leerem Pflichtfeld wird eine Inline-Fehlermeldung angezeigt.
6. Benutzer bestätigt mit Enter oder Blur → `ContentHeader` ruft den zugehörigen Service auf (z. B. `IApplicationGroupService.UpdateNameAsync` oder `UpdateSubtitleAsync`).
7. Bei Escape wird der Bearbeitungsmodus ohne Änderungen beendet.
8. Nach erfolgreichem Speichern aktualisiert der Service den Datensatz in der Datenbank; `ContentHeader` verlässt den Bearbeitungsmodus.

Beteiligte Klassen/Komponenten: `ContentHeader`, `IApplicationGroupService`, `IApplicationService`

---

### Icon-Upload

1. Benutzer klickt auf das Upload-Icon in `ContentHeader`.
2. `ContentHeader` öffnet einen unsichtbaren `<input type="file" accept="image/png,image/jpeg">` per JS-Interop.
3. Benutzer wählt eine Datei aus.
4. `ContentHeader` prüft clientseitig: Dateityp (`image/png` oder `image/jpeg`) und Dateigröße (≤ 524 288 Bytes).
5. Bei ungültiger Datei: Fehlermeldung, kein Upload.
6. Bei gültiger Datei: Datei wird als `byte[]` gelesen und an den zugehörigen Service übergeben (`IApplicationGroupService.UpdateIconAsync` bzw. `IApplicationService.UpdateIconAsync`).
7. Service persistiert die Bytes in `ApplicationGroup.IconData` bzw. `Application.IconData`.
8. `ContentHeader` zeigt das neue Icon.

Beteiligte Klassen/Komponenten: `ContentHeader`, `IApplicationGroupService`, `IApplicationService`

---

### Links-Verwaltung (CRUD in ApplicationContentView)

1. `ApplicationContentView` lädt beim Initialisieren alle Links der Anwendung via `IApplicationLinkService.GetLinksAsync(applicationId)`.
2. Benutzer klickt „+ Link hinzufügen" in `LinksManager`.
3. `LinksManager` zeigt ein Inline-Formular (URL + Beschriftung).
4. Nach Bestätigung ruft `LinksManager` `IApplicationLinkService.AddLinkAsync(link)` auf.
5. Nach erfolgreichem Speichern wird die Liste neu geladen.
6. Analog für Bearbeiten (`UpdateLinkAsync`) und Löschen (`DeleteLinkAsync`).

Beteiligte Klassen/Komponenten: `ApplicationContentView`, `LinksManager`, `IApplicationLinkService`, `ApplicationLinkService`, `IApplicationLinkRepository`, `ApplicationLinkRepository`

---

### History-Persistenz (Endpunkt wird ausgeführt)

1. Benutzer löst eine Endpunktausführung aus.
2. `EndpointExecutionService.ExecuteAsync` führt den HTTP-Aufruf durch.
3. Nach der Ausführung schreibt `EndpointExecutionService` zusätzlich einen `EndpointCallHistoryEntry` via `IHistoryService.AddEntryAsync(entry)`.
4. `IHistoryService` persistiert den Eintrag in der `EndpointCallHistory`-Tabelle.

Beteiligte Klassen/Komponenten: `EndpointExecutionService`, `IHistoryService`, `HistoryService`, `EndpointCallHistoryEntry`

---

### History-Anzeige (HistoryContentView)

1. `HistoryContentView` wird beim Betreten des History-Bereichs initialisiert.
2. `HistoryContentView` ruft `IHistoryService.GetPagedAsync(filter, page, pageSize)` auf.
3. `IHistoryService` fragt die `EndpointCallHistory`-Tabelle ab (mit Filterung nach Anwendung, Endpunkt, Zeitraum; absteigende Sortierung nach Ausführungszeitpunkt) und gibt eine Ergebnisseite zurück.
4. `HistoryContentView` rendert die Liste mit Paginierungssteuerung.
5. Bei Filteränderung oder Seitenwechsel wird Schritt 2 wiederholt.

Beteiligte Klassen/Komponenten: `HistoryContentView`, `IHistoryService`, `HistoryService`

---

### Top-5-Endpunkte (ApplicationContentView)

1. `ApplicationContentView` ruft beim Initialisieren `IHistoryService.GetTopEndpointsAsync(applicationId, 5)` auf.
2. `IHistoryService` aggregiert die Aufrufhäufigkeit aus der `EndpointCallHistory`-Tabelle für die gegebene Anwendung.
3. `ApplicationTopEndpointsTable` rendert das Ergebnis.

Beteiligte Klassen/Komponenten: `ApplicationContentView`, `ApplicationTopEndpointsTable`, `IHistoryService`, `HistoryService`

---

### Environments-Bereich

1. Benutzer wechselt in den Environments-Bereich.
2. `EnvironmentsLayout` rendert `EnvironmentsSidebar` (Umgebungsliste + „+ Neue Umgebung"-Button) und `EnvironmentContentView`.
3. Benutzer wählt eine Umgebung in `EnvironmentsSidebar`.
4. `EnvironmentContentView` lädt die Umgebung via `ISystemEnvironmentRepository.GetByIdAsync` und zeigt Name, Beschreibung (editierbar) und Variablentabelle.
5. In-place-Editing für Name und `Description` analog zu `ContentHeader`-Ablauf; Speichern via `ISystemEnvironmentRepository.UpdateAsync`.
6. „+ Neue Umgebung" öffnet ein Formular in `EnvironmentContentView`; Löschen über Lösch-Aktion in `EnvironmentsSidebar`.

Beteiligte Klassen/Komponenten: `EnvironmentsLayout`, `EnvironmentsSidebar`, `EnvironmentContentView`, `ISystemEnvironmentRepository`

---

### shadcn-Migration (Komponentenbibliothek-Wechsel)

1. NuGet-Paket `ShadcnBlazor` v1.0.14 wird hinzugefügt.
2. Bootstrap 5 CSS/JS und Bootstrap Icons werden aus `App.razor` / `_Host.cshtml` entfernt.
3. ShadcnBlazor-Initialisierung (CSS, JS, Service-Registrierung) wird eingetragen.
4. Alle bestehenden Bootstrap-Klassen in Razor-Komponenten werden durch shadcn-Entsprechungen ersetzt.
5. `theme-init.js` wird auf das shadcn-äquivalente Theme-Attribut angepasst.
6. `ThemeService.SetTheme` wird auf das von ShadcnBlazor verwendete Attribut/Klassenname angepasst (z. B. `data-theme` statt `data-bs-theme`).

Beteiligte Klassen/Komponenten: `App.razor`, `ThemeService`, `theme-init.js`, alle Razor-Komponenten

---

## Neue Klassen

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `AppShell` | Razor-Komponente (Layout) | Neues Root-Layout; rendert `TopBar` und das aktive Bereichs-Layout |
| `TopBar` | Razor-Komponente | Titelleiste: App-Name, Bereichs-Tabs, StorageMode-Selector, `ThemeToggle`, Profil-Icon (Kreissymbol mit Initiale des Benutzernamens), Einstellungs-Link, Hilfe-Icon |
| `WorkspacesLayout` | Razor-Komponente (Layout) | Zwei-Spalten-Layout: `WorkspacesSidebar` + Inhaltsbereich |
| `WorkspacesSidebar` | Razor-Komponente | Seitenleiste Workspaces: „+ New Collection"-Button, eingebetteter `ApplicationGroupTree` |
| `ContentBreadcrumb` | Razor-Komponente | Klickbare Breadcrumb-Leiste (max. 4 Ebenen) |
| `ContentHeader` | Razor-Komponente | Kopfbereich: Upload-Icon, Name (In-place), Untertitel (In-place); Read-only-Variante für Ordner/Endpunkt |
| `CollectionContentView` | Razor-Komponente | Inhaltsblöcke für eine Sammlung: Beschreibungsblock + Statusblock (Anzahl Anwendungen/Endpunkte) |
| `ApplicationContentView` | Razor-Komponente | Inhaltsblöcke für eine Anwendung: Beschreibung, URLs, Statusblock, `LinksManager`, `ApplicationTopEndpointsTable` |
| `FolderContentView` | Razor-Komponente | Inhaltsblöcke für einen Ordner: Tabelle aller Endpunkte des Ordners |
| `LinksManager` | Razor-Komponente | CRUD-Verwaltung von `ApplicationLink`-Einträgen (Inline-Formular) |
| `ApplicationTopEndpointsTable` | Razor-Komponente | Tabelle der Top-5-Endpunkte nach Aufrufhäufigkeit |
| `EnvironmentsLayout` | Razor-Komponente (Layout) | Zwei-Spalten-Layout: `EnvironmentsSidebar` + `EnvironmentContentView` |
| `EnvironmentsSidebar` | Razor-Komponente | Seitenleiste Environments: Umgebungsliste, „+ Neue Umgebung"-Button, Lösch-Aktion |
| `EnvironmentContentView` | Razor-Komponente | Inhaltsbereich Umgebung: Name (editierbar), Beschreibung (editierbar), Variablentabelle |
| `HistoryLayout` | Razor-Komponente (Layout) | Layout für den History-Bereich |
| `HistoryContentView` | Razor-Komponente | Liste vergangener API-Aufrufe mit Filterung, Paginierung, Sortierung |
| `EmptyContentView` | Razor-Komponente | Platzhalteransicht mit Text „Wählen Sie eine Sammlung oder Anwendung aus." |
| `HelpPage` | Razor-Komponente (Page, `@page "/help"`) | Leere Hilfe-Seite mit leerem `MainMenu` |
| `INavigationStateService` | Interface | Zustandsdienst für Bereichsnavigation und Workspace-Selektion (ohne URL-Wechsel) |
| `NavigationStateService` | Klasse (Scoped Service) | Implementierung von `INavigationStateService` |
| `NavigationArea` | Enum | Werte: `Workspaces`, `Environments`, `History` |
| `WorkspaceSelection` | Datenmodellklasse (record) | Enthält den aktuell selektierten Knoten und den vollständigen Pfad für das Breadcrumb |
| `ApplicationLink` | Datenmodellklasse (EF-Entity) | Verknüpfte URL je Anwendung; Felder: `Id`, `ApplicationId`, `Url`, `Label`, `SortOrder`, `RowVersion` |
| `EndpointCallHistoryEntry` | Datenmodellklasse (EF-Entity) | Persistierter Aufruf-Datensatz; Felder: `Id`, `ApplicationId`, `EndpointId`, `ExecutedAt`, `HttpMethod`, `RelativePath`, `StatusCode`, `DurationMs` |
| `IApplicationGroupService` | Interface | Service-Interface für Sammlung-Operationen (Inline-Editing, Icon-Upload) |
| `ApplicationGroupService` | Klasse (Scoped Service) | Implementierung von `IApplicationGroupService` |
| `IApplicationService` | Interface | Service-Interface für Anwendungs-Operationen (Subtitle, Icon, Top-Endpunkte) |
| `ApplicationService` | Klasse (Scoped Service) | Implementierung von `IApplicationService` |
| `IApplicationLinkService` | Interface | Service-Interface für `ApplicationLink`-CRUD |
| `ApplicationLinkService` | Klasse (Scoped Service) | Implementierung von `IApplicationLinkService` |
| `IApplicationLinkRepository` | Interface | Repository-Interface für `ApplicationLink`-Datenbankzugriffe |
| `ApplicationLinkRepository` | Klasse | Implementierung von `IApplicationLinkRepository` |
| `IHistoryService` | Interface | Service-Interface für persistente Aufrufhistorie (separate `EndpointCallHistory`-Tabelle) |
| `HistoryService` | Klasse (Scoped Service) | Implementierung von `IHistoryService` |
| `UploadSettings` | Konfigurationsklasse | Konfigurationseintrag `Upload:MaxIconSizeBytes` |
| `HistorySettings` | Konfigurationsklasse | Konfigurationseintrag `History:DefaultPageSize` |

---

## Änderungen an bestehenden Klassen

### `ApplicationGroup` (Datenmodellklasse)

- **Neue Eigenschaften:**
  - `Description` (`string?`) — Beschreibungstext der Sammlung; nullable
  - `Subtitle` (`string?`) — Untertitel für In-place-Editing; nullable
  - `IconData` (`byte[]?`) — Icon-Bytes (PNG/JPG); nullable

### `Application` (Datenmodellklasse)

- **Neue Eigenschaften:**
  - `Subtitle` (`string?`) — Untertitel für In-place-Editing; nullable
  - `IconData` (`byte[]?`) — Icon-Bytes (PNG/JPG); nullable
  - `Links` (`ICollection<ApplicationLink>`) — Navigationseigenschaft zu verknüpften URLs

### `SystemEnvironment` (Datenmodellklasse)

- **Neue Eigenschaften:**
  - `Description` (`string?`) — Beschreibungstext der Umgebung; nullable

### `IApplicationRepository` (Interface)

- **Neue Methoden:**
  - `GetApplicationCountByGroupAsync(int groupId)` — Anzahl Anwendungen je Gruppe; Rückgabe `Task<int>`
  - `GetEndpointCountByGroupAsync(int groupId)` — Anzahl Endpunkte je Gruppe; Rückgabe `Task<int>`

### `ApplicationRepository` (Klasse)

- **Neue Methoden:** Implementierungen der neuen `IApplicationRepository`-Methoden (`GetApplicationCountByGroupAsync`, `GetEndpointCountByGroupAsync`).

### `EndpointExecutionService` (Klasse)

- **Geänderte Methoden:** `ExecuteAsync` — nach erfolgreicher oder fehlgeschlagener Ausführung zusätzlich `IHistoryService.AddEntryAsync` aufrufen, um einen `EndpointCallHistoryEntry` zu persistieren.
- **Neue Event-Handler:** Abhängigkeit auf `IHistoryService` (Dependency Injection).

### `MainLayout` (Razor-Komponente)

- Wird vollständig durch `AppShell` ersetzt. Die bestehende Logik (SignalR, `ThemeService`, `StorageModeService`, `ActiveEnvironmentService`, localStorage-Wiederherstellung) wird in `AppShell` und `TopBar` migriert. `MainLayout` wird gelöscht oder auf einen leeren Wrapper reduziert, der `AppShell` rendert.

### `ApplicationGroupTree` (Razor-Komponente)

- **Geänderte Methoden:** Navigationsaufrufe (bisher direkte Parameter-Callbacks zu `Home.razor`) werden auf `INavigationStateService.SetWorkspaceSelectionAsync` umgestellt.
- **Neue Event-Handler:** Neue Callbacks für `OnApplicationGroupSelected` und `OnEndpointGroupSelected`, sofern noch nicht vorhanden.
- **Geänderte Beschriftungen:** „Neue Gruppe" → „New Collection" (bzw. „Neue Sammlung" gemäß UI-Sprache).

### `Home.razor` (Razor-Page)

- Wird durch das neue `AppShell`-Layout vollständig abgelöst. `Home.razor` wird auf eine minimale Weiterleitung (oder direktes Rendern von `AppShell`) reduziert; die gesamte `if-else-if`-Inhaltslogik wird in `WorkspacesLayout` und die einzelnen Content-Views verlagert.

### `ThemeService` / `IThemeService` (Service)

- **Geänderte Methoden:** `SetTheme` und `PersistTheme` müssen das Theme-Attribut von `data-bs-theme` auf das von ShadcnBlazor verwendete Attribut umstellen; `theme-init.js` entsprechend anpassen.

### `DbContext` (EF-Klasse)

- **Neue Eigenschaften:** `DbSet<ApplicationLink>` — neue Tabelle.
- **Neue Eigenschaften:** `DbSet<EndpointCallHistoryEntry>` — neue Tabelle.

### `Program.cs` / Service-Registrierung

- **Neu:** Registrierung aller neuen Services und Repositories (`INavigationStateService`, `IApplicationGroupService`, `IApplicationService`, `IApplicationLinkService`, `IApplicationLinkRepository`, `IHistoryService`, `HistoryService`).
- **Neu:** Bindung der Konfigurationsklassen `UploadSettings` und `HistorySettings`.
- **Neu:** ShadcnBlazor-Service-Registrierung (gemäß Paket-Dokumentation).

---

## Datenbankmigrationen

Alle neuen Spalten sind nullable; vorhandene Datensätze bleiben kompatibel.

| Migrationsname | Betroffene Tabellen/Spalten | Beschreibung der Änderung |
|----------------|----------------------------|---------------------------|
| `AddApplicationGroupDescriptionSubtitleIcon` | `ApplicationGroups`: `Description` (`nvarchar(max)`, nullable), `Subtitle` (`nvarchar(max)`, nullable), `IconData` (`varbinary(max)`, nullable) | Drei neue nullable Spalten für Sammlung |
| `AddApplicationSubtitleIcon` | `Applications`: `Subtitle` (`nvarchar(max)`, nullable), `IconData` (`varbinary(max)`, nullable) | Zwei neue nullable Spalten für Anwendung |
| `AddSystemEnvironmentDescription` | `SystemEnvironments`: `Description` (`nvarchar(max)`, nullable) | Neue nullable Beschreibungsspalte für Umgebung |
| `AddApplicationLinkTable` | Neue Tabelle `ApplicationLinks`: `Id` (PK), `ApplicationId` (FK → `Applications`, nicht nullable), `Url` (`nvarchar(max)`, nullable), `Label` (`nvarchar(max)`, nullable), `SortOrder` (`int`, nullable), `RowVersion` (`rowversion`) | Neue Entität für Links je Anwendung |
| `AddEndpointCallHistoryTable` | Neue Tabelle `EndpointCallHistory`: `Id` (PK, `bigint`), `ApplicationId` (FK → `Applications`, nullable), `EndpointId` (FK → `Endpoints`, nullable), `ExecutedAt` (`datetime2`, nullable), `HttpMethod` (`nvarchar(20)`, nullable), `RelativePath` (`nvarchar(max)`, nullable), `StatusCode` (`int`, nullable), `DurationMs` (`int`, nullable) | Persistente Aufrufhistorie |

---

## Validierungsregeln

| Feld / Objekt | Regel | Fehlerfall |
|---------------|-------|------------|
| `ContentHeader` — Name (In-place) | Pflichtfeld; nicht leer oder nur Leerzeichen | Inline-Fehlermeldung; Speichern gesperrt |
| `ContentHeader` — Untertitel (In-place) | Optional; kein spezifisches Format | — |
| `ContentHeader` — Icon-Upload: Dateiformat | Nur `image/png` oder `image/jpeg` | Fehlermeldung; kein Upload |
| `ContentHeader` — Icon-Upload: Dateigröße | Maximal 524 288 Bytes (512 KB) | Fehlermeldung; kein Upload |
| `ApplicationLink.Url` | Pflichtfeld; valide URL (beginnt mit `http://` oder `https://`) | Inline-Fehlermeldung; Speichern gesperrt |
| `ApplicationLink.Label` | Optional; max. 200 Zeichen | Inline-Fehlermeldung wenn überschritten |
| `SystemEnvironment.Name` (In-place) | Pflichtfeld; max. 200 Zeichen (bestehende Regel bleibt) | Inline-Fehlermeldung; Speichern gesperrt |

---

## Konfigurationsänderungen

| Eintrag | Typ | Standardwert | Zweck |
|---------|-----|--------------|-------|
| `Upload:MaxIconSizeBytes` | `int` | `524288` | Maximale Icon-Dateigröße in Bytes (512 KB); ausgelesen von `ContentHeader` via `UploadSettings` |
| `History:DefaultPageSize` | `int` | `50` | Standard-Seitengröße für `HistoryContentView` |

---

## Seiteneffekte und Risiken

- **`MainLayout`-Tests (`MainLayoutTests`):** Alle zehn bestehenden Tests beziehen sich auf `MainLayout`; da `MainLayout` durch `AppShell` ersetzt wird, müssen diese Tests auf `AppShell` / `TopBar` umgeschrieben werden.
- **Bootstrap-Migration:** Alle bestehenden Playwright-Tests und Unit-Tests, die Bootstrap-CSS-Selektoren (Klassen wie `btn`, `navbar`, `modal`) verwenden, können fehlschlagen, sobald shadcn-Komponenten andere DOM-Strukturen erzeugen. Alle Playwright-Selektoren sind nach der Migration zu überprüfen.
- **`ActivityLogService` bleibt In-Memory:** Der bestehende `IActivityLogService` wird nicht verändert. Das In-Memory-ActivityLog-Panel aus `MainLayout` entfällt durch das neue Layout; bestehende Debugging-Funktionalität muss bewusst migriert oder gestrichen werden.
- **`EndpointExecutionService`-Erweiterung:** Das Hinzufügen von `IHistoryService` als Abhängigkeit zu `EndpointExecutionService` macht das Anpassen bestehender Unit-Tests für `EndpointExecutionService` erforderlich (Mock für `IHistoryService` nötig).
- **`ApplicationGroupTree`-Callbacks:** Die Umstellung der Callback-Parameter auf `INavigationStateService` beeinflusst Playwright-Tests in `ApplicationCrudTests` und `HomePageTests`, wenn diese das Navigationsverhalten indirekt testen.
- **Icon-Speicherung als `byte[]`:** Große Datenbanken mit vielen Icons können die Datenbankgröße deutlich erhöhen. Kein unmittelbares technisches Risiko, aber ein operatives.
- **EF-Migrationen auf bestehenden Installationen:** Alle neuen Spalten sind nullable; vorhandene Datensätze bleiben kompatibel. `ApplicationLinks`- und `EndpointCallHistory`-Tabellen werden neu angelegt; kein Datenverlustrisiko.

---

## Umsetzungsreihenfolge

1. **NuGet-Paket hinzufügen:** `ShadcnBlazor` v1.0.14 installieren; Bootstrap 5 / Bootstrap Icons entfernen; ShadcnBlazor in `Program.cs` und `App.razor` einbinden.
2. **Enum `NavigationArea`** anlegen (wird vor allen Services und Komponenten benötigt).
3. **Datenmodellklassen erweitern:** `ApplicationGroup` (`Description`, `Subtitle`, `IconData`), `Application` (`Subtitle`, `IconData`, `Links`), `SystemEnvironment` (`Description`).
4. **Neue Datenmodellklassen anlegen:** `ApplicationLink`, `EndpointCallHistoryEntry`.
5. **`DbContext` erweitern:** `DbSet<ApplicationLink>`, `DbSet<EndpointCallHistoryEntry>` hinzufügen; FK-Konfigurationen eintragen.
6. **EF-Migrationen erstellen:** Alle fünf Migrationen aus der Tabelle oben in der genannten Reihenfolge.
7. **`WorkspaceSelection`-Klasse** anlegen (wird vor `INavigationStateService` benötigt).
8. **Interfaces anlegen:** `INavigationStateService`, `IApplicationGroupService`, `IApplicationService`, `IApplicationLinkService`, `IApplicationLinkRepository`, `IHistoryService`.
9. **Repository-Klassen implementieren:** `IApplicationRepository` um Zählmethoden erweitern + `ApplicationRepository`-Implementierung; `ApplicationLinkRepository` anlegen.
10. **Service-Klassen implementieren:** `NavigationStateService`, `ApplicationGroupService`, `ApplicationService`, `ApplicationLinkService`, `HistoryService`.
11. **Konfigurationsklassen anlegen:** `UploadSettings`, `HistorySettings`; Einträge in `appsettings.json` hinzufügen.
12. **`EndpointExecutionService` erweitern:** `IHistoryService`-Abhängigkeit hinzufügen; `ExecuteAsync` um History-Persistenz ergänzen.
13. **`ThemeService` / `theme-init.js` anpassen:** Theme-Attribut auf shadcn-Konvention umstellen.
14. **Service-Registrierung in `Program.cs`:** Alle neuen Services und Repositories registrieren; Konfigurationsklassen binden.
15. **Neue Razor-Komponenten anlegen (Layout-Ebene):** `AppShell`, `TopBar`, `WorkspacesLayout`, `EnvironmentsLayout`, `HistoryLayout`.
16. **`MainLayout` ersetzen:** Logik aus `MainLayout` in `AppShell`/`TopBar` migrieren; `MainLayout` entfernen.
17. **`ApplicationGroupTree` anpassen:** Callbacks auf `INavigationStateService` umstellen; Beschriftungen auf „Sammlung" anpassen.
18. **Neue Razor-Komponenten anlegen (Inhaltsebene):** `WorkspacesSidebar`, `ContentBreadcrumb`, `ContentHeader`, `EmptyContentView`.
19. **Content-Views anlegen:** `CollectionContentView`, `ApplicationContentView` (inkl. `LinksManager` und `ApplicationTopEndpointsTable`), `FolderContentView`.
20. **`EndpointContentView`** in neues Layout einbetten (bestehende Endpunktdarstellung).
21. **Environments-Bereich:** `EnvironmentsSidebar`, `EnvironmentContentView` anlegen; bestehende `EnvironmentManagementOverlay`/`EnvironmentEditor`-Logik integrieren.
22. **`HistoryContentView`** anlegen.
23. **`HelpPage`** anlegen (`@page "/help"`; leere Seite mit leerem `MainMenu`).
24. **`Home.razor` ablösen:** Auf minimalen Wrapper reduzieren; gesamte Inhaltslogik ist nun in Content-Views.
25. **UI-Umbenennung:** Alle verbleibenden „Anwendungsgruppe"/`ApplicationGroup`-Labels in der UI auf „Sammlung" umstellen.
26. **Bestehende Tests anpassen:** `MainLayoutTests` auf `AppShell`/`TopBar` umschreiben; Playwright-Selektoren prüfen und aktualisieren.
27. **Neue Tests schreiben** (s. Abschnitt Tests).

---

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `NavigationStateService_SetArea_FeuertOnAreaChanged` | `NavigationStateServiceTests` | `OnAreaChanged` wird nach `SetAreaAsync` gefeuert |
| `NavigationStateService_SetSelection_FeuertOnSelectionChanged` | `NavigationStateServiceTests` | `OnSelectionChanged` wird nach `SetWorkspaceSelectionAsync` gefeuert |
| `ApplicationGroupService_UpdateIcon_PersistiertBytes` | `ApplicationGroupServiceTests` | Icon wird als `byte[]` korrekt gespeichert |
| `ApplicationGroupService_UpdateIcon_ZuGroßeDatei_WirftException` | `ApplicationGroupServiceTests` | Dateigröße > 524 288 Bytes wird abgelehnt |
| `ApplicationLinkService_GetLinks_GibtLinksZurück` | `ApplicationLinkServiceTests` | Alle Links einer Anwendung werden zurückgegeben |
| `ApplicationLinkService_AddLink_PersistiertLink` | `ApplicationLinkServiceTests` | Neuer Link wird in Datenbank gespeichert |
| `ApplicationLinkService_UpdateLink_AktualisierLink` | `ApplicationLinkServiceTests` | Vorhandener Link wird aktualisiert |
| `ApplicationLinkService_DeleteLink_EntferntLink` | `ApplicationLinkServiceTests` | Link wird gelöscht |
| `HistoryService_AddEntry_PersistiertEintrag` | `HistoryServiceTests` | Eintrag wird in `EndpointCallHistory` gespeichert |
| `HistoryService_GetPaged_ReturnsKorrekteSortiertheitUndFilterung` | `HistoryServiceTests` | Filterung nach Anwendung, Zeitraum; absteigende Sortierung; Paginierung |
| `HistoryService_GetTopEndpoints_GibtTop5Zurück` | `HistoryServiceTests` | Aggregation nach Aufrufhäufigkeit korrekt |
| `Playwright_BereichswechselWorkspaces_ZeigtSidebar` | `NavigationTests` (neu) | Klick auf Workspaces-Tab zeigt `WorkspacesSidebar` |
| `Playwright_BereichswechselEnvironments_ZeigtUmgebungsliste` | `NavigationTests` | Klick auf Environments-Tab zeigt Umgebungsliste |
| `Playwright_BereichswechselHistory_ZeigtHistorieliste` | `NavigationTests` | Klick auf History-Tab zeigt Historieliste |
| `Playwright_BreadcrumbKlick_NavigiertZurückZurSammlung` | `NavigationTests` | Breadcrumb-Klick setzt Selektion auf Sammlungsebene |
| `Playwright_InplaceEditing_Sammlung_Name_Speichern` | `InplaceEditingTests` (neu) | Name einer Sammlung wird inline editiert und gespeichert |
| `Playwright_InplaceEditing_Sammlung_Name_PflichfeldValidierung` | `InplaceEditingTests` | Leerer Name wird nicht akzeptiert |
| `Playwright_InplaceEditing_Anwendung_Subtitle_Speichern` | `InplaceEditingTests` | Untertitel einer Anwendung wird gespeichert |
| `Playwright_InplaceEditing_Escape_BrichtAb` | `InplaceEditingTests` | Escape beendet Bearbeitungsmodus ohne Änderung |
| `Playwright_IconUpload_ValideDatei_ZeigtIcon` | `IconUploadTests` (neu) | Valide PNG-Datei wird hochgeladen und angezeigt |
| `Playwright_IconUpload_FalschesFormat_ZeigtFehler` | `IconUploadTests` | Nicht-PNG/JPG-Datei zeigt Fehlermeldung |
| `Playwright_IconUpload_ZuGroßeDatei_ZeigtFehler` | `IconUploadTests` | Datei > 512 KB zeigt Fehlermeldung |
| `CreateInMemoryDbContextWithHistory()` | `TestHelpers` | Erstellt InMemory-DbContext mit `EndpointCallHistory`-DbSet für Integrationstests |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| `MainLayoutTests` (alle 10 Tests) | `MainLayout` wird durch `AppShell`/`TopBar` ersetzt; Tests müssen auf die neuen Komponenten umgeschrieben werden |
| `HomePageTests.StartPage_ShowsSystemGroup` | DOM-Selektoren ändern sich durch shadcn-Migration und neues Layout |
| `HomePageTests.StartPage_ShowsOwnApiEndpoints` | DOM-Selektoren ändern sich durch shadcn-Migration und neues Layout |
| `StorageModeTests` (beide Tests) | StorageMode-Selektor befindet sich nun in `TopBar`, nicht mehr in `MainLayout`; Selektoren anpassen |
| `EnvironmentManagementTests.MaskierterWert_IstNichtImKlartextImDomSichtbar` | Umgebungsverwaltung ist nun in `EnvironmentsLayout`/`EnvironmentContentView`; Öffnungsweg und Selektoren ändern sich |
| `ApplicationCrudTests` (alle Tests) | `ApplicationGroupTree`-Callbacks und DOM-Struktur ändern sich durch shadcn-Migration; Selektoren prüfen |

---

## Offene Punkte

Keine.
