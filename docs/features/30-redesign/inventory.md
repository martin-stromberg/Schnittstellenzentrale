# Bestandsaufnahme: Neues modernes Design (Redesign)

Analysiert wurden die UI-Komponenten, Datenmodelle, Services und Tests der Schnittstellenzentrale bezogen auf Anforderung 30 (shadcn-Migration, neues Layout mit drei Bereichen, neue Entitäten und In-place-Editing).

---

## Zusammenfassung

- **`MainLayout`** existiert und enthält bereits StorageMode-Selektor, `ThemeToggle`, Umgebungsverwaltung, ActivityLog-Panel und SignalR-Integration — muss durch `AppShell`/`TopBar` ersetzt werden.
- **`ApplicationGroupTree`** existiert mit vollständiger Baum-Logik (Gruppen, Anwendungen, Ordner, Endpunkte, Drag-Drop, SignalR) — muss in `WorkspacesSidebar` eingebettet werden; aktuelle Buttons heißen „Neue Gruppe"/„Neue Anwendung".
- **`ThemeService`** und **`StorageModeService`** sind vollständig implementiert und benötigen keine Änderungen; sie werden von `MainLayout` abonniert.
- **`ApplicationCard`** zeigt bereits `Description`, `BaseUrl` und `InterfaceUrl` an; kein In-place-Editing, keine Links-Verwaltung, keine Top-5-Endpunkte.
- **`ApplicationGroup`** besitzt noch keine `Description`-, `Subtitle`- oder `IconData`-Eigenschaft.
- **`Application`** besitzt bereits `Description`, aber noch kein `Subtitle` und kein `IconData`.
- **`SystemEnvironment`** besitzt noch kein `Description`-Feld.
- **`ApplicationLink`** als neue Entität existiert nicht.
- **`IActivityLogService`** / `ActivityLogService` arbeiten rein In-Memory; keine Datenbankpersistenz der Aufrufhistorie.
- **`IApplicationRepository`** enthält noch keine Methoden für `ApplicationLink`-CRUD oder Zählung von Anwendungen/Endpunkten je Gruppe.
- Kein Bereich „Environments" oder „History" als eigenständiger Layout-Bereich vorhanden — alles läuft über `Home.razor` als einzige Page.
- **Tests:** vorhanden für `MainLayout`, `ActivityLogService`, `ApplicationRepository` (Integration), Playwright für Startseite, StorageMode, Umgebungsverwaltung und CRUD. Keine Tests für In-place-Editing, Icon-Upload, Links-CRUD oder History-Paginierung.
- Komponentenbibliothek: **Bootstrap 5 + Bootstrap Icons** (keine shadcn-Komponenten vorhanden).

---

## Details

- [Datenmodell](inventory/models.md)
- [Logik](inventory/logic.md)
- [Enums](inventory/enums.md)
- [Interfaces](inventory/interfaces.md)
- [Tests](inventory/tests.md)
