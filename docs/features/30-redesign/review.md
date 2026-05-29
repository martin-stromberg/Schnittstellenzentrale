# Plan-Review

## Ergebnis

**Status:** Offene Aufgaben vorhanden

## Umgesetzte Planelemente

### Neue Klassen / Komponenten

- [x] `AppShell` (Razor-Komponente, Layout) — angelegt; enthält StorageMode, ThemeService, ActiveEnvironmentService, SignalR, OnAreaChanged-Subscription; Logik aus `MainLayout` migriert
- [x] `TopBar` (Razor-Komponente) — angelegt; Bereichs-Tabs, StorageMode-Selektor, EnvironmentSelector, ThemeToggle, Einstellungs-Link, Hilfe-Link, Profil-Icon mit Initiale
- [x] `WorkspacesLayout` (Razor-Komponente, Layout) — angelegt; Zwei-Spalten-Layout, reagiert auf `OnSelectionChanged`, rendert alle Content-Views
- [x] `WorkspacesSidebar` (Razor-Komponente) — angelegt; „+ New Collection"-Button, eingebetteter `ApplicationGroupTree`, Callbacks für alle Navigations- und CRUD-Events gebunden
- [x] `ContentBreadcrumb` (Razor-Komponente) — angelegt; klickbare Breadcrumb-Leiste, ruft `SetWorkspaceSelectionAsync` bei Klick auf
- [x] `ContentHeader` (Razor-Komponente) — angelegt; Icon-Upload, In-place-Name, In-place-Subtitle, ReadOnly-Variante
- [x] `CollectionContentView` (Razor-Komponente) — angelegt; Beschreibungsblock, Statusblock (Anzahl Anwendungen/Endpunkte über `GetApplicationCountByGroupAsync` / `GetEndpointCountByGroupAsync`)
- [x] `ApplicationContentView` (Razor-Komponente) — angelegt; Beschreibung, URLs, `LinksManager`, `ApplicationTopEndpointsTable`
- [x] `FolderContentView` (Razor-Komponente) — angelegt; Tabelle aller Endpunkte des Ordners
- [x] `LinksManager` (Razor-Komponente) — angelegt; CRUD mit Inline-Formular, URL-Pflichtfeldvalidierung, Label-max-200-Zeichen-Validierung
- [x] `ApplicationTopEndpointsTable` (Razor-Komponente) — angelegt; ruft `GetTopEndpointsAsync(ApplicationId, 5)` auf
- [x] `EnvironmentsLayout` (Razor-Komponente, Layout) — angelegt; `EnvironmentsSidebar` + `EnvironmentContentView`
- [x] `EnvironmentsSidebar` (Razor-Komponente) — angelegt; Umgebungsliste, „+ Neue Umgebung"-Button, Lösch-Aktion
- [x] `EnvironmentContentView` (Razor-Komponente) — angelegt; Name editierbar, Beschreibung editierbar, Variablentabelle via `EnvironmentEditor`
- [x] `HistoryLayout` (Razor-Komponente, Layout) — angelegt; bettet `HistoryContentView` ein
- [x] `HistoryContentView` (Razor-Komponente) — angelegt; Filterung nach Zeitraum, Paginierung, absteigende Sortierung
- [x] `EmptyContentView` (Razor-Komponente) — angelegt; Text „Wählen Sie eine Sammlung oder Anwendung aus."
- [x] `HelpPage` (Razor-Komponente, `@page "/help"`) — angelegt
- [x] `INavigationStateService` (Interface) — angelegt; `CurrentArea`, `CurrentSelection`, `CurrentSelectionPath`, `OnAreaChanged`, `OnSelectionChanged`, `SetAreaAsync`, `SetWorkspaceSelectionAsync`
- [x] `NavigationStateService` (Klasse, Scoped Service) — angelegt; vollständige Implementierung
- [x] `NavigationArea` (Enum) — angelegt; Werte `Workspaces`, `Environments`, `History`
- [x] `WorkspaceSelection` (record) — angelegt; `SelectedItem`, `SelectionPath`
- [x] `ApplicationLink` (EF-Entity) — angelegt; `Id`, `ApplicationId`, `Url`, `Label`, `SortOrder`, `RowVersion`
- [x] `EndpointCallHistoryEntry` (EF-Entity) — angelegt; `Id` (long), `ApplicationId`, `EndpointId`, `ExecutedAt`, `HttpMethod`, `RelativePath`, `StatusCode`, `DurationMs`
- [x] `IApplicationGroupService` (Interface) — angelegt; `UpdateNameAsync`, `UpdateDescriptionAsync`, `UpdateSubtitleAsync`, `UpdateIconAsync`
- [x] `ApplicationGroupService` (Klasse, Scoped Service) — angelegt; vollständige Implementierung inkl. Größenprüfung
- [x] `IApplicationService` (Interface) — angelegt; `UpdateNameAsync`, `UpdateSubtitleAsync`, `UpdateIconAsync`
- [x] `ApplicationService` (Klasse, Scoped Service) — angelegt; vollständige Implementierung inkl. Größenprüfung
- [x] `IApplicationLinkService` (Interface) — angelegt; `GetLinksAsync`, `AddLinkAsync`, `UpdateLinkAsync`, `DeleteLinkAsync`
- [x] `ApplicationLinkService` (Klasse, Scoped Service) — angelegt; vollständige Implementierung
- [x] `IApplicationLinkRepository` (Interface) — angelegt; `GetByApplicationIdAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`
- [x] `ApplicationLinkRepository` (Klasse) — angelegt; vollständige Implementierung
- [x] `IHistoryService` (Interface) — angelegt; `AddEntryAsync`, `GetPagedAsync`, `GetTopEndpointsAsync`
- [x] `HistoryService` (Klasse, Scoped Service) — angelegt; vollständige Implementierung mit Filterung, absteigender Sortierung, Paginierung, Top-N-Aggregation
- [x] `UploadSettings` (Konfigurationsklasse) — angelegt; `MaxIconSizeBytes` mit Standardwert 524 288
- [x] `HistorySettings` (Konfigurationsklasse) — angelegt; `DefaultPageSize` mit Standardwert 50

### Änderungen an bestehenden Klassen

- [x] Feld `Description` (`string?`) in `ApplicationGroup` — vorhanden
- [x] Feld `Subtitle` (`string?`) in `ApplicationGroup` — vorhanden
- [x] Feld `IconData` (`byte[]?`) in `ApplicationGroup` — vorhanden
- [x] Feld `Subtitle` (`string?`) in `Application` — vorhanden
- [x] Feld `IconData` (`byte[]?`) in `Application` — vorhanden
- [x] Feld `Links` (`ICollection<ApplicationLink>`) in `Application` — vorhanden
- [x] Feld `Description` (`string?`) in `SystemEnvironment` — vorhanden
- [x] Methode `GetApplicationCountByGroupAsync(int)` in `IApplicationRepository` — vorhanden
- [x] Methode `GetEndpointCountByGroupAsync(int)` in `IApplicationRepository` — vorhanden
- [x] Methode `GetApplicationCountByGroupAsync` in `ApplicationRepository` — vorhanden
- [x] Methode `GetEndpointCountByGroupAsync` in `ApplicationRepository` — vorhanden
- [x] `EndpointExecutionService` — `IHistoryService`-Abhängigkeit per DI vorhanden; `PersistHistoryEntryAsync` wird nach Ausführung aufgerufen (sowohl bei Erfolg als auch bei HTTP-Fehler)
- [x] `DbSet<ApplicationLink>` in `AppDbContext` — vorhanden; FK-Konfiguration eingetragen
- [x] `DbSet<EndpointCallHistoryEntry>` in `AppDbContext` — vorhanden; FK-Konfigurationen eingetragen
- [x] `MainLayout` — auf leeren `LayoutComponentBase`-Wrapper reduziert
- [x] `Home.razor` — auf minimale Page (`@page "/"`) ohne Inhaltslogik reduziert
- [x] `theme.js` — verwendet `data-theme` (shadcn-konform) statt `data-bs-theme`
- [x] `ThemeService.SetTheme` / `PersistTheme` — ruft `applyTheme` mit `data-theme`-Attribut auf
- [x] `App.razor` — kein Bootstrap-CSS/JS mehr; ShadcnBlazor-CSS eingebunden
- [x] `Program.cs` — alle neuen Services und Repositories registriert; `UploadSettings` und `HistorySettings` gebunden; `AddShadcnBlazor()` aufgerufen
- [x] `appsettings.json` — Einträge `Upload:MaxIconSizeBytes` (524 288) und `History:DefaultPageSize` (50) vorhanden
- [x] NuGet-Paket `ShadcnBlazor` v1.0.14 — in `.csproj` referenziert; Bootstrap nicht mehr vorhanden
- [x] `ApplicationGroupTree` — neue Callbacks `OnApplicationGroupSelected` und `OnEndpointGroupSelected` vorhanden und verdrahtet; `RequestSelectGroup` feuert `OnApplicationGroupSelected`; `RequestSelectEndpointGroup` feuert `OnEndpointGroupSelected`; Beschriftung auf „Neue Sammlung" umgestellt

### Datenbankmigrationen

- [x] `AddApplicationGroupDescriptionSubtitleIcon` — angelegt; `ApplicationGroups`: `Description`, `Subtitle`, `IconData` (nullable)
- [x] `AddApplicationSubtitleIcon` — angelegt; `Applications`: `Subtitle`, `IconData` (nullable)
- [x] `AddSystemEnvironmentDescription` — angelegt; `SystemEnvironments`: `Description` (nullable)
- [x] `AddApplicationLinkTable` — angelegt; Tabelle `ApplicationLinks` mit FK zu `Applications`
- [x] `AddEndpointCallHistoryTable` — angelegt; Tabelle `EndpointCallHistory` mit FKs zu `Applications` und `Endpoints`

### Tests

- [x] `NavigationStateService_SetArea_FeuertOnAreaChanged` in `NavigationStateServiceTests` — vorhanden
- [x] `NavigationStateService_SetSelection_FeuertOnSelectionChanged` in `NavigationStateServiceTests` — vorhanden
- [x] `ApplicationGroupService_UpdateIcon_PersistiertBytes` in `ApplicationGroupServiceTests` — vorhanden
- [x] `ApplicationGroupService_UpdateIcon_ZuGroßeDatei_WirftException` in `ApplicationGroupServiceTests` — vorhanden
- [x] `ApplicationLinkService_GetLinks_GibtLinksZurück` in `ApplicationLinkServiceTests` — vorhanden
- [x] `ApplicationLinkService_AddLink_PersistiertLink` in `ApplicationLinkServiceTests` — vorhanden
- [x] `ApplicationLinkService_UpdateLink_AktualisierLink` in `ApplicationLinkServiceTests` — vorhanden
- [x] `ApplicationLinkService_DeleteLink_EntferntLink` in `ApplicationLinkServiceTests` — vorhanden
- [x] `HistoryService_AddEntry_PersistiertEintrag` in `HistoryServiceTests` — vorhanden
- [x] `HistoryService_GetPaged_ReturnsKorrekteSortiertheitUndFilterung` in `HistoryServiceTests` — vorhanden
- [x] `HistoryService_GetTopEndpoints_GibtTop5Zurück` in `HistoryServiceTests` — vorhanden
- [x] `Playwright_BereichswechselWorkspaces_ZeigtSidebar` in `NavigationTests` — vorhanden
- [x] `Playwright_BereichswechselEnvironments_ZeigtUmgebungsliste` in `NavigationTests` — vorhanden
- [x] `Playwright_BereichswechselHistory_ZeigtHistorieliste` in `NavigationTests` — vorhanden
- [x] `Playwright_BreadcrumbKlick_NavigiertZurückZurSammlung` in `NavigationTests` — vorhanden
- [x] `Playwright_InplaceEditing_Sammlung_Name_Speichern` in `InplaceEditingTests` — vorhanden
- [x] `Playwright_InplaceEditing_Sammlung_Name_PflichfeldValidierung` in `InplaceEditingTests` — vorhanden
- [x] `Playwright_InplaceEditing_Anwendung_Subtitle_Speichern` in `InplaceEditingTests` — vorhanden
- [x] `Playwright_InplaceEditing_Escape_BrichtAb` in `InplaceEditingTests` — vorhanden
- [x] `Playwright_IconUpload_ValideDatei_ZeigtIcon` in `IconUploadTests` — vorhanden
- [x] `Playwright_IconUpload_FalschesFormat_ZeigtFehler` in `IconUploadTests` — vorhanden
- [x] `Playwright_IconUpload_ZuGroßeDatei_ZeigtFehler` in `IconUploadTests` — vorhanden
- [x] `CreateInMemoryDbContextWithHistory()` in `TestHelpers` — vorhanden
- [x] `MainLayoutTests` — alle 10 Tests auf `AppShell`/`TopBar` umgeschrieben

---

## Offene Aufgaben

- [ ] `ApplicationContentView` — teilweise umgesetzt: Der Plan fordert einen „Statusblock" als expliziten Inhaltsblock in `ApplicationContentView` (neben Beschreibung, URLs, `LinksManager`, `ApplicationTopEndpointsTable`). In der Implementierung ist kein Statusblock vorhanden; stattdessen wird ein Block „Letzte Aufrufe" (paginierter History-Auszug) gerendert, der im Plan nicht für `ApplicationContentView` vorgesehen ist. Ein Statusblock mit z. B. Anzahl Endpunkte oder anderen Kennzahlen fehlt.

---

## Hinweise

- Der im Plan als Testname angegebene `Playwright_InplaceEditing_Sammlung_Name_PflichfeldValidierung` enthält einen Tippfehler gegenüber der Implementierung (`PflichtfeldValidierung` vs. `PflichfeldValidierung`). Der Implementierungsname ist korrekt; dies ist kein Implementierungsfehler.
- `HelpPage` enthält kein `MainMenu`-Element wie im Plan erwähnt. Da `MainMenu` keine eigenständig geplante Komponente ist und keinen eigenen Planeintrag hat, wird dies nicht als offene Aufgabe gewertet.
- Die fehlenden `EndpointContentView`-spezifischen Tests (z. B. `Playwright`-Tests für Endpunktdarstellung im neuen Layout) sind im Plan nicht explizit als neue Testmethoden gelistet und wurden daher nicht als offene Aufgabe aufgenommen.
