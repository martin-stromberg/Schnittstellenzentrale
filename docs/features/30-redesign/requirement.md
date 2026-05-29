# Anforderung 30 – Neues modernes Design (Redesign)

## Fachliche Zusammenfassung

Die gesamte UI der Schnittstellenzentrale wird auf die Komponentenbibliothek **shadcn** migriert und erhält ein neues Layout mit Hell-/Dunkel-Theme. Der bestehende Begriff `ApplicationGroup` wird in der Benutzeroberfläche durchgängig durch „Sammlung" ersetzt. Das bisherige Einzel-Layout (Seitenleiste + Inhaltsbereich) wird in drei Anwendungsbereiche aufgeteilt: **Workspaces**, **Environments** und **History**; die Titelleiste übernimmt die Bereichsnavigation. Im Bereich Workspaces ersetzt ein kontextsensitiver Inhaltsbereich mit Breadcrumb-Leiste und Inhaltsblöcken (3-Spalten-Raster) die bisherige statische Detailansicht; neue Blöcke zeigen KPI-Kennzahlen, Links-CRUD und Top-5-Endpunkte auf Basis der History-Aufrufhäufigkeit.

---

## Betroffene Klassen und Komponenten

### Umbenennung (UI-Ebene)

- Alle Razor-Komponenten und Labels, die „Anwendungsgruppe" oder `ApplicationGroup` als angezeigten Text verwenden, werden auf „Sammlung" umgestellt.
- Der Klassenname `ApplicationGroup` im Datenmodell bleibt unverändert (reine UI-Umbenennung).

### Neue UI-Komponenten

| Komponente | Beschreibung |
|---|---|
| `AppShell` / neues `MainLayout` | Neues Root-Layout mit shadcn-basierter Titelleiste und Bereichsnavigation |
| `TopBar` | Titelleiste: App-Name, Bereichs-Tabs (Workspaces/Environments/History), StorageMode-Selector, ThemeToggle, Profil-Icon, Einstellungs-Link, Hilfe-Icon |
| `WorkspacesLayout` | Zwei-Spalten-Layout für den Workspaces-Bereich: Hauptmenü + Inhaltsbereich |
| `WorkspacesSidebar` | Hauptmenü Workspaces: „+ New Collection"-Button, `ApplicationGroupTree` (bestehend, ggf. angepasst) |
| `ContentBreadcrumb` | Breadcrumb-Leiste (max. 4 Elemente: Sammlung > Anwendung > Ordner > Endpunkt), klickbar |
| `ContentHeader` | Kopfbereich: Upload-Icon (PNG/JPG, Datei-Picker), Name, Untertitel mit Hover-Bleistift → In-place-Editing; Read-only-Variante für Ordner/Endpunkt |
| `CollectionContentView` | Inhaltsblöcke für eine Sammlung (`ApplicationGroup`): Beschreibungsblock + Statusblock (Anzahl Anwendungen, Anzahl Endpunkte) |
| `ApplicationContentView` | Inhaltsblöcke für eine Anwendung: Beschreibung, Basis-URL, Swagger/OData-URL, Statusblock letzte Aufrufe, Links-Verwaltung, Top-5-Endpunkte-Tabelle |
| `FolderContentView` | Inhaltsblöcke für einen Ordner (`EndpointGroup`): Tabelle aller Endpunkte des Ordners |
| `EndpointContentView` | Bestehende Endpunktdarstellung (unverändert, in neuem Layout eingebettet) |
| `LinksManager` | CRUD-Verwaltung von URL-Links mit Beschriftung (neue Entität, s. u.) |
| `ApplicationTopEndpointsTable` | Top-5-Endpunkte-Tabelle auf Basis der Aufrufhäufigkeit aus `ActivityLog`/History |
| `EnvironmentsLayout` | Layout für den Environments-Bereich |
| `EnvironmentsSidebar` | Hauptmenü Environments: „+ Neue Umgebung"-Button, Liste der Umgebungen mit Lösch-Aktion |
| `EnvironmentContentView` | Inhaltsbereich je Umgebung: Name (editierbar), Beschreibung (editierbar), Variablentabelle (bestehend) |
| `HistoryLayout` | Layout für den History-Bereich |
| `HistoryContentView` | Liste vergangener API-Aufrufe mit Request/Response, Filterung, Paginierung, Sortierung |
| `EmptyContentView` | Platzhalteransicht bei leerem Inhaltsbereich (kein Element ausgewählt) |

### Neue Datenmodellklassen / neue Eigenschaften

| Artefakt | Begründung |
|---|---|
| `ApplicationGroup.Description` (`string?`) | Neue Eigenschaft für den Beschreibungsblock der Sammlung |
| `ApplicationGroup.Subtitle` (`string?`) | Untertitel für In-place-Editing im Kopfbereich |
| `ApplicationGroup.IconData` (`byte[]?`) oder gespeicherter Dateipfad | Icon-Upload (PNG/JPG) für Sammlungen |
| `Application.Subtitle` (`string?`) | Untertitel für In-place-Editing im Kopfbereich der Anwendung |
| `Application.IconData` (`byte[]?`) oder gespeicherter Dateipfad | Icon-Upload (PNG/JPG) für Anwendungen |
| `ApplicationLink` (neue Entität) | Verknüpfte URLs mit Beschriftung je Anwendung; Felder: `Id`, `ApplicationId`, `Url`, `Label`, `SortOrder`, `RowVersion` |
| EF-Migration | Für alle neuen Eigenschaften und die neue `ApplicationLink`-Tabelle |

> **Annahme:** Icon-Speicherung erfolgt als `byte[]` in der Datenbank oder als Dateisystemreferenz. Die konkrete Strategie ist noch offen (s. Offene Fragen).

### Bestehende Klassen mit Anpassungsbedarf

| Klasse | Art der Änderung |
|---|---|
| `MainLayout` | Wird durch `AppShell` / neues `TopBar`-Layout ersetzt; bestehende Logik (StorageMode, ThemeService, EnvironmentSelector, ActivityLog) wird migriert |
| `ApplicationGroupTree` | Wird als Teilkomponente in `WorkspacesSidebar` eingebettet; Klick-Navigation muss auf den neuen Inhaltsbereich-Router weitergeleitet werden |
| `ThemeService` | Bleibt unverändert; `TopBar` übernimmt die Schaltfläche wie bisher `MainLayout` |
| `StorageModeService` | Bleibt unverändert; `TopBar` enthält die Auswahlbox; Moduswechsel löst weiterhin Reload von Baum und Inhaltsbereich aus |
| `IApplicationRepository` / `ApplicationRepository` | Neue Methoden für `ApplicationLink`-CRUD; ggf. neue Methode zum Laden von Anzahl Anwendungen/Endpunkte je Gruppe |
| `ActivityLogService` / History-Datenpersistenz | Top-5-Endpunkte und History-Bereich setzen persistente Aufrufprotokolle voraus — derzeit werden Einträge **nicht** in der Datenbank gespeichert (s. Offene Fragen) |
| `SystemEnvironment`-Verwaltungskomponenten | Werden in `EnvironmentsSidebar` / `EnvironmentContentView` integriert |

### Services

| Service / Interface | Änderung |
|---|---|
| `IApplicationGroupService` (neu oder Erweiterung `IApplicationRepository`) | `UpdateDescriptionAsync`, `UpdateSubtitleAsync`, `UpdateIconAsync` für Sammlungen |
| `IApplicationService` (neu oder Erweiterung) | `UpdateSubtitleAsync`, `UpdateIconAsync`, `GetTopEndpointsAsync(applicationId, count)` |
| `IApplicationLinkService` (neu) | `GetLinksAsync`, `AddLinkAsync`, `UpdateLinkAsync`, `DeleteLinkAsync` |
| `IHistoryService` (neu oder Erweiterung `IActivityLogService`) | Persistente Aufrufhistorie; `GetPagedAsync(filter, page, pageSize)` mit Filtern nach Anwendung, Endpunkt, Zeitraum; sortiert nach Ausführungszeitpunkt absteigend |

### Tests

- Playwright-Tests für neue Navigationspfade (Bereichswechsel, Breadcrumb-Klick)
- Playwright-Tests für In-place-Editing (Sammlung, Anwendung)
- Playwright-Tests für Icon-Upload (valide und invalide Dateien)
- Unit-Tests für `IApplicationLinkService` (CRUD)
- Unit-Tests für `IHistoryService.GetPagedAsync` (Filterung, Paginierung)

---

## Implementierungsansatz

### Komponentenbibliothek-Migration

Die gesamte UI wird von der bestehenden Bibliothek (Bootstrap 5 / Bootstrap Icons) auf **shadcn** (Blazor-Port, z. B. `shadcn-blazor` oder äquivalent) migriert. Das Hell-/Dunkel-Theme wird weiterhin über `ThemeService` und `data-bs-theme` (bzw. äquivalentes shadcn-Attribut) gesteuert. Das bestehende `theme-init.js`-Skript muss ggf. angepasst werden.

### Layout-Umstrukturierung

`MainLayout.razor` wird durch ein neues `AppShell`-Layout ersetzt. Die Bereichsnavigation (Workspaces / Environments / History) wird als Router-Parameter oder über einen zentralen Zustandsdienst (`INavigationStateService`, neu) abgebildet. Innerhalb von Workspaces navigiert ein zweiter Zustand (ausgewähltes Element: `ApplicationGroup`, `Application`, `EndpointGroup`, `Endpoint`) den Inhaltsbereich.

### Breadcrumb und Inhaltsbereich-Navigation

Der Inhaltsbereich rendert kontextabhängig eine der vier Content-Views. Der Breadcrumb-Zustand wird aus dem aktuell selektierten Pfad im Baum abgeleitet. Klicks im Breadcrumb setzen den Navigationszustand auf die entsprechende Ebene zurück.

### In-place-Editing

Beim Hover über Name oder Untertitel erscheint ein Bleistift-Icon. Ein Klick darauf schaltet das Feld in einen editierbaren Zustand (Inline-`<input>` oder shadcn `EditableText`). Pflichtfeld-Validierung erfolgt direkt am Feld; Speichern bei Blur oder Enter, Abbrechen bei Escape. Die Persistenz erfolgt über den zugehörigen Service (`IApplicationGroupService.UpdateDescriptionAsync` etc.).

### Icon-Upload

Ein unsichtbares `<input type="file" accept="image/png,image/jpeg">` wird über das Upload-Icon ausgelöst. Die Dateigröße wird client-seitig geprüft (max. ca. 512 KB als gängige Symbolgröße — Annahme, s. Offene Fragen). Ungültige Dateien (falsches Format, zu groß) lösen eine Fehlermeldung aus, ohne einen Upload durchzuführen. Valide Dateien werden als `byte[]` oder Base64 an den Service übergeben und persistiert.

### History-Persistenz und Top-5-Endpunkte

Die bisherige `ActivityLogService`-Implementierung hält Einträge ausschließlich im Arbeitsspeicher. Für die Top-5-Endpunkte und den History-Bereich ist eine Datenbankpersistenz der `EndpointExecuted`-Einträge erforderlich. Dies stellt eine größere Erweiterung dar (neue Tabelle, neuer Service, Paginierung). Der `EndpointExecutionService` muss nach erfolgreicher Ausführung zusätzlich in die persistente History schreiben.

### Environments-Bereich

Die bestehenden Komponenten zur `SystemEnvironment`-Verwaltung werden in das neue `EnvironmentsSidebar`/`EnvironmentContentView`-Layout integriert. Neue Funktionalität: Beschreibungsfeld (`SystemEnvironment.Description`, neue Eigenschaft) editierbar im Inhaltsbereich.

> **Annahme:** `SystemEnvironment` erhält ein optionales `Description`-Feld, sofern es noch nicht vorhanden ist.

---

## Konfiguration

- **Theme (Hell/Dunkel):** weiterhin browserseitig in `localStorage` (keine Änderung an `ThemeService`).
- **StorageMode (Team/Benutzer):** weiterhin per `StorageModeService` (Scoped, pro Circuit); die Auswahlbox zieht in die `TopBar`.
- **Icon-Upload Maximalgröße:** wird als Konstante in einer Konfigurationsklasse oder `appsettings.json` hinterlegt (z. B. `Upload:MaxIconSizeBytes`).
- **History-Paginierung:** Seitengröße konfigurierbar (z. B. `History:DefaultPageSize`).

---

## Offene Fragen

1. **Icon-Speicherung:** Werden Icons als `byte[]` in der Datenbank (`ApplicationGroup.IconData`, `Application.IconData`) oder als Dateisystempfade gespeichert? Datenbankansatz ist einfacher (keine Dateisystemverwaltung), aber potenziell größenintensiv.
2. **Maximale Icon-Dateigröße:** „Gängige Symbolgröße" wurde als Orientierung genannt — konkreter Wert noch festzulegen (Vorschlag: 512 KB).
3. **History-Persistenz:** Derzeit werden `ActivityLogEntry`-Einträge nicht in der Datenbank gespeichert. Für den History-Bereich und die Top-5-Endpunkte ist eine neue persistente `EndpointCallHistory`-Tabelle (o. Ä.) erforderlich. Soll der bestehende `ActivityLogService` erweitert werden, oder wird ein separater `IHistoryService` mit eigener Tabelle eingeführt?
4. **Leerszustand Inhaltsbereich:** Was soll angezeigt werden, wenn kein Element im Baum ausgewählt ist? (Vorschlag: `EmptyContentView` mit Hinweistext „Wählen Sie eine Sammlung oder Anwendung aus.")
5. **shadcn-Blazor-Port:** Welcher konkrete NuGet-Port von shadcn wird eingesetzt (z. B. `shadcn-blazor`, `BlazorShadcnUI` o. Ä.)? Existiert dieser bereits im Projekt?
6. **Bereichsnavigation und Routing:** Werden die drei Bereiche als Blazor-Routen (`/workspaces`, `/environments`, `/history`) oder über einen internen Zustandsdienst ohne URL-Wechsel abgebildet?
7. **Benutzerprofil-Icon:** Nur Darstellung in dieser Anforderung — welche Quelle für das Avatar-Bild (Windows-Benutzername, Initialen-Fallback)?
8. **Hilfe-Funktion:** Was soll das Fragezeichen-Icon öffnen (bestehende `docs/help/`-Seiten, externe URL, Modal)?
9. **`SystemEnvironment.Description`:** Besitzt `SystemEnvironment` bereits ein Beschreibungsfeld, oder muss es neu hinzugefügt werden?
10. **Datenbankmigrationen bei laufenden Installationen:** Icon-Felder (`byte[]?`) und neue `ApplicationLink`-Tabelle erfordern eine EF-Migration. Upgrade-Sicherheit für bestehende Installationen muss geprüft werden.
