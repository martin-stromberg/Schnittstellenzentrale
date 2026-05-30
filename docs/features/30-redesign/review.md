# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

### Neue Klassen / Razor-Komponenten

- [x] `AppShell` (Razor-Komponente, Layout) — angelegt
- [x] `TopBar` (Razor-Komponente) — angelegt
- [x] `WorkspacesLayout` (Razor-Komponente, Layout) — angelegt
- [x] `WorkspacesSidebar` (Razor-Komponente) — angelegt
- [x] `ContentBreadcrumb` (Razor-Komponente) — angelegt
- [x] `ContentHeader` (Razor-Komponente) — angelegt
- [x] `CollectionContentView` (Razor-Komponente) — angelegt
- [x] `ApplicationContentView` (Razor-Komponente) — angelegt
- [x] `FolderContentView` (Razor-Komponente) — angelegt
- [x] `LinksManager` (Razor-Komponente) — angelegt
- [x] `ApplicationTopEndpointsTable` (Razor-Komponente) — angelegt
- [x] `EnvironmentsLayout` (Razor-Komponente, Layout) — angelegt
- [x] `EnvironmentsSidebar` (Razor-Komponente) — angelegt
- [x] `EnvironmentContentView` (Razor-Komponente) — angelegt
- [x] `HistoryLayout` (Razor-Komponente, Layout) — angelegt
- [x] `HistoryContentView` (Razor-Komponente) — angelegt
- [x] `EmptyContentView` (Razor-Komponente) — angelegt
- [x] `HelpPage` (Razor-Page, `@page "/help"`) — angelegt
- [x] `INavigationStateService` (Interface) — angelegt
- [x] `NavigationStateService` (Klasse, Scoped Service) — angelegt
- [x] `NavigationArea` (Enum, Werte: Workspaces, Environments, History) — angelegt
- [x] `WorkspaceSelection` (record) — angelegt
- [x] `ApplicationLink` (EF-Entity) — angelegt
- [x] `EndpointCallHistoryEntry` (EF-Entity) — angelegt
- [x] `IApplicationGroupService` (Interface) — angelegt
- [x] `ApplicationGroupService` (Klasse, Scoped Service) — angelegt
- [x] `IApplicationService` (Interface) — angelegt
- [x] `ApplicationService` (Klasse, Scoped Service) — angelegt
- [x] `IApplicationLinkService` (Interface) — angelegt
- [x] `ApplicationLinkService` (Klasse, Scoped Service) — angelegt
- [x] `IApplicationLinkRepository` (Interface) — angelegt
- [x] `ApplicationLinkRepository` (Klasse) — angelegt
- [x] `IHistoryService` (Interface, inkl. `HistoryFilter`- und `TopEndpointResult`-Records) — angelegt
- [x] `HistoryService` (Klasse, Scoped Service) — angelegt
- [x] `UploadSettings` (Konfigurationsklasse) — angelegt
- [x] `HistorySettings` (Konfigurationsklasse) — angelegt

### Neue Felder in bestehenden Klassen

- [x] Feld `Description` (`string?`) in `ApplicationGroup` — vorhanden
- [x] Feld `Subtitle` (`string?`) in `ApplicationGroup` — vorhanden
- [x] Feld `IconData` (`byte[]?`) in `ApplicationGroup` — vorhanden
- [x] Feld `Subtitle` (`string?`) in `Application` — vorhanden
- [x] Feld `IconData` (`byte[]?`) in `Application` — vorhanden
- [x] Navigationseigenschaft `Links` (`ICollection<ApplicationLink>`) in `Application` — vorhanden
- [x] Feld `Description` (`string?`) in `SystemEnvironment` — vorhanden

### Neue Methoden in bestehenden Klassen

- [x] `GetApplicationCountByGroupAsync(int groupId)` in `IApplicationRepository` / `ApplicationRepository` — vorhanden
- [x] `GetEndpointCountByGroupAsync(int groupId)` in `IApplicationRepository` / `ApplicationRepository` — vorhanden
- [x] `ExecuteAsync` in `EndpointExecutionService` — `IHistoryService`-Abhängigkeit hinzugefügt; `PersistHistoryEntryAsync` wird nach Ausführung aufgerufen

### DbContext-Erweiterungen

- [x] `DbSet<ApplicationLink>` in `AppDbContext` — vorhanden
- [x] `DbSet<EndpointCallHistoryEntry>` in `AppDbContext` — vorhanden
- [x] FK-Konfigurationen für `ApplicationLink` und `EndpointCallHistoryEntry` — vorhanden

### Datenbankmigrationen

- [x] Migration `AddApplicationGroupDescriptionSubtitleIcon` — vorhanden
- [x] Migration `AddApplicationSubtitleIcon` — vorhanden
- [x] Migration `AddSystemEnvironmentDescription` — vorhanden
- [x] Migration `AddApplicationLinkTable` — vorhanden
- [x] Migration `AddEndpointCallHistoryTable` — vorhanden

### Konfiguration

- [x] Eintrag `Upload:MaxIconSizeBytes` (Standardwert 524288) in `appsettings.json` — vorhanden
- [x] Eintrag `History:DefaultPageSize` (Standardwert 50) in `appsettings.json` — vorhanden

### Service-Registrierung in `Program.cs`

- [x] `INavigationStateService` → `NavigationStateService` — registriert
- [x] `IApplicationGroupService` → `ApplicationGroupService` — registriert
- [x] `IApplicationService` → `ApplicationService` — registriert
- [x] `IApplicationLinkService` → `ApplicationLinkService` — registriert
- [x] `IApplicationLinkRepository` → `ApplicationLinkRepository` — registriert
- [x] `IHistoryService` → `HistoryService` — registriert
- [x] `UploadSettings`- und `HistorySettings`-Bindung — vorhanden
- [x] `AddShadcnBlazor()` — registriert

### ShadcnBlazor-Migration / Bootstrap-Entfernung

- [x] Bootstrap-CSS/-JS aus `App.razor` entfernt; shadcn-CSS eingebunden
- [x] `ShadcnBlazor` NuGet-Paket in `.csproj` referenziert
- [x] `ThemeService` / `theme.js` auf shadcn-äquivalentes `dark`-Klassen-Schema (`classList.add('dark')`) umgestellt (keine `data-bs-theme` mehr)
- [x] `theme-init.js` angepasst

### Änderungen an bestehenden Komponenten

- [x] `MainLayout` — auf minimalen Wrapper reduziert (rendert nur `@Body`)
- [x] `Home.razor` — auf minimale Page ohne Inhaltslogik reduziert
- [x] `ApplicationGroupTree` — Callback `OnApplicationGroupSelected` vorhanden; Callback `OnEndpointGroupSelected` vorhanden; Button-Beschriftung auf „Neue Sammlung" umgestellt
- [x] `WorkspacesSidebar` — leitet alle Selektionsereignisse via `INavigationStateService.SetWorkspaceSelectionAsync` weiter

### Tests: Neue Testmethoden

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
- [x] `Playwright_InplaceEditing_Sammlung_Name_PflichfeldValidierung` in `InplaceEditingTests` — vorhanden (Methodenname weicht ab: `PflichtfeldValidierung` statt `PflichfeldValidierung`, inhaltlich identisch)
- [x] `Playwright_InplaceEditing_Anwendung_Subtitle_Speichern` in `InplaceEditingTests` — vorhanden
- [x] `Playwright_InplaceEditing_Escape_BrichtAb` in `InplaceEditingTests` — vorhanden
- [x] `Playwright_IconUpload_ValideDatei_ZeigtIcon` in `IconUploadTests` — vorhanden
- [x] `Playwright_IconUpload_FalschesFormat_ZeigtFehler` in `IconUploadTests` — vorhanden
- [x] `Playwright_IconUpload_ZuGroßeDatei_ZeigtFehler` in `IconUploadTests` — vorhanden
- [x] `CreateInMemoryDbContextWithHistory()` in `TestHelpers` — vorhanden (delegiert an `CreateInMemoryDbContext()`, da der reguläre DbContext bereits den `EndpointCallHistory`-DbSet enthält)

### Tests: Angepasste bestehende Tests

- [x] `MainLayoutTests` (alle Tests) — auf `AppShell` / `TopBar` umgeschrieben; 10 Tests vorhanden
- [x] `HomePageTests` — Selektoren auf `.sz-tree-chevron-btn` / `.sz-tree-children` angepasst
- [x] `StorageModeTests` — Selektor auf `.sz-topbar-select` angepasst
- [x] `EnvironmentManagementTests.MaskierterWert_IstNichtImKlartextImDomSichtbar` — Öffnungsweg auf `.sz-topbar-tab` „Environments" umgestellt

## Offene Aufgaben

(keine)

## Hinweise

- `HelpPage` enthält keine `MainMenu`-Komponente. Das Planelement „Leere Hilfe-Seite mit leerem `MainMenu`" bezog sich auf eine Bootstrap-Navigationskomponente, die durch das neue Layout nicht mehr existiert. Die Seite ist als einfache `@page "/help"`-Page implementiert, was der Absicht entspricht.
- `HistoryService.GetTopEndpointsAsync` verwendet einen Left-Join (GroupJoin + DefaultIfEmpty), sodass gelöschte Endpoints mit dem Platzhalter „(gelöscht)" erscheinen. Dies weicht von der ursprünglichen Planformulierung (Inner-Join) ab und stellt eine Verbesserung dar; der betreffende Code-Review-Befund aus `continue.md` ist damit ebenfalls adressiert.
- Die Testmethode `Playwright_InplaceEditing_Sammlung_Name_PflichtfeldValidierung` hat im Code einen Tippfehler im Methodennamen (`PflichtfeldValidierung` vs. Plan `PflichfeldValidierung`), ist aber inhaltlich vorhanden und korrekt.
